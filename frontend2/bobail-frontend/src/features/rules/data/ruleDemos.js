const cell = (row, column) => ({ row, column });
const selected = (row, column) => ({ row, column, col: column });

const piece = (id, type, row, column, owner = null) => ({
  id,
  type,
  owner,
  row,
  column
});

const redPiece = (id, row, column) =>
  piece(id, "PlayerPiece", row, column, "Red");

const greenPiece = (id, row, column) =>
  piece(id, "PlayerPiece", row, column, "Green");

const bobail = (row, column) =>
  piece("bobail", "Bobail", row, column);

const movePiece = (pieces, id, row, column) =>
  pieces.map((item) =>
    item.id === id
      ? { ...item, row, column }
      : item
  );

const rowHighlights = (row, type) =>
  Array.from({ length: 5 }, (_, column) => ({ row, column, type }));

const initialSetup = [
  redPiece("red-0", 0, 0),
  redPiece("red-1", 0, 1),
  redPiece("red-2", 0, 2),
  redPiece("red-3", 0, 3),
  redPiece("red-4", 0, 4),
  greenPiece("green-0", 4, 0),
  greenPiece("green-1", 4, 1),
  greenPiece("green-2", 4, 2),
  greenPiece("green-3", 4, 3),
  greenPiece("green-4", 4, 4),
  bobail(2, 2)
];

const overviewStart = [
  redPiece("red-a", 0, 1),
  redPiece("red-b", 0, 3),
  greenPiece("green-a", 4, 1),
  greenPiece("green-b", 4, 3),
  bobail(2, 2)
];

const overviewMoved = movePiece(overviewStart, "bobail", 2, 3);

const movementStart = [
  redPiece("red-runner", 3, 0),
  redPiece("red-anchor", 4, 1),
  greenPiece("green-blocker", 1, 4),
  bobail(2, 2)
];

const movementMoved = movePiece(movementStart, "red-runner", 3, 4);

const bobailStart = [
  redPiece("red-left", 1, 1),
  redPiece("red-low", 2, 1),
  greenPiece("green-low", 3, 2),
  greenPiece("green-right", 3, 3),
  bobail(2, 2)
];

const bobailMoved = movePiece(bobailStart, "bobail", 1, 3);

const blockedStart = [
  redPiece("red-slider", 3, 0),
  greenPiece("green-wall", 3, 3),
  greenPiece("green-corner", 0, 4),
  bobail(1, 1)
];

const blockedMoved = movePiece(blockedStart, "red-slider", 3, 2);

const turnStart = [
  redPiece("red-stop", 0, 0),
  redPiece("red-top", 0, 2),
  greenPiece("green-turn", 4, 4),
  greenPiece("green-side", 4, 0),
  bobail(2, 2)
];

const turnAfterBobail = movePiece(turnStart, "bobail", 2, 3);
const turnAfterPlayer = movePiece(turnAfterBobail, "green-turn", 1, 1);

const victoryStart = [
  redPiece("red-left", 0, 0),
  redPiece("red-right", 0, 4),
  greenPiece("green-left", 4, 1),
  greenPiece("green-right", 4, 3),
  bobail(1, 2)
];

const victoryMoved = movePiece(victoryStart, "bobail", 0, 2);

