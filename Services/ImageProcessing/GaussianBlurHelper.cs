using System;

namespace ImageEditor.Services.ImageProcessing
{
    internal static class GaussianBlurHelper
    {
        public static float[] CreateGaussianKernelFast(int radius, float sigma)
        {
            int size = radius * 2 + 1;
            float[] kernel = new float[size];

            float sigma2 = 2 * sigma * sigma;
            float sum = 0;

            for (int i = -radius; i <= radius; i++)
            {
                float v = (float)Math.Exp(-(i * i) / sigma2);
                kernel[i + radius] = v;
                sum += v;
            }

            for (int i = 0; i < size; i++)
                kernel[i] /= sum;

            return kernel;
        }
    }
}
