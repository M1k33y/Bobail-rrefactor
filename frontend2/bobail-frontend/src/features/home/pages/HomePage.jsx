import { useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/hooks/useAuth";
import "../styles/HomePage.css";

function HomePage() {
  const navigate = useNavigate();
  const { isAuthenticated, nickname } = useAuth();

  return (
    <div className="home-container">
      {isAuthenticated && (
        <div className="welcome-banner">
          <span className="welcome-label">Welcome back</span>
          <strong>{nickname || "Player"}</strong>
          <span className="welcome-copy">Ready for another Bobail match?</span>
        </div>
      )}

      <div className="hero">
        <h1>Bobail</h1>
        <p>
          A strategic African board game of movement, control,
          and tactical positioning.
        </p>

        <div className="hero-actions">
          <button
            className="btn btn-primary"
            onClick={() => navigate("/play/online")}
          >
            Play Now
          </button>

          <button
            className="btn btn-secondary"
            onClick={() => navigate("/rules")}
          >
            Learn the Rules
          </button>
        </div>
      </div>

      <div className="feature-grid">
        <div
          className="feature-card clickable"
          onClick={() => navigate("/play/local")}
        >
          <h3>2 Player Game</h3>
          <p>Play locally on the same device.</p>
        </div>

        <div
          className="feature-card clickable"
          onClick={() => navigate("/play/online")}
        >
          <h3>Play Online</h3>
          <p>Compete against another player in real time.</p>
        </div>

        <div
          className="feature-card clickable"
          onClick={() => navigate("/play/bot")}
        >
          <h3>Play vs Bot</h3>
          <p>Challenge the AI.</p>
        </div>
      </div>
    </div>
  );
}

export default HomePage;
