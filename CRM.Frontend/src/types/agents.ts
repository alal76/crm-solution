// ===== ENUMS =====

export enum AgentType {
  LeadScoring = 0,
  SupportTriage = 1,
  NextBestAction = 2,
  SalesIntelligence = 3,
  EmailAssistant = 4,
  CustomerSuccess = 5,
  RevenueIntelligence = 6,
  TicketResolution = 7,
  DocumentIntelligence = 8,
  SalesCoach = 9,
  MeetingIntelligence = 10,
  ConversationIntelligence = 11,
  Orchestrator = 12,
  GeneralAssistant = 13,
  SalesAssistant = 14,
  DealIntelligence = 15,
  ForecastAnalyst = 16,
  DataAnalyst = 17,
  OnboardingGuide = 18,
  ContractAnalyst = 19,
  KnowledgeExpert = 20,
}

export enum ConversationStatus {
  Active = 0,
  Completed = 1,
  Cancelled = 2,
  Failed = 3,
  WaitingForApproval = 4,
}

export enum ActionStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Executed = 3,
  Failed = 4,
  Cancelled = 5,
}

export enum ActionType {
  Read = 0,
  Write = 1,
  Search = 2,
  Analyze = 3,
  Notify = 4,
  Generate = 5,
}

export enum ApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Expired = 3,
  AutoApproved = 4,
}

export enum MemoryType {
  ShortTerm = 0,
  LongTerm = 1,
  Episodic = 2,
  Semantic = 3,
  Procedural = 4,
  UserPreference = 5,
}

// ===== ENTITIES =====

export interface Agent {
  id: number;
  name: string;
  displayName?: string;
  description?: string;
  systemPrompt: string;
  agentType: AgentType;
  allowedPlugins?: string;
  configuration?: string;
  isActive: boolean;
  requiresApproval: boolean;
  approvalTier?: string;
  temperature: number;
  maxTokens: number;
  modelOverride?: string;
  totalConversations: number;
  totalActions: number;
  averageRating?: number;
  createdAt: string;
  updatedAt?: string;
}

export interface AgentConversation {
  id: number;
  agentId: number;
  userId: number;
  entityType?: string;
  entityId?: number;
  status: ConversationStatus;
  messages: string;
  messageCount: number;
  totalTokensUsed: number;
  estimatedCost: number;
  userRating?: number;
  userFeedback?: string;
  completedAt?: string;
  createdAt: string;
}

export interface ChatMessageRecord {
  role: 'user' | 'assistant' | 'system' | 'tool';
  content: string;
}

export interface AgentAction {
  id: number;
  conversationId: number;
  agentId: number;
  actionType: ActionType;
  pluginName: string;
  functionName: string;
  inputParameters?: string;
  outputResult?: string;
  status: ActionStatus;
  approvalRequestId?: number;
  tokensUsed: number;
  executionTimeMs: number;
  errorMessage?: string;
  createdAt: string;
}

export interface AgentApproval {
  id: number;
  agentActionId: number;
  conversationId: number;
  agentId: number;
  requestedByUserId: number;
  approvedByUserId?: number;
  actionDescription: string;
  pluginName: string;
  functionName: string;
  parameters?: string;
  status: ApprovalStatus;
  approvalTier: string;
  rejectionReason?: string;
  decidedAt?: string;
  expiresAt?: string;
  createdAt: string;
}

export interface AgentMemory {
  id: number;
  agentId: number;
  memoryType: MemoryType;
  key: string;
  value: string;
  entityType?: string;
  entityId?: number;
  confidence: number;
  accessCount: number;
  lastAccessedAt?: string;
  expiresAt?: string;
  createdAt: string;
}

// ===== REQUEST/RESPONSE DTOs =====

export interface ChatRequest {
  message: string;
  conversationId?: number;
  entityType?: string;
  entityId?: number;
}

export interface ChatResponse {
  response: string;
  conversationId: number;
  history: ChatMessageRecord[];
}

export interface RateRequest {
  rating: number;
  feedback?: string;
}

export interface DraftEmailRequest {
  context: string;
  recipientEmail?: string;
  tone?: string;
  templateId?: number;
}

export interface OrchestrateRequest {
  message: string;
  agentTypes: string[];
  entityType?: string;
  entityId?: number;
}

export interface UpdateAgentRequest {
  systemPrompt?: string;
  temperature?: number;
  maxTokens?: number;
  allowedPlugins?: string;
  modelOverride?: string;
  requiresApproval?: boolean;
  approvalTier?: string;
}

