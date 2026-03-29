using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageEditor.Models
{
    public class ImageSnapshot
    {
        public byte[] Pixels { get; }
        public Int32Rect Region { get; }
        public int Stride { get; }

        public ImageSnapshot(WriteableBitmap bmp, Int32Rect region)
        {
            int x = System.Math.Max(0, region.X);
            int y = System.Math.Max(0, region.Y);
            int x2 = System.Math.Min(bmp.PixelWidth, region.X + region.Width);
            int y2 = System.Math.Min(bmp.PixelHeight, region.Y + region.Height);

            int w = System.Math.Max(0, x2 - x);
            int h = System.Math.Max(0, y2 - y);

            Region = new Int32Rect(x, y, w, h);
            Stride = Region.Width * 3;
            Pixels = new byte[Region.Height * Stride];

            if (Region.Width > 0 && Region.Height > 0)
                bmp.CopyPixels(Region, Pixels, Stride, 0);
        }

        public void Restore(WriteableBitmap bmp)
        {
            bmp.WritePixels(Region, Pixels, Stride, 0);
        }

        public static ImageSnapshot CreateDiff(WriteableBitmap bmp, Int32Rect region, byte[] previousPixels)
        {
            int stride = region.Width * 3;
            byte[] currentPixels = new byte[region.Height * stride];
            bmp.CopyPixels(region, currentPixels, stride, 0);

            bool hasChange = false;
            for (int i = 0; i < currentPixels.Length; i++)
            {
                if (currentPixels[i] != previousPixels[i])
                {
                    hasChange = true;
                    break;
                }
            }

            if (!hasChange) return null;

            return new ImageSnapshot(currentPixels, region.X, region.Y, region.Width, region.Height, stride);
        }

        public ImageSnapshot(byte[] pixels, int x, int y, int width, int height, int stride)
        {
            Pixels = pixels;
            Region = new Int32Rect(x, y, width, height);
            Stride = stride;
        }
    }
}