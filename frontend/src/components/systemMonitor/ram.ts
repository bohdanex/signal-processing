import { Workload } from "../../types";

export const ramChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  animation: {
    duration: 0, // Disable animation for real-time performance
  },
  interaction: {
    mode: "index" as const,
    intersect: false,
  },
  plugins: {
    legend: {
      display: true,
      position: "top" as const,
      labels: {
        color: "#9ca3af", // gray-400
      },
    },
    tooltip: {
      enabled: true,
    },
  },
  scales: {
    x: {
      grid: {
        color: "rgba(156, 163, 175, 0.1)", // subtle grid
      },
      ticks: {
        color: "#9ca3af",
        maxTicksLimit: 8, // Prevent x-axis crowding
      },
    },
    y: {
      min: 0,
      grid: {
        color: "rgba(156, 163, 175, 0.1)",
      },
      ticks: {
        color: "#9ca3af",
      },
    },
  },
};

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