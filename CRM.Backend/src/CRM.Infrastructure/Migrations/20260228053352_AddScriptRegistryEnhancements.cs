using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScriptRegistryEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InputSchemaJson",
                table: "ScriptPlugins",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LifecycleState",
                table: "ScriptPlugins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemoryLimitMb",
                table: "ScriptPlugins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutputSchemaJson",
                table: "ScriptPlugins",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PermissionsJson",
                table: "ScriptPlugins",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Runtime",
                table: "ScriptPlugins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SemVersion",
                table: "ScriptPlugins",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TimeoutSeconds",
                table: "ScriptPlugins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "LookupItems",
                type: "VARCHAR(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "LookupItems",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "LookupItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemValue",
                table: "LookupItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValidationRules",
                table: "LookupItems",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomValues",
                table: "LookupCategories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "LookupCategories",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemManaged",
                table: "LookupCategories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "LookupCategories",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ValidationSchema",
                table: "LookupCategories",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnumCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyName = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSystemManaged = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowCustomValues = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ValidationSchema = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScriptAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScriptPluginId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PerformedBy = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PerformedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousState = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewState = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScriptAuditLogs_ScriptPlugins_ScriptPluginId",
                        column: x => x.ScriptPluginId,
                        principalTable: "ScriptPlugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScriptVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScriptPluginId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentHash = table.Column<string>(type: "VARCHAR(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LifecycleState = table.Column<int>(type: "int", nullable: false),
                    ChangeNotes = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovedBy = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScriptVersions_ScriptPlugins_ScriptPluginId",
                        column: x => x.ScriptPluginId,
                        principalTable: "ScriptPlugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EnumValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemValue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Color = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidationRules = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnumValues_EnumCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "EnumCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EnumTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    FromValueId = table.Column<int>(type: "int", nullable: true),
                    ToValueId = table.Column<int>(type: "int", nullable: false),
                    IsAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowedRoles = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidateExpression = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnumTransitions_EnumCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "EnumCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnumTransitions_EnumValues_FromValueId",
                        column: x => x.FromValueId,
                        principalTable: "EnumValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnumTransitions_EnumValues_ToValueId",
                        column: x => x.ToValueId,
                        principalTable: "EnumValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_PriorityId",
                table: "ServiceRequests",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_StatusId",
                table: "ServiceRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptPlugins_LifecycleState",
                table: "ScriptPlugins",
                column: "LifecycleState");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_StageId",
                table: "Opportunities",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupCategories_EntityType",
                table: "LookupCategories",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_StatusId",
                table: "Leads",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_EnumCategories_Name",
                table: "EnumCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnumTransitions_CategoryId",
                table: "EnumTransitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnumTransitions_FromValueId",
                table: "EnumTransitions",
                column: "FromValueId");

            migrationBuilder.CreateIndex(
                name: "IX_EnumTransitions_ToValueId",
                table: "EnumTransitions",
                column: "ToValueId");

            migrationBuilder.CreateIndex(
                name: "IX_EnumValues_CategoryId_Key",
                table: "EnumValues",
                columns: new[] { "CategoryId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScriptAuditLogs_PerformedAt",
                table: "ScriptAuditLogs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptAuditLogs_ScriptPluginId",
                table: "ScriptAuditLogs",
                column: "ScriptPluginId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptVersions_ScriptPluginId",
                table: "ScriptVersions",
                column: "ScriptPluginId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptVersions_ScriptPluginId_IsCurrent",
                table: "ScriptVersions",
                columns: new[] { "ScriptPluginId", "IsCurrent" });

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_EnumValues_StatusId",
                table: "Leads",
                column: "StatusId",
                principalTable: "EnumValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_EnumValues_StageId",
                table: "Opportunities",
                column: "StageId",
                principalTable: "EnumValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_EnumValues_PriorityId",
                table: "ServiceRequests",
                column: "PriorityId",
                principalTable: "EnumValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_EnumValues_StatusId",
                table: "ServiceRequests",
                column: "StatusId",
                principalTable: "EnumValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_EnumValues_StatusId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_EnumValues_StageId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_EnumValues_PriorityId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_EnumValues_StatusId",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "EnumTransitions");

            migrationBuilder.DropTable(
                name: "ScriptAuditLogs");

            migrationBuilder.DropTable(
                name: "ScriptVersions");

            migrationBuilder.DropTable(
                name: "EnumValues");

            migrationBuilder.DropTable(
                name: "EnumCategories");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_PriorityId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_StatusId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ScriptPlugins_LifecycleState",
                table: "ScriptPlugins");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_StageId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_LookupCategories_EntityType",
                table: "LookupCategories");

            migrationBuilder.DropIndex(
                name: "IX_Leads_StatusId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "InputSchemaJson",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "LifecycleState",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "MemoryLimitMb",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "OutputSchemaJson",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "PermissionsJson",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "Runtime",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "SemVersion",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "TimeoutSeconds",
                table: "ScriptPlugins");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "IsSystemValue",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "ValidationRules",
                table: "LookupItems");

            migrationBuilder.DropColumn(
                name: "AllowCustomValues",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "IsSystemManaged",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "PropertyName",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "ValidationSchema",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Leads");
        }
    }
}
