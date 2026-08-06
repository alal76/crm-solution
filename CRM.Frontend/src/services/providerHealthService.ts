import apiClient from './apiClient';

// ─── Types (mirror ProviderHealthController.cs response shapes) ──────────────

/**
 * Health status for a single provider category (Search, Chat, Notifications,
 * Analytics, Signatures, AI, Integrations).
 */
export interface ProviderStatus {
  activeProvider: string;
  isHealthy: boolean;
  error?: string;
  availableProviders: string[];
  lastChecked: string;
}

/**
 * Overall provider health report returned by GET /api/health/providers.
 * `providers` is keyed by category name (e.g. "Search", "AI").
 */
export interface ProviderHealthReport {
  timestamp: string;
  overallHealthy: boolean;
  providers: Record<string, ProviderStatus>;
  registryStats?: unknown;
}

// ─── Provider categories (mirrors ProviderHealthController.cs) ───────────────

export const HEALTH_PROVIDER_CATEGORIES = [
  'Search',
  'Chat',
  'Notifications',
  'Analytics',
  'Signatures',
  'AI',
  'Integrations',
] as const;

export type HealthProviderCategory = typeof HEALTH_PROVIDER_CATEGORIES[number];

// ─── Service ───────────────────────────────────────────────────────────────

export const providerHealthService = {
  /** Fetch the health status of all pluggable providers. */
  getProviderHealth: async (): Promise<ProviderHealthReport> => {
    // NOTE: the backend returns HTTP 503 when overallHealthy is false, but the
    // response body is still a valid ProviderHealthReport, so we don't want
    // axios to treat that as a hard failure for this call.
    const response = await apiClient.get<ProviderHealthReport>('/health/providers', {
      validateStatus: (status) => status === 200 || status === 503,
    });
    return response.data;
  },
};
