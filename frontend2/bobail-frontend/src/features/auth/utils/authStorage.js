const TOKEN_KEY = "token";
const NICKNAME_KEY = "nickname";
const ROLE_KEY = "role";
const USER_ID_KEY = "userId";
const ROLE_CLAIM_KEYS = [
  "role",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
];
const USER_ID_CLAIM_KEYS = [
  "nameid",
  "sub",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
];

export const AUTH_SESSION_CHANGED = "auth-session-changed";

const getStorage = () => {
  if (localStorage.getItem(TOKEN_KEY)) {
    return localStorage;
  }

  if (sessionStorage.getItem(TOKEN_KEY)) {
    return sessionStorage;
  }

  return null;
};

export const getStoredToken = () =>
  localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);

export const getStoredNickname = () => {
  const storage = getStorage();
  return storage?.getItem(NICKNAME_KEY) || "";
};

export const getStoredRole = () => {
  const storage = getStorage();
  const storedRole = storage?.getItem(ROLE_KEY);

  if (storedRole) {
    return normalizeRole(storedRole);
  }

  return normalizeRole(getClaimValue(parseJwtPayload(getStoredToken()), ROLE_CLAIM_KEYS));
};

export const getStoredUserId = () => {
  const storage = getStorage();
  const storedUserId = storage?.getItem(USER_ID_KEY);

  if (storedUserId) {
    return storedUserId;
  }

  return getClaimValue(parseJwtPayload(getStoredToken()), USER_ID_CLAIM_KEYS) || "";
};

export const storeAuthSession = ({ token, nickname, role, userId, rememberMe }) => {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(NICKNAME_KEY);
  sessionStorage.removeItem(ROLE_KEY);
  sessionStorage.removeItem(USER_ID_KEY);
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(NICKNAME_KEY);
  localStorage.removeItem(ROLE_KEY);
  localStorage.removeItem(USER_ID_KEY);

  const storage = rememberMe ? localStorage : sessionStorage;
  const payload = parseJwtPayload(token);
  const resolvedRole = normalizeRole(role || getClaimValue(payload, ROLE_CLAIM_KEYS));
  const resolvedUserId = userId || getClaimValue(payload, USER_ID_CLAIM_KEYS) || "";

  storage.setItem(TOKEN_KEY, token);
  storage.setItem(NICKNAME_KEY, nickname || "");
  storage.setItem(ROLE_KEY, resolvedRole);
  storage.setItem(USER_ID_KEY, resolvedUserId);
  notifyAuthChanged();
};

export const clearStoredToken = () => {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(NICKNAME_KEY);
  localStorage.removeItem(ROLE_KEY);
  localStorage.removeItem(USER_ID_KEY);
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(NICKNAME_KEY);
  sessionStorage.removeItem(ROLE_KEY);
  sessionStorage.removeItem(USER_ID_KEY);
  notifyAuthChanged();
};

function parseJwtPayload(token) {
  if (!token) {
    return null;
  }

  try {
    const payload = token.split(".")[1];
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const paddedBase64 = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
    return JSON.parse(atob(paddedBase64));
  } catch {
    return null;
  }
}

function getClaimValue(payload, keys) {
  if (!payload) {
    return "";
  }

  for (const key of keys) {
    const value = payload[key];

    if (Array.isArray(value)) {
      return value[0] || "";
    }

    if (value) {
      return value;
    }
  }

  return "";
}

function normalizeRole(role) {
  const value = String(role || "").toLowerCase();

  if (value === "1" || value === "admin") {
    return "Admin";
  }

  if (value === "0" || value === "user") {
    return "User";
  }

  return String(role || "");
}

function notifyAuthChanged() {
  window.dispatchEvent(new Event(AUTH_SESSION_CHANGED));
}
