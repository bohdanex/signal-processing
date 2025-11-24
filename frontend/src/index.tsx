/* @refresh reload */
import { render } from "solid-js/web";
import "solid-devtools";
import { Route, Router } from "@solidjs/router";
import SystemInfoPage from "./pages/SystemInfoPage";
import Layout from "./components/Layout";
import Can_I_UsePage from "./pages/Can_I_UsePage";
import DSP1D from "./pages/DSP1D";
import DSP2D_Page from "./pages/DSP2D";

const root = document.getElementById("root");

if (import.meta.env.DEV && !(root instanceof HTMLElement)) {
  throw new Error(
    "Root element not found. Did you forget to add it to your index.html? Or maybe the id attribute got misspelled?"
  );
}

render(
  () => (
    <Router root={Layout}>
      <Route path={"/"} component={SystemInfoPage} />
      <Route path={"/sysinfo"} component={SystemInfoPage} />
      <Route path={"/can-i-use"} component={Can_I_UsePage} />
      <Route path={"/1d"} component={DSP1D} />
      <Route path={"/2d"} component={DSP2D_Page} />
    </Router>
  ),
  root!
);
