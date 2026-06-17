import { useEffect, useState } from "react";
import { createGameHubConnection } from "../api/gameHub";
import { clearStoredToken } from "../../auth/utils/authStorage";

export function useGameRealtime({
  gameId,
  gameMode,
  applyGameState,
  clearSelection,
  connectionRef,
  setOnlineError,
}) {
  const [isRealtimeConnected, setIsRealtimeConnected] = useState(false);

  useEffect(() => {
    if (!gameId || gameMode !== "OnlineMultiplayer") return;

    let disposed = false;
    const connection = createGameHubConnection();
    connectionRef.current = connection;

    const applyRemoteState = (nextGame) => {
      applyGameState(nextGame);
      clearSelection();
    };

    connection.on("GameState", applyRemoteState);
    connection.on("PlayerJoined", applyRemoteState);
    connection.on("MovePlayed", (result) => applyRemoteState(result.game));
    connection.on("GameEnded", applyRemoteState);
    connection.on("ForceLogout", () => {
      clearStoredToken();
      window.location.assign("/login");
    });
    connection.on("MoveRejected", (error) => {
      setOnlineError(error?.message || "Move rejected.");
      clearSelection();
    });
    connection.on("ResignRejected", (error) => {
      setOnlineError(error?.message || "Unable to resign game.");
      clearSelection();
    });
    connection.on("JoinRejected", (error) => {
      setOnlineError(error?.message || "Unable to join game.");
      clearSelection();
    });
    connection.onreconnecting(() => setIsRealtimeConnected(false));
    connection.onclose(() => setIsRealtimeConnected(false));
    connection.onreconnected(async () => {
      if (disposed) return;

      try {
        setIsRealtimeConnected(true);
        await connection.invoke("JoinGame", gameId);
      } catch (err) {
        console.error("SignalR reconnect error:", err);
        setOnlineError("Realtime reconnection failed.");
      }
    });

    async function startConnection() {
      try {
        await connection.start();

        if (disposed) return;

        setIsRealtimeConnected(true);
        await connection.invoke("JoinGame", gameId);
      } catch (err) {
        console.error("SignalR connection error:", err);
        setIsRealtimeConnected(false);
        setOnlineError("Realtime connection failed.");
      }
    }

    startConnection();

    return () => {
      disposed = true;
      connectionRef.current = null;
      setIsRealtimeConnected(false);
      connection.stop().catch((err) => {
        console.error("SignalR stop error:", err);
      });
    };
  }, [
    gameId,
    gameMode,
    applyGameState,
    clearSelection,
    connectionRef,
    setOnlineError,
  ]);

  return {
    isRealtimeConnected,
  };
}
