using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportSharesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionItems_Products_ProductId",
                table: "SubscriptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionUsages_SubscriptionItems_SubscriptionItemId",
                table: "SubscriptionUsages");

            migrationBuilder.AddColumn<int>(
                name: "ChainDepth",
                table: "WebhookDeliveriesGeneral",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "WebhookDeliveriesGeneral",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "WebhookDeliveriesGeneral",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "WebhookDeliveriesGeneral",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ParentEventId",
                table: "WebhookDeliveriesGeneral",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChunkNumber",
                table: "WebhookDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContinuationToken",
                table: "WebhookDeliveries",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "WebhookDeliveries",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "WebhookDeliveries",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentEventId",
                table: "WebhookDeliveries",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PayloadSizeBytes",
                table: "WebhookDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalChunks",
                table: "WebhookDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "WebAuthnCredentials",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "WebAuthnCredentials",
                type: "TEXT",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformCredential",
                table: "WebAuthnCredentials",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "WebAuthnCredentials",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "WebAuthnCredentials",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "WebAuthnCredentials",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SignCount",
                table: "WebAuthnCredentials",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IpBindingEnabled",
                table: "UserSessions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UsageType",
                table: "SubscriptionUsages",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SubscriptionUsages",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<string>(
                name: "MetricName",
                table: "SubscriptionUsages",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionUsages",
                type: "VARCHAR(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Limit",
                table: "SubscriptionUsageLimits",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "OneTimeFee",
                table: "Subscriptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MRR",
                table: "Subscriptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Subscriptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ARR",
                table: "Subscriptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingTimezone",
                table: "Subscriptions",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DunningAttemptCount",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DunningGracePeriodDays",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DunningNotificationEmails",
                table: "Subscriptions",
                type: "VARCHAR(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDunningDate",
                table: "Subscriptions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProrationType",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SendDunningEscalationEmails",
                table: "Subscriptions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndDate",
                table: "Subscriptions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartDate",
                table: "Subscriptions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "SubscriptionItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SubscriptionItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "SubscriptionItems",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionItems",
                type: "VARCHAR(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "SubscriptionItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnReason",
                table: "Orders",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ExchangeOrderId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibleForExchange",
                table: "Orders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QualityInspectionNotes",
                table: "Orders",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RestockingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnApprovedDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnAuthorizationNumber",
                table: "Orders",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnProcessedDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnReasonCategory",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnReceivedDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnRequestedDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnStatus",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReturnTrackingNumber",
                table: "Orders",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "Opportunities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetitorWinnerId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ForecastCategory",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LossReason",
                table: "Opportunities",
                type: "VARCHAR(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LossReasonCategory",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WinLossNotes",
                table: "Opportunities",
                type: "VARCHAR(4000)",
                maxLength: 4000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AuthorityScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BudgetScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChampionScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomQualificationJson",
                table: "Leads",
                type: "VARCHAR(4000)",
                maxLength: 4000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DecisionCriteriaScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecisionProcessScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EconomicBuyerScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstTouchDate",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentifyPainScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastContactedAt",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadSourceConfigId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadSourceId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetricsScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NeedScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NurtureCampaignEnrolledAt",
                table: "Leads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NurtureCampaignId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalSource",
                table: "Leads",
                type: "VARCHAR(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QualificationFrameworkType",
                table: "Leads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TerritoryId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimelineScore",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmCampaign",
                table: "Leads",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmMedium",
                table: "Leads",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmSource",
                table: "Leads",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AllowStacking",
                table: "CommissionRules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CommissionPlanId",
                table: "CommissionRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCommissionAmount",
                table: "CommissionRules",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinDealSize",
                table: "CommissionRules",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "CommissionRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SplitPercentage",
                table: "CommissionRules",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TriggerEvent",
                table: "CommissionRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "AuditLogs",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArticleVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortDescription = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedById = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangeNote = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleVersions_ITSMKnowledgeArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "ITSMKnowledgeArticles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleVersions_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Competitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Website = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Industry = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Strengths = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weaknesses = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OurAdvantages = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrimaryProducts = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PricingTier = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MarketSharePercent = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WinRateAgainst = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Notes = table.Column<string>(type: "VARCHAR(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitors", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContractVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    VersionLabel = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangeDescription = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangesJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SnapshotJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsArchived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContentHash = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractVersions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractVersions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceAuthorizationCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviceCode = table.Column<string>(type: "VARCHAR(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserCode = table.Column<string>(type: "VARCHAR(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientId = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Scope = table.Column<string>(type: "VARCHAR(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    IsAuthorized = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDenied = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AuthorizedUserId = table.Column<int>(type: "int", nullable: true),
                    AuthorizedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAuthorizationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAuthorizationCodes_Users_AuthorizedUserId",
                        column: x => x.AuthorizedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EscalationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    EscalatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EscalatedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationLogs_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscalationLogs_Users_EscalatedByUserId",
                        column: x => x.EscalatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadSourceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CostPerLead = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TrackingCode = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadSourceConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadSourceConfigs_MarketingCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LeadSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Medium = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CampaignName = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostPerLead = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TrackingUrl = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalPlatformId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadSources", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "VARCHAR(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "VARCHAR(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FailureReason = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    RiskFactors = table.Column<string>(type: "VARCHAR(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "VARCHAR(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "VARCHAR(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: true),
                    Longitude = table.Column<double>(type: "double", nullable: true),
                    IsAnomalous = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AlertSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "VARCHAR(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourOfDay = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NavigationConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Route = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiredRoles = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavigationConfigs_NavigationConfigs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "NavigationConfigs",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpportunityTeamMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SplitPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateRemoved = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Reason = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommissionPlanId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityTeamMembers_CommissionPlans_CommissionPlanId",
                        column: x => x.CommissionPlanId,
                        principalTable: "CommissionPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityTeamMembers_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityTeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReturnNumber = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RmaNumber = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    InitiatedById = table.Column<int>(type: "int", nullable: true),
                    ProcessedById = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonDescription = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RestockingFee = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ShippingRefund = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReturnTrackingNumber = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReturnCarrier = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefundTransactionId = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineItemsJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderReturns_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderReturns_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderReturns_Users_InitiatedById",
                        column: x => x.InitiatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderReturns_Users_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReportShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Permission = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SharedByUserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportShares_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportShares_Users_SharedByUserId",
                        column: x => x.SharedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportShares_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Territories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ParentTerritoryId = table.Column<int>(type: "int", nullable: true),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Countries = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    States = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cities = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCodePatterns = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Industries = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinCompanySize = table.Column<int>(type: "int", nullable: true),
                    MaxCompanySize = table.Column<int>(type: "int", nullable: true),
                    MinRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssignmentRulesJson = table.Column<string>(type: "VARCHAR(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Territories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Territories_Territories_ParentTerritoryId",
                        column: x => x.ParentTerritoryId,
                        principalTable: "Territories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Territories_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrustedDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "VARCHAR(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceName = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "VARCHAR(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "VARCHAR(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUsedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FingerprintHash = table.Column<string>(type: "VARCHAR(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrustedDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpportunityCompetitors",
                columns: table => new
                {
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    CompetitorId = table.Column<int>(type: "int", nullable: false),
                    ThreatLevel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompetitorPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IdentifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WonAgainst = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CompetitorId1 = table.Column<int>(type: "int", nullable: true),
                    OpportunityId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityCompetitors", x => new { x.OpportunityId, x.CompetitorId });
                    table.ForeignKey(
                        name: "FK_OpportunityCompetitors_Competitors_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityCompetitors_Competitors_CompetitorId1",
                        column: x => x.CompetitorId1,
                        principalTable: "Competitors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityCompetitors_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityCompetitors_Opportunities_OpportunityId1",
                        column: x => x.OpportunityId1,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebToLeadForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldsJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetLeadSourceId = table.Column<int>(type: "int", nullable: true),
                    CaptchaEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotifyEmail = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotifyEmails = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RedirectUrl = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThankYouMessage = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmbedKey = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultOwnerId = table.Column<int>(type: "int", nullable: true),
                    CustomStyling = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmissionCount = table.Column<int>(type: "int", nullable: false),
                    LastSubmissionAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebToLeadForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebToLeadForms_LeadSourceConfigs_TargetLeadSourceId",
                        column: x => x.TargetLeadSourceId,
                        principalTable: "LeadSourceConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebToLeadForms_Users_DefaultOwnerId",
                        column: x => x.DefaultOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveriesGeneral_ParentEventId",
                table: "WebhookDeliveriesGeneral",
                column: "ParentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_WebAuthnCredentials_UserId",
                table: "WebAuthnCredentials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CompetitorWinnerId",
                table: "Opportunities",
                column: "CompetitorWinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LeadSourceConfigId",
                table: "Leads",
                column: "LeadSourceConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LeadSourceId",
                table: "Leads",
                column: "LeadSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_NurtureCampaignId",
                table: "Leads",
                column: "NurtureCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_TerritoryId",
                table: "Leads",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVersions_ArticleId",
                table: "ArticleVersions",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVersions_ChangedById",
                table: "ArticleVersions",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContractVersions_ContractId",
                table: "ContractVersions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractVersions_CreatedById",
                table: "ContractVersions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthorizationCodes_AuthorizedUserId",
                table: "DeviceAuthorizationCodes",
                column: "AuthorizedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthorizationCodes_DeviceCode",
                table: "DeviceAuthorizationCodes",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthorizationCodes_ExpiresAt",
                table: "DeviceAuthorizationCodes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAuthorizationCodes_UserCode",
                table: "DeviceAuthorizationCodes",
                column: "UserCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLogs_EscalatedByUserId",
                table: "EscalationLogs",
                column: "EscalatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLogs_ServiceRequestId",
                table: "EscalationLogs",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadSourceConfigs_CampaignId",
                table: "LeadSourceConfigs",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_CreatedAt",
                table: "LoginAttempts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Email",
                table: "LoginAttempts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Email_Success_CreatedAt",
                table: "LoginAttempts",
                columns: new[] { "Email", "Success", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IpAddress",
                table: "LoginAttempts",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IpAddress_Success_CreatedAt",
                table: "LoginAttempts",
                columns: new[] { "IpAddress", "Success", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_UserId",
                table: "LoginAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationConfigs_ParentId",
                table: "NavigationConfigs",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCompetitors_CompetitorId",
                table: "OpportunityCompetitors",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCompetitors_CompetitorId1",
                table: "OpportunityCompetitors",
                column: "CompetitorId1");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCompetitors_OpportunityId1",
                table: "OpportunityCompetitors",
                column: "OpportunityId1");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityTeamMembers_CommissionPlanId",
                table: "OpportunityTeamMembers",
                column: "CommissionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityTeamMembers_OpportunityId",
                table: "OpportunityTeamMembers",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityTeamMembers_UserId",
                table: "OpportunityTeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturns_AccountId",
                table: "OrderReturns",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturns_InitiatedById",
                table: "OrderReturns",
                column: "InitiatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturns_OrderId",
                table: "OrderReturns",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturns_ProcessedById",
                table: "OrderReturns",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReportShares_ReportId",
                table: "ReportShares",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportShares_SharedByUserId",
                table: "ReportShares",
                column: "SharedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportShares_UserId",
                table: "ReportShares",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Territories_OwnerUserId",
                table: "Territories",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Territories_ParentTerritoryId",
                table: "Territories",
                column: "ParentTerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_UserId",
                table: "TrustedDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_UserId_DeviceId",
                table: "TrustedDevices",
                columns: new[] { "UserId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_UserId_ExpiresAt",
                table: "TrustedDevices",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WebToLeadForms_DefaultOwnerId",
                table: "WebToLeadForms",
                column: "DefaultOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_WebToLeadForms_TargetLeadSourceId",
                table: "WebToLeadForms",
                column: "TargetLeadSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_LeadSourceConfigs_LeadSourceConfigId",
                table: "Leads",
                column: "LeadSourceConfigId",
                principalTable: "LeadSourceConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_LeadSources_LeadSourceId",
                table: "Leads",
                column: "LeadSourceId",
                principalTable: "LeadSources",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_NurtureCampaignId",
                table: "Leads",
                column: "NurtureCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Territories_TerritoryId",
                table: "Leads",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Competitors_CompetitorWinnerId",
                table: "Opportunities",
                column: "CompetitorWinnerId",
                principalTable: "Competitors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionItems_Products_ProductId",
                table: "SubscriptionItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionUsages_SubscriptionItems_SubscriptionItemId",
                table: "SubscriptionUsages",
                column: "SubscriptionItemId",
                principalTable: "SubscriptionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WebAuthnCredentials_Users_UserId",
                table: "WebAuthnCredentials",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebhookDeliveriesGeneral_WebhookEvents_ParentEventId",
                table: "WebhookDeliveriesGeneral",
                column: "ParentEventId",
                principalTable: "WebhookEvents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_LeadSourceConfigs_LeadSourceConfigId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_LeadSources_LeadSourceId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_NurtureCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Territories_TerritoryId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Competitors_CompetitorWinnerId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionItems_Products_ProductId",
                table: "SubscriptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionUsages_SubscriptionItems_SubscriptionItemId",
                table: "SubscriptionUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_WebAuthnCredentials_Users_UserId",
                table: "WebAuthnCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_WebhookDeliveriesGeneral_WebhookEvents_ParentEventId",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropTable(
                name: "ArticleVersions");

            migrationBuilder.DropTable(
                name: "ContractVersions");

            migrationBuilder.DropTable(
                name: "DeviceAuthorizationCodes");

            migrationBuilder.DropTable(
                name: "EscalationLogs");

            migrationBuilder.DropTable(
                name: "LeadSources");

            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "NavigationConfigs");

            migrationBuilder.DropTable(
                name: "OpportunityCompetitors");

            migrationBuilder.DropTable(
                name: "OpportunityTeamMembers");

            migrationBuilder.DropTable(
                name: "OrderReturns");

            migrationBuilder.DropTable(
                name: "ReportShares");

            migrationBuilder.DropTable(
                name: "Territories");

            migrationBuilder.DropTable(
                name: "TrustedDevices");

            migrationBuilder.DropTable(
                name: "WebToLeadForms");

            migrationBuilder.DropTable(
                name: "Competitors");

            migrationBuilder.DropTable(
                name: "LeadSourceConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WebhookDeliveriesGeneral_ParentEventId",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropIndex(
                name: "IX_WebAuthnCredentials_UserId",
                table: "WebAuthnCredentials");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_CompetitorWinnerId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Leads_LeadSourceConfigId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_LeadSourceId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_NurtureCampaignId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_TerritoryId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ChainDepth",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropColumn(
                name: "ParentEventId",
                table: "WebhookDeliveriesGeneral");

            migrationBuilder.DropColumn(
                name: "ChunkNumber",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "ContinuationToken",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "ParentEventId",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "PayloadSizeBytes",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "TotalChunks",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "IsPlatformCredential",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "SignCount",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "IpBindingEnabled",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "BillingTimezone",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DunningAttemptCount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DunningGracePeriodDays",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DunningNotificationEmails",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastDunningDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ProrationType",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SendDunningEscalationEmails",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TrialEndDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TrialStartDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ExchangeOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsEligibleForExchange",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QualityInspectionNotes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RestockingFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnApprovedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnAuthorizationNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnProcessedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnReasonCategory",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnReceivedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnRequestedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnTrackingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CompetitorWinnerId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ForecastCategory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LossReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LossReasonCategory",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "WinLossNotes",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AuthorityScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BudgetScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ChampionScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CustomQualificationJson",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DecisionCriteriaScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DecisionProcessScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "EconomicBuyerScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FirstTouchDate",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IdentifyPainScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LastContactedAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadSourceConfigId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadSourceId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MetricsScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "NeedScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "NurtureCampaignEnrolledAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "NurtureCampaignId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "OriginalSource",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "QualificationFrameworkType",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TerritoryId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TimelineScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmCampaign",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmMedium",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UtmSource",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AllowStacking",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "CommissionPlanId",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "MaxCommissionAmount",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "MinDealSize",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "SplitPercentage",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "TriggerEvent",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UsageType",
                table: "SubscriptionUsages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SubscriptionUsages",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "MetricName",
                table: "SubscriptionUsages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionUsages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Limit",
                table: "SubscriptionUsageLimits",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "OneTimeFee",
                table: "Subscriptions",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MRR",
                table: "Subscriptions",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Subscriptions",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "ARR",
                table: "Subscriptions",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "SubscriptionItems",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SubscriptionItems",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "SubscriptionItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "SubscriptionItems",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnReason",
                table: "Orders",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionItems_Products_ProductId",
                table: "SubscriptionItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionUsages_SubscriptionItems_SubscriptionItemId",
                table: "SubscriptionUsages",
                column: "SubscriptionItemId",
                principalTable: "SubscriptionItems",
                principalColumn: "Id");
        }
    }
}
