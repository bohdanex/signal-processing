namespace backend.Models
{
    public class CpuWorkloadInfo
    {
        public float UsagePercent { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public uint CurrentClockSpeed { get; set; }
    }

    public class RamWorkloadInfo
    {
        public double TotalGb { get; set; }
        public double UsedGb { get; set; }
        public double FreeGb { get; set; }
        public float UsagePercent { get; set; }
    }

    public class GpuWorkloadInfo
    {
        public string Name { get; set; }
        public float UsagePercent { get; set; }
        public float MemoryUsedMb { get; set; }
        public float MemoryTotalMb { get; set; }
        public float TemperatureCelsius { get; set; }
    }

    public class OsWorkloadInfo
    {
        public CpuWorkloadInfo CPU { get; set; }
        public RamWorkloadInfo RAM { get; set; }
        public GpuWorkloadInfo GPU { get; set; }
    }
}
