namespace backend.Services.Implementation
{
    using backend.Models;
    using backend.Services.Abstraction;
    using backend.Utils;
    using Hardware.Info;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Management;

    public class WindowsWorkloadService : IOSWorkloadService
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _processCounter;
        private readonly PerformanceCounter _threadCounter;
        private readonly HardwareInfo _hardwareInfo;
        private readonly GPUMemoryMonitor _gpuMonitor;
        private readonly PerformanceCounter _cpuPerfCounter;
        private readonly uint _maxClockSpeed;
        public WindowsWorkloadService(HardwareInfo hardwareInfo, GPUMemoryMonitor gpuMemoryMonitor)
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            // Initialize System counters for counts
            _processCounter = new PerformanceCounter("System", "Processes");
            _threadCounter = new PerformanceCounter("System", "Threads");
            _cpuPerfCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");

            // Перший виклик дає 0 — прогріваємо
            _cpuCounter.NextValue();
            _cpuPerfCounter.NextValue();
            this._hardwareInfo = hardwareInfo;
            this._gpuMonitor = gpuMemoryMonitor;

            var searcher = new ManagementObjectSearcher("SELECT MaxClockSpeed FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                _maxClockSpeed = (uint)obj["MaxClockSpeed"];
                if (_maxClockSpeed > 0) break;
            }
        }

        public OsWorkloadInfo GetWorkload()
        {
            return new OsWorkloadInfo
            {
                CPU = GetCpuInfo(),
                RAM = GetRamInfo(),
                GPUs = GetGpuInfo()
            };
        }

        // ---------------- CPU ----------------
        private CpuWorkloadInfo GetCpuInfo()
        {

            float cpuUsage = float.Round(_cpuCounter.NextValue(), 2);
            float cpuPerf = _cpuPerfCounter.NextValue();

            int currentClockSpeed = (int)(_maxClockSpeed * (cpuPerf / 100));
            return new CpuWorkloadInfo
            {
                UsagePercent = cpuUsage,
                ProcessCount = (int)_processCounter.NextValue(),
                ThreadCount = (int)_threadCounter.NextValue(),
                CurrentClockSpeed = (uint)currentClockSpeed,
            };
        }

        // ---------------- RAM ----------------
        private RamWorkloadInfo GetRamInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            var mo = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

            double totalMb = Convert.ToDouble(mo["TotalVisibleMemorySize"]) / 1024;
            double freeMb = Convert.ToDouble(mo["FreePhysicalMemory"]) / 1024;
            double usedMb = totalMb - freeMb;

            return new RamWorkloadInfo
            {
                TotalGb = totalMb / 1024,
                FreeGb = freeMb / 1024,
                UsedGb = usedMb / 1024,
                UsagePercent = (float)(usedMb / totalMb * 100)
            };
        }

        // ---------------- GPU ----------------
        private GpuWorkloadInfo[] GetGpuInfo()
        {
            var usageInfo = _gpuMonitor.GetGpuMemoryUsage();
            return _hardwareInfo.VideoControllerList.Select((item) => {
                var usageData = usageInfo.First((info) => info.Name.ToLower() == item.Name.ToLower());

                return new GpuWorkloadInfo() {
                    Name = item.Name,
                    MemoryTotalMb = item.AdapterRAM / 1024/ 1024,
                    MemoryUsedMb = usageData.Dedicated / 1024/ 1024
                
                };
            }).ToArray();

        }
    }

}
