using System.Text;
using Silk.NET.OpenCL;
using backend.Models;

namespace backend.Utils
{
    public static unsafe class OpenCLUtility
    {
        // Load the API instance once
        private static CL _cl = CL.GetApi();
        
        public static OpenCLInfo GetStatus()
        {
            var info = new OpenCLInfo();

            try
            {
                // 1. Get number of platforms
                uint numPlatforms = 0;
                int error = _cl.GetPlatformIDs(0, null, &numPlatforms);

                if (error != 0 || numPlatforms == 0)
                {
                    info.IsSupported = false;
                    info.Error = $"No OpenCL platforms found. Error code: {error}";
                    return info;
                }

                // 2. Get platform IDs
                var platformIds = new nint[numPlatforms];
                fixed (nint* pPlatforms = platformIds)
                {
                    _cl.GetPlatformIDs(numPlatforms, pPlatforms, null);
                }

                info.IsSupported = true;

                // 3. Iterate Platforms
                foreach (var platformId in platformIds)
                {
                    var platformData = new CLPlatform
                    {
                        Name = GetPlatformInfo(platformId, PlatformInfo.Name),
                        Vendor = GetPlatformInfo(platformId, PlatformInfo.Vendor),
                        Version = GetPlatformInfo(platformId, PlatformInfo.Version)
                    };

                    // 4. Get Devices for this Platform
                    uint numDevices = 0;
                    _cl.GetDeviceIDs(platformId, DeviceType.All, 0, null, &numDevices);

                    if (numDevices > 0)
                    {
                        var deviceIds = new nint[numDevices];
                        fixed (nint* pDevices = deviceIds)
                        {
                            _cl.GetDeviceIDs(platformId, DeviceType.All, numDevices, pDevices, null);
                        }

                        foreach (var deviceId in deviceIds)
                        {
                            platformData.Devices.Add(new CLDevice
                            {
                                Name = GetDeviceInfo(deviceId, DeviceInfo.Name),
                                Vendor = GetDeviceInfo(deviceId, DeviceInfo.Vendor),
                                Version = GetDeviceInfo(deviceId, DeviceInfo.Version),
                                Type = GetDeviceType(deviceId),
                                ComputeUnits = GetDeviceUInt(deviceId, DeviceInfo.MaxComputeUnits)
                            });
                        }
                    }

                    info.Platforms.Add(platformData);
                }
            }
            catch (Exception ex)
            {
                info.IsSupported = false;
                info.Error = $"OpenCL Check Failed: {ex.Message}";
            }

            return info;
        }

        // --- Helpers ---

        private static string GetPlatformInfo(nint platform, PlatformInfo param)
        {
            nuint size = 0;
            _cl.GetPlatformInfo(platform, param, 0, null, &size);

            byte[] buffer = new byte[(int)size];
            fixed (byte* pBuffer = buffer)
            {
                _cl.GetPlatformInfo(platform, param, size, pBuffer, null);
            }

            return Encoding.UTF8.GetString(buffer).Trim('\0');
        }

        private static string GetDeviceInfo(nint device, DeviceInfo param)
        {
            nuint size = 0;
            _cl.GetDeviceInfo(device, param, 0, null, &size);

            byte[] buffer = new byte[(int)size];
            fixed (byte* pBuffer = buffer)
            {
                _cl.GetDeviceInfo(device, param, size, pBuffer, null);
            }

            return Encoding.UTF8.GetString(buffer).Trim('\0');
        }

        private static uint GetDeviceUInt(nint device, DeviceInfo param)
        {
            uint value = 0;
            nuint size = (nuint)sizeof(uint);
            _cl.GetDeviceInfo(device, param, size, &value, null);
            return value;
        }

        private static string GetDeviceType(nint device)
        {
            DeviceType type = 0;
            nuint size = (nuint)sizeof(DeviceType);
            _cl.GetDeviceInfo(device, DeviceInfo.Type, size, &type, null);

            if ((type & DeviceType.Cpu) != 0) return "CPU";
            if ((type & DeviceType.Gpu) != 0) return "GPU";
            if ((type & DeviceType.Accelerator) != 0) return "Accelerator";
            return "Unknown";
        }
    }
}
