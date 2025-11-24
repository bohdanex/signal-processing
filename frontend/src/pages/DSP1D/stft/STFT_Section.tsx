import { createEffect, createSignal, Show } from "solid-js";
import axiosInstance from "../../../axios/axiosInstance";
import { ResultWithBenchmark } from "../../../axios/types";
import BenchmarkResults from "../BenchmarkResults";
import { Line } from "solid-chartjs";
import { stftChartData } from "./stftChart";
import { createChartOptions } from "../../../utils/charts";
import { getAudioTime } from "..";

interface STFT_Settings {
  windowSize: number;
  hopSize: number;
}

interface ResponseData {
  channels: Array<Array<number[]>>;
  duration: number;
  hopSize: number;
  sampleRate: number;
  windowSize: number;
}

async function stftTransform(
  settings: STFT_Settings,
  audioFile: File,
  parallel: boolean
): Promise<ResultWithBenchmark<ResponseData>> {
  const formData = new FormData();
  formData.append("audioFile", audioFile);
  formData.append("windowSize", settings.windowSize.toString());
  formData.append("hopSize", settings.hopSize.toString());
  const result = await axiosInstance.postForm(
    `1d/${parallel ? "stft-parallel" : "stft"}`,
    formData
  );
  return result.data;
}

export default function STFT_Section(props: { audioFile: File }) {
  const { audioFile } = props;
  const [getResult, setResult] =
    createSignal<ResultWithBenchmark<ResponseData>>();
  const [getStftSettings, setStftSettings] = createSignal<STFT_Settings>({
    windowSize: 1024,
    hopSize: 256,
  });

  const transformSignal = async (parallel: boolean) => {
    const result = await stftTransform(getStftSettings(), audioFile, parallel);
    setSpectrum(result.data.channels[0][0]);
    setResult(result);
  };

  const [getSpectrum, setSpectrum] = createSignal<number[]>();

  createEffect(() => {
    const result = getResult();
    if (result) {
      const index = Math.round(
        (getAudioTime() / result.data.duration) *
          (result.data.channels[0].length - 1)
      );
      setSpectrum(result.data.channels[0][index]);
    }
  });

  return (
    <>
      <h3>Налаштування короткочасного перетворення Фур'є</h3>
      <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
        <div class="flex gap-x-2 justify-between">
          <label for="window-size">Розмір вікна</label>
          <input
            id="window-size"
            type="number"
            class="w-20 bg-blue-200 px-2 py-1 rounded-xs"
            min={0}
            step={1}
            value={getStftSettings().windowSize}
            on:input={(event) => {
              if (Number.isNaN(event.target.valueAsNumber)) return;
              setStftSettings((prev) => ({
                ...prev,
                windowSize: event.target.valueAsNumber,
              }));
            }}
          />
        </div>
        <div class="flex gap-x-2 justify-between">
          <label for="hop-size">Розмір стрибка</label>
          <input
            id="hop-size"
            type="number"
            class="w-20 bg-blue-200 px-2 py-1 rounded-xs"
            min={0}
            step={1}
            value={getStftSettings().hopSize}
            on:input={(event) => {
              if (Number.isNaN(event.target.valueAsNumber)) return;
              setStftSettings((prev) => ({
                ...prev,
                hopSize: event.target.valueAsNumber,
              }));
            }}
          />
        </div>
      </div>
      <div class="flex flex-row">
        <button
          onclick={() => transformSignal(false)}
          class="bg-amber-400 border border-gray-400 hover:bg-amber-500 duration-150 rounded-bl-md rounded-tl-md p-2 block text-lg font-bold cursor-pointer grow"
        >
          Перетворити
        </button>
        <button
          onclick={() => transformSignal(true)}
          class="bg-blue-400 border border-gray-400 hover:bg-blue-500 duration-150 rounded-br-md rounded-tr-md p-2 block text-lg font-bold cursor-pointer grow"
        >
          Паралельно
        </button>
      </div>
      <Show when={getResult()}>
        <BenchmarkResults results={getResult()!.benchmarkResult} />
        <div class="h-[400px] w-[400px] bg-gray-900 text-white">
          <Line
            data={stftChartData(getSpectrum()!)}
            options={createChartOptions(
              0,
              Math.max(...getResult()!.data.channels[0].flat()),
              true,
              4
            )}
            width={500}
            height={400}
          />
        </div>
      </Show>
    </>
  );
}
