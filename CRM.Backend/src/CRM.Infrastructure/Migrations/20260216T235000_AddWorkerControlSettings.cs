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

public partial class AddWorkerControlSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "WorkerControlState",
            table: "SystemSettings",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Running");

        migrationBuilder.AddColumn<int>(
            name: "WorkerMaxInstances",
            table: "SystemSettings",
            type: "int",
            nullable: false,
            defaultValue: 1);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "WorkerControlState",
            table: "SystemSettings");

        migrationBuilder.DropColumn(
            name: "WorkerMaxInstances",
            table: "SystemSettings");
    }
}
