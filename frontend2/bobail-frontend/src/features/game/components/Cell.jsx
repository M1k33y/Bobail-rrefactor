function Cell({ row, col, game, selected, validMoves, onClick }) {
  const piece = game.pieces.find(
    p => p.row === row && p.column === col
  );

  const isSelected =
    selected?.row === row && selected?.col === col;

  const isValidMove = validMoves.some(
    m => m.row === row && m.column === col
  );

  let pieceClass = "";

  if (piece) {
    if (piece.type === "Bobail") {
      pieceClass = "bobail";
    }

    if (piece.type === "PlayerPiece") {
      pieceClass =
        piece.owner === "Red"
          ? "red"
          : "green";
    }
  }

  return (
    <div
      className={`cell ${isSelected ? "selected" : ""} ${
        isValidMove ? "valid-move clickable" : ""
      }`}
      onClick={() => onClick(row, col)}
    >
      {piece && <div className={`piece ${pieceClass}`} />}
    </div>
  );
}

export default Cell;