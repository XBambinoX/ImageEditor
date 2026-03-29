using ImageEditor.Commands;
using ImageEditor.Services.ImageProcessing;
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
    public class SobelViewModel : BaseFilterViewModel
    {
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

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseThresholdCommand { get; }
        public ICommand DecreaseThresholdCommand { get; }

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
                int stride = _original.BackBufferStride;

                byte[] pixels = new byte[h * stride];
                _original.CopyPixels(pixels, stride, 0);

                byte[] resultBytes = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return null;
                    return SobelHelper.ApplySobel(pixels, w, h, stride, threshold, colorize);
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