import { createSignal, onCleanup, Show } from "solid-js";
import STFT_Section from "./stft/STFT_Section";
import throttle from "lodash.throttle";

export const [getAudioTime, setAudioTime] = createSignal(0);

export default function DSP1D() {
  const [getSelectedFile, setSelectedFile] = createSignal<File | null>(null);
  const [getAudioFileURL, setAudioFileURL] = createSignal<string | null>(null);
  let audioInterval: NodeJS.Timeout | undefined;

  onCleanup(() => {
    const fileURL = getAudioFileURL();
    if (fileURL) {
      URL.revokeObjectURL(fileURL);
    }
  });

  const handleSeek = (event: Event) => {
    setAudioTime((event.target as HTMLAudioElement).currentTime);
  };

  const throttledSeekHandler = throttle(handleSeek, 16, {
    trailing: true,
    leading: true,
  });
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
                const existingURL = getAudioFileURL();
                if (existingURL) {
                  URL.revokeObjectURL(existingURL);
                  setAudioFileURL(null);
                }
                setAudioFileURL(URL.createObjectURL(file));
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
          <audio
            controls
            loop
            on:seeked={throttledSeekHandler}
            on:play={(event) => {
              audioInterval = setInterval(
                () =>
                  setAudioTime((event.target as HTMLAudioElement).currentTime),
                1 / 60
              );
            }}
            on:pause={() => clearInterval(audioInterval)}
          >
            <source src={getAudioFileURL()!} type="audio/wav" />
            Your browser does not support the audio element.
          </audio>
        </Show>
      </div>
      <Show when={getSelectedFile()}>
        {/* <FilterSection audioFile={getSelectedFile()!} /> */}
        <STFT_Section audioFile={getSelectedFile()!} />
      </Show>
    </div>
  );
}
