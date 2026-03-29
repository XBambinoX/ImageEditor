using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageEditor.Services.Math;

namespace ImageEditor.Services.ImageProcessing
{
    public static class SharpenHelper
    {
        public static WriteableBitmap ApplySharpen(byte[] src, int w, int h, int stride, double dpiX, double dpiY, int strength, int radius)
        {
            byte[] blur = GaussianBlurHelper.ApplyGaussianBlurBytes(src, w, h, stride, radius);
            byte[] dst = new byte[src.Length];
            float amount = strength / 100f;

            for (int i = 0; i < src.Length; i += 3)
            {
                dst[i] = Tools.Clamp(src[i] + (int)((src[i] - blur[i]) * amount));                  // B
                dst[i + 1] = Tools.Clamp(src[i + 1] + (int)((src[i + 1] - blur[i + 1]) * amount)); // G
                dst[i + 2] = Tools.Clamp(src[i + 2] + (int)((src[i + 2] - blur[i + 2]) * amount)); // R
            }

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgr24, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            result.Freeze();
            return result;
        }
    }
}