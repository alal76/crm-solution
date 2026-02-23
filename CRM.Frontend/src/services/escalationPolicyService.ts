import apiClient from './apiClient';

export interface EscalationLevelDto {
  id?: number;
  levelNumber: number;
  name: string;
  timeThresholdMinutes: number;
  notifyRoles: string[];
  notifyUserIds: number[];
  actions: string[];
}

export interface EscalationPolicyDto {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  levels: EscalationLevelDto[];
  triggerConditions?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEscalationPolicyDto {
  name: string;
  description?: string;
  isActive?: boolean;
  levels: EscalationLevelDto[];
  triggerConditions?: string;
}

const escalationPolicyService = {
  getAll: async (): Promise<EscalationPolicyDto[]> => {
    const response = await apiClient.get<EscalationPolicyDto[]>('/escalationpolicies');
    return response.data;
  },

  getById: async (id: number): Promise<EscalationPolicyDto> => {
    const response = await apiClient.get<EscalationPolicyDto>(`/escalationpolicies/${id}`);
    return response.data;
  },

  create: async (dto: CreateEscalationPolicyDto): Promise<EscalationPolicyDto> => {
    const response = await apiClient.post<EscalationPolicyDto>('/escalationpolicies', dto);
    return response.data;
  },

  update: async (id: number, dto: Partial<CreateEscalationPolicyDto>): Promise<EscalationPolicyDto> => {
    const response = await apiClient.put<EscalationPolicyDto>(`/escalationpolicies/${id}`, dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/escalationpolicies/${id}`);
  },
};

export default escalationPolicyService;
