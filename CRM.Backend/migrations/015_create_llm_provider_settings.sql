-- Migration: 015_create_llm_provider_settings.sql
-- Creates table for storing LLM provider configuration in database
-- API keys are NOT stored here - they remain in environment variables
-- This table stores user-configurable settings like default provider, fallback order, etc.

-- PostgreSQL version
CREATE TABLE IF NOT EXISTS llm_provider_settings (
    id SERIAL PRIMARY KEY,
    setting_key VARCHAR(100) NOT NULL UNIQUE,
    setting_value TEXT NOT NULL,
    value_type VARCHAR(50) NOT NULL DEFAULT 'string',
    category VARCHAR(100) NOT NULL DEFAULT 'general',
    description TEXT,
    is_encrypted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_llm_provider_settings_key ON llm_provider_settings(setting_key);
CREATE INDEX IF NOT EXISTS idx_llm_provider_settings_category ON llm_provider_settings(category);

-- Insert default settings (these will override appsettings.json values when present)
INSERT INTO llm_provider_settings (setting_key, setting_value, value_type, category, description) VALUES
    ('DefaultProvider', 'openai', 'string', 'general', 'Default LLM provider to use'),
    ('EnableFallback', 'true', 'boolean', 'general', 'Whether to fallback to other providers on failure'),
    ('FallbackOrder', '["openai","azure","anthropic","google","deepseek","local"]', 'json', 'general', 'Order of providers for fallback'),
    ('DefaultMaxTokens', '1000', 'integer', 'general', 'Default maximum tokens for completions'),
    ('DefaultTemperature', '0.7', 'decimal', 'general', 'Default temperature for completions'),
    ('TimeoutSeconds', '60', 'integer', 'general', 'Timeout in seconds for LLM requests'),
    ('MaxRetries', '3', 'integer', 'general', 'Maximum retry attempts on failure'),
    
    -- Provider-specific settings (models, not API keys)
    ('OpenAI.DefaultModel', 'gpt-4', 'string', 'provider.openai', 'Default model for OpenAI'),
    ('OpenAI.BaseUrl', 'https://api.openai.com/v1', 'string', 'provider.openai', 'Base URL for OpenAI API'),
    
    ('Azure.DefaultModel', 'gpt-4', 'string', 'provider.azure', 'Default model for Azure OpenAI'),
    ('Azure.ApiVersion', '2024-02-01', 'string', 'provider.azure', 'API version for Azure OpenAI'),
    
    ('Anthropic.DefaultModel', 'claude-3-sonnet-20240229', 'string', 'provider.anthropic', 'Default model for Anthropic'),
    ('Anthropic.BaseUrl', 'https://api.anthropic.com/v1', 'string', 'provider.anthropic', 'Base URL for Anthropic API'),
    ('Anthropic.ApiVersion', '2023-06-01', 'string', 'provider.anthropic', 'API version for Anthropic'),
    
    ('Google.DefaultModel', 'gemini-1.5-pro', 'string', 'provider.google', 'Default model for Google Cloud'),
    ('Google.Location', 'us-central1', 'string', 'provider.google', 'Google Cloud location'),
    ('Google.UseVertexAI', 'false', 'boolean', 'provider.google', 'Use Vertex AI instead of Gemini API'),
    
    ('Bedrock.DefaultModel', 'anthropic.claude-3-sonnet-20240229-v1:0', 'string', 'provider.bedrock', 'Default model for AWS Bedrock'),
    ('Bedrock.Region', 'us-east-1', 'string', 'provider.bedrock', 'AWS region for Bedrock'),
    ('Bedrock.UseDefaultCredentials', 'true', 'boolean', 'provider.bedrock', 'Use default AWS credential chain'),
    
    ('DeepSeek.DefaultModel', 'deepseek-chat', 'string', 'provider.deepseek', 'Default model for DeepSeek'),
    ('DeepSeek.BaseUrl', 'https://api.deepseek.com', 'string', 'provider.deepseek', 'Base URL for DeepSeek API'),
    
    ('Local.DefaultModel', 'llama3', 'string', 'provider.local', 'Default model for local LLM'),
    ('Local.BaseUrl', 'http://localhost:11434', 'string', 'provider.local', 'Base URL for local LLM server'),
    ('Local.ApiFormat', 'ollama', 'string', 'provider.local', 'API format: ollama, openai, or custom'),
    ('Local.Enabled', 'false', 'boolean', 'provider.local', 'Whether local LLM is enabled')
ON CONFLICT (setting_key) DO NOTHING;
