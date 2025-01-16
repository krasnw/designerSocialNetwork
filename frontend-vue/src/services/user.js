import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";

const CACHE_NAME = "user-cache-v1";
const CACHE_KEY = "/api/user-profile";
const CACHE_DURATION = 30 * 60 * 1000;

// Error messages
const ERROR_MESSAGES = {
  PROFILE_FETCH_FAILED: "Unable to get profile data.",
  PROFILE_FETCH_ERROR: "Occurred error while fetching profile data:",
  EMPTY_USERNAME: "Username field is empty.",
  USER_NOT_FOUND: "There is no user with this username",
  REQUEST_FAILED: "Unable to send request.",
  REQUEST_ERROR: "Occurred error while sending request:",
};

async function fetchUserData(endpoint) {
  const response = await fetch(`${API_URL}${endpoint}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return response;
}

// API error handler
function handleFetchError(response, defaultErrorMessage) {
  if (response.status === 404) {
    throw new Error(ERROR_MESSAGES.USER_NOT_FOUND);
  }
  if (!response.ok) {
    throw new Error(defaultErrorMessage);
  }
}

export const userService = {
  async getMyData(forceRefresh = false) {
    try {
      if (!forceRefresh) {
        const cached = await this.getFromCache();
        if (cached) return cached;
      }

      const response = await fetchUserData("/User/profile/me");
      handleFetchError(response, ERROR_MESSAGES.PROFILE_FETCH_FAILED);
      const userData = await response.json();

      await this.saveToCache(userData);
      return userData;
    } catch (error) {
      throw new Error(`${ERROR_MESSAGES.PROFILE_FETCH_ERROR} ${error.message}`);
    }
  },

  async getFromCache() {
    try {
      const cache = await caches.open(CACHE_NAME);
      const response = await cache.match(CACHE_KEY);

      if (!response) return null;

      const { timestamp, data } = await response.json();
      return Date.now() - timestamp > CACHE_DURATION ? null : data;
    } catch {
      return null;
    }
  },

  async saveToCache(data) {
    try {
      const cache = await caches.open(CACHE_NAME);
      const cacheData = {
        timestamp: Date.now(),
        data,
      };
      await cache.put(CACHE_KEY, new Response(JSON.stringify(cacheData)));
    } catch (error) {
      console.error("Failed to cache user data:", error);
    }
  },

  async clearCache() {
    const cache = await caches.open(CACHE_NAME);
    await cache.delete(CACHE_KEY);
  },

  async getUserData(username) {
    try {
      if (!username) {
        throw new Error(ERROR_MESSAGES.EMPTY_USERNAME);
      }
      const response = await fetchUserData(`/User/profile/${username}`);
      handleFetchError(response, ERROR_MESSAGES.PROFILE_FETCH_FAILED);
      return await response.json();
    } catch (error) {
      if (error.message === ERROR_MESSAGES.USER_NOT_FOUND) {
        throw error; // Case for 404 res code status
      }
      throw new Error(`${ERROR_MESSAGES.PROFILE_FETCH_ERROR} ${error.message}`);
    }
  },

  async sendRequest(receiver, description) {
    try {
      const requestData = {
        receiver: receiver.toString(),
        description: description.toString(),
      };
      const response = await fetch(`${API_URL}/Chat/sendRequest`, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify(requestData),
      });

      if (!response.ok) {
        throw new Error(ERROR_MESSAGES.REQUEST_FAILED);
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        return await response.json();
      }

      return { success: true };
    } catch (error) {
      throw new Error(`${ERROR_MESSAGES.REQUEST_ERROR} ${error.message}`);
    }
  },
};
