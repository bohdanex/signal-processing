namespace backend.Models
{
    public class VulkanInfo
    {
        public bool IsSupported { get; set; }
        public string ApiVersion { get; set; } = "Unknown";
        public List<string> Extensions { get; set; } = new List<string>();
        public List<VulkanDevice> Devices { get; set; } = new List<VulkanDevice>();
        public string Error { get; set; }
    }

    public class VulkanDevice
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DriverVersion { get; set; }
        public uint ID { get; set; }
    }
}
