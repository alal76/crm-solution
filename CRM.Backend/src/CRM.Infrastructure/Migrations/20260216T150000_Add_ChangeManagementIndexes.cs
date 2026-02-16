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
/// Migration for Phase 4: Change Management - Performance Indexes
/// 
/// Adds critical indexes to Change Management entities to optimize:
/// - Change workflow queries (status filtering, timeline queries)
/// - CAB (Change Advisory Board) voting and approval workflows
/// - Change impact analysis and CI relationships
/// - Change blackout management
/// 
/// Tables affected:
/// - Changes: Add IX_Change_Status, IX_Change_CreatedAt, IX_Change_ScheduledStartDate
/// - ChangeApprovals: Add IX_ChangeApproval_ChangeId_ApproverId (CAB voting lookup)
/// - ChangeApprovals: Add IX_ChangeApproval_Status
/// - ChangeBlackout: Add IX_ChangeBlackout_ChangeId
/// - ChangeImpactedCI: Add IX_ChangeImpactedCI_ChangeId (reverse lookup)
/// </summary>
public partial class Add_ChangeManagementIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Change Management Workflow Indexes
        // =====================================================================

        // IX_Change_Status: For filtering changes by workflow state
        // (Draft, Submitted, Approved, Scheduled, Implemented, Completed, Cancelled, Rolled Back)
        // Essential for change calendars, approval dashboards, and workflow automation
        migrationBuilder.CreateIndex(
            name: "IX_Change_Status",
            table: "Changes",
            column: "State");

        // IX_Change_CreatedAt: For timeline analysis of changes
        // Supports change history reports and trend analysis
        migrationBuilder.CreateIndex(
            name: "IX_Change_CreatedAt",
            table: "Changes",
            column: "CreatedAt");

        // IX_Change_ScheduledStartDate: For change window management
        // Critical for finding changes scheduled during specific time periods
        migrationBuilder.CreateIndex(
            name: "IX_Change_ScheduledStartDate",
            table: "Changes",
            column: "ScheduledStartDate");

        // IX_Change_Priority: For prioritization and routing
        migrationBuilder.CreateIndex(
            name: "IX_Change_Priority",
            table: "Changes",
            column: "Priority");

        // IX_Change_RequestorId: For changes requested by specific users
        migrationBuilder.CreateIndex(
            name: "IX_Change_RequestorId",
            table: "Changes",
            column: "RequestorUserId");

        // =====================================================================
        // Change Approval (CAB Voting) Indexes
        // =====================================================================

        // IX_ChangeApproval_ChangeId_ApproverId: Composite index for CAB voting lookup
        // Used to find all approvals for a change or check if specific approver has voted
        // Essential for voting status dashboards and approval workflows
        migrationBuilder.CreateIndex(
            name: "IX_ChangeApproval_ChangeId_ApproverId",
            table: "ChangeApprovals",
            columns: new[] { "ChangeId", "ApproverId" });

        // IX_ChangeApproval_Status: For filtering approvals by vote status (Pending, Approved, Rejected, Abstain)
        // Supports CAB dashboard queries
        migrationBuilder.CreateIndex(
            name: "IX_ChangeApproval_Status",
            table: "ChangeApprovals",
            column: "ApprovalStatus");

        // IX_ChangeApproval_ApprovalLevel: For hierarchical approval workflows
        migrationBuilder.CreateIndex(
            name: "IX_ChangeApproval_ApprovalLevel",
            table: "ChangeApprovals",
            column: "ApprovalLevel");

        // IX_ChangeApproval_CreatedAt: For timeline of approval decisions
        migrationBuilder.CreateIndex(
            name: "IX_ChangeApproval_CreatedAt",
            table: "ChangeApprovals",
            column: "CreatedAt");

        // =====================================================================
        // Change Impact Analysis Indexes
        // =====================================================================

        // IX_ChangeImpactedCI_ChangeId: For reverse lookup - find all CIs impacted by a change
        // Essential for impact analysis, rollback planning, and risk assessment
        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCI_ChangeId",
            table: "ChangeImpactedCIs",
            column: "ChangeId");

        // IX_ChangeImpactedCI_CIId: For reverse lookup - find all changes impacting a specific CI
        // Supports CI-centric change tracking
        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCI_CIId",
            table: "ChangeImpactedCIs",
            column: "CIId");

        // IX_ChangeImpactedCI_Impact: For filtering CIs by impact level (High, Medium, Low, Unknown)
        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCI_Impact",
            table: "ChangeImpactedCIs",
            column: "ThePlannedDowntime");

        // =====================================================================
        // Change Blackout Management Indexes
        // =====================================================================

        // IX_ChangeBlackout_ChangeId: For finding blackout periods for a change
        migrationBuilder.CreateIndex(
            name: "IX_ChangeBlackout_ChangeId",
            table: "ChangeBlackouts",
            column: "ChangeId");

        // IX_ChangeBlackout_StartTime: For finding active blackout periods
        // Critical for change scheduling and conflict detection
        migrationBuilder.CreateIndex(
            name: "IX_ChangeBlackout_StartTime",
            table: "ChangeBlackouts",
            column: "StartTime");

        // =====================================================================
        // Change Task and Comments Indexes
        // =====================================================================

        // IX_ChangeTask_ChangeId: For retrieving all tasks for a change
        migrationBuilder.CreateIndex(
            name: "IX_ChangeTask_ChangeId",
            table: "ChangeTasks",
            column: "ChangeId");

        // IX_ChangeTask_Status: For filtering tasks by status
        migrationBuilder.CreateIndex(
            name: "IX_ChangeTask_Status",
            table: "ChangeTasks",
            column: "Status");

        // IX_ChangeComment_ChangeId: For retrieving all comments for a change
        migrationBuilder.CreateIndex(
            name: "IX_ChangeComment_ChangeId",
            table: "ChangeComments",
            column: "ChangeId");

        // IX_ChangeAttachment_ChangeId: For retrieving all attachments for a change
        migrationBuilder.CreateIndex(
            name: "IX_ChangeAttachment_ChangeId",
            table: "ChangeAttachments",
            column: "ChangeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Drop all Change Management indexes in reverse order
        // =====================================================================

        migrationBuilder.DropIndex(
            name: "IX_ChangeAttachment_ChangeId",
            table: "ChangeAttachments");

        migrationBuilder.DropIndex(
            name: "IX_ChangeComment_ChangeId",
            table: "ChangeComments");

        migrationBuilder.DropIndex(
            name: "IX_ChangeTask_Status",
            table: "ChangeTasks");

        migrationBuilder.DropIndex(
            name: "IX_ChangeTask_ChangeId",
            table: "ChangeTasks");

        migrationBuilder.DropIndex(
            name: "IX_ChangeBlackout_StartTime",
            table: "ChangeBlackouts");

        migrationBuilder.DropIndex(
            name: "IX_ChangeBlackout_ChangeId",
            table: "ChangeBlackouts");

        migrationBuilder.DropIndex(
            name: "IX_ChangeImpactedCI_Impact",
            table: "ChangeImpactedCIs");

        migrationBuilder.DropIndex(
            name: "IX_ChangeImpactedCI_CIId",
            table: "ChangeImpactedCIs");

        migrationBuilder.DropIndex(
            name: "IX_ChangeImpactedCI_ChangeId",
            table: "ChangeImpactedCIs");

        migrationBuilder.DropIndex(
            name: "IX_ChangeApproval_CreatedAt",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_ChangeApproval_ApprovalLevel",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_ChangeApproval_Status",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_ChangeApproval_ChangeId_ApproverId",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_Change_RequestorId",
            table: "Changes");

        migrationBuilder.DropIndex(
            name: "IX_Change_Priority",
            table: "Changes");

        migrationBuilder.DropIndex(
            name: "IX_Change_ScheduledStartDate",
            table: "Changes");

        migrationBuilder.DropIndex(
            name: "IX_Change_CreatedAt",
            table: "Changes");

        migrationBuilder.DropIndex(
            name: "IX_Change_Status",
            table: "Changes");
    }
}
