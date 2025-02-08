import { getAuthHeaders } from "./auth";
import { API_URL } from "./constants";
import axios from "axios";

const CACHE_NAME = "report-reasons-cache";
const CACHE_LIFETIME = 24 * 60 * 60 * 1000; // 24 hours

const getCachedData = async (key) => {
  const cache = await caches.open(CACHE_NAME);
  const response = await cache.match(key);

  if (!response) return null;

  const data = await response.json();
  const now = Date.now();

  return now - data.timestamp < CACHE_LIFETIME ? data.value : null;
};

const setCacheData = async (key, value) => {
  const cache = await caches.open(CACHE_NAME);
  const data = {
    value,
    timestamp: Date.now(),
  };

  await cache.put(
    key,
    new Response(JSON.stringify(data), {
      headers: { "Content-Type": "application/json" },
    })
  );
};

const reasons = async (reason) => {
  const cacheKey = `${API_URL}/Report/reasons/${reason}`;
  const cachedData = await getCachedData(cacheKey);

  if (cachedData) return cachedData;

  const response = await axios.get(cacheKey, {
    headers: getAuthHeaders(),
  });

  await setCacheData(cacheKey, response.data);
  return response.data;
};

export const reportService = {
  async getPostReportReasons() {
    return await reasons("post");
  },
  async getUserReportReasons() {
    return await reasons("user");
  },
  async reportContent(data, type) {
    return await axios.post(`${API_URL}/Report/${type}`, data, {
      headers: getAuthHeaders(),
    });
  },
};
