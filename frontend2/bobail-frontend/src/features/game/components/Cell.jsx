import "./styles/cell.css";
import Piece from "./Piece";

function Cell({
  row,
  col,
  piece,
  selected,
  validMoves,
  onClick,
  highlight,
  interactive = true
}) {
  const isSelected =
    selected?.row === row &&
    (selected?.col === col || selected?.column === col);

  const isValidMove = validMoves.some(
    m => m.row === row && m.column === col
  );

  const highlightClass = highlight?.type
    ? `cell-highlight-${highlight.type}`
    : "";

  return (
    <div
      className={`cell ${isSelected ? "selected" : ""} ${
        isValidMove ? `valid-move ${interactive ? "clickable" : ""}` : ""
      } ${highlightClass}`}
      onClick={interactive ? () => onClick(row, col) : undefined}
    >
      {piece && (
        <Piece
          type={piece.type}
          owner={piece.owner}
        />
      )}
    </div>
  );
}

export default Cell;
