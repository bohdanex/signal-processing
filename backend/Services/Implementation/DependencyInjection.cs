using backend.Services.Abstraction;
using Hardware.Info;
using System.Runtime.CompilerServices;

namespace backend.Services.Implementation
{
    public static class DependencyInjection
    {
        public static IServiceCollection InjectAppDependencies(this IServiceCollection services)
        {
            services.AddHardwareInfo();
            services.AddSingleton<ISystemInfoService, SystemInfoService>();
            services.AddSingleton<IOSWorkloadService, WindowsWorkloadService>();
            services.AddSingleton<IParallelComputeSupportService, WindowsComputeSupportService>();

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
