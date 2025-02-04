import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import axios from "axios";

const clearProfileCache = async () => {
  const cacheStorage = await caches.open("user-cache-v1");
  await cacheStorage.delete("/api/user-profile");
};

export const subscribeService = {
  async getSubscriptionState(username) {
    const response = await axios
      .get(`${API_URL}/Subscription/check?sellerUsername=${username}`, {
        headers: getAuthHeaders(),
      })
      .catch((error) => {
        throw new Error(`Error checking subscription state: ${error.message}`);
      });
    return response.data;
  },
  async getAccessFee(username) {
    const response = await axios
      .get(`${API_URL}/Subscription/fee?sellerUsername=${username}`, {
        headers: getAuthHeaders(),
      })
      .catch((error) => {
        throw new Error(`Error fetching access fee: ${error.message}`);
      });
    return response.data;
  },
  async subscribe(username) {
    try {
      const response = await axios.post(
        `${API_URL}/Subscription/buy?sellerUsername=${username}`,
        null,
        { headers: getAuthHeaders() }
      );
      await clearProfileCache();
      return response.data;
    } catch (error) {
      if (error.response) {
        // Если есть ответ от сервера с ошибкой
        throw {
          status: error.response.status,
          data: error.response.data,
        };
      }
      // Если это сетевая ошибка или что-то другое
      throw {
        status: 500,
        data: error.message,
      };
    }
  },
  async cancelSubscription(username) {
    const response = await axios
      .delete(`${API_URL}/Subscription/cancel?sellerUsername=${username}`, {
        headers: getAuthHeaders(),
      })
      .catch((error) => {
        throw new Error(`Error cancelling subscription: ${error.message}`);
      });
    return response.data;
  },
};
