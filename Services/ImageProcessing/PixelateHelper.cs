using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services.ImageProcessing
{
    public static class PixelateHelper
    {
        public static WriteableBitmap ApplyPixelate(byte[] src, int w, int h, int stride, double dpiX, double dpiY, int blockSize)
        {
            if (blockSize < 2) blockSize = 2;

            byte[] dst = new byte[src.Length];
            int blocksX = (w + blockSize - 1) / blockSize;
            int blocksY = (h + blockSize - 1) / blockSize;

            Parallel.For(0, blocksY, by =>
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    int startX = bx * blockSize;
                    int startY = by * blockSize;
                    int endX = System.Math.Min(startX + blockSize, w);
                    int endY = System.Math.Min(startY + blockSize, h);

                    //Average color calculation
                    long sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                    int count = 0;

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            int i = y * stride + x * 4;
                            sumB += src[i];
                            sumG += src[i + 1];
                            sumR += src[i + 2];
                            sumA += src[i + 3];
                            count++;
                        }
                    }

                    byte avgB = (byte)(sumB / count);
                    byte avgG = (byte)(sumG / count);
                    byte avgR = (byte)(sumR / count);
                    byte avgA = (byte)(sumA / count);

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            int i = y * stride + x * 4;
                            dst[i] = avgB;
                            dst[i + 1] = avgG;
                            dst[i + 2] = avgR;
                            dst[i + 3] = avgA;
                        }
                    }
                }
            });

            var result = new WriteableBitmap(w, h, dpiX, dpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, w, h), dst, stride, 0);
            result.Freeze();
            return result;
        }
    }
}