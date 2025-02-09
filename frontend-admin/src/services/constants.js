export const API_URL = "http://localhost:8082";
export const imageDirectory = "http://localhost:8088/images/";

export const formatNumber = (number) => {
  if (number >= 1000000000) {
    return (number / 1000000000).toFixed(1).replace(".", ",") + " B";
  }
  if (number >= 1000000) {
    return (number / 1000000).toFixed(1).replace(".", ",") + " M";
  }
  if (number >= 1000) {
    return (number / 1000).toFixed(1).replace(".", ",") + " K";
  }
  return number;
};
