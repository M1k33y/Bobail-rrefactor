import { useNavigate, useParams } from "react-router-dom";
import { useGame } from "../hooks/useGame";
import Board from "../components/Board";
import WinnerModal from "../components/WinnerModal";
import { useState, useEffect } from "react";
import "../../../styles/GamePage.css";
function GamePage() {
  const navigate = useNavigate();
  const { gameId } = useParams();
  

  const {
    game,
    selected,
    validMoves,
    handleCellClick
  } = useGame(gameId);

  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    if (game?.status === "Finished") {
      setShowModal(true);
    }
  }, [game]);

  if (!game) return <div>Loading...</div>;

  const isBotGame = game.mode === "PlayerVsBot";
  const isThinking =
  isBotGame &&
  game.botColor === game.currentTurn &&
  game.status === "InProgress";
  
  const playerColor =
    isBotGame && game.botColor
      ? game.botColor === "Red"
        ? "Green"
        : "Red"
      : null;

  const isBotTurn =
    isBotGame &&
    game.botColor === game.currentTurn &&
    game.status === "InProgress";

  return (
    <div className="game-page">
      <div className="game-card">

        {/* HEADER */}
        <div className="game-header">
          <h2>
            {game.status === "Finished"
              ? `Game Over - Winner: ${game.winner}`
              : `Turn: ${game.currentTurn}`}
          </h2>

          {isBotGame && (
            <div className="bot-badge">
              You are {playerColor}
            </div>
          )}
        </div>

        {/* BOARD WRAPPER (disable only in bot mode) */}
        <div
          className={
            isBotGame && isBotTurn ? "board-disabled" : ""
          }
        >
          <Board
            game={game}
            selected={selected}
            validMoves={validMoves}
            onCellClick={handleCellClick}
          />
        </div>

        {/* BOT THINKING INDICATOR */}
        {isBotGame && isThinking && (
          <div className="thinking-indicator">
            🤖 Bot is thinking...
          </div>
        )}

        {/* WINNER MODAL */}
        {showModal && (
          <WinnerModal
            winner={game.winner}
            onReplay={() => {
              setShowModal(false);

              if (isBotGame) {
                navigate("/play/bot");
              } else {
                navigate("/play/local");
              }
            }}
            onClose={() => setShowModal(false)}
          />
        )}
      </div>
    </div>
  );
}

export default GamePage;