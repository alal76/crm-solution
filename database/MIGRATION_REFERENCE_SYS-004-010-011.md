// CRM Solution - Database Migrations Reference
// Migration for SYS-004, SYS-010, SYS-011 Feature Implementation
// 
// This file documents the required Entity Framework Core migrations
// to support Feature Flag Management, User Interface Management, 
// and Performance Optimization features.
//
// To create this migration, run:
// dotnet ef migrations add "AddSystemFeatureEntities" --context CrmDbContext
//
// Tables to be created:
// - FeatureFlagAuditLogs (audit trail for feature flag changes)
// - UIPreferences (user UI settings)
// - UICustomizations (module-specific UI customization)
// - DashboardCustomizations (dashboard layouts and widgets)
// - PerformanceMetrics (API and query performance tracking)

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Core.Entities;

namespace CRM.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Migration: Add System Feature Entities
    /// - FeatureFlagAuditLog entity (48 lines in entity definition)
    /// - UIPreference entity (70 lines in entity definition)
    /// - UICustomization entity (79 lines in entity definition)
    /// - DashboardCustomization entity (66 lines in entity definition)
    /// - PerformanceMetric entity (64 lines in entity definition)
    /// 
    /// Relationships:
    /// - FeatureFlagAuditLog.ChangedById → User.Id (FK, cascade delete)
    /// - UIPreference.UserId → User.Id (FK, cascade delete)
    /// - UICustomization.UserId → User.Id (FK, cascade delete)
    /// - DashboardCustomization.UserId → User.Id (FK, cascade delete)
    /// - PerformanceMetric.UserId → User.Id (optional FK, no cascade)
    /// 
    /// Indexes:
    /// - IX_FeatureFlagAuditLogs_FlagName
    /// - IX_FeatureFlagAuditLogs_ChangedAt DESC
    /// - IX_UIPreferences_UserId (unique)
    /// - IX_UICustomizations_UserId_ModuleName_PageName (unique composite)
    /// - IX_DashboardCustomizations_UserId
    /// - IX_DashboardCustomizations_IsDefault
    /// - IX_PerformanceMetrics_EndpointName
    /// - IX_PerformanceMetrics_RequestTime DESC
    /// - IX_PerformanceMetrics_UserId (optional)
    /// 
    /// Column Details:
    /// 
    /// FeatureFlagAuditLogs:
    /// - Id: int, PK, auto-increment
    /// - FlagName: nvarchar(256), required
    /// - OldValue: nvarchar(1000), nullable
    /// - NewValue: nvarchar(1000), required
    /// - ChangeType: nvarchar(50), required ('Enable', 'Disable', 'SetRollout', 'SetVariants', 'UpdateProvider')
    /// - ChangedById: int, required, FK to User.Id
    /// - ChangedAt: DateTime2, required, UTC
    /// - Reason: nvarchar(500), nullable
    /// - TargetingInfo: nvarchar(2000), nullable (JSON: {Users:[], Roles:[]})
    /// - CreatedAt: DateTime2 (inherited from BaseEntity)
    /// - UpdatedAt: DateTime2 (inherited from BaseEntity)
    /// - IsDeleted: bit (inherited from BaseEntity)
    /// - RowVersion: timestamp (inherited from BaseEntity)
    /// 
    /// UIPreferences:
    /// - Id: int, PK, auto-increment
    /// - UserId: int, required, FK to User.Id, unique
    /// - Theme: nvarchar(20), required (PossibleValues: 'light', 'dark', 'auto')
    /// - SidebarPosition: nvarchar(20), required (PossibleValues: 'left', 'right', 'hidden')
    /// - SidebarWidth: int, required (default: 250 pixels)
    /// - FontSize: nvarchar(20), required (PossibleValues: 'small', 'normal', 'large')
    /// - ShowBreadcrumbs: bit, required (default: true)
    /// - ShowStatusBar: bit, required (default: true)
    /// - ShowTopNavigation: bit, required (default: true)
    /// - DefaultPageSize: int, required (default: 20)
    /// - DateFormat: nvarchar(50), required (default: 'MM/dd/yyyy')
    /// - TimeFormat: nvarchar(50), required (default: 'hh:mm a')
    /// - CustomColorScheme: nvarchar(2000), nullable (JSON: {primary: #xxx, secondary: #yyy})
    /// - LastPreferenceUpdate: DateTime2, required
    /// - CreatedAt: DateTime2 (inherited)
    /// - UpdatedAt: DateTime2 (inherited)
    /// - IsDeleted: bit (inherited)
    /// - RowVersion: timestamp (inherited)
    /// 
    /// UICustomizations:
    /// - Id: int, PK, auto-increment
    /// - UserId: int, required, FK to User.Id
    /// - ModuleName: nvarchar(256), required (e.g., 'Accounts', 'Opportunities', 'Contacts')
    /// - PageName: nvarchar(256), required (e.g., 'List', 'Detail', 'Dashboard')
    /// - VisibleColumns: nvarchar(1000), required (CSV format or JSON array)
    /// - DefaultSortColumn: nvarchar(256), nullable
    /// - DefaultSortOrder: nvarchar(5), nullable ('asc', 'desc')
    /// - StoredFilters: nvarchar(2000), nullable (JSON array of filter configs)
    /// - SavedSearches: nvarchar(2000), nullable (JSON array of saved view names)
    /// - RowHeight: nvarchar(20), nullable (PossibleValues: 'compact', 'normal', 'comfortable')
    /// - ShowRowNumbers: bit, required (default: true)
    /// - ShowFilters: bit, required (default: true)
    /// - ColumnWidths: nvarchar(2000), nullable (JSON dict of column widths)
    /// - RowsPerPage: int, nullable
    /// - CreatedAt: DateTime2 (inherited)
    /// - UpdatedAt: DateTime2 (inherited)
    /// - IsDeleted: bit (inherited)
    /// - RowVersion: timestamp (inherited)
    /// - CompositeKey: (UserId, ModuleName, PageName) unique, skip soft deletes
    /// 
    /// DashboardCustomizations:
    /// - Id: int, PK, auto-increment
    /// - UserId: int, required, FK to User.Id
    /// - DashboardName: nvarchar(256), required (e.g., 'Sales Dashboard', 'Marketing Dashboard')
    /// - LayoutConfig: nvarchar(3000), required (JSON: {version: '1.0', gridLayout: {...}})
    /// - Widgets: nvarchar(max), required (JSON array of widget configs)
    /// - IsDefault: bit, required (only one per user can be true)
    /// - GridColumns: int, required (default: 12)
    /// - AutoRefresh: bit, required (default: false)
    /// - RefreshIntervalSeconds: int, required (default: 30)
    /// - LastModified: DateTime2, required
    /// - CreatedAt: DateTime2 (inherited)
    /// - UpdatedAt: DateTime2 (inherited)
    /// - IsDeleted: bit (inherited)
    /// - RowVersion: timestamp (inherited)
    /// - Constraint: (UserId, DashboardName) unique, skip soft deletes
    /// - Migration check: When setting IsDefault=true for one dashboard, others set to false
    /// 
    /// PerformanceMetrics:
    /// - Id: int, PK, auto-increment
    /// - EndpointName: nvarchar(256), required (e.g., '/api/accounts')
    /// - HttpMethod: nvarchar(10), required (GET, POST, PUT, DELETE, PATCH)
    /// - Route: nvarchar(512), required (full route template)
    /// - ResponseTimeMs: bigint, required (milliseconds, min: 0)
    /// - StatusCode: int, required (200, 404, 500, etc.)
    /// - QueryDurationMs: bigint, nullable (database query time)
    /// - RowsAffected: int, nullable (rows returned/modified)
    /// - WasCached: bit, required (default: false)
    /// - UserId: int, nullable (optional FK to User.Id, no cascade)
    /// - RequestTime: DateTime2, required (UTC)
    /// - CreatedAt: DateTime2 (inherited, same as RequestTime)
    /// - ErrorMessage: nvarchar(1000), nullable
    /// - QuerySignature: nvarchar(512), nullable (for grouping similar queries)
    /// - UpdatedAt: DateTime2 (inherited)
    /// - IsDeleted: bit (inherited)
    /// - RowVersion: timestamp (inherited)
    /// - Purge policy: Automatically delete records older than 30 days via service method
    /// 
    /// Soft Delete Configuration:
    /// All 5 tables inherit IsDeleted from BaseEntity.
    /// Global query filters should exclude soft-deleted records:
    ///   .HasQueryFilter(e => !e.IsDeleted)
    /// Except for DashboardCustomizations and UICustomizations unique constraints
    /// which must not include soft-deleted records.
    /// 
    /// Performance Considerations:
    /// - ClusterIndex on RequestTime DESC for PerformanceMetrics rapid filtering
    /// - Non-clustered index on EndpointName for endpoint grouping
    /// - Index on (UserId, ModuleName, PageName) for UICustomizations
    /// - Regular purging of PerformanceMetrics to prevent table bloat
    /// - Archive old FeatureFlagAuditLogs to separate table monthly
    /// 
    /// Row Size Limits:
    /// - FeatureFlagAuditLogs: ~800 bytes per record
    /// - UIPreferences: ~200 bytes per record  
    /// - UICustomizations: ~1500 bytes (with JSON)
    /// - DashboardCustomizations: ~5000 bytes (with widget configs)
    /// - PerformanceMetrics: ~400 bytes per record
    /// MariaDB row limit: 65535 bytes. No single table exceeds this for reasonable data.
    /// 
    /// Data Type Mapping:
    /// .NET             SQL Server                MariaDB              PostgreSQL
    /// ——————————————— ——————————————————————— ——————————————————— ————————————————
    /// int              int                     int                    int
    /// long             bigint                  bigint                 bigint
    /// bool             bit                     tinyint(1)             boolean
    /// string           nvarchar(max)           longtext               text
    /// string(256)      nvarchar(256)           varchar(256)           varchar(256)
    /// DateTime2        datetime2               datetime(6)            timestamp
    /// byte[]           varbinary(max)          longblob               bytea
    /// decimal          decimal(18,2)           decimal(18,2)          decimal(18,2)
    /// 
    /// DataSource Audit:
    /// - FeatureFlagAuditLogs: Populated by FeatureFlagManagementService methods
    ///   * UpdateFlagAsync() → creates audit entry for each flag change
    ///   * SetRolloutPercentageAsync() → records rollout % changes
    ///   * SetVariantsAsync() → records A/B test variant configs
    ///   * UpdateProviderTypeAsync() → records provider switches
    /// 
    /// - UIPreferences: Modified by UserInterfaceService
    ///   * SaveUIPreferencesAsync() → updates theme, layout, font settings
    ///   * ResetUIPreferencesAsync() → resets all 12 preferences to defaults
    ///   * Timestamp LastPreferenceUpdate updated on every save
    /// 
    /// - UICustomizations: Modified by UserInterfaceService
    ///   * SaveUICustomizationAsync() → saves module-page-specific config
    ///   * DeleteUICustomizationAsync() → soft delete
    /// 
    /// - DashboardCustomizations: Modified by UserInterfaceService
    ///   * SaveDashboardCustomizationAsync() → saves layout and widgets
    ///   * DeleteDashboardCustomizationAsync() → soft delete
    ///   * SetDefaultDashboardAsync() → sets IsDefault=true for one, false for others
    /// 
    /// - PerformanceMetrics: Populated by PerformanceOptimizationService
    ///   * RecordMetricAsync() → inserts metric on every API request
    ///   * PurgeOldMetricsAsync() → deletes records older than configurable days
    /// 
    /// Queries Impacted:
    /// - ToListAsync() on any DbSet automatically includes soft delete filter
    /// - FirstOrDefaultAsync() respects soft delete filter
    /// - Include/ThenInclude navigation to related entities honored
    /// - Raw SQL queries should explicitly add WHERE IsDeleted = 0
    /// 
    /// Testing Migrations:
    /// 1. dotnet ef migrations add "AddSystemFeatureEntities"
    /// 2. Review generated migration file for accuracy
    /// 3. dotnet ef database update
    /// 4. Verify tables created with correct schemas
    /// 5. Run unit tests to verify service layer operations
    /// 6. Run integration tests against new entities
    /// 7. Verify soft delete filter working correctly
    /// 
    /// Rollback Plan:
    /// If migration fails or issues discovered:
    /// 1. dotnet ef migrations remove (removes latest migration)
    /// 2. Fix entity definitions or mapping configuration
    /// 3. dotnet ef migrations add with corrected name
    /// 4. dotnet ef database update
    /// 
    /// Post-Deployment:
    /// 1. Monitor PerformanceMetrics table growth (implement purging)
    /// 2. Archive old FeatureFlagAuditLogs to FeatureFlagAuditLogs_Archive monthly
    /// 3. Validate all indexes exist and are being used
    /// 4. Check query execution plans for slow endpoints
    /// 5. Review soft delete filters working as intended
    /// 6. Verify no N+1 query problems in Include() scenarios
    /// 
    /// Seed Data (Optional):
    /// - Default UIPreference for all new users
    /// - Sample DashboardCustomizations (Sales, Marketing, Support dashboards)
    /// - Initial FeatureFlagAuditLog entry for admin
    /// 
    /// Version: 1.0
    /// Generated: February 2026
    /// Status: Ready for implementation
    /// 
    /// Required NuGet Packages:
    /// - Microsoft.EntityFrameworkCore
    /// - Microsoft.EntityFrameworkCore.SqlServer (or PostgreSQL/MySql provider)
    /// - Microsoft.EntityFrameworkCore.Tools (for migrations CLI)
    /// 
    /// Configuration in CrmDbContext.OnModelCreating():
    /// 
    /// modelBuilder.Entity<FeatureFlagAuditLog>(entity =>
    /// {
    ///     entity.HasKey(e => e.Id);
    ///     entity.Property(e => e.FlagName).IsRequired().HasMaxLength(256);
    ///     entity.Property(e => e.ChangeType).IsRequired().HasMaxLength(50);
    ///     entity.HasIndex(e => e.FlagName);
    ///     entity.HasIndex(e => e.ChangedAt).IsDescending();
    ///     entity.HasOne(e => e.ChangedBy)
    ///         .WithMany()
    ///         .HasForeignKey(e => e.ChangedById)
    ///         .OnDelete(DeleteBehavior.Restrict);
    ///     entity.HasQueryFilter(e => !e.IsDeleted);
    /// });
    /// 
    /// [Similar configuration for UIPreference, UICustomization, DashboardCustomization, PerformanceMetric]
    /// 
    /// This migration enables full implementation of:
    /// - SPEC-SYS-004: Feature Flag Management (database persistence layer)
    /// - SPEC-SYS-010: User Interface Management (persistence across sessions)
    /// - SPEC-SYS-011: Non-Functional Requirements (performance metrics storage)
    ///
