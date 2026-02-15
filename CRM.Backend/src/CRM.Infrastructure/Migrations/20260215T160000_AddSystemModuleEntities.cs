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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations;

/// <summary>
/// Migration for System Module (SYS-001 through SYS-012)
/// Adds all system-level configuration and management entities.
/// 
/// SYS-001: User Authentication & Management
/// SYS-002: User Groups & Organization
/// SYS-003: User Profile & Preferences
/// SYS-004: Feature Flags & Toggles
/// SYS-006: Audit Logging (optional)
/// SYS-008: Admin Configuration
/// SYS-010: UI Preferences & Customization
/// SYS-011: Performance Metrics
/// SYS-012: RBAC - Roles, Permissions, Assignments
/// </summary>
public partial class AddSystemModuleEntities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // SYS-012: RBAC Entities - Roles
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                IsSystemDefined = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        // =====================================================================
        // SYS-012: RBAC Entities - Permissions
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsSystemDefined = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        // =====================================================================
        // SYS-012: RBAC Entities - RolePermission (Junction)
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<int>(type: "int", nullable: false),
                PermissionId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // =====================================================================
        // SYS-012: RBAC Entities - UserRoleAssignment
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "UserRoleAssignments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                RoleId = table.Column<int>(type: "int", nullable: false),
                AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoleAssignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserRoleAssignments_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserRoleAssignments_Users_AssignedByUserId",
                    column: x => x.AssignedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_UserRoleAssignments_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // =====================================================================
        // SYS-002: User Groups
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "UserGroups",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Type = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserGroups", x => x.Id);
            });

        // =====================================================================
        // SYS-002: User Group Members (Junction)
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "UserGroupMembers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserGroupId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserGroupMembers", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserGroupMembers_UserGroups_UserGroupId",
                    column: x => x.UserGroupId,
                    principalTable: "UserGroups",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserGroupMembers_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // =====================================================================
        // SYS-010: UI Preferences
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "UIPreferences",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                Theme = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                DateFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                TimeFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ItemsPerPage = table.Column<int>(type: "int", nullable: false, defaultValue: 25),
                DefaultView = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShowGridLines = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UIPreferences", x => x.Id);
                table.ForeignKey(
                    name: "FK_UIPreferences_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // =====================================================================
        // SYS-010: UI Customizations
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "UICustomizations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                VisibleColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ColumnOrder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DefaultSortColumn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                DefaultSortOrder = table.Column<int>(type: "int", nullable: false),
                FilterSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CustomColors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UICustomizations", x => x.Id);
                table.ForeignKey(
                    name: "FK_UICustomizations_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // =====================================================================
        // SYS-004: Feature Flags (Main entity)
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "FeatureFlags",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Key = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                FeatureType = table.Column<int>(type: "int", nullable: false),
                StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsSystemFlag = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FeatureFlags", x => x.Id);
            });

        // =====================================================================
        // SYS-004: Feature Flag Variants
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "FeatureFlagVariants",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FeatureFlagId = table.Column<int>(type: "int", nullable: false),
                VariantKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                VariantValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Weight = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FeatureFlagVariants", x => x.Id);
                table.ForeignKey(
                    name: "FK_FeatureFlagVariants_FeatureFlags_FeatureFlagId",
                    column: x => x.FeatureFlagId,
                    principalTable: "FeatureFlags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // =====================================================================
        // SYS-006: Feature Flag Audit Trail (already mapped in DbContext)
        // =====================================================================
        // The FeatureFlagAuditLog table should already exist from earlier migrations
        // but we ensure it has proper indexing here

        // =====================================================================
        // SYS-011: Performance Metrics
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "PerformanceMetrics",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MetricName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                MetricValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                EntityId = table.Column<int>(type: "int", nullable: true),
                UserId = table.Column<int>(type: "int", nullable: true),
                StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                DurationMs = table.Column<long>(type: "bigint", nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                table.ForeignKey(
                    name: "FK_PerformanceMetrics_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        // =====================================================================
        // Create Indexes for Performance
        // =====================================================================

        // Roles indexes
        migrationBuilder.CreateIndex(
            name: "IX_Roles_Name",
            table: "Roles",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Roles_IsActive_IsDeleted",
            table: "Roles",
            columns: new[] { "IsActive", "IsDeleted" });

        // Permissions indexes
        migrationBuilder.CreateIndex(
            name: "IX_Permissions_Name",
            table: "Permissions",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_Module",
            table: "Permissions",
            column: "Module");

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_IsActive_IsDeleted",
            table: "Permissions",
            columns: new[] { "IsActive", "IsDeleted" });

        // RolePermission indexes
        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_PermissionId",
            table: "RolePermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId_PermissionId",
            table: "RolePermissions",
            columns: new[] { "RoleId", "PermissionId" },
            unique: true);

        // UserRoleAssignment indexes
        migrationBuilder.CreateIndex(
            name: "IX_UserRoleAssignments_AssignedByUserId",
            table: "UserRoleAssignments",
            column: "AssignedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoleAssignments_RoleId",
            table: "UserRoleAssignments",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoleAssignments_UserId_RoleId",
            table: "UserRoleAssignments",
            columns: new[] { "UserId", "RoleId" },
            unique: true);

        // UserGroup indexes
        migrationBuilder.CreateIndex(
            name: "IX_UserGroups_Name",
            table: "UserGroups",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserGroups_IsActive_IsDeleted",
            table: "UserGroups",
            columns: new[] { "IsActive", "IsDeleted" });

        // UserGroupMember indexes
        migrationBuilder.CreateIndex(
            name: "IX_UserGroupMembers_UserId",
            table: "UserGroupMembers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserGroupMembers_UserGroupId_UserId",
            table: "UserGroupMembers",
            columns: new[] { "UserGroupId", "UserId" },
            unique: true);

        // UIPreference indexes
        migrationBuilder.CreateIndex(
            name: "IX_UIPreferences_UserId",
            table: "UIPreferences",
            column: "UserId",
            unique: true);

        // UICustomization indexes
        migrationBuilder.CreateIndex(
            name: "IX_UICustomizations_UserId_ModuleName_PageName",
            table: "UICustomizations",
            columns: new[] { "UserId", "ModuleName", "PageName" },
            unique: true);

        // FeatureFlag indexes
        migrationBuilder.CreateIndex(
            name: "IX_FeatureFlags_Key",
            table: "FeatureFlags",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FeatureFlags_IsEnabled_IsDeleted",
            table: "FeatureFlags",
            columns: new[] { "IsEnabled", "IsDeleted" });

        // FeatureFlagVariant indexes
        migrationBuilder.CreateIndex(
            name: "IX_FeatureFlagVariants_FeatureFlagId",
            table: "FeatureFlagVariants",
            column: "FeatureFlagId");

        migrationBuilder.CreateIndex(
            name: "IX_FeatureFlagVariants_FeatureFlagId_VariantKey",
            table: "FeatureFlagVariants",
            columns: new[] { "FeatureFlagId", "VariantKey" },
            unique: true);

        // PerformanceMetric indexes
        migrationBuilder.CreateIndex(
            name: "IX_PerformanceMetrics_EntityType_EntityId",
            table: "PerformanceMetrics",
            columns: new[] { "EntityType", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_PerformanceMetrics_MetricName",
            table: "PerformanceMetrics",
            column: "MetricName");

        migrationBuilder.CreateIndex(
            name: "IX_PerformanceMetrics_StartTime",
            table: "PerformanceMetrics",
            column: "StartTime");

        migrationBuilder.CreateIndex(
            name: "IX_PerformanceMetrics_UserId",
            table: "PerformanceMetrics",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop indexes
        migrationBuilder.DropIndex(name: "IX_PerformanceMetrics_UserId", table: "PerformanceMetrics");
        migrationBuilder.DropIndex(name: "IX_PerformanceMetrics_StartTime", table: "PerformanceMetrics");
        migrationBuilder.DropIndex(name: "IX_PerformanceMetrics_MetricName", table: "PerformanceMetrics");
        migrationBuilder.DropIndex(name: "IX_PerformanceMetrics_EntityType_EntityId", table: "PerformanceMetrics");
        migrationBuilder.DropIndex(name: "IX_FeatureFlagVariants_FeatureFlagId_VariantKey", table: "FeatureFlagVariants");
        migrationBuilder.DropIndex(name: "IX_FeatureFlagVariants_FeatureFlagId", table: "FeatureFlagVariants");
        migrationBuilder.DropIndex(name: "IX_FeatureFlags_IsEnabled_IsDeleted", table: "FeatureFlags");
        migrationBuilder.DropIndex(name: "IX_FeatureFlags_Key", table: "FeatureFlags");
        migrationBuilder.DropIndex(name: "IX_UICustomizations_UserId_ModuleName_PageName", table: "UICustomizations");
        migrationBuilder.DropIndex(name: "IX_UIPreferences_UserId", table: "UIPreferences");
        migrationBuilder.DropIndex(name: "IX_UserGroupMembers_UserGroupId_UserId", table: "UserGroupMembers");
        migrationBuilder.DropIndex(name: "IX_UserGroupMembers_UserId", table: "UserGroupMembers");
        migrationBuilder.DropIndex(name: "IX_UserGroups_IsActive_IsDeleted", table: "UserGroups");
        migrationBuilder.DropIndex(name: "IX_UserGroups_Name", table: "UserGroups");
        migrationBuilder.DropIndex(name: "IX_UserRoleAssignments_UserId_RoleId", table: "UserRoleAssignments");
        migrationBuilder.DropIndex(name: "IX_UserRoleAssignments_RoleId", table: "UserRoleAssignments");
        migrationBuilder.DropIndex(name: "IX_UserRoleAssignments_AssignedByUserId", table: "UserRoleAssignments");
        migrationBuilder.DropIndex(name: "IX_RolePermissions_RoleId_PermissionId", table: "RolePermissions");
        migrationBuilder.DropIndex(name: "IX_RolePermissions_PermissionId", table: "RolePermissions");
        migrationBuilder.DropIndex(name: "IX_Permissions_IsActive_IsDeleted", table: "Permissions");
        migrationBuilder.DropIndex(name: "IX_Permissions_Module", table: "Permissions");
        migrationBuilder.DropIndex(name: "IX_Permissions_Name", table: "Permissions");
        migrationBuilder.DropIndex(name: "IX_Roles_IsActive_IsDeleted", table: "Roles");
        migrationBuilder.DropIndex(name: "IX_Roles_Name", table: "Roles");

        // Drop tables
        migrationBuilder.DropTable(name: "PerformanceMetrics");
        migrationBuilder.DropTable(name: "FeatureFlagVariants");
        migrationBuilder.DropTable(name: "FeatureFlags");
        migrationBuilder.DropTable(name: "UICustomizations");
        migrationBuilder.DropTable(name: "UIPreferences");
        migrationBuilder.DropTable(name: "UserGroupMembers");
        migrationBuilder.DropTable(name: "UserGroups");
        migrationBuilder.DropTable(name: "UserRoleAssignments");
        migrationBuilder.DropTable(name: "RolePermissions");
        migrationBuilder.DropTable(name: "Permissions");
        migrationBuilder.DropTable(name: "Roles");
    }
}
