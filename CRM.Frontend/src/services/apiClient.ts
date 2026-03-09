import axios, { AxiosInstance } from 'axios';
import { debugLog, debugError } from '../utils/debug';
import { getApiBaseUrl, getServicePorts } from '../config/ports';

// Determine API URL at runtime to ensure window is available
let API_URL: string = '/api';

const initializeApiUrl = (): void => {
  const ports = getServicePorts();
  const API_BASE_URL = getApiBaseUrl();
  API_URL = `${API_BASE_URL}/api`;
  debugLog('API Client initialized', { API_URL, ports });
};

// Initialize on first use or immediately if window is available
if (typeof window !== 'undefined') {
  initializeApiUrl();
}

const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

// Reinitialize baseURL after module loads if not already set
if (!API_URL && typeof window !== 'undefined') {
  initializeApiUrl();
  apiClient.defaults.baseURL = API_URL;
}

// Add request interceptor with logging
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  debugLog('API Request:', {
    method: config.method,
    url: config.url,
    baseURL: config.baseURL,
    hasToken: !!token,
  });
  
  return config;
});

// Add response interceptor with logging
apiClient.interceptors.response.use(
  (response) => {
    debugLog('API Response Success:', {
      status: response.status,
      url: response.config.url,
      dataKeys: response.data ? Object.keys(response.data).slice(0, 5) : 'no data',
    });
    return response;
  },
  (error) => {
    if (error.response) {
      const status: number = error.response.status;
      const url: string = error.config?.url ?? 'unknown';
      const serverMsg: string =
        error.response.data?.message ||
        error.response.data?.error ||
        error.response.data?.title ||
        '';

      const statusLabels: Record<number, string> = {
        400: 'Bad Request',
        401: 'Unauthorised',
        403: 'Forbidden',
        404: 'Not Found',
        422: 'Unprocessable Entity',
        423: 'Account Locked',
        429: 'Rate Limited',
        500: 'Internal Server Error (possible DB or service failure)',
        502: 'Bad Gateway — API gateway cannot reach the backend/API container',
        503: 'Service Unavailable — backend may be starting up or overloaded',
        504: 'Gateway Timeout — backend or database did not respond in time',
      };
      const label = statusLabels[status] || `HTTP ${status}`;

      debugError(`API Error [${status} ${label}] ${url}`, {
        status,
        url,
        serverMessage: serverMsg || '(none)',
        responseData: error.response.data,
      });

      if (status === 502 || status === 503 || status === 504) {
        console.error(
          `[CRM API] ${label} on ${url}. ` +
          'Possible causes: crm-api container is down, the reverse proxy cannot route to it, ' +
          'or the database is unavailable. Check Docker/K8s logs for crm-api and crm-mariadb.'
        );
      }

      if (error.response.status === 401) {
        debugLog('Unauthorized - clearing tokens');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        // Only redirect to /login once per session to avoid redirect loops
        try {
          const alreadyRedirected = window.sessionStorage.getItem('crm_redirected_to_login');
          if (!alreadyRedirected && typeof window !== 'undefined') {
            window.sessionStorage.setItem('crm_redirected_to_login', '1');
            if (window.location.pathname !== '/login') {
              window.location.href = '/login';
            }
          }
        } catch (e) {
          // Fallback: direct navigate
          if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
            window.location.href = '/login';
          }
        }
      }
    } else if (error.request) {
      const url: string = error.config?.url ?? 'unknown';
      const isTimeout = error.code === 'ECONNABORTED' || (error as Error).message?.includes('timeout');
      debugError(
        isTimeout
          ? `API Timeout — no response from ${url} within ${error.config?.timeout ?? 10000}ms`
          : `API No Response — request to ${url} got no reply (service may be down or unreachable)`,
        {
          url,
          code: error.code,
          message: (error as Error).message,
          hint: 'Check that crm-api container is running. For 502/no-response: verify the API service port, Docker network, and that crm-mariadb is healthy.',
        }
      );
      console.error(
        `[CRM API] Cannot reach ${url}. ` +
        'Ensure the crm-api service is running (docker ps | grep crm-api) and accessible on the expected port.'
      );
    } else {
      debugError('API Request Setup Error:', { message: (error as Error).message, code: error.code });
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
