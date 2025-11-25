using backend.Services._2D;
using backend.Services.Implementation;
using Carter;
using ILGPU.Util;
using Microsoft.AspNetCore.Mvc;
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
                    var response = new ResponseWithBenchmark<string>(await ToBase64Image(resultImage), benchmarks);
                    resultImage.Dispose();

                    return Results.Ok(response);
                }
                catch (DllNotFoundException)
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

                    var response = new ResponseWithBenchmark<string>(await ToBase64Image(resultImage), benchmarks);
                    resultImage.Dispose();

                    return Results.Ok(response);
                }
                catch (DllNotFoundException ex)
                {
                    return Results.BadRequest("Make sure CUDA toolkit is installed on your machine");
                }
            }).DisableAntiforgery();
        }

        private static async Task<string> ToBase64Image(Image<L8> image)
        {
            using var memoryStream = new MemoryStream();
            await image.SaveAsPngAsync(memoryStream);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            return $"data:image/png;base64,{base64}";
        }

        private static async Task<(Image<L8>, BenchmarkResult)> MapImageBlurAsync(Image image, Func<Float2[,]> processor)
        {
            var benchmarker = new Benchmark();
            benchmarker.Start();
            var blurResult = processor();
            var benchmarkResult = benchmarker.Stop();

            var bluredImage = await SaveFloatArrayAsPngAsync(blurResult);

            return (bluredImage, benchmarkResult);
        }

        private static Float2[,] ExtractNormalizedGrayscaleValues(Image image)
        {
            // Convert to L8 grayscale explicitly
            using Image<L8> gray = image.CloneAs<L8>();

            int width = gray.Width;
            int height = gray.Height;

            Float2[,] result = new Float2[height, width];

            // Iterate rows for efficiency
            gray.ProcessPixelRows(accessor =>
            {
                for (int rowIndex = 0; rowIndex < height; rowIndex++)
                {
                    var row = accessor.GetRowSpan(rowIndex);

                    for (int colIndex = 0; colIndex < width; colIndex++)
                    {
                        byte value = row[colIndex].PackedValue;   // grayscale 0..255
                        result[rowIndex, colIndex] = new Float2(value / 255f);      // normalize to 0..1
                    }
                }
            });

            return result;
        }

        private static async Task<Image<L8>> SaveFloatArrayAsPngAsync(Float2[,] data)
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
                        float value = data[y, x].X;
                        byte byteValue = (byte)Math.Clamp(value * 255f, 0f, 255f);
                        row[x] = new L8(byteValue);
                    }
                }
            });

            return image;
        }
    }

}
