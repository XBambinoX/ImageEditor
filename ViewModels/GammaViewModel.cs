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
    public class GammaViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;
        private WriteableBitmap _preview;

        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set { _preview = value; OnPropertyChanged(); }
        }

        private double _gamma = 1.0;
        public double Gamma
        {
            get => _gamma;
            set
            {
                _gamma = Tools.Clamp(value, 0.1, 5.0);
                OnPropertyChanged();
                OnPropertyChanged(nameof(GammaDisplay));
                UpdatePreview();
            }
        }

        public string GammaDisplay => _gamma.ToString("F2");

        public int GammaSlider
        {
            get => (int)(_gamma * 100);
            set
            {
                Gamma = value / 100.0;
                OnPropertyChanged();
            }
        }

        public int MinGammaSlider => 10;  // 0.10
        public int MaxGammaSlider => 500; // 5.00

        public WriteableBitmap ResultImage { get; private set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseGammaCommand { get; }
        public ICommand DecreaseGammaCommand { get; }
        public ICommand ResetGammaCommand { get; }

        public Action<bool> CloseAction;

        public GammaViewModel(WriteableBitmap source)
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

            IncreaseGammaCommand = new RelayCommand(_ => Gamma = Math.Round(Gamma + 0.05, 2));
            DecreaseGammaCommand = new RelayCommand(_ => Gamma = Math.Round(Gamma - 0.05, 2));
            ResetGammaCommand = new RelayCommand(_ => Gamma = 1.0);

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

                double gamma = _gamma;
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
                    return GammaHelper.ApplyGamma(pixels, w, h, stride, gamma);
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