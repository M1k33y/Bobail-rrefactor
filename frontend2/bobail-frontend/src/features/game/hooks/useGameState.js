import { useCallback, useEffect, useState } from "react";
import { gameApi } from "../api/gameApi";

function mergeGameState(nextGame, previousGame) {
  if (!nextGame) {
    return previousGame;
  }

  const clock = nextGame.clock
    ? {
        ...nextGame.clock,
        receivedAtMs: Date.now(),
      }
    : nextGame.clock;

  return {
    ...nextGame,
    clock,
    playerColor: nextGame.playerColor || previousGame?.playerColor || null,
  };
}

export function useGameState(gameId) {
  const [game, setGame] = useState(null);
  const gameMode = game?.mode;
  const botColor = game?.botColor;
  const currentTurn = game?.currentTurn;
  const gameStatus = game?.status;

  const applyGameState = useCallback((nextGame) => {
    setGame((current) => mergeGameState(nextGame, current));
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

  return {
    game,
    applyGameState,
    loadGame,
  };
}
