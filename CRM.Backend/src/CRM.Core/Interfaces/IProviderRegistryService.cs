// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for managing and querying the provider registry.
///
/// HEXAGONAL ARCHITECTURE:
/// - Input Port: provides admin-facing operations for provider management
/// - Accessed by: AdminProvidersController
/// - Depends on: IConfiguration, IProviderHealthService, IProviderConfigurationService
///
/// Provider categories: Search, Chat, Notifications, Analytics, Signatures, AI, Integrations
/// </summary>
public interface IProviderRegistryService
{
    /// <summary>
    /// Get all registered providers across all categories.
    /// </summary>
    Task<IEnumerable<ProviderRegistryEntry>> GetAllProvidersAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a specific provider entry by category and provider type.
    /// Returns null if the provider is not registered.
    /// </summary>
    Task<ProviderRegistryEntry?> GetProviderAsync(string category, string providerType, CancellationToken ct = default);

    /// <summary>
    /// Get all registered providers for a specific category.
    /// </summary>
    Task<IEnumerable<ProviderRegistryEntry>> GetProvidersByCategoryAsync(string category, CancellationToken ct = default);

    /// <summary>
    /// Check if a provider type is available (registered and can be resolved).
    /// </summary>
    Task<bool> IsProviderAvailableAsync(string category, string providerType, CancellationToken ct = default);

    /// <summary>
    /// Perform a live health check against a specific provider.
    /// </summary>
    Task<ProviderHealthStatusResult> CheckProviderHealthAsync(string category, string providerType, CancellationToken ct = default);

    /// <summary>
    /// Get the active provider configuration for a category including its config fields.
    /// </summary>
    Task<ProviderConfigurationDto?> GetActiveProviderConfigAsync(string category, CancellationToken ct = default);

    /// <summary>
    /// Activate a provider for a category and persist its configuration.
    /// Updates the Providers:{Category}:Type configuration and stores connection settings.
    /// </summary>
    Task SetActiveProviderAsync(string category, string providerType, Dictionary<string, string> config, CancellationToken ct = default);

    /// <summary>
    /// Get the current state of all provider-related feature flags.
    /// </summary>
    Task<ProviderFeatureFlagsDto> GetProviderFeatureFlagsAsync(CancellationToken ct = default);

    /// <summary>
    /// Update provider feature flags (UseExternalSearch, UseExternalAI, etc.).
    /// </summary>
    Task UpdateProviderFeatureFlagsAsync(Dictionary<string, bool> flags, CancellationToken ct = default);
}
