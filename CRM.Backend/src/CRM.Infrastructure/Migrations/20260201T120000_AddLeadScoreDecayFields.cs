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
/// Migration to add Lead Score Decay fields (LastScoreDecayDate, LastActivityDate).
/// Part of Marketing &amp; Sales gap analysis implementation (G7).
/// </summary>
public partial class AddLeadScoreDecayFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "LastScoreDecayDate",
            table: "Leads",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastActivityDate",
            table: "Leads",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Leads_LastActivityDate",
            table: "Leads",
            column: "LastActivityDate");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Leads_LastActivityDate",
            table: "Leads");

        migrationBuilder.DropColumn(
            name: "LastActivityDate",
            table: "Leads");

        migrationBuilder.DropColumn(
            name: "LastScoreDecayDate",
            table: "Leads");
    }
}
