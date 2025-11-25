using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Util;
using System.Numerics;

namespace backend.Services._2D
{
    public static class Kernels
    {
        public static void NormalizeKernel(Index1D i, ArrayView<Float2> data, ArrayView<Float2> output)
        {
            output[i] = data[i] / data.Length;
        }

        public static void GaussianBlurKernel(
            Index2D i,
            ArrayView2D<Float2, Stride2D.DenseY> data,
            ArrayView2D<Float2, Stride2D.DenseY> output,
            float sigma)
        {
            long nx = data.Extent.X;   // number of rows
            long ny = data.Extent.Y;   // number of columns

            int u = i.X;  // row index
            int v = i.Y;  // col index

            // ---- Convert indices to centered frequency space ----
            long uCentered = (u < nx / 2) ? u : u - nx;
            long vCentered = (v < ny / 2) ? v : v - ny;

            float dist2 = (uCentered * uCentered) + (vCentered * vCentered);

            // Gaussian low-pass filter
            float gaussian = XMath.Exp(-dist2 / (2f * sigma * sigma));

            // Multiply frequency by gaussian blur factor
            output[u, v] = data[u, v] * gaussian;
        }

        public static void LaplacianKernel(
            Index2D idx,
            ArrayView2D<Float2, Stride2D.DenseY> input,
            ArrayView2D<Float2, Stride2D.DenseY> output,
            float scale) // optional intensity
        {
            long nx = input.Extent.X;
            long ny = input.Extent.Y;

            int u = idx.X;
            int v = idx.Y;

            long uC = (u < nx / 2) ? u : u - nx;
            long vC = (v < ny / 2) ? v : v - ny;

            float dist2 = (uC * uC) + (vC * vC);

            // Laplacian frequency response: scale can adjust intensity
            float factor = -4f * MathF.PI * MathF.PI * dist2 * scale;

            output[u, v] = input[u, v] * factor;
        }

        public static void DilationKernel(
            Index2D index,
            ArrayView2D<float, Stride2D.DenseY> input,
            ArrayView2D<float, Stride2D.DenseY> output,
            int radius)
        {
            int x = index.X;
            int y = index.Y;

            // image dimensions
            long width = input.Extent.X;
            long height = input.Extent.Y;

            float maxVal = float.MinValue;

            // iterate the neighborhood
            for (int dy = -radius; dy <= radius; dy++)
            {
                int ny = y + dy;
                if (ny < 0 || ny >= height)
                    continue;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    int nx = x + dx;
                    if (nx < 0 || nx >= width)
                        continue;

                    float v = input[new Index2D(nx, ny)];
                    if (v > maxVal)
                        maxVal = v;
                }
            }

            output[index] = maxVal;
        }

        public static void ErosionKernel(
            Index2D index,
            ArrayView2D<float, Stride2D.DenseY> input,
            ArrayView2D<float, Stride2D.DenseY> output,
        int radius)
            {
                int x = index.X;
                int y = index.Y;

                long width = input.Extent.X;
                long height = input.Extent.Y;

                float min = float.PositiveInfinity;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    int ix = x + dx;
                    if (ix < 0 || ix >= width) continue;

                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int iy = y + dy;
                        if (iy < 0 || iy >= height) continue;

                        float val = input[ix, iy];
                        if (val < min)
                            min = val;
                    }
                }

                output[x, y] = min;
            }
    }
}
