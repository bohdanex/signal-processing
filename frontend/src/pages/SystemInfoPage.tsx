import { createResource, For, Show, Suspense } from "solid-js";
import { CPUInfo, GPUInfo, RamInfo, SystemDataInfo } from "../types";
import axiosInstance from "../axios/axiosInstance";

const fetchOsInfo = async (): Promise<SystemDataInfo> => {
  const response = await axiosInstance.get("os/info");
  return response.data;
};

const createCPUData = (cpuInfo?: CPUInfo) => {
  return [
    { value: cpuInfo?.name || "Завантаження..." },
    {
      key: "Фізичних процесорів",
      value: cpuInfo?.physicalCores || "Завантаження...",
    },
    {
      key: "Логічних процесорів",
      value: cpuInfo?.logicalCores || "Завантаження...",
    },
    {
      key: "Частота",
      value: cpuInfo?.maxClockSpeed || "Завантаження...",
    },
  ];
};

const createGPUData = (gpuInfo?: GPUInfo) => {
  return [
    { value: gpuInfo?.name || "Завантаження..." },
    {
      key: "Відеопам'ять",
      value: gpuInfo?.memory || "Завантаження...",
    },
    {
      key: "Версія драйвера",
      value: gpuInfo?.driverVersion || "Завантаження...",
    },
  ];
};

const createRamData = (ramInfo?: RamInfo) => {
  return [
    {
      key: "Виробник",
      value: ramInfo?.manufacturer || "Завантаження...",
    },
    {
      key: "Номер",
      value: ramInfo?.partNumber || "Завантаження...",
    },
    {
      key: "Об'єм",
      value: ramInfo?.memorySize + "Mb" || "Завантаження...",
    },
    {
      key: "Кількість",
      value: ramInfo?.count || "Завантаження...",
    },
    {
      key: "Сумарна пам'ять",
      value: ramInfo?.totalMemorySize + "Mb" || "Завантаження...",
    },
  ];
};

export default function SystemInfoPage() {
  const [info] = createResource(fetchOsInfo);

  return (
    <div class="flex flex-col gap-y-3 mx-4 mt-2 grow">
      <h1 class="font-bold text-2xl">Системні характеристики</h1>
      <Show
        when={info()}
        fallback={
          <Block
            iconPath="/loaders/cog.svg"
            title="ЦП"
            data={createCPUData(undefined)}
          />
        }
      >
        <Block
          iconPath="/hardware/cpu.png"
          title="ЦП"
          data={createCPUData(info()!.cpu)}
        />
      </Show>
      <Show
        when={info()}
        fallback={
          <Block
            iconPath="/loaders/cog.svg"
            title="Відеокарта"
            data={createGPUData(undefined)}
          />
        }
      >
        <For each={info()!.gpus}>
          {(gpu) => (
            <Block
              iconPath="/hardware/gpu.png"
              title="Відеокарта"
              data={createGPUData(gpu)}
            />
          )}
        </For>
      </Show>
      <Show
        when={info()}
        fallback={
          <Block
            iconPath="/loaders/cog.svg"
            title="Оперативна пам'ять"
            data={createRamData(undefined)}
          />
        }
      >
        <For each={info()!.rams}>
          {(ram) => (
            <Block
              iconPath="/hardware/ram.png"
              title="Оперативна пам'ять"
              data={createRamData(ram)}
            />
          )}
        </For>
      </Show>
    </div>
  );
}

interface BlockData {
  iconPath: string;
  title: string;
  data: Array<{ key?: string; value: string | number }>;
}

export function Block({ iconPath, data, title }: BlockData) {
  if (!data || data.length === 0) return null;

  return (
    <div class="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
      <table class="min-w-full border-collapse text-left text-sm">
        <thead>
          <tr>
            <th class="w-40"></th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <For each={data}>
            {(row, index) => (
              <tr class="border-b border-gray-100 last:border-0 hover:bg-gray-50 transition-colors">
                {index() === 0 && (
                  <td
                    rowSpan={data.length}
                    class="bg-blue-50 p-4 text-center align-middle"
                  >
                    <div class="flex h-full items-center justify-center gap-y-1 flex-col">
                      <img
                        src={iconPath}
                        alt="Icon"
                        class="h-12 w-12 object-contain opacity-80"
                      />
                      <span class="text-xl">{title}</span>
                    </div>
                  </td>
                )}
                {row.key && (
                  <td class="whitespace-nowrap py-3 pl-4 pr-2 font-medium text-gray-500 w-1/3">
                    {row.key}
                  </td>
                )}
                <td
                  class={`py-3 pl-2 pr-4 font-semibold text-gray-800 ${
                    !row.key ? "pl-4" : ""
                  }`}
                  colSpan={!row.key ? 2 : 1}
                >
                  {row.value}
                </td>
              </tr>
            )}
          </For>
        </tbody>
      </table>
    </div>
  );
}
