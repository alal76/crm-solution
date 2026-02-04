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

﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDemoDbWithSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old demo database columns
            migrationBuilder.DropColumn(
                name: "UseDemoDatabase",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DemoDataSeeded",
                table: "SystemSettings");

            // Rename DemoDataLastSeeded to SampleDataLastSeeded
            migrationBuilder.RenameColumn(
                name: "DemoDataLastSeeded",
                table: "SystemSettings",
                newName: "SampleDataLastSeeded");

            // Add new SampleDataSeeded column
            migrationBuilder.AddColumn<bool>(
                name: "SampleDataSeeded",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop SampleDataSeeded column
            migrationBuilder.DropColumn(
                name: "SampleDataSeeded",
                table: "SystemSettings");

            // Rename SampleDataLastSeeded back to DemoDataLastSeeded
            migrationBuilder.RenameColumn(
                name: "SampleDataLastSeeded",
                table: "SystemSettings",
                newName: "DemoDataLastSeeded");

            // Add back the old demo database columns
            migrationBuilder.AddColumn<bool>(
                name: "UseDemoDatabase",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DemoDataSeeded",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
