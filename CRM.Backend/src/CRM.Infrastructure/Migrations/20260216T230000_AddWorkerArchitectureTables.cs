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

public partial class AddWorkerArchitectureTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WorkerJobs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                JobType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkerJobs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OutboxEvents",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxEvents", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "WorkerExecutions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                WorkerJobId = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NodeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkerExecutions", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkerExecutions_WorkerJobs_WorkerJobId",
                    column: x => x.WorkerJobId,
                    principalTable: "WorkerJobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkerJobs_Status_NextAttemptAt",
            table: "WorkerJobs",
            columns: new[] { "Status", "NextAttemptAt" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkerJobs_JobType",
            table: "WorkerJobs",
            column: "JobType");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvents_Status",
            table: "OutboxEvents",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxEvents_OccurredAt",
            table: "OutboxEvents",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_WorkerExecutions_WorkerJobId",
            table: "WorkerExecutions",
            column: "WorkerJobId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkerExecutions");

        migrationBuilder.DropTable(
            name: "OutboxEvents");

        migrationBuilder.DropTable(
            name: "WorkerJobs");
    }
}
