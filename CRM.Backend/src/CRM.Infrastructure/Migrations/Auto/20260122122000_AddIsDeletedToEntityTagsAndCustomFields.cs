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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    public partial class AddIsDeletedToEntityTagsAndCustomFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IsDeleted and UpdatedAt to EntityTags (table created earlier without BaseEntity columns)
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EntityTags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EntityTags",
                type: "datetime(6)",
                nullable: true);

            // Add IsDeleted and UpdatedAt to CustomFields as well
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomFields",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomFields",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsDeleted", table: "EntityTags");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "EntityTags");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "CustomFields");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "CustomFields");
        }
    }
}
