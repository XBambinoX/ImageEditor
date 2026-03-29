using ImageEditor.Commands;
using ImageEditor.Services.ImageProcessing;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageEditor.Services.Math;

namespace ImageEditor.ViewModels
{
    public class SharpenViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;

        private WriteableBitmap _preview;
        private int _strength = 1;

        public int MinStrength => 0;
        public int MaxStrength => 100;

        private int _radius = 1;
        public int MinRadius => 1;
        public int MaxRadius => 100;

        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set
            {
                _preview = value;
                OnPropertyChanged();
            }
        }

        public int Strength
        {
            get => _strength;
            set
            {
                int clamped = Tools.Clamp(value, MinStrength, MaxStrength);

                if (_strength == clamped) return;

                _strength = clamped;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public int Radius
        {
            get => _radius;
            set
            {
                int clamped = Tools.Clamp(value, MinRadius, MaxRadius);
                if (_radius == clamped) return;
                _radius = clamped;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public WriteableBitmap ResultImage { get; private set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseStrengthCommand { get; }
        public ICommand DecreaseStrengthCommand { get; }

        public ICommand IncreaseRadiusCommand { get; }
        public ICommand DecreaseRadiusCommand { get; }

        public Action<bool> CloseAction;

        public SharpenViewModel(WriteableBitmap source)
        {
            _original = source ?? throw new ArgumentNullException(nameof(source));
            PreviewImage = source;

            ApplyCommand = new RelayCommand(_ =>
            {
                ResultImage = PreviewImage;
                CloseAction?.Invoke(true);
            });

            CancelCommand = new RelayCommand(_ =>
            {
                CloseAction?.Invoke(false);
            });

            IncreaseStrengthCommand = new RelayCommand(_ => Strength++);
            DecreaseStrengthCommand = new RelayCommand(_ => Strength--);

            IncreaseRadiusCommand = new RelayCommand(_ => Radius++);
            DecreaseRadiusCommand = new RelayCommand(_ => Radius--);
        }

        private async void UpdatePreview()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Task.Delay(120, token);
                int strength = Strength;
                int radius = Radius;

                int w = _original.PixelWidth;
                int h = _original.PixelHeight;
                double dpiX = _original.DpiX;
                double dpiY = _original.DpiY;
                int stride = w * 3;
                byte[] pixels = new byte[h * stride];
                _original.CopyPixels(pixels, stride, 0);

                var result = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return null;
                    return SharpenHelper.ApplySharpen(pixels, w, h, stride, dpiX, dpiY, strength, radius);
                }, token);

                if (token.IsCancellationRequested || result == null) return;
                PreviewImage = result;
            }
            catch (TaskCanceledException) { }
        }
    }
}