import "./styles/Cell.css";
import Piece from "./Piece";

function Cell({ row, col, game, selected, validMoves, onClick }) {
  const piece = game.pieces.find(
    p => p.row === row && p.column === col
  );

  const isSelected =
    selected?.row === row && selected?.col === col;

  const isValidMove = validMoves.some(
    m => m.row === row && m.column === col
  );

  return (
    <div
      className={`cell ${isSelected ? "selected" : ""} ${
        isValidMove ? "valid-move clickable" : ""
      }`}
      onClick={() => onClick(row, col)}
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