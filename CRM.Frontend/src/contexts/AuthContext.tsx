/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

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
  login: (email: string, password: string) => Promise<{ requiresTwoFactor?: boolean; twoFactorToken?: string }>;
  verifyTwoFactor: (twoFactorToken: string, code: string) => Promise<void>;
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
    // Prevent repeated restoration attempts in a single session
    if (window.sessionStorage.getItem('crm_cookie_restore_attempted')) {
      debugLog('Cookie restoration already attempted this session. Skipping.');
      checkMicrosoftCallback();
      return;
    }
    window.sessionStorage.setItem('crm_cookie_restore_attempted', '1');

    // Check if token exists in localStorage
    let token = localStorage.getItem('accessToken');
    let restorationFailed = false;

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
          debugError('Failed to restore user from cookies (invalid JSON or corrupted data)', { error, savedUser });
          // Clear all related cookies and localStorage to break any reload loop
          deleteCookie('crm_auth_token');
          deleteCookie('crm_user_data');
          deleteCookie('crm_refresh_token');
          deleteCookie('crm_user_profile');
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('userProfile');
          restorationFailed = true;
        }
      } else if (savedToken || savedUser) {
        // If only one of the cookies exists, clear both to avoid partial state
        debugError('Partial cookie state detected. Clearing all auth cookies.');
        deleteCookie('crm_auth_token');
        deleteCookie('crm_user_data');
        deleteCookie('crm_refresh_token');
        deleteCookie('crm_user_profile');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('userProfile');
        restorationFailed = true;
      }
    }

    if (token && !restorationFailed) {
      setIsAuthenticated(true);
      // Optionally fetch current user
      fetchCurrentUser(token).catch(() => {
        // If /auth/me fails, clear tokens and show error, do not reload
        deleteCookie('crm_auth_token');
        deleteCookie('crm_user_data');
        deleteCookie('crm_refresh_token');
        deleteCookie('crm_user_profile');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('userProfile');
        setIsAuthenticated(false);
        setUser(null);
        window.sessionStorage.setItem('crm_login_error', 'Session expired or invalid. Please log in again.');
      });
    } else if (restorationFailed) {
      setIsAuthenticated(false);
      setUser(null);
      window.sessionStorage.setItem('crm_login_error', 'Session restoration failed. Please log in again.');
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

  const login = async (email: string, password: string): Promise<{ requiresTwoFactor?: boolean; twoFactorToken?: string }> => {
    try {
      const endpoint = '/auth/login';
      console.log('[AuthContext] Login attempt', { email, endpoint });
      debugLog('Login attempt', { email, endpoint });
      
      const response = await axiosInstance.post(endpoint, { email, password });
      console.log('[AuthContext] Login response received:', { userId: response.data.userId, status: response.status });
      
      // Check if 2FA is required
      if (response.data.requiresTwoFactor) {
        console.log('[AuthContext] 2FA required for user');
        return { 
          requiresTwoFactor: true, 
          twoFactorToken: response.data.twoFactorToken 
        };
      }
      
      debugLog('Login successful', { userId: response.data.userId, email: response.data.email });
      
      const accessToken = response.data.accessToken;
      const refreshToken = response.data.refreshToken;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      // Store in cookies for auto-login persistence (30 days)
      setCookie('crm_auth_token', accessToken, 30);
      setCookie('crm_refresh_token', refreshToken, 30);
      setCookie('crm_user_data', JSON.stringify(response.data), 30);
      
      // Store profile information including group permissions
      const profileData = {
        departmentId: response.data.departmentId,
        departmentName: response.data.departmentName,
        userProfileId: response.data.userProfileId,
        userProfileName: response.data.userProfileName,
        primaryGroupId: response.data.primaryGroupId,
        primaryGroupName: response.data.primaryGroupName,
        accessiblePages: response.data.accessiblePages || [],
        permissions: response.data.permissions || {},
        groupPermissions: response.data.groupPermissions || null
      };
      localStorage.setItem('userProfile', JSON.stringify(profileData));
      setCookie('crm_user_profile', JSON.stringify(profileData), 30);
      
      setUser(response.data);
      setIsAuthenticated(true);
      console.log('[AuthContext] Login complete, user authenticated, groupPermissions:', response.data.groupPermissions);
      
      // Clear any branding update flags on successful login
      localStorage.removeItem('brandingUpdated');
      localStorage.removeItem('brandingReset');
      
      return {};
    } catch (error: any) {
      console.error('[AuthContext] Login failed:', {
        email,
        message: error.message,
        status: error.response?.status,
        data: error.response?.data,
        code: error.code,
      });
      debugError('Login failed', {
        email,
        error: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      throw error;
    }
  };

  const verifyTwoFactor = async (twoFactorToken: string, code: string): Promise<void> => {
    try {
      console.log('[AuthContext] 2FA verification attempt');
      const response = await axiosInstance.post('/auth/login/2fa', { twoFactorToken, code });
      console.log('[AuthContext] 2FA verification successful');
      
      const accessToken = response.data.accessToken;
      const refreshToken = response.data.refreshToken;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      // Store in cookies for auto-login persistence (30 days)
      setCookie('crm_auth_token', accessToken, 30);
      setCookie('crm_refresh_token', refreshToken, 30);
      setCookie('crm_user_data', JSON.stringify(response.data), 30);
      
      // Store profile information including group permissions
      const profileData = {
        departmentId: response.data.departmentId,
        departmentName: response.data.departmentName,
        userProfileId: response.data.userProfileId,
        userProfileName: response.data.userProfileName,
        primaryGroupId: response.data.primaryGroupId,
        primaryGroupName: response.data.primaryGroupName,
        accessiblePages: response.data.accessiblePages || [],
        permissions: response.data.permissions || {},
        groupPermissions: response.data.groupPermissions || null
      };
      localStorage.setItem('userProfile', JSON.stringify(profileData));
      setCookie('crm_user_profile', JSON.stringify(profileData), 30);
      
      setUser(response.data);
      setIsAuthenticated(true);
    } catch (error: any) {
      console.error('[AuthContext] 2FA verification failed:', error.message);
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
      
      // Store profile information including group permissions
      const profileData = {
        departmentId: response.data.departmentId,
        departmentName: response.data.departmentName,
        userProfileId: response.data.userProfileId,
        userProfileName: response.data.userProfileName,
        primaryGroupId: response.data.primaryGroupId,
        primaryGroupName: response.data.primaryGroupName,
        accessiblePages: response.data.accessiblePages || [],
        permissions: response.data.permissions || {},
        groupPermissions: response.data.groupPermissions || null
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
    <AuthContext.Provider value={{ isAuthenticated, user, login, verifyTwoFactor, register, logout, googleLogin, initiateMicrosoftLogin }}>
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
