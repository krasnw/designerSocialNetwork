import { API_URL } from "./constants";
const JSON_HEADERS = { "Content-Type": "application/json" };

async function fetchAndHandleResponse(endpoint, method, requestBody) {
  const apiResponse = await fetch(`${API_URL}${endpoint}`, {
    method,
    headers: JSON_HEADERS,
    body: JSON.stringify(requestBody),
  });

  if (!apiResponse.ok) {
    const errorMessage = await apiResponse.text();
    throw new Error(errorMessage || "Request failed");
  }

  const token = await apiResponse.text();
  localStorage.setItem("JWT", token);
  return token;
}

export const authService = {
  async login(credentials) {
    try {
      return await fetchAndHandleResponse("/Auth/login", "POST", credentials);
    } catch (error) {
      throw new Error("Błąd podczas logowania: " + error.message);
    }
  },

  async register(userData) {
    try {
      return await fetchAndHandleResponse("/Auth/signup", "POST", userData);
    } catch (error) {
      throw new Error("Błąd podczas rejestracji: " + error.message);
    }
  },
};

const isTokenExpired = (token) => {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    // Convert expiration time from seconds to milliseconds and compare with current time
    const expirationTimeMs = payload.exp * 1000;
    const currentTimeMs = Date.now();
    return expirationTimeMs < currentTimeMs;
  } catch {
    return true;
  }
};

export const getAuthHeaders = () => {
  const token = localStorage.getItem("JWT");
  if (token && isTokenExpired(token)) {
    localStorage.removeItem("JWT");
    return JSON_HEADERS;
  }
  return {
    ...JSON_HEADERS,
    Authorization: token ? `Bearer ${token}` : "",
  };
};
