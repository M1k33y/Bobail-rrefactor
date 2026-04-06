import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { gameApi } from "../../game/api/gameApi";
import "../styles/GameHistoryPage.css";

function formatPlayedAt(value) {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function GameHistoryPage() {
  const navigate = useNavigate();
  const [games, setGames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function loadHistory() {
      try {
        setLoading(true);
        const data = await gameApi.getHistory();

        if (!active) {
          return;
        }

        setGames(data);
        setError("");
      } catch (err) {
        if (active) {
          setError(err.message || "Failed to load game history.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadHistory();

    return () => {
      active = false;
    };
  }, []);

  return (
    <div className="history-page">
      <div className="history-card">
        <div className="history-header">
          <div>
            <p className="history-eyebrow">Game History</p>
            <h1>Your completed matches</h1>
          </div>
          <p className="history-subtitle">
            Review past games against the bot or other players.
          </p>
        </div>

        {loading && <div className="history-empty">Loading history...</div>}

        {!loading && error && <div className="history-error">{error}</div>}

        {!loading && !error && games.length === 0 && (
          <div className="history-empty">
            No completed games yet. Finish a match and it will appear here.
          </div>
        )}

        {!loading && !error && games.length > 0 && (
          <div className="history-list">
            {games.map((game) => (
              <div key={game.gameId} className="history-row">
                <div className="history-main">
                  <div className="history-label">Played vs:</div>
                  <div className="history-opponent">{game.playedVs}</div>
                  <div className={`history-result ${game.result.toLowerCase()}`}>
                    {game.result}
                  </div>
                </div>

                <div className="history-meta">
                  <span>{game.mode}</span>
                  <span>{formatPlayedAt(game.playedAtUtc)}</span>
                </div>

                <button
                  type="button"
                  className="btn history-review-button"
                  onClick={() => navigate(`/game-history/${game.gameId}/review`)}
                >
                  Review
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default GameHistoryPage;
