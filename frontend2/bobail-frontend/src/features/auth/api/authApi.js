const BASE_URL = "https://localhost:7006/api/auth";

export const login = async (email, password) => {
  const res = await fetch(`${BASE_URL}/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
    }),
  });

  if (!res.ok) throw new Error("Login failed");

  return await res.text();
};

export const register = async (email, password,nickname) => {
  const res = await fetch(`${BASE_URL}/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
      nickname
    }),
  });

  if (!res.ok) throw new Error("Register failed");

  return res;
};  