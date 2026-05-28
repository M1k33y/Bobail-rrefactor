import "./styles/modal.css";

function getEndReasonText(endReason) {
  switch (endReason) {
    case "Timeout":
      return "by timeout";
    case "Resignation":
      return "by resignation";
    case "Forfeit":
      return "by forfeit";
    case "AdminBan":
      return "after opponent ban";
    default:
      return "";
  }
}

function WinnerModal({ winner, endReason, onReplay, onClose }) {
  const endReasonText = getEndReasonText(endReason);

  return (
    <div className="modal-overlay">
      <div className="modal-card">
        <button className="modal-close" onClick={onClose}>
          x
        </button>

        <h2>Game Over</h2>
        <p className="winner-text">
          {winner} wins {endReasonText || "the game"}!
        </p>

        <button className="btn btn-primary" onClick={onReplay}>
          Replay
        </button>
      </div>
    </div>
  );
}

export default WinnerModal;
