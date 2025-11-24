import { createSignal, onCleanup, Show } from "solid-js";
import axiosInstance from "../../axios/axiosInstance";
import { ResultWithBenchmark } from "../../axios/types";
import BandPassConfigs, {
  BandPassFilterData,
  getBandPassFilterSettings,
} from "./filters/BandPassConfig";
import LowHighPassConfigs, {
  getLowHighPassFilterSettings,
  LowHighPassFilterData,
} from "./filters/LowHighPassConfigs";
import { b64toBlob } from "../../utils/fileUtils";
import BenchmarkResults from "./BenchmarkResults";

export type FilterType = "high-pass" | "low-pass" | "band-pass";

const applyLowHighPassFilter = async (
  filterSettings: LowHighPassFilterData,
  audioFile: File,
  type: "high-pass" | "low-pass",
  parallel: boolean
): Promise<ResultWithBenchmark<string>> => {
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
): Promise<ResultWithBenchmark<string>> => {
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

export default function FilterSection(props: { audioFile: File }) {
  const [getFilterType, setFilterType] = createSignal<FilterType>("low-pass");
  const [getFilteredResult, setFilterResult] = createSignal<
    ResultWithBenchmark<string>["benchmarkResult"] | null
  >(null);
  const [getFilteredSignalURL, setFilteredSignalURL] = createSignal<
    string | null
  >(null);
  let filteredResultSourceElement: HTMLSourceElement | undefined;
  let filteredResultAudioElement: HTMLAudioElement | undefined;

  const onRunFilterClick = async (parallel: boolean) => {
    const filterType = getFilterType();
    const selectedFile = props.audioFile;

    if (!selectedFile) return;

    let result: ResultWithBenchmark<string> | null = null;
    if (filterType === "low-pass" || filterType === "high-pass") {
      result = await applyLowHighPassFilter(
        getLowHighPassFilterSettings(),
        selectedFile,
        filterType,
        parallel
      );
    } else if (filterType === "band-pass") {
      console.log("settings", getBandPassFilterSettings());

      result = await applyBandPassFilter(
        getBandPassFilterSettings(),
        selectedFile,
        parallel
      );
    }

    if (result) {
      const blob = b64toBlob(result.data, "audio/wav");
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
    <>
      <div class="flex gap-x-3">
        <label for="filter-selector">Оберіть фільтр</label>
        <select
          id="filter-selector"
          value={getFilterType()}
          class="bg-blue-50 border border-gray-400 duration-150 px-3 py-1"
          oninput={(event) => setFilterType(event.target.value as FilterType)}
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
      <div class="flex flex-row">
        <button
          onclick={() => onRunFilterClick(false)}
          class="bg-amber-400 border border-gray-400 hover:bg-amber-500 duration-150 rounded-bl-md rounded-tl-md p-2 block text-lg font-bold cursor-pointer grow"
        >
          Запустити фільтрацію
        </button>
        <button
          onclick={() => onRunFilterClick(true)}
          class="bg-blue-400 border border-gray-400 hover:bg-blue-500 duration-150 rounded-br-md rounded-tr-md p-2 block text-lg font-bold cursor-pointer grow"
        >
          Паралельно
        </button>
      </div>
      <Show when={getFilteredSignalURL()}>
        <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
          <div>
            <h2 class="text-lg">Аудіо сигнал після застосування фільтру</h2>
            <audio ref={filteredResultAudioElement} controls>
              <source
                src={getFilteredSignalURL()!}
                type="audio/wav"
                ref={filteredResultSourceElement}
              />
              Your browser does not support the audio element.
            </audio>
          </div>
          <hr />
          <BenchmarkResults results={getFilteredResult()!} />
        </div>
      </Show>
    </>
  );
}
