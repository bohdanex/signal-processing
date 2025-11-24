export function createChartOptions(minY?: number, maxY?: number, showTicksX = false, maxTicksX = 8, tickCallback?: (value: number, index: number, values: number[]) => void) {
  return {
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
          callback: tickCallback,
          color: "#9ca3af",
          maxTicksLimit: maxTicksX, // Prevent x-axis crowding
          display: showTicksX
        },
      },
      y: {
        min: minY,
        max: maxY,
        grid: {
          color: "rgba(156, 163, 175, 0.1)",
        },
        ticks: {
          color: "#9ca3af",
        },
      },
    },
  }
}