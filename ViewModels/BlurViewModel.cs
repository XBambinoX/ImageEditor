using ImageEditor.Commands;
using ImageEditor.Services.ImageProcessing;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageEditor.Services.Math;

namespace ImageEditor.ViewModels
{
    public class BlurViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;

        private WriteableBitmap _preview;
        private int _radius = 1;

        public int MinRadius => 0;
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
        public ICommand IncreaseRadiusCommand { get; }
        public ICommand DecreaseRadiusCommand { get; }

        public Action<bool> CloseAction;

        public BlurViewModel(WriteableBitmap source)
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
                await Task.Delay(150, token);

                int radius = Radius;

                int w = _original.PixelWidth;
                int h = _original.PixelHeight;
                int stride = w * 4;

                byte[] src = new byte[h * stride];
                _original.CopyPixels(src, stride, 0);

                byte[] resultBytes = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested)
                        return null;

                    return GaussianBlurHelper.ApplyGaussianBlurBytes(src, w, h, stride, radius);
                }, token);

                if (token.IsCancellationRequested || resultBytes == null)
                    return;

                var wb = new WriteableBitmap(
                    w, h,
                    _original.DpiX,
                    _original.DpiY,
                    PixelFormats.Bgra32,
                    null);

                wb.WritePixels(
                    new Int32Rect(0, 0, w, h),
                    resultBytes,
                    stride,
                    0);

                PreviewImage = wb;
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}