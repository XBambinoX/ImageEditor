using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services
{
    public static class DrawingService
    {
        public static void DrawCircle(WriteableBitmap bitmap, int cx, int cy, int radius, Color color, double hardness = 1.0)
        {
            if (bitmap == null) return;

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            int x0 = System.Math.Max(0, cx - radius);
            int y0 = System.Math.Max(0, cy - radius);
            int x1 = System.Math.Min(width - 1, cx + radius);
            int y1 = System.Math.Min(height - 1, cy + radius);

            if (x0 > x1 || y0 > y1) return;

            bitmap.Lock();

            unsafe
            {
                byte* buffer = (byte*)bitmap.BackBuffer;
                int stride = bitmap.BackBufferStride;
                double r = System.Math.Max(0.5, radius - 0.5);
                double hardEdge = r * hardness;

                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        double dx = x - cx;
                        double dy = y - cy;
                        double dist = System.Math.Sqrt(dx * dx + dy * dy);

                        if (dist > r) continue;

                        double alpha;
                        if (dist <= hardEdge)
                            alpha = 1.0;
                        else
                            alpha = 1.0 - (dist - hardEdge) / (r - hardEdge + 0.001);

                        alpha = System.Math.Max(0, System.Math.Min(1, alpha));

                        byte* pixel = buffer + y * stride + x * 4;

                        double srcA = color.A / 255.0 * alpha;
                        double dstA = pixel[3] / 255.0;
                        double outA = srcA + dstA * (1.0 - srcA);

                        if (outA > 0)
                        {
                            pixel[0] = (byte)((color.B * srcA + pixel[0] * dstA * (1.0 - srcA)) / outA);
                            pixel[1] = (byte)((color.G * srcA + pixel[1] * dstA * (1.0 - srcA)) / outA);
                            pixel[2] = (byte)((color.R * srcA + pixel[2] * dstA * (1.0 - srcA)) / outA);
                            pixel[3] = (byte)(outA * 255);
                        }
                    }
                }
            }

            int dirtyW = System.Math.Min(x1 - x0 + 1, width - x0);
            int dirtyH = System.Math.Min(y1 - y0 + 1, height - y0);
            bitmap.AddDirtyRect(new Int32Rect(x0, y0, dirtyW, dirtyH));
            bitmap.Unlock();
        }

        public static void DrawLine(WriteableBitmap bitmap, Point from, Point to, int radius, Color color, double hardness = 1.0)
        {
            if (bitmap == null) return;

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double dist = System.Math.Sqrt(dx * dx + dy * dy);
            int step = System.Math.Max(1, radius / 2);
            int steps = System.Math.Max(1, (int)(dist / step));

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                int cx = (int)(from.X + dx * t);
                int cy = (int)(from.Y + dy * t);
                DrawCircle(bitmap, cx, cy, radius, color, hardness);
            }
        }
    }
}