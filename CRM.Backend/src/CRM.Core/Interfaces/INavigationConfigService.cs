// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces
{
    /// <summary>
    /// Service for providing navigation configuration with pluggable architecture awareness and RBAC support.
    /// This service determines which navigation items should be shown based on:
    /// - User group permissions (RBAC)
    /// - Feature flags (enabled/disabled features)
    /// - Provider configuration (internal vs external services)
    /// - Module visibility configuration
    /// </summary>
    public interface INavigationConfigService
    {
        /// <summary>
        /// Gets the complete navigation configuration for the current deployment.
        /// </summary>
        Task<NavigationConfig> GetNavigationConfigAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets navigation items that are available based on current feature flags and providers.
        /// </summary>
        Task<IEnumerable<NavigationItemConfig>> GetAvailableNavItemsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets navigation configuration filtered by user permissions (RBAC).
        /// </summary>
        /// <param name="userId">The user ID to get permissions for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Navigation config filtered by user's group permissions.</returns>
        Task<NavigationConfig> GetNavigationConfigForUserAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets navigation items filtered by user's group permissions.
        /// </summary>
        /// <param name="userId">The user ID to get permissions for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Navigation items the user is allowed to access.</returns>
        Task<IEnumerable<NavigationItemConfig>> GetAvailableNavItemsForUserAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user's effective permissions from their group memberships.
        /// </summary>
        /// <param name="userId">The user ID to get permissions for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Aggregated permissions from all groups the user belongs to.</returns>
        Task<UserNavigationPermissions> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets module visibility configuration (which modules are enabled/visible).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Module visibility settings.</returns>
        Task<Dictionary<string, ModuleConfig>> GetModuleConfigsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets external service URLs for pluggable providers (n8n, Superset, etc.).
        /// </summary>
        Task<Dictionary<string, ExternalServiceConfig>> GetExternalServiceConfigsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the provider status for all pluggable services.
        /// </summary>
        Task<Dictionary<string, ProviderStatus>> GetProviderStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates the navigation config cache (call after permission changes).
        /// </summary>
        void InvalidateCache();
    }

}
