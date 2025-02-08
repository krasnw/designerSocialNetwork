import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import axios from "axios";

const clearProfileCache = async () => {
  const cacheStorage = await caches.open("user-cache-v1");
  await cacheStorage.delete("/api/user-profile");
};

const updateProfile = async (endpoint, formData, errorMessage) => {
  const contentType =
    endpoint === "basic" ? "multipart/form-data" : "application/json";
  const headers = {
    ...getAuthHeaders(),
    "Content-Type": contentType,
  };

  try {
    const response = await axios.put(
      `${API_URL}/User/profile/me/edit/${endpoint}`,
      formData,
      { headers }
    );
    await clearProfileCache();
    return response.data;
  } catch (error) {
    if (error.response) {
      throw error.response.data;
    }
    throw new Error(`${errorMessage}: ${error.message}`);
  }
};

export const editProfileService = {
  async getUserProfile() {
    try {
      const response = await fetch(`${API_URL}/User/profile/me/edit`, {
        method: "GET",
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        throw new Error(await response.text());
      }

      return await response.json();
    } catch (error) {
      throw new Error(`Error fetching profile: ${error.message}`);
    }
  },

  async updateBasicUserInfo(formData) {
    return updateProfile("basic", formData, "Error updating basic user info");
  },

  async updateSensitiveUserInfo(formData) {
    return updateProfile(
      "sensitive",
      formData,
      "Error updating sensitive user info"
    );
  },
};
