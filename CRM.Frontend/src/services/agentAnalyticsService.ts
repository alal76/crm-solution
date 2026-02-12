import apiClient from './apiClient';
import { AgentUsageMetric, AgentAccuracyMetric, AgentCostMetric } from '../types/agents';

const agentAnalyticsService = {
  getUsage: (days: number = 30) =>
    apiClient.get<AgentUsageMetric[]>('/agents/analytics/usage', { params: { days } }),
  getAccuracy: (days: number = 30) =>
    apiClient.get<AgentAccuracyMetric[]>('/agents/analytics/accuracy', { params: { days } }),
  getCost: (days: number = 30) =>
    apiClient.get<AgentCostMetric[]>('/agents/analytics/cost', { params: { days } }),
};

export default agentAnalyticsService;
