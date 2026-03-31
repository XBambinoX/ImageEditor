using ImageEditor.Services.Math;

namespace ImageEditor.Services.ImageProcessing
{
    public static class SharpenHelper
    {
        public static byte[] ApplySharpen(byte[] src, int w, int h, int stride, int strength, int radius)
        {
            byte[] blur = GaussianBlurHelper.ApplyGaussianBlurBytes(src, w, h, stride, radius);
            byte[] dst = new byte[src.Length];
            float amount = strength / 100f;

            for (int i = 0; i < src.Length; i += 3)
            {
                dst[i] = Tools.Clamp(src[i] + (int)((src[i] - blur[i]) * amount));
                dst[i + 1] = Tools.Clamp(src[i + 1] + (int)((src[i + 1] - blur[i + 1]) * amount));
                dst[i + 2] = Tools.Clamp(src[i + 2] + (int)((src[i + 2] - blur[i + 2]) * amount));
            }

            return dst;
        }
    }
}