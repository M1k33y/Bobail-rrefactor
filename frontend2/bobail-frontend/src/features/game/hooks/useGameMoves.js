import { useCallback, useState } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { gameApi } from "../api/gameApi";

export function useGameMoves({
  gameId,
  game,
  connectionRef,
  setOnlineError,
  loadGame,
}) {
  const [selected, setSelected] = useState(null);
  const [validMoves, setValidMoves] = useState([]);

  const clearSelection = useCallback(() => {
    setSelected(null);
    setValidMoves([]);
  }, []);

  const canSelectPiece = useCallback((piece) => {
    if (!piece || !game) return false;

    if (game.currentPhase === "PlayerMoveRequired") {
      return piece.type === "PlayerPiece" && piece.owner === game.currentTurn;
    }

    if (game.currentPhase === "BobailMoveRequired") {
      return piece.type === "Bobail";
    }

    return false;
  }, [game]);

  const selectPiece = useCallback(async (row, col) => {
    if (!game) return;

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
      clearSelection();
      return;
    }

    if (!moves || moves.length === 0) {
      clearSelection();
      return;
    }

    setSelected({ row, col });
    setValidMoves(moves);
  }, [clearSelection, game, gameId]);

  const handleCellClick = useCallback(async (row, col) => {
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

    const isSelectedCell =
      selected?.row === row && selected?.col === col;

    const isValidDestination = validMoves.some(
      (move) => move.row === row && move.column === col
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
        await new Promise((resolve) => setTimeout(resolve, 50));
        await loadGame();
      }
    } catch (err) {
      console.error("Move error:", err);
      setOnlineError(err.message || "Move failed.");
      clearSelection();
    }
  }, [
    canSelectPiece,
    clearSelection,
    connectionRef,
    game,
    gameId,
    loadGame,
    selectPiece,
    selected,
    setOnlineError,
    validMoves,
  ]);

  const handleResign = useCallback(async () => {
    if (!game || game.status !== "InProgress") return;

    const isOnlinePlayerTurn =
      game.mode === "OnlineMultiplayer" &&
      game.playerColor &&
      game.playerColor === game.currentTurn;

    if (!isOnlinePlayerTurn) return;

    try {
      const connection = connectionRef.current;

      if (!connection || connection.state !== HubConnectionState.Connected) {
        setOnlineError("Realtime connection is not ready.");
        clearSelection();
        return;
      }

      await connection.invoke("ResignGame", gameId);
      clearSelection();
      setOnlineError("");
    } catch (err) {
      console.error("Resign error:", err);
      setOnlineError(err.message || "Resign failed.");
      clearSelection();
    }
  }, [
    clearSelection,
    connectionRef,
    game,
    gameId,
    setOnlineError,
  ]);

  return {
    selected,
    validMoves,
    clearSelection,
    handleCellClick,
    handleResign,
  };
}
