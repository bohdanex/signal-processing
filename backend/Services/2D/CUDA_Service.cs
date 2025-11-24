using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.Cuda.API;
using System.Numerics;

namespace backend.Services._2D
{
    public class CUDA_Service
    {
        public float[,] ApplyKernel<TKernelParam>(float[,] data2D, int nx, int ny, Action<Index2D, ArrayView2D<Complex, Stride2D.DenseY>, ArrayView2D<Complex, Stride2D.DenseY>, TKernelParam> kernel, TKernelParam kernelParam, int deviceIndex = 0) where TKernelParam : struct
        {
            // 1. Set up ILGPU
            using var context = Context.Create((builder) => builder.EnableAlgorithms().AllAccelerators());
            var device = context.GetCudaDevice(deviceIndex);
            using var accelerator = device.CreateCudaAccelerator(context);

            // 2. Create cuFFT wrapper
            var cufft = new CuFFT(CuFFTAPIVersion.V11);

            // 3. Convert input values to double-precision Complex
            var inputComplex = new Complex[nx, ny];
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    inputComplex[i, j] = new Complex(data2D[i, j], 0);
                }
            }

            // 4. Allocate GPU buffers
            using var inputBuffer = accelerator.Allocate2DDenseY(inputComplex);
            using var freqBuffer = accelerator.Allocate2DDenseY<Complex>(new Index2D(nx, ny));

            // 5. Create FFT plan (double-precision complex)
            CuFFTException.ThrowIfFailed(
                cufft.API.Plan2D(out nint plan, nx, ny, CuFFTType.CUFFT_Z2Z)
            );

            // --------------------------
            // FORWARD FFT: input → freqBuffer
            // --------------------------
            CuFFTException.ThrowIfFailed(
                cufft.API.ExecZ2Z(
                    plan,
                    inputBuffer.View.BaseView,
                    freqBuffer.View.BaseView,
                    CuFFTDirection.FORWARD
                )
            );

            accelerator.Synchronize();

            // apply a kernel
            var kernelResultBuffer = accelerator.Allocate2DDenseY<Complex>(new Index2D(nx, ny));
            var kernelExecutor = accelerator.LoadAutoGroupedStreamKernel(kernel);
            kernelExecutor(new Index2D(nx, ny), freqBuffer.View, kernelResultBuffer.View, kernelParam);
            accelerator.Synchronize();


            using var outputBuffer = accelerator.Allocate2DDenseY<Complex>(new Index2D(nx, ny));

            // --------------------------a
            // INVERSE FFT: freqBuffer → outputBuffer
            // --------------------------
            CuFFTException.ThrowIfFailed(
                cufft.API.ExecZ2Z(
                    plan,
                    kernelResultBuffer.View.BaseView,
                    outputBuffer.View.BaseView,
                    CuFFTDirection.INVERSE
                )
            );

            // wait until fft is done
            accelerator.Synchronize();

            //// uFFT does NOT normalize, so load a kernel
            var normalizationKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Complex>, ArrayView<Complex>>(Kernels.NormalizeKernel);
            using var outputNormBuffer = accelerator.Allocate2DDenseY<Complex>(new Index2D(nx, ny));
            normalizationKernel((int)outputBuffer.Length, outputBuffer.View.BaseView, outputNormBuffer.View.BaseView);

            accelerator.Synchronize();

            var resultFromBuffer = outputNormBuffer.GetAsArray2D();

            var result = new float[nx, ny];
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    result[i, j] = (float)resultFromBuffer[i, j].Real;
                }
            }

            return result;
        }

        public float[,] ApplyGaussianBlur(float[,] data2D, int nx, int ny, float sigma, int deviceIndex = 0)
        {
            return ApplyKernel(data2D, nx, ny, Kernels.GaussianBlurKernel, sigma, deviceIndex);
        }

        public float[,] ApplyLaplacianFilter(float[,] data2D, int nx, int ny, float sigma, int deviceIndex = 0)
        {
            return ApplyKernel(data2D, nx, ny, Kernels.LaplacianKernel, sigma, deviceIndex);
        }
    }
}
