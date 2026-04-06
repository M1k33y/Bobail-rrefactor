const TOKEN_KEY = "token";
const NICKNAME_KEY = "nickname";

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

export const storeAuthSession = ({ token, nickname, rememberMe }) => {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(NICKNAME_KEY);
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(NICKNAME_KEY);

  const storage = rememberMe ? localStorage : sessionStorage;
  storage.setItem(TOKEN_KEY, token);
  storage.setItem(NICKNAME_KEY, nickname || "");
};

export const clearStoredToken = () => {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(NICKNAME_KEY);
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(NICKNAME_KEY);
};
