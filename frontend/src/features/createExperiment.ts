import { Accessor, createEffect, createSignal } from "solid-js";
import { Experiment } from "../types";
import { BenchmarkResult } from "../axios/types";

export default function createExperiment<T>(getInputData: Accessor<T>, name: string) {
  const [experimentData, setExperimentData] = createSignal<Experiment<T>>({
    name,
    time: Date.now(),
    inputData: getInputData(),
    results: [],
  });

  const addResult = (benchmarkResult: BenchmarkResult) => {
    setExperimentData((prev) => {
      const newResults = [...prev.results];
      newResults.push(benchmarkResult);

      return { ...prev, results: newResults };
    });
  }

  const clearResult = () => {
    setExperimentData((prev) => {
      return { ...prev, results: [] };
    });
  }

  createEffect(() => {
    const inputData = getInputData();
    setExperimentData((prev) => {
      return { ...prev, inputData };
    });
  })

  const downloadJSON = (fileName: string) => {
    const a = document.createElement("a");
    const jsonData = JSON.stringify(experimentData(), null, 2);
    const blob = new Blob([jsonData], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    a.download = `${fileName}.json`;
    a.href = url;
    a.click();

    URL.revokeObjectURL(url);
  }

  return { experimentData, addResult, downloadJSON, clearResult };
}