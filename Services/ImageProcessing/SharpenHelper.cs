using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageEditor.Services.Math;

namespace ImageEditor.Services.ImageProcessing
{
    public static class SharpenHelper
    {
        public static WriteableBitmap ApplySharpen(
            byte[] src, int w, int h, int stride,
            double dpiX, double dpiY, int strength, int radius)
        {
            byte[] blur = GaussianBlurHelper.ApplyGaussianBlurBytes(src, w, h, stride, radius);
            byte[] dst = new byte[src.Length];
            float amount = strength / 100f;

            for (int i = 0; i < src.Length; i += 4)
            {
                int b = src[i] + (int)((src[i] - blur[i]) * amount);
                int g = src[i + 1] + (int)((src[i + 1] - blur[i + 1]) * amount);
                int r = src[i + 2] + (int)((src[i + 2] - blur[i + 2]) * amount);
                dst[i] = Tools.Clamp(b);
                dst[i + 1] = Tools.Clamp(g);
                dst[i+ 2] = Tools.Clamp(r);
                dst[i + 3] = src[i + 3];
            }

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            result.Freeze();
            return result;
        }
    }
}