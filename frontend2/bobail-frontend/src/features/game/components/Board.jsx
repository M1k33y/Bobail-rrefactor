import Cell from "./Cell";
import Piece from "./Piece";
import "./styles/board.css";

function Board({
  game,
  selected,
  validMoves = [],
  onCellClick = () => {},
  className = "",
  cellHighlights = [],
  animatedPieces = false,
  interactive = true
}) {
  const pieces = game?.pieces ?? [];
  const boardClassName = [
    "board",
    animatedPieces ? "board-animated-pieces" : "",
    interactive ? "" : "board-static",
    className
  ]
    .filter(Boolean)
    .join(" ");

  function getCellHighlight(row, col) {
    return cellHighlights.find(
      (highlight) =>
        highlight.row === row &&
        (highlight.column === col || highlight.col === col)
    );
  }

  return (
    <div className={boardClassName}>
      {Array.from({ length: 5 }).map((_, row) =>
        Array.from({ length: 5 }).map((_, col) => (
          <Cell
            key={`${row}-${col}`}
            row={row}
            col={col}
            piece={
              animatedPieces
                ? null
                : pieces.find((piece) => piece.row === row && piece.column === col)
            }
            selected={selected}
            validMoves={validMoves}
            onClick={onCellClick}
            highlight={getCellHighlight(row, col)}
            interactive={interactive}
          />
        ))
      )}

      {animatedPieces && (
        <div className="board-piece-layer" aria-hidden="true">
          {pieces.map((piece, index) => (
            <div
              key={piece.id ?? `${piece.type}-${piece.owner ?? "neutral"}-${index}`}
              className="board-piece-slot"
              style={{
                "--piece-row": piece.row,
                "--piece-col": piece.column
              }}
            >
              <Piece type={piece.type} owner={piece.owner} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default Board;
