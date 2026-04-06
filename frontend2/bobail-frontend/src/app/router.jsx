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
import ForgotPasswordPage from "../features/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "../features/auth/pages/ResetPasswordPage";
import VerifyEmailPage from "../features/auth/pages/VerifyEmailPage";
import ProtectedRoute from "../routes/ProtectedRoute.jsx";
import GameHistoryPage from "../features/gameHistory/pages/GameHistoryPage";
import GameReviewPage from "../features/gameHistory/pages/GameReviewPage";

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
        ),
      },
      {
        path: "/play/bot",
        element: (
          <ProtectedRoute>
            <BotGameStartPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/play/local/:gameId",
        element: (
          <ProtectedRoute>
            <GamePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/play/:gameId",
        element: (
          <ProtectedRoute>
            <GamePage />
          </ProtectedRoute>
        ),
      },
      { path: "/play/online", element: <div>Online Coming Soon</div> },
      {
        path: "/game-history",
        element: (
          <ProtectedRoute>
            <GameHistoryPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/game-history/:gameId/review",
        element: (
          <ProtectedRoute>
            <GameReviewPage />
          </ProtectedRoute>
        ),
      },
      { path: "/settings", element: <SettingsPage /> },
      { path: "/login", element: <LoginPage /> },
      { path: "/register", element: <RegisterPage /> },
      { path: "/forgot-password", element: <ForgotPasswordPage /> },
      { path: "/reset-password", element: <ResetPasswordPage /> },
      { path: "/verify-email", element: <VerifyEmailPage /> },
      { path: "*", element: <NotFound /> },
    ],
  },
]);
