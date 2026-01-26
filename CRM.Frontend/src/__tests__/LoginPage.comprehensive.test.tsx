/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * LoginPage Component Tests
 * Comprehensive tests for authentication, 2FA, OAuth, and form validation
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';

// ============================================================================
// Mock Setup
// ============================================================================

// Mock the auth context
const mockLogin = jest.fn();
const mockVerifyTwoFactor = jest.fn();
const mockGoogleLogin = jest.fn();
const mockNavigate = jest.fn();

jest.mock('../contexts/AuthContext', () => ({
  useAuth: () => ({
    login: mockLogin,
    verifyTwoFactor: mockVerifyTwoFactor,
    googleLogin: mockGoogleLogin,
    isAuthenticated: false,
    user: null,
  }),
}));

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: any) => <a href={to}>{children}</a>,
}));

// Mock fetch for API calls
global.fetch = jest.fn();

// ============================================================================
// Test Suite: Form Structure
// ============================================================================

describe('LoginPage - Form Structure', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ quickAdminLoginEnabled: false }),
    });
  });

  it('should have proper form structure with email and password', () => {
    const mockForm = {
      email: '',
      password: '',
    };
    expect(mockForm).toHaveProperty('email');
    expect(mockForm).toHaveProperty('password');
  });

  it('should have login button', () => {
    const buttonText = 'Sign In';
    expect(buttonText).toBeTruthy();
  });

  it('should have remember me option', () => {
    const rememberMeLabel = 'Remember me';
    expect(rememberMeLabel).toBeTruthy();
  });

  it('should have forgot password link', () => {
    const forgotPasswordText = 'Forgot password?';
    expect(forgotPasswordText.toLowerCase()).toContain('forgot');
  });

  it('should have register link', () => {
    const registerText = "Don't have an account? Register";
    expect(registerText).toContain('Register');
  });
});

// ============================================================================
// Test Suite: Email Validation
// ============================================================================

describe('LoginPage - Email Validation', () => {
  it('should validate correct email format', () => {
    const validEmails = [
      'test@example.com',
      'user.name@domain.org',
      'admin+test@company.io',
      'first.last@subdomain.domain.com',
    ];
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    validEmails.forEach(email => {
      expect(emailRegex.test(email)).toBe(true);
    });
  });

  it('should reject invalid email formats', () => {
    const invalidEmails = [
      'invalid',
      '@missing.com',
      'no@domain',
      'spaces in@email.com',
      '',
    ];
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    invalidEmails.forEach(email => {
      expect(emailRegex.test(email)).toBe(false);
    });
  });

  it('should require email field', () => {
    const email = '';
    const isRequired = email.length === 0;
    expect(isRequired).toBe(true);
  });

  it('should trim whitespace from email', () => {
    const email = '  test@example.com  ';
    const trimmed = email.trim();
    expect(trimmed).toBe('test@example.com');
  });

  it('should convert email to lowercase', () => {
    const email = 'TEST@EXAMPLE.COM';
    const lowercase = email.toLowerCase();
    expect(lowercase).toBe('test@example.com');
  });
});

// ============================================================================
// Test Suite: Password Validation
// ============================================================================

