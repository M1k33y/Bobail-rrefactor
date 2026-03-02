import MainLayout from "../layout/MainLayout";
import { createBrowserRouter } from "react-router-dom";
import HomePage from "../pages/HomePage";
import RulesPage from "../pages/RulesPage";
import GamePage from "../features/game/pages/GamePage";
import LocalGameStartPage from "../features/game/pages/LocalGameStartPage";
import SettingsPage from "../pages/SettingsPage";
export const router = createBrowserRouter([
  {
  element: <MainLayout />,
  children: [
    { path: "/", element: <HomePage /> },
    { path: "/rules", element: <RulesPage /> },

    { path: "/play/local", element: <LocalGameStartPage /> },
    
    { path: "/play/local/:gameId", element: <GamePage /> },

    { path: "/play/online", element: <div>Online Coming Soon</div> },
    { path: "/play/bot", element: <div>Bot Coming Soon</div> },
    { path: "/settings", element: <SettingsPage /> }
  ]
}
]);