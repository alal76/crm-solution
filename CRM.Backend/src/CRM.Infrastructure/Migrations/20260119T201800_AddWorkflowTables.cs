using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Workflows table
            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            // Create WorkflowRules table
            migrationBuilder.CreateTable(
                name: "WorkflowRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkflowId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    TargetUserGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    ConditionLogic = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "AND"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowRules_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowRules_UserGroups_TargetUserGroupId",
                        column: x => x.TargetUserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create WorkflowRuleConditions table
            migrationBuilder.CreateTable(
                name: "WorkflowRuleConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkflowRuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ValueTwo = table.Column<string>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRuleConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowRuleConditions_WorkflowRules_WorkflowRuleId",
                        column: x => x.WorkflowRuleId,
                        principalTable: "WorkflowRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create WorkflowExecutions table
            migrationBuilder.CreateTable(
                name: "WorkflowExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkflowId = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkflowRuleId = table.Column<int>(type: "INTEGER", nullable: true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceUserGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetUserGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Success"),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    EntitySnapshotJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_WorkflowRules_WorkflowRuleId",
                        column: x => x.WorkflowRuleId,
                        principalTable: "WorkflowRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_UserGroups_SourceUserGroupId",
                        column: x => x.SourceUserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_UserGroups_TargetUserGroupId",
                        column: x => x.TargetUserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRules_WorkflowId",
                table: "WorkflowRules",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRules_TargetUserGroupId",
                table: "WorkflowRules",
                column: "TargetUserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRuleConditions_WorkflowRuleId",
                table: "WorkflowRuleConditions",
                column: "WorkflowRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_WorkflowId_CreatedAt",
                table: "WorkflowExecutions",
                columns: new[] { "WorkflowId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_EntityType_EntityId",
                table: "WorkflowExecutions",
                columns: new[] { "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "WorkflowExecutions");
            migrationBuilder.DropTable(name: "WorkflowRuleConditions");
            migrationBuilder.DropTable(name: "WorkflowRules");
            migrationBuilder.DropTable(name: "Workflows");
        }
    }
}
