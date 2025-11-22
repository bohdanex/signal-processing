import { createResource, For, Show } from "solid-js";
import axiosInstance from "../axios/axiosInstance";
import { ComputeSupport } from "../types";

async function fetchData(): Promise<ComputeSupport[]> {
  const result = await axiosInstance.get("/os/parallel-compute-support");
  return result.data;
}

export default function Can_I_UsePage() {
  const [computeSupportData] = createResource(fetchData);

  return (
    <div class="flex flex-col gap-y-3 mt-2 grow">
      <h1 class="font-bold text-2xl">Перевірка підтримки технологій</h1>

      <div class="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        <table class="min-w-full border-collapse text-left text-sm">
          <thead>
            <tr>
              <th class="w-20"></th>
              <th class="w-30 px-4 text-center">Назва</th>
              <th class="px-4 text-center">Підтримує</th>
              <th class="text-left px-4">Версія</th>
              <th class="text-left px-4">Додатково</th>
            </tr>
          </thead>
          <tbody>
            <Show
              when={computeSupportData()}
              fallback={<img src="/loaders/cog.svg" />}
            >
              <For each={computeSupportData()}>
                {(item) => (
                  <tr class="border-b border-gray-100 last:border-0 hover:bg-gray-50 transition-colors">
                    <td class="px-4">
                      <img src={`/tech-icons/${item.name.toLowerCase()}.png`} />
                    </td>
                    <td class="text-center">{item.name}</td>
                    <td class="text-center">
                      {item.isSupported ? "✅" : "❌"}
                    </td>
                    <td class="text-left px-4">{item.version.length > 20 ? `${item.version.slice(0,20)}...`: item.version}</td>
                    <td class="text-left">{item.details}</td>
                  </tr>
                )}
              </For>
            </Show>
          </tbody>
        </table>
      </div>
    </div>
  );
}
