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
  const [getResult, setResult] =
    createSignal<ResultWithBenchmark<ResponseData>>();
  const [getStftSettings, setStftSettings] = createSignal<STFT_Settings>({
    windowSize: 1024,
    hopSize: 256,
  });

  const transformSignal = async (parallel: boolean) => {
    const result = await stftTransform(
      getStftSettings(),
      props.audioFile,
      parallel
    );
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
      <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
        <h3 class="font-bold">
          Налаштування короткочасного перетворення Фур'є
        </h3>
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
          <label for="hop-size">Зсув вікна</label>
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
        <div class="bg-blue-50 p-4 shadow-2xs rounded-lg flex flex-col gap-y-2">
          <BenchmarkResults results={getResult()!.benchmarkResult} />
          <div>
            <p class="mb-1 text-lg font-bold text-center">Спектрограма</p>
            <div class="h-[400px] w-full bg-gray-900 text-white p-2 rounded-lg">
              <Line
                data={stftChartData(getSpectrum()!)}
                options={createChartOptions(
                  0,
                  Math.max(...getResult()!.data.channels[0].flat()),
                  true,
                  10,
                  (_value, index, values) => {
                    if (index === 0 || index === values.length - 1) {
                      return (
                        Math.round((index / (values.length - 1)) * 22050) +
                        " Гц"
                      );
                    }
                  }
                )}
                width={500}
                height={400}
              />
            </div>
          </div>
        </div>
      </Show>
    </>
  );
}
