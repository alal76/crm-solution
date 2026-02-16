import apiClient from './apiClient';

export interface WorkerHealthStatus {
  status: string;
  timestamp: string;
}

export interface WorkerQueueStats {
  timestamp: string;
  jobs: {
    queued: number;
    inProgress: number;
    completed: number;
    failed: number;
    deadLettered: number;
    total: number;
  };
  outbox: {
    pending: number;
    processing: number;
    completed: number;
    failed: number;
    total: number;
  };
  metrics: {
    oldestQueuedAt?: string | null;
    oldestQueuedAgeSeconds?: number | null;
    lastFailedJobAt?: string | null;
    oldestPendingOutboxAt?: string | null;
    oldestPendingOutboxAgeSeconds?: number | null;
    lastFailedOutboxAt?: string | null;
  };
}

export interface WorkerControlStatus {
  controlState: string;
  maxWorkers: number;
  timestamp: string;
}

export const workerAdminService = {
  getHealth: async (): Promise<WorkerHealthStatus> => {
    const response = await apiClient.get<WorkerHealthStatus>('/workers/health');
    return response.data;
  },
  getStats: async (): Promise<WorkerQueueStats> => {
    const response = await apiClient.get<WorkerQueueStats>('/workers/stats');
    return response.data;
  },
  getControlStatus: async (): Promise<WorkerControlStatus> => {
    const response = await apiClient.get<WorkerControlStatus>('/workers/control');
    return response.data;
  },
  updateMaxWorkers: async (maxWorkers: number): Promise<WorkerControlStatus> => {
    const response = await apiClient.put<WorkerControlStatus>('/workers/control/max-workers', { maxWorkers });
    return response.data;
  },
  startWorkers: async (): Promise<WorkerControlStatus> => {
    const response = await apiClient.post<WorkerControlStatus>('/workers/control/start');
    return response.data;
  },
  stopWorkers: async (): Promise<WorkerControlStatus> => {
    const response = await apiClient.post<WorkerControlStatus>('/workers/control/stop');
    return response.data;
  },
  restartWorkers: async (): Promise<WorkerControlStatus> => {
    const response = await apiClient.post<WorkerControlStatus>('/workers/control/restart');
    return response.data;
  },
};

export default workerAdminService;
