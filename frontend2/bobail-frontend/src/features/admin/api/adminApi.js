import { authFetch } from "../../auth/api/authFetch";

const API = "https://localhost:7006/api/admin";

const readApiError = async (res, fallbackMessage) => {
  const text = await res.text();

  try {
    const parsed = JSON.parse(text);

    if (parsed?.error) {
      return parsed.error;
    }

    if (typeof parsed === "string" && parsed) {
      return parsed;
    }
  } catch {
    // Plain text responses are handled below.
  }

  return text || fallbackMessage;
};

export const adminApi = {
  getUsers: async ({ page = 1, pageSize = 25, search = "" } = {}) => {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
    });

    if (search.trim()) {
      params.set("search", search.trim());
    }

    const res = await authFetch(`${API}/users?${params.toString()}`);

    if (!res.ok) {
      throw new Error(await readApiError(res, "Failed to load users."));
    }

    return res.json();
  },

  toggleUserActive: async (userId) => {
    const res = await authFetch(`${API}/users/${userId}/toggle-active`, {
      method: "PATCH",
    });

    if (!res.ok) {
      throw new Error(await readApiError(res, "Failed to update user."));
    }

    return res.json();
  },
};
