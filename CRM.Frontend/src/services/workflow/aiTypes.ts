/**
 * CRM Solution - AI Workflow Types
 * 
 * This file contains all AI-enhanced workflow node configuration types.
 * Extracted from workflowService.ts for better modularity.
 */

// ============================================================================
// AI Tool Definitions
// ============================================================================

/**
 * AI Tool definition for AI Agent nodes
 */
export interface AIToolDefinition {
  id: string;
  name: string;
  description: string;
  category: 'crm' | 'communication' | 'data' | 'external' | 'custom';
  icon: string;
  parameters: AIToolParameter[];
  requiresApproval?: boolean;
  costPerCall?: number;
}

export interface AIToolParameter {
  name: string;
  type: 'string' | 'number' | 'boolean' | 'object' | 'array';
  description: string;
  required: boolean;
  default?: unknown;
}

/**
 * Prompt template for reuse across AI nodes
 */
export interface PromptTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  template: string;
  variables: string[];
  exampleOutput?: string;
  tags: string[];
}

// ============================================================================
// AI Decision Node Configuration
// ============================================================================

/**
 * AI Decision Node Configuration
 * Routes workflow based on AI analysis of content
 */
export interface AIDecisionConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Decision prompt
  systemPrompt: string;
  userPromptTemplate: string;
  
  // Decision options
  decisionOptions: AIDecisionOption[];
  defaultOption?: string;
  confidenceThreshold: number;
  
  // Input/Output
  inputVariables: string[];
  outputVariable: string;
  
  // Error handling
  fallbackOption?: string;
  retryOnError: boolean;
}

export interface AIDecisionOption {
  id: string;
  label: string;
  description: string;
  criteria: string;
  outputHandle?: string;
}

// ============================================================================
// AI Agent Node Configuration
// ============================================================================

/**
 * AI Agent Node Configuration
 * Autonomous agent that can use tools
 */
export interface AIAgentConfig {
  // Agent identity
  agentName: string;
  agentDescription: string;
  systemPrompt: string;
  
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  maxIterations: number;
  
  // Tool access
  availableTools: string[];
  toolApprovalRequired: boolean;
  
  // Autonomy settings
  autonomyLevel: 'low' | 'medium' | 'high' | 'full';
  canModifyData: boolean;
  canSendCommunications: boolean;
  
  // Memory settings
  enableMemory: boolean;
  memoryType: 'conversation' | 'summary' | 'vector';
  maxMemoryItems: number;
  
  // Goals and constraints
  primaryGoal: string;
  constraints: string[];
  stopConditions: string[];
  
  // Budget controls
  maxCost?: number;
  maxApiCalls?: number;
  
  // Output
  outputVariable: string;
  outputSchema?: object;
}

// ============================================================================
// AI Content Generator Node Configuration
// ============================================================================

/**
 * AI Content Generator Node Configuration
 * Generates emails, summaries, reports, etc.
 */
export interface AIContentGeneratorConfig {
  // Content type
  contentType: 'email' | 'summary' | 'report' | 'response' | 'document' | 'custom';
  
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Prompt configuration
  systemPrompt: string;
  userPromptTemplate: string;
  
  // Content settings
  tone: 'professional' | 'friendly' | 'formal' | 'casual' | 'empathetic';
  language: string;
  maxLength?: number;
  
  // Template settings
  useTemplate: boolean;
  templateId?: string;
  
  // Input variables
  inputVariables: string[];
  contextFields: string[];
  
  // Output
  outputVariable: string;
  outputFormat: 'text' | 'html' | 'markdown' | 'json';
  
  // Review settings
  requiresReview: boolean;
  reviewerRole?: string;
}

// ============================================================================
// AI Data Extractor Node Configuration
// ============================================================================

/**
 * AI Data Extractor Node Configuration
 * Extracts structured data from unstructured text
 */
export interface AIDataExtractorConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Extraction settings
  inputVariable: string;
  extractionSchema: AIExtractionField[];
  
  // Prompt customization
  systemPrompt?: string;
  additionalInstructions?: string;
  
  // Output
  outputVariable: string;
  outputFormat: 'object' | 'array';
  
  // Validation
  validateOutput: boolean;
  validationRules?: AIValidationRule[];
  
  // Error handling
  onExtractionFailure: 'error' | 'skip' | 'default';
  defaultValues?: Record<string, unknown>;
}

