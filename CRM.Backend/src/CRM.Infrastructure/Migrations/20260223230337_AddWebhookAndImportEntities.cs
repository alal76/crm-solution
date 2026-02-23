using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookAndImportEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportErrors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImportJobId = table.Column<int>(type: "int", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    Field = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawValue = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false, defaultValue: "Error")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportErrors_ImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalTable: "ImportJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImportMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MappingDefinition = table.Column<string>(type: "VARCHAR(4000)", maxLength: 4000, nullable: false, defaultValue: "[]")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportMappings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionRenewals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    BillingPeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BillingPeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionRenewals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionRenewals_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SubscriptionRenewals_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebhookEndpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Secret = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    EventTypes = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: false, defaultValue: "[]")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Headers = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    Description = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConsecutiveFailures = table.Column<int>(type: "int", nullable: false),
                    LastSuccessAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastFailureAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DisabledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DisabledReason = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEndpoints", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebhookEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TriggeredByUserId = table.Column<int>(type: "int", nullable: true),
                    CorrelationId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentEventId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEvents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebhookDeliveriesGeneral",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WebhookEndpointId = table.Column<int>(type: "int", nullable: false),
                    WebhookEventId = table.Column<int>(type: "int", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false, defaultValue: "Pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveriesGeneral", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveriesGeneral_WebhookEndpoints_WebhookEndpointId",
                        column: x => x.WebhookEndpointId,
                        principalTable: "WebhookEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveriesGeneral_WebhookEvents_WebhookEventId",
                        column: x => x.WebhookEventId,
                        principalTable: "WebhookEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BillingHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    CycleStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CycleEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    ProratedAmount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    UsageCharges = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    EventDetails = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BilledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DunningRecordId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingHistory_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BillingHistory_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillingHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DunningRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    RetryAttempt = table.Column<int>(type: "int", nullable: false),
                    NextRetryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastErrorMessage = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InitialFailureDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NotificationEmail = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsExhausted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    GracePeriodEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OutstandingAmount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    RecoveredAmount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    Notes = table.Column<string>(type: "VARCHAR(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingHistoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DunningRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DunningRecords_BillingHistory_BillingHistoryId",
                        column: x => x.BillingHistoryId,
                        principalTable: "BillingHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DunningRecords_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DunningRecords_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_DunningRecordId",
                table: "BillingHistory",
                column: "DunningRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_EventDate",
                table: "BillingHistory",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_EventType",
                table: "BillingHistory",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_InvoiceId",
                table: "BillingHistory",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_SubscriptionId",
                table: "BillingHistory",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_UserId",
                table: "BillingHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_BillingHistoryId",
                table: "DunningRecords",
                column: "BillingHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_InvoiceId",
                table: "DunningRecords",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_NextRetryDate",
                table: "DunningRecords",
                column: "NextRetryDate");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_Status",
                table: "DunningRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_SubscriptionId",
                table: "DunningRecords",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportErrors_ImportJobId",
                table: "ImportErrors",
                column: "ImportJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportErrors_Severity",
                table: "ImportErrors",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ImportMappings_CreatedByUserId",
                table: "ImportMappings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportMappings_EntityType",
                table: "ImportMappings",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_InvoiceId",
                table: "SubscriptionRenewals",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_RenewalDate",
                table: "SubscriptionRenewals",
                column: "RenewalDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_Status",
                table: "SubscriptionRenewals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_SubscriptionId",
                table: "SubscriptionRenewals",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveriesGeneral_NextRetryAt",
                table: "WebhookDeliveriesGeneral",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveriesGeneral_Status",
                table: "WebhookDeliveriesGeneral",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveriesGeneral_WebhookEndpointId",
                table: "WebhookDeliveriesGeneral",
                column: "WebhookEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveriesGeneral_WebhookEventId",
                table: "WebhookDeliveriesGeneral",
                column: "WebhookEventId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEndpoints_CreatedByUserId",
                table: "WebhookEndpoints",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEndpoints_IsActive",
                table: "WebhookEndpoints",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_CorrelationId",
                table: "WebhookEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_EntityType",
                table: "WebhookEvents",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_EventType",
                table: "WebhookEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_OccurredAt",
                table: "WebhookEvents",
                column: "OccurredAt");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingHistory_DunningRecords_DunningRecordId",
                table: "BillingHistory",
                column: "DunningRecordId",
                principalTable: "DunningRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingHistory_DunningRecords_DunningRecordId",
                table: "BillingHistory");

            migrationBuilder.DropTable(
                name: "ImportErrors");

            migrationBuilder.DropTable(
                name: "ImportMappings");

            migrationBuilder.DropTable(
                name: "SubscriptionRenewals");

            migrationBuilder.DropTable(
                name: "WebhookDeliveriesGeneral");

            migrationBuilder.DropTable(
                name: "WebhookEndpoints");

            migrationBuilder.DropTable(
                name: "WebhookEvents");

            migrationBuilder.DropTable(
                name: "DunningRecords");

            migrationBuilder.DropTable(
                name: "BillingHistory");
        }
    }
}
