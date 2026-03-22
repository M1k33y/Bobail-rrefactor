import { useState, useEffect, useCallback } from "react";
import { gameApi } from "../api/gameApi";

export function useGame(gameId) {
  const [game, setGame] = useState(null);
  const [selected, setSelected] = useState(null);
  const [validMoves, setValidMoves] = useState([]);

 
  const loadGame = useCallback(async () => {
    if (!gameId) return;

    try {
      const data = await gameApi.get(gameId);
      setGame(data);
    } catch (err) {
      console.error("Load game error:", err);
    }
  }, [gameId]);


  useEffect(() => {
    if (!gameId) return;
    loadGame();
  }, [gameId, loadGame]);


  useEffect(() => {
    if (!game) return;

    const isThinking =
      game.mode === "PlayerVsBot" &&
      game.botColor === game.currentTurn &&
      game.status === "InProgress";

    if (!isThinking) return;

    const interval = setInterval(() => {
      loadGame();
    }, 500);

    return () => clearInterval(interval);
  }, [
    game?.mode,
    game?.botColor,
    game?.currentTurn,
    game?.status,
    loadGame,
  ]);


  async function handleCellClick(row, col) {
    if (!game || game.status !== "InProgress") return;

    const isBotTurn =
      game.mode === "PlayerVsBot" &&
      game.botColor === game.currentTurn;

    if (isBotTurn) return;

    const piece = game.pieces.find(
      (p) => p.row === row && p.column === col
    );


    if (!selected) {
      if (!piece) return;

      if (
        game.currentPhase === "PlayerMoveRequired" &&
        (piece.type !== "PlayerPiece" ||
          piece.owner !== game.currentTurn)
      )
        return;

      if (
        game.currentPhase === "BobailMoveRequired" &&
        piece.type !== "Bobail"
      )
        return;

      let moves = [];

      try {
        if (game.currentPhase === "PlayerMoveRequired") {
          moves = await gameApi.getValidPlayerMoves(gameId, row, col);
        }

        if (game.currentPhase === "BobailMoveRequired") {
          moves = await gameApi.getValidBobailMoves(gameId);
        }
      } catch (err) {
        console.error("Get moves error:", err);
        return;
      }

      if (!moves || moves.length === 0) return;

      setSelected({ row, col });
      setValidMoves(moves);
      return;
    }

    try {
      if (game.currentPhase === "BobailMoveRequired") {
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


      setSelected(null);
      setValidMoves([]);

      await new Promise((r) => setTimeout(r, 50));

      await loadGame();
    } catch (err) {
      console.error("Move error:", err);
      setSelected(null);
      setValidMoves([]);
    }
  }

  return {
    game,
    selected,
    validMoves,
    handleCellClick,
  };
}