// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Generic factory interface for runtime provider resolution.
/// Implements the Strategy + Factory pattern for pluggable architecture.
/// </summary>
/// <typeparam name="TProvider">The provider port interface type (e.g., ISearchPort, IChatPort)</typeparam>
public interface IProviderFactory<TProvider> where TProvider : class
{
    /// <summary>
    /// Gets the currently configured provider based on feature flags and configuration.
    /// Falls back to BuiltIn provider if external provider is not available.
    /// </summary>
    /// <returns>The active provider implementation</returns>
    TProvider GetProvider();

    /// <summary>
    /// Gets a specific provider by name.
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "Meilisearch", "Algolia", "BuiltIn")</param>
    /// <returns>The requested provider implementation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not found or not configured</exception>
    TProvider GetProvider(string providerName);

    /// <summary>
    /// Gets all available provider names for this category.
    /// </summary>
    /// <returns>Collection of available provider names</returns>
    IEnumerable<string> GetAvailableProviders();

    /// <summary>
    /// Gets the name of the currently active provider.
    /// </summary>
    /// <returns>The active provider name</returns>
    string GetActiveProviderName();

    /// <summary>
    /// Checks if a specific provider is configured and available.
    /// </summary>
    /// <param name="providerName">The provider name to check</param>
    /// <returns>True if the provider is available, false otherwise</returns>
    Task<bool> IsProviderAvailableAsync(string providerName);
}
