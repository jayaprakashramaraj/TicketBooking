// API Configuration
const hostname = window.location.hostname;
const port = window.location.port;

// Detect if we are running in development mode (e.g., npm run dev)
// or in production/docker (served via NGINX)
const isDev = import.meta.env.DEV;

// If we are running on port 5173, we are in local Vite dev mode.
// If we are running on any other port (like 3000), we are likely in Docker.
const isViteDev = port === '5173';

const API_BASE_URL = (isDev && isViteDev) ? {
  // Local Debugging Ports (Visual Studio / run-local.cmd)
  IDENTITY: `http://${hostname}:7000`,
  CATALOG: `http://${hostname}:7001`,
  BOOKING: `http://${hostname}:7002`,
  NOTIFICATION: `http://${hostname}:7004`,
} : {
  // Docker / Production Ports (via NGINX Proxy)
  // All routed through the NGINX port (5002)
  IDENTITY: `http://${hostname}:5002`,
  CATALOG: `http://${hostname}:5002`,
  BOOKING: `http://${hostname}:5002`,
  NOTIFICATION: `http://${hostname}:5002`,
};

export default API_BASE_URL;
