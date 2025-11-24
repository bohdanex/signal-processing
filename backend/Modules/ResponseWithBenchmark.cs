using backend.Services.Implementation;

namespace backend.Modules
{
    public class ResponseWithBenchmark<T>
    {
        public ResponseWithBenchmark(T data, BenchmarkResult benchmarkResult)
        {
            Data = data;
            BenchmarkResult = benchmarkResult;
        }

        public T Data { get; private set; }
        public BenchmarkResult BenchmarkResult{ get; private set; }
    }
}