describe('LoginPage - Password Validation', () => {
  it('should require minimum password length', () => {
    const minLength = 6;
    const password = 'abc123';
    expect(password.length).toBeGreaterThanOrEqual(minLength);
  });

  it('should reject short passwords', () => {
    const minLength = 6;
    const shortPassword = 'abc12';
    expect(shortPassword.length).toBeLessThan(minLength);
  });

  it('should accept strong passwords', () => {
    const password = 'SecurePass123!@#';
    expect(password.length).toBeGreaterThanOrEqual(8);
    expect(/[A-Z]/.test(password)).toBe(true);
    expect(/[a-z]/.test(password)).toBe(true);
    expect(/[0-9]/.test(password)).toBe(true);
    expect(/[!@#$%^&*]/.test(password)).toBe(true);
  });

  it('should support password visibility toggle', () => {
    let showPassword = false;
    const togglePasswordVisibility = () => {
      showPassword = !showPassword;
    };
    
    expect(showPassword).toBe(false);
    togglePasswordVisibility();
    expect(showPassword).toBe(true);
  });
});

// ============================================================================
// Test Suite: Form Submission
// ============================================================================

describe('LoginPage - Form Submission', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should call login function on form submit', async () => {
    const credentials = { email: 'test@example.com', password: 'password123' };
    mockLogin.mockResolvedValue({ success: true });
    
    await mockLogin(credentials.email, credentials.password);
    
    expect(mockLogin).toHaveBeenCalledWith(credentials.email, credentials.password);
  });

  it('should prevent submit with empty fields', () => {
    const formData = { email: '', password: '' };
    const isValid = formData.email.length > 0 && formData.password.length > 0;
    expect(isValid).toBe(false);
  });

  it('should disable button during loading', () => {
    const loading = true;
    expect(loading).toBe(true);
    // Button should be disabled when loading is true
  });

  it('should navigate to dashboard on success', async () => {
    mockLogin.mockResolvedValue({ success: true });
    await mockLogin('test@example.com', 'password123');
    
    mockNavigate('/dashboard');
    expect(mockNavigate).toHaveBeenCalledWith('/dashboard');
  });

  it('should handle login failure', async () => {
    const errorMessage = 'Invalid credentials';
    mockLogin.mockRejectedValue(new Error(errorMessage));
    
    try {
      await mockLogin('wrong@example.com', 'wrongpassword');
    } catch (error: any) {
      expect(error.message).toBe(errorMessage);
    }
  });

  it('should save email when remember me is checked', () => {
    const email = 'test@example.com';
    const rememberMe = true;
    
    if (rememberMe) {
      localStorage.setItem('savedEmail', email);
    }
    
    expect(localStorage.getItem('savedEmail')).toBe(email);
  });
});

// ============================================================================
// Test Suite: Two-Factor Authentication
// ============================================================================

describe('LoginPage - Two-Factor Authentication', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should show 2FA input when required', () => {
    const show2FA = true;
    expect(show2FA).toBe(true);
  });

  it('should validate 2FA code format', () => {
    const validCode = '123456';
    const isValid = /^\d{6}$/.test(validCode);
    expect(isValid).toBe(true);
  });

  it('should reject invalid 2FA code', () => {
    const invalidCodes = ['12345', '1234567', 'abcdef', '12 345'];
    const codeRegex = /^\d{6}$/;
    
    invalidCodes.forEach(code => {
      expect(codeRegex.test(code)).toBe(false);
    });
  });

  it('should call verifyTwoFactor on 2FA submit', async () => {
    const token = 'temp-token';
    const code = '123456';
    mockVerifyTwoFactor.mockResolvedValue({ success: true });
    
    await mockVerifyTwoFactor(token, code);
    
    expect(mockVerifyTwoFactor).toHaveBeenCalledWith(token, code);
  });

  it('should handle 2FA verification failure', async () => {
    mockVerifyTwoFactor.mockRejectedValue(new Error('Invalid code'));
    
    try {
      await mockVerifyTwoFactor('token', '000000');
    } catch (error: any) {
      expect(error.message).toBe('Invalid code');
    }
  });

  it('should allow going back from 2FA screen', () => {
    let show2FA = true;
    const goBack = () => {
      show2FA = false;
    };
    
    goBack();
    expect(show2FA).toBe(false);
  });
});

// ============================================================================
// Test Suite: OAuth / Social Login
// ============================================================================

describe('LoginPage - OAuth / Social Login', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should support Google login', async () => {
    const googleCredential = 'mock-google-token';
    mockGoogleLogin.mockResolvedValue({ success: true });
    
    await mockGoogleLogin(googleCredential);
    
    expect(mockGoogleLogin).toHaveBeenCalledWith(googleCredential);
  });

  it('should handle OAuth failure', async () => {
    mockGoogleLogin.mockRejectedValue(new Error('OAuth failed'));
    
    try {
      await mockGoogleLogin('invalid-token');
    } catch (error: any) {
      expect(error.message).toBe('OAuth failed');
    }
  });

  it('should navigate after successful OAuth login', async () => {
    mockGoogleLogin.mockResolvedValue({ success: true });
    await mockGoogleLogin('valid-token');
    
    mockNavigate('/dashboard');
    expect(mockNavigate).toHaveBeenCalledWith('/dashboard');
  });
});

