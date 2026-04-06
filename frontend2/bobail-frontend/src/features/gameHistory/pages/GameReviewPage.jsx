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

function getMoveLabel(moveNumber) {
  return moveNumber === 0 ? "Start" : `Move ${moveNumber}`;
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

  if (loading) {
    return <div className="review-status">Loading replay...</div>;
  }

  if (error) {
    return <div className="review-status review-error">{error}</div>;
  }

  if (!replay || !currentState) {
    return <div className="review-status">No replay data available for this game.</div>;
  }

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
                {getMoveLabel(currentState.moveNumber)} / {replay.states.length - 1 < 0 ? 0 : replay.states.length - 1}
              </h2>
              <p className="review-caption">
                {currentState.moveNumber === 0
                  ? "Initial board setup"
                  : currentState.status === "Finished"
                    ? `Winner: ${currentState.winner}`
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
                  <span>{getMoveLabel(state.moveNumber)}</span>
                  <span>{formatPlayedAt(state.createdAtUtc)}</span>
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
