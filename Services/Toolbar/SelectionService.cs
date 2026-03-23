using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services
{
    public static class SelectionService
    {
        public static WriteableBitmap Copy(WriteableBitmap source, Int32Rect region)
        {
            region = ClampRect(region, source.PixelWidth, source.PixelHeight);
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

            region = ClampRect(region, source.PixelWidth, source.PixelHeight);

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

        private static Int32Rect ClampRect(Int32Rect r, int maxW, int maxH)
        {
            int x = System.Math.Max(0, r.X);
            int y = System.Math.Max(0, r.Y);
            int w = System.Math.Min(r.Width, maxW - x);
            int h = System.Math.Min(r.Height, maxH - y);
            return new Int32Rect(x, y, System.Math.Max(0, w), System.Math.Max(0, h));
        }
    }
}