export interface AIExtractionField {
  name: string;
  type: 'string' | 'number' | 'boolean' | 'date' | 'email' | 'phone' | 'address' | 'array' | 'object';
  description: string;
  required: boolean;
  format?: string;
  examples?: string[];
  arrayItemType?: string;
  objectSchema?: AIExtractionField[];
}

export interface AIValidationRule {
  field: string;
  rule: 'required' | 'minLength' | 'maxLength' | 'pattern' | 'min' | 'max' | 'enum';
  value: unknown;
  errorMessage: string;
}

// ============================================================================
// AI Classifier Node Configuration
// ============================================================================

/**
 * AI Classifier Node Configuration
 * Categorizes and tags content
 */
export interface AIClassifierConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Classification type
  classificationType: 'single' | 'multi';
  
  // Categories
  categories: AIClassifierCategory[];
  allowCustomCategories: boolean;
  maxCategories?: number;
  
  // Input
  inputVariable: string;
  contextVariables: string[];
  
  // Prompt customization
  systemPrompt?: string;
  classificationCriteria?: string;
  
  // Output
  outputVariable: string;
  includeConfidence: boolean;
  includeReasoning: boolean;
  confidenceThreshold: number;
}

export interface AIClassifierCategory {
  id: string;
  name: string;
  description: string;
  keywords?: string[];
  examples?: string[];
  color?: string;
}

// ============================================================================
// AI Sentiment Analyzer Node Configuration
// ============================================================================

/**
 * AI Sentiment Analyzer Node Configuration
 * Analyzes sentiment and emotion
 */
export interface AISentimentAnalyzerConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Analysis type
  analysisType: 'basic' | 'detailed' | 'emotional';
  
  // Input
  inputVariable: string;
  contextVariables: string[];
  
  // Output settings
  outputVariable: string;
  includeScore: boolean;
  includeEmotions: boolean;
  includeKeyPhrases: boolean;
  includeSuggestions: boolean;
  
  // Thresholds for routing
  sentimentThresholds?: {
    positive: number;
    neutral: number;
    negative: number;
  };
  
  // Custom prompts
  systemPrompt?: string;
  analysisInstructions?: string;
}

export interface AISentimentResult {
  sentiment: 'positive' | 'neutral' | 'negative' | 'mixed';
  score: number;
  confidence: number;
  emotions?: {
    joy: number;
    anger: number;
    sadness: number;
    fear: number;
    surprise: number;
  };
  keyPhrases?: string[];
  suggestions?: string[];
}

// ============================================================================
// Human Review Node Configuration
// ============================================================================

/**
 * Human Review Node Configuration
 * Human-in-the-loop review for AI outputs
 */
export interface HumanReviewConfig {
  // Task settings
  taskTitle: string;
  taskDescription: string;
  
  // Assignment
  assignToRole?: string;
  assignToUser?: string;
  escalationRules?: AIEscalationRule[];
  
  // Review content
  reviewVariable: string;
  contextVariables: string[];
  showOriginalInput: boolean;
  
  // Review options
  reviewOptions: HumanReviewOption[];
  allowEdit: boolean;
  requireComments: boolean;
  
  // SLA
  dueInMinutes: number;
  escalateAfterMinutes?: number;
  
  // Output
  outputVariable: string;
  captureReviewerFeedback: boolean;
}

export interface HumanReviewOption {
  id: string;
  label: string;
  description: string;
  action: 'approve' | 'reject' | 'modify' | 'escalate';
  requiresComment: boolean;
  nextHandle?: string;
}

export interface AIEscalationRule {
  condition: 'timeout' | 'rejection' | 'uncertainty';
  escalateTo: string;
  notifyOriginal: boolean;
}

// ============================================================================
// AI Analytics Types
// ============================================================================

/**
 * AI Analytics types for cost and performance tracking
 */
export interface AINodeExecution {
  nodeId: number;
  nodeType: string;
  model: string;
  inputTokens: number;
  outputTokens: number;
  totalTokens: number;
  cost: number;
  latencyMs: number;
  success: boolean;
  errorMessage?: string;
  timestamp: string;
}

export interface AIAnalyticsSummary {
  period: string;
  totalCost: number;
  totalTokens: number;
  totalExecutions: number;
  successRate: number;
  averageLatencyMs: number;
  byModel: {
    model: string;
    cost: number;
    tokens: number;
    executions: number;
  }[];
  byNodeType: {
    nodeType: string;
    cost: number;
    tokens: number;
    executions: number;
  }[];
}
