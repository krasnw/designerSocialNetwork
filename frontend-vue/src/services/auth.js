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

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Ошибка авторизации");
      }

      // Сохраняем JWT в localStorage
      localStorage.setItem("JWT", data.token);
      return data;
    } catch (error) {
      throw error;
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
