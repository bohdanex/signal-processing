using NWaves.Audio;
using NWaves.Signals;

namespace backend.Utils
{
    public static class SignalExtensions
    {
        public static List<DiscreteSignal> Subdivide(this DiscreteSignal signal, int divisions)
        {
            List<DiscreteSignal> signals = new List<DiscreteSignal>();
            var segmentLength = signal.Length / divisions;
            for (int i = 0; i < divisions; i++)
            {
                signals.Add(signal[i * segmentLength, (i + 1) * segmentLength]);
            }

            return signals;
        }

        public static async Task<WaveFile> ToWaveFileAsync(this IFormFile audioFile)
        {
            using var memoryStream = new MemoryStream();
            await audioFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return new WaveFile(memoryStream);
        }
    }
}
