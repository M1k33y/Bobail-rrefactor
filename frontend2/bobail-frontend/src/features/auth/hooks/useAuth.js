import { useState } from "react";
import { login } from "../api/authApi";
import {
  clearStoredToken,
  getStoredNickname,
  getStoredToken,
  storeAuthSession,
} from "../utils/authStorage";

export const useAuth = () => {
  const [isAuth, setIsAuth] = useState(!!getStoredToken());
  const [nickname, setNickname] = useState(getStoredNickname());

  const loginUser = async (email, password, rememberMe) => {
    const response = await login(email, password, rememberMe);
    storeAuthSession({
      token: response.token,
      nickname: response.nickname,
      rememberMe,
    });
    setNickname(response.nickname || "");
    setIsAuth(true);
  };

  const logout = () => {
    clearStoredToken();
    setNickname("");
    setIsAuth(false);
  };

  return { loginUser, logout, isAuthenticated: isAuth, nickname };
};
