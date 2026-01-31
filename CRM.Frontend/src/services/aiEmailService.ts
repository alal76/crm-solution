/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * AI Email Service - Frontend API service for AI email intelligence
 */

import apiClient from './apiClient';

// Request Types
export interface EmailAnalysisRequest {
  emailContent: string;
  subject?: string;
  senderEmail?: string;
  customerId?: number;
}

export interface ResponseSuggestionRequest {
  emailContent: string;
  subject?: string;
  tone?: 'formal' | 'friendly' | 'casual' | 'apologetic' | 'enthusiastic';
  numSuggestions?: number;
  customerId?: number;
}

export interface SubjectOptimizationRequest {
  subject?: string;
  emailBody?: string;
  purpose?: 'sales' | 'followup' | 'support' | 'marketing' | 'internal';
}

export interface EmailImproveRequest {
  emailContent: string;
  subject?: string;
  improvementAreas?: ('clarity' | 'grammar' | 'tone' | 'professionalism' | 'brevity')[];
}

// Response Types
export interface SentimentInfo {
  label: 'positive' | 'negative' | 'neutral' | 'mixed';
  confidence: number;
  explanation?: string;
}

export interface EntityInfo {
  dates?: string[];
  amounts?: string[];
  names?: string[];
  action_items?: string[];
}

export interface EmailAnalysis {
  sentiment?: SentimentInfo;
  urgency?: 'low' | 'medium' | 'high' | 'critical';
  classification?: 'inquiry' | 'complaint' | 'follow-up' | 'thank_you' | 'request' | 'information' | 'urgent_action';
  entities?: EntityInfo;
  suggested_actions?: string[];
  topics?: string[];
  summary?: string;
}

export interface EmailAnalysisResponse {
  success: boolean;
  analysis?: EmailAnalysis;
  rawAnalysis?: string;
  error?: string;
  provider?: string;
  tokensUsed?: number;
}

export interface EmailSuggestion {
  subject: string;
  body: string;
  tone?: string;
  intent?: string;
}

export interface ResponseSuggestionResponse {
  success: boolean;
  suggestions?: EmailSuggestion[];
  quickReplies?: string[];
  rawContent?: string;
  error?: string;
  provider?: string;
}

export interface SubjectSuggestion {
  subject: string;
  score: number;
  reason?: string;
}

export interface SubjectOptimizationResponse {
  success: boolean;
  originalScore?: number;
  suggestions?: SubjectSuggestion[];
  tips?: string[];
  rawContent?: string;
  error?: string;
  provider?: string;
}

export interface ScoreSet {
  clarity: number;
  tone: number;
  grammar: number;
  overall: number;
}

export interface EmailScores {
  original?: ScoreSet;
  improved?: ScoreSet;
}

export interface ImprovedEmail {
  subject?: string;
  body: string;
}

export interface EmailChange {
  original: string;
  improved: string;
  reason?: string;
}

export interface EmailImproveResponse {
  success: boolean;
  improvedEmail?: ImprovedEmail;
  changes?: EmailChange[];
  scores?: EmailScores;
  summary?: string;
  rawContent?: string;
  error?: string;
  provider?: string;
}

// API Functions
export const aiEmailService = {
  /**
   * Analyze an email for sentiment, entities, and suggested actions
   */
  analyze: async (request: EmailAnalysisRequest): Promise<EmailAnalysisResponse> => {
    const response = await apiClient.post<EmailAnalysisResponse>('/ai/email/analyze', request);
    return response.data;
  },

  /**
   * Generate response suggestions for an email
   */
  suggestResponse: async (request: ResponseSuggestionRequest): Promise<ResponseSuggestionResponse> => {
    const response = await apiClient.post<ResponseSuggestionResponse>('/ai/email/suggest-response', request);
    return response.data;
  },

  /**
   * Optimize email subject line for better engagement
   */
  optimizeSubject: async (request: SubjectOptimizationRequest): Promise<SubjectOptimizationResponse> => {
    const response = await apiClient.post<SubjectOptimizationResponse>('/ai/email/optimize-subject', request);
    return response.data;
  },

  /**
   * Improve email writing - grammar, tone, clarity
   */
  improve: async (request: EmailImproveRequest): Promise<EmailImproveResponse> => {
    const response = await apiClient.post<EmailImproveResponse>('/ai/email/improve', request);
    return response.data;
  },
};

export default aiEmailService;
