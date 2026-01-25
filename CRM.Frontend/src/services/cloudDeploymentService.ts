import apiClient from './apiClient';

// Types
export interface CloudProvider {
  id: number;
  name: string;
  providerType: string;
  description?: string;
  region?: string;
  endpoint?: string;
  isActive: boolean;
  isDefault: boolean;
  createdAt: string;
  deploymentCount: number;
}

export interface CreateCloudProviderRequest {
  name: string;
  providerType: 'AWS' | 'Azure' | 'GoogleCloud' | 'DigitalOcean' | 'Kubernetes' | 'Docker' | 'OnPremise';
  description?: string;
  accessKeyId?: string;
  secretAccessKey?: string;
  tenantId?: string;
  subscriptionId?: string;
  projectId?: string;
  region?: string;
  endpoint?: string;
  configuration?: Record<string, string>;
  isDefault?: boolean;
}

export interface UpdateCloudProviderRequest {
  name?: string;
  description?: string;
  accessKeyId?: string;
  secretAccessKey?: string;
  tenantId?: string;
  subscriptionId?: string;
  projectId?: string;
  region?: string;
  endpoint?: string;
  configuration?: Record<string, string>;
  isActive?: boolean;
  isDefault?: boolean;
}

export interface ProviderConnectionResult {
  success: boolean;
  message: string;
  availableRegions: string[];
  availableResources: ResourceOption[];
}

export interface ResourceOption {
  id: string;
  name: string;
  type: string;
  region?: string;
  status?: string;
}

export interface CloudDeployment {
  id: number;
  name: string;
  description?: string;
  cloudProviderId: number;
  providerName: string;
  providerType: string;
  clusterName?: string;
  namespace?: string;
  resourceGroup?: string;
  backendVersion?: string;
  frontendVersion?: string;
  frontendUrl?: string;
  apiUrl?: string;
  domainName?: string;
  sslEnabled: boolean;
  cpuUnits: number;
  memoryMb: number;
  replicas: number;
  status: string;
  healthStatus: string;
  lastHealthCheck?: string;
  deployedAt?: string;
  lastError?: string;
  createdAt: string;
  attemptCount: number;
}

export interface CreateDeploymentRequest {
  name: string;
  description?: string;
  cloudProviderId: number;
  clusterName?: string;
  namespace?: string;
  resourceGroup?: string;
  vpcId?: string;
  subnetIds?: string[];
  backendImage?: string;
  frontendImage?: string;
  databaseImage?: string;
  domainName?: string;
  sslEnabled?: boolean;
  cpuUnits?: number;
  memoryMb?: number;
  replicas?: number;
  environmentVariables?: Record<string, string>;
}

export interface UpdateDeploymentRequest {
  name?: string;
  description?: string;
  clusterName?: string;
  namespace?: string;
  domainName?: string;
  sslEnabled?: boolean;
  cpuUnits?: number;
  memoryMb?: number;
  replicas?: number;
  environmentVariables?: Record<string, string>;
}

export interface TriggerDeploymentRequest {
  deploymentId: number;
  backendVersion?: string;
  frontendVersion?: string;
  gitBranch?: string;
  gitCommitHash?: string;
  forceBuild?: boolean;
}

export interface DeploymentResult {
  success: boolean;
  message: string;
  attemptId?: number;
  frontendUrl?: string;
  apiUrl?: string;
  buildLog?: string;
  deployLog?: string;
}

export interface DeploymentAttempt {
  id: number;
  cloudDeploymentId: number;
  deploymentName: string;
  attemptNumber: string;
  status: string;
  gitCommitHash?: string;
  gitBranch?: string;
  buildNumber?: string;
  backendImageTag?: string;
  frontendImageTag?: string;
  startedAt: string;
  completedAt?: string;
  durationSeconds?: number;
  buildLog?: string;
  deployLog?: string;
  errorMessage?: string;
  triggeredByUserId?: number;
  triggeredByUser?: string;
  triggerType?: string;
}

export interface HealthCheck {
  id: number;
  cloudDeploymentId: number;
  deploymentName: string;
  status: string;
  checkedAt: string;
  apiHealthy?: boolean;
  frontendHealthy?: boolean;
  databaseHealthy?: boolean;
  apiResponseTimeMs?: number;
  frontendResponseTimeMs?: number;
  databaseResponseTimeMs?: number;
  apiResponse?: string;
  frontendResponse?: string;
  errorDetails?: string;
}

export interface HealthCheckResult {
  success: boolean;
  overallStatus: string;
  checkedAt: string;
  api: ComponentHealth;
  frontend: ComponentHealth;
  database: ComponentHealth;
  message?: string;
}

export interface ComponentHealth {
  healthy: boolean;
  responseTimeMs: number;
  response?: string;
  error?: string;
}

export interface DeploymentDashboard {
  totalProviders: number;
  activeProviders: number;
  totalDeployments: number;
  runningDeployments: number;
  healthyDeployments: number;
  failedDeployments: number;
  recentDeployments: DeploymentSummary[];
  recentAttempts: DeploymentAttemptSummary[];
  recentHealthChecks: HealthCheck[];
}

export interface DeploymentSummary {
  id: number;
  name: string;
  providerType: string;
  status: string;
  healthStatus: string;
  frontendUrl?: string;
  deployedAt?: string;
}

