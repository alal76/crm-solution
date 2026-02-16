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
/// Migration for Phase 5: Commission Management - Performance Indexes
/// 
/// Adds critical indexes to Commission entities to optimize:
/// - Commission rule configuration and lookup
/// - Commission calculation and history tracking
/// - Sales rep commission statements and reporting
/// - Commission plan assignment management
/// 
/// Tables affected:
/// - CommissionRule: Add IX_CommissionRule_Status, IX_CommissionRule_RuleType
/// - CommissionHistory: Add IX_CommissionHistory_EmployeeId_Date
/// - CommissionStatement: Add IX_CommissionStatement_SalesRepId_PeriodDate
/// - CommissionPlanAssignment: Add IX_CommissionPlanAssignment_UserId
/// </summary>
public partial class Add_CommissionManagementIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Commission Rule Configuration Indexes
        // =====================================================================

        // IX_CommissionRule_Status: For filtering active/inactive rules
        // Essential for rule selection during commission calculations
        migrationBuilder.CreateIndex(
            name: "IX_CommissionRule_Status",
            table: "CommissionRules",
            column: "IsActive");

        // IX_CommissionRule_RuleType: For filtering rules by type (Percentage, Flat, Tiered)
        // Supports rule-based commission calculations
        migrationBuilder.CreateIndex(
            name: "IX_CommissionRule_RuleType",
            table: "CommissionRules",
            column: "RuleType");

        // IX_CommissionRule_Name: For searching rules by name
        migrationBuilder.CreateIndex(
            name: "IX_CommissionRule_Name",
            table: "CommissionRules",
            column: "Name");

        // IX_CommissionRule_CreatedAt: For timeline of rule changes
        migrationBuilder.CreateIndex(
            name: "IX_CommissionRule_CreatedAt",
            table: "CommissionRules",
            column: "CreatedAt");

        // =====================================================================
        // Commission History Audit Trail Indexes
        // =====================================================================

        // IX_CommissionHistory_EmployeeId: For retrieving commission history for a sales rep
        // Essential for sales rep dashboards and commission audits
        migrationBuilder.CreateIndex(
            name: "IX_CommissionHistory_EmployeeId",
            table: "CommissionHistories",
            column: "EmployeeId");

        // IX_CommissionHistory_SalesRepId_Date: Composite index for period-based commission reports
        // Optimizes queries for "commissions earned by rep in date range"
        // Critical for monthly/quarterly commission statements
        migrationBuilder.CreateIndex(
            name: "IX_CommissionHistory_SalesRepId_Date",
            table: "CommissionHistories",
            columns: new[] { "EmployeeId", "CalculationDate" });

        // IX_CommissionHistory_RuleId: For tracking which rule calculated a commission
        migrationBuilder.CreateIndex(
            name: "IX_CommissionHistory_RuleId",
            table: "CommissionHistories",
            column: "CommissionRuleId");

        // IX_CommissionHistory_CalculationDate: For timeline queries
        migrationBuilder.CreateIndex(
            name: "IX_CommissionHistory_CalculationDate",
            table: "CommissionHistories",
            column: "CalculationDate");

        // =====================================================================
        // Commission Statements and Payments Indexes
        // =====================================================================

        // IX_CommissionStatement_SalesRepId_PeriodDate: Composite index for statement retrieval
        // Used for generating monthly/quarterly commission statements
        // Essential for commission payout processing
        migrationBuilder.CreateIndex(
            name: "IX_CommissionStatement_SalesRepId_PeriodDate",
            table: "CommissionStatements",
            columns: new[] { "SalesRepId", "PeriodStartDate" });

        // IX_CommissionStatement_Status: For filtering statements by status (Draft, Approved, Paid, Disputed)
        // Supports approval workflow and payment processing
        migrationBuilder.CreateIndex(
            name: "IX_CommissionStatement_Status",
            table: "CommissionStatements",
            column: "Status");

        // IX_CommissionStatement_ApprovalDate: For filtering approved statements
        migrationBuilder.CreateIndex(
            name: "IX_CommissionStatement_ApprovalDate",
            table: "CommissionStatements",
            column: "ApprovalDate");

        // =====================================================================
        // Commission Plan Assignment Indexes
        // =====================================================================

        // IX_CommissionPlanAssignment_UserId: For retrieving assigned plans for a user
        // Essential for determining applicable commission rules for a sales rep
        migrationBuilder.CreateIndex(
            name: "IX_CommissionPlanAssignment_UserId",
            table: "CommissionPlanAssignments",
            column: "UserId");

        // IX_CommissionPlanAssignment_PlanId: For finding users on a specific plan
        migrationBuilder.CreateIndex(
            name: "IX_CommissionPlanAssignment_PlanId",
            table: "CommissionPlanAssignments",
            column: "CommissionPlanId");

        // IX_CommissionPlanAssignment_EffectiveDate: For time-based plan lookups
        // Important for handling plan changes mid-period
        migrationBuilder.CreateIndex(
            name: "IX_CommissionPlanAssignment_EffectiveDate",
            table: "CommissionPlanAssignments",
            column: "EffectiveFromDate");

        // =====================================================================
        // Commission Plan and Tier Indexes
        // =====================================================================

        // IX_CommissionPlan_IsActive: For filtering active plans
        migrationBuilder.CreateIndex(
            name: "IX_CommissionPlan_IsActive",
            table: "CommissionPlans",
            column: "IsActive");

        // IX_CommissionTier_PlanId: For retrieving tiers for a plan
        migrationBuilder.CreateIndex(
            name: "IX_CommissionTier_PlanId",
            table: "CommissionTiers",
            column: "CommissionPlanId");

        // =====================================================================
        // Commission Transactions Indexes
        // =====================================================================

        // IX_Commission_SalesRepId: For retrieving commissions for a sales rep
        migrationBuilder.CreateIndex(
            name: "IX_Commission_SalesRepId",
            table: "Commissions",
            column: "SalesRepId");

        // IX_Commission_EarnedDate: For timeline-based commission queries
        migrationBuilder.CreateIndex(
            name: "IX_Commission_EarnedDate",
            table: "Commissions",
            column: "EarnedDate");

        // IX_Commission_Status: For filtering commissions by status (Pending, Approved, Paid, Disputed)
        migrationBuilder.CreateIndex(
            name: "IX_Commission_Status",
            table: "Commissions",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Drop all Commission Management indexes in reverse order
        // =====================================================================

        migrationBuilder.DropIndex(
            name: "IX_Commission_Status",
            table: "Commissions");

        migrationBuilder.DropIndex(
            name: "IX_Commission_EarnedDate",
            table: "Commissions");

        migrationBuilder.DropIndex(
            name: "IX_Commission_SalesRepId",
            table: "Commissions");

        migrationBuilder.DropIndex(
            name: "IX_CommissionTier_PlanId",
            table: "CommissionTiers");

        migrationBuilder.DropIndex(
            name: "IX_CommissionPlan_IsActive",
            table: "CommissionPlans");

        migrationBuilder.DropIndex(
            name: "IX_CommissionPlanAssignment_EffectiveDate",
            table: "CommissionPlanAssignments");

        migrationBuilder.DropIndex(
            name: "IX_CommissionPlanAssignment_PlanId",
            table: "CommissionPlanAssignments");

        migrationBuilder.DropIndex(
            name: "IX_CommissionPlanAssignment_UserId",
            table: "CommissionPlanAssignments");

        migrationBuilder.DropIndex(
            name: "IX_CommissionStatement_ApprovalDate",
            table: "CommissionStatements");

        migrationBuilder.DropIndex(
            name: "IX_CommissionStatement_Status",
            table: "CommissionStatements");

        migrationBuilder.DropIndex(
            name: "IX_CommissionStatement_SalesRepId_PeriodDate",
            table: "CommissionStatements");

        migrationBuilder.DropIndex(
            name: "IX_CommissionHistory_CalculationDate",
            table: "CommissionHistories");

        migrationBuilder.DropIndex(
            name: "IX_CommissionHistory_RuleId",
            table: "CommissionHistories");

        migrationBuilder.DropIndex(
            name: "IX_CommissionHistory_SalesRepId_Date",
            table: "CommissionHistories");

        migrationBuilder.DropIndex(
            name: "IX_CommissionHistory_EmployeeId",
            table: "CommissionHistories");

        migrationBuilder.DropIndex(
            name: "IX_CommissionRule_CreatedAt",
            table: "CommissionRules");

        migrationBuilder.DropIndex(
            name: "IX_CommissionRule_Name",
            table: "CommissionRules");

        migrationBuilder.DropIndex(
            name: "IX_CommissionRule_RuleType",
            table: "CommissionRules");

        migrationBuilder.DropIndex(
            name: "IX_CommissionRule_Status",
            table: "CommissionRules");
    }
}
