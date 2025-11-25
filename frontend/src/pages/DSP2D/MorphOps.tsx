import { createSignal, Show } from "solid-js";
import { ResultWithBenchmark } from "../../axios/types";
import Process2DResult from "./2DProcessResult";
import axiosInstance from "../../axios/axiosInstance";
import createExperiment from "../../features/createExperiment";
import { BsInfoCircleFill } from "solid-icons/bs";

type AcceleratorType = "cpu" | "cl" | "cuda";
type OperationType = "dilation" | "erosion";

interface Settings {
  radius: number;
  accelerator: AcceleratorType;
  operation: OperationType;
}

export default function MorphOps(props: { imageFile: File }) {
  const [getSettings, setSettings] = createSignal<Settings>({
    radius: 10,
    accelerator: "cl",
    operation: "dilation",
  });
  const [getResult, setResult] = createSignal<ResultWithBenchmark<string>>();
  const { experimentData, addResult, downloadJSON } = createExperiment(
    getSettings,
    "morph operation"
  );
  const [experimentIsRunning, setExperimentIsRunning] = createSignal(false);

  const makeRequest = async (): Promise<ResultWithBenchmark<string>> => {
    const form = new FormData();
    const { radius, accelerator, operation } = getSettings();
    form.append("file", props.imageFile);
    form.append("radius", radius.toString());
    const response = await axiosInstance.postForm(
      `2d/morph/${operation}/${accelerator}`,
      form
    );
    return response.data;
  };

  const onRunClick = async () => {
    setResult(await makeRequest());
  };

  let experimentInput: HTMLInputElement | undefined;

  const onRunExperiment = async () => {
    setResult(undefined);
    setExperimentIsRunning(true);
    if (!experimentInput) return;
    const nTimes = experimentInput.valueAsNumber;

    for (let i = 0; i < nTimes; i++) {
      const benchmarkResult = (await makeRequest()).benchmarkResult;
      addResult(benchmarkResult);
    }

    downloadJSON(`${experimentData().name}_${new Date().toISOString()}`);
    setExperimentIsRunning(false);
  };

  return (
    <>
      <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
        <h3 class="text-lg font-bold">Морфологічні операції</h3>
        <div class="flex gap-x-2 justify-between">
          <label for="morph-radius">Радіус</label>
          <input
            id="morph-radius"
            type="number"
            class="w-28 bg-blue-200 px-2 py-1 rounded-xs"
            min={0}
            step={1}
            value={getSettings().radius}
            on:input={(event) => {
              if (Number.isNaN(event.target.valueAsNumber)) return;
              setSettings((prev) => ({
                ...prev,
                radius: event.target.valueAsNumber,
              }));
            }}
          />
        </div>
        <div class="flex gap-x-2 justify-between">
          <label for="accelerator-type">Тип акселератора</label>
          <select
            id="accelerator-type"
            value={getSettings().accelerator}
            class="bg-blue-200 px-2 py-1 rounded-xs w-28 cursor-pointer hover:bg-blue-300 duration-150"
            on:input={(event) =>
              setSettings((prev) => ({
                ...prev,
                accelerator: event.target.value as AcceleratorType,
              }))
            }
          >
            <option value={"cpu"}>CPU</option>
            <option value={"cl"}>OpenCL</option>
            <option value={"cuda"}>CUDA</option>
          </select>
        </div>
        <div class="flex gap-x-2 justify-between">
          <label for="morph-operation">Операція</label>
          <select
            id="morph-operation"
            value={getSettings().operation}
            class="bg-blue-200 px-2 py-1 rounded-xs w-28 cursor-pointer hover:bg-blue-300 duration-150"
            on:input={(event) =>
              setSettings((prev) => ({
                ...prev,
                operation: event.target.value as OperationType,
              }))
            }
          >
            <option value={"dilation"}>Дилатація</option>
            <option value={"erosion"}>Ерозія</option>
          </select>
        </div>
        <div class="italic flex flex-row">
          <BsInfoCircleFill
            color="var(--color-blue-400)"
            class="mt-2 mr-2 inline-block"
          />
          <p>
            Вкажіть радіус згортки, тип акселератора (<strong>CPU</strong>,{" "}
            <strong>OpenCL</strong> чи <strong>CUDA</strong>)
            <br />
            та операцію (<strong>дилатація</strong> чи <strong>ерозія</strong>).
            <br />
            <br />
            Для запуску експерименту в полі вводу на кнопці
            <br />
            праворуч вкажіть кількість ітерацій.
            <br />
            <br />
            Після закінчення есперименту файл автоматично
            <br />
            завантажиться на ваш пристрій у форматі <strong>JSON.</strong>
          </p>
        </div>
      </div>
      <div class="flex flex-row gap-0">
        <button
          on:click={onRunClick}
          class="bg-amber-400 hover:bg-amber-500 duration-150 rounded-tl-md rounded-bl-md p-2 block text-lg font-bold cursor-pointer grow border-r border-black"
        >
          Запустити
        </button>
        <button
          on:click={(event) => {
            if (event.target !== event.currentTarget) return;
            onRunExperiment();
          }}
          class="bg-amber-400 hover:bg-amber-500 duration-150 rounded-tr-md rounded-br-md p-2 block text-lg font-bold cursor-pointer"
        >
          <span class="pointer-events-none">x</span>
          <input
            class="bg-[rgba(0,0,0,0.18)] w-16 ml-1 pl-1"
            type="number"
            step={1}
            value={10}
            ref={experimentInput}
          />
        </button>
      </div>
      <Show when={getResult()}>
        <Process2DResult
          benchmark={getResult()!.benchmarkResult}
          imageSrc={getResult()!.data}
        />
      </Show>
      <Show when={experimentIsRunning()}>
        <div class="flex flex-col items-center bg-[rgba(255,255,255,0.18)] rounded-md">
          <span class="text-xl font-bold">Експеримент триває...</span>
          <img src="/loaders/pulse.svg" class="mt-2 w-40 h-40" />
        </div>
      </Show>
    </>
  );
}
