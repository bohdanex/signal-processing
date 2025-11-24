using Carter;
using ILGPU;
using ILGPU.Runtime;

internal class ILGPU_InfoResponse
{
    public ILGPU_InfoResponse(AcceleratorType acceleratorType, string acceleratorName, string deviceName, int maxThreads, long memorySize)
    {
        AcceleratorType = acceleratorType;
        AcceleratorName = acceleratorName;
        DeviceName = deviceName;
        MaxThreads = maxThreads;
        MemorySize = memorySize;
    }

    public AcceleratorType AcceleratorType { get; private set; }
    public string AcceleratorName { get; private set; }
    public string DeviceName { get; private set; }
    public int MaxThreads { get; private set; }
    public long MemorySize { get; private set; }
}

namespace backend.Modules
{
    public class ILGPU_InfoModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("ilgpu", () =>
            {
                Context context = Context.Create(builder => builder.AllAccelerators());
                List<ILGPU_InfoResponse> response = new();

                foreach (Device device in context)
                {
                    response.Add(new ILGPU_InfoResponse(device.AcceleratorType, device.AcceleratorType.ToString(), device.Name, device.MaxNumThreadsPerMultiprocessor, device.MemorySize));
                }

                return response;
            });
        }
    }
}
