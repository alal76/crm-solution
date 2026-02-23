using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionRenewalBillingHistoryDunningRecordTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create SubscriptionRenewals table
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
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionRenewals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionRenewals_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionRenewals_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_SubscriptionId",
                table: "SubscriptionRenewals",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_RenewalDate",
                table: "SubscriptionRenewals",
                column: "RenewalDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_Status",
                table: "SubscriptionRenewals",
                column: "Status");

            // Create BillingHistory table
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
                    Status = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false, defaultValue: "Pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BilledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DunningRecordId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingHistory_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillingHistory_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BillingHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_SubscriptionId",
                table: "BillingHistory",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_EventType",
                table: "BillingHistory",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_EventDate",
                table: "BillingHistory",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_InvoiceId",
                table: "BillingHistory",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_UserId",
                table: "BillingHistory",
                column: "UserId");

            // Create DunningRecords table (after BillingHistory so FK can reference it)
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
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DunningRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DunningRecords_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DunningRecords_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DunningRecords_BillingHistory_BillingHistoryId",
                        column: x => x.BillingHistoryId,
                        principalTable: "BillingHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_SubscriptionId",
                table: "DunningRecords",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_Status",
                table: "DunningRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_NextRetryDate",
                table: "DunningRecords",
                column: "NextRetryDate");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_InvoiceId",
                table: "DunningRecords",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DunningRecords_BillingHistoryId",
                table: "DunningRecords",
                column: "BillingHistoryId");

            // Back-fill FK: BillingHistory.DunningRecordId -> DunningRecords
            migrationBuilder.AddForeignKey(
                name: "FK_BillingHistory_DunningRecords_DunningRecordId",
                table: "BillingHistory",
                column: "DunningRecordId",
                principalTable: "DunningRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_BillingHistory_DunningRecordId",
                table: "BillingHistory",
                column: "DunningRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove back-fill FK first to avoid dependency issue on drop
            migrationBuilder.DropForeignKey(
                name: "FK_BillingHistory_DunningRecords_DunningRecordId",
                table: "BillingHistory");

            migrationBuilder.DropTable(name: "SubscriptionRenewals");
            migrationBuilder.DropTable(name: "DunningRecords");
            migrationBuilder.DropTable(name: "BillingHistory");
        }
    }
}
