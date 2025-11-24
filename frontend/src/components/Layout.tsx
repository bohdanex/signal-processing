import { A } from "@solidjs/router";
import { TbDeviceDesktopAnalytics } from "solid-icons/tb";
import { createSignal, ParentProps, Show } from "solid-js";
import SystemMonitor from "./systemMonitor";

export default function Layout(props: ParentProps) {
  const [getShowSidebar, setShowSidebar] = createSignal(false);
  createSignal(false);

  return (
    <div class="flex flex-row gap-x-2">
      <div
        on:click={() => setShowSidebar(true)}
        class="p-2 absolute top-2 right-2 opacity-25 bg-white rounded-md hover:bg-blue-200 hover:opacity-100 duration-150 cursor-pointer"
      >
        <TbDeviceDesktopAnalytics size={32} />
      </div>
      <ul class="flex flex-col w-50 text-center">
        <TabItem name="Характеристики системи" route="/sysinfo" />
        <TabItem name="Підтримка технологій" route="/can-i-use" />
        <TabItem name="Обробка аудіо" route="/1d" />
        <TabItem name="Обробка зображень" route="/2d" />
      </ul>
      {props.children}
      <div
        on:mouseover={() => setShowSidebar(true)}
        on:mouseleave={() => setShowSidebar(false)}
        class="absolute right-0 duration-200 max-h-full overflow-y-scroll"
        style={{
          transform: `translate(${getShowSidebar() ? 0 : "100%"}, 0)`,
        }}
      >
        <SystemMonitor />
      </div>
    </div>
  );
}

interface TabItemProps {
  name: string;
  imagePath?: string;
  route: string;
}

function TabItem(props: TabItemProps) {
  const { name, imagePath, route } = props;

  return (
    <A
      class="px-6 py-3 flex flex-row gap-x-2 bg-blue-50 border-b border-gray-400 last:border-0 hover:bg-blue-200 duration-150 last:rounded-br-2xl active:hover:bg-blue-300"
      href={route}
    >
      <Show when={imagePath}>
        <img alt={`tab item ${name}`} src={imagePath} />
      </Show>
      <span class="text-xl">{name}</span>
    </A>
  );
}
