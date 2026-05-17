import "./styles/piece.css";

function Piece({ type, owner }) {
  let pieceClass = "";

  if (type === "Bobail") {
    pieceClass = "bobail";
  }

  if (type === "PlayerPiece") {
    pieceClass = owner === "Red" ? "red" : "green";
  }

  return <div className={`piece ${pieceClass}`} />;
}

export default Piece;
