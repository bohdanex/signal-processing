using NWaves.Filters.Base;
using NWaves.Filters.Bessel;
using NWaves.Signals;

namespace backend.Services._1D
{
    public class SignalFilter1D
    {
        private record SignalSegment(int SignalIndex, int ChannelIndex, DiscreteSignal Signal);
        public List<DiscreteSignal> Signals { get; private set; }

        public SignalFilter1D(List<DiscreteSignal> signals)
        {
            this.Signals = signals;
        }

        public List<DiscreteSignal> ApplyFilter(IFilter filter)
        {
            return Signals.Select((signal) => filter.ApplyTo(signal)).ToList();
        }

        public Task<List<DiscreteSignal>> ApplyLowPassFilterParallel(double frequency, int order)
        {
            return ApplyFilterParallel(() => new LowPassFilter(frequency, order));
        }

        public async Task<List<DiscreteSignal>> ApplyFilterParallel(Func<IFilter> filterFactory)
        {
            DiscreteSignal[][] filteredSignals = new DiscreteSignal[Signals.Count][];
            var segmentsPerChannel = (Environment.ProcessorCount / Signals.Count);
            var segmentedSignals = new List<SignalSegment>();

            for (int i = 0; i < Signals.Count; i++)
            {
                var signal = Signals[i];
                var segmentLength = signal.Length / segmentsPerChannel;
                filteredSignals[i] = new DiscreteSignal[segmentsPerChannel];
                for (int j = 0; j < segmentsPerChannel; j++)
                {
                    segmentedSignals.Add(new SignalSegment(j, i, signal[j * segmentLength, (j + 1) * segmentLength]));
                }
            }

            await Parallel.ForEachAsync(segmentedSignals, async (signalSegment, token) =>
            {
                var lowPassFilter = filterFactory();
                var filtered = lowPassFilter.ApplyTo(signalSegment.Signal);
                filteredSignals[signalSegment.ChannelIndex][signalSegment.SignalIndex] = filtered;
            });

            var mergedSignal = new List<DiscreteSignal>();

            for (int i = 0; i < filteredSignals.Length; i++)
            {
                var firstSignal = filteredSignals[i][0];
                for (int j = 1; j < filteredSignals[i].Length; j++)
                {
                    firstSignal = firstSignal.Concatenate(filteredSignals[i][j]);
                }

                mergedSignal.Add(firstSignal);
            }

            return mergedSignal;
        }
    }   
}
