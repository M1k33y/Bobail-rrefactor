const BASE_URL = "https://localhost:7006/api/auth";

export const login = async (email, password) => {
  const res = await fetch(
    `${BASE_URL}/login?email=${email}&password=${password}`,
    { method: "POST" }
  );

  if (!res.ok) throw new Error("Login failed");

  return await res.text();
};

export const register = async (email, password) => {
  const res = await fetch(`${BASE_URL}/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
    }),
  });

  if (!res.ok) throw new Error("Register failed");

  return res;
};  