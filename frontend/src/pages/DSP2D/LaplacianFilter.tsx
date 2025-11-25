import { createSignal, Show } from "solid-js";
import axiosInstance from "../../axios/axiosInstance";
import { ResultWithBenchmark } from "../../axios/types";
import Process2DResult from "./2DProcessResult";

export default function LaplacianFilter(props: { imageFile: File }) {
  const [getSettings, setSettings] = createSignal({
    scale: 2,
  });
  const [getResult, setResult] = createSignal<ResultWithBenchmark<string>>();

  const onRunClick = async () => {
    const form = new FormData();
    form.append("file", props.imageFile);
    form.append("scale", getSettings().scale.toString());
    const response = await axiosInstance.postForm("2d/filter/laplacian", form);
    const result = response.data as ResultWithBenchmark<string>;
    setResult(result);
  };

  return (
    <>
      <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
        <h3 class="text-lg font-bold">Фільтр Лапласа</h3>
        <div class="flex gap-x-2 justify-between">
          <label for="laplacian-scale">Масштаб</label>
          <input
            id="laplacian-scale"
            type="number"
            class="w-20 bg-blue-200 px-2 py-1 rounded-xs"
            min={0}
            step={0.001}
            value={getSettings().scale}
            on:input={(event) => {
              if (Number.isNaN(event.target.valueAsNumber)) return;
              setSettings((prev) => ({
                ...prev,
                scale: event.target.valueAsNumber,
              }));
            }}
          />
        </div>
      </div>
      <div class="flex flex-row">
        <button
          onclick={() => onRunClick()}
          class="bg-amber-400 border border-gray-400 hover:bg-amber-500 duration-150 rounded-md p-2 block text-lg font-bold cursor-pointer grow"
        >
          Запустити
        </button>
      </div>
      <Show when={getResult()}>
        <Process2DResult
          benchmark={getResult()!.benchmarkResult}
          imageSrc={getResult()!.data}
        />
      </Show>
    </>
  );
}
