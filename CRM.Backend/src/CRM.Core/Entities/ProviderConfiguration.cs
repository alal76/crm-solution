// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// FUNCTIONAL VIEW:
/// ===============
/// Configuration storage for external providers and integrations.
/// Allows administrators to manage API keys, credentials, and provider settings
/// from the Admin UI without modifying environment variables or configuration files.
///
/// TECHNICAL VIEW:
/// ===============
/// Stores encrypted JSON configuration data for each provider.
/// Supports both system-level and CRM-specific configurations.
/// Audit trail maintained through ConfigurationChangeLog entity.
/// Can be toggled to use built-in providers vs external providers.
/// </summary>
public class ProviderConfiguration : BaseEntity
{
    /// <summary>
    /// FUNCTIONAL: Unique identifier for this configuration (e.g., 'ai.provider.openai', 'email.server.smtp')
    /// TECHNICAL: Unique constraint, used as cache key
    /// </summary>
    public string ConfigurationKey { get; set; } = null!;

    /// <summary>
    /// FUNCTIONAL: Category type for organizing configurations
    /// TECHNICAL: Enum-like string: 'system' (infrastructure, security) or 'crm' (business, integrations)
    /// </summary>
    public string ConfigurationType { get; set; } = null!;  // 'system' or 'crm'

    /// <summary>
    /// FUNCTIONAL: Name of the provider (e.g., 'openai', 'chatwoot', 'meilisearch')
    /// TECHNICAL: Used for filtering and categorization
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// FUNCTIONAL: Encrypted JSON containing the provider configuration
    /// TECHNICAL: Stored encrypted at rest, decrypted on retrieval
    /// Example: {"apiKey":"sk-...", "model":"gpt-4", "temperature":0.7}
    /// </summary>
    public string ConfigurationData { get; set; } = null!;

    /// <summary>
    /// FUNCTIONAL: Indicates whether the stored data is encrypted
    /// TECHNICAL: Used to determine if data needs decryption on retrieval
    /// </summary>
    public bool IsEncrypted { get; set; } = true;

    /// <summary>
    /// FUNCTIONAL: Whether this configuration is active
    /// TECHNICAL: Inactive configs are not loaded at startup
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// FUNCTIONAL: Whether this configuration can be toggled without Docker restart
    /// TECHNICAL: Determines if change requires application restart
    /// </summary>
    public bool CanBeDisabledAtRuntime { get; set; }

    /// <summary>
    /// FUNCTIONAL: When the configuration was last tested for connectivity
    /// TECHNICAL: Used to show staleness of test results
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// FUNCTIONAL: Result of last connectivity test
    /// TECHNICAL: Values: 'success', 'error', 'untested'
    /// </summary>
    public string? LastTestedStatus { get; set; }

    /// <summary>
    /// FUNCTIONAL: Error message from last failed test
    /// TECHNICAL: Helps administrators diagnose configuration issues
    /// </summary>
    public string? LastTestedError { get; set; }

    /// <summary>
    /// TECHNICAL: User who created this configuration
    /// </summary>
    public int? CreatedByUserId { get; set; }

    /// <summary>
    /// TECHNICAL: Navigation property to Creator user
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// TECHNICAL: User who last updated this configuration
    /// </summary>
    public int? UpdatedByUserId { get; set; }

    /// <summary>
    /// TECHNICAL: Navigation property to Updater user
    /// </summary>
    public User? UpdatedByUser { get; set; }

    /// <summary>
    /// TECHNICAL: Change logs for audit trail
    /// </summary>
    public ICollection<ConfigurationChangeLog> ChangeLogs { get; set; } = new List<ConfigurationChangeLog>();
}

/// <summary>
/// FUNCTIONAL VIEW:
/// ===============
/// Audit trail for configuration changes.
/// Tracks who changed what, when, and what the values were before/after.
///
/// TECHNICAL VIEW:
/// ===============
/// Immutable records created on each configuration update.
/// Supports rollback to previous configuration states.
/// Stores old/new values for comparison (encrypted if sensitive).
/// </summary>
public class ConfigurationChangeLog : BaseEntity
{
    /// <summary>
    /// FUNCTIONAL: Which configuration this log entry is for
    /// TECHNICAL: Maps to ProviderConfiguration.ConfigurationKey
    /// </summary>
    public string ConfigurationKey { get; set; } = null!;

    /// <summary>
    /// FUNCTIONAL: Previous value before change
    /// TECHNICAL: Encrypted if sensitive, null for creation
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// FUNCTIONAL: New value after change
    /// TECHNICAL: Encrypted if sensitive
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// FUNCTIONAL: Type of change
    /// TECHNICAL: Values: 'created', 'updated', 'deleted'
    /// </summary>
    public string ChangeType { get; set; } = null!;

    /// <summary>
    /// FUNCTIONAL: When the change occurred
    /// TECHNICAL: Timestamp in UTC
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// TECHNICAL: User who made the change
    /// </summary>
    public int ChangedByUserId { get; set; }

    /// <summary>
    /// TECHNICAL: Navigation property to User
    /// </summary>
    public User? ChangedByUser { get; set; }

    /// <summary>
    /// TECHNICAL: IP address of the request (for forensics)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// TECHNICAL: User agent string (for browser/client identification)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// TECHNICAL: Reference to the ProviderConfiguration being changed
    /// </summary>
    public int? ProviderConfigurationId { get; set; }

    /// <summary>
    /// TECHNICAL: Navigation property
    /// </summary>
    public ProviderConfiguration? ProviderConfiguration { get; set; }
}
