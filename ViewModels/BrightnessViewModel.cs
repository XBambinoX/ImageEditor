using ImageEditor.Commands;
using ImageEditor.Services.Math;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public class BrightnessViewModel : BaseFilterViewModel
    {
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

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseBrightnessCommand { get; }
        public ICommand DecreaseBrightnessCommand { get; }
        public ICommand IncreaseContrastCommand { get; }
        public ICommand DecreaseContrastCommand { get; }

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
                int stride = _original.BackBufferStride;

                byte[] pixels = new byte[h * stride];
                _original.CopyPixels(pixels, stride, 0);

                byte[] resultBytes = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return null;
                    byte[] tmp = BrightnessHelper.ApplyBrightnessBytes(pixels, w, h, stride, brightness);
                    return BrightnessHelper.ApplyContrastBytes(tmp, w, h, stride, contrast);
                }, token);

                if (token.IsCancellationRequested || resultBytes == null) return;
                var wb = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgr24, null);
                wb.WritePixels(new Int32Rect(0, 0, w, h), resultBytes, wb.BackBufferStride, 0);

                PreviewImage = wb;
            }
            catch (TaskCanceledException) { }
        }
    }
}