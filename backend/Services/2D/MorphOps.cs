using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;

namespace backend.Services._2D
{
    public class MorphOps
    {
        private class KernelFactory
        {
            public Action<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<float, Stride2D.DenseY>, int> DilationKernel { get; private set; }
            public Action<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<float, Stride2D.DenseY>, int> ErosionKernel { get; private set; }

            public KernelFactory(Accelerator accelerator)
            {
                DilationKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<float, Stride2D.DenseY>, int>(Kernels.DilationKernel);
                ErosionKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<float, Stride2D.DenseY>, int>(Kernels.ErosionKernel);
            }
        }

        private readonly Context _context;

        // mapping: typeof(AcceleratorType) -> Accelerator
        private readonly Dictionary<Type, Accelerator> _accelerators =
            new Dictionary<Type, Accelerator>();

        // mapping: typeof(AcceleratorType) -> KernelFactory
        private readonly Dictionary<Type, KernelFactory> _factories =
            new Dictionary<Type, KernelFactory>();


        public MorphOps()
        {
            _context = Context.Create(builder =>
                builder.EnableAlgorithms().AllAccelerators());

            // discover all devices
            foreach (var device in _context)
            {
                Accelerator acc = device.CreateAccelerator(_context);

                _accelerators[acc.GetType()] = acc;
                _factories[acc.GetType()] = new KernelFactory(acc);
            }
        }

        private Accelerator GetAccelerator<T>() where T : Accelerator
        {
            if (_accelerators.TryGetValue(typeof(T), out var acc))
                return acc;

            throw new InvalidOperationException(
                $"Accelerator of type {typeof(T).Name} is not available");
        }

        private KernelFactory GetFactory<T>() where T : Accelerator
        {
            if (_factories.TryGetValue(typeof(T), out var fac))
                return fac;

            throw new InvalidOperationException(
                $"KernelFactory for accelerator {typeof(T).Name} not found.");
        }

        private float[,] RunKernel<TKernelInput>(Accelerator accelerator, float[,] input, Action<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<float, Stride2D.DenseY>, TKernelInput> kernelExecutor, TKernelInput kernelInput) where TKernelInput : struct
        {
            int nx = input.GetLength(0);
            int ny = input.GetLength(1);
            var inputBuffer = accelerator.Allocate2DDenseY(input);
            var index = new Index2D(nx, ny);
            var outputBuffer = accelerator.Allocate2DDenseY<float>(index);

            kernelExecutor(index, inputBuffer, outputBuffer, kernelInput);
            accelerator.Synchronize();

            return outputBuffer.GetAsArray2D();
        }

        public float[,] Dilation<T>(float[,] input, int radius) where T : Accelerator
        {
            return RunKernel(GetAccelerator<T>(), input, GetFactory<T>().DilationKernel, radius);
        }

        public float[,] Erosion<T>(float[,] input, int radius) where T : Accelerator
        {
            return RunKernel(GetAccelerator<T>(), input, GetFactory<T>().ErosionKernel, radius);
        }
    }
}
