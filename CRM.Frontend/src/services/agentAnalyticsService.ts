import apiClient from './apiClient';
import { AgentUsageMetric, AgentAccuracyMetric, AgentCostMetric } from '../types/agents';
import { AIAnalyticsSummary } from './workflow/aiTypes';

const agentAnalyticsService = {
  getUsage: (days: number = 30) =>
    apiClient.get<AgentUsageMetric[]>('/agents/analytics/usage', { params: { days } }),
  getAccuracy: (days: number = 30) =>
    apiClient.get<AgentAccuracyMetric[]>('/agents/analytics/accuracy', { params: { days } }),
  getCost: (days: number = 30) =>
    apiClient.get<AgentCostMetric[]>('/agents/analytics/cost', { params: { days } }),
  /**
   * Fetches the unified AI analytics summary used by the AI Analytics Dashboard.
   * Returns totals, cost/token breakdowns by model and agent type, and recent executions.
   */
  getSummary: (days: number = 30) =>
    apiClient.get<AIAnalyticsSummary>('/agents/analytics/summary', { params: { days } }),
};

export default agentAnalyticsService;
