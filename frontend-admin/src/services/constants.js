let API_URL = `http://localhost:${import.meta.env.VITE_ADMINPANEL_PORT}`;
let imageDirectory = `http://localhost:${
	import.meta.env.VITE_SERVER_PORT
}/images/`;
export { API_URL, imageDirectory };

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
