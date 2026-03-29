using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageEditor.Services;
using ImageEditor.Services.Math;

public static class BrightnessHelper
{
    public static byte[] ApplyBrightnessBytes(byte[] src, int w, int h, int stride, int strength)
    {
        byte[] dst = new byte[src.Length];
        int delta = (int)(strength / 100f * 255);

        for (int i = 0; i < src.Length; i += 3)
        {
            dst[i] = Tools.Clamp(src[i] + delta);
            dst[i + 1] = Tools.Clamp(src[i + 1] + delta);
            dst[i + 2] = Tools.Clamp(src[i + 2] + delta);
        }
        return dst;
    }

    public static byte[] ApplyContrastBytes(byte[] src, int w, int h, int stride, int strength)
    {
        byte[] dst = new byte[src.Length];
        float factor = 1f + strength / 100f;

        for (int i = 0; i < src.Length; i += 3)
        {
            dst[i] = Tools.Clamp((int)((src[i] - 128) * factor + 128));
            dst[i + 1] = Tools.Clamp((int)((src[i + 1] - 128) * factor + 128));
            dst[i + 2] = Tools.Clamp((int)((src[i + 2] - 128) * factor + 128));
        }
        return dst;
    }
}