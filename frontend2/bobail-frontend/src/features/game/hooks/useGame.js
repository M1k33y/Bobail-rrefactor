import { useRef, useState } from "react";
import { useGameMoves } from "./useGameMoves";
import { useGameRealtime } from "./useGameRealtime";
import { useGameState } from "./useGameState";

export function useGame(gameId) {
  const connectionRef = useRef(null);
  const [onlineError, setOnlineError] = useState("");

  const {
    game,
    applyGameState,
    loadGame,
  } = useGameState(gameId);

  const {
    selected,
    validMoves,
    clearSelection,
    handleCellClick,
    handleResign,
  } = useGameMoves({
    gameId,
    game,
    connectionRef,
    setOnlineError,
    loadGame,
  });

  const {
    isRealtimeConnected,
  } = useGameRealtime({
    gameId,
    gameMode: game?.mode,
    applyGameState,
    clearSelection,
    connectionRef,
    setOnlineError,
  });

  return {
    game,
    selected,
    validMoves,
    onlineError,
    isRealtimeConnected,
    handleCellClick,
    handleResign,
  };
}
