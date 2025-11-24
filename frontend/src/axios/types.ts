export interface BenchmarkResult {
  startMemory: number;
  endMemory: number;
  usedMemory: number;
  startTime: string;
  finishTime: string;
  elapsedSeconds: number;
};

export interface ResultWithBenchmark<T> {
  data: T;
  benchmarkResult: BenchmarkResult;
}