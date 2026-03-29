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
    public class GrayscaleViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;
        private WriteableBitmap _preview;

        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set { _preview = value; OnPropertyChanged(); }
        }

        // 0 = Luminance, 1 = Average, 2 = Lightness
        private int _mode = 0;
        public int Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        private int _intensity = 0;
        public int Intensity
        {
            get => _intensity;
            set
            {
                _intensity = Tools.Clamp(value, 0, 100);
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public bool IsLuminance
        {
            get => _mode == 0;
            set { if (value) { _mode = 0; OnPropertyChanged(); UpdatePreview(); } }
        }

        public bool IsAverage
        {
            get => _mode == 1;
            set { if (value) { _mode = 1; OnPropertyChanged(); UpdatePreview(); } }
        }

        public bool IsLightness
        {
            get => _mode == 2;
            set { if (value) { _mode = 2; OnPropertyChanged(); UpdatePreview(); } }
        }

        public int MinIntensity => 0;
        public int MaxIntensity => 100;

        public WriteableBitmap ResultImage { get; private set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseIntensityCommand { get; }
        public ICommand DecreaseIntensityCommand { get; }

        public Action<bool> CloseAction;

        public GrayscaleViewModel(WriteableBitmap source)
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

            IncreaseIntensityCommand = new RelayCommand(_ => Intensity++);
            DecreaseIntensityCommand = new RelayCommand(_ => Intensity--);

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

                int mode = _mode;
                int intensity = _intensity;
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
                    return GrayscaleHelper.ApplyGrayscale(pixels, w, h, stride, mode, intensity);
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