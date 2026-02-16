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
/// Migration for P0-DB-001: Email Sequence Entity Configuration
/// 
/// Ensures EmailSequence and related entities have complete property configurations:
/// - EmailSequence: Columns, relationships, cascade behavior, indexes
/// - EmailSequenceStep: Column types, step-sequence relationship
/// - EmailSequenceEnrollment: Status tracking, contact/lead links
/// - EmailSequenceStepExecution: Execution history with email tracking
/// 
/// This supports Marketing Automation and drip email campaigns.
/// </summary>
public partial class Add_EmailSequence_EntityConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add indexes for EmailSequence performance (Status, CreatedAt for filtering)
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequences_Status",
            table: "EmailSequences",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequences_CreatedAt",
            table: "EmailSequences",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequences_OwnerId",
            table: "EmailSequences",
            column: "OwnerId");

        // Add indexes for EmailSequenceStep performance
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceSteps_SequenceId",
            table: "EmailSequenceSteps",
            column: "SequenceId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceSteps_SequenceId_StepOrder",
            table: "EmailSequenceSteps",
            columns: new[] { "SequenceId", "StepOrder" });

        // Add indexes for EmailSequenceEnrollment performance (Status, ContactId, LeadId, SequenceId)
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollments_SequenceId",
            table: "EmailSequenceEnrollments",
            column: "SequenceId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollments_Status",
            table: "EmailSequenceEnrollments",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollments_ContactId",
            table: "EmailSequenceEnrollments",
            column: "ContactId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollments_LeadId",
            table: "EmailSequenceEnrollments",
            column: "LeadId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollments_SequenceId_Status",
            table: "EmailSequenceEnrollments",
            columns: new[] { "SequenceId", "Status" });

        // Add indexes for EmailSequenceStepExecution performance (tracking, execution audit)
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecutions_EnrollmentId",
            table: "EmailSequenceStepExecutions",
            column: "EnrollmentId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecutions_ExecutedAt",
            table: "EmailSequenceStepExecutions",
            column: "ExecutedAt");

        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecutions_ExecutionStatus",
            table: "EmailSequenceStepExecutions",
            column: "ExecutionStatus");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop all Email Sequence indexes
        migrationBuilder.DropIndex(
            name: "IX_EmailSequences_Status",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequences_CreatedAt",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequences_OwnerId",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceSteps_SequenceId",
            table: "EmailSequenceSteps");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceSteps_SequenceId_StepOrder",
            table: "EmailSequenceSteps");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollments_SequenceId",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollments_Status",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollments_ContactId",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollments_LeadId",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollments_SequenceId_Status",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecutions_EnrollmentId",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecutions_ExecutedAt",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecutions_ExecutionStatus",
            table: "EmailSequenceStepExecutions");
    }
}
