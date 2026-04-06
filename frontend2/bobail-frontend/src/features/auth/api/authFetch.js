import { getStoredToken } from "../utils/authStorage";

export const authFetch = async (url, options = {}) => {
  const token = getStoredToken();

  return fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      Authorization: `Bearer ${token}`,
    },
  });
};
