using System.Runtime.InteropServices;
using backend.Models;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace backend.Utils
{
    public static unsafe class VulkanUtility
    {
        private static Vk _vk = Vk.GetApi();

        public static VulkanInfo GetStatus()
        {
            var info = new VulkanInfo();
            Instance instance = default;

            try
            {
                // 1. App Info (Optional but good practice)
                var appInfo = new ApplicationInfo
                {
                    SType = StructureType.ApplicationInfo,
                    PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("ServerCheck"),
                    ApplicationVersion = new Version32(1, 0, 0),
                    PEngineName = (byte*)Marshal.StringToHGlobalAnsi("NoEngine"),
                    EngineVersion = new Version32(1, 0, 0),
                    ApiVersion = Vk.Version10
                };

                // 2. Create Instance Info
                var createInfo = new InstanceCreateInfo
                {
                    SType = StructureType.InstanceCreateInfo,
                    PApplicationInfo = &appInfo,
                    EnabledLayerCount = 0,
                    PpEnabledLayerNames = null,
                    EnabledExtensionCount = 0,
                    PpEnabledExtensionNames = null
                };

                // 3. Create Instance
                // If this fails, Vulkan is likely not installed or supported
                if (_vk.CreateInstance(ref createInfo, null, out instance) != Result.Success)
                {
                    info.IsSupported = false;
                    info.Error = "Failed to create Vulkan Instance. Drivers may be missing.";
                    // Clean up unmanaged strings
                    Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
                    Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
                    return info;
                }

                info.IsSupported = true;

                // Clean up strings immediately after creation
                Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
                Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);

                // 4. Get Physical Devices
                uint deviceCount = 0;
                _vk.EnumeratePhysicalDevices(instance, ref deviceCount, null);

                if (deviceCount > 0)
                {
                    var devices = new PhysicalDevice[deviceCount];
                    fixed (PhysicalDevice* pDevices = devices)
                    {
                        _vk.EnumeratePhysicalDevices(instance, ref deviceCount, pDevices);
                    }

                    foreach (var device in devices)
                    {
                        _vk.GetPhysicalDeviceProperties(device, out var props);

                        // Helper to read fixed-buffer string
                        string deviceName = Marshal.PtrToStringAnsi((IntPtr)props.DeviceName);

                        info.Devices.Add(new VulkanDevice
                        {
                            Name = deviceName,
                            Type = props.DeviceType.ToString(),
                            ID = props.DeviceID,
                            DriverVersion = props.DriverVersion.ToString()
                        });

                        // Check highest supported version by this device
                        info.ApiVersion = $"{props.ApiVersion}";
                    }
                }
            }
            catch (Exception ex)
            {
                info.IsSupported = false;
                info.Error = $"Vulkan Check Exception: {ex.Message}";
            }
            finally
            {
                // 5. Cleanup
                if (instance.Handle != 0)
                {
                    _vk.DestroyInstance(instance, null);
                }
            }

            return info;
        }
    }
}
