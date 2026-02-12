import apiClient from './apiClient';
import {
  Agent,
  AgentConversation,
  AgentApproval,
  ChatRequest,
  ChatResponse,
  RateRequest,
  DraftEmailRequest,
  OrchestrateRequest,
} from '../types/agents';

const agentService = {
  // Agent discovery
  getAll: () => apiClient.get<Agent[]>('/agents'),
  getById: (id: number) => apiClient.get<Agent>(`/agents/${id}`),

  // Chat
  chat: (agentId: number, request: ChatRequest) =>
    apiClient.post<ChatResponse>(`/agents/${agentId}/chat`, request),

  // Conversations
  getConversations: (agentId: number, limit: number = 20) =>
    apiClient.get<AgentConversation[]>(`/agents/${agentId}/conversations`, { params: { limit } }),
  getConversation: (conversationId: number) =>
    apiClient.get<AgentConversation>(`/agents/conversations/${conversationId}`),
  rateConversation: (conversationId: number, request: RateRequest) =>
    apiClient.post(`/agents/conversations/${conversationId}/rate`, request),

  // Approvals
  getApprovals: () => apiClient.get<AgentApproval[]>('/agents/approvals'),
  approveAction: (approvalId: number) =>
    apiClient.post(`/agents/approvals/${approvalId}/approve`),
  rejectAction: (approvalId: number, reason: string) =>
    apiClient.post(`/agents/approvals/${approvalId}/reject`, { reason }),

  // Specialized operations
  draftEmail: (request: DraftEmailRequest) =>
    apiClient.post('/agents/email/draft', request),
  resolveTicket: (serviceRequestId: number) =>
    apiClient.post(`/agents/resolve/${serviceRequestId}`),
  orchestrate: (request: OrchestrateRequest) =>
    apiClient.post('/agents/orchestrate', request),
  getDealIntelligence: (opportunityId: number) =>
    apiClient.get(`/agents/deal-intelligence/${opportunityId}`),
  getNextBestActions: (entityType: string, entityId: number) =>
    apiClient.get(`/agents/next-best-actions/${entityType}/${entityId}`),
};

export default agentService;
