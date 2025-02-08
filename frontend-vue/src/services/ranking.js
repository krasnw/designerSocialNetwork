import { API_URL } from "./constants";
import { getAuthHeaders } from "./auth";

export const ITEMS_PER_PAGE = 10;

export const rankingService = {
  async getRanking(offset = 0) {
    const response = await fetch(
      `${API_URL}/Rating?limit=${ITEMS_PER_PAGE}&offset=${offset}`,
      {
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
        },
      }
    ).catch((error) => {
      console.error("Error:", error);
      throw new Error("Failed to fetch ranking");
    });

    return await response.json();
  },
};
