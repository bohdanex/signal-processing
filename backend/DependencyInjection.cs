using backend.Services._1D;
using backend.Services.Abstraction;
using backend.Services.Implementation;
using backend.Utils;
using Hardware.Info;
using System.Runtime.CompilerServices;

namespace backend
{
    public static class DependencyInjection
    {
        public static IServiceCollection InjectAppDependencies(this IServiceCollection services)
        {
            services.AddHardwareInfo();
            services.AddTransient<Benchmark>();
            services.AddSingleton<GPUMemoryMonitor>();
            services.AddSingleton<ISystemInfoService, SystemInfoService>();
            services.AddSingleton<IOSWorkloadService, WindowsWorkloadService>();
            services.AddSingleton<IParallelComputeSupportService, WindowsComputeSupportService>();
            services.AddSingleton<STFTService>();

            return services;
        }

        public static IServiceCollection AddHardwareInfo(this IServiceCollection services)
        {
            var hardwareInfo = new HardwareInfo();

            hardwareInfo.RefreshCPUList();
            hardwareInfo.RefreshMemoryList();
            hardwareInfo.RefreshVideoControllerList();
            hardwareInfo.RefreshOperatingSystem();

            services.AddSingleton(hardwareInfo);

            return services;
        }
    }
}
