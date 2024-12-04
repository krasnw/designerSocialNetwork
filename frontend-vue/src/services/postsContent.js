import { getAuthHeaders } from "./auth";
const API_URL = "http://localhost:8088";

export const postsContentService = {
  async getFeedPosts(
    pageNumber = 1,
    pageSize = null,
    tags = null,
    accessType = null
  ) {
    const defaultPageSize = localStorage.getItem("postsPerRequest") || 3;
    const params = new URLSearchParams({
      pageNumber: pageNumber,
      pageSize: pageSize || defaultPageSize,
    });

    if (tags) params.append("tags", tags);
    if (accessType) params.append("accessType", accessType);

    const response = await fetch(`${API_URL}/Post/feed?${params}`, {
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error("Failed to fetch posts");
    }

    return await response.json();
  },
};
