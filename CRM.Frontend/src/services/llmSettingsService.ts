// CRM Solution - LLM Settings API Service
// Uses apiClient for automatic auth token handling

import apiClient from './apiClient';

// ─── Types ───────────────────────────────────────────────────────────

export interface LLMProviderSettingsDto {
  defaultModel: string;
  baseUrl?: string;
  apiVersion?: string;
  location?: string;
  region?: string;
  apiFormat?: string;
  enabled?: boolean;
  useVertexAI?: boolean;
  useDefaultCredentials?: boolean;
  isConfigured: boolean;
  apiKeyMasked?: string | null;
  hasApiKey: boolean;
  endpoint?: string | null;
  deploymentName?: string | null;
  projectId?: string | null;
}

export interface LLMSettingsDto {
  defaultProvider: string;
  enableFallback: boolean;
  fallbackOrder: string[];
  effectiveFallbackOrder: string[];
  defaultMaxTokens: number;
  defaultTemperature: number;
  timeoutSeconds: number;
  maxRetries: number;
  openAI: LLMProviderSettingsDto;
  azure: LLMProviderSettingsDto;
  anthropic: LLMProviderSettingsDto;
  google: LLMProviderSettingsDto;
  bedrock: LLMProviderSettingsDto;
  deepSeek: LLMProviderSettingsDto;
  groq: LLMProviderSettingsDto;
  allenAI: LLMProviderSettingsDto;
  local: LLMProviderSettingsDto;
  custom: LLMProviderSettingsDto;
}

export interface LLMProviderUpdateDto {
  defaultModel?: string;
  baseUrl?: string;
  apiVersion?: string;
  location?: string;
  region?: string;
  apiFormat?: string;
  enabled?: boolean;
  useVertexAI?: boolean;
  useDefaultCredentials?: boolean;
  apiKey?: string;       // plaintext - encrypted at rest by backend
  endpoint?: string;
  deploymentName?: string;
  projectId?: string;
}

export interface UpdateLLMSettingsRequest {
  defaultProvider?: string;
  enableFallback?: boolean;
  fallbackOrder?: string[];
  defaultMaxTokens?: number;
  defaultTemperature?: number;
  timeoutSeconds?: number;
  maxRetries?: number;
  providers?: Record<string, LLMProviderUpdateDto>;
}

export interface TestConnectionResult {
  success: boolean;
  message: string;
  provider: string;
}

export interface CircuitBreakerState {
  serviceName: string;
  state: string;
  lastStateChange?: string;
  failureCount: number;
  successCount: number;
  lastFailure?: string;
  lastError?: string;
}

// ─── Provider metadata ───────────────────────────────────────────────

export interface ProviderMeta {
  key: string;       // backend key (openAI, azure, etc.)
  apiKey: string;    // provider key for API calls (openai, azure, etc.)
  label: string;
  description: string;
  icon: string;
  fields: ProviderField[];
}

export interface ProviderField {
  key: string;
  label: string;
  type: 'text' | 'password' | 'url' | 'select' | 'switch' | 'number';
  placeholder?: string;
  helperText?: string;
  options?: { value: string; label: string }[];
  required?: boolean;
}

