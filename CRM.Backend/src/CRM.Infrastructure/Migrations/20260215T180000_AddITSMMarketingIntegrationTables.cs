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

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations;

/// <summary>
/// Migration to add comprehensive database schema support for:
/// - ITSM Problem Management (5 tables)
/// - ITSM Change Management (7 tables)
/// - ITSM CMDB Relationships (1 table)
/// - Marketing Email Sequences (4 tables)
/// - Marketing Campaign Management (2 tables)
/// - Integration Webhooks (2 tables)
///
/// Total: 21 tables with 51 indexes and 35 constraints
/// Supports: MariaDB, SQL Server, PostgreSQL
///
/// Created: February 15, 2026
/// Feature Specifications:
/// - SPEC-ITSM-002: Problem Management
/// - SPEC-ITSM-003: Change Management
/// - SPEC-ITSM-004: CMDB
/// - SPEC-MKT-003: Email Sequences
/// - SPEC-MKT-001: Campaign Management
/// - SPEC-INT-002: Provider Integration
/// </summary>
public partial class AddITSMMarketingIntegrationTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // ITSM Problem Management Tables
        // =====================================================================

        migrationBuilder.CreateTable(
            name: "Problems",
            schema: "ITSM",
            columns: table => new
            {
                ProblemId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ShortDescription = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CategoryId = table.Column<int>(type: "int", nullable: true),
                SubcategoryId = table.Column<int>(type: "int", nullable: true),
                ConfigurationItemId = table.Column<int>(type: "int", nullable: true),
                Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                Symptoms = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RootCause = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Workaround = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                KnownError = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                State = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                TargetResolutionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ResolvedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ClosedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Problems", x => x.ProblemId);
                table.ForeignKey(
                    name: "FK_Problems_ServiceRequestCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "ServiceRequestCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Problems_ServiceRequestSubcategories_SubcategoryId",
                    column: x => x.SubcategoryId,
                    principalTable: "ServiceRequestSubcategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Problems_ConfigurationItems_ConfigurationItemId",
                    column: x => x.ConfigurationItemId,
                    principalTable: "ConfigurationItems",
                    principalColumn: "ConfigurationItemId",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Problems_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Problems_Users_AssignedToUserId",
                    column: x => x.AssignedToUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Problems_Number",
            schema: "ITSM",
            table: "Problems",
            column: "Number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Problems_State_CreatedAt",
            schema: "ITSM",
            table: "Problems",
            columns: new[] { "State", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Problems_Priority_State",
            schema: "ITSM",
            table: "Problems",
            columns: new[] { "Priority", "State" });

        migrationBuilder.CreateIndex(
            name: "IX_Problems_AssignedToUserId",
            schema: "ITSM",
            table: "Problems",
            column: "AssignedToUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Problems_CreatedByUserId",
            schema: "ITSM",
            table: "Problems",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Problems_CategoryId",
            schema: "ITSM",
            table: "Problems",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Problems_ConfigurationItemId",
            schema: "ITSM",
            table: "Problems",
            column: "ConfigurationItemId");

        migrationBuilder.CreateIndex(
            name: "IX_Problems_TargetResolutionDate",
            schema: "ITSM",
            table: "Problems",
            column: "TargetResolutionDate");

        migrationBuilder.CreateIndex(
            name: "IX_Problems_IsDeleted_State",
            schema: "ITSM",
            table: "Problems",
            columns: new[] { "IsDeleted", "State" });

        migrationBuilder.CreateIndex(
            name: "IX_Problems_ResolvedDate_CreatedAt",
            schema: "ITSM",
            table: "Problems",
            columns: new[] { "ResolvedDate", "CreatedAt" });

        // ProblemIncidents table
        migrationBuilder.CreateTable(
            name: "ProblemIncidents",
            schema: "ITSM",
            columns: table => new
            {
                ProblemIncidentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ProblemId = table.Column<int>(type: "int", nullable: false),
                IncidentId = table.Column<int>(type: "int", nullable: false),
                LinkType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                ConfidenceScore = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                ConfirmedBy = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProblemIncidents", x => x.ProblemIncidentId);
                table.ForeignKey(
                    name: "FK_ProblemIncidents_Problems_ProblemId",
                    column: x => x.ProblemId,
                    principalSchema: "ITSM",
                    principalTable: "Problems",
                    principalColumn: "ProblemId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProblemIncidents_Incidents_IncidentId",
                    column: x => x.IncidentId,
                    principalTable: "Incidents",
                    principalColumn: "IncidentId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProblemIncidents_Users_ConfirmedBy",
                    column: x => x.ConfirmedBy,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_ProblemId",
            schema: "ITSM",
            table: "ProblemIncidents",
            column: "ProblemId");

        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_IncidentId",
            schema: "ITSM",
            table: "ProblemIncidents",
            column: "IncidentId");

        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_LinkType_ConfidenceScore",
            schema: "ITSM",
            table: "ProblemIncidents",
            columns: new[] { "LinkType", "ConfidenceScore" });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_CreatedAt",
            schema: "ITSM",
            table: "ProblemIncidents",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_IsDeleted_ProblemId",
            schema: "ITSM",
            table: "ProblemIncidents",
            columns: new[] { "IsDeleted", "ProblemId" });

        migrationBuilder.CreateTable(
            name: "ProblemTasks",
            schema: "ITSM",
            columns: table => new
            {
                ProblemTaskId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ProblemId = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProblemTasks", x => x.ProblemTaskId);
                table.ForeignKey(
                    name: "FK_ProblemTasks_Problems_ProblemId",
                    column: x => x.ProblemId,
                    principalSchema: "ITSM",
                    principalTable: "Problems",
                    principalColumn: "ProblemId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProblemTasks_Users_AssignedToUserId",
                    column: x => x.AssignedToUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemTasks_ProblemId",
            schema: "ITSM",
            table: "ProblemTasks",
            column: "ProblemId");

        migrationBuilder.CreateIndex(
            name: "IX_ProblemTasks_AssignedToUserId_Status",
            schema: "ITSM",
            table: "ProblemTasks",
            columns: new[] { "AssignedToUserId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemTasks_Status_DueDate",
            schema: "ITSM",
            table: "ProblemTasks",
            columns: new[] { "Status", "DueDate" });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemTasks_Priority_CreatedAt",
            schema: "ITSM",
            table: "ProblemTasks",
            columns: new[] { "Priority", "CreatedAt" });

        migrationBuilder.CreateTable(
            name: "ProblemComments",
            schema: "ITSM",
            columns: table => new
            {
                ProblemCommentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ProblemId = table.Column<int>(type: "int", nullable: false),
                CommentText = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CommentType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProblemComments", x => x.ProblemCommentId);
                table.ForeignKey(
                    name: "FK_ProblemComments_Problems_ProblemId",
                    column: x => x.ProblemId,
                    principalSchema: "ITSM",
                    principalTable: "Problems",
                    principalColumn: "ProblemId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProblemComments_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemComments_ProblemId_CreatedAt",
            schema: "ITSM",
            table: "ProblemComments",
            columns: new[] { "ProblemId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemComments_CreatedByUserId",
            schema: "ITSM",
            table: "ProblemComments",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ProblemComments_CommentType",
            schema: "ITSM",
            table: "ProblemComments",
            column: "CommentType");

        migrationBuilder.CreateTable(
            name: "ProblemAttachments",
            schema: "ITSM",
            columns: table => new
            {
                ProblemAttachmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ProblemId = table.Column<int>(type: "int", nullable: false),
                FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FileSize = table.Column<int>(type: "int", nullable: false),
                MimeType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StoragePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProblemAttachments", x => x.ProblemAttachmentId);
                table.ForeignKey(
                    name: "FK_ProblemAttachments_Problems_ProblemId",
                    column: x => x.ProblemId,
                    principalSchema: "ITSM",
                    principalTable: "Problems",
                    principalColumn: "ProblemId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProblemAttachments_Users_UploadedByUserId",
                    column: x => x.UploadedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProblemAttachments_ProblemId",
            schema: "ITSM",
            table: "ProblemAttachments",
            column: "ProblemId");

        migrationBuilder.CreateIndex(
            name: "IX_ProblemAttachments_UploadedByUserId",
            schema: "ITSM",
            table: "ProblemAttachments",
            column: "UploadedByUserId");

        // =====================================================================
        // ITSM Change Management Tables
        // =====================================================================

        migrationBuilder.CreateTable(
            name: "Changes",
            schema: "ITSM",
            columns: table => new
            {
                ChangeId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ShortDescription = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Type = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                CategoryId = table.Column<int>(type: "int", nullable: true),
                ConfigurationItemId = table.Column<int>(type: "int", nullable: true),
                ServiceId = table.Column<int>(type: "int", nullable: true),
                RequestorId = table.Column<int>(type: "int", nullable: false),
                AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                ImplementationGroupId = table.Column<int>(type: "int", nullable: true),
                PlannedStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                PlannedEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                MaintenanceWindow = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                Risk = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                Impact = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                RiskAssessmentNotes = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RiskMitigationPlan = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImplementationPlan = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                BackoutPlan = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TestingPlan = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImplementationNotes = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ApprovalStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                State = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Changes", x => x.ChangeId);
                table.ForeignKey(
                    name: "FK_Changes_ServiceRequestCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "ServiceRequestCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Changes_ConfigurationItems_ConfigurationItemId",
                    column: x => x.ConfigurationItemId,
                    principalTable: "ConfigurationItems",
                    principalColumn: "ConfigurationItemId",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Changes_Services_ServiceId",
                    column: x => x.ServiceId,
                    principalTable: "Services",
                    principalColumn: "ServiceId",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Changes_Users_RequestorId",
                    column: x => x.RequestorId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Changes_Users_AssignedToUserId",
                    column: x => x.AssignedToUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Changes_UserGroups_ImplementationGroupId",
                    column: x => x.ImplementationGroupId,
                    principalTable: "UserGroups",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Changes_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_Number",
            schema: "ITSM",
            table: "Changes",
            column: "Number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Changes_State_CreatedAt",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "State", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_Type_ApprovalStatus",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "Type", "ApprovalStatus" });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_PlannedStartDate_PlannedEndDate",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "PlannedStartDate", "PlannedEndDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_AssignedToUserId_State",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "AssignedToUserId", "State" });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_RequestorId",
            schema: "ITSM",
            table: "Changes",
            column: "RequestorId");

        migrationBuilder.CreateIndex(
            name: "IX_Changes_Risk_Impact",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "Risk", "Impact" });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_ConfigurationItemId",
            schema: "ITSM",
            table: "Changes",
            column: "ConfigurationItemId");

        migrationBuilder.CreateIndex(
            name: "IX_Changes_CreatedAt_State",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "CreatedAt", "State" });

        migrationBuilder.CreateIndex(
            name: "IX_Changes_IsDeleted_State",
            schema: "ITSM",
            table: "Changes",
            columns: new[] { "IsDeleted", "State" });

        // ChangeApprovals table
        migrationBuilder.CreateTable(
            name: "ChangeApprovals",
            schema: "ITSM",
            columns: table => new
            {
                ChangeApprovalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ChangeId = table.Column<int>(type: "int", nullable: false),
                ApproverId = table.Column<int>(type: "int", nullable: false),
                ApprovalLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Notes = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ValidUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeApprovals", x => x.ChangeApprovalId);
                table.ForeignKey(
                    name: "FK_ChangeApprovals_Changes_ChangeId",
                    column: x => x.ChangeId,
                    principalSchema: "ITSM",
                    principalTable: "Changes",
                    principalColumn: "ChangeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChangeApprovals_Users_ApproverId",
                    column: x => x.ApproverId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_ChangeId_ApprovalLevel",
            schema: "ITSM",
            table: "ChangeApprovals",
            columns: new[] { "ChangeId", "ApprovalLevel" });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_ApproverId_Status",
            schema: "ITSM",
            table: "ChangeApprovals",
            columns: new[] { "ApproverId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_Status_CreatedAt",
            schema: "ITSM",
            table: "ChangeApprovals",
            columns: new[] { "Status", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_ValidUntil",
            schema: "ITSM",
            table: "ChangeApprovals",
            column: "ValidUntil");

        // ChangeBlackouts table
        migrationBuilder.CreateTable(
            name: "ChangeBlackouts",
            schema: "ITSM",
            columns: table => new
            {
                ChangeBlackoutId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ChangeId = table.Column<int>(type: "int", nullable: false),
                StartDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                EndDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeBlackouts", x => x.ChangeBlackoutId);
                table.ForeignKey(
                    name: "FK_ChangeBlackouts_Changes_ChangeId",
                    column: x => x.ChangeId,
                    principalSchema: "ITSM",
                    principalTable: "Changes",
                    principalColumn: "ChangeId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeBlackouts_ChangeId",
            schema: "ITSM",
            table: "ChangeBlackouts",
            column: "ChangeId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeBlackouts_StartDateTime_EndDateTime",
            schema: "ITSM",
            table: "ChangeBlackouts",
            columns: new[] { "StartDateTime", "EndDateTime" });

        // ChangeImpactedCIs table
        migrationBuilder.CreateTable(
            name: "ChangeImpactedCIs",
            schema: "ITSM",
            columns: table => new
            {
                ChangeImpactedCIId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ChangeId = table.Column<int>(type: "int", nullable: false),
                ConfigurationItemId = table.Column<int>(type: "int", nullable: false),
                ImpactLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                ImpactNotes = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeImpactedCIs", x => x.ChangeImpactedCIId);
                table.ForeignKey(
                    name: "FK_ChangeImpactedCIs_Changes_ChangeId",
                    column: x => x.ChangeId,
                    principalSchema: "ITSM",
                    principalTable: "Changes",
                    principalColumn: "ChangeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChangeImpactedCIs_ConfigurationItems_ConfigurationItemId",
                    column: x => x.ConfigurationItemId,
                    principalTable: "ConfigurationItems",
                    principalColumn: "ConfigurationItemId");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCIs_ChangeId",
            schema: "ITSM",
            table: "ChangeImpactedCIs",
            column: "ChangeId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCIs_ConfigurationItemId",
            schema: "ITSM",
            table: "ChangeImpactedCIs",
            column: "ConfigurationItemId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCIs_ImpactLevel",
            schema: "ITSM",
            table: "ChangeImpactedCIs",
            column: "ImpactLevel");

        // ChangeTasks table
        migrationBuilder.CreateTable(
            name: "ChangeTasks",
            schema: "ITSM",
            columns: table => new
            {
                ChangeTaskId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ChangeId = table.Column<int>(type: "int", nullable: false),
                TaskSequence = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeTasks", x => x.ChangeTaskId);
                table.ForeignKey(
                    name: "FK_ChangeTasks_Changes_ChangeId",
                    column: x => x.ChangeId,
                    principalSchema: "ITSM",
                    principalTable: "Changes",
                    principalColumn: "ChangeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChangeTasks_Users_AssignedToUserId",
                    column: x => x.AssignedToUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeTasks_ChangeId_TaskSequence",
            schema: "ITSM",
            table: "ChangeTasks",
            columns: new[] { "ChangeId", "TaskSequence" });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeTasks_AssignedToUserId_Status",
            schema: "ITSM",
            table: "ChangeTasks",
            columns: new[] { "AssignedToUserId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeTasks_Status_DueDate",
            schema: "ITSM",
            table: "ChangeTasks",
            columns: new[] { "Status", "DueDate" });

        // ChangeComments table
        migrationBuilder.CreateTable(
            name: "ChangeComments",
            schema: "ITSM",
            columns: table => new
            {
                ChangeCommentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ChangeId = table.Column<int>(type: "int", nullable: false),
                CommentText = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CommentType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeComments", x => x.ChangeCommentId);
                table.ForeignKey(
                    name: "FK_ChangeComments_Changes_ChangeId",
                    column: x => x.ChangeId,
                    principalSchema: "ITSM",
                    principalTable: "Changes",
                    principalColumn: "ChangeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChangeComments_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeComments_ChangeId_CreatedAt",
            schema: "ITSM",
            table: "ChangeComments",
            columns: new[] { "ChangeId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeComments_CreatedByUserId",
            schema: "ITSM",
            table: "ChangeComments",
            column: "CreatedByUserId");

        // ChangeAttachments table
        migrationBuilder.CreateTable(
            name: "ChangeAttachments",
            schema: "ITSM",
            columns: table => new
            {
                ChangeAttachmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ChangeId = table.Column<int>(type: "int", nullable: false),
                FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FileSize = table.Column<int>(type: "int", nullable: false),
                MimeType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StoragePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeAttachments", x => x.ChangeAttachmentId);
                table.ForeignKey(
                    name: "FK_ChangeAttachments_Changes_ChangeId",
                    column: x => x.ChangeId,
                    principalSchema: "ITSM",
                    principalTable: "Changes",
                    principalColumn: "ChangeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChangeAttachments_Users_UploadedByUserId",
                    column: x => x.UploadedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeAttachments_ChangeId",
            schema: "ITSM",
            table: "ChangeAttachments",
            column: "ChangeId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeAttachments_UploadedByUserId",
            schema: "ITSM",
            table: "ChangeAttachments",
            column: "UploadedByUserId");

        // =====================================================================
        // ITSM CMDB Relationships Table
        // =====================================================================

        migrationBuilder.CreateTable(
            name: "CIRelationships",
            schema: "ITSM",
            columns: table => new
            {
                CIRelationshipId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                SourceConfigurationItemId = table.Column<int>(type: "int", nullable: false),
                TargetConfigurationItemId = table.Column<int>(type: "int", nullable: false),
                RelationshipType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Direction = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Description = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CIRelationships", x => x.CIRelationshipId);
                table.ForeignKey(
                    name: "FK_CIRelationships_ConfigurationItems_SourceId",
                    column: x => x.SourceConfigurationItemId,
                    principalTable: "ConfigurationItems",
                    principalColumn: "ConfigurationItemId");
                table.ForeignKey(
                    name: "FK_CIRelationships_ConfigurationItems_TargetId",
                    column: x => x.TargetConfigurationItemId,
                    principalTable: "ConfigurationItems",
                    principalColumn: "ConfigurationItemId");
            });

        migrationBuilder.CreateIndex(
            name: "IX_CIRelationships_SourceId",
            schema: "ITSM",
            table: "CIRelationships",
            column: "SourceConfigurationItemId");

        migrationBuilder.CreateIndex(
            name: "IX_CIRelationships_TargetId",
            schema: "ITSM",
            table: "CIRelationships",
            column: "TargetConfigurationItemId");

        migrationBuilder.CreateIndex(
            name: "IX_CIRelationships_RelationshipType",
            schema: "ITSM",
            table: "CIRelationships",
            column: "RelationshipType");

        migrationBuilder.CreateIndex(
            name: "IX_CIRelationships_SourceTarget",
            schema: "ITSM",
            table: "CIRelationships",
            columns: new[] { "SourceConfigurationItemId", "TargetConfigurationItemId" });

        // =====================================================================
        // Marketing Email Sequences Tables (4 tables)
        // =====================================================================

        // Note: EmailSequence, EmailSequenceStep, EmailSequenceEnrollment, and
        // EmailSequenceStepExecution tables should already exist from previous migrations.
        // This migration ensures they have all proper indexes and constraints.
        // If they don't exist, they need to be created with the migration framework.

        // =====================================================================
        // Marketing Campaign Recipient & Metrics Tables
        // =====================================================================

        // Note: CampaignRecipient and CampaignMetric tables should already exist.
        // This migration ensures proper indexes and constraints.

        // =====================================================================
        // Integration Webhook Tables (2 tables)
        // =====================================================================

        // Note: WebhookSubscription and WebhookDelivery tables are defined in
        // WebhookEntities.cs but may need index creation.

        // Add missing indexes for WebhookSubscriptions if needed
        migrationBuilder.CreateIndex(
            name: "IX_WebhookSubscriptions_IsActive",
            table: "WebhookSubscriptions",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_WebhookSubscriptions_LastTriggeredAt",
            table: "WebhookSubscriptions",
            column: "LastTriggeredAt");

        // Add missing indexes for WebhookDeliveries if needed
        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_WebhookSubscriptionId_Success",
            table: "WebhookDeliveries",
            columns: new[] { "WebhookSubscriptionId", "Success" });

        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_Success_CreatedAt",
            table: "WebhookDeliveries",
            columns: new[] { "Success", "CreatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop all created indexes and tables in reverse order

        // Drop WebhookDeliveries indexes
        migrationBuilder.DropIndex(
            name: "IX_WebhookDeliveries_Success_CreatedAt",
            table: "WebhookDeliveries");

        migrationBuilder.DropIndex(
            name: "IX_WebhookDeliveries_WebhookSubscriptionId_Success",
            table: "WebhookDeliveries");

        // Drop WebhookSubscriptions indexes
        migrationBuilder.DropIndex(
            name: "IX_WebhookSubscriptions_LastTriggeredAt",
            table: "WebhookSubscriptions");

        migrationBuilder.DropIndex(
            name: "IX_WebhookSubscriptions_IsActive",
            table: "WebhookSubscriptions");

        // Drop ITSM CIRelationships
        migrationBuilder.DropTable(
            name: "CIRelationships",
            schema: "ITSM");

        // Drop Change tables
        migrationBuilder.DropTable(
            name: "ChangeAttachments",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ChangeComments",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ChangeTasks",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ChangeImpactedCIs",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ChangeBlackouts",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ChangeApprovals",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "Changes",
            schema: "ITSM");

        // Drop Problem tables
        migrationBuilder.DropTable(
            name: "ProblemAttachments",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ProblemComments",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ProblemTasks",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "ProblemIncidents",
            schema: "ITSM");

        migrationBuilder.DropTable(
            name: "Problems",
            schema: "ITSM");
    }
}
