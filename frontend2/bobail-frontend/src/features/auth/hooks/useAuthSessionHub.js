import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { createAuthHubConnection } from "../api/authHub";
import {
  AUTH_SESSION_CHANGED,
  clearStoredToken,
  getStoredToken,
} from "../utils/authStorage";

export function useAuthSessionHub() {
  const navigate = useNavigate();

  useEffect(() => {
    let disposed = false;
    let connection = null;

    const stopConnection = async () => {
      if (!connection) return;

      const currentConnection = connection;
      connection = null;

      try {
        await currentConnection.stop();
      } catch {
        // Connection may already be closing.
      }
    };

    const startConnection = async () => {
      await stopConnection();

      if (disposed || !getStoredToken()) {
        return;
      }

      const nextConnection = createAuthHubConnection();
      connection = nextConnection;

      nextConnection.on("ForceLogout", async () => {
        clearStoredToken();
        await stopConnection();
        navigate("/login", { replace: true });
      });

      try {
        await nextConnection.start();
      } catch {
        if (!disposed) {
          await stopConnection();
        }
      }
    };

    const handleAuthChanged = () => {
      void startConnection();
    };

    window.addEventListener(AUTH_SESSION_CHANGED, handleAuthChanged);
    void startConnection();

    return () => {
      disposed = true;
      window.removeEventListener(AUTH_SESSION_CHANGED, handleAuthChanged);
      void stopConnection();
    };
  }, [navigate]);
}
