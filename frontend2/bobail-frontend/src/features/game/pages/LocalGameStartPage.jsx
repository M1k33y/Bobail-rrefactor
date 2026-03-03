import { useNavigate } from "react-router-dom";
import { gameApi } from "../services/gameApi";

function LocalGameStartPage() {
  const navigate = useNavigate();

  const handleStart = async () => {
    const data = await gameApi.create();
    navigate(`/play/local/${data.gameId}`);
  };

  return (
    <div className="game-page">
      <div className="game-card">
        <h2>2 Player Game</h2>

        <button className="btn" onClick={handleStart}>
          Start New Game
        </button>
      </div>
    </div>
  );
}

export default LocalGameStartPage;