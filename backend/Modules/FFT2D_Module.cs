using backend.Services._2D;
using backend.Services.Implementation;
using Carter;
using Microsoft.AspNetCore.Mvc;
using NWaves.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace backend.Modules
{
    public class FFT2D_Module : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("2d/blur/gaussian", async (IFormFile file, [FromForm] float sigma, [FromServices] CUDA_Service fft2d) => {
                using var image = await Image.LoadAsync(file.OpenReadStream());
                image.Mutate((context) => context.Resize(512, 512).Grayscale());
                var pixelCount = image.Width * image.Height;
                var normalizedPixels = ExtractNormalizedGrayscaleValues(image);

                try
                {
                    var (resultImage, benchmarks) = await MapImageBlurAsync(image, () => fft2d.ApplyGaussianBlur(normalizedPixels, image.Width, image.Height, sigma));
                    using var memoryStream = new MemoryStream();
                    await resultImage.SaveAsPngAsync(memoryStream);
                    var response = new ResponseWithBenchmark<byte[]>(memoryStream.ToArray(), benchmarks);
                    resultImage.Dispose();
                    return Results.Ok(response);
                }
                catch (DllNotFoundException ex)
                {
                    return Results.BadRequest("Make sure CUDA toolkit is installed on your machine");
                }
            }).DisableAntiforgery();

            app.MapPost("2d/filter/laplacian", async (IFormFile file, [FromForm] float scale, [FromServices] CUDA_Service fft2d) => {
                using var image = await Image.LoadAsync(file.OpenReadStream());
                image.Mutate((context) => context.Resize(512, 512).Grayscale());
                var pixelCount = image.Width * image.Height;
                var normalizedPixels = ExtractNormalizedGrayscaleValues(image);

                try
                {
                    var (resultImage, benchmarks) = await MapImageBlurAsync(image, () => fft2d.ApplyLaplacianFilter(normalizedPixels, image.Width, image.Height, scale));
                    using var memoryStream = new MemoryStream();
                    await resultImage.SaveAsPngAsync(memoryStream);
                    var response = new ResponseWithBenchmark<byte[]>(memoryStream.ToArray(), benchmarks);
                    resultImage.Dispose();
                    return Results.Ok(response);
                }
                catch (DllNotFoundException ex)
                {
                    return Results.BadRequest("Make sure CUDA toolkit is installed on your machine");
                }
            }).DisableAntiforgery();
        }

        private static async Task<(Image<L8>, BenchmarkResult)> MapImageBlurAsync(Image image, Func<float[,]> processor)
        {
            var benchmarker = new Benchmark();
            benchmarker.Start();
            var blurResult = processor();
            var bluredImage = await SaveFloatArrayAsPngAsync(blurResult);
            var benchmarkResult = benchmarker.Stop();

            return (bluredImage, benchmarkResult);
        }

        private static float[,] ExtractNormalizedGrayscaleValues(Image image)
        {
            // Convert to L8 grayscale explicitly
            using Image<L8> gray = image.CloneAs<L8>();

            int width = gray.Width;
            int height = gray.Height;

            float[,] result = new float[height, width];

            // Iterate rows for efficiency
            gray.ProcessPixelRows(accessor =>
            {
                for (int rowIndex = 0; rowIndex < height; rowIndex++)
                {
                    var row = accessor.GetRowSpan(rowIndex);

                    for (int colIndex = 0; colIndex < width; colIndex++)
                    {
                        byte value = row[colIndex].PackedValue;   // grayscale 0..255
                        result[rowIndex, colIndex] = value / 255f;      // normalize to 0..1
                    }
                }
            });

            return result;
        }

        private static async Task<Image<L8>> SaveFloatArrayAsPngAsync(float[,] data)
        {
            int height = data.GetLength(0); // rows
            int width = data.GetLength(1); // columns

            // Create a new grayscale image
            var image = new Image<L8>(width, height);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        // Convert float [0,1] to byte [0,255]
                        float value = data[y, x];
                        byte byteValue = (byte)Math.Clamp(value * 255f, 0f, 255f);
                        row[x] = new L8(byteValue);
                    }
                }
            });

            return image;
        }
    }

}
