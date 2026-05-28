import { useNavigate, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { Check, Copy, Flag } from "lucide-react";
import { useGame } from "../hooks/useGame";
import Board from "../components/Board";
import WinnerModal from "../components/WinnerModal";
import "../styles/GamePage.css";

function formatClock(milliseconds) {
  const totalSeconds = Math.ceil(Math.max(0, milliseconds) / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}

function getDisplayClockMilliseconds(game, color, nowMs) {
  if (!game.clock) {
    return 0;
  }

  const remaining =
    color === "Red"
      ? game.clock.redRemainingMilliseconds
      : game.clock.greenRemainingMilliseconds;

  if (
    game.status !== "InProgress" ||
    game.currentTurn !== color ||
    !game.clock.receivedAtMs
  ) {
    return remaining;
  }

  return Math.max(0, remaining - (nowMs - game.clock.receivedAtMs));
}

function OnlineClock({ color, milliseconds, isActive }) {
  return (
    <div className={isActive ? "online-clock active" : "online-clock"}>
      <span>{color}</span>
      <strong>{formatClock(milliseconds)}</strong>
    </div>
  );
}

function getGameOverTitle(game) {
  if (game.status !== "Finished") {
    return `Turn: ${game.currentTurn}`;
  }

  switch (game.endReason) {
    case "Timeout":
      return `Game Over - ${game.winner} won on time`;
    case "Resignation":
      return `Game Over - ${game.winner} won by resignation`;
    case "Forfeit":
      return `Game Over - ${game.winner} won by forfeit`;
    case "AdminBan":
      return `Game Over - ${game.winner} won after ban`;
    default:
      return `Game Over - Winner: ${game.winner}`;
  }
}

async function copyTextToClipboard(text) {
  if (navigator.clipboard?.writeText) {
    await navigator.clipboard.writeText(text);
    return;
  }

  const textarea = document.createElement("textarea");
  textarea.value = text;
  textarea.setAttribute("readonly", "");
  textarea.style.position = "fixed";
  textarea.style.opacity = "0";
  document.body.appendChild(textarea);
  textarea.select();

  try {
    if (!document.execCommand("copy")) {
      throw new Error("Clipboard copy failed");
    }
  } finally {
    document.body.removeChild(textarea);
  }
}

function GamePage() {
  const navigate = useNavigate();
  const { gameId } = useParams();

  const {
    game,
    selected,
    validMoves,
    onlineError,
    isRealtimeConnected,
    handleCellClick,
    handleResign
  } = useGame(gameId);

  const [dismissedFinishedGameId, setDismissedFinishedGameId] = useState(null);
  const [clockNowMs, setClockNowMs] = useState(() => Date.now());
  const [copyInviteStatus, setCopyInviteStatus] = useState("");

  useEffect(() => {
    if (!game?.clock || game.status !== "InProgress") return;

    const interval = setInterval(() => {
      setClockNowMs(Date.now());
    }, 250);

    return () => clearInterval(interval);
  }, [game?.clock, game?.status, game?.currentTurn]);

  useEffect(() => {
    if (!copyInviteStatus) return;

    const timeout = setTimeout(() => {
      setCopyInviteStatus("");
    }, 1800);

    return () => clearTimeout(timeout);
  }, [copyInviteStatus]);

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
  const redClockMs = getDisplayClockMilliseconds(game, "Red", clockNowMs);
  const greenClockMs = getDisplayClockMilliseconds(game, "Green", clockNowMs);
  const showOnlineClocks = isOnlineGame && game.clock;
  const showResignButton =
    isOnlineGame &&
    game.status === "InProgress";
  const showOnlinePlayerColor =
    isOnlineGame &&
    game.status !== "WaitingForPlayers" &&
    playerColor;
  const showInviteGameId =
    isOnlineGame &&
    game.status === "WaitingForPlayers";
  const canResign =
    showResignButton &&
    game.playerColor &&
    game.playerColor === game.currentTurn;

  function confirmAndResign() {
    if (!window.confirm("Resign this game?")) {
      return;
    }

    handleResign();
  }

  async function copyInviteGameId() {
    if (!game?.id) {
      return;
    }

    try {
      await copyTextToClipboard(game.id);
      setCopyInviteStatus("Game ID copied");
    } catch {
      setCopyInviteStatus("Copy failed");
    }
  }

  return (
    <div className="game-page">
      <div className="game-card">
        <div className="game-header">
          <h2>
            {getGameOverTitle(game)}
          </h2>

          {isBotGame && (
            <div className="bot-badge">
              You are {playerColor}
            </div>
          )}

          {isOnlineGame && (
            <div className="online-game-info">
              {showOnlinePlayerColor && (
                <span>You are {playerColor}</span>
              )}
              {showInviteGameId && (
                <span className="online-game-id-stack">
                  <span className="online-game-id-group">
                    <span>Game ID: {game.id}</span>
                    <button
                      type="button"
                      className="online-copy-button"
                      onClick={copyInviteGameId}
                      title="Copy game ID"
                      aria-label="Copy game ID"
                    >
                      {copyInviteStatus === "Game ID copied" ? (
                        <Check size={15} aria-hidden="true" />
                      ) : (
                        <Copy size={15} aria-hidden="true" />
                      )}
                    </button>
                  </span>
                  {copyInviteStatus && (
                    <span
                      className={
                        copyInviteStatus === "Game ID copied"
                          ? "online-copy-feedback"
                          : "online-copy-feedback error"
                      }
                      role="status"
                      aria-live="polite"
                    >
                      {copyInviteStatus}
                    </span>
                  )}
                </span>
              )}
              <span className={isRealtimeConnected ? "online-connected" : "online-connecting"}>
                {isRealtimeConnected ? "Connected" : "Connecting"}
              </span>
            </div>
          )}

        </div>

        {showOnlineClocks && (
          <div className="online-clock-row online-clock-row-top">
            <OnlineClock
              color="Red"
              milliseconds={redClockMs}
              isActive={game.currentTurn === "Red"}
            />
          </div>
        )}

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

        {(showOnlineClocks || showResignButton) && (
          <div className="online-clock-row online-clock-row-bottom">
            <div className="online-bottom-actions">
              {showOnlineClocks && (
                <OnlineClock
                  color="Green"
                  milliseconds={greenClockMs}
                  isActive={game.currentTurn === "Green"}
                />
              )}

              {showResignButton && (
                <button
                  type="button"
                  className="btn btn-secondary resign-button"
                  onClick={confirmAndResign}
                  disabled={!canResign}
                  title={canResign ? "Resign game" : "You can resign only on your turn"}
                >
                  <Flag size={16} aria-hidden="true" />
                  Resign
                </button>
              )}
            </div>
          </div>
        )}

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
            endReason={game.endReason}
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
