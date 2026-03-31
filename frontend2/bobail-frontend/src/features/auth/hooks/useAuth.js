import { useState, useEffect } from "react";
import { login } from "../api/authApi";

export const useAuth = () => {
  const [isAuth, setIsAuth] = useState(!!localStorage.getItem("token"));

  const loginUser = async (email, password) => {
    const token = await login(email, password);
    localStorage.setItem("token", token);
    setIsAuth(true); 
  };

  const logout = () => {
    localStorage.removeItem("token");
    setIsAuth(false); 
  };

  return { loginUser, logout, isAuthenticated: isAuth };
};