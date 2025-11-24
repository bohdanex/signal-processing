import * as grafar from "grafar";
import { onMount } from "solid-js";

export default function StftChart3D(props: {
  sampleRate: number;
  windowSize: number;
  hopSize: number;
  signalDuration: number;
  spectrum: number[][];
  songDuration: number;
}) {
  let container: HTMLDivElement | undefined;
  onMount(() => {
    if (!container) return;

    const panel = grafar.panel(container);
    const fqBinSize = 512;
    const xAxis = grafar.range(0, fqBinSize - 1, fqBinSize).select();
    const yAxis = grafar
      .range(0, props.spectrum.length - 1, props.spectrum.length)
      .select();
    const z = grafar.map([xAxis, yAxis], (x: number, y: number) => {
      return (props.spectrum[Math.round(y)][Math.round(x)] * 20050) / 100;
    });

    grafar.pin([xAxis, yAxis, z], panel);
  });

  return <div class="w-[344px] h-[344px]" ref={container}></div>;
}
