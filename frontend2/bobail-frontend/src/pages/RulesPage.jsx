import {
  Info,
  Move,
  Flag,
  RotateCcw,
  Trophy
} from "lucide-react";

import "../styles/RulesPage.css";

function RulesPage() {
  return (
    <div className="rules-page">
      <h1>Game Rules</h1>

      <div className="rules-grid">

        <div className="rule-card">
          <div className="rule-header">
            <Info size={18} strokeWidth={1.5} />
            <h2>Game Overview</h2>
          </div>
          <p>
            Bobail is a two-player African strategy game played on a 5x5 grid.
          </p>
          <p>
            Each player controls five tokens — one plays red, the other green.
          </p>
          <p>
            A neutral yellow token, called the <strong>Bobail</strong>,
            is moved by both players.
          </p>
        </div>

        <div className="rule-card">
          <div className="rule-header">
            <Move size={18} strokeWidth={1.5} />
            <h2>Movement</h2>
          </div>
          <ul>
            <li>Tokens move horizontally, vertically, or diagonally.</li>
            <li>No token can jump over another token.</li>
            <li>The Bobail moves one square per turn.</li>
            <li>Player tokens move as far as possible in one direction.</li>
          </ul>
        </div>

        <div className="rule-card">
          <div className="rule-header">
            <Flag size={18} strokeWidth={1.5} />
            <h2>Game Setup</h2>
          </div>
          <ul>
            <li>Each player's tokens start on their home row.</li>
            <li>The Bobail begins in the center square.</li>
          </ul>
        </div>

        <div className="rule-card">
          <div className="rule-header">
            <RotateCcw size={18} strokeWidth={1.5} />
            <h2>Turn Structure</h2>
          </div>
          <p>
            On the first turn, the first player only moves one of their tokens.
          </p>
          <p>
            Every following turn consists of:
          </p>
          <ol>
            <li>Move the Bobail (one square).</li>
            <li>Move one of your tokens.</li>
          </ol>
        </div>

        <div className="rule-card highlight">
          <div className="rule-header">
            <Trophy size={18} strokeWidth={1.5} />
            <h2>Victory</h2>
          </div>
          <ul>
            <li>
              If the Bobail reaches your home row, you win.
            </li>
            <li>
              If the Bobail is completely surrounded and cannot move,
              the current player loses.
            </li>
          </ul>
        </div>

      </div>
    </div>
  );
}

export default RulesPage;