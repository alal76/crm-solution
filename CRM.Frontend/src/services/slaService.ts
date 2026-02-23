import apiClient from './apiClient';

export interface SLAPolicyDto {
  id: number;
  name: string;
  description?: string;
  priority: string;
  responseTimeMinutes: number;
  resolutionTimeMinutes: number;
  escalationEnabled: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSLAPolicyDto {
  name: string;
  description?: string;
  priority: string;
  responseTimeMinutes: number;
  resolutionTimeMinutes: number;
  escalationEnabled?: boolean;
  isActive?: boolean;
}

export interface UpdateSLAPolicyDto extends Partial<CreateSLAPolicyDto> {}

export interface SLAInstanceDto {
  id: number;
  policyId: number;
  serviceRequestId: number;
  responseDeadline: string;
  resolutionDeadline: string;
  status: string;
}

const slaService = {
  getAll: async (): Promise<SLAPolicyDto[]> => {
    const response = await apiClient.get<SLAPolicyDto[]>('/slapolicies');
    return response.data;
  },

  getById: async (id: number): Promise<SLAPolicyDto> => {
    const response = await apiClient.get<SLAPolicyDto>(`/slapolicies/${id}`);
    return response.data;
  },

  create: async (dto: CreateSLAPolicyDto): Promise<SLAPolicyDto> => {
    const response = await apiClient.post<SLAPolicyDto>('/slapolicies', dto);
    return response.data;
  },

  update: async (id: number, dto: UpdateSLAPolicyDto): Promise<SLAPolicyDto> => {
    const response = await apiClient.put<SLAPolicyDto>(`/slapolicies/${id}`, dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/slapolicies/${id}`);
  },

  assignPolicy: async (policyId: number, serviceRequestId: number): Promise<SLAInstanceDto> => {
    const response = await apiClient.post<SLAInstanceDto>(`/slapolicies/${policyId}/assign/${serviceRequestId}`);
    return response.data;
  },

  getApplicable: async (priority?: string, category?: string): Promise<SLAPolicyDto[]> => {
    const response = await apiClient.get<SLAPolicyDto[]>('/slapolicies/applicable', { params: { priority, category } });
    return response.data;
  },
};

export default slaService;
