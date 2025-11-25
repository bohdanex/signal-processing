using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace backend.Utils
{
    public static class ImageUtils
    {
        public static async Task<string> ToBase64Image(this Image<L8> image)
        {
            using var memoryStream = new MemoryStream();
            await image.SaveAsPngAsync(memoryStream);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            return $"data:image/png;base64,{base64}";
        }

        public static async Task<Image<L8>> SaveFloatArrayAsPngAsync(float[,] data)
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

        public static float[,] NormalizeImagePixels(this Image<L8> image)
        {
            float[,] result = new float[image.Height, image.Width];

            image.ProcessPixelRows(accessor =>
            {
                for (int rowIndex = 0; rowIndex < image.Height; rowIndex++)
                {
                    var row = accessor.GetRowSpan(rowIndex);

                    for (int colIndex = 0; colIndex < image.Width; colIndex++)
                    {
                        byte value = row[colIndex].PackedValue;   // grayscale 0..255
                        result[rowIndex, colIndex] = value / 255f;      // normalize to 0..1
                    }
                }
            });

            return result;
        }
    }
}
