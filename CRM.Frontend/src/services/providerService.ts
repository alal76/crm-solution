import apiClient from './apiClient';

// ─── Types ───────────────────────────────────────────────────────────────────

export interface ProviderConfigField {
  key: string;
  label: string;
  type: 'text' | 'password' | 'url' | 'number' | 'boolean';
  required: boolean;
  defaultValue?: string;
  placeholder?: string;
  helpText?: string;
}

export interface ProviderHealthStatusResult {
  isHealthy: boolean;
  message: string;
  checkedAt: string;
  latencyMs?: number;
  errorDetails?: string;
}

export interface ProviderRegistryEntry {
  category: string;
  providerType: string;
  displayName: string;
  description: string;
  isActive: boolean;
  isConfigured: boolean;
  isBuiltIn: boolean;
  isSaaS: boolean;
  documentationUrl?: string;
  configFields: ProviderConfigField[];
  healthStatus?: ProviderHealthStatusResult;
}

export interface ProviderFeatureFlagsDto {
  useExternalSearch: boolean;
  useExternalChat: boolean;
  useExternalNotifications: boolean;
  useExternalAnalytics: boolean;
  useExternalSignatures: boolean;
  useExternalAI: boolean;
  useExternalIntegrations: boolean;
  activeProviders: Record<string, string>;
}

export interface SetActiveProviderRequest {
  providerType: string;
  config: Record<string, string>;
}

export interface UpdateProviderFlagsRequest {
  flags: Record<string, boolean>;
}

// ─── Provider categories (mirrors ProviderTypes.cs) ──────────────────────────

export const PROVIDER_CATEGORIES = [
  'Search',
  'Chat',
  'Notifications',
  'Analytics',
  'Signatures',
  'AI',
  'Integrations',
  'Accounting',
  'Marketing',
  'Scheduling',
  'GeoIP',
  'LinkedIn',
] as const;

export type ProviderCategory = typeof PROVIDER_CATEGORIES[number];

// Map a category to its feature flag key (mirrors FeatureFlags.cs). The 5 business
// connector categories (Accounting/Marketing/Scheduling/GeoIP/LinkedIn) have no
// corresponding global on/off feature flag on the backend -- they're always-available
// connector categories gated per-provider by configuration, not a subsystem toggle like
// Search/Chat -- so they're intentionally absent from this map rather than given a
// fabricated key.
export const CATEGORY_FEATURE_FLAG: Partial<Record<ProviderCategory, keyof ProviderFeatureFlagsDto>> = {
  Search: 'useExternalSearch',
  Chat: 'useExternalChat',
  Notifications: 'useExternalNotifications',
  Analytics: 'useExternalAnalytics',
  Signatures: 'useExternalSignatures',
  AI: 'useExternalAI',
  Integrations: 'useExternalIntegrations',
};

// ─── Service ─────────────────────────────────────────────────────────────────

const BASE = '/admin/providers';

export const providerService = {
  /** Fetch all providers across all categories */
  getAllProviders: async (): Promise<ProviderRegistryEntry[]> => {
    const response = await apiClient.get<ProviderRegistryEntry[]>(BASE);
    return response.data;
  },

  /** Fetch providers for a specific category */
  getProvidersByCategory: async (category: string): Promise<ProviderRegistryEntry[]> => {
    const response = await apiClient.get<ProviderRegistryEntry[]>(`${BASE}/${category}`);
    return response.data;
  },

  /** Get the active provider configuration for a category */
  getActiveProvider: async (category: string): Promise<unknown> => {
    const response = await apiClient.get(`${BASE}/${category}/active`);
    return response.data;
  },

  /** Activate a provider for a category with its configuration */
  setActiveProvider: async (category: string, request: SetActiveProviderRequest): Promise<void> => {
    await apiClient.put(`${BASE}/${category}/active`, request);
  },

  /** Check provider health */
  checkProviderHealth: async (category: string, type: string): Promise<ProviderHealthStatusResult> => {
    const response = await apiClient.get<ProviderHealthStatusResult>(`${BASE}/${category}/${type}/health`);
    return response.data;
  },

  /** Test provider connectivity */
  testProviderConnection: async (category: string, type: string): Promise<ProviderHealthStatusResult> => {
    const response = await apiClient.post<ProviderHealthStatusResult>(`${BASE}/${category}/${type}/test`);
    return response.data;
  },

  /** Get provider feature flags */
  getFeatureFlags: async (): Promise<ProviderFeatureFlagsDto> => {
    const response = await apiClient.get<ProviderFeatureFlagsDto>(`${BASE}/feature-flags`);
    return response.data;
  },

  /** Update provider feature flags */
  updateFeatureFlags: async (request: UpdateProviderFlagsRequest): Promise<void> => {
    await apiClient.put(`${BASE}/feature-flags`, request);
  },
};
