import apiClient from './apiClient';
import { Agent, UpdateAgentRequest } from '../types/agents';

const agentAdminService = {
  getConfigs: () => apiClient.get<Agent[]>('/agents/admin'),
  updateConfig: (agentId: number, request: UpdateAgentRequest) =>
    apiClient.put(`/agents/admin/${agentId}`, request),
  toggleAgent: (agentId: number) =>
    apiClient.post(`/agents/admin/${agentId}/toggle`),
};

export default agentAdminService;
