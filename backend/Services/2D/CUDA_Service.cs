using backend.Services.Implementation;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.Cuda.API;
using ILGPU.Util;

namespace backend.Services._2D
{
    public class CUDA_Service
    {
        private Context _context;
        private CudaDevice _device;
        private Accelerator _accelerator;
        private CuFFTAPI _api;
        private Action<Index2D, ArrayView2D<Float2, Stride2D.DenseY>, ArrayView2D<Float2, Stride2D.DenseY>, float> _gaussianBlurKernel;
        private Action<Index2D, ArrayView2D<Float2, Stride2D.DenseY>, ArrayView2D<Float2, Stride2D.DenseY>, float> _laplacianKernel;
        private Action<Index1D, ArrayView<Float2>, ArrayView<Float2>> _normalizeKernel;

        public CUDA_Service()
        {
            _context = Context.Create((builder) => builder.EnableAlgorithms().AllAccelerators());
            _device = _context.GetCudaDevice(0);
            _accelerator = _device.CreateCudaAccelerator(_context);
            _api = new CuFFT(CuFFTAPIVersion.V11).API;
            _gaussianBlurKernel = _accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<Float2, Stride2D.DenseY>, ArrayView2D<Float2, Stride2D.DenseY>, float>(Kernels.GaussianBlurKernel);
            _laplacianKernel = _accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<Float2, Stride2D.DenseY>, ArrayView2D<Float2, Stride2D.DenseY>, float>(Kernels.LaplacianKernel);
            _normalizeKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Float2>, ArrayView<Float2>>(Kernels.NormalizeKernel);
        }

        public Float2[,] ApplyKernel<TKernelParam>(Float2[,] data2D, int nx, int ny, Action<Index2D, ArrayView2D<Float2, Stride2D.DenseY>, ArrayView2D<Float2, Stride2D.DenseY>, TKernelParam> kernelExecutor, TKernelParam kernelParam, int deviceIndex = 0) where TKernelParam : struct
        {
            // 1. Allocate GPU buffers
            using var inputBuffer = _accelerator.Allocate2DDenseY(data2D);
            using var freqBuffer = _accelerator.Allocate2DDenseY<Float2>(new Index2D(nx, ny));
            using var kernelResultBuffer = _accelerator.Allocate2DDenseY<Float2>(new Index2D(nx, ny));
            using var outputBuffer = _accelerator.Allocate2DDenseY<Float2>(new Index2D(nx, ny));

            // 2. Create FFT plan
            _api.Plan2D(out nint plan, nx, ny, CuFFTType.CUFFT_C2C);

            // --------------------------
            // FORWARD FFT: input → freqBuffer
            // --------------------------
            _api.ExecC2C(
                plan,
                inputBuffer.View.BaseView,
                freqBuffer.View.BaseView,
                CuFFTDirection.FORWARD
            );

            _accelerator.Synchronize();

            // apply a kernel
            
            kernelExecutor(new Index2D(nx, ny), freqBuffer.View, kernelResultBuffer.View, kernelParam);
            _accelerator.Synchronize();


            // --------------------------a
            // INVERSE FFT: freqBuffer → outputBuffer
            // --------------------------
            _api.ExecC2C(
                plan,
                kernelResultBuffer.View.BaseView,
                outputBuffer.View.BaseView,
                CuFFTDirection.INVERSE
            );

            // wait until fft is done
            _accelerator.Synchronize();

            //// uFFT does NOT normalize, so load a kernel
            using var outputNormBuffer = _accelerator.Allocate2DDenseY<Float2>(new Index2D(nx, ny));
            _normalizeKernel((int)outputBuffer.Length, outputBuffer.View.BaseView, outputNormBuffer.View.BaseView);

            _accelerator.Synchronize();
                
            var result = outputNormBuffer.GetAsArray2D();

            _api.Destroy(plan);

            return result;
        }

        public Float2[,] ApplyGaussianBlur(Float2[,] data2D, int nx, int ny, float sigma, int deviceIndex = 0)
        {
            _accelerator.ClearCache(ClearCacheMode.Everything);
            return ApplyKernel(data2D, nx, ny, _gaussianBlurKernel, sigma, deviceIndex);
        }

        public Float2[,] ApplyLaplacianFilter(Float2[,] data2D, int nx, int ny, float sigma, int deviceIndex = 0)
        {
            _accelerator.ClearCache(ClearCacheMode.Everything);
            return ApplyKernel(data2D, nx, ny, _laplacianKernel, sigma, deviceIndex);
        }
    }
}
