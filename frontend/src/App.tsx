import { createResource, Show, Suspense, type Component } from "solid-js";
import SystemInfo from "./components/SystemInfo";
import axiosInstance from "./axios/axiosInstance";
import { SystemDataInfo } from "./types";

const App: Component = () => {
  return (
    <>
      <SystemInfo />
    </>
  );
};

export default App;
