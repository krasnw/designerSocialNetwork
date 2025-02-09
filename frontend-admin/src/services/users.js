import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import axios from "axios";

const userReportService = {
  async fetchUserReport() {
    const response = await axios.get(`${API_URL}/api/ReportHandler/users`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },
  async toggleFrozenUser(username) {
    const response = await axios.post(
      `${API_URL}/api/ReportHandler/users/${username}/toggle-freeze`,
      {},
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },
  async dismissUser(userReportId) {
    const response = await axios.post(
      `${API_URL}/api/ReportHandler/users/${userReportId}/dismiss`,
      {},
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },
  async getFrozenUsers() {
    const response = await axios.get(
      `${API_URL}/api/ReportHandler/users/frozen`,
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },
};

export default userReportService;
