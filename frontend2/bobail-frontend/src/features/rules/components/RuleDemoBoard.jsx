import Board from "../../game/components/Board";
import { useRuleDemo } from "../hooks/useRuleDemo";

function RuleDemoBoard({ demo }) {
  const { cycle, frame, game } = useRuleDemo(demo);

  return (
    <div className="rule-demo" aria-label={`${demo.id} rule demonstration`}>
      <div className="rule-demo-board-wrap">
        <Board
          key={`${demo.id}-${cycle}`}
          game={game}
          selected={frame.selected ?? null}
          validMoves={frame.validMoves ?? []}
          onCellClick={() => {}}
          cellHighlights={frame.highlights ?? []}
          className="rule-demo-board"
          animatedPieces
          interactive={false}
        />
      </div>

      <p className="rule-demo-caption">{frame.caption}</p>
    </div>
  );
}

export default RuleDemoBoard;
