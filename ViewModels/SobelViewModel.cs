using ImageEditor.Commands;
using ImageEditor.Services.ImageProcessing;
using ImageEditor.Services.Math;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public class SobelViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;
        private WriteableBitmap _preview;

        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set { _preview = value; OnPropertyChanged(); }
        }

        private int _threshold = 30;
        public int Threshold
        {
            get => _threshold;
            set
            {
                _threshold = Tools.Clamp(value, 0, 255);
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        private bool _colorize = false;
        public bool Colorize
        {
            get => _colorize;
            set
            {
                _colorize = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public int MinThreshold => 0;
        public int MaxThreshold => 255;

        public WriteableBitmap ResultImage { get; private set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseThresholdCommand { get; }
        public ICommand DecreaseThresholdCommand { get; }

        public Action<bool> CloseAction;

        public SobelViewModel(WriteableBitmap source)
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

            IncreaseThresholdCommand = new RelayCommand(_ => Threshold++);
            DecreaseThresholdCommand = new RelayCommand(_ => Threshold--);

            UpdatePreview();
        }

        private async void UpdatePreview()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Task.Delay(120, token);

                int threshold = _threshold;
                bool colorize = _colorize;
                int w = _original.PixelWidth;
                int h = _original.PixelHeight;
                double dpiX = _original.DpiX;
                double dpiY = _original.DpiY;
                int stride = w * 4;

                byte[] pixels = new byte[h * stride];
                _original.CopyPixels(pixels, stride, 0);

                var result = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return null;
                    return SobelHelper.ApplySobel(pixels, w, h, stride, dpiX, dpiY, threshold, colorize);
                }, token);

                if (token.IsCancellationRequested || result == null) return;

                PreviewImage = result;
            }
            catch (TaskCanceledException) { }
        }
    }
}