export interface CreateAgentRequest {
  name: string;
  displayName: string;
  description?: string;
  agentType: number;
  systemPrompt?: string;
  allowedPlugins?: string;
  requiresApproval?: boolean;
  approvalTier?: string;
  temperature?: number;
  maxTokens?: number;
  modelOverride?: string;
}

// ===== ANALYTICS DTOs =====

export interface AgentUsageMetric {
  agentId: number;
  agentName: string;
  totalConversations: number;
  totalActions: number;
  averageMessagesPerConversation: number;
}

export interface AgentAccuracyMetric {
  agentId: number;
  agentName: string;
  averageRating: number;
  ratedConversations: number;
  totalConversations: number;
}

export interface AgentCostMetric {
  agentId: number;
  agentName: string;
  totalActions: number;
  dailyCosts: DailyCost[];
}

export interface DailyCost {
  date: string;
  actionCount: number;
}

// ===== HELPER: Labels =====

export const AgentTypeLabels: Record<AgentType, string> = {
  [AgentType.LeadScoring]: 'Lead Scoring',
  [AgentType.SupportTriage]: 'Support Triage',
  [AgentType.NextBestAction]: 'Next Best Action',
  [AgentType.SalesIntelligence]: 'Sales Intelligence',
  [AgentType.EmailAssistant]: 'Email Assistant',
  [AgentType.CustomerSuccess]: 'Customer Success',
  [AgentType.RevenueIntelligence]: 'Revenue Intelligence',
  [AgentType.TicketResolution]: 'Ticket Resolution',
  [AgentType.DocumentIntelligence]: 'Document Intelligence',
  [AgentType.SalesCoach]: 'Sales Coach',
  [AgentType.MeetingIntelligence]: 'Meeting Intelligence',
  [AgentType.ConversationIntelligence]: 'Conversation Intelligence',
  [AgentType.Orchestrator]: 'Orchestrator',
  [AgentType.GeneralAssistant]: 'General Assistant',
  [AgentType.SalesAssistant]: 'Sales Assistant',
  [AgentType.DealIntelligence]: 'Deal Intelligence',
  [AgentType.ForecastAnalyst]: 'Forecast Analyst',
  [AgentType.DataAnalyst]: 'Data Analyst',
  [AgentType.OnboardingGuide]: 'Onboarding Guide',
  [AgentType.ContractAnalyst]: 'Contract Analyst',
  [AgentType.KnowledgeExpert]: 'Knowledge Expert',
};

// ===== HELPER: Colors =====

/**
 * Returns an MUI-compatible color string for an agent type, grouped by category:
 * - Sales agents → blue shades
 * - Support agents → green shades
 * - Analytics agents → orange shades
 * - General/utility agents → purple shades
 */
export const getAgentTypeColor = (agentType: AgentType): string => {
  switch (agentType) {
    // Sales — blue
    case AgentType.LeadScoring:
      return '#1565c0'; // blue 800
    case AgentType.SalesIntelligence:
      return '#1976d2'; // blue 700
    case AgentType.SalesAssistant:
      return '#1e88e5'; // blue 600
    case AgentType.SalesCoach:
      return '#2196f3'; // blue 500
    case AgentType.DealIntelligence:
      return '#42a5f5'; // blue 400
    case AgentType.RevenueIntelligence:
      return '#0d47a1'; // blue 900
    case AgentType.ForecastAnalyst:
      return '#1565c0'; // blue 800
    case AgentType.ContractAnalyst:
      return '#1e88e5'; // blue 600

    // Support — green
    case AgentType.SupportTriage:
      return '#2e7d32'; // green 800
    case AgentType.TicketResolution:
      return '#388e3c'; // green 700
    case AgentType.CustomerSuccess:
      return '#43a047'; // green 600
    case AgentType.KnowledgeExpert:
      return '#66bb6a'; // green 400

    // Analytics — orange
    case AgentType.DataAnalyst:
      return '#e65100'; // orange 900
    case AgentType.NextBestAction:
      return '#ef6c00'; // orange 800
    case AgentType.DocumentIntelligence:
      return '#f57c00'; // orange 700
    case AgentType.MeetingIntelligence:
      return '#fb8c00'; // orange 600
    case AgentType.ConversationIntelligence:
      return '#ff9800'; // orange 500

    // General / utility — purple
    case AgentType.GeneralAssistant:
      return '#6a1b9a'; // purple 800
    case AgentType.EmailAssistant:
      return '#7b1fa2'; // purple 700
    case AgentType.OnboardingGuide:
      return '#8e24aa'; // purple 600
    case AgentType.Orchestrator:
      return '#9c27b0'; // purple 500

    default:
      return '#757575'; // grey 600
  }
};
