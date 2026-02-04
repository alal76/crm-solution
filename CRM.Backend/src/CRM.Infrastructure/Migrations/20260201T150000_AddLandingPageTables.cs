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
/// Migration to add Landing Page tables (LandingPages, LandingPageBlocks, LandingPageVisits).
/// Part of Marketing and Sales gap analysis implementation (G6).
/// </summary>
public partial class AddLandingPageTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create LandingPages table
        migrationBuilder.CreateTable(
            name: "LandingPages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                MetaDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                MetaKeywords = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Template = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                ContentJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                HtmlContent = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CustomCss = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CustomJs = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FeaturedImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FacebookPixelId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                GoogleAnalyticsId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TrackingCode = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FormDefinitionId = table.Column<int>(type: "int", nullable: true),
                CampaignId = table.Column<int>(type: "int", nullable: true),
                ThankYouPageId = table.Column<int>(type: "int", nullable: true),
                RedirectUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ScheduledPublishAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ScheduledUnpublishAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                ABTestVariant = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OriginalPageId = table.Column<int>(type: "int", nullable: true),
                ABTestTrafficPercentage = table.Column<int>(type: "int", nullable: true),
                PageViews = table.Column<int>(type: "int", nullable: false),
                UniqueVisitors = table.Column<int>(type: "int", nullable: false),
                Conversions = table.Column<int>(type: "int", nullable: false),
                AverageTimeOnPage = table.Column<double>(type: "double", nullable: false),
                BounceRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                SettingsJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LandingPages", x => x.Id);
                table.ForeignKey(
                    name: "FK_LandingPages_FormDefinitions_FormDefinitionId",
                    column: x => x.FormDefinitionId,
                    principalTable: "FormDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_LandingPages_MarketingCampaigns_CampaignId",
                    column: x => x.CampaignId,
                    principalTable: "MarketingCampaigns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_LandingPages_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Create LandingPageBlocks table
        migrationBuilder.CreateTable(
            name: "LandingPageBlocks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                LandingPageId = table.Column<int>(type: "int", nullable: false),
                BlockType = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                ContentJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StyleJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                VisibilityCondition = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LandingPageBlocks", x => x.Id);
                table.ForeignKey(
                    name: "FK_LandingPageBlocks_LandingPages_LandingPageId",
                    column: x => x.LandingPageId,
                    principalTable: "LandingPages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Create LandingPageVisits table
        migrationBuilder.CreateTable(
            name: "LandingPageVisits",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                LandingPageId = table.Column<int>(type: "int", nullable: false),
                VisitorId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IpAddressHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UserAgent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Referrer = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UtmSource = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UtmMedium = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UtmCampaign = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UtmTerm = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UtmContent = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                VisitedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                TimeOnPageSeconds = table.Column<int>(type: "int", nullable: true),
                Converted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                ConvertedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                LeadId = table.Column<int>(type: "int", nullable: true),
                DeviceType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Browser = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OperatingSystem = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LandingPageVisits", x => x.Id);
                table.ForeignKey(
                    name: "FK_LandingPageVisits_LandingPages_LandingPageId",
                    column: x => x.LandingPageId,
                    principalTable: "LandingPages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        // Indexes for LandingPages
        migrationBuilder.CreateIndex(
            name: "IX_LandingPages_Slug",
            table: "LandingPages",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LandingPages_Status",
            table: "LandingPages",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_LandingPages_Status_IsActive",
            table: "LandingPages",
            columns: new[] { "Status", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_LandingPages_FormDefinitionId",
            table: "LandingPages",
            column: "FormDefinitionId");

        migrationBuilder.CreateIndex(
            name: "IX_LandingPages_CampaignId",
            table: "LandingPages",
            column: "CampaignId");

        migrationBuilder.CreateIndex(
            name: "IX_LandingPages_CreatedByUserId",
            table: "LandingPages",
            column: "CreatedByUserId");

        // Indexes for LandingPageBlocks
        migrationBuilder.CreateIndex(
            name: "IX_LandingPageBlocks_LandingPageId_SortOrder",
            table: "LandingPageBlocks",
            columns: new[] { "LandingPageId", "SortOrder" });

        // Indexes for LandingPageVisits
        migrationBuilder.CreateIndex(
            name: "IX_LandingPageVisits_LandingPageId",
            table: "LandingPageVisits",
            column: "LandingPageId");

        migrationBuilder.CreateIndex(
            name: "IX_LandingPageVisits_VisitedAt",
            table: "LandingPageVisits",
            column: "VisitedAt");

        migrationBuilder.CreateIndex(
            name: "IX_LandingPageVisits_LandingPageId_VisitorId",
            table: "LandingPageVisits",
            columns: new[] { "LandingPageId", "VisitorId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LandingPageVisits");
        migrationBuilder.DropTable(name: "LandingPageBlocks");
        migrationBuilder.DropTable(name: "LandingPages");
    }
}
