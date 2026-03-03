import MainLayout from "../layout/pages/MainLayout";
import { createBrowserRouter } from "react-router-dom";
import HomePage from "../features/home/pages/HomePage";
import RulesPage from "../features/rules/pages/RulesPage";
import SettingsPage from "../features/settings/pages/SettingsPage";
import GamePage from "../features/game/pages/GamePage";
import LocalGameStartPage from "../features/game/pages/LocalGameStartPage";
import BotGameStartPage from "../features/game/pages/BotGameStartPage";
import NotFound from "./pages/NotFound";
export const router = createBrowserRouter([
  {
  element: <MainLayout />,
  children: [
    { path: "/", element: <HomePage /> },
    { path: "/rules", element: <RulesPage /> },
    { path: "/play/local", element: <LocalGameStartPage /> },
    { path: "/play/bot", element: <BotGameStartPage />},
    { path: "/play/local/:gameId", element: <GamePage /> },
    { path: "/play/:gameId", element: <GamePage /> },
    { path: "/play/online", element: <div>Online Coming Soon</div> },
    { path: "/settings", element: <SettingsPage /> },
    { path: "*", element: <NotFound /> }
  ]
}
]);