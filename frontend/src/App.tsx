import { createResource, Show, Suspense, type Component } from "solid-js";
import SystemData from "./components/SystemData";
import axiosInstance from "./axios/axiosInstance";
import { SystemDataInfo } from "./types";

const App: Component = () => {
  return (
    <>
      <SystemData />
    </>
  );
};

export default App;
