import Sidebar from "./Sidebar";
import { Outlet } from "react-router-dom";
import { useAuthSessionHub } from "../../features/auth/hooks/useAuthSessionHub";
import "../styles/MainLayout.css";

function MainLayout() {
  useAuthSessionHub();

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
