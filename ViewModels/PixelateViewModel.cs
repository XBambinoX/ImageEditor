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
    public class PixelateViewModel : BaseViewModel
    {
        private readonly WriteableBitmap _original;
        private CancellationTokenSource _cts;
        private WriteableBitmap _preview;

        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set { _preview = value; OnPropertyChanged(); }
        }

        private int _blockSize = 10;
        public int BlockSize
        {
            get => _blockSize;
            set
            {
                _blockSize = Tools.Clamp(value, 2, 100);
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public int MinBlockSize => 2;
        public int MaxBlockSize => 100;

        public WriteableBitmap ResultImage { get; private set; }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseBlockSizeCommand { get; }
        public ICommand DecreaseBlockSizeCommand { get; }

        public Action<bool> CloseAction;

        public PixelateViewModel(WriteableBitmap source)
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

            IncreaseBlockSizeCommand = new RelayCommand(_ => BlockSize++);
            DecreaseBlockSizeCommand = new RelayCommand(_ => BlockSize--);

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

                int blockSize = _blockSize;
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
                    return PixelateHelper.ApplyPixelate(pixels, w, h, stride, dpiX, dpiY, blockSize);
                }, token);

                if (token.IsCancellationRequested || result == null) return;

                PreviewImage = result;
            }
            catch (TaskCanceledException) { }
        }
    }
}