const BASE_URL = "https://localhost:7006/api/auth";

const readError = async (res, fallbackMessage) => {
  const text = await res.text();

  try {
    const parsed = JSON.parse(text);
    if (typeof parsed === "string" && parsed) {
      return parsed;
    }
  } catch {
    // plain text deci e ok
  }

  return text || fallbackMessage;
};

export const login = async (email, password, rememberMe) => {
  const res = await fetch(`${BASE_URL}/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
      rememberMe,
    }),
  });

  if (!res.ok) throw new Error(await readError(res, "Login failed"));

  return await res.json();
};

export const register = async (email, password, nickname) => {
  const res = await fetch(`${BASE_URL}/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
      nickname,
    }),
  });

  if (!res.ok) throw new Error(await readError(res, "Register failed"));

  return await res.json();
};

export const forgotPassword = async (email) => {
  const res = await fetch(`${BASE_URL}/forgot-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ email }),
  });

  if (!res.ok) throw new Error(await readError(res, "Password reset failed"));

  return await res.json();
};

export const resetPassword = async (token, newPassword) => {
  const res = await fetch(`${BASE_URL}/reset-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      token,
      newPassword,
    }),
  });

  if (!res.ok) throw new Error(await readError(res, "Reset password failed"));
};

export const verifyEmail = async (token) => {
  const res = await fetch(`${BASE_URL}/verify-email`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ token }),
  });

  if (!res.ok) throw new Error(await readError(res, "Email verification failed"));

  return await res.json();
};

export const resendVerification = async (email) => {
  const res = await fetch(`${BASE_URL}/resend-verification`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ email }),
  });

  if (!res.ok) {
    throw new Error(await readError(res, "Could not resend verification email"));
  }

  return await res.json();
};
