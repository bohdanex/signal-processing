using backend.Services._1D;
using backend.Services.Implementation;
using backend.Utils;
using Carter;
using Microsoft.AspNetCore.Mvc;
using NWaves.Signals;
using System.Linq;
using static OpenTK.Graphics.OpenGL.GL;

public record STFT_Response(IEnumerable<float[][]> Channels, float SampleRate, double Duration, int WindowSize, int HopSize);

namespace backend.Modules
{
    public class STFT_Module : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("1d/stft", async ([FromServices] Benchmark benchmark, [FromServices] STFTService stftService, IFormFile audioFile, [FromForm] int windowSize, [FromForm] int hopSize) =>
            {
                var waveFile = await audioFile.ToWaveFileAsync();
                var signals = waveFile.Signals.Aggregate((left, right) => left.Concatenate(right));
                benchmark.Start();
                var spectrograms = stftService.Transform(signals, windowSize, hopSize);
                var benchResult = benchmark.Stop();

                var response = new STFT_Response(spectrograms.Chunk(spectrograms.Count / waveFile.Signals.Count), waveFile.Signals[0].SamplingRate, waveFile.Signals[0].Duration, windowSize, hopSize);
                return new ResponseWithBenchmark<STFT_Response>(response, benchResult);

            }).DisableAntiforgery();

            app.MapPost("1d/stft-parallel", async ([FromServices] Benchmark benchmark, [FromServices] STFTService stftService, IFormFile audioFile, [FromForm] int windowSize, [FromForm] int hopSize) =>
            {
                var waveFile = await audioFile.ToWaveFileAsync();
                var signals = waveFile.Signals.Aggregate((left, right) => left.Concatenate(right));
                benchmark.Start();
                var spectrograms = await stftService.TransformParallel(signals, Environment.ProcessorCount, windowSize, hopSize);
                var benchResult = benchmark.Stop();

                var response = new STFT_Response(spectrograms.Chunk(spectrograms.Count/waveFile.Signals.Count), waveFile.Signals[0].SamplingRate, waveFile.Signals[0].Duration, windowSize, hopSize);
                return new ResponseWithBenchmark<STFT_Response>(response, benchResult);
            }).DisableAntiforgery();
        }
    }

}
