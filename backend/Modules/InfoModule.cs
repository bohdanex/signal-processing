using backend.Services.Abstraction;
using Carter;
using System.Text.Json;

namespace backend.Modules
{
    public class InfoModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("os/info", (ISystemInfoService sysInfoService) =>
            {
                return new
                {
                    os = sysInfoService.GetOsInfo(),
                    rams = sysInfoService.GetRamInfo(),
                    cpu = sysInfoService.GetCpuInfo(),
                    gpus = sysInfoService.GetGpuInfo(),
                };
            });

            app.MapGet("os/parallel-compute-support", async (IParallelComputeSupportService service) =>
            {
                return await service.GetAllSupportInfoAsync();
            });


            app.MapGet("/sse/workload", async (HttpContext context, IOSWorkloadService workloadService) =>
            {
                context.Response.Headers.ContentType = "text/event-stream";
                context.Response.Headers.CacheControl = "no-cache";
                context.Response.Headers.Connection = "keep-alive";

                try
                {
                    while (!context.RequestAborted.IsCancellationRequested)
                    {
                        var workload = workloadService.GetWorkload();
                        var json = JsonSerializer.Serialize(workload);

                        await context.Response.WriteAsync($"data: {json}\n\n");
                        await context.Response.Body.FlushAsync();

                        await Task.Delay(500, context.RequestAborted);
                    }
                }
                catch (TaskCanceledException)
                {

                }
            });
        }
    }
}
