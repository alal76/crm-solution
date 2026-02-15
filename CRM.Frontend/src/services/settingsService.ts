import apiClient from './apiClient';

/**
 * System Settings DTO
 */
export interface SystemSettingsDto {
  id: number;
  organizationName: string;
  solutionName: string;
  smtpHost: string;
  smtpPort: number;
  smtpUsername: string;
  smtpPassword?: string;
  smtpUseSSL: boolean;
  defaultTimezone: string;
  defaultCurrency: string;
  defaultLanguage: string;
  dateFormat: string;
  timeFormat: string;
  logoUrl: string;
  faviconUrl: string;
  rateLimitPerMinute: number;
  accountsEnabled: boolean;
  contactsEnabled: boolean;
  leadsEnabled: boolean;
  opportunitiesEnabled: boolean;
  productsEnabled: boolean;
  campaignsEnabled: boolean;
  quoteEnabled: boolean;
  createdAt?: string;
  updatedAt?: string;
}

/**
 * Settings service for managing system configuration
 * Handles CRUD operations for system and feature settings
 */
export const settingsService = {
  /**
   * Get all system settings
   */
  getSettings: async (): Promise<SystemSettingsDto> => {
    const response = await apiClient.get<SystemSettingsDto>('/settings');
    return response.data;
  },

  /**
   * Update system settings
   */
  updateSettings: async (settings: Partial<SystemSettingsDto>): Promise<SystemSettingsDto> => {
    const response = await apiClient.put<SystemSettingsDto>('/settings', settings);
    return response.data;
  },

  /**
   * Get specific setting value by key
   */
  getSettingByKey: async (key: string): Promise<any> => {
    const response = await apiClient.get(`/settings/${key}`);
    return response.data;
  },

  /**
   * Update specific setting value by key
   */
  updateSettingByKey: async (key: string, value: any): Promise<any> => {
    const response = await apiClient.put(`/settings/${key}`, { value });
    return response.data;
  },

  /**
   * Reset settings to default values
   */
  resetToDefaults: async (): Promise<SystemSettingsDto> => {
    const response = await apiClient.post<SystemSettingsDto>('/settings/reset');
    return response.data;
  },

  /**
   * Validate SMTP settings
   */
  validateSmtpSettings: async (
    host: string,
    port: number,
    username: string,
    password: string,
    useSSL: boolean
  ): Promise<{ isValid: boolean; message: string }> => {
    const response = await apiClient.post('/settings/smtp/validate', {
      host,
      port,
      username,
      password,
      useSSL,
    });
    return response.data;
  },

  /**
   * Get provider health status
   */
  getProviderStatus: async (): Promise<Record<string, any>> => {
    const response = await apiClient.get('/health/providers');
    return response.data;
  },
};

/**
 * Default export for backward compatibility
 */
export default settingsService;
