import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import { filterTagController } from "./filterTag";

export const postsContentService = {
  async getFeedPosts(pageNumber = 1) {
    const defaultPageSize = localStorage.getItem("postsPerRequest") || 3;
    const params = new URLSearchParams({
      pageNumber: pageNumber,
      pageSize: defaultPageSize,
    });

    const tags = filterTagController.getAllSelectedTags();
    const accessType = filterTagController.getAccessType();

    if (tags) params.append("tags", tags);
    if (accessType) params.append("accessType", accessType);

    const response = await fetch(`${API_URL}/Post/feed?${params}`, {
      headers: getAuthHeaders(),
    }).catch((error) => {
      console.error("Error:", error);
      throw new Error("Failed to fetch posts");
    });

    return await response.json();
  },
  async getPost(postId) {
    const response = await fetch(`${API_URL}/Post/${postId}`, {
      headers: getAuthHeaders(),
    }).catch((error) => {
      console.error("Error:", error);
      throw new Error("Failed to fetch post");
    });

    return await response.json();
  },
  async deletePost(postId) {
    const response = await fetch(`${API_URL}/Post/${postId}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    }).catch((error) => {
      console.error("Error:", error);
      throw new Error("Failed to delete post");
    });

    return await response.json();
  },
};

export const miniPostsContentService = {
  async getPortfolioPosts(username) {
    const params = new URLSearchParams({});
    const tags = filterTagController.getAllSelectedTags();
    const accessType = filterTagController.getAccessType();
    if (tags) params.append("tags", tags);
    if (accessType) params.append("accessType", accessType);

    const response = await fetch(
      `${API_URL}/Post/profile/${username}/mini?${params}`,
      {
        headers: getAuthHeaders(),
      }
    ).catch((error) => {
      console.error("Error:", error);
      throw new Error("Failed to fetch posts");
    });

    return await response.json();
  },
};
