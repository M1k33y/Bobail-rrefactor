import "../styles/HomePage.css";


function HomePage() {
  return (
    <div className="home-container">

      <div className="hero">
        <h1>Bobail</h1>
        <p>
          A strategic African board game of movement, control,
          and tactical positioning.
        </p>

        <div className="hero-actions">
          <button className="btn btn-primary">
            Play Now
          </button>

          <button className="btn btn-secondary">
            Learn the Rules
          </button>
        </div>
      </div>

      <div className="feature-grid">
        <div className="feature-card">
          <h3>2 Player Game</h3>
          <p>Play locally on the same device.</p>
        </div>

        <div className="feature-card disabled">
          <h3>Play Online</h3>
          <p>Compete against other players. (Coming Soon)</p>
        </div>

        <div className="feature-card disabled">
          <h3>Play vs Bot</h3>
          <p>Challenge the AI. (Coming Soon)</p>
        </div>
      </div>

    </div>
  );
}

export default HomePage;