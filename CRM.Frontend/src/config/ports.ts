/**
 * Port Configuration for CRM Application
 * Caches API and database ports for use across the application
 */

interface ServicePorts {
  api: number;
  apiExternal: number;
  database: number;
  frontend: number;
}

// Default ports (used if environment variables are not set)
const DEFAULT_PORTS: ServicePorts = {
  api: 5000,           // Internal API port (inside Docker)
  apiExternal: 5001,   // External API port (exposed to browser)
  database: 3306,      // MariaDB port
  frontend: 3000,      // Frontend port
};

/**
 * Get cached service ports configuration
 * @returns ServicePorts object with all configured ports
 */
export const getServicePorts = (): ServicePorts => {
  return {
    api: parseInt(process.env.REACT_APP_API_PORT || String(DEFAULT_PORTS.api), 10),
    apiExternal: parseInt(process.env.REACT_APP_API_EXTERNAL_PORT || String(DEFAULT_PORTS.apiExternal), 10),
    database: parseInt(process.env.REACT_APP_DB_PORT || String(DEFAULT_PORTS.database), 10),
    frontend: parseInt(process.env.REACT_APP_FRONTEND_PORT || String(DEFAULT_PORTS.frontend), 10),
  };
};

/**
 * Get the appropriate API base URL based on current location
 * @returns Full API base URL (without trailing slash)
 */
export const getApiBaseUrl = (): string => {
  const ports = getServicePorts();
  const isLocalhost = window.location.hostname === 'localhost' || 
                      window.location.hostname === '127.0.0.1' ||
                      window.location.hostname.startsWith('192.168.');
  
  if (isLocalhost) {
    // Browser accessing from localhost - use external port
    return `http://${window.location.hostname}:${ports.apiExternal}`;
  }
  
  // In Docker container or production - use internal service name
  return process.env.REACT_APP_API_URL || `http://api:${ports.api}`;
};

/**
 * Get the health check URL
 * @returns Full health check endpoint URL
 */
export const getHealthCheckUrl = (): string => {
  const baseUrl = getApiBaseUrl();
  return `${baseUrl}/health`;
};

/**
 * Get the API endpoint URL for a given path
 * @param path - The API path (e.g., '/auth/login')
 * @returns Full API endpoint URL
 */
export const getApiEndpoint = (path: string): string => {
  const baseUrl = getApiBaseUrl();
  const apiPath = '/api';
  return `${baseUrl}${apiPath}${path.startsWith('/') ? path : '/' + path}`;
};

const portsConfig = {
  getServicePorts,
  getApiBaseUrl,
  getHealthCheckUrl,
  getApiEndpoint,
  DEFAULT_PORTS,
};

export default portsConfig;
