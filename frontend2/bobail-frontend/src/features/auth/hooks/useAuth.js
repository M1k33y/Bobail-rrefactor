import { useEffect, useState } from "react";
import { login } from "../api/authApi";
import {
  AUTH_SESSION_CHANGED,
  clearStoredToken,
  getStoredNickname,
  getStoredRole,
  getStoredToken,
  getStoredUserId,
  storeAuthSession,
} from "../utils/authStorage";

export const useAuth = () => {
  const [isAuth, setIsAuth] = useState(!!getStoredToken());
  const [nickname, setNickname] = useState(getStoredNickname());
  const [role, setRole] = useState(getStoredRole());
  const [userId, setUserId] = useState(getStoredUserId());

  useEffect(() => {
    const syncAuthState = () => {
      setIsAuth(!!getStoredToken());
      setNickname(getStoredNickname());
      setRole(getStoredRole());
      setUserId(getStoredUserId());
    };

    window.addEventListener(AUTH_SESSION_CHANGED, syncAuthState);
    window.addEventListener("storage", syncAuthState);

    return () => {
      window.removeEventListener(AUTH_SESSION_CHANGED, syncAuthState);
      window.removeEventListener("storage", syncAuthState);
    };
  }, []);

  const loginUser = async (email, password, rememberMe) => {
    const response = await login(email, password, rememberMe);
    storeAuthSession({
      token: response.token,
      nickname: response.nickname,
      role: response.role,
      userId: response.userId,
      rememberMe,
    });
    setNickname(response.nickname || "");
    setRole(getStoredRole());
    setUserId(getStoredUserId());
    setIsAuth(true);
  };

  const logout = () => {
    clearStoredToken();
    setNickname("");
    setRole("");
    setUserId("");
    setIsAuth(false);
  };

  return {
    loginUser,
    logout,
    isAuthenticated: isAuth,
    nickname,
    role,
    userId,
    isAdmin: role === "Admin",
  };
};
