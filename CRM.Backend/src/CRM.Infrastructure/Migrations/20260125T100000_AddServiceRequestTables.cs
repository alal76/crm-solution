/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRequestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ServiceRequestCategories table
            migrationBuilder.CreateTable(
                name: "ServiceRequestCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultPriority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SlaResponseHours = table.Column<int>(type: "int", nullable: true),
                    SlaResolutionHours = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create ServiceRequestSubcategories table
            migrationBuilder.CreateTable(
                name: "ServiceRequestSubcategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultWorkflowId = table.Column<int>(type: "int", nullable: true),
                    DefaultAssigneeGroupId = table.Column<int>(type: "int", nullable: true),
                    SlaResponseHours = table.Column<int>(type: "int", nullable: true),
                    SlaResolutionHours = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestSubcategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequestSubcategories_ServiceRequestCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ServiceRequestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequestSubcategories_Workflows_DefaultWorkflowId",
                        column: x => x.DefaultWorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequestSubcategories_UserGroups_DefaultAssigneeGroupId",
                        column: x => x.DefaultAssigneeGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create ServiceRequestCustomFieldDefinitions table
            migrationBuilder.CreateTable(
                name: "ServiceRequestCustomFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Options = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidationPattern = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidationMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AppliesToCategoryId = table.Column<int>(type: "int", nullable: true),
                    AppliesToSubcategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestCustomFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequestCustomFieldDefinitions_ServiceRequestCategories_AppliesToCategoryId",
                        column: x => x.AppliesToCategoryId,
                        principalTable: "ServiceRequestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequestCustomFieldDefinitions_ServiceRequestSubcategories_AppliesToSubcategoryId",
                        column: x => x.AppliesToSubcategoryId,
                        principalTable: "ServiceRequestSubcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create ServiceRequests table
            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TicketNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    SubcategoryId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedToGroupId = table.Column<int>(type: "int", nullable: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: true),
                    CurrentWorkflowStepId = table.Column<int>(type: "int", nullable: true),
                    SlaResponseDueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SlaResolutionDueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FirstResponseDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsSlaBreached = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ResolutionNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InternalNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerSatisfactionRating = table.Column<int>(type: "int", nullable: true),
                    CustomerFeedback = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalReferenceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceSystemId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceRequestCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ServiceRequestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceRequestSubcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "ServiceRequestSubcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_UserGroups_AssignedToGroupId",
                        column: x => x.AssignedToGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_WorkflowSteps_CurrentWorkflowStepId",
                        column: x => x.CurrentWorkflowStepId,
                        principalTable: "WorkflowSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create ServiceRequestCustomFieldValues table
            migrationBuilder.CreateTable(
                name: "ServiceRequestCustomFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestCustomFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequestCustomFieldValues_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequestCustomFieldValues_ServiceRequestCustomFieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "ServiceRequestCustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCategories_Name",
                table: "ServiceRequestCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCategories_IsActive",
                table: "ServiceRequestCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestSubcategories_CategoryId",
                table: "ServiceRequestSubcategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestSubcategories_DefaultWorkflowId",
                table: "ServiceRequestSubcategories",
                column: "DefaultWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestSubcategories_DefaultAssigneeGroupId",
                table: "ServiceRequestSubcategories",
                column: "DefaultAssigneeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCustomFieldDefinitions_AppliesToCategoryId",
                table: "ServiceRequestCustomFieldDefinitions",
                column: "AppliesToCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCustomFieldDefinitions_AppliesToSubcategoryId",
                table: "ServiceRequestCustomFieldDefinitions",
                column: "AppliesToSubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCustomFieldDefinitions_IsActive",
                table: "ServiceRequestCustomFieldDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_TicketNumber",
                table: "ServiceRequests",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Status",
                table: "ServiceRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Priority",
                table: "ServiceRequests",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CategoryId",
                table: "ServiceRequests",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SubcategoryId",
                table: "ServiceRequests",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CustomerId",
                table: "ServiceRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ContactId",
                table: "ServiceRequests",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_AssignedToUserId",
                table: "ServiceRequests",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_AssignedToGroupId",
                table: "ServiceRequests",
                column: "AssignedToGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_WorkflowId",
                table: "ServiceRequests",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CurrentWorkflowStepId",
                table: "ServiceRequests",
                column: "CurrentWorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CreatedAt",
                table: "ServiceRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_IsSlaBreached",
                table: "ServiceRequests",
                column: "IsSlaBreached");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCustomFieldValues_ServiceRequestId",
                table: "ServiceRequestCustomFieldValues",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCustomFieldValues_FieldDefinitionId",
                table: "ServiceRequestCustomFieldValues",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestCustomFieldValues_ServiceRequest_Field",
                table: "ServiceRequestCustomFieldValues",
                columns: new[] { "ServiceRequestId", "FieldDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ServiceRequestCustomFieldValues");
            migrationBuilder.DropTable(name: "ServiceRequests");
            migrationBuilder.DropTable(name: "ServiceRequestCustomFieldDefinitions");
            migrationBuilder.DropTable(name: "ServiceRequestSubcategories");
            migrationBuilder.DropTable(name: "ServiceRequestCategories");
        }
    }
}
