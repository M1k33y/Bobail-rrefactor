function Cell({ row, col, game, selected, validMoves, onClick }) {
  const piece = game.pieces.find(
    p => p.row === row && p.column === col
  );

  const isSelected =
    selected?.row === row && selected?.col === col;

  const isValidMove = validMoves.some(
    m => m.row === row && m.column === col
  );

  const className = `
    cell
    ${piece?.type === "Red" ? "red" : ""}
    ${piece?.type === "Green" ? "green" : ""}
    ${piece?.type === "Bobail" ? "bobail" : ""}
    ${isSelected ? "selected" : ""}
    ${isValidMove ? "valid-move clickable" : ""}
  `;

  return (
  <div
    className={`cell ${isSelected ? "selected" : ""} ${
      isValidMove ? "valid-move clickable" : ""
    }`}
    onClick={() => onClick(row, col)}
  >
    {piece && <div className={`piece ${piece.type.toLowerCase()}`} />}
  </div>
);
}

export default Cell;