// API Configuration
const hostname = window.location.hostname;
const port = window.location.port;

// Detect if we are running in development mode (e.g., npm run dev)
// or in production/docker (served via NGINX)
const isDev = import.meta.env.DEV;

// If we are running on port 5173, we are in local Vite dev mode.
// Docker mode is served through the NGINX gateway.
const isViteDev = port === '5173';

const API_BASE_URL = (isDev && isViteDev) ? {
  // Local Debugging Ports (Visual Studio / run-local.cmd)
  IDENTITY: import.meta.env.VITE_IDENTITY_API_URL || `http://${hostname}:7000`,
  CATALOG: import.meta.env.VITE_CATALOG_API_URL || `http://${hostname}:7001`,
  BOOKING: import.meta.env.VITE_BOOKING_API_URL || `http://${hostname}:7002`,
  NOTIFICATION: import.meta.env.VITE_NOTIFICATION_API_URL || `http://${hostname}:7004`,
} : {
  // Docker / Production Ports (via NGINX Proxy)
  // All routed through the NGINX port (5002)
  IDENTITY: `http://${hostname}:5002`,
  CATALOG: `http://${hostname}:5002`,
  BOOKING: `http://${hostname}:5002`,
  NOTIFICATION: `http://${hostname}:5002`,
};

export default API_BASE_URL;
