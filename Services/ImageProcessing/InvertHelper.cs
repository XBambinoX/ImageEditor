using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services.ImageProcessing
{
    public static class InvertHelper
    {
        public static WriteableBitmap ApplyInvert(BitmapSource image)
        {
            int w = image.PixelWidth;
            int h = image.PixelHeight;
            double dpiX = image.DpiX;
            double dpiY = image.DpiY;

            var wb = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgr24, null);
            int stride = wb.BackBufferStride;

            byte[] pixels = new byte[h * stride];
            image.CopyPixels(pixels, stride, 0);

            byte[] dst = new byte[pixels.Length];

            Parallel.For(0, h, y =>
            {
                int offset = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int i = offset + x * 3;
                    dst[i] = (byte)(255 - pixels[i]);
                    dst[i + 1] = (byte)(255 - pixels[i + 1]);
                    dst[i + 2] = (byte)(255 - pixels[i + 2]);
                }
            });

            wb.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            return wb;
        }
    }
}