export const ruleDemos = {
  overview: {
    id: "overview",
    frames: [
      {
        pieces: overviewStart,
        selected: selected(2, 2),
        validMoves: [cell(2, 3)],
        highlights: [{ row: 2, column: 3, type: "destination" }],
        caption: "Both players share the neutral Bobail.",
        duration: 1500
      },
      {
        pieces: overviewMoved,
        selected: selected(2, 3),
        validMoves: [],
        highlights: [{ row: 2, column: 3, type: "soft" }],
        caption: "It moves on the same board as the player tokens.",
        duration: 2000
      }
    ]
  },
  movement: {
    id: "movement",
    frames: [
      {
        pieces: movementStart,
        selected: selected(3, 0),
        validMoves: [cell(3, 4), cell(0, 0), cell(0, 3)],
        highlights: [{ row: 3, column: 4, type: "destination" }],
        caption: "Player tokens choose a direction.",
        duration: 1500
      },
      {
        pieces: movementMoved,
        selected: selected(3, 4),
        validMoves: [],
        highlights: [{ row: 3, column: 4, type: "destination" }],
        caption: "They slide to the farthest open square.",
        duration: 2000
      }
    ]
  },
  setup: {
    id: "setup",
    frames: [
      {
        pieces: initialSetup,
        selected: null,
        validMoves: [],
        highlights: [
          ...rowHighlights(0, "red-home"),
          ...rowHighlights(4, "green-home")
        ],
        caption: "Red and Green begin on their home rows.",
        duration: 2000
      },
      {
        pieces: initialSetup,
        selected: selected(2, 2),
        validMoves: [],
        highlights: [{ row: 2, column: 2, type: "win" }],
        caption: "The Bobail starts in the center.",
        duration: 2000
      }
    ]
  },
  bobail: {
    id: "bobail",
    frames: [
      {
        pieces: bobailStart,
        selected: selected(2, 2),
        validMoves: [cell(1, 2), cell(1, 3), cell(2, 3), cell(3, 1)],
        highlights: [{ row: 1, column: 3, type: "destination" }],
        caption: "The Bobail only steps to an adjacent open cell.",
        duration: 1500
      },
      {
        pieces: bobailMoved,
        selected: selected(1, 3),
        validMoves: [],
        highlights: [{ row: 1, column: 3, type: "destination" }],
        caption: "One square is the whole Bobail move.",
        duration: 2000
      }
    ]
  },
  legal: {
    id: "legal",
    frames: [
      {
        pieces: blockedStart,
        selected: selected(3, 0),
        validMoves: [cell(3, 2)],
        highlights: [
          { row: 3, column: 2, type: "destination" },
          { row: 3, column: 4, type: "danger" }
        ],
        caption: "A blocker stops the line of travel.",
        duration: 1500
      },
      {
        pieces: blockedMoved,
        selected: selected(3, 2),
        validMoves: [],
        highlights: [
          { row: 3, column: 2, type: "destination" },
          { row: 3, column: 4, type: "danger" }
        ],
        caption: "The square beyond the blocker is illegal.",
        duration: 2000
      }
    ]
  },
  turn: {
    id: "turn",
    frames: [
      {
        pieces: turnStart,
        selected: selected(2, 2),
        validMoves: [cell(2, 3)],
        highlights: [{ row: 2, column: 3, type: "destination" }],
        caption: "After the first turn, move the Bobail first.",
        duration: 1500
      },
      {
        pieces: turnAfterBobail,
        selected: selected(4, 4),
        validMoves: [cell(1, 1)],
        highlights: [{ row: 1, column: 1, type: "destination" }],
        caption: "Then move one token for the current player.",
        duration: 1700
      },
      {
        pieces: turnAfterPlayer,
        selected: selected(1, 1),
        validMoves: [],
        highlights: [{ row: 1, column: 1, type: "destination" }],
        caption: "The turn is complete.",
        duration: 1200
      }
    ]
  },
  victory: {
    id: "victory",
    frames: [
      {
        pieces: victoryStart,
        selected: selected(1, 2),
        validMoves: [cell(0, 2)],
        highlights: [
          ...rowHighlights(0, "red-home"),
          { row: 0, column: 2, type: "win" }
        ],
        caption: "Guide the Bobail onto your home row.",
        duration: 1500
      },
      {
        pieces: victoryMoved,
        selected: selected(0, 2),
        validMoves: [],
        highlights: [
          ...rowHighlights(0, "red-home"),
          { row: 0, column: 2, type: "win" }
        ],
        caption: "Red wins when the Bobail reaches the top row.",
        duration: 2000
      }
    ]
  }
};
