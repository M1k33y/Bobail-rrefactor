import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import Board from "../../game/components/Board";
import { gameApi } from "../../game/api/gameApi";
import "../styles/GameReviewPage.css";

function formatPlayedAt(value) {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function formatClock(milliseconds) {
  const totalSeconds = Math.ceil(Math.max(0, milliseconds) / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}

function formatEndReason(endReason) {
  switch (endReason) {
    case "Timeout":
      return "by timeout";
    case "Resignation":
      return "by resignation";
    case "Forfeit":
      return "by forfeit";
    case "AdminBan":
      return "after opponent ban";
    default:
      return null;
  }
}

function formatFinishedCaption(state) {
  const reason = formatEndReason(state.endReason);

  return reason
    ? `Winner: ${state.winner} ${reason}`
    : `Winner: ${state.winner}`;
}

function hasSameBoardPosition(firstState, secondState) {
  if (!firstState || !secondState) {
    return false;
  }

  const firstPieces = [...(firstState.pieces || [])].sort(comparePieces);
  const secondPieces = [...(secondState.pieces || [])].sort(comparePieces);

  if (firstPieces.length !== secondPieces.length) {
    return false;
  }

  return firstPieces.every((piece, index) => {
    const other = secondPieces[index];

    return (
      piece.type === other.type &&
      piece.owner === other.owner &&
      piece.row === other.row &&
      piece.column === other.column
    );
  });
}

function comparePieces(first, second) {
  return `${first.type}:${first.owner || ""}:${first.row}:${first.column}`
    .localeCompare(`${second.type}:${second.owner || ""}:${second.row}:${second.column}`);
}

function isFinishSnapshot(state, index, states) {
  if (index === 0 || state.status !== "Finished") {
    return false;
  }

  return hasSameBoardPosition(states[index - 1], state);
}

function getStateLabel(state, index, states) {
  if (index === 0) {
    return "Start";
  }

  return isFinishSnapshot(state, index, states)
    ? "Finish"
    : `Move ${state.moveNumber}`;
}

function getInitialOnlineClockMilliseconds(replay) {
  return replay.states.find((state) => state.clock)?.clock?.initialTimeMilliseconds ?? null;
}

function getTimelineDetail(state, replay) {
  if (state.mode === "OnlineMultiplayer" && state.clock) {
    return `Red ${formatClock(state.clock.redRemainingMilliseconds)} / Green ${formatClock(state.clock.greenRemainingMilliseconds)}`;
  }

  if (state.mode === "OnlineMultiplayer") {
    const initialTime = getInitialOnlineClockMilliseconds(replay);

    if (initialTime !== null) {
      return `Red ${formatClock(initialTime)} / Green ${formatClock(initialTime)}`;
    }
  }

  return formatPlayedAt(state.createdAtUtc);
}

function GameReviewPage() {
  const { gameId } = useParams();
  const [replay, setReplay] = useState(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function loadReplay() {
      try {
        setLoading(true);
        const data = await gameApi.getReplay(gameId);

        if (!active) {
          return;
        }

        setReplay(data);
        setCurrentIndex(Math.max((data.states?.length || 1) - 1, 0));
        setError("");
      } catch (err) {
        if (active) {
          setError(err.message || "Failed to load replay.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadReplay();

    return () => {
      active = false;
    };
  }, [gameId]);

  const currentState = useMemo(() => {
    if (!replay?.states?.length) {
      return null;
    }

    return replay.states[currentIndex] ?? replay.states[0];
  }, [currentIndex, replay]);

  const replayStateLabels = useMemo(() => {
    if (!replay?.states?.length) {
      return [];
    }

    return replay.states.map((state, index, states) =>
      getStateLabel(state, index, states)
    );
  }, [replay]);

  const moveCount = useMemo(() => {
    if (!replay?.states?.length) {
      return 0;
    }

    return replay.states.filter((state, index, states) =>
      index > 0 && !isFinishSnapshot(state, index, states)
    ).length;
  }, [replay]);

  if (loading) {
    return <div className="review-status">Loading replay...</div>;
  }

  if (error) {
    return <div className="review-status review-error">{error}</div>;
  }

  if (!replay || !currentState) {
    return <div className="review-status">No replay data available for this game.</div>;
  }

  const currentLabel = replayStateLabels[currentIndex] ?? getStateLabel(currentState, currentIndex, replay.states);

  return (
    <div className="review-page">
      <div className="review-card">
        <div className="review-header">
          <div>
            <p className="review-eyebrow">Replay</p>
            <h1>Review match</h1>
            <p className="review-versus">Played vs: {replay.playedVs}</p>
          </div>

          <div className="review-summary">
            <span className={`review-result ${replay.result.toLowerCase()}`}>
              {replay.result}
            </span>
            <span>{formatPlayedAt(replay.playedAtUtc)}</span>
          </div>
        </div>

        <div className="review-content">
          <div className="review-board">
            <Board
              game={currentState}
              selected={null}
              validMoves={[]}
              onCellClick={() => {}}
            />
          </div>

          <div className="review-panel">
            <div className="review-move-card">
              <p className="review-label">Current position</p>
              <h2>
                {currentLabel.startsWith("Move")
                  ? `${currentLabel} / ${moveCount}`
                  : currentLabel}
              </h2>
              <p className="review-caption">
                {currentState.moveNumber === 0
                  ? "Initial board setup"
                  : currentState.status === "Finished"
                    ? formatFinishedCaption(currentState)
                    : `Turn: ${currentState.currentTurn}`}
              </p>
            </div>

            <div className="review-controls">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setCurrentIndex((value) => Math.max(value - 1, 0))}
                disabled={currentIndex === 0}
              >
                Previous
              </button>

              <button
                type="button"
                className="btn"
                onClick={() =>
                  setCurrentIndex((value) =>
                    Math.min(value + 1, replay.states.length - 1)
                  )
                }
                disabled={currentIndex === replay.states.length - 1}
              >
                Next
              </button>
            </div>

            <div className="review-timeline">
              {replay.states.map((state, index) => (
                <button
                  key={state.moveNumber}
                  type="button"
                  className={`review-timeline-item ${
                    index === currentIndex ? "active" : ""
                  }`}
                  onClick={() => setCurrentIndex(index)}
                >
                  <span>{replayStateLabels[index]}</span>
                  <span className="review-timeline-detail">{getTimelineDetail(state, replay)}</span>
                </button>
              ))}
            </div>

            <Link to="/game-history" className="review-back-link">
              Back to history
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}

export default GameReviewPage;
