using ImageEditor.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageEditor.Services.Math;

namespace ImageEditor.ViewModels
{
    public class BrightnessViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;
        private WriteableBitmap _preview;

        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set { _preview = value; OnPropertyChanged(); }
        }

        private int _brightness = 0;
        public int Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                OnPropertyChanged();
                UpdatePreview();    
            }
        }

        private int _contrast = 0;
        public int Contrast
        {
            get => _contrast;
            set
            {
                _contrast = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public int MinBrightness => -100;
        public int MaxBrightness => 100;
        public int MinContrast => -100;
        public int MaxContrast => 100;

        public WriteableBitmap ResultImage { get; private set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseBrightnessCommand { get; }
        public ICommand DecreaseBrightnessCommand { get; }
        public ICommand IncreaseContrastCommand { get; }
        public ICommand DecreaseContrastCommand { get; }

        public Action<bool> CloseAction;

        public BrightnessViewModel(WriteableBitmap source)
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

            IncreaseBrightnessCommand = new RelayCommand(_ => Brightness = Tools.Clamp(Brightness + 1, MinBrightness, MaxBrightness));
            DecreaseBrightnessCommand = new RelayCommand(_ => Brightness = Tools.Clamp(Brightness - 1, MinBrightness, MaxBrightness));
            IncreaseContrastCommand = new RelayCommand(_ => Contrast = Tools.Clamp(Contrast + 1, MinContrast, MaxContrast));
            DecreaseContrastCommand = new RelayCommand(_ => Contrast = Tools.Clamp(Contrast - 1, MinContrast, MaxContrast));
        }

        private async void UpdatePreview()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Task.Delay(120, token);

                int brightness = _brightness;
                int contrast = _contrast;
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

                    byte[] tmp = BrightnessHelper.ApplyBrightnessBytes(pixels, w, h, stride, brightness);
                    return BrightnessHelper.ApplyContrastBytes(tmp, w, h, stride, dpiX, dpiY, contrast);

                }, token);

                if (token.IsCancellationRequested || result == null) return;

                PreviewImage = result;
            }
            catch (TaskCanceledException) { }
        }
    }
}