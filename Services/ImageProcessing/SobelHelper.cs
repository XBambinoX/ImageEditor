using ImageEditor.Services.Math;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services.ImageProcessing
{
    public static class SobelHelper
    {
        public static WriteableBitmap ApplySobel(byte[] src, int w, int h, int stride, double dpiX, double dpiY, int threshold, bool colorize)
        {
            byte[] gray = GrayscaleHelper.ApplyGrayscaleBytes(src);
            byte[] dst = new byte[src.Length];

            int[] kX = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
            int[] kY = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int gx = 0, gy = 0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int px = Tools.Clamp(x + kx, 0, w - 1);
                            int py = Tools.Clamp(y + ky, 0, h - 1);
                            int idx = py * stride + px * 4;
                            int k = (ky + 1) * 3 + (kx + 1);

                            gx += gray[idx] * kX[k];
                            gy += gray[idx] * kY[k];
                        }
                    }

                    int magnitude = (int)System.Math.Sqrt(gx * gx + gy * gy);
                    magnitude = Tools.Clamp(magnitude, 0, 255);

                    int i = y * stride + x * 4;

                    if (magnitude < threshold)
                    {
                        dst[i] = 0;
                        dst[i + 1] = 0;
                        dst[i + 2] = 0;
                    }
                    else if (colorize)
                    {
                        double angle = System.Math.Atan2(gy, gx) + System.Math.PI; // 0..2pi
                        HsvToRgb(angle / (2 * System.Math.PI) * 360, 1.0, magnitude / 255.0,
                            out byte cr, out byte cg, out byte cb);
                        dst[i] = cb;
                        dst[i + 1] = cg;
                        dst[i + 2] = cr;
                    }
                    else
                    {
                        byte m = (byte)magnitude;
                        dst[i] = m;
                        dst[i + 1] = m;
                        dst[i + 2] = m;
                    }

                    dst[i + 3] = src[i + 3];
                }
            }

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            result.Freeze();
            return result;
        }

        private static void HsvToRgb(double h, double s, double v,
            out byte r, out byte g, out byte b)
        {
            int hi = (int)(h / 60) % 6;
            double f = h / 60 - System.Math.Floor(h / 60);
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            double dr, dg, db;
            switch (hi)
            {
                case 0:
                    dr = v; dg = t; db = p;
                    break;

                case 1:
                    dr = q; dg = v; db = p;
                    break;

                case 2:
                    dr = p; dg = v; db = t;
                    break;

                case 3:
                    dr = p; dg = q; db = v;
                    break;

                case 4:
                    dr = t; dg = p; db = v;
                    break;

                default:
                    dr = v; dg = p; db = q;
                    break;
            }

            r = (byte)(dr * 255);
            g = (byte)(dg * 255);
            b = (byte)(db * 255);
        }
    }
}