using backend.Utils;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Windows;
using Silk.NET.OpenCL;

namespace backend.Services._1D
{
    public class STFTService
    {
        public List<float[]> Transform(DiscreteSignal signal, int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Blackman)
        {
            var stft = new Stft(windowSize, hopSize, window);
            return stft.Spectrogram(signal, true);
        }

        public async Task<List<float[]>> TransformParallel(DiscreteSignal signal, int divisions = 6, int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Blackman)
        {
            IEnumerable<float[]>[] result = new List<float[]>[divisions];
            var signals = signal.Subdivide(divisions).Select((sig, index) => new { Index = index, Signal = sig });

            await Parallel.ForEachAsync(signals, async (signalData, cancelToken) =>
            {
                var stft = new Stft(windowSize, hopSize, window);
                List<float[]> spectrogram = stft.Spectrogram(signalData.Signal, true);
                result[signalData.Index] = spectrogram;
            });

            
            return result.Aggregate((left, right) => left.Concat(right)).ToList();
        }
    }
}