export const PROVIDER_DEFINITIONS: ProviderMeta[] = [
  {
    key: 'openAI', apiKey: 'openai', label: 'OpenAI', icon: '🤖',
    description: 'GPT-4o, GPT-4, GPT-3.5 Turbo models',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password', placeholder: 'sk-...', required: true },
      { key: 'baseUrl', label: 'Base URL', type: 'url', placeholder: 'https://api.openai.com/v1' },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'gpt-4o', label: 'GPT-4o' },
        { value: 'gpt-4o-mini', label: 'GPT-4o Mini' },
        { value: 'gpt-4-turbo', label: 'GPT-4 Turbo' },
        { value: 'gpt-4', label: 'GPT-4' },
        { value: 'gpt-3.5-turbo', label: 'GPT-3.5 Turbo' },
        { value: 'o1-preview', label: 'o1 Preview' },
        { value: 'o1-mini', label: 'o1 Mini' },
      ]},
    ],
  },
  {
    key: 'azure', apiKey: 'azure', label: 'Azure OpenAI', icon: '☁️',
    description: 'Microsoft Azure-hosted OpenAI models',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password', required: true },
      { key: 'endpoint', label: 'Endpoint', type: 'url', placeholder: 'https://your-resource.openai.azure.com' },
      { key: 'deploymentName', label: 'Deployment Name', type: 'text', placeholder: 'gpt-4o' },
      { key: 'baseUrl', label: 'API Version', type: 'text', placeholder: '2024-02-01', helperText: 'Azure API version' },
      { key: 'defaultModel', label: 'Default Model', type: 'text', placeholder: 'gpt-4' },
    ],
  },
  {
    key: 'anthropic', apiKey: 'anthropic', label: 'Anthropic (Claude)', icon: '🧠',
    description: 'Claude 3.5 Sonnet, Claude 3 Opus, Claude 3 Haiku',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password', required: true },
      { key: 'baseUrl', label: 'Base URL', type: 'url', placeholder: 'https://api.anthropic.com/v1' },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'claude-3-5-sonnet-20241022', label: 'Claude 3.5 Sonnet' },
        { value: 'claude-3-5-haiku-20241022', label: 'Claude 3.5 Haiku' },
        { value: 'claude-3-opus-20240229', label: 'Claude 3 Opus' },
      ]},
    ],
  },
  {
    key: 'google', apiKey: 'google', label: 'Google (Gemini)', icon: '💎',
    description: 'Gemini 2.0, Gemini 1.5 Pro/Flash models',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password', required: true },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'gemini-2.0-flash-exp', label: 'Gemini 2.0 Flash' },
        { value: 'gemini-1.5-pro', label: 'Gemini 1.5 Pro' },
        { value: 'gemini-1.5-flash', label: 'Gemini 1.5 Flash' },
      ]},
      { key: 'projectId', label: 'Project ID', type: 'text', placeholder: 'my-gcp-project', helperText: 'Required for Vertex AI' },
      { key: 'location', label: 'Location', type: 'text', placeholder: 'us-central1' },
    ],
  },
  {
    key: 'bedrock', apiKey: 'bedrock', label: 'AWS Bedrock', icon: '🪨',
    description: 'Claude, Titan, Llama models via AWS',
    fields: [
      { key: 'apiKey', label: 'Access Key ID', type: 'password' },
      { key: 'region', label: 'Region', type: 'text', placeholder: 'us-east-1' },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'anthropic.claude-3-5-sonnet-20241022-v2:0', label: 'Claude 3.5 Sonnet (Bedrock)' },
        { value: 'anthropic.claude-3-haiku-20240307-v1:0', label: 'Claude 3 Haiku (Bedrock)' },
        { value: 'amazon.titan-text-premier-v1:0', label: 'Titan Text Premier' },
        { value: 'meta.llama3-70b-instruct-v1:0', label: 'Llama 3 70B' },
      ]},
    ],
  },
  {
    key: 'deepSeek', apiKey: 'deepseek', label: 'DeepSeek', icon: '🔍',
    description: 'DeepSeek Chat, Coder, and Reasoner models',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password', required: true },
      { key: 'baseUrl', label: 'Base URL', type: 'url', placeholder: 'https://api.deepseek.com' },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'deepseek-chat', label: 'DeepSeek Chat' },
        { value: 'deepseek-coder', label: 'DeepSeek Coder' },
        { value: 'deepseek-reasoner', label: 'DeepSeek Reasoner' },
      ]},
    ],
  },
  {
    key: 'groq', apiKey: 'groq', label: 'Groq (Fast Inference)', icon: '⚡',
    description: 'Ultra-fast inference — Llama, Mixtral, Gemma via LPU',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password', required: true },
      { key: 'baseUrl', label: 'Base URL', type: 'url', placeholder: 'https://api.groq.com/openai/v1' },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'llama-3.3-70b-versatile', label: 'Llama 3.3 70B' },
        { value: 'llama-3.1-8b-instant', label: 'Llama 3.1 8B Instant' },
        { value: 'mixtral-8x7b-32768', label: 'Mixtral 8x7B' },
        { value: 'gemma2-9b-it', label: 'Gemma 2 9B' },
      ]},
    ],
  },
  {
    key: 'allenAI', apiKey: 'allenai', label: 'Allen AI (Open Research)', icon: '🔬',
    description: 'OLMo, Tulu open-source research models via HuggingFace',
    fields: [
      { key: 'apiKey', label: 'HuggingFace API Token', type: 'password' },
      { key: 'baseUrl', label: 'Base URL', type: 'url', placeholder: 'https://api-inference.huggingface.co/models' },
      { key: 'defaultModel', label: 'Default Model', type: 'select', options: [
        { value: 'allenai/OLMo-7B-Instruct', label: 'OLMo 7B Instruct' },
        { value: 'allenai/tulu-2-7b', label: 'Tulu 2 7B' },
        { value: 'allenai/OLMo-1B', label: 'OLMo 1B (Fast)' },
      ]},
      { key: 'enabled', label: 'Enabled', type: 'switch' },
    ],
  },
  {
    key: 'local', apiKey: 'local', label: 'Local LLM (Ollama)', icon: '🏠',
    description: 'Self-hosted models: Ollama, LM Studio, vLLM',
    fields: [
      { key: 'baseUrl', label: 'Base URL', type: 'url', placeholder: 'http://localhost:11434', required: true },
      { key: 'defaultModel', label: 'Default Model', type: 'text', placeholder: 'llama3' },
      { key: 'apiFormat', label: 'API Format', type: 'select', options: [
        { value: 'ollama', label: 'Ollama' },
        { value: 'openai', label: 'OpenAI-compatible' },
      ]},
      { key: 'enabled', label: 'Enabled', type: 'switch' },
    ],
  },
  {
    key: 'custom', apiKey: 'custom', label: 'Custom Endpoint', icon: '🔧',
    description: 'Any OpenAI-compatible API endpoint',
    fields: [
      { key: 'apiKey', label: 'API Key', type: 'password' },
      { key: 'baseUrl', label: 'Endpoint URL', type: 'url', placeholder: 'https://your-llm-api.com/v1', required: true },
      { key: 'defaultModel', label: 'Model', type: 'text', placeholder: 'custom-model' },
    ],
  },
];

// ─── API Functions ───────────────────────────────────────────────────

const llmSettingsService = {
  /** Fetch all LLM settings (merged DB + config) */
  getSettings: () =>
    apiClient.get<LLMSettingsDto>('/workflows/llm-settings'),

  /** Update LLM settings (partial updates, API keys encrypted on save) */
  updateSettings: (request: UpdateLLMSettingsRequest) =>
    apiClient.put<LLMSettingsDto>('/workflows/llm-settings', request),

  /** Reset all settings to defaults from appsettings.json */
  resetToDefaults: () =>
    apiClient.post<LLMSettingsDto>('/workflows/llm-settings/reset'),

  /** Initialize default settings in DB (first-time setup) */
  initialize: () =>
    apiClient.post<LLMSettingsDto>('/workflows/llm-settings/initialize'),

  /** Test connectivity to a provider */
  testConnection: (provider: string) =>
    apiClient.post<TestConnectionResult>(`/workflows/llm-settings/test/${provider}`),

  /** Get circuit breaker states */
  getCircuitBreakers: () =>
    apiClient.get<CircuitBreakerState[]>('/monitoring/resilience'),
};

export default llmSettingsService;
