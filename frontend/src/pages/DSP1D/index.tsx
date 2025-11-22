import { createSignal, onCleanup, Show } from "solid-js";
import LowHighPassConfigs, {
  getLowHighPassFilterSettings,
  LowHighPassFilterData,
} from "./filters/LowHighPassConfigs";
import axiosInstance from "../../axios/axiosInstance";
import { b64toBlob } from "../../utils/fileUtils";
import BandPassConfigs, {
  BandPassFilterData,
  getBandPassFilterSettings,
} from "./filters/BandPassConfig";

type FilterType = "high-pass" | "low-pass" | "band-pass";

interface FilterResult {
  data: string;
  benchmarkResult: {
    startMemory: number;
    endMemory: number;
    usedMemory: number;
    startTime: string;
    finishTime: string;
    elapsedSeconds: number;
  };
}

const [getIsParallel, setIsParallel] = createSignal(false);

const applyLowHighPassFilter = async (
  filterSettings: LowHighPassFilterData,
  audioFile: File,
  type: "high-pass" | "low-pass",
  parallel: boolean
): Promise<FilterResult> => {
  const formData = new FormData();
  formData.append("qFactor", filterSettings.qFactor.toString());
  formData.append("cutOffFreq", filterSettings.cutOffFreq.toString());
  formData.append("audioFile", audioFile);
  const result = await axiosInstance.postForm(
    `1d/filter/${type}?parallel=${parallel}`,
    formData
  );
  return result.data;
};

const applyBandPassFilter = async (
  filterSettings: BandPassFilterData,
  audioFile: File,
  parallel: boolean
): Promise<FilterResult> => {
  const formData = new FormData();
  formData.append("highFrequency", filterSettings.highFrequency.toString());
  formData.append("lowFrequency", filterSettings.lowFrequency.toString());
  formData.append("order", filterSettings.order.toString());
  formData.append("audioFile", audioFile);
  const result = await axiosInstance.postForm(
    `1d/filter/band-pass?parallel=${parallel}`,
    formData
  );
  return result.data;
};

