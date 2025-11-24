import { GPUWorkload } from "../../types";
import { createChartOptions } from "../../utils/charts";

export const gpuChartOptions = createChartOptions(0);

export function getGPU_chartData(timing: number[], gpu: GPUWorkload[]) {
  return {
    labels: timing.map((time) =>
      new Date(time).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
      })
    ),
    datasets: [
      {
        label: "GPU Memory usage",
        data: gpu.map((info) => info.MemoryUsedMb ?? 0),
        borderColor: "rgb(59, 130, 246)", // Blue-500
        backgroundColor: "rgba(59, 130, 246, 0.1)", // Transparent Blue
        borderWidth: 2,
        pointRadius: 0, // Hide points for cleaner look, hover will still work
        pointHoverRadius: 4,
        fill: true,
        tension: 0.4, // Smooth curves
      },
    ],
  };
}
