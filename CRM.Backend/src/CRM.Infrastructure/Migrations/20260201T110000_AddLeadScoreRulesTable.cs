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
/// Migration to add LeadScoreRules table for configurable lead scoring.
/// Part of Marketing & Sales gap analysis implementation (G2).
/// </summary>
public partial class AddLeadScoreRulesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LeadScoreRules",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                RuleType = table.Column<int>(type: "int", nullable: false),
                FieldName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Operator = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ConditionsJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ScoreImpact = table.Column<int>(type: "int", nullable: false),
                MaxApplications = table.Column<int>(type: "int", nullable: true),
                DecayDaysThreshold = table.Column<int>(type: "int", nullable: true),
                DecayPointsPerPeriod = table.Column<int>(type: "int", nullable: true),
                DecayPeriodDays = table.Column<int>(type: "int", nullable: true),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                Priority = table.Column<int>(type: "int", nullable: false),
                Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ActionType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ActionIdentifier = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeadScoreRules", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_LeadScoreRules_IsActive",
            table: "LeadScoreRules",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_LeadScoreRules_RuleType",
            table: "LeadScoreRules",
            column: "RuleType");

        migrationBuilder.CreateIndex(
            name: "IX_LeadScoreRules_Category",
            table: "LeadScoreRules",
            column: "Category");

        // Seed default scoring rules
        var seedValues = new object[,]
        {
            { "C-Level Executive", "Bonus points for C-Level executives", 0, "JobTitle", 2, "CEO,CFO,CTO,CIO,COO,CMO", 30, true, 10, "Demographics", DateTime.UtcNow, false },
            { "Director Level", "Bonus points for Director titles", 0, "JobTitle", 2, "Director,VP,Vice President", 20, true, 20, "Demographics", DateTime.UtcNow, false },
            { "Manager Level", "Bonus points for Manager titles", 0, "JobTitle", 2, "Manager,Head of", 10, true, 30, "Demographics", DateTime.UtcNow, false },
            { "Technology Industry", "Bonus for tech industry leads", 0, "Industry", 2, "Technology,Software,SaaS", 15, true, 40, "Demographics", DateTime.UtcNow, false },
            { "Enterprise Company", "Bonus for large company size", 0, "CompanySize", 4, "1000", 20, true, 50, "Demographics", DateTime.UtcNow, false },
            { "Email Opened", "Points for opening marketing emails", 1, null, 0, null, 5, true, 100, "Engagement", DateTime.UtcNow, false },
            { "Email Clicked", "Points for clicking email links", 1, null, 0, null, 10, true, 110, "Engagement", DateTime.UtcNow, false },
            { "Form Submitted", "Points for submitting contact forms", 1, null, 0, null, 20, true, 120, "Engagement", DateTime.UtcNow, false },
            { "Inactivity Decay", "Reduce score for inactive leads", 2, null, 0, null, -5, true, 200, "Decay", DateTime.UtcNow, false }
        };

        migrationBuilder.InsertData(
            table: "LeadScoreRules",
            columns: new[] { "Name", "Description", "RuleType", "FieldName", "Operator", "Value", "ScoreImpact", "IsActive", "Priority", "Category", "CreatedAt", "IsDeleted" },
            values: seedValues);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LeadScoreRules");
    }
}