export default function DSP1D() {
  const [getSelectedFile, setSelectedFile] = createSignal<File | null>(null);
  const [getFileURL, setFileURL] = createSignal<string | null>(null);
  const [getFilterType, setFilterType] = createSignal<FilterType>("low-pass");
  const [getFilteredResult, setFilterResult] = createSignal<
    FilterResult["benchmarkResult"] | null
  >(null);
  const [getFilteredSignalURL, setFilteredSignalURL] = createSignal<
    string | null
  >(null);
  let filteredResultSourceElement: HTMLSourceElement | undefined;
  let filteredResultAudioElement: HTMLAudioElement | undefined;

  onCleanup(() => {
    const fileURL = getFileURL();
    if (fileURL) {
      URL.revokeObjectURL(fileURL);
    }
  });

  const onRunFilterClick = async () => {
    const filterType = getFilterType();
    const selectedFile = getSelectedFile();
    if (!selectedFile) return;

    let result: FilterResult | null = null;
    if (filterType === "low-pass" || filterType === "high-pass") {
      result = await applyLowHighPassFilter(
        getLowHighPassFilterSettings(),
        selectedFile,
        filterType,
        getIsParallel()
      );
    } else if (filterType === "band-pass") {
      console.log("settings", getBandPassFilterSettings());

      result = await applyBandPassFilter(
        getBandPassFilterSettings(),
        selectedFile,
        getIsParallel()
      );
    }

    if (result) {
      const blob = b64toBlob(result.data, "audio/wav");
      const file = new File([blob], "filtered.wav");
      setFilterResult(result.benchmarkResult);

      const currentURL = getFilteredSignalURL();
      if (currentURL) {
        URL.revokeObjectURL(currentURL);
      }

      const url = URL.createObjectURL(blob);
      if (filteredResultSourceElement && filteredResultAudioElement) {
        filteredResultSourceElement.src = url;
        filteredResultAudioElement.load();
        filteredResultAudioElement.play();
      }

      setFilteredSignalURL(url);
    }
  };

  return (
    <div class="flex flex-col mt-2 gap-y-4">
      <div class="flex flex-col gap-y-2 p-3 border-gray-900 border-2 border-dashed h-min rounded-md">
        <div class="flex gap-x-4">
          <label
            for="audio-file-picker"
            class="bg-blue-50 border border-gray-400 hover:bg-blue-200 duration-150 rounded-md p-4 block text-xl"
          >
            Виберіть файл
          </label>
          <input
            id="audio-file-picker"
            type="file"
            accept="audio/wav"
            class="hidden"
            placeholder="Виберіть файл"
            on:input={(event) => {
              const file = event.target.files?.[0];

              if (file) {
                setSelectedFile(file);
                const existingURL = getFileURL();
                if (existingURL) {
                  URL.revokeObjectURL(existingURL);
                  setFileURL(null);
                }
                setFileURL(URL.createObjectURL(file));
              }
            }}
          />
          <Show when={getSelectedFile()}>
            <div class="flex flex-col gap-y-2 justify-between">
              <span class="text-lg">
                Назва: <span class="font-bold">{getSelectedFile()!.name}</span>
              </span>
              <span>
                Розмір: {(getSelectedFile()!.size / 1024 / 1024).toFixed(2)} Мб
              </span>
            </div>
          </Show>
        </div>
        <Show when={getSelectedFile()}>
          <audio controls>
            <source src={getFileURL()!} type="audio/wav" />
            Your browser does not support the audio element.
          </audio>
        </Show>
      </div>
      <Show when={getSelectedFile()}>
        <>
          <div class="flex gap-x-3">
            <label for="filter-selector">Оберіть тип фільтру</label>
            <select
              id="filter-selector"
              value={getFilterType()}
              class="bg-blue-50 border border-gray-400 duration-150 px-3 py-1"
              oninput={(event) =>
                setFilterType(event.target.value as FilterType)
              }
            >
              <option value={"low-pass"}>ФНЧ</option>
              <option value={"high-pass"}>ФВЧ</option>
              <option value={"band-pass"}>Band pass</option>
            </select>
          </div>
          <Show when={getFilterType() === "low-pass"}>
            <LowHighPassConfigs title="Налаштування ФНЧ" />
          </Show>
          <Show when={getFilterType() === "high-pass"}>
            <LowHighPassConfigs title="Налаштування ФВЧ" />
          </Show>
          <Show when={getFilterType() === "band-pass"}>
            <BandPassConfigs />
          </Show>
          <div class="flex flex-row gap-x-2 items-center">
            <button
              onclick={onRunFilterClick}
              class="bg-amber-400 border border-gray-400 hover:bg-blue-200 duration-150 rounded-md p-2 block text-lg font-bold cursor-pointer grow"
            >
              Запустити фільтрацію
            </button>
            <div class="flex flex-col gap-y-2">
              <label for="checkbox-parallel">Паралелізація на CPU</label>
              <input
                type="checkbox"
                value={String(getIsParallel())}
                on:input={(event) => setIsParallel(event.target.checked)}
              />
            </div>
          </div>
        </>
      </Show>
      <Show when={getFilteredSignalURL()}>
        <hr class="my-2" />
        <div>
          <h2 class="mb-2 text-lg">Аудіо сигнал після застосування фільтру</h2>
          <audio ref={filteredResultAudioElement} controls>
            <source
              src={getFilteredSignalURL()!}
              type="audio/wav"
              ref={filteredResultSourceElement}
            />
            Your browser does not support the audio element.
          </audio>
        </div>
        <Show when={getFilteredResult()}>
          <div>
            <div class="flex flex-row gap-x-2">
              <span>Час обробки</span>
              <span class="font-bold">
                {getFilteredResult()!.elapsedSeconds.toFixed(4)} с
              </span>
            </div>
            <div class="flex flex-row gap-x-2 mt-1">
              <span>Виділено пам'яті</span>
              <span class="font-bold">
                {(getFilteredResult()!.usedMemory / 1024 / 1024).toFixed(2)} Мб
              </span>
            </div>
          </div>
        </Show>
      </Show>
    </div>
  );
}
