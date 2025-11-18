namespace backend.Models
{
    public class OsInfo ()
    {
        public string Name { get; set; }
        public string Architecture { get; set; }
        public string Version { get; set; }
    }

    public class CpuInfo
    {
        public string Name { get; set; }
        public int PhysicalCores { get; set; }
        public int LogicalCores { get; set; }
        public string MaxClockSpeed { get; set; }
    }

    public class GpuInfo
    {
        public string Name { get; set; }
        public string DriverVersion { get; set; }
        public string Memory { get; set; }
    }

    public class RamInfo
    {
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public ulong MemorySize { get; set; }
        public uint ClockSpeed { get; set; }
        public int Count { get; set; }
        public ulong TotalMemorySize { get; set; }
    }

}
