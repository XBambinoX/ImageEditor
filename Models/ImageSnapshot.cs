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
            Region = region;
            Stride = region.Width * 3;
            Pixels = new byte[region.Height * Stride];
            bmp.CopyPixels(region, Pixels, Stride, 0);
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