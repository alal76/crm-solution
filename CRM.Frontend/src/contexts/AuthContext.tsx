import React, { createContext, useState, useContext, useEffect } from 'react';
import axiosInstance from '../services/apiClient';
import { debugLog, debugError } from '../utils/debug';

// Cookie utility functions
const setCookie = (name: string, value: string, days: number = 30) => {
  const date = new Date();
  date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
  const expires = `expires=${date.toUTCString()}`;
  document.cookie = `${name}=${encodeURIComponent(value)};${expires};path=/;SameSite=Strict`;
};

const getCookie = (name: string): string | null => {
  const nameEQ = name + '=';
  const cookies = document.cookie.split(';');
  for (let cookie of cookies) {
    cookie = cookie.trim();
    if (cookie.indexOf(nameEQ) === 0) {
      return decodeURIComponent(cookie.substring(nameEQ.length));
    }
  }
  return null;
};

const deleteCookie = (name: string) => {
  document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;`;
};

interface AuthContextType {
  isAuthenticated: boolean;
  user: any | null;
  login: (email: string, password: string) => Promise<void>;
  register: (data: any) => Promise<void>;
  logout: () => void;
  googleLogin: (idToken: string) => Promise<void>;
  initiateMicrosoftLogin: () => void;
}

declare global {
  interface Window {
    google?: any;
  }
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState(null);

  useEffect(() => {
    // Check if token exists in localStorage
    let token = localStorage.getItem('accessToken');
    
    // If no token in localStorage, try to recover from cookies
    if (!token) {
      const savedToken = getCookie('crm_auth_token');
      const savedUser = getCookie('crm_user_data');
      
      if (savedToken && savedUser) {
        try {
          const userData = JSON.parse(savedUser);
          localStorage.setItem('accessToken', savedToken);
          localStorage.setItem('refreshToken', getCookie('crm_refresh_token') || '');
          localStorage.setItem('userProfile', getCookie('crm_user_profile') || '{}');
          setUser(userData);
          setIsAuthenticated(true);
          token = savedToken;
          debugLog('User auto-logged in from cookies', { userId: userData.id });
        } catch (error) {
          debugError('Failed to restore user from cookies', { error });
          deleteCookie('crm_auth_token');
          deleteCookie('crm_user_data');
          deleteCookie('crm_refresh_token');
          deleteCookie('crm_user_profile');
        }
      }
    }
    
    if (token) {
      setIsAuthenticated(true);
      // Optionally fetch current user
      fetchCurrentUser(token);
    }

    // Check for Microsoft OAuth callback
    checkMicrosoftCallback();
  }, []);

  const checkMicrosoftCallback = () => {
    // Check if we're returning from Microsoft OAuth
    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');
    const state = params.get('state');

    if (code && state) {
      // Store the code for processing
      sessionStorage.setItem('microsoft_code', code);
      sessionStorage.setItem('microsoft_state', state);
      // Remove parameters from URL
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  };

  const fetchCurrentUser = async (token: string) => {
    try {
      debugLog('Fetching current user', { token: token ? 'present' : 'missing' });
      const response = await axiosInstance.get('/auth/me', {
        headers: { Authorization: `Bearer ${token}` }
      });
      debugLog('Current user fetched', { userId: response.data.id });
      setUser(response.data);
    } catch (error: any) {
      debugError('Failed to fetch user', {
        error: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      localStorage.removeItem('accessToken');
      setIsAuthenticated(false);
    }
  };

  const login = async (email: string, password: string) => {
    try {
      debugLog('Login attempt', { email });
      const response = await axiosInstance.post('/auth/login', { email, password });
      debugLog('Login successful', { userId: response.data.userId, email: response.data.email });
      
      const accessToken = response.data.accessToken;
      const refreshToken = response.data.refreshToken;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      // Store in cookies for auto-login persistence (30 days)
      setCookie('crm_auth_token', accessToken, 30);
      setCookie('crm_refresh_token', refreshToken, 30);
      setCookie('crm_user_data', JSON.stringify(response.data), 30);
      
      // Store profile information
      const profileData = {
        departmentId: response.data.departmentId,
        departmentName: response.data.departmentName,
        userProfileId: response.data.userProfileId,
        userProfileName: response.data.userProfileName,
        accessiblePages: response.data.accessiblePages || [],
        permissions: response.data.permissions || {}
      };
      localStorage.setItem('userProfile', JSON.stringify(profileData));
      setCookie('crm_user_profile', JSON.stringify(profileData), 30);
      
      setUser(response.data);
      setIsAuthenticated(true);
    } catch (error: any) {
      debugError('Login failed', {
        email,
        error: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      throw error;
    }
  };

  const register = async (data: any) => {
    try {
      const response = await axiosInstance.post('/auth/register', data);
      const accessToken = response.data.accessToken;
      const refreshToken = response.data.refreshToken;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      // Store in cookies for auto-login persistence (30 days)
      setCookie('crm_auth_token', accessToken, 30);
      setCookie('crm_refresh_token', refreshToken, 30);
      setCookie('crm_user_data', JSON.stringify(response.data), 30);
      
      // Store profile information
      const profileData = {
        departmentId: response.data.departmentId,
        departmentName: response.data.departmentName,
        userProfileId: response.data.userProfileId,
        userProfileName: response.data.userProfileName,
        accessiblePages: response.data.accessiblePages || [],
        permissions: response.data.permissions || {}
      };
      localStorage.setItem('userProfile', JSON.stringify(profileData));
      setCookie('crm_user_profile', JSON.stringify(profileData), 30);
      
      setUser(response.data);
      setIsAuthenticated(true);
    } catch (error) {
      console.error('Registration failed', error);
      throw error;
    }
  };

  const googleLogin = async (idToken: string) => {
    const googleClientId = process.env.REACT_APP_GOOGLE_CLIENT_ID;
    if (!googleClientId) {
      throw new Error('Google OAuth is not configured. Please contact administrator.');
    }
    try {
      const response = await axiosInstance.post('/auth/oauth-login', {
        provider: 'google',
        token: idToken
      });
      const accessToken = response.data.accessToken;
      const refreshToken = response.data.refreshToken;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      // Store in cookies for auto-login persistence (30 days)
      setCookie('crm_auth_token', accessToken, 30);
      setCookie('crm_refresh_token', refreshToken, 30);
      setCookie('crm_user_data', JSON.stringify(response.data), 30);
      
      // Store profile information
      const profileData = {
        departmentId: response.data.departmentId,
        departmentName: response.data.departmentName,
        userProfileId: response.data.userProfileId,
        userProfileName: response.data.userProfileName,
        accessiblePages: response.data.accessiblePages || [],
        permissions: response.data.permissions || {}
      };
      localStorage.setItem('userProfile', JSON.stringify(profileData));
      setCookie('crm_user_profile', JSON.stringify(profileData), 30);
      
      setUser(response.data);
      setIsAuthenticated(true);
    } catch (error) {
      console.error('Google login failed', error);
      throw error;
    }
  };

  const initiateMicrosoftLogin = () => {
    // Microsoft OAuth configuration
    const clientId = process.env.REACT_APP_MICROSOFT_CLIENT_ID;
    const tenantId = process.env.REACT_APP_MICROSOFT_TENANT_ID;

    if (!clientId || !tenantId) {
      throw new Error('Microsoft OAuth is not configured. Please contact administrator.');
    }

    const redirectUri = `${window.location.origin}/login`;
    const responseType = 'code';
    const scope = 'openid profile email';

    // Construct authorization URL
    const authUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize?` +
      `client_id=${encodeURIComponent(clientId)}&` +
      `redirect_uri=${encodeURIComponent(redirectUri)}&` +
      `response_type=${encodeURIComponent(responseType)}&` +
      `scope=${encodeURIComponent(scope)}&` +
      `state=${encodeURIComponent(Math.random().toString(36).substring(7))}`;

    // Redirect to Microsoft login
    window.location.href = authUrl;
  };

  const logout = () => {
    // Clear localStorage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userProfile');
    sessionStorage.removeItem('microsoft_code');
    sessionStorage.removeItem('microsoft_state');
    
    // Clear cookies
    deleteCookie('crm_auth_token');
    deleteCookie('crm_refresh_token');
    deleteCookie('crm_user_data');
    deleteCookie('crm_user_profile');
    
    debugLog('User logged out successfully');
    setUser(null);
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, user, login, register, logout, googleLogin, initiateMicrosoftLogin }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
