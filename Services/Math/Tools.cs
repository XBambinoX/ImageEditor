using System.Windows;

namespace ImageEditor.Services.Math
{
    internal static class Tools
    {
        public static byte Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return (byte)v;
        }

        public static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public static double Clamp(double v, double min, double max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public static Int32Rect ClampRect(Int32Rect r, int maxW, int maxH)
        {
            int x = System.Math.Max(0, r.X);
            int y = System.Math.Max(0, r.Y);
            int w = System.Math.Min(r.Width, maxW - x);
            int h = System.Math.Min(r.Height, maxH - y);
            return new Int32Rect(x, y, System.Math.Max(0, w), System.Math.Max(0, h));
        }
    }
}
