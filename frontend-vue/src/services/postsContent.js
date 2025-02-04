import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import { filterTagController } from "./filterTag";
import axios from "axios";

export const PAGE_LIMIT = 5;

const buildPostsParams = (pageNumber, pageSize) => {
  const params = new URLSearchParams({
    pageNumber,
    pageSize,
  });

  const tags = filterTagController.getAllSelectedTags();
  const accessType = filterTagController.getAccessType();

  if (tags) params.append("tags", tags);
  if (accessType) params.append("accessType", accessType);

  return params;
};

const fetchPosts = async (url) => {
  const response = await fetch(url, {
    headers: getAuthHeaders(),
  }).catch((error) => {
    console.error("Error:", error);
    throw new Error("Failed to fetch posts");
  });

  return await response.json();
};

export const postsContentService = {
  async getFeedPosts(pageNumber = 1) {
    const defaultPageSize = localStorage.getItem("postsPerRequest") || 3;
    const params = buildPostsParams(pageNumber, defaultPageSize);
    return fetchPosts(`${API_URL}/Post/feed?${params}`);
  },
  async likePost(postId) {
    const response = await axios.post(`${API_URL}/Post/${postId}/like`, null, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },
  async getPost(postId) {
    const response = await axios
      .get(`${API_URL}/Post/${postId}`, {
        headers: getAuthHeaders(),
      })
      .catch((error) => {
        console.error("Error:", error);
        if (error.response) {
          throw error.response;
        }
        throw new Error("Failed to fetch post");
      });

    return response.data;
  },
  async getProtectedPost(hash) {
    const response = await axios
      .get(`${API_URL}/Post/protected/${hash}`, {
        headers: getAuthHeaders(),
      })
      .catch((error) => {
        console.error("Error:", error);
        if (error.response) {
          throw error.response;
        }
        throw new Error("Failed to fetch post");
      });

    return response.data;
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
  async createPost(formData) {
    const response = await axios
      .post(`${API_URL}/Post`, formData, {
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "multipart/form-data",
        },
      })
      .catch((error) => {
        console.error("Error:", error);
        throw new Error("Failed to create post");
      });

    return response.data;
  },
};

export const miniPostsContentService = {
  async getPortfolioPosts(username, pageNumber = 1) {
    const params = buildPostsParams(pageNumber, PAGE_LIMIT);
    return fetchPosts(`${API_URL}/Post/profile/${username}/mini?${params}`);
  },
  async getMyMiniPosts(pageNumber = 1) {
    const params = buildPostsParams(pageNumber, PAGE_LIMIT);
    return fetchPosts(`${API_URL}/Post/own?${params}`);
  },
};
