import Sidebar from "./Sidebar";
import { Outlet } from "react-router-dom";
import "../styles/MainLayout.css";
<Outlet />
function MainLayout() {
  return (
    <div className="app-layout">
      <Sidebar />
      <div className="main-content">
        <Outlet />
      </div>
    </div>
  );
}

export default MainLayout;