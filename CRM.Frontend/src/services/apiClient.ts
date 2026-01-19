import axios, { AxiosInstance } from 'axios';
import { debugLog, debugError } from '../utils/debug';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

debugLog('API Client initialized with URL:', API_URL);

const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

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
