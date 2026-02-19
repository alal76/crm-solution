using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustryToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesRepUserId",
                table: "Commissions");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SLAPolicies",
                type: "VARCHAR(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SalesConfigurations",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "EmailSequenceSteps",
                keyColumn: "Template",
                keyValue: null,
                column: "Template",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Template",
                table: "EmailSequenceSteps",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EmailSequenceSteps",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "EmailSequenceSteps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ITSMEscalationPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITSMEscalationPolicies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ITSMEscalationLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EscalateAfterMinutes = table.Column<int>(type: "int", nullable: false),
                    NotifyUserId = table.Column<int>(type: "int", nullable: true),
                    NotifyTeamId = table.Column<int>(type: "int", nullable: true),
                    SendEmail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SendSms = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmailTemplateId = table.Column<int>(type: "int", nullable: true),
                    NotificationTemplate = table.Column<string>(type: "VARCHAR(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITSMEscalationLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationLevels_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ITSMEscalationLevels_ITSMEscalationPolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "ITSMEscalationPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationLevels_UserGroups_NotifyTeamId",
                        column: x => x.NotifyTeamId,
                        principalTable: "UserGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ITSMEscalationLevels_Users_NotifyUserId",
                        column: x => x.NotifyUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ITSMEscalationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    EscalationPolicyId = table.Column<int>(type: "int", nullable: false),
                    EscalationLevelId = table.Column<int>(type: "int", nullable: false),
                    EscalatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NotifiedUserId = table.Column<int>(type: "int", nullable: true),
                    NotifiedTeamId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "VARCHAR(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITSMEscalationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationHistories_ITSMEscalationLevels_EscalationLevel~",
                        column: x => x.EscalationLevelId,
                        principalTable: "ITSMEscalationLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationHistories_ITSMEscalationPolicies_EscalationPol~",
                        column: x => x.EscalationPolicyId,
                        principalTable: "ITSMEscalationPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationHistories_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationHistories_UserGroups_NotifiedTeamId",
                        column: x => x.NotifiedTeamId,
                        principalTable: "UserGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ITSMEscalationHistories_Users_NotifiedUserId",
                        column: x => x.NotifiedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationHistories_EscalationLevelId",
                table: "ITSMEscalationHistories",
                column: "EscalationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationHistories_EscalationPolicyId",
                table: "ITSMEscalationHistories",
                column: "EscalationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationHistories_IncidentId",
                table: "ITSMEscalationHistories",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationHistories_NotifiedTeamId",
                table: "ITSMEscalationHistories",
                column: "NotifiedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationHistories_NotifiedUserId",
                table: "ITSMEscalationHistories",
                column: "NotifiedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationLevels_EmailTemplateId",
                table: "ITSMEscalationLevels",
                column: "EmailTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationLevels_NotifyTeamId",
                table: "ITSMEscalationLevels",
                column: "NotifyTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationLevels_NotifyUserId",
                table: "ITSMEscalationLevels",
                column: "NotifyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationLevels_PolicyId",
                table: "ITSMEscalationLevels",
                column: "PolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ITSMEscalationHistories");

            migrationBuilder.DropTable(
                name: "ITSMEscalationLevels");

            migrationBuilder.DropTable(
                name: "ITSMEscalationPolicies");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "EmailSequenceSteps");

            migrationBuilder.UpdateData(
                table: "SLAPolicies",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SLAPolicies",
                type: "VARCHAR(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "SalesConfigurations",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SalesConfigurations",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Template",
                table: "EmailSequenceSteps",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EmailSequenceSteps",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SalesRepUserId",
                table: "Commissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
