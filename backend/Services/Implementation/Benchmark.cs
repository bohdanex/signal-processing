using System.Diagnostics;
using System.Runtime;
using System.Text;

namespace backend.Services.Implementation
{
    /// <summary>
    /// Stores the results of a single benchmark run.
    /// </summary>
    public class BenchmarkResult
    {
        public ulong StartMemory { get; }
        public ulong EndMemory { get; }
        public ulong UsedMemory { get; }
        public DateTime StartTime { get; }
        public DateTime FinishTime { get; }
        public double ElapsedSeconds { get; }

        public BenchmarkResult(ulong startMem, ulong endMem, DateTime start, DateTime finish, double elapsed)
        {
            StartMemory = startMem;
            EndMemory = endMem;
            UsedMemory = endMem - startMem;
            StartTime = start;
            FinishTime = finish;
            ElapsedSeconds = elapsed;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Benchmark Results ---");
            sb.AppendLine($"Start Time: {StartTime:HH:mm:ss.fff}");
            sb.AppendLine($"Finish Time: {FinishTime:HH:mm:ss.fff}");
            sb.AppendLine($"Total Elapsed Time: {ElapsedSeconds:F4} seconds");
            sb.AppendLine($"Start Memory: {StartMemory / 1024.0 / 1024.0:F2} MB");
            sb.AppendLine($"End Memory: {EndMemory / 1024.0 / 1024.0:F2} MB");
            sb.AppendLine($"Memory Used (Net): {UsedMemory / 1024.0 / 1024.0:F2} MB");
            sb.AppendLine("-------------------------");
            return sb.ToString();
        }
    }

    public class Benchmark
    {
        // 1. Fields to hold state
        private Stopwatch _stopwatch;
        private ulong _startMemory;
        private BenchmarkResult? _result;

        // --- Public Methods ---

        /// <summary>
        /// Initializes and starts the benchmark.
        /// </summary>
        public void Start()
        {
            // Reset state
            _result = null;

            // Force garbage collection to get a cleaner baseline memory usage
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Get the current memory usage (in bytes)
            // The argument 'true' causes the method to wait for a garbage collection pass
            // to get a more accurate number.
            _startMemory = (ulong)GC.GetTotalMemory(true);

            // Initialize and start the stopwatch
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stops the benchmark and calculates the results.
        /// </summary>
        public BenchmarkResult Stop()
        {
            if (_stopwatch == null || !_stopwatch.IsRunning)
            {
                throw new InvalidOperationException("Benchmark was not started or has already been stopped.");
            }

            // Stop the stopwatch
            _stopwatch.Stop();

            // Capture end time and elapsed time
            var finishTime = DateTime.Now;
            var elapsed = _stopwatch.Elapsed.TotalSeconds;

            // Force garbage collection and capture end memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            ulong endMemory = (ulong)GC.GetTotalMemory(true);

            // Store the results
            _result = new BenchmarkResult(
                _startMemory,
                endMemory,
                finishTime.Subtract(_stopwatch.Elapsed), // Calculate StartTime
                finishTime,
                elapsed
            );

            return _result;
        }

        /// <summary>
        /// Gets the final BenchmarkResult object.
        /// </summary>
        public BenchmarkResult? GetResult()
        {
            return _result;
        }
    }
}