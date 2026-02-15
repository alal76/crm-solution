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
/// Migration for SPEC-SALES-006: Subscription Management
/// Adds BillingHistory and DunningRecord entities for recurring billing support.
/// </summary>
public partial class AddSubscriptionBillingEntities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create BillingHistory table
        migrationBuilder.CreateTable(
            name: "BillingHistory",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SubscriptionId = table.Column<int>(type: "int", nullable: false),
                InvoiceId = table.Column<int>(type: "int", nullable: true),
                CycleStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CycleEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                ProratedAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                UsageCharges = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                TaxAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                EventType = table.Column<int>(type: "int", nullable: false),
                EventDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                UserId = table.Column<int>(type: "int", nullable: true),
                EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                BilledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DunningRecordId = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
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
            });

        // Create DunningRecords table
        migrationBuilder.CreateTable(
            name: "DunningRecords",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SubscriptionId = table.Column<int>(type: "int", nullable: false),
                InvoiceId = table.Column<int>(type: "int", nullable: false),
                RetryAttempt = table.Column<int>(type: "int", nullable: false),
                NextRetryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                LastErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                InitialFailureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                NotificationEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                IsExhausted = table.Column<bool>(type: "bit", nullable: false),
                CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                GracePeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                OutstandingAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                RecoveredAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                BillingHistoryId = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
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
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DunningRecords_Subscriptions_SubscriptionId",
                    column: x => x.SubscriptionId,
                    principalTable: "Subscriptions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Create indexes for performance
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_SubscriptionId_CycleEndDate",
            table: "BillingHistory",
            columns: new[] { "SubscriptionId", "CycleEndDate" },
            descending: new [] { false, true });

        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_Status_EventDate",
            table: "BillingHistory",
            columns: new[] { "Status", "EventDate" });

        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_InvoiceId",
            table: "BillingHistory",
            column: "InvoiceId");

        migrationBuilder.CreateIndex(
            name: "IX_DunningRecords_SubscriptionId",
            table: "DunningRecords",
            column: "SubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_DunningRecords_Status_NextRetryDate",
            table: "DunningRecords",
            columns: new[] { "Status", "NextRetryDate" });

        migrationBuilder.CreateIndex(
            name: "IX_DunningRecords_InvoiceId",
            table: "DunningRecords",
            column: "InvoiceId");

        migrationBuilder.CreateIndex(
            name: "IX_DunningRecords_IsExhausted",
            table: "DunningRecords",
            column: "IsExhausted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DunningRecords");
        migrationBuilder.DropTable(name: "BillingHistory");
    }
}
