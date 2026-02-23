/**
 * Configuration Management API Service
 * Handles all API calls for the Unified Configuration Management System
 */
import apiClient from './apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

export interface EmailServerConfigDto {
  smtpServer: string;
  smtpPort: number;
  useTls: boolean;
  fromEmail: string;
  fromName: string;
  username?: string;
  password?: string;
  isConfigured: boolean;
  lastTested?: string;
  connectionStatus?: string;
  testError?: string;
}

export interface TwoFactorConfigDto {
  provider: string;
  required: boolean;
  smsProvider?: string;
  twilioAccountSid?: string;
  twilioAuthToken?: string;
  twilioFromNumber?: string;
  issuer?: string;
}

export interface SocialLoginProviderConfig {
  enabled: boolean;
  clientId?: string;
  clientSecret?: string;
  tenantId?: string;
  authority?: string;
  appId?: string;
  appSecret?: string;
}

export interface SocialLoginConfigDto {
  google?: SocialLoginProviderConfig;
  microsoft?: SocialLoginProviderConfig;
  azureAd?: SocialLoginProviderConfig;
  linkedIn?: SocialLoginProviderConfig;
  facebook?: SocialLoginProviderConfig;
}

export interface AIProviderConfigDto {
  provider: string;
  enabled: boolean;
  apiKey?: string;
  apiUrl?: string;
  organizationId?: string;
  model?: string;
  temperature?: number;
  maxTokens?: number;
  costTrackingEnabled: boolean;
  lastTested?: string;
  connectionStatus?: string;
  testError?: string;
}

export interface IntegrationConfigDto {
  type: string;
  provider: string;
  enabled: boolean;
  useBuiltIn: boolean;
  configuration?: Record<string, any>;
  credentials?: Record<string, string>;
  testEndpoint?: string;
  lastTested?: string;
  connectionStatus?: string;
  testError?: string;
}

export interface WorkerConfigDto {
  enabled: boolean;
  maxConcurrentJobs: number;
  jobTimeoutMinutes: number;
  retryAttempts: number;
  retryDelaySeconds: number;
  scheduleExpression?: string;
}

export interface AIAgentConfigDto {
  id: number;
  name: string;
  description?: string;
  enabled: boolean;
  iconUrl?: string;
  settings?: Record<string, any>;
}

export interface SystemConfigResponseDto {
  emailServer?: EmailServerConfigDto;
  twoFactor?: TwoFactorConfigDto;
  socialLogin?: SocialLoginConfigDto;
  lastUpdated: string;
  updatedBy?: string;
}

export interface CRMConfigResponseDto {
  aiProviders: AIProviderConfigDto[];
  integrations: IntegrationConfigDto[];
  workerConfig?: WorkerConfigDto;
  aiAgents: AIAgentConfigDto[];
  lastUpdated: string;
  updatedBy?: string;
}

export interface ConfigurationTestResultDto {
  success: boolean;
  message?: string;
  errorDetails?: string;
  testedAt: string;
}

export interface ConfigurationChangeLogDto {
  id: number;
  configurationKey: string;
  oldValue?: string;
  newValue?: string;
  changeType: string;
  changedAt: string;
  changedByUserName?: string;
  ipAddress?: string;
}

export interface ProviderInfoDto {
  name: string;
  type: string;
  description?: string;
  isBuiltIn: boolean;
  isConfigured: boolean;
}

// ── Service ──────────────────────────────────────────────────────────────────

export const configurationService = {
  // ── System Configuration ────────────────────────────────────────────────

  getSystemConfig: async (): Promise<SystemConfigResponseDto> => {
    const response = await apiClient.get<SystemConfigResponseDto>('/admin/config/system');
    return response.data;
  },

  updateEmailConfig: async (config: EmailServerConfigDto): Promise<void> => {
    await apiClient.put('/admin/config/system/email', config);
  },

  testEmailConfig: async (config: EmailServerConfigDto): Promise<ConfigurationTestResultDto> => {
    const response = await apiClient.post<ConfigurationTestResultDto>(
      '/admin/config/system/email/test',
      config
    );
    return response.data;
  },

  updateTwoFactorConfig: async (config: TwoFactorConfigDto): Promise<void> => {
    await apiClient.put('/admin/config/system/2fa', config);
  },

  updateSocialLoginConfig: async (config: SocialLoginConfigDto): Promise<void> => {
    await apiClient.put('/admin/config/system/social', config);
  },

  testSocialLoginProvider: async (
    provider: string,
    config: Record<string, any>
  ): Promise<ConfigurationTestResultDto> => {
    const response = await apiClient.post<ConfigurationTestResultDto>(
      `/admin/config/system/social/${provider}/test`,
      config
    );
    return response.data;
  },

  // ── CRM Configuration ──────────────────────────────────────────────────

  getCRMConfig: async (): Promise<CRMConfigResponseDto> => {
    const response = await apiClient.get<CRMConfigResponseDto>('/admin/config/crm');
    return response.data;
  },

  updateAIProviderConfig: async (
    provider: string,
    config: AIProviderConfigDto
  ): Promise<void> => {
    await apiClient.put(`/admin/config/crm/ai/${provider}`, config);
  },

  testAIProvider: async (
    provider: string,
    config: AIProviderConfigDto
  ): Promise<ConfigurationTestResultDto> => {
    const response = await apiClient.post<ConfigurationTestResultDto>(
      `/admin/config/crm/ai/${provider}/test`,
      config
    );
    return response.data;
  },

  updateIntegrationConfig: async (
    type: string,
    provider: string,
    config: IntegrationConfigDto
  ): Promise<void> => {
    await apiClient.put(`/admin/config/crm/integration/${type}/${provider}`, config);
  },

  testIntegration: async (
    type: string,
    provider: string,
    config: IntegrationConfigDto
  ): Promise<ConfigurationTestResultDto> => {
    const response = await apiClient.post<ConfigurationTestResultDto>(
      `/admin/config/crm/integration/${type}/${provider}/test`,
      config
    );
    return response.data;
  },

  updateWorkerConfig: async (config: WorkerConfigDto): Promise<void> => {
    await apiClient.put('/admin/config/crm/worker', config);
  },

  updateAIAgentsConfig: async (agents: AIAgentConfigDto[]): Promise<void> => {
    await apiClient.put('/admin/config/crm/agents', agents);
  },

  // ── Changelog & Providers ──────────────────────────────────────────────

  getChangelog: async (
    configKey?: string,
    pageSize: number = 50
  ): Promise<ConfigurationChangeLogDto[]> => {
    const params: Record<string, any> = { pageSize };
    if (configKey) params.configKey = configKey;
    const response = await apiClient.get<ConfigurationChangeLogDto[]>(
      '/admin/config/changelog',
      { params }
    );
    return response.data;
  },

  rollbackChange: async (changeId: number): Promise<ConfigurationTestResultDto> => {
    const response = await apiClient.post<ConfigurationTestResultDto>(
      `/admin/config/changelog/${changeId}/rollback`
    );
    return response.data;
  },

  getProviders: async (type: string): Promise<ProviderInfoDto[]> => {
    const response = await apiClient.get<ProviderInfoDto[]>(
      `/admin/config/providers/${type}`
    );
    return response.data;
  },
};

export default configurationService;
