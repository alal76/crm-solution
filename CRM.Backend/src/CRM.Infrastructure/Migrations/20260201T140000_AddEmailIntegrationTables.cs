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
/// Migration to add Email Integration tables (EmailIntegrations, EmailSyncLogs, EmailMessageMappings).
/// Part of Marketing & Sales gap analysis implementation (G5).
/// </summary>
public partial class AddEmailIntegrationTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create EmailIntegrations table
        migrationBuilder.CreateTable(
            name: "EmailIntegrations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserId = table.Column<int>(type: "int", nullable: false),
                Provider = table.Column<int>(type: "int", nullable: false),
                EmailAddress = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                DisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                AccessToken = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RefreshToken = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TokenExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ImapHost = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImapPort = table.Column<int>(type: "int", nullable: true),
                ImapUseSsl = table.Column<bool>(type: "tinyint(1)", nullable: true),
                SmtpHost = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SmtpPort = table.Column<int>(type: "int", nullable: true),
                SmtpUseSsl = table.Column<bool>(type: "tinyint(1)", nullable: true),
                EncryptedPassword = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FoldersToSync = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SyncDaysBack = table.Column<int>(type: "int", nullable: false),
                LastSyncAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                LastSyncStatus = table.Column<int>(type: "int", nullable: false),
                LastSyncError = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                NextSyncAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                SyncIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                ImapUidValidity = table.Column<long>(type: "bigint", nullable: true),
                ImapHighestModSeq = table.Column<long>(type: "bigint", nullable: true),
                LastSyncedUid = table.Column<long>(type: "bigint", nullable: true),
                LastSyncMessagesCount = table.Column<int>(type: "int", nullable: true),
                TotalMessagesSynced = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_EmailIntegrations", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmailIntegrations_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Create EmailSyncLogs table
        migrationBuilder.CreateTable(
            name: "EmailSyncLogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                EmailIntegrationId = table.Column<int>(type: "int", nullable: false),
                StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                MessagesDownloaded = table.Column<int>(type: "int", nullable: false),
                MessagesSent = table.Column<int>(type: "int", nullable: false),
                MessagesDeleted = table.Column<int>(type: "int", nullable: false),
                AttachmentsDownloaded = table.Column<int>(type: "int", nullable: false),
                ErrorMessage = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ErrorStackTrace = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FoldersSynced = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailSyncLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmailSyncLogs_EmailIntegrations_EmailIntegrationId",
                    column: x => x.EmailIntegrationId,
                    principalTable: "EmailIntegrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Create EmailMessageMappings table
        migrationBuilder.CreateTable(
            name: "EmailMessageMappings",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CommunicationMessageId = table.Column<int>(type: "int", nullable: false),
                EmailIntegrationId = table.Column<int>(type: "int", nullable: false),
                ExternalMessageId = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ImapUid = table.Column<long>(type: "bigint", nullable: true),
                FolderName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ThreadId = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ConversationId = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                LastSyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ExternalLastModified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                Flags = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                IsStarred = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedFromExternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailMessageMappings", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmailMessageMappings_CommunicationMessages_CommunicationMessageId",
                    column: x => x.CommunicationMessageId,
                    principalTable: "CommunicationMessages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EmailMessageMappings_EmailIntegrations_EmailIntegrationId",
                    column: x => x.EmailIntegrationId,
                    principalTable: "EmailIntegrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Indexes for EmailIntegrations
        migrationBuilder.CreateIndex(
            name: "IX_EmailIntegrations_UserId",
            table: "EmailIntegrations",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailIntegrations_IsActive_NextSyncAt",
            table: "EmailIntegrations",
            columns: new[] { "IsActive", "NextSyncAt" });

        migrationBuilder.CreateIndex(
            name: "IX_EmailIntegrations_UserId_Provider_EmailAddress",
            table: "EmailIntegrations",
            columns: new[] { "UserId", "Provider", "EmailAddress" },
            unique: true);

        // Indexes for EmailSyncLogs
        migrationBuilder.CreateIndex(
            name: "IX_EmailSyncLogs_EmailIntegrationId",
            table: "EmailSyncLogs",
            column: "EmailIntegrationId");

        // Indexes for EmailMessageMappings
        migrationBuilder.CreateIndex(
            name: "IX_EmailMessageMappings_CommunicationMessageId",
            table: "EmailMessageMappings",
            column: "CommunicationMessageId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailMessageMappings_EmailIntegrationId_ExternalMessageId",
            table: "EmailMessageMappings",
            columns: new[] { "EmailIntegrationId", "ExternalMessageId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_EmailMessageMappings_EmailIntegrationId_ImapUid",
            table: "EmailMessageMappings",
            columns: new[] { "EmailIntegrationId", "ImapUid" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EmailMessageMappings");
        migrationBuilder.DropTable(name: "EmailSyncLogs");
        migrationBuilder.DropTable(name: "EmailIntegrations");
    }
}
