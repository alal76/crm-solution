import apiClient from './apiClient';

export interface ServiceQueueDto {
  id: number;
  name: string;
  description?: string;
  priority: number;
  isActive: boolean;
  assignmentGroup?: string;
  defaultSLAPolicyId?: number;
  maxQueueDepth?: number;
  routingConfiguration?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateServiceQueueDto {
  name: string;
  description?: string;
  priority?: number;
  isActive?: boolean;
  assignmentGroup?: string;
  defaultSLAPolicyId?: number;
  maxQueueDepth?: number;
  routingConfiguration?: string;
}

export interface UpdateServiceQueueDto extends Partial<CreateServiceQueueDto> {}

export interface ServiceRequestQueueItemDto {
  id: number;
  title: string;
  priority: string;
  status: string;
  assignedTo?: string;
  createdAt: string;
}

const serviceQueueService = {
  getAll: async (): Promise<ServiceQueueDto[]> => {
    const response = await apiClient.get<ServiceQueueDto[]>('/servicequeues');
    return response.data;
  },

  getById: async (id: number): Promise<ServiceQueueDto> => {
    const response = await apiClient.get<ServiceQueueDto>(`/servicequeues/${id}`);
    return response.data;
  },

  create: async (dto: CreateServiceQueueDto): Promise<ServiceQueueDto> => {
    const response = await apiClient.post<ServiceQueueDto>('/servicequeues', dto);
    return response.data;
  },

  update: async (id: number, dto: UpdateServiceQueueDto): Promise<ServiceQueueDto> => {
    const response = await apiClient.put<ServiceQueueDto>(`/servicequeues/${id}`, dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/servicequeues/${id}`);
  },

  assignToQueue: async (serviceRequestId: number, queueId: number): Promise<void> => {
    await apiClient.post(`/servicequeues/${queueId}/assign/${serviceRequestId}`);
  },

  getQueueItems: async (queueId: number): Promise<ServiceRequestQueueItemDto[]> => {
    const response = await apiClient.get<ServiceRequestQueueItemDto[]>(`/servicequeues/${queueId}/items`);
    return response.data;
  },

  getQueueStats: async (queueId: number): Promise<ServiceQueueDto> => {
    const response = await apiClient.get<ServiceQueueDto>(`/servicequeues/${queueId}/stats`);
    return response.data;
  },
};

export default serviceQueueService;
