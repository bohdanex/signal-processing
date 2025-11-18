using backend.Models;

namespace backend.Services.Abstraction
{
    public interface ISystemInfoService
    {
        OsInfo GetOsInfo();
        CpuInfo GetCpuInfo();
        GpuInfo[] GetGpuInfo();
        RamInfo[] GetRamInfo();
    }

}
