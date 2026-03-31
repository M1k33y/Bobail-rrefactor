import MainLayout from "../layout/pages/MainLayout";
import { createBrowserRouter } from "react-router-dom";
import HomePage from "../features/home/pages/HomePage";
import RulesPage from "../features/rules/pages/RulesPage";
import SettingsPage from "../features/settings/pages/SettingsPage";
import GamePage from "../features/game/pages/GamePage";
import LocalGameStartPage from "../features/game/pages/LocalGameStartPage";
import BotGameStartPage from "../features/game/pages/BotGameStartPage";
import NotFound from "./pages/NotFound";
import LoginPage from "../features/auth/pages/LoginPage";
import RegisterPage from "../features/auth/pages/RegisterPage";
import ProtectedRoute from "../routes/ProtectedRoute.jsx";
export const router = createBrowserRouter([
  {
  element: <MainLayout />,
  children: [
    { path: "/", element: <HomePage /> },
    { path: "/rules", element: <RulesPage /> },
    {
        path: "/play/local",
        element: (
          <ProtectedRoute>
            <LocalGameStartPage />
          </ProtectedRoute>
        )
      },
      {
        path: "/play/bot",
        element: (
          <ProtectedRoute>
            <BotGameStartPage />
          </ProtectedRoute>
        )
      },
      {
        path: "/play/local/:gameId",
        element: (
          <ProtectedRoute>
            <GamePage />
          </ProtectedRoute>
        )
      },
      {
        path: "/play/:gameId",
        element: (
          <ProtectedRoute>
            <GamePage />
          </ProtectedRoute>
        )
      },
    { path: "/play/online", element: <div>Online Coming Soon</div> },
    { path: "/settings", element: <SettingsPage /> },
    { path: "/login", element: <LoginPage /> },
    { path: "/register", element: <RegisterPage /> },
    { path: "*", element: <NotFound /> }
  ]
}
]);