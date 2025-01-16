import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";

const CACHE_NAME = "tags-cache-v1";
const CACHE_KEY = "/api/tags";
const CACHE_DURATION = 7 * 24 * 60 * 60 * 1000; // 7 days

export const filterTagService = {
  async getTags(forceRefresh = false) {
    try {
      if (!forceRefresh) {
        const cachedData = await this.getFromCache();
        if (cachedData) return cachedData;
      }

      const response = await fetch(`${API_URL}/Tag`, {
        method: "GET",
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        throw new Error(await response.text());
      }

      const tags = await response.json();
      await this.saveToCache(tags);
      return tags;
    } catch (error) {
      throw new Error(`Error fetching tags: ${error.message}`);
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
      console.error("Failed to cache tags:", error);
    }
  },

  async clearCache() {
    const cache = await caches.open(CACHE_NAME);
    await cache.delete(CACHE_KEY);
  },
};

export const filterTagController = {
  getSelectedFilters() {
    const filters = sessionStorage.getItem("selectedFilters");
    return filters ? JSON.parse(filters) : null;
  },

  getAllSelectedTags() {
    const filters = this.getSelectedFilters();
    if (!filters) return null;

    const allTags = [
      ...(filters.selected_ui || []),
      ...(filters.selected_style || []),
      ...(filters.selected_color || []),
    ];

    return allTags.length > 0 ? allTags.join(",") : null;
  },

  getAccessType() {
    const filters = this.getSelectedFilters();
    return filters?.activeToggle || null;
  },
};
