import Cell from "./Cell";
import "./styles/Board.css";
function Board({ game, selected, validMoves, onCellClick }) {
  return (
    <div id="board">
      {Array.from({ length: 5 }).map((_, row) =>
        Array.from({ length: 5 }).map((_, col) => (
          <Cell
            key={`${row}-${col}`}
            row={row}
            col={col}
            game={game}
            selected={selected}
            validMoves={validMoves}
            onClick={onCellClick}
          />
        ))
      )}
    </div>
  );
}

export default Board;