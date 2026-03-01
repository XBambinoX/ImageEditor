using ImageEditor.Commands;
using ImageEditor.Services.ImageProcessing;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public class BlurViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;

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
                int clamped = Clamp(value, MinRadius, MaxRadius);

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
            var radius = Radius;

            int w = _original.PixelWidth;
            int h = _original.PixelHeight;
            int stride = w * 4;

            byte[] src = new byte[h * stride];
            _original.CopyPixels(src, stride, 0);

            await Task.Run(() =>
            {
                var blurred = GaussianBlurHelper
                    .ApplyGaussianBlurFromBytes(src, w, h, stride, radius);

                blurred.Freeze();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PreviewImage = blurred;
                });
            });
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}