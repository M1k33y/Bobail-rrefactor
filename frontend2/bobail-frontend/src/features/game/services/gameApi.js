const API = "https://localhost:7006/api/games";

export const gameApi = {
    create: async () => {
        const res = await fetch(API, { method: "POST" });
        return res.json();
    },

    get: async (gameId) => {
        const res = await fetch(`${API}/${gameId}`);
        return res.json();
    },

    playerMove: async (gameId, payload) =>
        fetch(`${API}/${gameId}/player-move`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        }),

    getValidPlayerMoves: async (gameId, row, col) => {
        const res = await fetch(
            `${API}/${gameId}/valid-player-moves?row=${row}&col=${col}`
        );
        return res.json();
    },

    getValidBobailMoves: async (gameId) => {
        const res = await fetch(
            `${API}/${gameId}/valid-bobail-moves`
        );
        return res.json();
    },

    bobailMove: async (gameId, payload) =>
        fetch(`${API}/${gameId}/bobail-move`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        })
};