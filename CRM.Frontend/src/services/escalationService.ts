import apiClient from './apiClient';

export interface EscalationRuleDto {
  id: number;
  name: string;
  description?: string;
  priority: string;
  conditionType: string;
  conditionValue: string;
  targetType: string;
  targetId?: number;
  targetName?: string;
  escalationDelayMinutes: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEscalationRuleDto {
  name: string;
  description?: string;
  priority: string;
  conditionType: string;
  conditionValue: string;
  targetType: string;
  targetId?: number;
  targetName?: string;
  escalationDelayMinutes: number;
  isActive?: boolean;
}

export interface UpdateEscalationRuleDto extends Partial<CreateEscalationRuleDto> {}

export interface EscalationRuleTestResultDto {
  ruleId: number;
  serviceRequestId: number;
  wouldApply: boolean;
  reason: string;
  matchedConditions: string[];
}

const escalationService = {
  getAll: async (): Promise<EscalationRuleDto[]> => {
    const response = await apiClient.get<EscalationRuleDto[]>('/escalationrules');
    return response.data;
  },

  getById: async (id: number): Promise<EscalationRuleDto> => {
    const response = await apiClient.get<EscalationRuleDto>(`/escalationrules/${id}`);
    return response.data;
  },

  create: async (dto: CreateEscalationRuleDto): Promise<EscalationRuleDto> => {
    const response = await apiClient.post<EscalationRuleDto>('/escalationrules', dto);
    return response.data;
  },

  update: async (id: number, dto: UpdateEscalationRuleDto): Promise<EscalationRuleDto> => {
    const response = await apiClient.put<EscalationRuleDto>(`/escalationrules/${id}`, dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/escalationrules/${id}`);
  },

  testRule: async (ruleId: number, serviceRequestId: number): Promise<EscalationRuleTestResultDto> => {
    const response = await apiClient.post<EscalationRuleTestResultDto>(`/escalationrules/${ruleId}/test/${serviceRequestId}`);
    return response.data;
  },

  getApplicable: async (priority: string): Promise<EscalationRuleDto[]> => {
    const response = await apiClient.get<EscalationRuleDto[]>(`/escalationrules/applicable?priority=${priority}`);
    return response.data;
  },
};

export default escalationService;
