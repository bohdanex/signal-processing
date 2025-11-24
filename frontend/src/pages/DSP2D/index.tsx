import { createSignal, For, JSX, Show } from "solid-js";

const [selectedImageFile, setSelectedImageFile] = createSignal<File>();
const [selectedImageURL, setSelectedImageURL] = createSignal<string | null>();

interface Tab {
  name: string;
  element: JSX.Element;
}

export default function DSP2D_Page() {
  let inputRef: HTMLInputElement | undefined;
  const [getTabIndex, setTabIndex] = createSignal(0);

  const tabs: Tab[] = [
    {
      name: "ШПФ",
      element: <></>,
    },
    {
      name: "Фільтри",
      element: <></>,
    },
    {
      name: "Морфологічні операції",
      element: <></>,
    },
  ];

  return (
    <div class="mt-2 flex flex-col gap-y-2">
      <div class="flex flex-col gap-y-2 p-3 border-gray-900 border-2 border-dashed h-min rounded-md">
        <label
          for="audio-file-picker"
          class="bg-blue-50 border border-gray-400 hover:bg-blue-200 duration-150 rounded-md p-4 block text-xl cursor-pointer"
        >
          Виберіть файл
        </label>
        <input
          ref={inputRef}
          id="audio-file-picker"
          type="file"
          accept="image/*"
          class="hidden"
          on:input={(event) => {
            const file = event.target.files?.[0];

            if (file) {
              setSelectedImageFile(file);
              const existingURL = selectedImageURL();
              if (existingURL) {
                URL.revokeObjectURL(existingURL);
                setSelectedImageURL(null);
              }
              setSelectedImageURL(URL.createObjectURL(file));
            }
          }}
        />
        <Show when={selectedImageURL()}>
          <img
            src={selectedImageURL()!}
            width={400}
            height={400}
            class="border-2 border-gray-800 outline-1 outline-gray-50 rounded-md cursor-pointer"
            on:click={() => inputRef?.click()}
          />
        </Show>
      </div>
      <Show when={selectedImageFile()}>
        <ul class="flex flex-row h-10 rounded-md overflow-hidden w-min">
          <For each={tabs}>
            {(tab, index) => (
              <li
                class="p-2 cursor-pointer hover:bg-gray-100 duration-150 not-last:border-r border-black min-w-20 text-center"
                classList={{
                  "bg-gray-50": getTabIndex() === index(),
                  "bg-gray-200": getTabIndex() !== index(),
                }}
                on:click={() => setTabIndex(index())}
              >
                {tab.name}
              </li>
            )}
          </For>
        </ul>
      </Show>
    </div>
  );
}
