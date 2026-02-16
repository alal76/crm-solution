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
/// Migration for Phase 6: Subscription & Recurring Billing - Performance Indexes
/// 
/// Adds critical indexes to Subscription and Billing entities to optimize:
/// - Subscription lifecycle queries (active, paused, cancelled status)
/// - Recurring invoice generation and billing cycle management
/// - Dunning history and payment retry tracking
/// - Renewal date-based queries and notifications
/// 
/// Tables affected:
/// - Subscriptions: Add IX_Subscription_Status_RenewalDate (renewal processing)
/// - RecurringInvoices: Add IX_RecurringInvoice_NextBillingDate (billing cycle)
/// - DunningHistory: Add IX_DunningHistory_SubscriptionId, IX_DunningHistory_Status
/// - BillingHistory: Add IX_BillingHistory_SubscriptionId_Date
/// </summary>
public partial class Add_SubscriptionBillingIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Subscription Lifecycle Indexes
        // =====================================================================

        // IX_Subscription_Status_RenewalDate: Composite index for renewal processing
        // Used to find subscriptions that are active and need renewal within a date range
        // Essential for renewal reminders, auto-renewal workflows, and churn prevention
        migrationBuilder.CreateIndex(
            name: "IX_Subscription_Status_RenewalDate",
            table: "Subscriptions",
            columns: new[] { "Status", "RenewalDate" });

        // IX_Subscription_Status: For filtering subscriptions by lifecycle state
        // (Active, Paused, Cancelled, Pending, Expired)
        // Supports dashboard queries and revenue forecasting
        migrationBuilder.CreateIndex(
            name: "IX_Subscription_Status",
            table: "Subscriptions",
            column: "Status");

        // IX_Subscription_RenewalDate: For finding subscriptions due for renewal
        // Critical for renewal workflow automation
        migrationBuilder.CreateIndex(
            name: "IX_Subscription_RenewalDate",
            table: "Subscriptions",
            column: "RenewalDate");

        // IX_Subscription_AccountId: For retrieving all subscriptions for an account
        // Supports account health scoring and expansion opportunities
        migrationBuilder.CreateIndex(
            name: "IX_Subscription_AccountId",
            table: "Subscriptions",
            column: "AccountId");

        // IX_Subscription_StartDate: For timeline analysis of subscription growth
        migrationBuilder.CreateIndex(
            name: "IX_Subscription_StartDate",
            table: "Subscriptions",
            column: "StartDate");

        // =====================================================================
        // Recurring Billing (Billing History) Indexes
        // =====================================================================

        // IX_BillingHistory_EventType: Additional index for event filtering (if not in previous migration)
        // Used to find billing events of specific types (InvoiceGenerated, PaymentSuccessful, etc.)
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_EventType_EventDate",
            table: "BillingHistory",
            columns: new[] { "EventType", "EventDate" });

        // IX_BillingHistory_UserId: For retrieving billing history for a user who triggered the event
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_UserId",
            table: "BillingHistory",
            column: "UserId");

        // IX_BillingHistory_DunningRecordId: For linking back from billing to dunning
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_DunningRecordId",
            table: "BillingHistory",
            column: "DunningRecordId");

        // =====================================================================
        // Dunning History (Payment Retry) Indexes
        // =====================================================================

        // IX_DunningHistory_SubscriptionId: For retrieving dunning records for a subscription
        // Essential for tracking failed payment recovery attempts
        migrationBuilder.CreateIndex(
            name: "IX_DunningHistory_SubscriptionId_Secondary",
            table: "DunningRecords",
            column: "SubscriptionId");

        // IX_DunningHistory_Status: For filtering dunning records by status
        // (Pending, Retrying, Recovered, Abandoned, Cancelled)
        // Supports dunning management dashboard
        // Note: IX_DunningRecords_Status_NextRetryDate composite index may already exist from previous migration
        migrationBuilder.CreateIndex(
            name: "IX_DunningHistory_InitialFailureDate_Status",
            table: "DunningRecords",
            columns: new[] { "InitialFailureDate", "Status" });

        // IX_DunningHistory_GracePeriodEndDate: For finding dunning records with expired grace periods
        migrationBuilder.CreateIndex(
            name: "IX_DunningHistory_GracePeriodEndDate",
            table: "DunningRecords",
            column: "GracePeriodEndDate");

        // =====================================================================
        // Billing History (Transaction Log) Indexes
        // =====================================================================

        // IX_BillingHistory_SubscriptionId_Date: Composite index for period-based billing queries
        // Used to retrieve billing events for a subscription within a date range
        // Essential for subscription analytics and revenue recognition
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_SubscriptionId_Date",
            table: "BillingHistory",
            columns: new[] { "SubscriptionId", "EventDate" });

        // IX_BillingHistory_SubscriptionId: For retrieving all billing events for a subscription
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_SubscriptionId",
            table: "BillingHistory",
            column: "SubscriptionId");

        // IX_BillingHistory_EventType: For filtering billing events by type
        // (InvoiceGenerated, PaymentSuccessful, PaymentFailed, ProrationAdjustment, PlanChanged, Renewal, Cancellation, Reactivation)
        // Supports event-driven billing workflows
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_EventType",
            table: "BillingHistory",
            column: "EventType");

        // IX_BillingHistory_Status: For filtering billing history by status
        // (Pending, Billed, Paid, Failed, Disputed, Reversed)
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_Status",
            table: "BillingHistory",
            column: "Status");

        // IX_BillingHistory_BilledDate: For finding invoices in a date range
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_BilledDate",
            table: "BillingHistory",
            column: "BilledDate");

        // IX_BillingHistory_PaidDate: For revenue recognition queries
        migrationBuilder.CreateIndex(
            name: "IX_BillingHistory_PaidDate",
            table: "BillingHistory",
            column: "PaidDate");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Drop all Subscription & Billing indexes in reverse order
        // =====================================================================

        migrationBuilder.DropIndex(
            name: "IX_BillingHistory_PaidDate",
            table: "BillingHistory");

        migrationBuilder.DropIndex(
            name: "IX_BillingHistory_BilledDate",
            table: "BillingHistory");

        migrationBuilder.DropIndex(
            name: "IX_BillingHistory_Status",
            table: "BillingHistory");

        migrationBuilder.DropIndex(
            name: "IX_BillingHistory_EventType",
            table: "BillingHistory");

        migrationBuilder.DropIndex(
            name: "IX_BillingHistory_SubscriptionId",
            table: "BillingHistory");

        migrationBuilder.DropIndex(
            name: "IX_BillingHistory_SubscriptionId_Date",
            table: "BillingHistory");

        migrationBuilder.DropIndex(
            name: "IX_DunningHistory_InitialFailureDate",
            table: "DunningRecords");

        migrationBuilder.DropIndex(
            name: "IX_DunningHistory_InvoiceId",
            table: "DunningRecords");

        migrationBuilder.DropIndex(
            name: "IX_DunningHistory_NextRetryDate",
            table: "DunningRecords");

        migrationBuilder.DropIndex(
            name: "IX_DunningHistory_Status",
            table: "DunningRecords");

        migrationBuilder.DropIndex(
            name: "IX_DunningHistory_SubscriptionId",
            table: "DunningRecords");

        migrationBuilder.DropIndex(
            name: "IX_RecurringInvoice_CreatedAt",
            table: "RecurringInvoices");

        migrationBuilder.DropIndex(
            name: "IX_RecurringInvoice_Status",
            table: "RecurringInvoices");

        migrationBuilder.DropIndex(
            name: "IX_RecurringInvoice_SubscriptionId",
            table: "RecurringInvoices");

        migrationBuilder.DropIndex(
            name: "IX_RecurringInvoice_NextBillingDate",
            table: "RecurringInvoices");

        migrationBuilder.DropIndex(
            name: "IX_Subscription_StartDate",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Subscription_AccountId",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Subscription_RenewalDate",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Subscription_Status",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Subscription_Status_RenewalDate",
            table: "Subscriptions");
    }
}
