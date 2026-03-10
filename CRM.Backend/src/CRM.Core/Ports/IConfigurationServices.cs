// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Ports;

/// <summary>
/// Port interface for provider configuration management.
/// Hexagonal Architecture: Primary/Driving Port.
///
/// Allows the application core to manage provider configurations
/// (AI, integrations, email, etc.) without knowing implementation details.
/// </summary>
public interface IProviderConfigurationService
{
    /// <summary>
    /// FUNCTIONAL: Get a specific configuration by its key
    /// TECHNICAL: Decrypts sensitive data before returning
    /// </summary>
    Task<ProviderConfigurationDto?> GetConfigurationAsync(
        string configKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Get all configurations of a specific type
    /// TECHNICAL: Returns decrypted configurations, filtered by ConfigurationType
    /// </summary>
    Task<List<ProviderConfigurationDto>> GetAllConfigurationsAsync(
        string? configurationType = null,  // 'system' or 'crm' or null for all
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update a configuration
    /// TECHNICAL: Encrypts sensitive data, creates audit log entry
    /// </summary>
    Task<ProviderConfigurationDto> UpdateConfigurationAsync(
        string configKey,
        Dictionary<string, object> configData,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Test a provider configuration for connectivity
    /// TECHNICAL: Validates credentials and connectivity without saving
    /// </summary>
    Task<ConfigurationTestResultDto> TestConfigurationAsync(
        string providerType,  // 'ai', 'search', 'chat', 'email', etc.
        string provider,      // 'openai', 'meilisearch', 'chatwoot', 'smtp', etc.
        Dictionary<string, object> configData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Get available providers for a given type
    /// TECHNICAL: Returns static list of supported providers
    /// </summary>
    Task<List<ProviderInfoDto>> GetAvailableProvidersAsync(
        string type,  // 'ai', 'search', 'chat', 'notifications', 'analytics', 'signatures', 'workflows'
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Get audit trail of configuration changes
    /// TECHNICAL: Returns change history, optionally filtered by config key
    /// </summary>
    Task<List<ConfigurationChangeLogDto>> GetChangeHistoryAsync(
        string? configKey = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Rollback configuration to a previous state
    /// TECHNICAL: Creates new change log entry for rollback, maintains full audit trail
    /// </summary>
    Task<ConfigurationTestResultDto> RollbackConfigurationAsync(
        int changeLogId,
        int userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Port interface for system-level configuration management.
/// Manages infrastructure and security configurations.
/// </summary>
public interface ISystemConfigurationService
{
    /// <summary>
    /// FUNCTIONAL: Get all system configurations (email, 2FA, social login)
    /// TECHNICAL: Decrypts sensitive data in response
    /// </summary>
    Task<SystemConfigResponseDto> GetSystemConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update email server configuration
    /// TECHNICAL: Encrypts credentials, creates audit log
    /// </summary>
    Task UpdateEmailServerAsync(
        EmailServerConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update two-factor authentication configuration
    /// TECHNICAL: Encrypts sensitive provider credentials
    /// </summary>
    Task UpdateTwoFactorAsync(
        TwoFactorConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update social login provider configurations
    /// TECHNICAL: Encrypts OAuth secrets for all providers
    /// </summary>
    Task UpdateSocialLoginAsync(
        SocialLoginConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Test email server connectivity
    /// TECHNICAL: Sends test email to validate configuration
    /// </summary>
    Task<ConfigurationTestResultDto> TestEmailServerAsync(
        EmailServerConfigDto config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Test social provider OAuth configuration
    /// TECHNICAL: Validates OAuth credentials with provider
    /// </summary>
    Task<ConfigurationTestResultDto> TestSocialProviderAsync(
        string provider,  // 'google', 'microsoft', 'azure', 'linkedin', 'facebook'
        Dictionary<string, string> credentials,
        CancellationToken cancellationToken = default);

    // UX-CONF-013: Aggregated communications config endpoint
    /// <summary>
    /// FUNCTIONAL: Get aggregated communications configuration (SMTP + channels).
    /// TECHNICAL: Delegates to existing email/channel config keys.
    /// </summary>
    Task<CommunicationsConfigDto> GetCommunicationsConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update communications configuration (SMTP + channel flags).
    /// TECHNICAL: Updates email config and channel flags independently.
    /// </summary>
    Task UpdateCommunicationsConfigAsync(
        CommunicationsConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Port interface for CRM-specific configuration management.
/// Manages business-related configurations like AI, integrations, agents.
/// </summary>
public interface ICRMConfigurationService
{
    /// <summary>
    /// FUNCTIONAL: Get all CRM configurations
    /// TECHNICAL: Decrypts sensitive data, returns organized response
    /// </summary>
    Task<CRMConfigResponseDto> GetCRMConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update AI provider configuration
    /// TECHNICAL: Encrypts API keys, updates in database
    /// </summary>
    Task UpdateAIProviderAsync(
        string provider,  // 'openai', 'azure', 'ollama', 'anthropic', 'bedrock', etc.
        AIProviderConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update integration configuration
    /// TECHNICAL: Encrypts sensitive data, creates audit log
    /// </summary>
    Task UpdateIntegrationAsync(
        string type,      // 'search', 'chat', 'notifications', 'analytics', 'signatures', 'workflows'
        string provider,  // provider-specific identifier
        IntegrationConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update worker/background job configuration
    /// TECHNICAL: No sensitive data, requires restart
    /// </summary>
    Task UpdateWorkerConfigAsync(
        WorkerConfigDto config,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Update AI agents configuration and enablement
    /// TECHNICAL: No sensitive data
    /// </summary>
    Task UpdateAIAgentsAsync(
        List<AIAgentConfigDto> agents,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Test AI provider connectivity
    /// TECHNICAL: Validates API key and connectivity
    /// </summary>
    Task<ConfigurationTestResultDto> TestAIProviderAsync(
        string provider,
        AIProviderConfigDto config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// FUNCTIONAL: Test integration connectivity
    /// TECHNICAL: Provider-specific validation
    /// </summary>
    Task<ConfigurationTestResultDto> TestIntegrationAsync(
        string type,
        string provider,
        IntegrationConfigDto config,
        CancellationToken cancellationToken = default);
}
