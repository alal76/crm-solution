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
/// Migration to add Calendar Integration tables (CalendarIntegrations, CalendarSyncLogs, CalendarEventMappings).
/// Part of Marketing & Sales gap analysis implementation (G4).
/// </summary>
public partial class AddCalendarIntegrationTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create CalendarIntegrations table
        migrationBuilder.CreateTable(
            name: "CalendarIntegrations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserId = table.Column<int>(type: "int", nullable: false),
                Provider = table.Column<int>(type: "int", nullable: false),
                AccessToken = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RefreshToken = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TokenExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CalendarId = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CalendarName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExternalEmail = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SyncDirection = table.Column<int>(type: "int", nullable: false),
                LastSyncAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                LastSyncStatus = table.Column<int>(type: "int", nullable: false),
                LastSyncError = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                NextSyncAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                SyncIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                SyncToken = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                LastSyncEventsCount = table.Column<int>(type: "int", nullable: true),
                TotalEventsSynced = table.Column<int>(type: "int", nullable: false),
                SettingsJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendarIntegrations", x => x.Id);
                table.ForeignKey(
                    name: "FK_CalendarIntegrations_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Create CalendarSyncLogs table
        migrationBuilder.CreateTable(
            name: "CalendarSyncLogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CalendarIntegrationId = table.Column<int>(type: "int", nullable: false),
                StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                EventsCreated = table.Column<int>(type: "int", nullable: false),
                EventsUpdated = table.Column<int>(type: "int", nullable: false),
                EventsDeleted = table.Column<int>(type: "int", nullable: false),
                ConflictsResolved = table.Column<int>(type: "int", nullable: false),
                ErrorMessage = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ErrorStackTrace = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Direction = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendarSyncLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_CalendarSyncLogs_CalendarIntegrations_CalendarIntegrationId",
                    column: x => x.CalendarIntegrationId,
                    principalTable: "CalendarIntegrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Create CalendarEventMappings table
        migrationBuilder.CreateTable(
            name: "CalendarEventMappings",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ActivityId = table.Column<int>(type: "int", nullable: false),
                CalendarIntegrationId = table.Column<int>(type: "int", nullable: false),
                ExternalEventId = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExternalEventUid = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExternalETag = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                LastSyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ExternalLastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CrmLastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedFromExternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendarEventMappings", x => x.Id);
                table.ForeignKey(
                    name: "FK_CalendarEventMappings_Activities_ActivityId",
                    column: x => x.ActivityId,
                    principalTable: "Activities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CalendarEventMappings_CalendarIntegrations_CalendarIntegrationId",
                    column: x => x.CalendarIntegrationId,
                    principalTable: "CalendarIntegrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Indexes for CalendarIntegrations
        migrationBuilder.CreateIndex(
            name: "IX_CalendarIntegrations_UserId",
            table: "CalendarIntegrations",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_CalendarIntegrations_IsActive_NextSyncAt",
            table: "CalendarIntegrations",
            columns: new[] { "IsActive", "NextSyncAt" });

        migrationBuilder.CreateIndex(
            name: "IX_CalendarIntegrations_UserId_Provider",
            table: "CalendarIntegrations",
            columns: new[] { "UserId", "Provider" },
            unique: true);

        // Indexes for CalendarSyncLogs
        migrationBuilder.CreateIndex(
            name: "IX_CalendarSyncLogs_CalendarIntegrationId",
            table: "CalendarSyncLogs",
            column: "CalendarIntegrationId");

        // Indexes for CalendarEventMappings
        migrationBuilder.CreateIndex(
            name: "IX_CalendarEventMappings_ActivityId",
            table: "CalendarEventMappings",
            column: "ActivityId");

        migrationBuilder.CreateIndex(
            name: "IX_CalendarEventMappings_CalendarIntegrationId_ExternalEventId",
            table: "CalendarEventMappings",
            columns: new[] { "CalendarIntegrationId", "ExternalEventId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CalendarEventMappings");
        migrationBuilder.DropTable(name: "CalendarSyncLogs");
        migrationBuilder.DropTable(name: "CalendarIntegrations");
    }
}
