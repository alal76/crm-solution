import axios, { AxiosInstance } from 'axios';
import { debugLog, debugError } from '../utils/debug';
import { getApiBaseUrl, getServicePorts } from '../config/ports';

// Determine API URL at runtime to ensure window is available
let API_URL: string;

const initializeApiUrl = () => {
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
  baseURL: API_URL || 'http://localhost:5001/api',
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
      debugError('API Response Error:', {
        status: error.response.status,
        url: error.config?.url,
        data: error.response.data,
      });
      
      if (error.response.status === 401) {
        debugLog('Unauthorized - clearing tokens');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
      }
    } else if (error.request) {
      debugError('API No Response:', {
        url: error.config?.url,
        message: error.message,
      });
    } else {
      debugError('API Error:', error.message);
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
