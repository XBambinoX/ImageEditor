using System.Threading.Tasks;

namespace ImageEditor.Services.ImageProcessing
{
    public static class GammaHelper
    {
        public static byte[] ApplyGamma(byte[] src, int w, int h, int stride, double gamma)
        {
            byte[] dst = new byte[src.Length];
            byte[] lut = BuildLut(gamma);

            Parallel.For(0, h, y =>
            {
                int offset = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int i = offset + x * 3;
                    dst[i] = lut[src[i]];         //B
                    dst[i + 1] = lut[src[i + 1]]; //G
                    dst[i + 2] = lut[src[i + 2]]; //R
                }
            });

            return dst;
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