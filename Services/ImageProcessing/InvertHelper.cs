using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace ImageEditor.Services.ImageProcessing
{
    public static class InvertHelper
    {
        private static WriteableBitmap _ApplyInvert(byte[] src, int w, int h, int stride, double dpiX, double dpiY)
        {
            byte[] dst = new byte[src.Length];

            Parallel.For(0, h, y =>
            {
                int offset = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int i = offset + x * 4;
                    dst[i] = (byte)(255 - src[i]);         // B
                    dst[i + 1] = (byte)(255 - src[i + 1]); // G
                    dst[i + 2] = (byte)(255 - src[i + 2]); // R
                    dst[i + 3] = src[i + 3];               // A
                }
            });

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            result.Freeze();
            return result;
        }

        public static WriteableBitmap ApplyInvert(BitmapSource image)
        {
            int w = image.PixelWidth;
            int h = image.PixelHeight;
            int stride = w * 4;
            double dpiX = image.DpiX;
            double dpiY = image.DpiY;

            byte[] pixels = new byte[h * stride];
            image.CopyPixels(pixels, stride, 0);

            return _ApplyInvert(pixels, w, h, stride, dpiX, dpiY);
        }
    }
}