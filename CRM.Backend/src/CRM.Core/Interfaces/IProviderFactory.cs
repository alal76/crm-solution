// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
