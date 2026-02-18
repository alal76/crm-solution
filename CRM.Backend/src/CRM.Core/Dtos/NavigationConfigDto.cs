// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;

namespace CRM.Core.Dtos
{
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
        public bool Accounts { get; set; }
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
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public string? RequiredPermission { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? AdminSubcategory { get; set; }
        public double Order { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public string? RequiredFeature { get; set; }
        public string? RequiredProvider { get; set; }
        public string? ModuleName { get; set; }
        public bool IsExternal { get; set; }
        public string? ExternalUrl { get; set; }
        public string? ProviderType { get; set; }
        public bool RequiresSystemAdmin { get; set; }
        public bool IsAdminItem { get; set; }
    }

    /// <summary>
    /// Navigation category configuration.
    /// </summary>
    public class NavigationCategoryConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Icon { get; set; }
    }

    /// <summary>
    /// Navigation subcategory for admin section.
    /// </summary>
    public class NavigationSubcategoryConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Icon { get; set; }
    }

    /// <summary>
    /// External service configuration for provider integration.
    /// </summary>
    public class ExternalServiceConfig
    {
        public bool Enabled { get; set; }
        public string? Url { get; set; }
        public string ProviderType { get; set; } = string.Empty;
        public bool UseInternal { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provider health status.
    /// </summary>
    public class ProviderStatus
    {
        public string ProviderType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsHealthy { get; set; }
        public DateTime LastHealthCheck { get; set; }
        public string? Message { get; set; }
    }
}
