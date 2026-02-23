// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for email server configuration
/// </summary>
public class EmailServerConfigDto
{
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public bool UseTls { get; set; } = true;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }  // Sensitive
    public bool IsConfigured { get; set; }
    public DateTime? LastTested { get; set; }
    public string? ConnectionStatus { get; set; }  // 'connected', 'disconnected', 'error'
    public string? TestError { get; set; }
}

/// <summary>
/// DTO for two-factor authentication configuration
/// </summary>
public class TwoFactorConfigDto
{
    public string Provider { get; set; } = "disabled";  // 'email', 'sms', 'totp', 'disabled'
    public bool Required { get; set; }
    public string? SmsProvider { get; set; }  // 'twilio', 'nexmo'
    public string? TwilioAccountSid { get; set; }  // Sensitive
    public string? TwilioAuthToken { get; set; }   // Sensitive
    public string? TwilioFromNumber { get; set; }
    public string? Issuer { get; set; }  // For TOTP identification
}

/// <summary>
/// DTO for Google OAuth configuration
/// </summary>
public class GoogleOAuthDto
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }  // Sensitive
}

/// <summary>
/// DTO for Microsoft OAuth configuration
/// </summary>
public class MicrosoftOAuthDto
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }  // Sensitive
    public string? TenantId { get; set; }
}

/// <summary>
/// DTO for Azure AD configuration
/// </summary>
public class AzureAdDto
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }  // Sensitive
    public string? TenantId { get; set; }
    public string? Authority { get; set; }
}

/// <summary>
/// DTO for LinkedIn OAuth configuration
/// </summary>
public class LinkedInOAuthDto
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }  // Sensitive
}

/// <summary>
/// DTO for Facebook OAuth configuration
/// </summary>
public class FacebookOAuthDto
{
    public bool Enabled { get; set; }
    public string? AppId { get; set; }
    public string? AppSecret { get; set; }  // Sensitive
}

/// <summary>
/// DTO for social login configuration (all providers)
/// </summary>
public class SocialLoginConfigDto
{
    public GoogleOAuthDto? Google { get; set; }
    public MicrosoftOAuthDto? Microsoft { get; set; }
    public AzureAdDto? AzureAd { get; set; }
    public LinkedInOAuthDto? LinkedIn { get; set; }
    public FacebookOAuthDto? Facebook { get; set; }
}

/// <summary>
/// DTO for AI provider configuration
/// </summary>
public class AIProviderConfigDto
{
    public string Provider { get; set; } = null!;  // 'openai', 'azure', 'ollama', 'anthropic', 'bedrock', etc.
    public bool Enabled { get; set; }
    public string? ApiKey { get; set; }            // Sensitive
    public string? ApiUrl { get; set; }
    public string? OrganizationId { get; set; }    // OpenAI org ID
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public bool CostTrackingEnabled { get; set; }
    public DateTime? LastTested { get; set; }
    public string? ConnectionStatus { get; set; }
    public string? TestError { get; set; }
}

/// <summary>
/// DTO for integration configuration (search, chat, notifications, etc.)
/// </summary>
public class IntegrationConfigDto
{
    public string Type { get; set; } = null!;      // 'search', 'chat', 'notifications', 'analytics', 'signatures', 'workflows'
    public string Provider { get; set; } = null!;  // 'meilisearch', 'chatwoot', 'novu', etc.
    public bool Enabled { get; set; }
    public bool UseBuiltIn { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, string>? Credentials { get; set; }  // Sensitive - masked in response
    public string? TestEndpoint { get; set; }
    public DateTime? LastTested { get; set; }
    public string? ConnectionStatus { get; set; }
    public string? TestError { get; set; }
}

/// <summary>
/// DTO for worker/background job configuration
/// </summary>
public class WorkerConfigDto
{
    public bool Enabled { get; set; }
    public int MaxConcurrentJobs { get; set; } = 5;
    public int JobTimeoutMinutes { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    public string? ScheduleExpression { get; set; }  // Cron expression
}

/// <summary>
/// DTO for AI agent configuration
/// </summary>
public class AIAgentConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool Enabled { get; set; }
    public string? IconUrl { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// DTO for test configuration result
/// </summary>
public class ConfigurationTestResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for configuration change log entry
/// </summary>
public class ConfigurationChangeLogDto
{
    public int Id { get; set; }
    public string ConfigurationKey { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = null!;  // 'created', 'updated', 'deleted'
    public DateTime ChangedAt { get; set; }
    public string? ChangedByUserName { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// DTO for provider information (available providers)
/// </summary>
public class ProviderInfoDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? DocumentationUrl { get; set; }
    public List<string> RequiredFields { get; set; } = new();
    public bool IsBuiltIn { get; set; }
    public bool IsSaaS { get; set; }
}

/// <summary>
/// Response DTO for system configuration
/// </summary>
public class SystemConfigResponseDto
{
    public EmailServerConfigDto? EmailServer { get; set; }
    public TwoFactorConfigDto? TwoFactor { get; set; }
    public SocialLoginConfigDto? SocialLogin { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Response DTO for CRM configuration
/// </summary>
public class CRMConfigResponseDto
{
    public List<AIProviderConfigDto> AIProviders { get; set; } = new();
    public List<IntegrationConfigDto> Integrations { get; set; } = new();
    public WorkerConfigDto? WorkerConfig { get; set; }
    public List<AIAgentConfigDto> AIAgents { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for storing/retrieving provider configuration
/// </summary>
public class ProviderConfigurationDto
{
    public int Id { get; set; }
    public string ConfigurationKey { get; set; } = null!;
    public string ConfigurationType { get; set; } = null!;
    public string? ProviderName { get; set; }
    public string ConfigurationData { get; set; } = null!;  // Already decrypted by service
    public bool IsEncrypted { get; set; }
    public bool IsActive { get; set; }
    public bool CanBeDisabledAtRuntime { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestedStatus { get; set; }
    public string? LastTestedError { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedByUserName { get; set; }
}
