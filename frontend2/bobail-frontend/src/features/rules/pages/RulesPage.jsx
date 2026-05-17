import {
  CircleDot,
  Flag,
  Info,
  Move,
  RotateCcw,
  ShieldAlert,
  Trophy
} from "lucide-react";

import RuleDemoBoard from "../components/RuleDemoBoard";
import { ruleDemos } from "../data/ruleDemos";
import "../styles/RulesPage.css";

const rules = [
  {
    id: "overview",
    title: "Game Overview",
    icon: Info,
    demo: ruleDemos.overview,
    content: (
      <>
        <p>
          Bobail is a two-player African strategy game played on a 5x5 grid.
        </p>
        <p>
          Each player controls five tokens: one plays Red, the other Green.
        </p>
        <p>
          A neutral yellow token, called the <strong>Bobail</strong>, is moved
          by both players.
        </p>
      </>
    )
  },
  {
    id: "setup",
    title: "Game Setup",
    icon: Flag,
    demo: ruleDemos.setup,
    content: (
      <ul>
        <li>Each player's tokens start on their home row.</li>
        <li>The Bobail begins in the center square.</li>
      </ul>
    )
  },
  {
    id: "bobail",
    title: "Bobail Movement",
    icon: CircleDot,
    demo: ruleDemos.bobail,
    content: (
      <ul>
        <li>The Bobail moves one square per turn.</li>
        <li>It may move horizontally, vertically, or diagonally.</li>
        <li>It can only move onto an empty adjacent square.</li>
      </ul>
    )
  },
  {
    id: "movement",
    title: "Player Movement",
    icon: Move,
    demo: ruleDemos.movement,
    content: (
      <ul>
        <li>Player tokens move horizontally, vertically, or diagonally.</li>
        <li>After choosing a direction, the token moves as far as possible.</li>
        <li>Tokens cannot jump over another token.</li>
      </ul>
    )
  },
  {
    id: "legal",
    title: "Legal vs Illegal Moves",
    icon: ShieldAlert,
    demo: ruleDemos.legal,
    content: (
      <ul>
        <li>A token must stop at the farthest legal square in its direction.</li>
        <li>Occupied cells block movement.</li>
        <li>Cells beyond a blocker are not legal destinations.</li>
      </ul>
    )
  },
  {
    id: "turn",
    title: "Turn Structure",
    icon: RotateCcw,
    demo: ruleDemos.turn,
    content: (
      <>
        <p>
          On the first turn, the first player only moves one of their tokens.
        </p>
        <p>Every following turn consists of:</p>
        <ol>
          <li>Move the Bobail one square.</li>
          <li>Move one of your tokens.</li>
        </ol>
      </>
    )
  },
  {
    id: "victory",
    title: "Victory",
    icon: Trophy,
    demo: ruleDemos.victory,
    highlight: true,
    content: (
      <ul>
        <li>If the Bobail reaches your home row, you win.</li>
        <li>
          If the Bobail is completely surrounded and cannot move, the current
          player loses.
        </li>
      </ul>
    )
  }
];

function RulesPage() {
  return (
    <div className="rules-page">
      <div className="rules-hero">
        <p className="rules-eyebrow">Learn</p>
        <h1>Game Rules</h1>
        
      </div>

      <div className="rules-grid">
        {rules.map((rule) => {
          const Icon = rule.icon;

          return (
            <article
              key={rule.id}
              className={`rule-card ${rule.highlight ? "highlight" : ""}`}
            >
              <div className="rule-copy">
                <div className="rule-header">
                  <Icon size={18} strokeWidth={1.7} />
                  <h2>{rule.title}</h2>
                </div>

                <div className="rule-body">{rule.content}</div>
              </div>

              <RuleDemoBoard demo={rule.demo} />
            </article>
          );
        })}
      </div>
    </div>
  );
}

export default RulesPage;
