import { createSignal } from "solid-js";

export interface LowHighPassFilterData {
  qFactor: number;
  cutOffFreq: number;
}
export const [getLowHighPassFilterSettings, setLowHighPassFilterSettings] =
  createSignal<LowHighPassFilterData>({
    qFactor: 0.2,
    cutOffFreq: 1000,
  });

export default function LowHighPassConfigs(props: { title: string }) {
  return (
    <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
      <h3 class="text-lg font-bold">{props.title}</h3>
      <div class="flex gap-x-2 justify-between">
        <label for="filter-low-q-factor">Фактор q</label>
        <input
          id="filter-low-q-factor"
          type="number"
          class="w-20 bg-blue-200 px-2 py-1 rounded-xs"
          min={0}
          step={0.1}
          value={getLowHighPassFilterSettings().qFactor}
          on:input={(event) => {
            if (Number.isNaN(event.target.valueAsNumber)) return;
            setLowHighPassFilterSettings((prev) => ({
              ...prev,
              qFactor: event.target.valueAsNumber,
            }));
          }}
        />
      </div>
      <div class="flex gap-x-2 justify-between">
        <label for="filter-low-cut-off">Частота (cut-off)</label>
        <input
          id="filter-low-cut-off"
          type="number"
          class="w-20 bg-blue-200 px-2 py-1 rounded-xs"
          min={0}
          value={getLowHighPassFilterSettings().cutOffFreq}
          on:input={(event) => {
            if (Number.isNaN(event.target.valueAsNumber)) return;
            setLowHighPassFilterSettings((prev) => ({
              ...prev,
              cutOffFreq: event.target.valueAsNumber,
            }));
          }}
        />
      </div>
    </div>
  );
}
