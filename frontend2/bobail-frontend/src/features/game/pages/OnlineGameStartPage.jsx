import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { gameApi } from "../api/gameApi";
import { useAuth } from "../../auth/hooks/useAuth";
import "../styles/OnlineGameStartPage.css";

function OnlineGameStartPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [gameId, setGameId] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    let active = true;

    async function redirectToActiveGame() {
      try {
        const current = await gameApi.getCurrentOnline();

        if (active && current?.hasActiveGame && current.gameId) {
          navigate(`/play/online/${current.gameId}`, { replace: true });
        }
      } catch (err) {
        if (active) {
          setError(err.message || "Failed to load active online game.");
        }
      }
    }

    redirectToActiveGame();

    return () => {
      active = false;
    };
  }, [isAuthenticated, navigate]);

  const ensureAuthenticated = () => {
    if (isAuthenticated) {
      return true;
    }

    navigate("/login");
    return false;
  };

  async function handleCreate() {
    if (!ensureAuthenticated()) return;

    try {
      setLoading(true);
      setError("");

      const data = await gameApi.createOnline();
      navigate(`/play/online/${data.gameId}`);
    } catch (err) {
      setError(err.message || "Failed to create online game.");
    } finally {
      setLoading(false);
    }
  }

  async function handleJoin(event) {
    event.preventDefault();

    if (!ensureAuthenticated()) return;

    const trimmedGameId = gameId.trim();

    if (!trimmedGameId) {
      setError("Game ID is required.");
      return;
    }

    try {
      setLoading(true);
      setError("");

      await gameApi.joinOnline(trimmedGameId);
      navigate(`/play/online/${trimmedGameId}`);
    } catch (err) {
      setError(err.message || "Failed to join online game.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="game-page">
      <div className="game-card online-config-card">
        <div className="game-header">
          <h2>Play Online</h2>
        </div>

        <button
          className="btn btn-primary online-create-btn"
          onClick={handleCreate}
          disabled={loading}
        >
          {loading ? "Starting..." : "Create Game"}
        </button>

        <form className="online-join-form" onSubmit={handleJoin}>
          <input
            className="online-game-input"
            value={gameId}
            onChange={(event) => setGameId(event.target.value)}
            placeholder="Game ID"
            disabled={loading}
          />

          <button className="btn" type="submit" disabled={loading}>
            Join Game
          </button>
        </form>

        {error && <div className="online-error">{error}</div>}
      </div>
    </div>
  );
}

export default OnlineGameStartPage;
