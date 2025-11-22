import { createSignal, onCleanup, createMemo, For } from "solid-js";
import { GPUWorkload, Workload } from "../../types";
import { Line } from "solid-chartjs";
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  Filler,
} from "chart.js";
import { getCPU_chartData, cpuChartOptions } from "./cpu";
import { getGPU_chartData, gpuChartOptions } from "./gpu";
import { getRAM_chartData, ramChartOptions } from "./ram";

// Register Chart.js components
ChartJS.register(
  Title,
  Tooltip,
  Legend,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  Filler
);

export default function SystemMonitor() {
  const [getWorkloadData, setWorkloadData] = createSignal<
    Array<Workload & { time: number }>
  >([]);

  let eventSource: EventSource;

  const startSSE = () => {
    if (eventSource) eventSource.close();

    eventSource = new EventSource(
      `${import.meta.env.VITE_API_URL}/sse/workload`
    );

    eventSource.onmessage = (ev) => {
      try {
        const data = JSON.parse(ev.data);
        if (!data || !data.CPU) return;

        setWorkloadData((prev) => {
          const arr = [...prev];
          const dataWithTime = { ...data, time: Date.now() };

          arr.push(dataWithTime);

          // Limit history to keep charts performant
          if (arr.length > 100) arr.shift();

          return arr;
        });
      } catch (e) {
        console.error("bad sse json", e);
      }
    };
  };

  startSSE();

  onCleanup(() => {
    if (eventSource) eventSource.close();
  });

  // Prepare chart data using createMemo to optimize updates
  const cpuChartData = createMemo(() => getCPU_chartData(getWorkloadData()));
  const gpuChartData = createMemo(() => {
    const workloadData = getWorkloadData();
    const timing = workloadData.map((x) => x.time);
    const gpuCount = workloadData[0]?.GPUs?.length || 0;

    return Array.from({ length: gpuCount }, (_v, i) => ({
      name: workloadData[0].GPUs[i].Name,
      data: getGPU_chartData(
        timing,
        workloadData.map((data) => data.GPUs[i])
      ),
    }));
  });
  const ramChartData = createMemo(() => getRAM_chartData(getWorkloadData()));
  const lastData = createMemo(() => getWorkloadData().at(-1));

  return (
    <div class="p-4 bg-gray-900 text-white flex flex-col gap-y-3">
      <h2 class="text-lg font-bold mb-4 text-gray-100 text-center">
        Навантаження системи
      </h2>
      <div class="w-full flex flex-col gap-y-2">
        <h3 class="text-xl">Процесор</h3>
        <div class="w-full h-[300px]">
          <Line
            data={cpuChartData()}
            options={cpuChartOptions}
            width={500}
            height={400}
          />
        </div>
        <table class="border-collapse shrink">
          <tbody>
            <tr class="">
              <td class="px-2 w-40 py-1 border border-white">Використання</td>
              <td class="px-2 py-1 border border-white font-bold">
                {lastData() ? `${lastData()?.CPU.UsagePercent}%` : "N/A"}
              </td>
            </tr>
            <tr class="">
              <td class="px-2 w-40 py-1 border border-white">Швидкість</td>
              <td class="px-2 py-1 border border-white font-bold">
                {lastData()
                  ? `${lastData()?.CPU.CurrentClockSpeed} ГГц`
                  : "N/A"}
              </td>
            </tr>
            <tr class="">
              <td class="px-2 w-40 py-1 border border-white">Процеси</td>
              <td class="px-2 py-1 border border-white font-bold">
                {lastData() ? `${lastData()?.CPU.ProcessCount}` : "N/A"}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <hr class="border-white" />
      <div class="w-full flex flex-col gap-y-2">
        <For
          each={gpuChartData().filter(
            (data) => data.data.datasets[0].data.at(-1) !== 0
          )}
        >
          {(gpuData) => (
            <>
              <h3 class="text-xl">{gpuData.name}</h3>
              <div class="h-[300px]">
                <Line
                  data={gpuData.data}
                  options={gpuChartOptions}
                  width={500}
                  height={300}
                />
              </div>
            </>
          )}
        </For>
      </div>
      <hr class="border-white" />
      <div class="w-full flex flex-col gap-y-2">
        <h3 class="text-xl">Оперативна пам'ять</h3>
        <div class="w-full h-[300px]">
          <Line
            data={ramChartData()}
            options={ramChartOptions}
            width={500}
            height={300}
          />
        </div>
        <table class="border-collapse shrink">
          <tbody>
            <tr class="">
              <td class="px-2 w-40 py-1 border border-white">%</td>
              <td class="px-2 py-1 border border-white font-bold">
                {lastData()
                  ? `${lastData()?.RAM.UsagePercent.toFixed(2)}%`
                  : "N/A"}
              </td>
            </tr>
            <tr class="">
              <td class="px-2 w-40 py-1 border border-white">Використано</td>
              <td class="px-2 py-1 border border-white font-bold">
                {lastData()
                  ? `${lastData()?.RAM.UsedGb.toFixed(
                      2
                    )}/${lastData()?.RAM.TotalGb.toFixed(2)} Гб `
                  : "N/A"}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
