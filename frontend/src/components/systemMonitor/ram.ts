import { Workload } from "../../types";
import { createChartOptions } from "../../utils/charts";

export const ramChartOptions = createChartOptions(0)

export function getRAM_chartData(workloadData: Array<Workload & { time: number }>) {
  return {
    labels: workloadData.map((d) =>
      new Date(d.time).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
      })
    ),
    datasets: [
      {
        label: "Ram Usage %",
        data: workloadData.map((d) => d.RAM?.UsagePercent ?? 0),
        borderColor: "rgb(59, 130, 246)", // Blue-500
        backgroundColor: "rgba(59, 130, 246, 0.1)", // Transparent Blue
        borderWidth: 2,
        pointRadius: 0, // Hide points for cleaner look, hover will still work
        pointHoverRadius: 4,
        fill: true,
        tension: 0.1, // Smooth curves
      },
    ],
  };
}