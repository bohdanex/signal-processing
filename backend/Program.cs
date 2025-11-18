using backend.Services.Abstraction;
using backend.Services.Implementation;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .InjectAppDependencies()
    .AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors((c) =>
{
    c.AllowAnyOrigin();
    c.AllowAnyHeader();
    c.AllowAnyMethod();
});
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

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
    } catch (TaskCanceledException)
    {

    }
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
