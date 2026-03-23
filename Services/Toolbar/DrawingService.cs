using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services
{
    public static class DrawingService
    {
        public static void DrawCircle(WriteableBitmap bitmap, int cx, int cy, int radius, Color color)
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
                int r2 = radius * radius;

                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        int dx = x - cx;
                        int dy = y - cy;
                        if (dx * dx + dy * dy <= r2)
                        {
                            byte* pixel = buffer + y * stride + x * 4;
                            pixel[0] = color.B;
                            pixel[1] = color.G;
                            pixel[2] = color.R;
                            pixel[3] = color.A;
                        }
                    }
                }
            }

            int dirtyW = System.Math.Min(x1 - x0 + 1, width - x0);
            int dirtyH = System.Math.Min(y1 - y0 + 1, height - y0);

            bitmap.AddDirtyRect(new Int32Rect(x0, y0, dirtyW, dirtyH));
            bitmap.Unlock();
        }

        public static void DrawLine(WriteableBitmap bitmap, Point from, Point to, int radius, Color color)
        {
            if (bitmap == null) return;

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double dist = System.Math.Sqrt(dx * dx + dy * dy);
            int steps = System.Math.Max(1, (int)dist);

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                int cx = (int)(from.X + dx * t);
                int cy = (int)(from.Y + dy * t);
                DrawCircle(bitmap, cx, cy, radius, color);
            }
        }
    }
}