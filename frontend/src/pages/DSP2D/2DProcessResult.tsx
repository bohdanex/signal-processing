import { BenchmarkResult } from "../../axios/types";
import BenchmarkResults from "../DSP1D/BenchmarkResults";

export default function Process2DResult(props: {
  benchmark: BenchmarkResult;
  imageSrc: string;
}) {
  return (
    <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
      <h3 class="text-lg font-bold">Результат обробки зображення</h3>
      <BenchmarkResults results={props.benchmark} />
      <img
        src={props.imageSrc}
        width={400}
        height={400}
        class="border-2 border-gray-800 outline-1 outline-gray-50 rounded-md"
      />
    </div>
  );
}
