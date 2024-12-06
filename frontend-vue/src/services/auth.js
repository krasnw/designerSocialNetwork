const API_URL = "http://localhost:8088";

export const authService = {
  async login(credentials) {
    try {
      const response = await fetch(`${API_URL}/Auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        throw new Error("Nieprawidłowa nazwa użytkownika lub hasło");
      }

      const token = await response.text();
      localStorage.setItem("JWT", token);
      return token;
    } catch (error) {
      throw new Error("Błąd podczas logowania: " + error.message);
    }
  },

  async register(userData) {
    try {
      const response = await fetch(`${API_URL}/Auth/signup`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(userData),
      });

      if (!response.ok) {
        throw new Error("Błąd podczas rejestracji");
      }

      const token = await response.text();
      localStorage.setItem("JWT", token);
      return token;
    } catch (error) {
      throw new Error("Błąd podczas rejestracji: " + error.message);
    }
  },
};

// Interceptor для добавления токена к запросам
export const authInterceptor = {
  request(config) {
    const token = localStorage.getItem("JWT");
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    }
    return config;
  },
};

export const getAuthHeaders = () => {
  const token = localStorage.getItem("JWT");
  return {
    "Content-Type": "application/json",
    Authorization: token ? `Bearer ${token}` : "",
  };
};
