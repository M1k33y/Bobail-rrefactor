import { useState, useEffect } from "react";
import { gameApi } from "../services/gameApi";

export function useGame(gameId) {
  const [game, setGame] = useState(null);
  const [selected, setSelected] = useState(null);
  const [validMoves, setValidMoves] = useState([]);

  useEffect(() => {
    if (!gameId) return;
    loadGame();
  }, [gameId]);

  async function loadGame() {
    const data = await gameApi.get(gameId);
    setGame(data);
  }

  async function handleCellClick(row, col) {
    if (!game || game.status !== "InProgress") return;

    const piece = game.pieces.find(
      p => p.row === row && p.column === col
    );

    const clickedType = piece?.type;

    // -------------------------
    // SELECT PHASE
    // -------------------------
    if (!selected) {
      if (!clickedType) return;

      if (
        game.currentPhase === "PlayerMoveRequired" &&
        clickedType !== game.currentTurn
      )
        return;

      if (
        game.currentPhase === "BobailMoveRequired" &&
        clickedType !== "Bobail"
      )
        return;

      let moves = [];

      if (game.currentPhase === "PlayerMoveRequired") {
        moves = await gameApi.getValidPlayerMoves(gameId, row, col);
      }

      if (game.currentPhase === "BobailMoveRequired") {
        moves = await gameApi.getValidBobailMoves(gameId);
      }

      if (!moves || moves.length === 0) return;

      setSelected({ row, col });
      setValidMoves(moves);
      return;
    }

    // -------------------------
    // RE-SELECTION
    // -------------------------
    if (
      clickedType &&
      clickedType === game.currentTurn &&
      game.currentPhase === "PlayerMoveRequired"
    ) {
      const moves = await gameApi.getValidPlayerMoves(gameId, row, col);
      setSelected({ row, col });
      setValidMoves(moves);
      return;
    }

    // -------------------------
    // MOVE EXECUTION
    // -------------------------
    try {
      if (game.currentPhase === "BobailMoveRequired") {
        const response = await gameApi.bobailMove(gameId, {
          toRow: row,
          toColumn: col
        });

        if (!response.ok) {
          setSelected(null);
          setValidMoves([]);
          return;
        }
      } else {
        const response = await gameApi.playerMove(gameId, {
          fromRow: selected.row,
          fromColumn: selected.col,
          toRow: row,
          toColumn: col
        });

        if (!response.ok) {
          setSelected(null);
          setValidMoves([]);
          return;
        }
      }

      setSelected(null);
      setValidMoves([]);
      await loadGame();
    } catch (err) {
      console.error(err);
      setSelected(null);
      setValidMoves([]);
    }
  }

  return {
    game,
    selected,
    validMoves,
    handleCellClick
  };
}

