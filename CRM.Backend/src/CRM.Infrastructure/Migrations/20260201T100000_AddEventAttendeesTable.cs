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
/// Migration to add EventAttendees table for tracking meeting/event participants.
/// Part of Marketing & Sales gap analysis implementation (G1).
/// </summary>
public partial class AddEventAttendeesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EventAttendees",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ActivityId = table.Column<int>(type: "int", nullable: false),
                AttendeeType = table.Column<int>(type: "int", nullable: false),
                AttendeeId = table.Column<int>(type: "int", nullable: false),
                ResponseStatus = table.Column<int>(type: "int", nullable: false),
                RespondedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsOrganizer = table.Column<bool>(type: "tinyint(1)", nullable: false),
                IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                Role = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                DidAttend = table.Column<bool>(type: "tinyint(1)", nullable: true),
                ExternalCalendarEventId = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventAttendees", x => x.Id);
                table.ForeignKey(
                    name: "FK_EventAttendees_Activities_ActivityId",
                    column: x => x.ActivityId,
                    principalTable: "Activities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_EventAttendees_ActivityId",
            table: "EventAttendees",
            column: "ActivityId");

        migrationBuilder.CreateIndex(
            name: "IX_EventAttendees_AttendeeType_AttendeeId",
            table: "EventAttendees",
            columns: new[] { "AttendeeType", "AttendeeId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EventAttendees");
    }
}
