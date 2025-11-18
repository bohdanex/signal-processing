namespace backend.Models
{
    public enum ComputeTechnology
    {
        CUDA,
        OpenGL,
        OpenCL,
        Vulkan,
        DirectX12
    }

    public class ComputeSupportInfo
    {
        public string? Name { get; set; }
        public ComputeTechnology TechnologyId { get; set; }
        public bool IsSupported { get; set; }
        public string? Version { get; set; }
        public string? Details { get; set; }
    }
}
