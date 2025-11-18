namespace backend.Services.Implementation
{
    using backend.Models;
    using backend.Services.Abstraction;
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

        public WindowsWorkloadService(HardwareInfo hardwareInfo)
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            // Initialize System counters for counts
            _processCounter = new PerformanceCounter("System", "Processes");
            _threadCounter = new PerformanceCounter("System", "Threads");

            // Перший виклик дає 0 — прогріваємо
            _cpuCounter.NextValue();
            System.Threading.Thread.Sleep(200);
            this._hardwareInfo = hardwareInfo;
        }

        public OsWorkloadInfo GetWorkload()
        {
            return new OsWorkloadInfo
            {
                CPU = GetCpuInfo(),
                RAM = GetRamInfo(),
                GPU = GetGpuInfo()
            };
        }

        // ---------------- CPU ----------------
        private CpuWorkloadInfo GetCpuInfo()
        {

            _hardwareInfo.RefreshCPUList();
            var cpuInfo = _hardwareInfo.CpuList[0];
            return new CpuWorkloadInfo
            {
                UsagePercent = cpuInfo.PercentProcessorTime,
                ProcessCount = (int)_processCounter.NextValue(),
                ThreadCount = (int)_threadCounter.NextValue(),
                CurrentClockSpeed = cpuInfo.CurrentClockSpeed,
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
        private GpuWorkloadInfo GetGpuInfo()
        {
            string name = "Unknown GPU";
            float loadPercent = 0f;
            float memoryUsed = 0;
            float memoryTotal = 0;
            float temp = 0;

            try
            {
                // 1. Назва відеокарти
                var gpuInfo = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController")
                    .Get().Cast<ManagementObject>().FirstOrDefault();

                if (gpuInfo != null)
                    name = gpuInfo["Name"]?.ToString() ?? "Unknown";

                // 2. Завантаження % – Windows Performance Counters
                var gpuLoad = new PerformanceCounter("GPU Engine", "Utilization Percentage", "engtype_3D");
                loadPercent = gpuLoad.NextValue();

                // 3. Пам'ять GPU
                var gpuMemTotalObj = new PerformanceCounter("GPU Adapter Memory", "Dedicated Usage");
                var gpuMemUsedObj = new PerformanceCounter("GPU Adapter Memory", "Process Memory");

                memoryTotal = gpuMemTotalObj.NextValue() / 1024 / 1024;
                memoryUsed = gpuMemUsedObj.NextValue() / 1024 / 1024;

                // 4. Температура GPU (Win32_PerfFormattedData_GPUPerformanceCounters)
                var tempSearcher = new ManagementObjectSearcher(
                    "ROOT\\CIMV2",
                    "SELECT Temperature FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine");

                foreach (ManagementObject obj in tempSearcher.Get())
                {
                    if (obj["Temperature"] != null)
                    {
                        temp = Convert.ToSingle(obj["Temperature"]);
                        break;
                    }
                }
            }
            catch
            {
                // деякі лічильники можуть бути недоступні
            }

            return new GpuWorkloadInfo
            {
                Name = name,
                UsagePercent = loadPercent,
                MemoryTotalMb = memoryTotal,
                MemoryUsedMb = memoryUsed,
                TemperatureCelsius = temp
            };
        }
    }

}
