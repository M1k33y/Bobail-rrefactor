let gameId = null;
let selected = null;
let currentGame = null;

const boardDiv = document.getElementById("board");

document.getElementById("createGame").onclick = async () => {
    const response = await fetch("https://localhost:7006/api/games", {
        method: "POST"
    });

    const data = await response.json();
    gameId = data.gameId;

    selected = null;
    clearHighlights();
    await loadGame();
};

async function loadGame() {
    const response = await fetch(`https://localhost:7006/api/games/${gameId}`);
    const game = await response.json();

    currentGame = game;

    console.log("Game state:", game);

    renderBoard(game);
}

function renderBoard(game) {
    boardDiv.innerHTML = "";

    for (let row = 0; row < 5; row++) {
        for (let col = 0; col < 5; col++) {

            const cell = document.createElement("div");
            cell.className = "cell";
            cell.dataset.row = row;
            cell.dataset.col = col;

            const piece = game.pieces.find(p => p.row === row && p.column === col);

            if (piece) {
                cell.dataset.type = piece.type;

                if (piece.type === "Red") cell.classList.add("red");
                if (piece.type === "Green") cell.classList.add("green");
                if (piece.type === "Bobail") cell.classList.add("bobail");
            }

            cell.onclick = () => handleClick(row, col);
            boardDiv.appendChild(cell);
        }
    }
}

async function handleClick(row, col) {

    if (!currentGame || currentGame.status !== "InProgress")
        return;

    const clickedCell = document.querySelector(
        `[data-row='${row}'][data-col='${col}']`
    );

    const clickedType = clickedCell?.dataset.type;

    // -------------------------
    // SELECT PHASE
    // -------------------------
    if (!selected) {

        if (!clickedType)
            return;

        // blocăm piesa adversarului
        if (currentGame.currentPhase === "PlayerMoveRequired" &&
            clickedType !== currentGame.currentTurn)
            return;

        if (currentGame.currentPhase === "BobailMoveRequired" &&
            clickedType !== "Bobail")
            return;

        let moves = [];

        if (currentGame.currentPhase === "PlayerMoveRequired") {
            const response = await fetch(
                `https://localhost:7006/api/games/${gameId}/valid-player-moves?row=${row}&col=${col}`
            );
            moves = await response.json();
        }

        if (currentGame.currentPhase === "BobailMoveRequired") {
            const response = await fetch(
                `https://localhost:7006/api/games/${gameId}/valid-bobail-moves`
            );
            moves = await response.json();
        }

        if (!moves || moves.length === 0)
            return;

        selected = { row, col };

        clearHighlights();
        highlightSelected(row, col);

        moves.forEach(m => {
            const cell = document.querySelector(
                `[data-row='${m.row}'][data-col='${m.column}']`
            );
            if (cell)
                cell.classList.add("valid-move");
        });

        return;
    }

    // -------------------------
    // RE-SELECTION
    // -------------------------
    if (clickedType &&
        clickedType === currentGame.currentTurn &&
        currentGame.currentPhase === "PlayerMoveRequired") {

        selected = { row, col };

        clearHighlights();
        highlightSelected(row, col);
        await showValidMoves(row, col);
        return;
    }

    // -------------------------
    // MOVE EXECUTION
    // -------------------------
    try {

        if (currentGame.currentPhase === "BobailMoveRequired") {

            const response = await fetch(
                `https://localhost:7006/api/games/${gameId}/bobail-move`,
                {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        toRow: row,
                        toColumn: col
                    })
                });

            if (!response.ok) {
                const error = await response.json();
                alert(error.error);
                selected = null;
                clearHighlights();
                return;
            }

        } else {

            const response = await fetch(
                `https://localhost:7006/api/games/${gameId}/player-move`,
                {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        fromRow: selected.row,
                        fromColumn: selected.col,
                        toRow: row,
                        toColumn: col
                    })
                });

            if (!response.ok) {
                const error = await response.json();
                alert(error.error);
                selected = null;
                clearHighlights();
                return;
            }
        }

        selected = null;
        clearHighlights();
        await loadGame();

    } catch (err) {
        console.error(err);
        selected = null;
        clearHighlights();
    }
}

// ----------------------------------
// HELPERS
// ----------------------------------

function highlightSelected(row, col) {
    const cell = document.querySelector(
        `[data-row='${row}'][data-col='${col}']`
    );

    if (cell)
        cell.classList.add("selected");
}

async function showValidMoves(row, col) {

    const response = await fetch(
        `https://localhost:7006/api/games/${gameId}/valid-player-moves?row=${row}&col=${col}`
    );

    const moves = await response.json();

    moves.forEach(m => {
        const cell = document.querySelector(
            `[data-row='${m.row}'][data-col='${m.column}']`
        );

        if (cell)
            cell.classList.add("valid-move");
    });
}

function clearHighlights() {
    document.querySelectorAll(".cell").forEach(c => {
        c.classList.remove("selected");
        c.classList.remove("valid-move");
    });
}
