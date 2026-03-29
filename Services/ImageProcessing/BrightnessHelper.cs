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
            dst[i] = Tools.Clamp(src[i] + delta); // B
            dst[i + 1] = Tools.Clamp(src[i + 1] + delta); // G
            dst[i + 2] = Tools.Clamp(src[i + 2] + delta); // R
        }
        return dst;
    }

    public static WriteableBitmap ApplyContrastBytes(byte[] src, int w, int h, int stride, double dpiX, double dpiY, int strength)
    {
        byte[] dst = new byte[src.Length];
        float factor = 1f + strength / 100f;

        for (int i = 0; i < src.Length; i += 3)
        {
            dst[i] = Tools.Clamp((int)((src[i] - 128) * factor + 128)); // B
            dst[i + 1] = Tools.Clamp((int)((src[i + 1] - 128) * factor + 128)); // G
            dst[i + 2] = Tools.Clamp((int)((src[i + 2] - 128) * factor + 128)); // R
        }

        var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgr24, null);
        result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
        result.Freeze();
        return result;
    }
}