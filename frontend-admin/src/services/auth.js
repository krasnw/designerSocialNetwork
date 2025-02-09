import { API_URL } from "./constants";
import axios from "axios";
import { ref } from "vue";

const JSON_HEADERS = { "Content-Type": "application/json" };
const authState = ref(false);

async function fetchAndHandleResponse(endpoint, formData) {
  const response = await axios.post(`${API_URL}${endpoint}`, formData, {
    headers: JSON_HEADERS,
  });

  const token = response.data;
  localStorage.setItem("JWT", token);
  return token;
}

const authService = {
  async login(credentials) {
    const token = await fetchAndHandleResponse(
      "/api/AdminAuth/login",
      credentials
    );
    authState.value = true;
    return token;
  },
  isLogged() {
    const logged =
      localStorage.getItem("JWT") &&
      !isTokenExpired(localStorage.getItem("JWT"));
    authState.value = logged;
    return logged;
  },
  logout() {
    localStorage.removeItem("JWT");
    authState.value = false;
  },
  authState,
};

export default authService;

const isTokenExpired = (token) => {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
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