export interface DeploymentAttemptSummary {
  id: number;
  attemptNumber: string;
  status: string;
  gitBranch?: string;
  backendImageTag?: string;
  frontendImageTag?: string;
  startedAt: string;
  completedAt?: string;
  durationSeconds?: number;
  triggerType?: string;
  errorMessage?: string;
}

// Cloud Deployment API Service
const cloudDeploymentService = {
  // Cloud Providers
  getProviders: async (): Promise<CloudProvider[]> => {
    const response = await apiClient.get('/clouddeployment/providers');
    return response.data;
  },

  getProvider: async (id: number): Promise<CloudProvider> => {
    const response = await apiClient.get(`/clouddeployment/providers/${id}`);
    return response.data;
  },

  createProvider: async (request: CreateCloudProviderRequest): Promise<CloudProvider> => {
    const response = await apiClient.post('/clouddeployment/providers', request);
    return response.data;
  },

  updateProvider: async (id: number, request: UpdateCloudProviderRequest): Promise<CloudProvider> => {
    const response = await apiClient.put(`/clouddeployment/providers/${id}`, request);
    return response.data;
  },

  deleteProvider: async (id: number): Promise<void> => {
    await apiClient.delete(`/clouddeployment/providers/${id}`);
  },

  testProviderConnection: async (providerId: number): Promise<ProviderConnectionResult> => {
    const response = await apiClient.post('/clouddeployment/providers/test', { providerId });
    return response.data;
  },

  getProviderResources: async (providerId: number, resourceType: string): Promise<ResourceOption[]> => {
    const response = await apiClient.get(`/clouddeployment/providers/${providerId}/resources/${resourceType}`);
    return response.data;
  },

  // Deployments
  getDeployments: async (providerId?: number, status?: string): Promise<CloudDeployment[]> => {
    const params = new URLSearchParams();
    if (providerId) params.append('providerId', providerId.toString());
    if (status) params.append('status', status);
    const response = await apiClient.get(`/clouddeployment/deployments?${params.toString()}`);
    return response.data;
  },

  getDeployment: async (id: number): Promise<CloudDeployment> => {
    const response = await apiClient.get(`/clouddeployment/deployments/${id}`);
    return response.data;
  },

  createDeployment: async (request: CreateDeploymentRequest): Promise<CloudDeployment> => {
    const response = await apiClient.post('/clouddeployment/deployments', request);
    return response.data;
  },

  updateDeployment: async (id: number, request: UpdateDeploymentRequest): Promise<CloudDeployment> => {
    const response = await apiClient.put(`/clouddeployment/deployments/${id}`, request);
    return response.data;
  },

  deleteDeployment: async (id: number): Promise<void> => {
    await apiClient.delete(`/clouddeployment/deployments/${id}`);
  },

  triggerDeployment: async (id: number, request: TriggerDeploymentRequest): Promise<DeploymentResult> => {
    const response = await apiClient.post(`/clouddeployment/deployments/${id}/deploy`, request);
    return response.data;
  },

  stopDeployment: async (id: number): Promise<DeploymentResult> => {
    const response = await apiClient.post(`/clouddeployment/deployments/${id}/stop`);
    return response.data;
  },

  restartDeployment: async (id: number): Promise<DeploymentResult> => {
    const response = await apiClient.post(`/clouddeployment/deployments/${id}/restart`);
    return response.data;
  },

  scaleDeployment: async (id: number, replicas: number): Promise<DeploymentResult> => {
    const response = await apiClient.post(`/clouddeployment/deployments/${id}/scale?replicas=${replicas}`);
    return response.data;
  },

  // Deployment Attempts
  getDeploymentAttempts: async (deploymentId: number): Promise<DeploymentAttempt[]> => {
    const response = await apiClient.get(`/clouddeployment/deployments/${deploymentId}/attempts`);
    return response.data;
  },

  getDeploymentAttempt: async (attemptId: number): Promise<DeploymentAttempt> => {
    const response = await apiClient.get(`/clouddeployment/attempts/${attemptId}`);
    return response.data;
  },

  getDeploymentAttemptLogs: async (attemptId: number): Promise<string> => {
    const response = await apiClient.get(`/clouddeployment/attempts/${attemptId}/logs`);
    return response.data.logs;
  },

  // Health Checks
  runHealthCheck: async (deploymentId: number): Promise<HealthCheckResult> => {
    const response = await apiClient.post(`/clouddeployment/deployments/${deploymentId}/health-check`);
    return response.data;
  },

  getHealthCheckHistory: async (deploymentId: number, limit?: number): Promise<HealthCheck[]> => {
    const params = limit ? `?limit=${limit}` : '';
    const response = await apiClient.get(`/clouddeployment/deployments/${deploymentId}/health-history${params}`);
    return response.data;
  },

  getAllDeploymentHealth: async (): Promise<HealthCheck[]> => {
    const response = await apiClient.get('/clouddeployment/health');
    return response.data;
  },

  // Dashboard
  getDashboard: async (): Promise<DeploymentDashboard> => {
    const response = await apiClient.get('/clouddeployment/dashboard');
    return response.data;
  },
};

export default cloudDeploymentService;
