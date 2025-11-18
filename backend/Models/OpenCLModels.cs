namespace backend.Models
{
    public class OpenCLInfo
    {
        public bool IsSupported { get; set; }
        public List<CLPlatform> Platforms { get; set; } = new List<CLPlatform>();
        public string Error { get; set; }
    }

    public class CLPlatform
    {
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }
        public List<CLDevice> Devices { get; set; } = new List<CLDevice>();
    }

    public class CLDevice
    {
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public uint ComputeUnits { get; set; }
    }
}
