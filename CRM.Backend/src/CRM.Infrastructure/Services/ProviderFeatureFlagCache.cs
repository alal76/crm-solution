// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Services;

/// <summary>
/// Singleton cache for provider-related feature flags.
/// Populated once at application startup by <see cref="ProviderFeatureFlagCacheInitializer"/>
/// so that provider factories can read flag values synchronously without any
/// sync-over-async (GetAwaiter().GetResult()) calls (AP-015).
/// </summary>
public class ProviderFeatureFlagCache
{
    /// <summary>Gets or sets whether to use the external Search provider.</summary>
    public bool UseExternalSearch { get; set; }

    /// <summary>Gets or sets whether to use the external Chat provider.</summary>
    public bool UseExternalChat { get; set; }

    /// <summary>Gets or sets whether to use the external Notification provider.</summary>
    public bool UseExternalNotifications { get; set; }

    /// <summary>Gets or sets whether to use the external Analytics provider.</summary>
    public bool UseExternalAnalytics { get; set; }

    /// <summary>Gets or sets whether to use the external Signature provider.</summary>
    public bool UseExternalSignatures { get; set; }

    /// <summary>Gets or sets whether to use the external AI/LLM provider.</summary>
    public bool UseExternalAI { get; set; }

    /// <summary>Gets or sets whether to use the external Integrations provider.</summary>
    public bool UseExternalIntegrations { get; set; }
}
