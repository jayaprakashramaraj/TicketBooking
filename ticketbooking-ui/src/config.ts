// Microservice endpoints
// When running in Docker, we still use localhost because the browser 
// is the one making the calls from your machine to the published Docker ports.
const API_BASE_URL = {
  IDENTITY: 'http://localhost:5000',
  CATALOG: 'http://localhost:5001',
  BOOKING: 'http://localhost:5002',
};

export default API_BASE_URL;
