/**
 * Monitoring Service — AP-041
 * Centralizes all monitoring and system-controls API calls.
 * Replaces direct fetch() calls in MonitoringDashboard.tsx.
 */

import apiClient from './apiClient';

export interface EnvironmentInfo {
  deploymentType: string;
  isDocker: boolean;
  isKubernetes: boolean;
  databaseProvider: string;
  databaseConnected: boolean;
  hostname: string;
  version: string;
  dotNetVersion?: string;
  enabledMonitors?: string[];
}

export interface ExternalToolStatus {
  status: 'online' | 'offline' | 'degraded' | 'error';
  version?: string;
  url?: string;
  port?: number;
  message?: string;
}

export interface MonitoringToolsData {
  uptimeKuma: ExternalToolStatus;
  portainer: ExternalToolStatus;
  timestamp: string;
}

export interface UptimeKumaMonitor {
  id: string;
  status: number; // 0 = down, 1 = up, 2 = pending
  ping: number;
  time: string;
  msg?: string;
}

export interface UptimeKumaMonitorsData {
  connected: boolean;
  monitors: UptimeKumaMonitor[];
  uptimeList?: Record<string, number>;
  monitorCount: number;
  message?: string;
}

export interface PortainerData {
  connected: boolean;
  version?: string;
  instanceId?: string;
  message?: string;
}

export interface RateLimitStatus {
  isEnabled: boolean;
  enabled?: boolean;
  lastChangedAt?: string | null;
}

export interface RateLimitToggleResult {
  isEnabled: boolean;
  changedAt?: string;
}

export interface JwtRotationInfo {
  fingerprint?: string;
  lastRotatedAt?: string | null;
  lastRotatedBy?: string | null;
}

export interface JwtRotateResult {
  message?: string;
  newFingerprint?: string;
  error?: string;
}

const monitoringService = {
  /**
   * Fetch environment information from the monitoring API.
   */
  getEnvironmentInfo: async (): Promise<EnvironmentInfo> => {
    const response = await apiClient.get<EnvironmentInfo>('/monitoring/environment');
    return response.data;
  },

  /**
   * Fetch the status of all external monitoring tools (Uptime Kuma, Portainer).
   */
  getToolsStatus: async (): Promise<MonitoringToolsData> => {
    const response = await apiClient.get<MonitoringToolsData>('/monitoring/tools/status');
    return response.data;
  },

  /**
   * Fetch Uptime Kuma monitor data.
   */
  getUptimeKumaMonitors: async (): Promise<UptimeKumaMonitorsData> => {
    const response = await apiClient.get<UptimeKumaMonitorsData>('/monitoring/uptime-kuma/monitors');
    return response.data;
  },

  /**
   * Fetch Portainer container data.
   */
  getPortainerContainers: async (): Promise<PortainerData> => {
    const response = await apiClient.get<PortainerData>('/monitoring/portainer/containers');
    return response.data;
  },

  /**
   * Fetch the current rate-limiting status.
   */
  getRateLimitStatus: async (): Promise<RateLimitStatus> => {
    const response = await apiClient.get<RateLimitStatus>('/system-controls/rate-limiting');
    return response.data;
  },

  /**
   * Enable or disable rate limiting.
   */
  setRateLimit: async (enable: boolean): Promise<RateLimitToggleResult> => {
    const action = enable ? 'enable' : 'disable';
    const response = await apiClient.post<RateLimitToggleResult>(`/system-controls/rate-limiting/${action}`);
    return response.data;
  },

  /**
   * Fetch JWT rotation info (fingerprint, last rotated).
   */
  getJwtRotationInfo: async (): Promise<JwtRotationInfo> => {
    const response = await apiClient.get<JwtRotationInfo>('/system-controls/jwt-rotation');
    return response.data;
  },

  /**
   * Rotate the JWT signing secret. All active tokens become invalid.
   */
  rotateJwtSecret: async (): Promise<JwtRotateResult> => {
    const response = await apiClient.post<JwtRotateResult>('/system-controls/jwt-rotation/rotate');
    return response.data;
  },
};

export default monitoringService;
