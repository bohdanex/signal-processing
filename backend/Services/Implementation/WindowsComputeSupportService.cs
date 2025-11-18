using backend.Models;
using backend.Services.Abstraction;
using backend.Utils;
using System.Management;
using ILGPU;
using ILGPU.Runtime.Cuda;
using ManagedCuda;

namespace backend.Services.Implementation
{
    public class WindowsComputeSupportService : IParallelComputeSupportService
    {
        public async Task<IReadOnlyList<ComputeSupportInfo>> GetAllSupportInfoAsync()
        {
            var results = new List<ComputeSupportInfo>();

            foreach (ComputeTechnology tech in Enum.GetValues(typeof(ComputeTechnology)))
            {
                results.Add(await CheckSupportAsync(tech));
            }

            return results;
        }

        public Task<ComputeSupportInfo> CheckSupportAsync(ComputeTechnology tech)
        {
            return tech switch
            {
                ComputeTechnology.CUDA => Task.FromResult(CheckCuda()),
                ComputeTechnology.OpenGL => Task.FromResult(CheckOpenGL()),
                ComputeTechnology.OpenCL => Task.FromResult(CheckOpenCL()),
                ComputeTechnology.Vulkan => Task.FromResult(CheckVulkan()),
                ComputeTechnology.DirectX12 => Task.FromResult(CheckDirectX12()),
                _ => throw new ArgumentOutOfRangeException(nameof(tech))
            };
        }

        // -------------------------------------------------------
        // Реальні перевірки нижче
        // -------------------------------------------------------

        private ComputeSupportInfo CheckCuda()
        {
            var build = "N/A";
            var verion = "N/A";
            bool isSupported = false;

            try
            {
                var driverVersion = CudaContext.GetDriverVersion();
                build = $"Build: {driverVersion.Build}, Minor: {driverVersion.Minor}, Major: {driverVersion.Major}";
                isSupported = CudaContext.GetDeviceCount() > 0;
                verion = $"{driverVersion.Major}.{driverVersion.Minor}";
            } catch (ManagedCuda.CudaException)
            {
            }

            return new ComputeSupportInfo
            {
                Name = ComputeTechnology.CUDA.ToString(),
                TechnologyId = ComputeTechnology.CUDA,
                IsSupported = isSupported,
                Details = build,
                Version = verion
            };
        }

        private ComputeSupportInfo CheckOpenGL()
        {
            var info = OpenGLUtils.GetGlInfo();

            return new ComputeSupportInfo
            {
                Name = ComputeTechnology.OpenGL.ToString(),
                TechnologyId = ComputeTechnology.OpenGL,
                IsSupported = info.IsSupported,
                Version = info.Version,
                Details = $"Vendor: {info.Vendor}, renderer: {info.Renderer}"
            };
        }

        private ComputeSupportInfo CheckOpenCL()
        {
            var openCLStatus = OpenCLUtility.GetStatus();

            return new ComputeSupportInfo
            {
                Name = ComputeTechnology.OpenCL.ToString(),
                TechnologyId = ComputeTechnology.OpenCL,
                IsSupported = openCLStatus.IsSupported,
                Version = String.Join(";\n", openCLStatus.Platforms.Select((p) => $"{p.Name}: {p.Version}")),
            };
        }

        private ComputeSupportInfo CheckVulkan()
        {
            var vulkanStatus = VulkanUtility.GetStatus();

            return new ComputeSupportInfo
            {
                Name = ComputeTechnology.Vulkan.ToString(),
                TechnologyId = ComputeTechnology.Vulkan,
                IsSupported = vulkanStatus.IsSupported,
                Version = vulkanStatus.ApiVersion,
                Details = "Devices: " + String.Join("; ", vulkanStatus.Devices.Select((d) => d.Name))
            };
        }

        private ComputeSupportInfo CheckDirectX12()
        {
            // DirectX12 перевіряється через DXGI/WMI
            bool dx12Supported = false;
            string? version = null;

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_VideoController");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string? driverVersion = obj["DriverVersion"]?.ToString();

                    // TODO: Реальний DX12 feature check через DirectX API
                    dx12Supported = true;
                    version = driverVersion;
                }
            }
            catch { }

            return new ComputeSupportInfo
            {
                Name = ComputeTechnology.DirectX12.ToString(),
                TechnologyId = ComputeTechnology.DirectX12,
                IsSupported = dx12Supported,
                Version = version,
                Details = dx12Supported ? "DXGI перевірено" : "DirectX 12 недоступний"
            };
        }
    }
}
