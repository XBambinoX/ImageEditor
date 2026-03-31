namespace ImageEditor.Services.ImageProcessing
{
    public static class GrayscaleHelper
    {
        public static byte[] ApplyGrayscale(byte[] src, int w, int h, int stride, int mode = 0, int intensity = 50)
        {
            byte[] dst = new byte[src.Length];
            float blend = intensity / 100f;

            for (int i = 0; i < src.Length; i += 3)
            {
                byte b = src[i];
                byte g = src[i + 1];
                byte r = src[i + 2];


                byte gray;
                switch (mode)
                {
                    case 0:
                        gray = (byte)(r * 0.299f + g * 0.587f + b * 0.114f);
                        break;
                    case 1:
                        gray = (byte)((r + g + b) / 3f);
                        break;
                    case 2:
                        gray = (byte)((System.Math.Max(r, System.Math.Max(g, b)) + System.Math.Min(r, System.Math.Min(g, b))) / 2f);
                        break;
                    default:
                        gray = 0;
                        break;
                }

                dst[i] = (byte)(b + (gray - b) * blend);
                dst[i + 1] = (byte)(g + (gray - g) * blend);
                dst[i + 2] = (byte)(r + (gray - r) * blend);
            }

            return dst;
        }

        public static byte[] ApplyGrayscaleBytes(byte[] src, int mode = 0)
        {
            byte[] dst = new byte[src.Length];

            for (int i = 0; i < src.Length; i += 3)
            {
                byte b = src[i];
                byte g = src[i + 1];
                byte r = src[i + 2];

                byte gray;
                switch (mode)
                {
                    case 0:
                        gray = (byte)(r * 0.299f + g * 0.587f + b * 0.114f);
                        break;
                    case 1:
                        gray = (byte)((r + g + b) / 3f);
                        break;
                    case 2:
                        gray = (byte)((System.Math.Max(r, System.Math.Max(g, b)) + System.Math.Min(r, System.Math.Min(g, b))) / 2f);
                        break;
                    default:
                        gray = 0;
                        break;
                }

                dst[i] = gray;
                dst[i + 1] = gray;
                dst[i + 2] = gray;
            }

            return dst;
        }
    }
}