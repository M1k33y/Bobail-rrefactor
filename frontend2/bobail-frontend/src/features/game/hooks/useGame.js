import { useState, useEffect, useCallback, useRef } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { gameApi } from "../api/gameApi";
import { createGameHubConnection } from "../api/gameHub";

function mergeGameState(nextGame, previousGame) {
  if (!nextGame) {
    return previousGame;
  }

  return {
    ...nextGame,
    playerColor: nextGame.playerColor || previousGame?.playerColor || null,
  };
}

export function useGame(gameId) {
  const [game, setGame] = useState(null);
  const [selected, setSelected] = useState(null);
  const [validMoves, setValidMoves] = useState([]);
  const [onlineError, setOnlineError] = useState("");
  const [isRealtimeConnected, setIsRealtimeConnected] = useState(false);
  const connectionRef = useRef(null);
  const gameMode = game?.mode;
  const botColor = game?.botColor;
  const currentTurn = game?.currentTurn;
  const gameStatus = game?.status;

  const applyGameState = useCallback((nextGame) => {
    setGame((current) => mergeGameState(nextGame, current));
  }, []);

  const clearSelection = useCallback(() => {
    setSelected(null);
    setValidMoves([]);
  }, []);

  const loadGame = useCallback(async () => {
    if (!gameId) return;

    try {
      const data = await gameApi.get(gameId);
      applyGameState(data);
    } catch (err) {
      console.error("Load game error:", err);
    }
  }, [gameId, applyGameState]);

  useEffect(() => {
    if (!gameId) return;

    let disposed = false;

    queueMicrotask(() => {
      if (!disposed) {
        loadGame();
      }
    });

    return () => {
      disposed = true;
    };
  }, [gameId, loadGame]);

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
    connection.on("MoveRejected", (error) => {
      setOnlineError(error?.message || "Move rejected.");
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
      connection.stop();
    };
  }, [gameId, gameMode, applyGameState, clearSelection]);

  useEffect(() => {
    if (!gameMode) return;

    const isThinking =
      gameMode === "PlayerVsBot" &&
      botColor === currentTurn &&
      gameStatus === "InProgress";

    if (!isThinking) return;

    const interval = setInterval(() => {
      loadGame();
    }, 500);

    return () => clearInterval(interval);
  }, [
    gameMode,
    botColor,
    currentTurn,
    gameStatus,
    loadGame,
  ]);

  async function handleCellClick(row, col) {
    if (!game || game.status !== "InProgress") return;

    const isBotTurn =
      game.mode === "PlayerVsBot" &&
      game.botColor === game.currentTurn;

    const isOnlineOpponentTurn =
      game.mode === "OnlineMultiplayer" &&
      game.playerColor &&
      game.playerColor !== game.currentTurn;

    if (isBotTurn || isOnlineOpponentTurn) return;

    const piece = game.pieces.find(
      (p) => p.row === row && p.column === col
    );

    if (!selected) {
      if (canSelectPiece(piece)) {
        await selectPiece(row, col);
      }
      return;
    }

    if (isSelectedCell) {
      clearSelection();
      return;
    }

    if (canSelectPiece(piece)) {
      await selectPiece(row, col);
      return;
    }

    if (!isValidDestination) {
      return;
    }

    try {
      const isOnlineGame = game.mode === "OnlineMultiplayer";

      if (isOnlineGame) {
        const connection = connectionRef.current;

        if (!connection || connection.state !== HubConnectionState.Connected) {
          setOnlineError("Realtime connection is not ready.");
          clearSelection();
          return;
        }

        if (game.currentPhase === "BobailMoveRequired") {
          await connection.invoke("MakeBobailMove", gameId, {
            toRow: row,
            toColumn: col,
          });
        } else {
          await connection.invoke("MakePlayerMove", gameId, {
            fromRow: selected.row,
            fromColumn: selected.col,
            toRow: row,
            toColumn: col,
          });
        }
      } else if (game.currentPhase === "BobailMoveRequired") {
        await gameApi.bobailMove(gameId, {
          toRow: row,
          toColumn: col,
        });
      } else {
        await gameApi.playerMove(gameId, {
          fromRow: selected.row,
          fromColumn: selected.col,
          toRow: row,
          toColumn: col,
        });
      }

      clearSelection();
      setOnlineError("");

      if (!isOnlineGame) {
        await new Promise((r) => setTimeout(r, 50));
        await loadGame();
      }
    } catch (err) {
      console.error("Move error:", err);
      setOnlineError(err.message || "Move failed.");
      clearSelection();
    }
  }

  return {
    game,
    selected,
    validMoves,
    onlineError,
    isRealtimeConnected,
    handleCellClick,
  };
}
