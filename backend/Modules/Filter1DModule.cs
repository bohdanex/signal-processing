using backend.Services._1D;
using backend.Services.Implementation;
using backend.Utils;
using Carter;
using Microsoft.AspNetCore.Mvc;
using NWaves.Audio;
using NWaves.Filters.Base;
using BIQuad = NWaves.Filters.BiQuad;
using Chebyshev = NWaves.Filters.ChebyshevI;

namespace backend.Modules
{
    public class Filter1DModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("1d/filter/low-pass", async (IFormFile audioFile, [FromServices] Benchmark benchmark, [FromForm] double cutOffFreq, [FromForm] float qFactor, [FromQuery] bool parallel = false) =>
            {
                var waveFile = await audioFile.ToWaveFileAsync();
                double norm = cutOffFreq / waveFile.Signals[0].SamplingRate;
                 BIQuad.LowPassFilter filterFactory() => new (norm, qFactor);

                var result = parallel ? await UseFilterParallel(waveFile, filterFactory) : await UseFilter(waveFile, filterFactory());
                return Results.Ok(result);
            }).DisableAntiforgery();

            app.MapPost("1d/filter/high-pass", async (IFormFile audioFile, [FromServices] Benchmark benchmark, [FromForm] double cutOffFreq, [FromForm] float qFactor, [FromQuery] bool parallel = false) =>
            {
                var waveFile = await audioFile.ToWaveFileAsync();
                double norm = cutOffFreq / waveFile.Signals[0].SamplingRate;
                BIQuad.HighPassFilter filterFactory() => new(norm, qFactor);

                var result = parallel ? await UseFilterParallel(waveFile, filterFactory) : await UseFilter(waveFile, filterFactory());
                return Results.Ok(result);
            }).DisableAntiforgery();


            app.MapPost("1d/filter/band-pass", async (IFormFile audioFile, [FromServices] Benchmark benchmark, [FromForm] double lowFrequency, [FromForm] double highFrequency, [FromForm] int order, [FromQuery] bool parallel = false) =>
            {
                var waveFile = await audioFile.ToWaveFileAsync();
                double normLow = lowFrequency / waveFile.Signals[0].SamplingRate;
                double normHigh = highFrequency / waveFile.Signals[0].SamplingRate;

                Chebyshev.BandPassFilter filterFactory () => new(normLow, normHigh, order); ;

                var result = parallel ? await UseFilterParallel(waveFile, filterFactory) : await UseFilter(waveFile, filterFactory());
                return Results.Ok(result);
            }).DisableAntiforgery();
        }

        private async Task<ResponseWithBenchmark<byte[]>> UseFilter(WaveFile audioFile, IFilter filter)
        {
            var benchmark = new Benchmark();
            // read audio file

            var filterHandler = new SignalFilter1D(audioFile.Signals);

            // apply filter and start benchmarks
            benchmark.Start();
            var filteredSignals = filterHandler.ApplyFilter(filter);
            var benchmarkResult = benchmark.Stop();

            // get byte data and create result object
            var filteredWaveFile = new WaveFile(filteredSignals);
            var memoryStream = new MemoryStream();
            memoryStream.SetLength(0);
            memoryStream.Position = 0;
            filteredWaveFile.SaveTo(memoryStream);
            var byteArray = memoryStream.ToArray();

            return new (byteArray, benchmarkResult);
        }

        private async Task<ResponseWithBenchmark<byte[]>> UseFilterParallel(WaveFile audioFile, Func<IFilter> filterFactory)
        {
            var benchmark = new Benchmark();
            // read audio file

            var filterHandler = new SignalFilter1D(audioFile.Signals);

            // apply filter and start benchmarks
            benchmark.Start();
            var filteredSignals = await filterHandler.ApplyFilterParallel(filterFactory);
            var benchmarkResult = benchmark.Stop();

            // get byte data and create result object
            var filteredWaveFile = new WaveFile(filteredSignals);
            var memoryStream = new MemoryStream();
            memoryStream.SetLength(0);
            memoryStream.Position = 0;
            filteredWaveFile.SaveTo(memoryStream);
            var byteArray = memoryStream.ToArray();

            return new (byteArray, benchmarkResult);
        }
    } 
}
