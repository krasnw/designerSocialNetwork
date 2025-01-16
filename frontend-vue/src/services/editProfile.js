import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";

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

  async updateUserProfile(profileData) {
    try {
      const response = await fetch(`${API_URL}/User/profile/me/edit`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(profileData),
      });

      if (!response.ok) {
        throw new Error(await response.text());
      }
      return true; // Return success flag instead of parsing JSON
    } catch (error) {
      throw new Error(`Error updating profile: ${error.message}`);
    }
  },
};
