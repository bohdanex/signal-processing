using backend.Services.Implementation;
using Carter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .InjectAppDependencies()
    .AddOpenApi()
    .AddCarter();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000") // Replace with the actual origin of your frontend application
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()); // If you need to send credentials like cookies
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
//app.UseHttpsRedirection();

Console.WriteLine("Submitting initial tasks to warm up the Thread Pool...");
Parallel.For(0, Environment.ProcessorCount * 2, i =>
{
    // Perform a very quick, non-blocking operation
    _ = Math.Sqrt(i);
});
Console.WriteLine("Initial Thread Pool warm-up complete.");

// This add all endpoint modules from the Modules/ folder
app.MapCarter();

app.Run();
