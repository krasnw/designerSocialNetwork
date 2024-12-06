import { getAuthHeaders } from "./auth";
const API_URL = "http://localhost:8088";

export const userService = {
  async getMyData() {
    try {
      const response = await fetch(`${API_URL}/User/profile/me`, {
        method: "GET",
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        throw new Error("Не удалось получить данные профиля");
      }

      return await response.json();
    } catch (error) {
      throw new Error("Ошибка при получении данных профиля: " + error.message);
    }
  },

  async getUserData(username) {
    try {
      console.log("Getting data for username:", username);

      if (!username) {
        throw new Error("Username не может быть пустым");
      }

      const response = await fetch(`${API_URL}/User/profile/${username}`, {
        method: "GET",
        headers: getAuthHeaders(),
      });

      console.log("Response status:", response.status);

      if (response.status === 404) {
        throw new Error("404");
      }

      if (!response.ok) {
        throw new Error("Не удалось получить данные профиля");
      }

      const data = await response.json();
      console.log("Received data:", data);
      return data;
    } catch (error) {
      console.error("Service error:", error);
      if (error.message === "404") {
        throw error;
      }
      throw new Error("Ошибка при получении данных профиля: " + error.message);
    }
  },
};
