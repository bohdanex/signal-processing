using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Hardware.Info;
using System.Management;
using SharpDX.DXGI;

#pragma warning disable CA1416
namespace backend.Utils
{
    public class GPUMemoryUsage
    {
        // Property for the GPU instance name/identifier
        public string Name { get; set; }
        public ulong Dedicated { get; set; }
        public ulong Shared { get; set; }
    }

    public class GPUMemoryMonitor
    {
        // Helper class to group counters with their instance name
        private class GpuInstance
        {
            public string Name { get; }
            public PerformanceCounter DedicatedCounter { get; set; }
            public PerformanceCounter SharedCounter { get; set; }

            public GpuInstance(string name)
            {
                Name = name;
            }
        }

        private readonly PerformanceCounterCategory _category;
        // Stores all GPU instances with their associated counters
        private readonly List<GpuInstance> _gpuInstances = new();

        public GPUMemoryMonitor(HardwareInfo hardwareInfo)
        {
            hardwareInfo.RefreshVideoControllerList();

            _category = new("GPU Adapter Memory");
            string[] counterNames = _category.GetInstanceNames(); // These are the GPU identifiers
            Factory factory = new Factory1();

            foreach (string counterName in counterNames)
            {
                long luid = 0;
                var luidParsed = LuidParser.TryParseLuid(counterName, out luid);
                var instance = new GpuInstance(counterName);
                if (luidParsed)
                {
                    var adapter = factory.Adapters.FirstOrDefault((adapter) => adapter.Description.Luid == luid);
                    if (adapter != null)
                    {
                        instance = new GpuInstance(adapter.Description.Description);
                    }
                }
                

                // Get all counters for this specific GPU instance
                foreach (var counter in _category.GetCounters(counterName))
                {
                    if (counter.CounterName == "Dedicated Usage")
                    {
                        instance.DedicatedCounter = counter;
                    }
                    else if (counter.CounterName == "Shared Usage")
                    {
                        instance.SharedCounter = counter;
                    }
                }

                // Only add the instance if both counters were found
                if (instance.DedicatedCounter != null && instance.SharedCounter != null)
                {
                    _gpuInstances.Add(instance);
                    // Call NextValue() for the first time to initialize internal counter values
                    instance.DedicatedCounter.NextValue();
                    instance.SharedCounter.NextValue();
                }
            }

            var res = GetGpuMemoryUsage();
        }

        // Method to read the latest usage values for all GPUs
        public List<GPUMemoryUsage> GetGpuMemoryUsage()
        {
            return _gpuInstances.Select(instance => new GPUMemoryUsage
            {
                Name = instance.Name,
                Dedicated = (ulong)instance.DedicatedCounter.NextValue(),
                Shared = (ulong)instance.SharedCounter.NextValue()
            }).ToList();
        }
    }
}
#pragma warning restore CA1416