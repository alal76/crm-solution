import apiClient from './apiClient';

// ─── Types (mirrors CRM.Core.Dtos.FeatureFlagDto / UpdateFeatureFlagDto) ─────

export interface FeatureFlagDto {
  name: string;
  displayName: string;
  description: string;
  enabled: boolean;
  category: string;
  providerCategory?: string | null;
  activeProvider?: string | null;
  requiresRestart: boolean;
  rolloutPercentage: number;
  targetedUserIds: string[];
  targetedRoles: string[];
}

export interface UpdateFeatureFlagRequest {
  name: string;
  enabled: boolean;
  rolloutPercentage?: number;
  reason?: string;
  targetedUserIds?: string[];
  targetedRoles?: string[];
}

// ─── Service ─────────────────────────────────────────────────────────────────

const BASE = '/feature-flags';

export const featureFlagService = {
  /** Fetch all feature flags (module + provider selection flags) */
  getAllFlags: async (): Promise<FeatureFlagDto[]> => {
    const response = await apiClient.get<FeatureFlagDto[]>(BASE);
    return response.data;
  },

  /** Fetch a single feature flag by name */
  getFlag: async (flagName: string): Promise<FeatureFlagDto> => {
    const response = await apiClient.get<FeatureFlagDto>(`${BASE}/${flagName}`);
    return response.data;
  },

  /** Update (enable/disable) a feature flag */
  updateFlag: async (flagName: string, request: UpdateFeatureFlagRequest): Promise<void> => {
    await apiClient.put(`${BASE}/${flagName}`, request);
  },
};
