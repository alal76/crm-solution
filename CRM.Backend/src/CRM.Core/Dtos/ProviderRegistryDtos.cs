// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// Represents a provider registered in the provider registry.
/// Each entry describes a specific provider implementation for a given category.
/// </summary>
public class ProviderRegistryEntry
{
    /// <summary>Provider category (Search, Chat, Notifications, Analytics, Signatures, AI, Integrations)</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Provider type identifier (Meilisearch, Ollama, Chatwoot, etc.)</summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>Human-readable display name</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Short description of the provider and its capabilities</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Whether this is the currently active provider for its category</summary>
    public bool IsActive { get; set; }

    /// <summary>Whether this provider has been configured with the required settings</summary>
    public bool IsConfigured { get; set; }

    /// <summary>Whether this is the built-in fallback provider (no external dependency)</summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>Whether this provider is a SaaS (cloud-hosted) service</summary>
    public bool IsSaaS { get; set; }

    /// <summary>URL to the provider's documentation</summary>
    public string? DocumentationUrl { get; set; }

    /// <summary>Configuration fields required to set up this provider</summary>
    public List<ProviderConfigField> ConfigFields { get; set; } = new();

    /// <summary>Current health status of the provider (null if not checked)</summary>
    public ProviderHealthStatusResult? HealthStatus { get; set; }
}

/// <summary>
/// Describes a single configuration field for a provider.
/// </summary>
public class ProviderConfigField
{
    /// <summary>Field key used in configuration (e.g., "Url", "ApiKey")</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable label for the field</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Input type: "text", "password", "url", "number", "boolean"</summary>
    public string Type { get; set; } = "text";

    /// <summary>Whether this field is required</summary>
    public bool Required { get; set; }

    /// <summary>Default value for the field</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Placeholder or hint text</summary>
    public string? Placeholder { get; set; }

    /// <summary>Help text explaining the field</summary>
    public string? HelpText { get; set; }
}

/// <summary>
/// Represents the health check result for a provider instance.
/// Named ProviderHealthStatusResult to avoid collision with the existing
/// ProviderHealthStatus enum in IProviderHealthService.
/// </summary>
public class ProviderHealthStatusResult
{
    /// <summary>Whether the provider is healthy and reachable</summary>
    public bool IsHealthy { get; set; }

    /// <summary>Human-readable status message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>UTC timestamp of when the health check was performed</summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional latency in milliseconds</summary>
    public long? LatencyMs { get; set; }

    /// <summary>Error details if unhealthy</summary>
    public string? ErrorDetails { get; set; }
}

/// <summary>
/// Request body for setting the active provider in a category.
/// </summary>
public class SetActiveProviderRequest
{
    /// <summary>Provider type to activate (e.g., "Meilisearch", "Ollama")</summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>Configuration key-value pairs for the provider</summary>
    public Dictionary<string, string> Config { get; set; } = new();
}

/// <summary>
/// Request body for updating provider feature flags.
/// </summary>
public class UpdateProviderFlagsRequest
{
    /// <summary>Map of feature flag name to enabled state</summary>
    public Dictionary<string, bool> Flags { get; set; } = new();
}

/// <summary>
/// Response containing all provider feature flags.
/// </summary>
public class ProviderFeatureFlagsDto
{
    public bool UseExternalSearch { get; set; }
    public bool UseExternalChat { get; set; }
    public bool UseExternalNotifications { get; set; }
    public bool UseExternalAnalytics { get; set; }
    public bool UseExternalSignatures { get; set; }
    public bool UseExternalAI { get; set; }
    public bool UseExternalIntegrations { get; set; }

    /// <summary>Active provider type per category (from Providers:{Category}:Type config)</summary>
    public Dictionary<string, string> ActiveProviders { get; set; } = new();
}
