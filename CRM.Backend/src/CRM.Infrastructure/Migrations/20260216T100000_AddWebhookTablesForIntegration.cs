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
/// Migration to add Webhook Integration Tables for Sprint 1
/// 
/// Creates:
/// - WebhookSubscriptions: Webhook subscription management
/// - WebhookDeliveries: Webhook delivery attempt tracking
///
/// Includes proper indexes and constraints for performance and data integrity.
/// Supports: MariaDB, SQL Server, PostgreSQL
///
/// Created: February 16, 2026
/// </summary>
public partial class AddWebhookTablesForIntegration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Webhook Subscription Table
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "WebhookSubscriptions",
            columns: table => new
            {
                WebhookSubscriptionId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TargetUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Secret = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                EventTypes = table.Column<string>(type: "longtext", nullable: false, defaultValue: "[]")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Headers = table.Column<string>(type: "longtext", nullable: false, defaultValue: "{}")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                TimeoutSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                LastTriggeredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                SuccessCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                FailureCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WebhookSubscriptions", x => x.WebhookSubscriptionId);
                table.ForeignKey(
                    name: "FK_WebhookSubscriptions_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Create indexes for WebhookSubscriptions
        migrationBuilder.CreateIndex(
            name: "IX_WebhookSubscriptions_IsActive",
            table: "WebhookSubscriptions",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_WebhookSubscriptions_LastTriggeredAt",
            table: "WebhookSubscriptions",
            column: "LastTriggeredAt");

        migrationBuilder.CreateIndex(
            name: "IX_WebhookSubscriptions_CreatedByUserId",
            table: "WebhookSubscriptions",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_WebhookSubscriptions_IsDeleted",
            table: "WebhookSubscriptions",
            column: "IsDeleted");

        // =====================================================================
        // Webhook Delivery Log Table
        // =====================================================================
        migrationBuilder.CreateTable(
            name: "WebhookDeliveries",
            columns: table => new
            {
                WebhookDeliveryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                WebhookSubscriptionId = table.Column<int>(type: "int", nullable: false),
                EventType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TargetUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RequestBody = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                ResponseBody = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Success = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                AttemptNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                DurationMs = table.Column<double>(type: "double", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
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
            });

        // Create indexes for WebhookDeliveries
        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_WebhookSubscriptionId",
            table: "WebhookDeliveries",
            column: "WebhookSubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_WebhookSubscriptionId_Success",
            table: "WebhookDeliveries",
            columns: new[] { "WebhookSubscriptionId", "Success" });

        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_Success_CreatedAt",
            table: "WebhookDeliveries",
            columns: new[] { "Success", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_EventType",
            table: "WebhookDeliveries",
            column: "EventType");

        migrationBuilder.CreateIndex(
            name: "IX_WebhookDeliveries_IsDeleted",
            table: "WebhookDeliveries",
            column: "IsDeleted");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop WebhookDeliveries table and indexes
        migrationBuilder.DropTable(
            name: "WebhookDeliveries");

        // Drop WebhookSubscriptions table and indexes
        migrationBuilder.DropTable(
            name: "WebhookSubscriptions");
    }
}
