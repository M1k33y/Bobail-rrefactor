import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../styles/BotGameStartPage.css";
import { authFetch } from "../../auth/api/authFetch";
import { useAuth } from "../../auth/hooks/useAuth";

function BotGameStartPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  const [difficulty, setDifficulty] = useState("Easy");
  const [playerColor, setPlayerColor] = useState("Red");
  const [loading, setLoading] = useState(false);

  const botColor = playerColor === "Red" ? "Green" : "Red";

  async function handleStart() {
    if (!isAuthenticated) {
      navigate("/login");
      return;
    }

    try {
      setLoading(true);

      const res = await authFetch(
        "https://localhost:7006/api/games/vs-bot",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            difficulty,
            botColor
          })
        }
      );

      if (!res.ok) {
        throw new Error("Failed to start game");
      }

      const data = await res.json();
      navigate(`/play/${data.gameId}`);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="game-page">
      <div className="game-card bot-config-card">
        <div className="game-header">
          <h2>Play vs Bot</h2>
        </div>

        {/* DIFFICULTY */}
        <div className="settings-section centered">
          <h3>Difficulty</h3>
          <div className="option-group">
            <button
              className={`btn difficulty-easy ${
                difficulty === "Easy" ? "active" : ""
              }`}
              onClick={() => setDifficulty("Easy")}
            >
              Easy
            </button>

            <button
              className={`btn difficulty-medium ${
                difficulty === "Medium" ? "active" : ""
              }`}
              onClick={() => setDifficulty("Medium")}
            >
              Medium
            </button>

            <button
              className={`btn difficulty-hard ${
                difficulty === "Hard" ? "active" : ""
              }`}
              onClick={() => setDifficulty("Hard")}
            >
              Hard
            </button>
          </div>
        </div>

        {/* PLAYER COLOR */}
        <div className="settings-section centered">
          <h3>Your Color</h3>
          <div className="option-group">
            <button
              className={`btn color-red ${
                playerColor === "Red" ? "active" : ""
              }`}
              onClick={() => setPlayerColor("Red")}
            >
              Red
            </button>

            <button
              className={`btn color-green ${
                playerColor === "Green" ? "active" : ""
              }`}
              onClick={() => setPlayerColor("Green")}
            >
              Green
            </button>
          </div>
        </div>

        <div className="start-button-wrapper">
          <button
            className="btn btn-primary start-btn"
            onClick={handleStart}
            disabled={loading}
          >
            {loading ? "Starting..." : "Play Game"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default BotGameStartPage;