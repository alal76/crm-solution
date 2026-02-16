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
/// Migration for Phase 3: Problem Management - Performance Indexes
/// 
/// Adds critical indexes to Problem Management entities to optimize:
/// - Problem workflow queries (status filtering, sorting by creation date)
/// - Reverse lookups for incident-to-problem relationships
/// 
/// Tables affected:
/// - Problems: Add IX_Problem_Status, IX_Problem_CreatedAt
/// - ProblemIncidents: Add IX_ProblemIncident_IncidentId (reverse lookup)
/// </summary>
public partial class Add_ProblemManagementIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Problem Management Indexes for Workflow Queries
        // =====================================================================

        // IX_Problem_Status: For filtering problems by workflow state (New, Assigned, In Progress, Resolved, Closed)
        // This is essential for dashboard queries, list views, and SLA management
        migrationBuilder.CreateIndex(
            name: "IX_Problem_Status",
            table: "Problems",
            column: "State");

        // IX_Problem_CreatedAt: For sorting problems by creation date
        // Supports timeline analytics and report generation
        migrationBuilder.CreateIndex(
            name: "IX_Problem_CreatedAt",
            table: "Problems",
            column: "CreatedAt");

        // IX_Problem_Priority: For filtering by priority level (Urgent, High, Medium, Low)
        // Supports prioritization queries and escalation workflows
        migrationBuilder.CreateIndex(
            name: "IX_Problem_Priority",
            table: "Problems",
            column: "Priority");

        // IX_Problem_ProblemManagerId: For looking up problems assigned to a specific manager
        // Supports manager dashboards and workload balancing
        migrationBuilder.CreateIndex(
            name: "IX_Problem_ProblemManagerId",
            table: "Problems",
            column: "ProblemManagerId");

        // =====================================================================
        // Problem-Incident Junction Table Indexes - Reverse Lookup
        // =====================================================================

        // IX_ProblemIncident_IncidentId: For reverse lookup - find problems related to an incident
        // Essential for incident resolution workflows and traceability
        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncident_IncidentId",
            table: "ProblemIncidents",
            column: "IncidentId");

        // IX_ProblemIncident_CreatedAt: For timeline queries of problem-incident relationships
        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncident_CreatedAt",
            table: "ProblemIncidents",
            column: "CreatedAt");

        // =====================================================================
        // Problem Supporting Tables Indexes
        // =====================================================================

        // IX_ProblemTask_ProblemId: For retrieving all tasks for a problem
        migrationBuilder.CreateIndex(
            name: "IX_ProblemTask_ProblemId",
            table: "ProblemTasks",
            column: "ProblemId");

        // IX_ProblemTask_Status: For filtering tasks by status
        migrationBuilder.CreateIndex(
            name: "IX_ProblemTask_Status",
            table: "ProblemTasks",
            column: "Status");

        // IX_ProblemComment_ProblemId: For retrieving all comments for a problem
        migrationBuilder.CreateIndex(
            name: "IX_ProblemComment_ProblemId",
            table: "ProblemComments",
            column: "ProblemId");

        // IX_ProblemComment_CreatedAt: For timeline ordering of comments
        migrationBuilder.CreateIndex(
            name: "IX_ProblemComment_CreatedAt",
            table: "ProblemComments",
            column: "CreatedAt");

        // IX_ProblemAttachment_ProblemId: For retrieving all attachments for a problem
        migrationBuilder.CreateIndex(
            name: "IX_ProblemAttachment_ProblemId",
            table: "ProblemAttachments",
            column: "ProblemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Drop all Problem Management indexes in reverse order
        // =====================================================================

        migrationBuilder.DropIndex(
            name: "IX_ProblemAttachment_ProblemId",
            table: "ProblemAttachments");

        migrationBuilder.DropIndex(
            name: "IX_ProblemComment_CreatedAt",
            table: "ProblemComments");

        migrationBuilder.DropIndex(
            name: "IX_ProblemComment_ProblemId",
            table: "ProblemComments");

        migrationBuilder.DropIndex(
            name: "IX_ProblemTask_Status",
            table: "ProblemTasks");

        migrationBuilder.DropIndex(
            name: "IX_ProblemTask_ProblemId",
            table: "ProblemTasks");

        migrationBuilder.DropIndex(
            name: "IX_ProblemIncident_CreatedAt",
            table: "ProblemIncidents");

        migrationBuilder.DropIndex(
            name: "IX_ProblemIncident_IncidentId",
            table: "ProblemIncidents");

        migrationBuilder.DropIndex(
            name: "IX_Problem_ProblemManagerId",
            table: "Problems");

        migrationBuilder.DropIndex(
            name: "IX_Problem_Priority",
            table: "Problems");

        migrationBuilder.DropIndex(
            name: "IX_Problem_CreatedAt",
            table: "Problems");

        migrationBuilder.DropIndex(
            name: "IX_Problem_Status",
            table: "Problems");
    }
}
