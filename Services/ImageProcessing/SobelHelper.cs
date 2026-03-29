using System.Threading.Tasks;

namespace ImageEditor.Services.ImageProcessing
{
    public static class SobelHelper
    {
        public static byte[] ApplySobel(byte[] src, int w, int h, int stride, int threshold, bool colorize)
        {
            byte[] gray = GrayscaleHelper.ApplyGrayscaleBytes(src);
            byte[] dst = new byte[src.Length];

            Parallel.For(0, h, y =>
            {
                int rowOffset = y * stride;

                for (int x = 0; x < w; x++)
                {
                    int yPrev = y == 0 ? 0 : y - 1;
                    int yNext = y == h - 1 ? h - 1 : y + 1;
                    int xPrev = x == 0 ? 0 : x - 1;
                    int xNext = x == w - 1 ? w - 1 : x + 1;

                    int offTop = yPrev * stride;
                    int offMid = y * stride;
                    int offBot = yNext * stride;

                    int tl = gray[offTop + xPrev * 3];
                    int tc = gray[offTop + x * 3];
                    int tr = gray[offTop + xNext * 3];
                    int ml = gray[offMid + xPrev * 3];
                    int mr = gray[offMid + xNext * 3];
                    int bl = gray[offBot + xPrev * 3];
                    int bc = gray[offBot + x * 3];
                    int br = gray[offBot + xNext * 3];

                    int gx = (tr + 2 * mr + br) - (tl + 2 * ml + bl);
                    int gy = (bl + 2 * bc + br) - (tl + 2 * tc + tr);

                    int magnitude = System.Math.Min(255, System.Math.Abs(gx) + System.Math.Abs(gy));
                    int i = rowOffset + x * 3;

                    if (magnitude < threshold)
                    {
                        dst[i] = dst[i + 1] = dst[i + 2] = 0;
                    }
                    else if (colorize)
                    {
                        double angle = System.Math.Atan2(gy, gx) + System.Math.PI;
                        HsvToRgb(angle / (2 * System.Math.PI) * 360, 1.0, magnitude / 255.0,
                            out byte cr, out byte cg, out byte cb);
                        dst[i] = cb;
                        dst[i + 1] = cg;
                        dst[i + 2] = cr;
                    }
                    else
                    {
                        byte m = (byte)magnitude;
                        dst[i] = dst[i + 1] = dst[i + 2] = m;
                    }
                }
            });

            return dst;
        }

        private static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
        {
            int hi = (int)(h / 60) % 6;
            double f = h / 60 - System.Math.Floor(h / 60);
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            double dr, dg, db;
            switch (hi)
            {
                case 0: dr = v; dg = t; db = p; break;
                case 1: dr = q; dg = v; db = p; break;
                case 2: dr = p; dg = v; db = t; break;
                case 3: dr = p; dg = q; db = v; break;
                case 4: dr = t; dg = p; db = v; break;
                default: dr = v; dg = p; db = q; break;
            }

            r = (byte)(dr * 255);
            g = (byte)(dg * 255);
            b = (byte)(db * 255);
        }
    }
}