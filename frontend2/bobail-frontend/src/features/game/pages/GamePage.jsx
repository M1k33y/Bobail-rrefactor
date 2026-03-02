import { useNavigate, useParams } from "react-router-dom";
import { useGame } from "../hooks/useGame";
import Board from "../components/Board";
import WinnerModal from "../components/WinnerModal";
import { useState, useEffect } from "react";

function GamePage() {
    const navigate = useNavigate();
    const { gameId } = useParams();
    const { game, selected, validMoves, handleCellClick } = useGame(gameId);
    const [showModal, setShowModal] = useState(false);

    useEffect(() => {
        if (game?.status === "Finished") {
            setShowModal(true);
        }
    }, [game]);

    if (!game) return <div>Loading...</div>;

    return (
        <div className="game-page">
            {/* <div className="game-actions">
                <button
                    className="btn btn-secondary"
                    onClick={() => handleResign()}
                >
                    🏳 Resign
                </button>
            </div> */}

            <div className="game-card">
                <div className="game-header">
                    <h2>
                        {game.status === "Finished"
                            ? `Game Over - Winner: ${game.winner}`
                            : `Turn: ${game.currentTurn}`}
                    </h2>
                </div>

                <Board
                    game={game}
                    selected={selected}
                    validMoves={validMoves}
                    onCellClick={handleCellClick}
                />

                {showModal && (
                    <WinnerModal
                        winner={game.winner}
                        onReplay={() => {
                            setShowModal(false);
                            navigate("/play/local");
                        }}
                        onClose={() => setShowModal(false)}
                    />
                )}
            </div>
        </div>


    );
}

// function handleResign() {
//   const winner =
//     game.currentTurn === "Red" ? "Green" : "Red";

//   setGame({
//     ...game,
//     status: "Finished",
//     winner
//   });
// }

export default GamePage;