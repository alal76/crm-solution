// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for subscription management operations.
/// Handles subscription lifecycle from creation to billing.
/// </summary>
public interface ISubscriptionService
{
    #region CRUD Operations

    /// <summary>Gets all subscriptions with optional filtering.</summary>
    Task<IEnumerable<Subscription>> GetAllAsync(
        int? accountId = null,
        SubscriptionStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a subscription by ID.</summary>
    Task<Subscription?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a subscription by subscription number.</summary>
    Task<Subscription?> GetBySubscriptionNumberAsync(string subscriptionNumber, CancellationToken cancellationToken = default);

    /// <summary>Creates a new subscription.</summary>
    Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing subscription.</summary>
    Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>Deletes a subscription (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Subscription Operations

    /// <summary>Creates a subscription from an order.</summary>
    Task<Subscription> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Generates the next subscription number.</summary>
    Task<string> GenerateSubscriptionNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>Activates a subscription.</summary>
    Task<Subscription> ActivateAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Pauses a subscription.</summary>
    Task<Subscription> PauseAsync(int subscriptionId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>Resumes a paused subscription.</summary>
    Task<Subscription> ResumeAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Cancels a subscription.</summary>
    Task<Subscription> CancelAsync(int subscriptionId, string reason, bool immediate = false, CancellationToken cancellationToken = default);

    #endregion

    #region Status Management

    /// <summary>Updates subscription status.</summary>
    Task<Subscription> UpdateStatusAsync(int subscriptionId, SubscriptionStatus status, CancellationToken cancellationToken = default);

    /// <summary>Suspends a subscription for non-payment.</summary>
    Task<Subscription> SuspendAsync(int subscriptionId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Reactivates a suspended subscription.</summary>
    Task<Subscription> ReactivateAsync(int subscriptionId, CancellationToken cancellationToken = default);

    #endregion

    #region Billing

    /// <summary>Generates the next invoice for a subscription.</summary>
    Task<Invoice> GenerateInvoiceAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Gets billing history for a subscription.</summary>
    Task<IEnumerable<Invoice>> GetBillingHistoryAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Calculates prorated amount for mid-cycle changes.</summary>
    Task<decimal> CalculateProratedAmountAsync(int subscriptionId, DateTime changeDate, decimal newAmount, CancellationToken cancellationToken = default);

    /// <summary>Gets next billing date for a subscription.</summary>
    Task<DateTime?> GetNextBillingDateAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Updates billing details.</summary>
    Task<Subscription> UpdateBillingDetailsAsync(int subscriptionId, BillingDetails details, CancellationToken cancellationToken = default);

    #endregion

    #region Plan Changes

    /// <summary>Upgrades a subscription to a higher plan.</summary>
    Task<Subscription> UpgradeAsync(int subscriptionId, int newPlanId, bool immediate = true, CancellationToken cancellationToken = default);

    /// <summary>Downgrades a subscription to a lower plan.</summary>
    Task<Subscription> DowngradeAsync(int subscriptionId, int newPlanId, CancellationToken cancellationToken = default);

    /// <summary>Changes subscription plan.</summary>
    Task<Subscription> ChangePlanAsync(int subscriptionId, int newPlanId, SubscriptionChangeType changeType, CancellationToken cancellationToken = default);

    /// <summary>Adds an add-on to a subscription.</summary>
    Task<Subscription> AddAddonAsync(int subscriptionId, int addonId, int quantity = 1, CancellationToken cancellationToken = default);

    /// <summary>Removes an add-on from a subscription.</summary>
    Task<Subscription> RemoveAddonAsync(int subscriptionId, int addonId, CancellationToken cancellationToken = default);

    #endregion

    #region Renewal

    /// <summary>Renews a subscription.</summary>
    Task<Subscription> RenewAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Gets subscriptions due for renewal within days.</summary>
    Task<IEnumerable<Subscription>> GetDueForRenewalAsync(int withinDays, CancellationToken cancellationToken = default);

    /// <summary>Sets auto-renewal preference.</summary>
    Task<Subscription> SetAutoRenewalAsync(int subscriptionId, bool autoRenew, CancellationToken cancellationToken = default);

    #endregion

    #region Usage

    /// <summary>Records usage for a metered subscription.</summary>
    Task<bool> RecordUsageAsync(int subscriptionId, string metricName, decimal quantity, DateTime? timestamp = null, CancellationToken cancellationToken = default);

    /// <summary>Gets usage data for a subscription.</summary>
    Task<SubscriptionUsageData> GetUsageAsync(int subscriptionId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets current usage against limits.</summary>
    Task<IEnumerable<UsageLimit>> GetUsageLimitsAsync(int subscriptionId, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets active subscriptions for an account.</summary>
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(int accountId, CancellationToken cancellationToken = default);
    /// <summary>Gets expiring subscriptions within a date range.</summary>
    Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets subscription statistics.</summary>
    Task<SubscriptionStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Calculates Monthly Recurring Revenue (MRR).</summary>
    Task<decimal> CalculateMRRAsync(CancellationToken cancellationToken = default);

    /// <summary>Calculates Annual Recurring Revenue (ARR).</summary>
    Task<decimal> CalculateARRAsync(CancellationToken cancellationToken = default);

    Task<double> GetChurnRateAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Billing period frequency.
/// </summary>
public enum BillingPeriod
{
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

/// <summary>
/// Billing details for subscription.
/// </summary>
public class BillingDetails
{
    public string? BillingEmail { get; set; }
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string? BillingCountry { get; set; }
    public string? PaymentMethodId { get; set; }
}

/// <summary>
/// Subscription change type.
/// </summary>
public enum SubscriptionChangeType
{
    Immediate,
    EndOfPeriod,
    NextBillingCycle
}

/// <summary>
/// Subscription usage data.
/// </summary>
public class SubscriptionUsageData
{
    public int SubscriptionId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<UsageMetric> Metrics { get; set; } = new();
}

/// <summary>
/// Usage metric detail.
/// </summary>
public class UsageMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal TotalUsage { get; set; }
    public string? Unit { get; set; }
    public List<UsageRecord> Records { get; set; } = new();
}

/// <summary>
/// Individual usage record.
/// </summary>
public class UsageRecord
{
    public DateTime Timestamp { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Usage limit information.
/// </summary>
public class UsageLimit
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining => Limit - Used;
    public double UsagePercentage => Limit > 0 ? (double)(Used / Limit) * 100 : 0;
}

/// <summary>
/// Subscription statistics for reporting.
/// </summary>
public class SubscriptionStatistics
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int PausedSubscriptions { get; set; }
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public double ChurnRate { get; set; }
    public double ConversionRate { get; set; }
    public decimal AverageRevenuePerUser { get; set; }
    public int NewSubscriptionsThisMonth { get; set; }
    public int CancellationsThisMonth { get; set; }
    public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
}
