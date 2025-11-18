using backend.Models;
using backend.Services.Abstraction;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using Hardware.Info;

namespace backend.Services.Implementation
{
    public class SystemInfoService : ISystemInfoService
    {
        private HardwareInfo _hardwareInfo;

        public SystemInfoService(HardwareInfo hardwareInfo)
        {
            this._hardwareInfo = hardwareInfo;
            hardwareInfo.RefreshVideoControllerList();
            hardwareInfo.RefreshCPUList();
        }

        public OsInfo GetOsInfo()
        {
            return new OsInfo
            {
                Name = RuntimeInformation.OSDescription,
                Architecture = RuntimeInformation.OSArchitecture.ToString(),
                Version = Environment.OSVersion.VersionString
            };
        }

        public CpuInfo GetCpuInfo()
        {
            var info = _hardwareInfo.CpuList[0];
            return new CpuInfo() { 
                Name = info.Name,
                MaxClockSpeed = info.MaxClockSpeed + " MHz",
                PhysicalCores = (int)info.NumberOfCores,
                LogicalCores = (int) info.NumberOfLogicalProcessors
            };
        }


        private string RunBash(string cmd)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{cmd}\"",
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        public GpuInfo[] GetGpuInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetGpuInfoWindows();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return [GetGpuInfoLinux()];

            return [];
        }

        private GpuInfo[] GetGpuInfoWindows()
        {
            return _hardwareInfo.VideoControllerList.Select((info) => new GpuInfo
            {
                Name = info.Name,
                DriverVersion = info.DriverVersion,
                Memory = (info.AdapterRAM / 1024 / 1024) + "MB"
            }).ToArray();
        }

        private GpuInfo GetGpuInfoLinux()
        {
            string lspci = RunCmd("lspci | grep -E 'VGA|3D'");
            string driver = RunCmd("glxinfo | grep 'OpenGL version'");

            return new GpuInfo
            {
                Name = lspci,
                DriverVersion = driver,
                Memory = "Unknown (needs nvidia-smi / vendor tools)"
            };
        }

        private string RunCmd(string cmd)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{cmd}\"",
                    RedirectStandardOutput = true
                });
                return p.StandardOutput.ReadToEnd();
            }
            catch
            {
                return "N/A";
            }
        }

        public RamInfo[] GetRamInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetRamInfoWindows();

            return null;
        }

        private RamInfo[] GetRamInfoWindows()
        {
            // Query the Win32_PhysicalMemory WMI class
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            List<RamInfo> rams = new();
            try
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    // --- 1. Data Cleaning and Initialization ---
                    string manufacturer = obj["Manufacturer"]?.ToString().Trim() ?? "N/A";
                    string partNumber = obj["PartNumber"]?.ToString().Trim() ?? "N/A";
                    string speedString = obj["Speed"]?.ToString().Trim() ?? "0";
                    string capacityString = obj["Capacity"]?.ToString().Trim() ?? "0";

                    // Clean up part number string (WMI often adds leading/trailing garbage)
                    // Example: "PartNumber" might contain leading spaces/null chars, using replace helps normalize.
                    partNumber = partNumber.Replace("\0", string.Empty);

                    // Create an entry for the individual RAM stick
                    rams.Add(
                        new RamInfo()
                        {
                            Manufacturer = manufacturer,
                            PartNumber = partNumber,
                            ClockSpeed = Convert.ToUInt32(speedString),
                            // Convert Capacity from Bytes to MB
                            MemorySize = Convert.ToUInt64(capacityString) / 1024 / 1024,
                            Count = 1 // Each individual stick starts with a count of 1
                        });
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine($"An error occurred while querying for WMI data: {e.Message}");
            }


            // --- 2. Grouping by ALL unique parameters ---
            var uniqueRamGroups = rams
                .GroupBy(ram => new // Anonymous object is the composite key
                {
                    ram.Manufacturer,
                    ram.PartNumber,
                    ram.ClockSpeed,
                    ram.MemorySize // Group by the size of the individual stick (e.g., 8192 MB)
                })
                .Select(group =>
                {
                    // The number of sticks in the group
                    int count = group.Sum(info => info.Count);

                    // Get the common properties from the first item in the group
                    var ramInfoFirst = group.First();

                    return new RamInfo()
                    {
                        Manufacturer = ramInfoFirst.Manufacturer,
                        PartNumber = ramInfoFirst.PartNumber,
                        ClockSpeed = ramInfoFirst.ClockSpeed,
                        MemorySize = ramInfoFirst.MemorySize, // The size of *one* stick in the group
                        Count = count, // The number of sticks in this configuration
                                       // Calculate the total memory for this configuration
                        TotalMemorySize = ramInfoFirst.MemorySize * (ulong)count
                    };
                })
                // If you need to return a single RamInfo object representing the whole system 
                // (this structure seems odd, but based on your return type), 
                // you would need to aggregate further, otherwise return the list.
                .ToList();

            return uniqueRamGroups.ToArray();
        }


        private double ParseMeminfo(string[] lines, string key)
        {
            string line = lines.FirstOrDefault(l => l.StartsWith(key));
            var parts = line.Split(':')[1].Trim().Split(' ')[0];
            return double.Parse(parts) * 1024; // kB → bytes
        }
    }

}