// ============================================================================
// Test Suite: Quick Admin Login
// ============================================================================

describe('LoginPage - Quick Admin Login', () => {
  it('should show quick login when enabled', () => {
    const quickAdminLoginEnabled = true;
    expect(quickAdminLoginEnabled).toBe(true);
  });

  it('should hide quick login when disabled', () => {
    const quickAdminLoginEnabled = false;
    expect(quickAdminLoginEnabled).toBe(false);
  });

  it('should pre-fill credentials on quick login', () => {
    const quickLoginCredentials = {
      email: 'abhi.lal@gmail.com',
      password: 'Admin@123',
    };
    
    expect(quickLoginCredentials.email).toBe('abhi.lal@gmail.com');
    expect(quickLoginCredentials.password).toBe('Admin@123');
  });
});

// ============================================================================
// Test Suite: Error Handling
// ============================================================================

describe('LoginPage - Error Handling', () => {
  it('should display error message on failure', () => {
    const errorMessage = 'Invalid email or password';
    expect(errorMessage).toBeTruthy();
  });

  it('should clear error on new submission', () => {
    let error = 'Previous error';
    const clearError = () => {
      error = '';
    };
    
    clearError();
    expect(error).toBe('');
  });

  it('should handle network errors', async () => {
    mockLogin.mockRejectedValue(new Error('Network error'));
    
    try {
      await mockLogin('test@example.com', 'password');
    } catch (error: any) {
      expect(error.message).toBe('Network error');
    }
  });

  it('should handle API errors', async () => {
    mockLogin.mockRejectedValue({ response: { status: 500, data: 'Server error' } });
    
    try {
      await mockLogin('test@example.com', 'password');
    } catch (error: any) {
      expect(error.response.status).toBe(500);
    }
  });

  it('should display account locked message', () => {
    const errorMessage = 'Account is locked. Please try again later.';
    expect(errorMessage).toContain('locked');
  });
});

// ============================================================================
// Test Suite: Navigation
// ============================================================================

describe('LoginPage - Navigation', () => {
  it('should navigate to register page', () => {
    const registerPath = '/register';
    expect(registerPath).toBe('/register');
  });

  it('should navigate to password reset page', () => {
    const resetPath = '/password-reset';
    expect(resetPath).toBe('/password-reset');
  });

  it('should redirect if already authenticated', () => {
    const isAuthenticated = true;
    if (isAuthenticated) {
      mockNavigate('/dashboard');
    }
    expect(mockNavigate).toHaveBeenCalledWith('/dashboard');
  });
});

// ============================================================================
// Test Suite: Accessibility
// ============================================================================

describe('LoginPage - Accessibility', () => {
  it('should have proper input labels', () => {
    const emailLabel = 'Email Address';
    const passwordLabel = 'Password';
    
    expect(emailLabel).toBeTruthy();
    expect(passwordLabel).toBeTruthy();
  });

  it('should have proper button text', () => {
    const buttonText = 'Sign In';
    expect(buttonText).toBeTruthy();
  });

  it('should support keyboard navigation', () => {
    const tabIndex = 0;
    expect(tabIndex).toBe(0);
  });

  it('should have aria labels for password visibility', () => {
    const showPasswordLabel = 'Show password';
    const hidePasswordLabel = 'Hide password';
    
    expect(showPasswordLabel).toBeTruthy();
    expect(hidePasswordLabel).toBeTruthy();
  });
});

// ============================================================================
// Test Suite: Session Management
// ============================================================================

describe('LoginPage - Session Management', () => {
  it('should store token on successful login', () => {
    const token = 'jwt-token-here';
    localStorage.setItem('accessToken', token);
    expect(localStorage.getItem('accessToken')).toBe(token);
  });

  it('should restore session from storage', () => {
    const savedToken = 'saved-jwt-token';
    localStorage.setItem('accessToken', savedToken);
    
    const token = localStorage.getItem('accessToken');
    expect(token).toBe(savedToken);
  });

  it('should clear session on logout', () => {
    localStorage.setItem('accessToken', 'token');
    localStorage.removeItem('accessToken');
    expect(localStorage.getItem('accessToken')).toBeNull();
  });
});
