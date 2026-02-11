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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>
    /// User's effective navigation permissions from their group memberships.
    /// </summary>
    public class UserNavigationPermissions
    {
        /// <summary>
        /// User ID these permissions are for.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Whether the user has system admin access (bypasses all checks).
        /// </summary>
        public bool IsSystemAdmin { get; set; }

        /// <summary>
        /// Menu access permissions (aggregated from all groups).
        /// </summary>
        public MenuAccessPermissions MenuAccess { get; set; } = new();

        /// <summary>
        /// CRUD permissions per entity type.
        /// </summary>
        public Dictionary<string, EntityCrudPermissions> EntityPermissions { get; set; } = new();

        /// <summary>
        /// Data access scope (own, team, all).
        /// </summary>
        public string DataAccessScope { get; set; } = "own";

        /// <summary>
        /// Bulk operation permissions.
        /// </summary>
        public BulkOperationPermissions BulkOperations { get; set; } = new();

        /// <summary>
        /// Group IDs the user belongs to.
        /// </summary>
        public List<int> GroupIds { get; set; } = new();

        /// <summary>
        /// Group names the user belongs to.
        /// </summary>
        public List<string> GroupNames { get; set; } = new();
    }

    /// <summary>
    /// Menu/navigation access permissions.
    /// </summary>
    public class MenuAccessPermissions
    {
        public bool Dashboard { get; set; }
        public bool Customers { get; set; }
        public bool Contacts { get; set; }
        public bool Leads { get; set; }
        public bool Opportunities { get; set; }
        public bool Products { get; set; }
        public bool Services { get; set; }
        public bool Campaigns { get; set; }
        public bool Quotes { get; set; }
        public bool Tasks { get; set; }
        public bool Activities { get; set; }
        public bool Notes { get; set; }
        public bool Workflows { get; set; }
        public bool ServiceRequests { get; set; }
        public bool ITSM { get; set; }
        public bool Reports { get; set; }
        public bool Settings { get; set; }
        public bool UserManagement { get; set; }
    }

    /// <summary>
    /// CRUD permissions for an entity type.
    /// </summary>
    public class EntityCrudPermissions
    {
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanViewAll { get; set; }
    }

    /// <summary>
    /// Bulk operation permissions.
    /// </summary>
    public class BulkOperationPermissions
    {
        public bool CanExport { get; set; }
        public bool CanImport { get; set; }
        public bool CanBulkEdit { get; set; }
        public bool CanBulkDelete { get; set; }
    }

    /// <summary>
    /// Module configuration for visibility.
    /// </summary>
    public class ModuleConfig
    {
        /// <summary>
        /// Module name (e.g., "CRM", "ITSM", "Marketing").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the module.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this module is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether this module is visible in navigation.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Feature flag that controls this module.
        /// </summary>
        public string? FeatureFlag { get; set; }

        /// <summary>
        /// Navigation items in this module.
        /// </summary>
        public List<string> NavItems { get; set; } = new();
    }

    /// <summary>
    /// Complete navigation configuration response.
    /// </summary>
    public class NavigationConfig
    {
        /// <summary>
        /// All available navigation items.
        /// </summary>
        public List<NavigationItemConfig> NavItems { get; set; } = new();

        /// <summary>
        /// Navigation categories.
        /// </summary>
        public List<NavigationCategoryConfig> Categories { get; set; } = new();

        /// <summary>
        /// Admin subcategories.
        /// </summary>
        public List<NavigationSubcategoryConfig> AdminSubcategories { get; set; } = new();

        /// <summary>
        /// External service configurations for pluggable providers.
        /// </summary>
        public Dictionary<string, ExternalServiceConfig> ExternalServices { get; set; } = new();

        /// <summary>
        /// Provider status information.
        /// </summary>
        public Dictionary<string, ProviderStatus> ProviderStatus { get; set; } = new();

        /// <summary>
        /// Feature flags affecting navigation.
        /// </summary>
        public Dictionary<string, bool> FeatureFlags { get; set; } = new();
    }

    /// <summary>
    /// Configuration for a navigation item.
    /// </summary>
    public class NavigationItemConfig
    {
        /// <summary>
        /// Unique identifier for the nav item.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Path/route for the item.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Icon identifier.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Menu permission name.
        /// </summary>
        public string MenuName { get; set; } = string.Empty;

        /// <summary>
        /// Category ID this item belongs to.
        /// </summary>
        public string Category { get; set; } = "main";

        /// <summary>
        /// Admin subcategory (for admin items only).
        /// </summary>
        public string? AdminSubcategory { get; set; }

        /// <summary>
        /// Display order within category.
        /// </summary>
        public double Order { get; set; }

        /// <summary>
        /// Whether this item is visible by default.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Whether this item is enabled (based on feature flags/providers).
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Required feature flag for this item to be available.
        /// </summary>
        public string? RequiredFeature { get; set; }

        /// <summary>
        /// Required provider type for this item.
        /// </summary>
        public string? RequiredProvider { get; set; }

        /// <summary>
        /// Module name this item belongs to (for module-based visibility).
        /// </summary>
        public string? ModuleName { get; set; }

        /// <summary>
        /// Whether this item opens an external URL (for pluggable providers).
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// External URL (when using external provider).
        /// </summary>
        public string? ExternalUrl { get; set; }

        /// <summary>
        /// Provider type this item is associated with.
        /// </summary>
        public string? ProviderType { get; set; }

        /// <summary>
        /// Required permission for this nav item (maps to UserGroup.CanAccess* properties).
        /// E.g., "Customers", "Leads", "ITSM", "Settings"
        /// </summary>
        public string? RequiredPermission { get; set; }

        /// <summary>
        /// Whether this item requires system admin access.
        /// </summary>
        public bool RequiresSystemAdmin { get; set; }

        /// <summary>
        /// Whether this item is an admin-only item (requires CanAccessSettings or CanAccessUserManagement).
        /// </summary>
        public bool IsAdminItem { get; set; }
    }

    /// <summary>
    /// Configuration for a navigation category.
    /// </summary>
    public class NavigationCategoryConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Icon { get; set; }
        public bool Visible { get; set; } = true;
    }

    /// <summary>
    /// Configuration for an admin subcategory.
    /// </summary>
    public class NavigationSubcategoryConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    /// <summary>
    /// Configuration for an external/pluggable service.
    /// </summary>
    public class ExternalServiceConfig
    {
        /// <summary>
        /// Whether this external service is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// External service URL (e.g., n8n instance URL).
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Provider type (e.g., "N8n", "Superset", "BuiltIn").
        /// </summary>
        public string ProviderType { get; set; } = "BuiltIn";

        /// <summary>
        /// Whether to use the internal CRM feature instead of external.
        /// </summary>
        public bool UseInternal { get; set; } = true;

        /// <summary>
        /// Display name for the service.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Status of a pluggable provider.
    /// </summary>
    public class ProviderStatus
    {
        /// <summary>
        /// Provider type name.
        /// </summary>
        public string ProviderType { get; set; } = string.Empty;

        /// <summary>
        /// Whether the provider is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether the provider is healthy/available.
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Last health check timestamp.
        /// </summary>
        public DateTime? LastHealthCheck { get; set; }

        /// <summary>
        /// Any status message.
        /// </summary>
        public string? Message { get; set; }
    }
}
