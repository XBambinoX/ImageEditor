using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageEditor.Services.Math;

namespace ImageEditor.Services
{
    public static class SelectionService
    {
        public static WriteableBitmap Copy(WriteableBitmap source, Int32Rect region)
        {
            region = Tools.ClampRect(region, source.PixelWidth, source.PixelHeight);
            if (region.Width <= 0 || region.Height <= 0) return null;

            var result = new WriteableBitmap(region.Width, region.Height, source.DpiX, source.DpiY, source.Format, null);
            int stride = region.Width * (source.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[region.Height * stride];
            source.CopyPixels(region, pixels, stride, 0);
            result.WritePixels(new Int32Rect(0, 0, region.Width, region.Height), pixels, stride, 0);
            return result;
        }

        public static WriteableBitmap Cut(WriteableBitmap source, Int32Rect region)
        {
            if (source == null || source.IsFrozen) return null;

            var copied = Copy(source, region);
            if (copied == null) return null;

            region = Tools.ClampRect(region, source.PixelWidth, source.PixelHeight);

            int stride = region.Width * (source.Format.BitsPerPixel / 8);
            byte[] white = new byte[region.Height * stride];

            for (int i = 0; i < white.Length; i += 4)
            {
                white[i] = 255;     // B
                white[i + 1] = 255; // G
                white[i + 2] = 255; // R
                white[i + 3] = 255; // A
            }

            source.WritePixels(region, white, stride, 0);
            return copied;
        }

        public static void Paste(WriteableBitmap target, WriteableBitmap clipboard, int destX, int destY)
        {
            if (clipboard == null || target == null) return;

            int srcW = clipboard.PixelWidth;
            int srcH = clipboard.PixelHeight;

            // Clamp destination to target bounds
            int x0 = System.Math.Max(0, destX);
            int y0 = System.Math.Max(0, destY);
            int x1 = System.Math.Min(target.PixelWidth, destX + srcW);
            int y1 = System.Math.Min(target.PixelHeight, destY + srcH);

            if (x0 >= x1 || y0 >= y1) return;

            int drawW = x1 - x0;
            int drawH = y1 - y0;

            int bpp = clipboard.Format.BitsPerPixel / 8;
            int srcStride = srcW * bpp;
            byte[] pixels = new byte[srcH * srcStride];
            clipboard.CopyPixels(pixels, srcStride, 0);

            int cropX = x0 - destX;
            int cropY = y0 - destY;
            int dstStride = drawW * bpp;
            byte[] cropped = new byte[drawH * dstStride];

            for (int row = 0; row < drawH; row++)
            {
                int srcOffset = (cropY + row) * srcStride + cropX * bpp;
                int dstOffset = row * dstStride;
                Array.Copy(pixels, srcOffset, cropped, dstOffset, dstStride);
            }

            target.WritePixels(new Int32Rect(x0, y0, drawW, drawH), cropped, dstStride, 0);
        }

        public static WriteableBitmap Resize(WriteableBitmap source, int newW, int newH)
        {
            if (source == null || newW <= 0 || newH <= 0) return source;

            int srcW = source.PixelWidth;
            int srcH = source.PixelHeight;
            int bpp = source.Format.BitsPerPixel / 8;
            int srcStride = srcW * bpp;

            byte[] srcPixels = new byte[srcH * srcStride];
            source.CopyPixels(srcPixels, srcStride, 0);

            int dstStride = newW * bpp;
            byte[] dstPixels = new byte[newH * dstStride];

            for (int y = 0; y < newH; y++)
            {
                double fy = (double)y / (newH - 1) * (srcH - 1);
                int y0 = System.Math.Max(0, (int)fy);
                int y1 = System.Math.Min(srcH - 1, y0 + 1);
                double ty = fy - y0;

                for (int x = 0; x < newW; x++)
                {
                    double fx = (double)x / (newW - 1) * (srcW - 1);
                    int x0 = System.Math.Max(0, (int)fx);
                    int x1 = System.Math.Min(srcW - 1, x0 + 1);
                    double tx = fx - x0;

                    for (int c = 0; c < bpp; c++)
                    {
                        double top = srcPixels[y0 * srcStride + x0 * bpp + c] * (1 - tx)
                                   + srcPixels[y0 * srcStride + x1 * bpp + c] * tx;
                        double bot = srcPixels[y1 * srcStride + x0 * bpp + c] * (1 - tx)
                                   + srcPixels[y1 * srcStride + x1 * bpp + c] * tx;
                        dstPixels[y * dstStride + x * bpp + c] = (byte)(top * (1 - ty) + bot * ty);
                    }
                }
            }

            var result = new WriteableBitmap(newW, newH, source.DpiX, source.DpiY, source.Format, null);
            result.WritePixels(new Int32Rect(0, 0, newW, newH), dstPixels, dstStride, 0);
            return result;
        }
    }
}