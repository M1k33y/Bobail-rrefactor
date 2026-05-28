const API = "https://localhost:7006/api/games";

import { authFetch } from "../../auth/api/authFetch";
export const gameApi = {
    create: async () => {
        const res = await authFetch(API, { method: "POST" });
        return res.json();
    },

    createOnline: async () => {
        const res = await authFetch(`${API}/online`, { method: "POST" });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || "Failed to create online game.");
        }

        return res.json();
    },

    getCurrentOnline: async () => {
        const res = await authFetch(`${API}/online/current`);

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || "Failed to load active online game.");
        }

        return res.json();
    },

    joinOnline: async (gameId) => {
        const res = await authFetch(`${API}/${gameId}/join-online`, {
            method: "POST"
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || "Failed to join online game.");
        }

        return res.json();
    },

    get: async (gameId) => {
        const res = await authFetch(`${API}/${gameId}`);
        return res.json();
    },

    getHistory: async (page = 1, pageSize = 50) => {
        const res = await authFetch(`${API}/history?page=${page}&pageSize=${pageSize}`);

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || "Failed to load history.");
        }

        return res.json();
    },

    getUserStats: async () => {
        const res = await authFetch(`${API}/user-stats`);

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || "Failed to load stats.");
        }

        return res.json();
    },

    getReplay: async (gameId) => {
        const res = await authFetch(`${API}/${gameId}/replay`);

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || "Failed to load replay.");
        }

        return res.json();
    },

    playerMove: async (gameId, payload) => {
        const res = await authFetch(`${API}/${gameId}/player-move`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error);
        }

        return;
    },

    getValidPlayerMoves: async (gameId, row, col) => {
        const res = await authFetch(
            `${API}/${gameId}/valid-player-moves?row=${row}&col=${col}`
        );
        return res.json();
    },

    getValidBobailMoves: async (gameId) => {
        const res = await authFetch(
            `${API}/${gameId}/valid-bobail-moves`
        );
        return res.json();
    },

    bobailMove: async (gameId, payload) => {
        const res = await authFetch(`${API}/${gameId}/bobail-move`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error);
        }

        return;
    },

    resign: async (gameId) => {
        const res = await authFetch(`${API}/${gameId}/resign`, {
            method: "POST"
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || "Failed to resign game.");
        }

        return res.json();
    }
};
