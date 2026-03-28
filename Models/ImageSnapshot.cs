using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageEditor.Models
{
    public class ImageSnapshot
    {
        public byte[] Pixels { get; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }

        public ImageSnapshot(WriteableBitmap bmp, Int32Rect region)
        {
            X = region.X;
            Y = region.Y;
            Width = region.Width;
            Height = region.Height;
            Stride = Width * 4;
            Pixels = new byte[Height * Stride];
            bmp.CopyPixels(region, Pixels, Stride, 0);
        }

        public ImageSnapshot(byte[] pixels, int x, int y, int width, int height, int stride)
        {
            Pixels = pixels;
            X = x; Y = y;
            Width = width; Height = height;
            Stride = stride;
        }

        public void Restore(WriteableBitmap bmp)
        {
            bmp.WritePixels(new Int32Rect(X, Y, Width, Height), Pixels, Stride, 0);
        }
    }
}