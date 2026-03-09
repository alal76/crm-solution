/**
 * Feature Management Service — AP-043
 * Centralizes all feature-flag and database-sync API calls from FeatureManagementTab.tsx.
 */

import apiClient from './apiClient';

export interface FeatureStatus {
  enabled: boolean;
  name: string;
  description: string;
}

export interface FeatureConfiguration {
  coreModules: {
    customers: FeatureStatus;
    contacts: FeatureStatus;
    leads: FeatureStatus;
    opportunities: FeatureStatus;
    products: FeatureStatus;
    services: FeatureStatus;
  };
  salesModules: {
    campaigns: FeatureStatus;
    quotes: FeatureStatus;
  };
  productivityModules: {
    tasks: FeatureStatus;
    activities: FeatureStatus;
    notes: FeatureStatus;
  };
  automationModules: {
    workflows: FeatureStatus;
  };
  analyticsModules: {
    reports: FeatureStatus;
    dashboard: FeatureStatus;
  };
  communicationModules: {
    email: FeatureStatus;
    whatsapp: FeatureStatus;
    socialMedia: FeatureStatus;
  };
  systemSettings?: {
    demoModeEnabled: boolean;
    useDemoDatabase: boolean;
  };
  databaseProviders: {
    mariadb: FeatureStatus;
    postgresql: FeatureStatus;
    sqlserver: FeatureStatus;
    sqlite: FeatureStatus;
    mysql: FeatureStatus;
  };
  activeDatabaseProvider: string;
}

export interface DatabaseStatus {
  productionDatabase: {
    name: string;
    isActive: boolean;
    modules: Record<string, number>;
  };
  demoDatabase?: {
    name: string;
    isActive: boolean;
    modules: Record<string, number>;
  };
  inSync: boolean;
  lastChecked: string;
}

export interface FeatureUpdateRequest {
  customersEnabled?: boolean;
  contactsEnabled?: boolean;
  leadsEnabled?: boolean;
  opportunitiesEnabled?: boolean;
  productsEnabled?: boolean;
  servicesEnabled?: boolean;
  campaignsEnabled?: boolean;
  quotesEnabled?: boolean;
  tasksEnabled?: boolean;
  activitiesEnabled?: boolean;
  notesEnabled?: boolean;
  workflowsEnabled?: boolean;
  reportsEnabled?: boolean;
  dashboardEnabled?: boolean;
  emailEnabled?: boolean;
  whatsAppEnabled?: boolean;
  socialMediaEnabled?: boolean;
  activeDatabaseProvider?: string;
}

export interface DatabaseSyncResult {
  fieldsSynced: number;
  message?: string;
}

const featureManagementService = {
  /**
   * Fetch the current feature flag configuration.
   */
  getFeatures: async (): Promise<FeatureConfiguration> => {
    const response = await apiClient.get<FeatureConfiguration>('/systemsettings/features');
    return response.data;
  },

  /**
   * Fetch the current database sync status.
   */
  getDatabaseStatus: async (): Promise<DatabaseStatus> => {
    const response = await apiClient.get<DatabaseStatus>('/systemsettings/database/status');
    return response.data;
  },

  /**
   * Save updated feature flag settings.
   */
  updateFeatures: async (request: FeatureUpdateRequest): Promise<void> => {
    await apiClient.put('/systemsettings/features', request);
  },

  /**
   * Trigger a database synchronisation.
   */
  syncDatabases: async (): Promise<DatabaseSyncResult> => {
    const response = await apiClient.post<DatabaseSyncResult>('/systemsettings/database/sync');
    return response.data;
  },
};

export default featureManagementService;
