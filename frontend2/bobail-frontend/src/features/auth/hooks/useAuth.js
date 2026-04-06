import { useState } from "react";
import { login } from "../api/authApi";
import {
  clearStoredToken,
  getStoredToken,
  storeToken,
} from "../utils/authStorage";

export const useAuth = () => {
  const [isAuth, setIsAuth] = useState(!!getStoredToken());

  const loginUser = async (email, password, rememberMe) => {
    const response = await login(email, password, rememberMe);
    storeToken(response.token, rememberMe);
    setIsAuth(true);
  };

  const logout = () => {
    clearStoredToken();
    setIsAuth(false);
  };

  return { loginUser, logout, isAuthenticated: isAuth };
};
