import { API_URL } from "./constants";
import { getAuthHeaders } from "./auth";
import axios from "axios";

export const taskService = {
  async getTasks() {
    const response = await axios.get(`${API_URL}/Chat/requests`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },
  async acceptRequest(requestId) {
    const response = await axios.post(
      `${API_URL}/Chat/requests/${requestId}/accept`,
      {},
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  },
  async rejectRequest(requestId) {
    const response = await axios.delete(
      `${API_URL}/Chat/requests/${requestId}`,
      {
        headers: getAuthHeaders(),
      }
    );
    response.data;
  },
};
