// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Proration Calculator - Calculates partial-month charges when subscriptions
/// are added/removed mid-cycle or plans are changed.
/// 
/// Supports 4 proration algorithms:
/// 1. ProrateCreditMethod: Full month credit applied to next cycle
/// 2. ProrateDaysMethod: Days used / total days in month
/// 3. ProrateCycleMethod: Billing cycle percentage
/// 4. ProrateIntervalMethod: Specific intervals (hourly, weekly, etc)
/// 
/// SPEC: PHASE 6 - Subscription Billing Services (25 hours)
/// </summary>
public interface IProrateCalculator
{
    /// <summary>
    /// Calculate prorated credit using full-month credit method.
    /// Applicable when subscription ends mid-cycle.
    /// Credit is applied to the customer's next billing cycle.
    /// </summary>
    /// <param name="monthlyAmount">Full monthly subscription amount</param>
    /// <param name="changeDate">Date when subscription ended/changed</param>
    /// <param name="billingPeriod">Billing period info (start, end, interval)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Credit amount to apply</returns>
    Task<ProrateResultDto> CalculateCreditByFullMonthAsync(
        decimal monthlyAmount,
        DateTime changeDate,
        BillingPeriodDto billingPeriod,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculate prorated charge using days-used method.
    /// Charge = (Days Used / Total Days in Period) × Monthly Amount
    /// Applicable when subscription starts or ends mid-cycle.
    /// </summary>
    /// <param name="monthlyAmount">Full monthly subscription amount</param>
    /// <param name="changeDate">Date when subscription started/ended</param>
    /// <param name="billingPeriod">Billing period info</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prorated charge amount</returns>
    Task<ProrateResultDto> CalculateChargeByDaysAsync(
        decimal monthlyAmount,
        DateTime changeDate,
        BillingPeriodDto billingPeriod,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculate prorated amount using billing cycle method.
    /// Prorates based on position in current billing cycle percentage.
    /// Applicable for mid-cycle plan upgrades/downgrades.
    /// </summary>
    /// <param name="monthlyAmount">Full monthly subscription amount</param>
    /// <param name="changeDate">Date of plan change</param>
    /// <param name="billingPeriod">Billing period info</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prorated amount adjustment</returns>
    Task<ProrateResultDto> CalculateByBillingCycleAsync(
        decimal monthlyAmount,
        DateTime changeDate,
        BillingPeriodDto billingPeriod,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculate prorated amount using specific interval method.
    /// Supports hourly, daily, weekly, or custom intervals within a month.
    /// Most precise for subscriptions with non-standard billing cycles.
    /// </summary>
    /// <param name="monthlyAmount">Full monthly subscription amount</param>
    /// <param name="changeDate">Date of change</param>
    /// <param name="interval">Proration interval (Hourly, Daily, Weekly)</param>
    /// <param name="billingPeriod">Billing period info</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prorated amount by interval</returns>
    Task<ProrateResultDto> CalculateByIntervalAsync(
        decimal monthlyAmount,
        DateTime changeDate,
        ProrateInterval interval,
        BillingPeriodDto billingPeriod,
        CancellationToken cancellationToken);

    /// <summary>
    /// Determine which proration method to use based on business rules.
    /// Can be overridden per subscription or account.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID to lookup configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configured proration method</returns>
    Task<ProrateInterval> GetConfiguredMethodAsync(
        int subscriptionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validate prorations for a plan change (upgrade/downgrade).
    /// Returns additional charge or credit owed for the change.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="oldAmount">Current (old) plan amount</param>
    /// <param name="newAmount">New plan amount</param>
    /// <param name="changeDate">Date of change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prorated adjustment (+ charge, - credit)</returns>
    Task<ProrateResultDto> CalculatePlanChangeAdjustmentAsync(
        int subscriptionId,
        decimal oldAmount,
        decimal newAmount,
        DateTime changeDate,
        CancellationToken cancellationToken);
}

/// <summary>
/// Billing period information for proration calculations.
/// </summary>
public class BillingPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysInPeriod => (EndDate.Date - StartDate.Date).Days + 1;
}

/// <summary>
/// Result of proration calculation.
/// </summary>
public class ProrateResultDto
{
    public decimal Amount { get; set; }
    public decimal MonthlyAmount { get; set; }
    public ProrateInterval Method { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysIncluded { get; set; }
    public string CalculationDetails { get; set; } = string.Empty;
}

/// <summary>
/// Proration interval types.
/// </summary>
public enum ProrateInterval
{
    FullMonth = 0,  // Credit method - full month carries over
    Days = 1,       // Days-used method
    Cycle = 2,      // Billing cycle percentage
    Hourly = 3,     // Hourly interval
    Daily = 4,      // Daily interval
    Weekly = 5,     // Weekly interval
    None = 6        // No proration (full month charge)
}
