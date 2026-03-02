import "../../../styles/modal.css";

function WinnerModal({ winner, onReplay, onClose }) {
  return (
    <div className="modal-overlay">
      <div className="modal-card">
        <button className="modal-close" onClick={onClose}>
          x
        </button>

        <h2>Game Over</h2>
        <p className="winner-text">
          {winner} wins the game!
        </p>

        <button className="btn btn-primary" onClick={onReplay}>
          Replay
        </button>
      </div>
    </div>
  );
}

export default WinnerModal;