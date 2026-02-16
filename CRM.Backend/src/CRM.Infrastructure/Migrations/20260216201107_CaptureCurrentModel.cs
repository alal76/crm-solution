using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaptureCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeApprovals_Users_ApproverId",
                table: "ChangeApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeImpactedCIs_ConfigurationItems_CIId",
                table: "ChangeImpactedCIs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_Contacts_ContactId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_EmailSequences_EmailSequenceId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_Leads_LeadId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_Users_EnrolledById",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequences_Users_OwnerId",
                table: "EmailSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequences_Users_SenderId",
                table: "EmailSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceStepExecutions_EmailSequenceSteps_EmailSequence~",
                table: "EmailSequenceStepExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationRules_SLAPolicies_SLAPolicyId",
                table: "EscalationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationRules_Users_ReassignToUserId",
                table: "EscalationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCIs_ConfigurationItems_CIId",
                table: "ServiceCIs");

            migrationBuilder.DropForeignKey(
                name: "FK_SLAInstances_SLAPolicies_SLAPolicyId",
                table: "SLAInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_SLAPolicies_BusinessHoursConfigs_BusinessHoursId",
                table: "SLAPolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_SLATargets_SLAPolicies_SLAPolicyId",
                table: "SLATargets");

            migrationBuilder.DropForeignKey(
                name: "FK_WebPageViews_WebSessions_WebSessionId",
                table: "WebPageViews");

            migrationBuilder.DropForeignKey(
                name: "FK_WebVisitors_Contacts_ContactId",
                table: "WebVisitors");

            migrationBuilder.DropForeignKey(
                name: "FK_WebVisitors_Leads_LeadId",
                table: "WebVisitors");

            migrationBuilder.DropIndex(
                name: "IX_SLAPolicies_BusinessHoursId",
                table: "SLAPolicies");

            migrationBuilder.DropIndex(
                name: "IX_ProblemIncidents_ProblemId",
                table: "ProblemIncidents");

            migrationBuilder.DropIndex(
                name: "IX_EmailSequenceEnrollments_EmailSequenceId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EscalationRules",
                table: "EscalationRules");

            migrationBuilder.DropIndex(
                name: "IX_EscalationRules_IsActive",
                table: "EscalationRules");

            migrationBuilder.DropIndex(
                name: "IX_EscalationRules_TriggerMetric",
                table: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "CasePriority",
                table: "SLAPolicies");

            migrationBuilder.DropColumn(
                name: "ExcludeHolidays",
                table: "SLAPolicies");

            migrationBuilder.RenameTable(
                name: "EscalationRules",
                newName: "EscalationRule");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                table: "SLAPolicies",
                newName: "WorkingHoursOnly");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSequenceStepExecutions_EmailSequenceEnrollmentId",
                table: "EmailSequenceStepExecutions",
                newName: "IX_EmailSequenceStepExecutions_EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_EscalationRules_SLAPolicyId",
                table: "EscalationRule",
                newName: "IX_EscalationRule_SLAPolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_EscalationRules_ReassignToUserId",
                table: "EscalationRule",
                newName: "IX_EscalationRule_ReassignToUserId");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ZipCodes",
                type: "TEXT",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "VisitorId",
                table: "WebVisitors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "WebVisitors",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "WebVisitors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "WebVisitors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "WebVisitors",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceType",
                table: "WebVisitors",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "WebVisitors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "WebVisitors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "BrowserVersion",
                table: "WebVisitors",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Browser",
                table: "WebVisitors",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "WebSessions",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Referrer",
                table: "WebSessions",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LandingPage",
                table: "WebSessions",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "WebSessions",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ExitPage",
                table: "WebSessions",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Referrer",
                table: "WebPageViews",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PageUrl",
                table: "WebPageViews",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PageTitle",
                table: "WebPageViews",
                type: "VARCHAR(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PagePath",
                table: "WebPageViews",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CommissionPlanId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomersEnabled",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "SLAPolicies",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EscalationPath",
                table: "SLAPolicies",
                type: "TEXT",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "InitialResponseTimeMinutes",
                table: "SLAPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResolutionTimeMinutes",
                table: "SLAPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ServiceRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusCode",
                table: "ServiceRequests",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ConfigurationItemCIId",
                table: "ServiceCIs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessHoursId",
                table: "ITSMSLAPolicies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "EntityAddressLinks",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "Template",
                table: "EmailSequenceSteps",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "Success",
                table: "EmailSequenceStepExecutions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "Replied",
                table: "EmailSequenceStepExecutions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<int>(
                name: "Opens",
                table: "EmailSequenceStepExecutions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MessageId",
                table: "EmailSequenceStepExecutions",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "EmailSequenceStepExecutions",
                type: "VARCHAR(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Clicks",
                table: "EmailSequenceStepExecutions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "Bounced",
                table: "EmailSequenceStepExecutions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "BounceType",
                table: "EmailSequenceStepExecutions",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EmailSequences",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EmailSequences",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "EmailSequenceEnrollments",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientEmail",
                table: "EmailSequenceEnrollments",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrolledAt",
                table: "EmailSequenceEnrollments",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStepIndex",
                table: "EmailSequenceEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SequenceId",
                table: "EmailSequenceEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumAmount",
                table: "CommissionTiers",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumAmount",
                table: "CommissionTiers",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "CommissionTiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Commissions",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SalesRepUserId",
                table: "Commissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CommissionPlans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "CommissionPlans",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "CommissionPlanAssignments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ChangeId",
                table: "ChangeBlackouts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                table: "CampaignRecipients",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TotalClicked",
                table: "CampaignMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalConverted",
                table: "CampaignMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDelivered",
                table: "CampaignMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalOpened",
                table: "CampaignMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalSent",
                table: "CampaignMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "ShippingSameAsBilling",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Accounts",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "WebhookUrl",
                table: "EscalationRule",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EscalationRule",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EscalationRule",
                table: "EscalationRule",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CommissionApprovalAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommissionId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionApprovalAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionApprovalAudits_Commissions_CommissionId",
                        column: x => x.CommissionId,
                        principalTable: "Commissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissionApprovalAudits_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommissionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: true),
                    CalculatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SalesAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CalculationDetails = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommissionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SaleType = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ApplicableProductIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicableUserIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRules", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DashboardCustomizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DashboardName = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LayoutConfig = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Widgets = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GridColumns = table.Column<int>(type: "int", nullable: false),
                    AutoRefresh = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RefreshIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardCustomizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardCustomizations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiscountHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    RuleId = table.Column<int>(type: "int", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DiscountRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MinQuantity = table.Column<int>(type: "int", nullable: true),
                    MaxQuantity = table.Column<int>(type: "int", nullable: true),
                    PromotionalCode = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerTier = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductCategory = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxDiscount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaxDiscountValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCumulative = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CumulativeWithOther = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ApplicableProductIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicableUserIds = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Conditions = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRules", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FeatureFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FeatureType = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSystemFlag = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ITSMEscalationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Queue = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgeInMinutes = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    TargetName = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Conditions = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SLAPolicyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITSMEscalationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITSMEscalationRules_ITSMSLAPolicies_SLAPolicyId",
                        column: x => x.SLAPolicyId,
                        principalTable: "ITSMSLAPolicies",
                        principalColumn: "SLAPolicyId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EndpointName = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HttpMethod = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Route = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    QueryDurationMs = table.Column<long>(type: "bigint", nullable: true),
                    RowsAffected = table.Column<int>(type: "int", nullable: true),
                    WasCached = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuerySignature = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Module = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSystemDefined = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    IsSystemDefined = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalesConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataType = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSystem = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesConfigurations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AssignmentGroup = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultSLAPolicyId = table.Column<int>(type: "int", nullable: true),
                    MaxQueueDepth = table.Column<int>(type: "int", nullable: true),
                    RoutingConfiguration = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceQueues", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SLAPolicy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CasePriority = table.Column<int>(type: "int", nullable: true),
                    CustomerSegmentsJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductsJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaseTypesJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerTiersJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchConditionsJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessHoursId = table.Column<int>(type: "int", nullable: true),
                    ExcludeHolidays = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLAPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SLAPolicy_BusinessHoursConfigs_BusinessHoursId",
                        column: x => x.BusinessHoursId,
                        principalTable: "BusinessHoursConfigs",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UICustomizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ModuleName = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PageName = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VisibleColumns = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultSortColumn = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultSortOrder = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoredFilters = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SavedSearches = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowHeight = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShowRowNumbers = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowFilters = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ColumnWidths = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowsPerPage = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UIPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SidebarPosition = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SidebarWidth = table.Column<int>(type: "int", nullable: false),
                    FontSize = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShowBreadcrumbs = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowStatusBar = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowTopNavigation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DefaultPageSize = table.Column<int>(type: "int", nullable: false),
                    DateFormat = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeFormat = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomColorScheme = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastPreferenceUpdate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebhookSubscriptions",
                columns: table => new
                {
                    WebhookSubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetUrl = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Secret = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    EventTypes = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Headers = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    LastTriggeredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookSubscriptions", x => x.WebhookSubscriptionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FeatureFlagAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FlagName = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValue = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangeType = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedById = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetingInfo = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeatureFlagId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagAuditLogs_FeatureFlags_FeatureFlagId",
                        column: x => x.FeatureFlagId,
                        principalTable: "FeatureFlags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeatureFlagAuditLogs_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FeatureFlagVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FeatureFlagId = table.Column<int>(type: "int", nullable: false),
                    VariantKey = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VariantValue = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoleAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    Notes = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignment_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignment_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignment_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebhookDeliveries",
                columns: table => new
                {
                    WebhookDeliveryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WebhookSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetUrl = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestBody = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DurationMs = table.Column<double>(type: "double", nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveries", x => x.WebhookDeliveryId);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveries_WebhookSubscriptions_WebhookSubscriptionId",
                        column: x => x.WebhookSubscriptionId,
                        principalTable: "WebhookSubscriptions",
                        principalColumn: "WebhookSubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WebVisitors_CreatedAt",
                table: "WebVisitors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebVisitors_VisitorId",
                table: "WebVisitors",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_WebSessions_SessionId",
                table: "WebSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebSessions_StartedAt",
                table: "WebSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebSessions_WebVisitorId_StartedAt",
                table: "WebSessions",
                columns: new[] { "WebVisitorId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WebPageViews_CreatedAt",
                table: "WebPageViews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebPageViews_WebVisitorId_CreatedAt",
                table: "WebPageViews",
                columns: new[] { "WebVisitorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCIs_ConfigurationItemCIId",
                table: "ServiceCIs",
                column: "ConfigurationItemCIId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCIs_ServiceId_CIId",
                table: "ServiceCIs",
                columns: new[] { "ServiceId", "CIId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProblemIncidents_ProblemId_IncidentId",
                table: "ProblemIncidents",
                columns: new[] { "ProblemId", "IncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ITSMSLAPolicies_BusinessHoursId",
                table: "ITSMSLAPolicies",
                column: "BusinessHoursId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAddressLinks_AccountId",
                table: "EntityAddressLinks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceStepExecutions_ExecutedAt",
                table: "EmailSequenceStepExecutions",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceStepExecutions_Success",
                table: "EmailSequenceStepExecutions",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_SequenceId",
                table: "EmailSequenceEnrollments",
                column: "SequenceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_SequenceId_Status",
                table: "EmailSequenceEnrollments",
                columns: new[] { "SequenceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_Status",
                table: "EmailSequenceEnrollments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeImpactedCIs_ChangeId_CIId",
                table: "ChangeImpactedCIs",
                columns: new[] { "ChangeId", "CIId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeImpactedCIs_ImpactLevel",
                table: "ChangeImpactedCIs",
                column: "Impact");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeBlackouts_ChangeId",
                table: "ChangeBlackouts",
                column: "ChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeApprovals_ApprovalStatus",
                table: "ChangeApprovals",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeApprovals_ChangeId_ApprovalRole",
                table: "ChangeApprovals",
                columns: new[] { "ChangeId", "ApprovalRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionApprovalAudits_ApprovedById",
                table: "CommissionApprovalAudits",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionApprovalAudits_CommissionId",
                table: "CommissionApprovalAudits",
                column: "CommissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardCustomizations_UserId",
                table: "DashboardCustomizations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagAuditLogs_ChangedById",
                table: "FeatureFlagAuditLogs",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagAuditLogs_FeatureFlagId",
                table: "FeatureFlagAuditLogs",
                column: "FeatureFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagVariants_FeatureFlagId",
                table: "FeatureFlagVariants",
                column: "FeatureFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_ITSMEscalationRules_SLAPolicyId",
                table: "ITSMEscalationRules",
                column: "SLAPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Category",
                table: "Permissions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_IsActive",
                table: "Permissions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_IsActive_IsSystemDefined",
                table: "Permissions",
                columns: new[] { "IsActive", "IsSystemDefined" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module",
                table: "Permissions",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module_Category",
                table: "Permissions",
                columns: new[] { "Module", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_HierarchyLevel",
                table: "Roles",
                column: "HierarchyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsActive",
                table: "Roles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsActive_IsSystemDefined",
                table: "Roles",
                columns: new[] { "IsActive", "IsSystemDefined" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SLAPolicy_BusinessHoursId",
                table: "SLAPolicy",
                column: "BusinessHoursId");

            migrationBuilder.CreateIndex(
                name: "IX_UICustomizations_UserId",
                table: "UICustomizations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UIPreferences_UserId",
                table: "UIPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignment_EffectiveFrom_EffectiveTo",
                table: "UserRoleAssignment",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignment_RoleId",
                table: "UserRoleAssignment",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignment_UserId",
                table: "UserRoleAssignment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignment_UserId_EffectiveFrom_EffectiveTo",
                table: "UserRoleAssignment",
                columns: new[] { "UserId", "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignment_UserId_RoleId_EffectiveFrom",
                table: "UserRoleAssignment",
                columns: new[] { "UserId", "RoleId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignment_UserId1",
                table: "UserRoleAssignment",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_Success_CreatedAt",
                table: "WebhookDeliveries",
                columns: new[] { "Success", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_WebhookSubscriptionId",
                table: "WebhookDeliveries",
                column: "WebhookSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_WebhookSubscriptionId_Success",
                table: "WebhookDeliveries",
                columns: new[] { "WebhookSubscriptionId", "Success" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscriptions_CreatedByUserId",
                table: "WebhookSubscriptions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscriptions_IsActive",
                table: "WebhookSubscriptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscriptions_LastTriggeredAt",
                table: "WebhookSubscriptions",
                column: "LastTriggeredAt");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeApprovals_Users_ApproverId",
                table: "ChangeApprovals",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeBlackouts_Changes_ChangeId",
                table: "ChangeBlackouts",
                column: "ChangeId",
                principalTable: "Changes",
                principalColumn: "ChangeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeImpactedCIs_ConfigurationItems_CIId",
                table: "ChangeImpactedCIs",
                column: "CIId",
                principalTable: "ConfigurationItems",
                principalColumn: "CIId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_Contacts_ContactId",
                table: "EmailSequenceEnrollments",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_EmailSequences_SequenceId",
                table: "EmailSequenceEnrollments",
                column: "SequenceId",
                principalTable: "EmailSequences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_Leads_LeadId",
                table: "EmailSequenceEnrollments",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_Users_EnrolledById",
                table: "EmailSequenceEnrollments",
                column: "EnrolledById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequences_Users_OwnerId",
                table: "EmailSequences",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequences_Users_SenderId",
                table: "EmailSequences",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceStepExecutions_EmailSequenceSteps_EmailSequence~",
                table: "EmailSequenceStepExecutions",
                column: "EmailSequenceStepId",
                principalTable: "EmailSequenceSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityAddressLinks_Accounts_AccountId",
                table: "EntityAddressLinks",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationRule_SLAPolicy_SLAPolicyId",
                table: "EscalationRule",
                column: "SLAPolicyId",
                principalTable: "SLAPolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationRule_Users_ReassignToUserId",
                table: "EscalationRule",
                column: "ReassignToUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ITSMSLAPolicies_BusinessHoursConfigs_BusinessHoursId",
                table: "ITSMSLAPolicies",
                column: "BusinessHoursId",
                principalTable: "BusinessHoursConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCIs_ConfigurationItems_CIId",
                table: "ServiceCIs",
                column: "CIId",
                principalTable: "ConfigurationItems",
                principalColumn: "CIId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCIs_ConfigurationItems_ConfigurationItemCIId",
                table: "ServiceCIs",
                column: "ConfigurationItemCIId",
                principalTable: "ConfigurationItems",
                principalColumn: "CIId");

            migrationBuilder.AddForeignKey(
                name: "FK_SLAInstances_SLAPolicy_SLAPolicyId",
                table: "SLAInstances",
                column: "SLAPolicyId",
                principalTable: "SLAPolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SLATargets_SLAPolicy_SLAPolicyId",
                table: "SLATargets",
                column: "SLAPolicyId",
                principalTable: "SLAPolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebPageViews_WebSessions_WebSessionId",
                table: "WebPageViews",
                column: "WebSessionId",
                principalTable: "WebSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WebVisitors_Contacts_ContactId",
                table: "WebVisitors",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WebVisitors_Leads_LeadId",
                table: "WebVisitors",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeApprovals_Users_ApproverId",
                table: "ChangeApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeBlackouts_Changes_ChangeId",
                table: "ChangeBlackouts");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeImpactedCIs_ConfigurationItems_CIId",
                table: "ChangeImpactedCIs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_Contacts_ContactId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_EmailSequences_SequenceId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_Leads_LeadId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceEnrollments_Users_EnrolledById",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequences_Users_OwnerId",
                table: "EmailSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequences_Users_SenderId",
                table: "EmailSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailSequenceStepExecutions_EmailSequenceSteps_EmailSequence~",
                table: "EmailSequenceStepExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityAddressLinks_Accounts_AccountId",
                table: "EntityAddressLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationRule_SLAPolicy_SLAPolicyId",
                table: "EscalationRule");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalationRule_Users_ReassignToUserId",
                table: "EscalationRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ITSMSLAPolicies_BusinessHoursConfigs_BusinessHoursId",
                table: "ITSMSLAPolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCIs_ConfigurationItems_CIId",
                table: "ServiceCIs");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCIs_ConfigurationItems_ConfigurationItemCIId",
                table: "ServiceCIs");

            migrationBuilder.DropForeignKey(
                name: "FK_SLAInstances_SLAPolicy_SLAPolicyId",
                table: "SLAInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_SLATargets_SLAPolicy_SLAPolicyId",
                table: "SLATargets");

            migrationBuilder.DropForeignKey(
                name: "FK_WebPageViews_WebSessions_WebSessionId",
                table: "WebPageViews");

            migrationBuilder.DropForeignKey(
                name: "FK_WebVisitors_Contacts_ContactId",
                table: "WebVisitors");

            migrationBuilder.DropForeignKey(
                name: "FK_WebVisitors_Leads_LeadId",
                table: "WebVisitors");

            migrationBuilder.DropTable(
                name: "CommissionApprovalAudits");

            migrationBuilder.DropTable(
                name: "CommissionHistories");

            migrationBuilder.DropTable(
                name: "CommissionRules");

            migrationBuilder.DropTable(
                name: "DashboardCustomizations");

            migrationBuilder.DropTable(
                name: "DiscountHistories");

            migrationBuilder.DropTable(
                name: "DiscountRules");

            migrationBuilder.DropTable(
                name: "FeatureFlagAuditLogs");

            migrationBuilder.DropTable(
                name: "FeatureFlagVariants");

            migrationBuilder.DropTable(
                name: "ITSMEscalationRules");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SalesConfigurations");

            migrationBuilder.DropTable(
                name: "ServiceQueues");

            migrationBuilder.DropTable(
                name: "SLAPolicy");

            migrationBuilder.DropTable(
                name: "UICustomizations");

            migrationBuilder.DropTable(
                name: "UIPreferences");

            migrationBuilder.DropTable(
                name: "UserRoleAssignment");

            migrationBuilder.DropTable(
                name: "WebhookDeliveries");

            migrationBuilder.DropTable(
                name: "FeatureFlags");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "WebhookSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_WebVisitors_CreatedAt",
                table: "WebVisitors");

            migrationBuilder.DropIndex(
                name: "IX_WebVisitors_VisitorId",
                table: "WebVisitors");

            migrationBuilder.DropIndex(
                name: "IX_WebSessions_SessionId",
                table: "WebSessions");

            migrationBuilder.DropIndex(
                name: "IX_WebSessions_StartedAt",
                table: "WebSessions");

            migrationBuilder.DropIndex(
                name: "IX_WebSessions_WebVisitorId_StartedAt",
                table: "WebSessions");

            migrationBuilder.DropIndex(
                name: "IX_WebPageViews_CreatedAt",
                table: "WebPageViews");

            migrationBuilder.DropIndex(
                name: "IX_WebPageViews_WebVisitorId_CreatedAt",
                table: "WebPageViews");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCIs_ConfigurationItemCIId",
                table: "ServiceCIs");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCIs_ServiceId_CIId",
                table: "ServiceCIs");

            migrationBuilder.DropIndex(
                name: "IX_ProblemIncidents_ProblemId_IncidentId",
                table: "ProblemIncidents");

            migrationBuilder.DropIndex(
                name: "IX_ITSMSLAPolicies_BusinessHoursId",
                table: "ITSMSLAPolicies");

            migrationBuilder.DropIndex(
                name: "IX_EntityAddressLinks_AccountId",
                table: "EntityAddressLinks");

            migrationBuilder.DropIndex(
                name: "IX_EmailSequenceStepExecutions_ExecutedAt",
                table: "EmailSequenceStepExecutions");

            migrationBuilder.DropIndex(
                name: "IX_EmailSequenceStepExecutions_Success",
                table: "EmailSequenceStepExecutions");

            migrationBuilder.DropIndex(
                name: "IX_EmailSequenceEnrollments_SequenceId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_EmailSequenceEnrollments_SequenceId_Status",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_EmailSequenceEnrollments_Status",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_ChangeImpactedCIs_ChangeId_CIId",
                table: "ChangeImpactedCIs");

            migrationBuilder.DropIndex(
                name: "IX_ChangeImpactedCIs_ImpactLevel",
                table: "ChangeImpactedCIs");

            migrationBuilder.DropIndex(
                name: "IX_ChangeBlackouts_ChangeId",
                table: "ChangeBlackouts");

            migrationBuilder.DropIndex(
                name: "IX_ChangeApprovals_ApprovalStatus",
                table: "ChangeApprovals");

            migrationBuilder.DropIndex(
                name: "IX_ChangeApprovals_ChangeId_ApprovalRole",
                table: "ChangeApprovals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EscalationRule",
                table: "EscalationRule");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ZipCodes");

            migrationBuilder.DropColumn(
                name: "CommissionPlanId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomersEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "SLAPolicies");

            migrationBuilder.DropColumn(
                name: "EscalationPath",
                table: "SLAPolicies");

            migrationBuilder.DropColumn(
                name: "InitialResponseTimeMinutes",
                table: "SLAPolicies");

            migrationBuilder.DropColumn(
                name: "ResolutionTimeMinutes",
                table: "SLAPolicies");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ConfigurationItemCIId",
                table: "ServiceCIs");

            migrationBuilder.DropColumn(
                name: "BusinessHoursId",
                table: "ITSMSLAPolicies");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "EntityAddressLinks");

            migrationBuilder.DropColumn(
                name: "Template",
                table: "EmailSequenceSteps");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EmailSequences");

            migrationBuilder.DropColumn(
                name: "SequenceId",
                table: "EmailSequenceEnrollments");

            migrationBuilder.DropColumn(
                name: "MaximumAmount",
                table: "CommissionTiers");

            migrationBuilder.DropColumn(
                name: "MinimumAmount",
                table: "CommissionTiers");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "CommissionTiers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Commissions");

            migrationBuilder.DropColumn(
                name: "SalesRepUserId",
                table: "Commissions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CommissionPlans");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "CommissionPlans");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "CommissionPlanAssignments");

            migrationBuilder.DropColumn(
                name: "ChangeId",
                table: "ChangeBlackouts");

            migrationBuilder.DropColumn(
                name: "Segment",
                table: "CampaignRecipients");

            migrationBuilder.DropColumn(
                name: "TotalClicked",
                table: "CampaignMetrics");

            migrationBuilder.DropColumn(
                name: "TotalConverted",
                table: "CampaignMetrics");

            migrationBuilder.DropColumn(
                name: "TotalDelivered",
                table: "CampaignMetrics");

            migrationBuilder.DropColumn(
                name: "TotalOpened",
                table: "CampaignMetrics");

            migrationBuilder.DropColumn(
                name: "TotalSent",
                table: "CampaignMetrics");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ShippingSameAsBilling",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "EscalationRule",
                newName: "EscalationRules");

            migrationBuilder.RenameColumn(
                name: "WorkingHoursOnly",
                table: "SLAPolicies",
                newName: "IsDefault");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSequenceStepExecutions_EnrollmentId",
                table: "EmailSequenceStepExecutions",
                newName: "IX_EmailSequenceStepExecutions_EmailSequenceEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_EscalationRule_SLAPolicyId",
                table: "EscalationRules",
                newName: "IX_EscalationRules_SLAPolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_EscalationRule_ReassignToUserId",
                table: "EscalationRules",
                newName: "IX_EscalationRules_ReassignToUserId");

            migrationBuilder.AlterColumn<string>(
                name: "VisitorId",
                table: "WebVisitors",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceType",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "BrowserVersion",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Browser",
                table: "WebVisitors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "WebSessions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Referrer",
                table: "WebSessions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LandingPage",
                table: "WebSessions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "WebSessions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ExitPage",
                table: "WebSessions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Referrer",
                table: "WebPageViews",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PageUrl",
                table: "WebPageViews",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PageTitle",
                table: "WebPageViews",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PagePath",
                table: "WebPageViews",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.AddColumn<int>(
                name: "CasePriority",
                table: "SLAPolicies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExcludeHolidays",
                table: "SLAPolicies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AlterColumn<bool>(
                name: "Success",
                table: "EmailSequenceStepExecutions",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Replied",
                table: "EmailSequenceStepExecutions",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Opens",
                table: "EmailSequenceStepExecutions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MessageId",
                table: "EmailSequenceStepExecutions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "EmailSequenceStepExecutions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Clicks",
                table: "EmailSequenceStepExecutions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "Bounced",
                table: "EmailSequenceStepExecutions",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "BounceType",
                table: "EmailSequenceStepExecutions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EmailSequences",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "EmailSequenceEnrollments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "RecipientEmail",
                table: "EmailSequenceEnrollments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrolledAt",
                table: "EmailSequenceEnrollments",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStepIndex",
                table: "EmailSequenceEnrollments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "WebhookUrl",
                table: "EscalationRules",
                type: "VARCHAR(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EscalationRules",
                type: "VARCHAR(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EscalationRules",
                table: "EscalationRules",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SLAPolicies_BusinessHoursId",
                table: "SLAPolicies",
                column: "BusinessHoursId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemIncidents_ProblemId",
                table: "ProblemIncidents",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSequenceEnrollments_EmailSequenceId",
                table: "EmailSequenceEnrollments",
                column: "EmailSequenceId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationRules_IsActive",
                table: "EscalationRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationRules_TriggerMetric",
                table: "EscalationRules",
                column: "TriggerMetric");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeApprovals_Users_ApproverId",
                table: "ChangeApprovals",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeImpactedCIs_ConfigurationItems_CIId",
                table: "ChangeImpactedCIs",
                column: "CIId",
                principalTable: "ConfigurationItems",
                principalColumn: "CIId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_Contacts_ContactId",
                table: "EmailSequenceEnrollments",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_EmailSequences_EmailSequenceId",
                table: "EmailSequenceEnrollments",
                column: "EmailSequenceId",
                principalTable: "EmailSequences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_Leads_LeadId",
                table: "EmailSequenceEnrollments",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceEnrollments_Users_EnrolledById",
                table: "EmailSequenceEnrollments",
                column: "EnrolledById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequences_Users_OwnerId",
                table: "EmailSequences",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequences_Users_SenderId",
                table: "EmailSequences",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSequenceStepExecutions_EmailSequenceSteps_EmailSequence~",
                table: "EmailSequenceStepExecutions",
                column: "EmailSequenceStepId",
                principalTable: "EmailSequenceSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationRules_SLAPolicies_SLAPolicyId",
                table: "EscalationRules",
                column: "SLAPolicyId",
                principalTable: "SLAPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationRules_Users_ReassignToUserId",
                table: "EscalationRules",
                column: "ReassignToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCIs_ConfigurationItems_CIId",
                table: "ServiceCIs",
                column: "CIId",
                principalTable: "ConfigurationItems",
                principalColumn: "CIId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SLAInstances_SLAPolicies_SLAPolicyId",
                table: "SLAInstances",
                column: "SLAPolicyId",
                principalTable: "SLAPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SLAPolicies_BusinessHoursConfigs_BusinessHoursId",
                table: "SLAPolicies",
                column: "BusinessHoursId",
                principalTable: "BusinessHoursConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SLATargets_SLAPolicies_SLAPolicyId",
                table: "SLATargets",
                column: "SLAPolicyId",
                principalTable: "SLAPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebPageViews_WebSessions_WebSessionId",
                table: "WebPageViews",
                column: "WebSessionId",
                principalTable: "WebSessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebVisitors_Contacts_ContactId",
                table: "WebVisitors",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebVisitors_Leads_LeadId",
                table: "WebVisitors",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id");
        }
    }
}
