import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { gameApi } from "../../game/api/gameApi";
import "../styles/GameHistoryPage.css";

const PAGE_SIZE = 50;

function formatPlayedAt(value) {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function GameHistoryPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [games, setGames] = useState([]);
  const [pageInfo, setPageInfo] = useState({
    page: 1,
    totalPages: 0,
    totalCount: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const pageParam = Number.parseInt(searchParams.get("page") ?? "1", 10);
  const currentPage = Number.isNaN(pageParam) || pageParam < 1 ? 1 : pageParam;

  useEffect(() => {
    let active = true;

    async function loadHistory() {
      try {
        setLoading(true);
        const data = await gameApi.getHistory(currentPage, PAGE_SIZE);

        if (!active) {
          return;
        }

        setGames(data.items ?? []);
        setPageInfo({
          page: data.page ?? 1,
          totalPages: data.totalPages ?? 0,
          totalCount: data.totalCount ?? 0,
          hasPreviousPage: data.hasPreviousPage ?? false,
          hasNextPage: data.hasNextPage ?? false,
        });
        setError("");

        if ((data.page ?? 1) !== currentPage) {
          setSearchParams({ page: String(data.page ?? 1) }, { replace: true });
        }

        window.scrollTo({ top: 0, behavior: "smooth" });
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
  }, [currentPage, setSearchParams]);

  function goToPage(page) {
    setSearchParams({ page: String(page) });
  }

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
          <>
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

            <div className="history-pagination">
              <div className="history-pagination-summary">
                Page {pageInfo.page} of {Math.max(pageInfo.totalPages, 1)} - {pageInfo.totalCount} games
              </div>

              <div className="history-pagination-actions">
                <button
                  type="button"
                  className="btn history-pagination-button"
                  onClick={() => goToPage(1)}
                  disabled={!pageInfo.hasPreviousPage}
                >
                  Newest games
                </button>

                <button
                  type="button"
                  className="btn history-pagination-button"
                  onClick={() => goToPage(pageInfo.page + 1)}
                  disabled={!pageInfo.hasNextPage}
                >
                  Older games
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

export default GameHistoryPage;
