using System.Threading.Tasks;
using ImageEditor.Services.Math;

namespace ImageEditor.Services.ImageProcessing
{
    internal static class GaussianBlurHelper
    {
        public static byte[] ApplyGaussianBlurBytes(byte[] src, int w,int h,int stride, int radius)
        {
            byte[] tmp = new byte[src.Length];
            byte[] dst = new byte[src.Length];

            BoxBlur(src, tmp, w, h, radius);
            BoxBlur(tmp, dst, w, h, radius);
            BoxBlur(dst, tmp, w, h, radius);

            return tmp;
        }

        private static void BoxBlur(byte[] src, byte[] dst, int w, int h, int r)
        {
            int stride = w * 4;
            int window = r * 2 + 1;

            byte[] tmp = new byte[src.Length];

            Parallel.For(0, h, y =>
            {
                int row = y * stride;

                int b = 0, g = 0, rC = 0;

                for (int i = -r; i <= r; i++)
                {
                    int x = Tools.Clamp(i, 0, w - 1);
                    int idx = row + x * 4;
                    b += src[idx];
                    g += src[idx + 1];
                    rC += src[idx + 2];
                }

                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;

                    tmp[idx] = (byte)(b / window);
                    tmp[idx + 1] = (byte)(g / window);
                    tmp[idx + 2] = (byte)(rC / window);
                    tmp[idx + 3] = src[idx + 3];

                    int x1 = Tools.Clamp(x - r, 0, w - 1);
                    int x2 = Tools.Clamp(x + r + 1, 0, w - 1);

                    int i1 = row + x1 * 4;
                    int i2 = row + x2 * 4;

                    b += src[i2] - src[i1];
                    g += src[i2 + 1] - src[i1 + 1];
                    rC += src[i2 + 2] - src[i1 + 2];
                }
            });

            Parallel.For(0, w, x =>
            {
                int b = 0, g = 0, rC = 0;

                for (int i = -r; i <= r; i++)
                {
                    int y = Tools.Clamp(i, 0, h - 1);
                    int idx = y * stride + x * 4;
                    b += tmp[idx];
                    g += tmp[idx + 1];
                    rC += tmp[idx + 2];
                }

                for (int y = 0; y < h; y++)
                {
                    int idx = y * stride + x * 4;

                    dst[idx] = (byte)(b / window);
                    dst[idx + 1] = (byte)(g / window);
                    dst[idx + 2] = (byte)(rC / window);
                    dst[idx + 3] = tmp[idx + 3];

                    int y1 = Tools.Clamp(y - r, 0, h - 1);
                    int y2 = Tools.Clamp(y + r + 1, 0, h - 1);

                    int i1 = y1 * stride + x * 4;
                    int i2 = y2 * stride + x * 4;

                    b += tmp[i2] - tmp[i1];
                    g += tmp[i2 + 1] - tmp[i1 + 1];
                    rC += tmp[i2 + 2] - tmp[i1 + 2];
                }
            });
        }
    }
}
