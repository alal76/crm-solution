/**
 * Security Settings Service — AP-042
 * Centralizes all security-related API calls from SecuritySettingsTab.tsx.
 * Covers SSL certificate management and two-factor authentication (2FA).
 */

import apiClient from './apiClient';

export interface SslStatus {
  isEnabled: boolean;
  hasCertificate: boolean;
  certSubject?: string;
  certThumbprint?: string;
  certExpiry?: string;
  issuer?: string;
  expiresInDays?: number | null;
}

export interface PasswordComplexitySettings {
  minPasswordLength: number;
  maxPasswordLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
  defaultPasswordExpirationDays: number;
}

export interface SecuritySystemSettings extends PasswordComplexitySettings {
  quickAdminLoginEnabled: boolean;
  sessionTimeoutMinutes?: number;
}

export interface SslToggleRequest {
  enabled: boolean;
  forceRedirect?: boolean;
}

export interface SslGenerateRequest {
  commonName: string;
  validityDays: number;
}

export interface SslGenerateResult {
  message?: string;
  expiresOn?: string;
}

export interface TwoFactorSetupData {
  secret: string;
  qrCode?: string;
  backupCodes: string[];
}

export interface TwoFactorEnableRequest {
  secret: string;
  backupCodes: string[];
}

export interface TwoFactorVerifyRequest {
  code: string;
}

export interface TwoFactorVerifyResult {
  success: boolean;
  message?: string;
}

const securitySettingsService = {
  /**
   * Fetch current SSL certificate status.
   */
  getSslStatus: async (): Promise<SslStatus> => {
    const response = await apiClient.get<SslStatus>('/systemsettings/ssl/status');
    return response.data;
  },

  /**
   * Fetch full system settings (includes password complexity and quick admin login).
   */
  getSecuritySettings: async (): Promise<SecuritySystemSettings> => {
    const response = await apiClient.get<SecuritySystemSettings>('/systemsettings');
    return response.data;
  },

  /**
   * Update system settings (password complexity, quick admin login, etc.).
   */
  updateSecuritySettings: async (settings: Partial<SecuritySystemSettings>): Promise<SecuritySystemSettings> => {
    const response = await apiClient.put<SecuritySystemSettings>('/systemsettings', settings);
    return response.data;
  },

  /**
   * Upload an SSL certificate (PEM/CRT or PFX).
   * Uses multipart/form-data — caller must pass a FormData instance.
   */
  uploadSslCertificate: async (formData: FormData): Promise<{ message?: string }> => {
    const response = await apiClient.post<{ message?: string }>('/systemsettings/ssl/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  /**
   * Enable or disable HTTPS.
   */
  toggleHttps: async (request: SslToggleRequest): Promise<{ message?: string }> => {
    const response = await apiClient.post<{ message?: string }>('/systemsettings/ssl/toggle', request);
    return response.data;
  },

  /**
   * Remove the current SSL certificate and disable HTTPS.
   */
  removeSslCertificate: async (): Promise<void> => {
    await apiClient.delete('/systemsettings/ssl');
  },

  /**
   * Generate a self-signed SSL certificate.
   */
  generateSelfSignedCertificate: async (request: SslGenerateRequest): Promise<SslGenerateResult> => {
    const response = await apiClient.post<SslGenerateResult>('/systemsettings/ssl/generate', request);
    return response.data;
  },

  // ── Two-Factor Authentication ────────────────────────────────────────────

  /**
   * Initiate 2FA setup — returns QR code and backup codes.
   */
  setup2FA: async (): Promise<TwoFactorSetupData> => {
    const response = await apiClient.post<TwoFactorSetupData>('/auth/2fa/setup');
    return response.data;
  },

  /**
   * Enable 2FA with verified secret and backup codes.
   */
  enable2FA: async (request: TwoFactorEnableRequest): Promise<void> => {
    await apiClient.post('/auth/2fa/enable', request);
  },

  /**
   * Verify a 2FA code.
   */
  verify2FA: async (request: TwoFactorVerifyRequest): Promise<TwoFactorVerifyResult> => {
    const response = await apiClient.post<TwoFactorVerifyResult>('/auth/2fa/verify', request);
    return response.data;
  },

  /**
   * Disable 2FA for the current user (after code verification).
   */
  disable2FA: async (): Promise<void> => {
    await apiClient.post('/auth/2fa/disable');
  },
};

export default securitySettingsService;
