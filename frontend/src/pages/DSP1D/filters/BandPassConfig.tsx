import { createSignal } from "solid-js";

export interface BandPassFilterData {
  lowFrequency: number;
  highFrequency: number;
  order: number;
}
export const [getBandPassFilterSettings, setBandPassFilterSettings] =
  createSignal<BandPassFilterData>({
    lowFrequency: 800,
    highFrequency: 5000,
    order: 4,
  });

export default function BandPassConfigs() {
  return (
    <div class="flex flex-col gap-y-2 bg-blue-50 p-4 shadow-2xs rounded-lg">
      <h3 class="text-lg font-bold">Band-Pass Фільтр</h3>
      <div class="flex gap-x-2 justify-between">
        <label for="filter-low">Низька частота</label>
        <input
          id="filter-low"
          type="number"
          class="w-30 bg-blue-200 px-2 py-1 rounded-xs"
          min={0}
          step={0.1}
          value={getBandPassFilterSettings().lowFrequency}
          on:input={(event) => {
            if (Number.isNaN(event.target.valueAsNumber)) return;
            setBandPassFilterSettings((prev) => ({
              ...prev,
              lowFrequency: event.target.valueAsNumber,
            }));
          }}
        />
      </div>
      <div class="flex gap-x-2 justify-between">
        <label for="filter-high-freq">Висока частота</label>
        <input
          id="filter-high-freq"
          type="number"
          class="w-30 bg-blue-200 px-2 py-1 rounded-xs"
          min={0}
          step={0.1}
          value={getBandPassFilterSettings().highFrequency}
          on:input={(event) => {
            if (Number.isNaN(event.target.valueAsNumber)) return;
            setBandPassFilterSettings((prev) => ({
              ...prev,
              highFrequency: event.target.valueAsNumber,
            }));
          }}
        />
      </div>
      <div class="flex gap-x-2 justify-between">
        <label for="filter-high-freq">Порядок</label>
        <input
          id="filter-high-freq"
          type="number"
          class="w-20 bg-blue-200 px-2 py-1 rounded-xs"
          min={0}
          step={1}
          value={getBandPassFilterSettings().order}
          on:input={(event) => {
            if (Number.isNaN(event.target.valueAsNumber)) return;
            setBandPassFilterSettings((prev) => ({
              ...prev,
              order: event.target.valueAsNumber,
            }));
          }}
        />
      </div>
    </div>
  );
}
