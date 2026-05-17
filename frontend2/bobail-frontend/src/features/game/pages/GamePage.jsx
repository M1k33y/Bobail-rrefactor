import { useNavigate, useParams } from "react-router-dom";
import { useState } from "react";
import { useGame } from "../hooks/useGame";
import Board from "../components/Board";
import WinnerModal from "../components/WinnerModal";
import "../styles/GamePage.css";

function GamePage() {
  const navigate = useNavigate();
  const { gameId } = useParams();

  const {
    game,
    selected,
    validMoves,
    onlineError,
    isRealtimeConnected,
    handleCellClick
  } = useGame(gameId);

  const [dismissedFinishedGameId, setDismissedFinishedGameId] = useState(null);

  if (!game) return <div>Loading...</div>;

  const isBotGame = game.mode === "PlayerVsBot";
  const isOnlineGame = game.mode === "OnlineMultiplayer";
  const isThinking =
    isBotGame &&
    game.botColor === game.currentTurn &&
    game.status === "InProgress";

  const playerColor =
    isBotGame && game.botColor
      ? game.botColor === "Red"
        ? "Green"
        : "Red"
      : isOnlineGame
        ? game.playerColor
        : null;

  const isBotTurn =
    isBotGame &&
    game.botColor === game.currentTurn &&
    game.status === "InProgress";

  const isOnlineOpponentTurn =
    isOnlineGame &&
    game.playerColor &&
    game.playerColor !== game.currentTurn &&
    game.status === "InProgress";
  const showModal =
    game.status === "Finished" &&
    dismissedFinishedGameId !== game.id;

  return (
    <div className="game-page">
      <div className="game-card">
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

          {isOnlineGame && (
            <div className="online-game-info">
              <span>You are {playerColor || "waiting"}</span>
              <span>Game ID: {game.id}</span>
              <span className={isRealtimeConnected ? "online-connected" : "online-connecting"}>
                {isRealtimeConnected ? "Connected" : "Connecting"}
              </span>
            </div>
          )}
        </div>

        <div
          className={
            (isBotGame && isBotTurn) || isOnlineOpponentTurn ? "board-disabled" : ""
          }
        >
          <Board
            game={game}
            selected={selected}
            validMoves={validMoves}
            onCellClick={handleCellClick}
          />
        </div>

        {isBotGame && isThinking && (
          <div className="thinking-indicator">
            Bot is thinking...
          </div>
        )}

        {isOnlineGame && game.status === "WaitingForPlayers" && (
          <div className="online-waiting">
            Waiting for opponent
          </div>
        )}

        {isOnlineGame && onlineError && (
          <div className="online-error">
            {onlineError}
          </div>
        )}

        {showModal && (
          <WinnerModal
            winner={game.winner}
            onReplay={() => {
              setDismissedFinishedGameId(game.id);

              if (isBotGame) {
                navigate("/play/bot");
              } else if (isOnlineGame) {
                navigate("/play/online");
              } else {
                navigate("/play/local");
              }
            }}
            onClose={() => setDismissedFinishedGameId(game.id)}
          />
        )}
      </div>
    </div>
  );
}

export default GamePage;
