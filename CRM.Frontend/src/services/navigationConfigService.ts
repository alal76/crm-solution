/**
 * Navigation Configuration Service
 * 
 * Fetches dynamic navigation configuration from the backend at startup.
 * The backend is aware of the pluggable architecture and returns navigation
 * items configured for internal routes or external provider URLs.
 * 
 * This service runs at application startup to ensure navigation is always
 * up-to-date with the deployed configuration.
 */

import apiClient from './apiClient';

/**
 * Navigation item configuration returned from the backend
 */
export interface NavigationItemConfig {
  id: string;
  label: string;
  path: string;
  icon: string;
  menuName: string;
  category: string;
  adminSubcategory?: string;
  order: number;
  visible: boolean;
  enabled: boolean;
  requiredFeature?: string;
  requiredProvider?: string;
  moduleName?: string;
  isExternal: boolean;
  externalUrl?: string;
  providerType?: string;
}

/**
 * Navigation category configuration
 */
export interface NavigationCategoryConfig {
  id: string;
  name: string;
  order: number;
  visible: boolean;
  items: NavigationItemConfig[];
}

/**
 * Navigation subcategory for admin section
 */
export interface NavigationSubcategoryConfig {
  id: string;
  name: string;
  order: number;
  visible: boolean;
  items: NavigationItemConfig[];
}

/**
 * External service configuration for provider integration
 */
export interface ExternalServiceConfig {
  providerType: string;
  displayName: string;
  baseUrl: string;
  isEnabled: boolean;
  healthStatus: 'Healthy' | 'Degraded' | 'Unhealthy' | 'Unknown';
  navItems: string[];
}

/**
 * Provider health status
 */
export interface ProviderStatus {
  providerType: string;
  isAvailable: boolean;
  lastChecked: string;
  latencyMs?: number;
  errorMessage?: string;
}

/**
 * Complete navigation configuration from backend
 */
export interface NavigationConfig {
  categories: NavigationCategoryConfig[];
  adminCategories: NavigationSubcategoryConfig[];
  allItems: NavigationItemConfig[];
  externalServices: ExternalServiceConfig[];
  lastUpdated: string;
  cacheExpiresAt: string;
}

// In-memory cache for navigation config
let cachedConfig: NavigationConfig | null = null;
let cacheExpiresAt: Date | null = null;

/**
 * Fetches the complete navigation configuration from the backend.
 * Results are cached to avoid repeated API calls during the session.
 */
export const getNavigationConfig = async (): Promise<NavigationConfig> => {
  // Check if we have a valid cached config
  if (cachedConfig && cacheExpiresAt && new Date() < cacheExpiresAt) {
    return cachedConfig;
  }

  try {
    const response = await apiClient.get<NavigationConfig>('/navigation/config');
    cachedConfig = response.data;
    
    // Set cache expiration from server response or default to 5 minutes
    if (response.data.cacheExpiresAt) {
      cacheExpiresAt = new Date(response.data.cacheExpiresAt);
    } else {
      cacheExpiresAt = new Date(Date.now() + 5 * 60 * 1000);
    }
    
    return cachedConfig;
  } catch (error) {
    console.error('Failed to fetch navigation config:', error);
    // Return empty config on error - fallback to static config will be handled by consumer
    throw error;
  }
};

/**
 * Fetches only the available navigation items (lightweight endpoint)
 */
export const getNavigationItems = async (): Promise<NavigationItemConfig[]> => {
  try {
    const response = await apiClient.get<NavigationItemConfig[]>('/navigation/items');
    return response.data;
  } catch (error) {
    console.error('Failed to fetch navigation items:', error);
    throw error;
  }
};

/**
 * Fetches external service configurations (requires authentication)
 */
export const getExternalServiceConfigs = async (): Promise<ExternalServiceConfig[]> => {
  try {
    const response = await apiClient.get<ExternalServiceConfig[]>('/navigation/external-services');
    return response.data;
  } catch (error) {
    console.error('Failed to fetch external service configs:', error);
    throw error;
  }
};

/**
 * Fetches provider health status (requires authentication)
 */
export const getProviderStatus = async (): Promise<ProviderStatus[]> => {
  try {
    const response = await apiClient.get<ProviderStatus[]>('/navigation/provider-status');
    return response.data;
  } catch (error) {
    console.error('Failed to fetch provider status:', error);
    throw error;
  }
};

/**
 * Invalidates the cached navigation configuration.
 * Call this when provider settings are changed.
 */
export const invalidateNavigationCache = (): void => {
  cachedConfig = null;
  cacheExpiresAt = null;
};

/**
 * Converts backend navigation config to the format expected by Navigation.tsx
 * This bridges the gap between the dynamic backend config and the static frontend config
 */
export const convertToNavItemsConfig = (items: NavigationItemConfig[]): Record<string, {
  label: string;
  icon: string;
  path: string;
  order?: number;
  isExternal?: boolean;
  externalUrl?: string;
}> => {
  const config: Record<string, {
    label: string;
    icon: string;
    path: string;
    order?: number;
    isExternal?: boolean;
    externalUrl?: string;
  }> = {};

  items.forEach(item => {
    config[item.id] = {
      label: item.label,
      icon: item.icon,
      path: item.isExternal && item.externalUrl ? item.externalUrl : item.path,
      order: item.order,
      isExternal: item.isExternal,
      externalUrl: item.externalUrl,
    };
  });

  return config;
};

/**
 * Converts backend navigation config to defaultNavItemsWithCategory format
 */
export const convertToDefaultNavItems = (items: NavigationItemConfig[]): Record<string, {
  menuName: string;
  category: string;
  adminSubcategory?: string;
  order: number;
}> => {
  const config: Record<string, {
    menuName: string;
    category: string;
    adminSubcategory?: string;
    order: number;
  }> = {};

  items.filter(item => item.visible).forEach(item => {
    config[item.id] = {
      menuName: item.menuName,
      category: item.category,
      adminSubcategory: item.adminSubcategory,
      order: item.order,
    };
  });

  return config;
};

/**
 * Helper to check if a navigation item should open in a new tab
 */
export const shouldOpenInNewTab = (item: NavigationItemConfig): boolean => {
  return item.isExternal && !!item.externalUrl;
};

/**
 * Get the effective path for a navigation item (internal route or external URL)
 */
export const getEffectivePath = (item: NavigationItemConfig): string => {
  if (item.isExternal && item.externalUrl) {
    return item.externalUrl;
  }
  return item.path;
};

export default {
  getNavigationConfig,
  getNavigationItems,
  getExternalServiceConfigs,
  getProviderStatus,
  invalidateNavigationCache,
  convertToNavItemsConfig,
  convertToDefaultNavItems,
  shouldOpenInNewTab,
  getEffectivePath,
};
