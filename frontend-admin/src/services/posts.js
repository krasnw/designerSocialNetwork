import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import axios from "axios";

export const postReportService = {
  async fetchPostReport() {
    const response = await axios.get(`${API_URL}/api/ReportHandler/posts`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },
  async deletePost(id) {
    const response = await axios.delete(
      `${API_URL}/api/ReportHandler/posts/${id}`,
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },
  async dismissPost(postReportId) {
    const response = await axios.post(
      `${API_URL}/api/ReportHandler/posts/${postReportId}/dismiss`,
      {},
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },
};
