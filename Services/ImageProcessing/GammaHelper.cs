using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services.ImageProcessing
{
    public static class GammaHelper
    {
        public static WriteableBitmap ApplyGamma(
            byte[] src, int w, int h, int stride,
            double dpiX, double dpiY, double gamma)
        {
            byte[] dst = new byte[src.Length];
            byte[] lut = BuildLut(gamma);

            Parallel.For(0, h, y =>
            {
                int offset = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int i = offset+x* 4;
                    dst[i] = lut[src[i]];          // B
                    dst[i + 1] = lut[src[i + 1]];  // G
                    dst[i + 2] = lut[src[i + 2]];  // R
                    dst[i + 3] = src[i + 3];       // A
                }
            });

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            result.Freeze();
            return result;
        }

        private static byte[] BuildLut(double gamma)
        {
            byte[] lut = new byte[256];
            double inv = 1.0 / gamma;

            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;                       // 0.0 .. 1.0
                double corrected = System.Math.Pow(normalized, inv); // gamma correction
                lut[i] = (byte)System.Math.Round(corrected * 255.0);
            }
            return lut;
        }
    }
}