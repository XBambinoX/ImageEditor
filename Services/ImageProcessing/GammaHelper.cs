using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services.ImageProcessing
{
    public static class GammaHelper
    {
        public static WriteableBitmap ApplyGamma(byte[] src, int w, int h, int stride, double dpiX, double dpiY, double gamma)
        {
            byte[] dst = new byte[src.Length];
            byte[] lut = BuildLut(gamma);

            Parallel.For(0, h, y =>
            {
                int offset = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int i = offset + x * 3;
                    dst[i] = lut[src[i]];     // B
                    dst[i + 1] = lut[src[i + 1]]; // G
                    dst[i + 2] = lut[src[i + 2]]; // R
                }
            });

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgr24, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, result.BackBufferStride, 0);
            result.Freeze();
            return result;
        }

        private static byte[] BuildLut(double gamma)
        {
            byte[] lut = new byte[256];
            double inv = 1.0 / gamma;

            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double corrected = System.Math.Pow(normalized, inv);
                lut[i] = (byte)System.Math.Round(corrected * 255.0);
            }
            return lut;
        }
    }
}