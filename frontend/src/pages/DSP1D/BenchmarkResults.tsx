import { BenchmarkResult } from "../../axios/types";

export default function BenchmarkResults(props: { results: BenchmarkResult }) {
  return (
    <div>
      <div class="flex flex-row gap-x-2">
        <span>Час обробки</span>
        <span class="font-bold">
          {props.results.elapsedSeconds.toFixed(4)} с
        </span>
      </div>
      <div class="flex flex-row gap-x-2">
        <span>Виділено пам'яті</span>
        <span class="font-bold">
          {(props.results.usedMemory / 1024 / 1024).toFixed(2)} Мб
        </span>
      </div>
    </div>
  );
}
