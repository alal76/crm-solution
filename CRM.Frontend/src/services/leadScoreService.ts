// CRM Solution - Customer Relationship Management System
// FEAT-AISCORING: AI Lead Scoring Real-time Triggers — Frontend service
import apiClient from './apiClient';

export interface LeadScoreHistoryItem {
  id: number;
  leadId: number;
  score: number;
  previousScore: number;
  delta: number;
  reason: string;
  scoreComponents?: Record<string, number>;
  scoredAt: string;
  scoredBy: string;
}

export interface ScoreComponents {
  fit?: number;
  engagement?: number;
  budget?: number;
  authority?: number;
  need?: number;
  timeline?: number;
  metrics?: number;
  economicBuyer?: number;
  decisionCriteria?: number;
  decisionProcess?: number;
  identifyPain?: number;
  champion?: number;
}

export interface LeadScoreExplanation {
  leadId: number;
  currentScore: number;
  components: ScoreComponents;
  qualificationFramework: string;
  recentHistory: LeadScoreHistoryItem[];
  trend: 'improving' | 'declining' | 'stable';
}

export const getScoreHistory = async (
  leadId: number,
  limit = 20,
): Promise<LeadScoreHistoryItem[]> => {
  const response = await apiClient.get<{ history: LeadScoreHistoryItem[] }>(
    `/leads/${leadId}/score-history`,
    { params: { limit } },
  );
  return response.data.history ?? [];
};

export const getScoreExplanation = async (
  leadId: number,
): Promise<LeadScoreExplanation> => {
  const response = await apiClient.get<LeadScoreExplanation>(
    `/leads/${leadId}/score-explanation`,
  );
  return response.data;
};
