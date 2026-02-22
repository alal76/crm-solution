// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// Stores LLM provider configuration settings in the database.
/// Allows administrators to configure AI/LLM settings from the UI.
/// API keys and secrets are NOT stored here - they come from environment variables.
///
/// TECHNICAL VIEW:
/// ===============
/// Key-value store for LLM settings that can be modified at runtime.
/// Settings are grouped by category (general, provider.openai, provider.anthropic, etc.)
/// Values support multiple types: string, integer, decimal, boolean, json
/// </summary>
[Table("llm_provider_settings")]
public class LLMProviderSetting : BaseEntity
{
    /// <summary>
    /// Unique key for the setting (e.g., "DefaultProvider", "OpenAI.DefaultModel")
    /// </summary>
    public string SettingKey { get; set; } = "";

    /// <summary>
    /// Value stored as string, parsed based on ValueType
    /// </summary>
    public string SettingValue { get; set; } = "";

    /// <summary>
    /// Type hint for parsing: string, integer, decimal, boolean, json
    /// </summary>
    public string ValueType { get; set; } = "string";

    /// <summary>
    /// Category for grouping: general, provider.openai, provider.anthropic, etc.
    /// </summary>
    public string Category { get; set; } = "general";

    /// <summary>
    /// Human-readable description of the setting
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this value is encrypted (for future use with non-secret sensitive data)
    /// </summary>
    public bool IsEncrypted { get; set; } = false;
}

/// <summary>
/// DTO for LLM settings API responses
/// </summary>
public class LLMSettingsDto
{
    public string DefaultProvider { get; set; } = "local";
    public bool EnableFallback { get; set; } = true;
    public List<string> FallbackOrder { get; set; } = new() { "local", "openai", "azure", "anthropic", "google", "deepseek", "groq", "allenai" };

    /// <summary>
    /// Computed fallback order containing only configured providers.
    /// If no providers are configured with API keys, only 'local' will be included.
    /// </summary>
    public List<string> EffectiveFallbackOrder { get; set; } = new();

    public int DefaultMaxTokens { get; set; } = 1000;
    public double DefaultTemperature { get; set; } = 0.7;
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;

    public LLMProviderSettingsDto OpenAI { get; set; } = new();
    public LLMProviderSettingsDto Azure { get; set; } = new();
    public LLMProviderSettingsDto Anthropic { get; set; } = new();
    public LLMProviderSettingsDto Google { get; set; } = new();
    public LLMProviderSettingsDto Bedrock { get; set; } = new();
    public LLMProviderSettingsDto DeepSeek { get; set; } = new();
    public LLMProviderSettingsDto Groq { get; set; } = new();
    public LLMProviderSettingsDto AllenAI { get; set; } = new();
    public LLMProviderSettingsDto Local { get; set; } = new();
    public LLMProviderSettingsDto Custom { get; set; } = new();
}

/// <summary>
/// DTO for individual provider settings
/// </summary>
public class LLMProviderSettingsDto
{
    public string DefaultModel { get; set; } = "";
    public string? BaseUrl { get; set; }
    public string? ApiVersion { get; set; }
    public string? Location { get; set; }
    public string? Region { get; set; }
    public string? ApiFormat { get; set; }
    public bool? Enabled { get; set; }
    public bool? UseVertexAI { get; set; }
    public bool? UseDefaultCredentials { get; set; }
    public bool IsConfigured { get; set; }

    /// <summary>
    /// Masked API key for display (e.g. "sk-****abcd"). Never contains the full key.
    /// </summary>
    public string? ApiKeyMasked { get; set; }

    /// <summary>
    /// Whether an API key is stored in the database for this provider.
    /// </summary>
    public bool HasApiKey { get; set; }

    /// <summary>
    /// For Azure: the endpoint URL (e.g. https://myresource.openai.azure.com)
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// For Azure: the deployment name
    /// </summary>
    public string? DeploymentName { get; set; }

    /// <summary>
    /// For Google: the project ID
    /// </summary>
    public string? ProjectId { get; set; }
}

/// <summary>
/// Request DTO for updating LLM settings
/// </summary>
public class UpdateLLMSettingsRequest
{
    public string? DefaultProvider { get; set; }
    public bool? EnableFallback { get; set; }
    public List<string>? FallbackOrder { get; set; }
    public int? DefaultMaxTokens { get; set; }
    public double? DefaultTemperature { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? MaxRetries { get; set; }

    public Dictionary<string, LLMProviderUpdateDto>? Providers { get; set; }
}

/// <summary>
/// DTO for updating individual provider settings
/// </summary>
public class LLMProviderUpdateDto
{
    public string? DefaultModel { get; set; }
    public string? BaseUrl { get; set; }
    public string? ApiVersion { get; set; }
    public string? Location { get; set; }
    public string? Region { get; set; }
    public string? ApiFormat { get; set; }
    public bool? Enabled { get; set; }
    public bool? UseVertexAI { get; set; }
    public bool? UseDefaultCredentials { get; set; }

    /// <summary>
    /// The new API key to store (will be encrypted at rest).
    /// Send empty string to clear the key. Omit (null) to leave unchanged.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// For Azure: the endpoint URL
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// For Azure: the deployment name
    /// </summary>
    public string? DeploymentName { get; set; }

    /// <summary>
    /// For Google: the project ID
    /// </summary>
    public string? ProjectId { get; set; }
}
