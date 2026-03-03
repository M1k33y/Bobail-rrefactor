import { useState, useEffect } from "react";
import { gameApi } from "../services/gameApi";

export function useGame(gameId) {
  const [game, setGame] = useState(null);
  const [selected, setSelected] = useState(null);
  const [validMoves, setValidMoves] = useState([]);


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

}, [game]);

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

    const isBotGame = game.mode === "PlayerVsBot";
    const isBotTurn =
      isBotGame && game.botColor === game.currentTurn;

   
    if (isBotTurn) return;

    const piece = game.pieces.find(
      p => p.row === row && p.column === col
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

    try {
      if (game.currentPhase === "BobailMoveRequired") {
        await gameApi.bobailMove(gameId, {
          toRow: row,
          toColumn: col
        });
      } else {
        await gameApi.playerMove(gameId, {
          fromRow: selected.row,
          fromColumn: selected.col,
          toRow: row,
          toColumn: col
        });
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