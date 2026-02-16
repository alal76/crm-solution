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
/// Migration for Phase 7: Email Sequence Management - Performance Indexes
/// 
/// Adds critical indexes to Email Sequence entities to optimize:
/// - Email sequence execution and enrollment workflows
/// - Recipient progress tracking and email delivery management
/// - Sequence condition evaluation and filtering
/// - Campaign performance analytics and reporting
/// 
/// Tables affected:
/// - EmailSequences: Add IX_EmailSequence_Status (Active/Draft/Paused filters)
/// - EmailSequenceEnrollments: Add IX_EmailSequenceEnrollment_Status
/// - EmailSequenceStepExecutions: Add IX_EmailSequenceStepExecution_Status_NextExecutionDate
/// - RecipientProgress: Add IX_RecipientProgress_SequenceId_Status (sequence progress)
/// - RecipientProgress: Add IX_RecipientProgress_ContactId (contact-centric view)
/// </summary>
public partial class Add_EmailSequenceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Email Sequence Lifecycle Indexes
        // =====================================================================

        // IX_EmailSequence_Status: For filtering sequences by workflow state
        // (Draft, Active, Paused, Completed, Archived, Deleted)
        // Essential for sequence management dashboards and execution controls
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequence_Status",
            table: "EmailSequences",
            column: "Status");

        // IX_EmailSequence_CampaignId: For retrieving sequences for a campaign
        // Supports campaign analytics and performance tracking
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequence_CampaignId",
            table: "EmailSequences",
            column: "CampaignId");

        // IX_EmailSequence_CreatedAt: For timeline of sequence creation
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequence_CreatedAt",
            table: "EmailSequences",
            column: "CreatedAt");

        // IX_EmailSequence_OwnerId: For retrieving sequences owned by a user
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequence_OwnerId",
            table: "EmailSequences",
            column: "OwnerId");

        // =====================================================================
        // Email Sequence Enrollment Management Indexes
        // =====================================================================

        // IX_EmailSequenceEnrollment_Status: For filtering enrollments by status
        // (Active, Paused, Completed, Unsubscribed, Bounced, Failed, ManualPause)
        // Supports enrollment dashboard and health monitoring
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollment_Status",
            table: "EmailSequenceEnrollments",
            column: "Status");

        // IX_EmailSequenceEnrollment_SequenceId: For retrieving all enrollments in a sequence
        // Essential for sequence analytics and reporting
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollment_SequenceId",
            table: "EmailSequenceEnrollments",
            column: "EmailSequenceId");

        // IX_EmailSequenceEnrollment_ContactId: For retrieving sequences a contact is enrolled in
        // Supports contact journey tracking and engagement analysis
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollment_ContactId",
            table: "EmailSequenceEnrollments",
            column: "ContactId");

        // IX_EmailSequenceEnrollment_EnrolledDate: For cohort analysis and timeline queries
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollment_EnrolledDate",
            table: "EmailSequenceEnrollments",
            column: "EnrolledAt");

        // IX_EmailSequenceEnrollment_SequenceId_Status: Composite for sequence health analysis
        // Used to find active, paused, or completed enrollments in a sequence
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceEnrollment_SequenceId_Status",
            table: "EmailSequenceEnrollments",
            columns: new[] { "EmailSequenceId", "Status" });

        // =====================================================================
        // Email Sequence Step Execution Indexes
        // =====================================================================

        // IX_EmailSequenceStepExecution_Status_NextExecutionDate: Composite for workflow execution
        // Used to find step executions pending delivery within a time window
        // Critical for email sender queue processing
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecution_Status_NextExecutionDate",
            table: "EmailSequenceStepExecutions",
            columns: new[] { "ExecutionStatus", "NextExecutionDate" });

        // IX_EmailSequenceStepExecution_EnrollmentId: For retrieving execution history for an enrollment
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecution_EnrollmentId",
            table: "EmailSequenceStepExecutions",
            column: "EnrollmentId");

        // IX_EmailSequenceStepExecution_StepId: For retrieving executions of a specific step
        // Supports step performance analytics
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecution_StepId",
            table: "EmailSequenceStepExecutions",
            column: "StepId");

        // IX_EmailSequenceStepExecution_SentAt: For timeline analysis of email sends
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecution_SentAt",
            table: "EmailSequenceStepExecutions",
            column: "SentAt");

        // IX_EmailSequenceStepExecution_OpenedAt: For engagement tracking analysis
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecution_OpenedAt",
            table: "EmailSequenceStepExecutions",
            column: "OpenedAt");

        // IX_EmailSequenceStepExecution_ClickedAt: For click-through rate analysis
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStepExecution_ClickedAt",
            table: "EmailSequenceStepExecutions",
            column: "ClickedAt");

        // =====================================================================
        // Email Sequence Step Configuration Indexes
        // =====================================================================

        // IX_EmailSequenceStep_SequenceId: For retrieving steps in a sequence
        // Ordered by display order for playback
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequenceStep_SequenceId",
            table: "EmailSequenceSteps",
            column: "EmailSequenceId");

        // =====================================================================
        // Recipient Progress Tracking Indexes (if using separate progress table)
        // =====================================================================

        // IX_RecipientProgress_SequenceId_Status: Composite for sequence progress analysis
        // Used to find recipients at different stages of a sequence
        // Essential for stage-based engagement scoring
        migrationBuilder.CreateIndex(
            name: "IX_RecipientProgress_SequenceId_Status",
            table: "EmailSequenceEnrollments",  // Using enrollments as progress table
            columns: new[] { "EmailSequenceId", "Status" });

        // IX_RecipientProgress_ContactId: For contact-centric sequence journey view
        // Shows all sequences a contact has engaged with
        migrationBuilder.CreateIndex(
            name: "IX_RecipientProgress_ContactId",
            table: "EmailSequenceEnrollments",
            column: "ContactId");

        // IX_RecipientProgress_LastInteractionDate: For recency-based filtering
        // Supports "recently engaged" and "at-risk" recipient identification
        migrationBuilder.CreateIndex(
            name: "IX_RecipientProgress_LastInteractionDate",
            table: "EmailSequenceEnrollments",
            column: "UpdatedAt");

        // =====================================================================
        // Sequence Delivery Performance Indexes
        // =====================================================================

        // IX_EmailSequence_TotalEnfollmentCount: Helper index for enrollment counts
        // Supporting "active sequences" and "top performing" analytics
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequence_TotalSent",
            table: "EmailSequences",
            column: "TotalEmailsSent");

        // Combined index for active sequence list performance
        migrationBuilder.CreateIndex(
            name: "IX_EmailSequence_Status_CreatedAt",
            table: "EmailSequences",
            columns: new[] { "Status", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Drop all Email Sequence indexes in reverse order
        // =====================================================================

        migrationBuilder.DropIndex(
            name: "IX_EmailSequence_Status_CreatedAt",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequence_TotalSent",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_RecipientProgress_LastInteractionDate",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_RecipientProgress_ContactId",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_RecipientProgress_SequenceId_Status",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStep_SequenceId",
            table: "EmailSequenceSteps");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecution_ClickedAt",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecution_OpenedAt",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecution_SentAt",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecution_StepId",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecution_EnrollmentId",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceStepExecution_Status_NextExecutionDate",
            table: "EmailSequenceStepExecutions");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollment_SequenceId_Status",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollment_EnrolledDate",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollment_ContactId",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollment_SequenceId",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequenceEnrollment_Status",
            table: "EmailSequenceEnrollments");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequence_OwnerId",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequence_CreatedAt",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequence_CampaignId",
            table: "EmailSequences");

        migrationBuilder.DropIndex(
            name: "IX_EmailSequence_Status",
            table: "EmailSequences");
    }
}
