export const stftChartData = (data: number[]) => {
  const frequs = data.map((_d, index) => Math.round((index / data.length) * 22000));

  return {
    labels: frequs,
    datasets: [
      {
        label: "Amplitude",
        data: data.map((d) => d * 100),
        borderColor: "rgb(59, 130, 246)", // Blue-500
        backgroundColor: "rgba(59, 130, 246, 0.1)", // Transparent Blue
        borderWidth: 2,
        pointRadius: 0, // Hide points for cleaner look, hover will still work
        pointHoverRadius: 4,
        fill: true,
        tension: 0.1, // Smooth curves
      },
    ],
  }
}