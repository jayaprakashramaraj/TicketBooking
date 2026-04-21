// Dynamic API detection
// Uses the hostname from the browser URL to allow access via Local IP (WiFi)
const hostname = window.location.hostname;

const API_BASE_URL = {
  IDENTITY: `http://${hostname}:5000`,
  CATALOG: `http://${hostname}:5001`,
  BOOKING: `http://${hostname}:5002`,
  NOTIFICATION: `http://${hostname}:5002`,
};

export default API_BASE_URL;
