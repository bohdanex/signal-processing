using backend.Services._2D;
using backend.Services.Implementation;
using backend.Utils;
using Carter;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using ILGPU.Util;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace backend.Modules
{
    public class MorphOps2D_Module : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("2d/morph/dilation/{accelerator}", async (IFormFile file, [FromForm] int radius, [FromServices] MorphOps morphOps, [FromRoute] string accelerator) =>
            {
                var (result, benchmark) = await UseMorphOperation(file, (pixels) =>
                {
                    if (accelerator == "cl") return morphOps.Dilation<CLAccelerator>(pixels, radius);
                    else if (accelerator == "cuda") return morphOps.Dilation<CudaAccelerator>(pixels, radius);
                    return morphOps.Dilation<CPUAccelerator>(pixels, radius);
                });
                var responseData = new ResponseWithBenchmark<string>(result.ToBase64String(PngFormat.Instance), benchmark);

                return Results.Ok(responseData);
            }).DisableAntiforgery();

            app.MapPost("2d/morph/erosion/{accelerator}", async (IFormFile file, [FromForm] int radius, [FromServices] MorphOps morphOps, [FromRoute] string accelerator) =>
            {
                var (result, benchmark) = await UseMorphOperation(file, (pixels) =>
                {
                    if (accelerator == "cl") return morphOps.Erosion<CLAccelerator>(pixels, radius);
                    else if (accelerator == "cuda") return morphOps.Erosion<CudaAccelerator>(pixels, radius);
                    return morphOps.Erosion<CPUAccelerator>(pixels, radius);
                });
                var responseData = new ResponseWithBenchmark<string>(result.ToBase64String(PngFormat.Instance), benchmark);

                return Results.Ok(responseData);
            }).DisableAntiforgery();
        }

        private async Task<(Image<L8>, BenchmarkResult)> UseMorphOperation(IFormFile file, Func<float[,], float[,]> opeation)
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());
            var grayScaleImage = image.CloneAs<L8>();
            var pixels = grayScaleImage.NormalizeImagePixels();

            var bc = new Benchmark();
            bc.Start();
            var morphOperationResult = opeation(pixels);
            var benchmarkResult = bc.Stop();
            var resultImage = await ImageUtils.SaveFloatArrayAsPngAsync(morphOperationResult);

            return (resultImage, benchmarkResult);
        }
    }
}
