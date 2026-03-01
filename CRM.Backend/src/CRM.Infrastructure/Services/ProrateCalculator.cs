// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProrateResultDto = CRM.Core.Dtos.ProrateResultDto;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Proration Calculator Service - Handles 4 different proration algorithms.
///
/// FINANCIAL CRITICAL: All calculations use DECIMAL(18,4) for intermediate steps,
/// rounded to DECIMAL(18,2) for storage to prevent floating-point errors.
///
/// Algorithms:
/// 1. ProRata (Time-Based): Amount * (DaysUsed / DaysInCycle)
/// 2. FullPrice: No adjustment, charge full amount
/// 3. OneMonth: Always charge 1 full month
/// 4. None: No adjustment, only charge difference
/// </summary>
public interface IProrateCalculator
{
    Task<ProrateResultDto> CalculateAsync(
        int subscriptionId,
        string prorationType,
        DateTime changeDate,
        CancellationToken cancellationToken);

    /// <summary>Calculate pro-rata amount based on days used.</summary>
    decimal CalculateProRata(decimal fullCycleAmount, DateTime cycleStart, DateTime cycleEnd, DateTime changeDate);

    /// <summary>No proration - return full amount.</summary>
    decimal CalculateFullPrice(decimal fullCycleAmount);

    /// <summary>Always charge one month.</summary>
    decimal CalculateOneMonth(decimal monthlyAmount);

    /// <summary>No proration - charge difference only.</summary>
    decimal CalculateNone(decimal oldAmount, decimal newAmount);
}

/// <summary>
/// Implementation of IProrateCalculator with 4 proration methods.
/// </summary>
public class ProrateCalculator : IProrateCalculator
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ProrateCalculator> _logger;

    public ProrateCalculator(ICrmDbContext context, ILogger<ProrateCalculator> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Orchestrate proration calculation based on algorithm type.
    /// </summary>
    public async Task<ProrateResultDto> CalculateAsync(
        int subscriptionId,
        string prorationType,
        DateTime changeDate,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Subscription {subscriptionId} not found");

        var currentPeriodStart = subscription.CurrentPeriodStart ?? subscription.BillingStartDate ?? DateTime.UtcNow;
        var currentPeriodEnd = subscription.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1);

        decimal proratedAmount = prorationType switch
        {
            "ProRata" => CalculateProRata(subscription.Amount, currentPeriodStart, currentPeriodEnd, changeDate),
            "FullPrice" => CalculateFullPrice(subscription.Amount),
            "OneMonth" => CalculateOneMonth(subscription.Amount),
            "None" => CalculateNone(subscription.Amount, subscription.Amount),
            _ => subscription.Amount
        };

        var daysInCycle = (currentPeriodEnd - currentPeriodStart).Days;
        var daysUsed = (changeDate - currentPeriodStart).Days;

        var result = new ProrateResultDto
        {
            SubscriptionId = subscriptionId,
            ProrationType = prorationType,
            OriginalAmount = subscription.Amount,
            DaysInCycle = daysInCycle,
            DaysUsed = daysUsed,
            ProratedAmount = proratedAmount,
            CreditOrCharge = proratedAmount - subscription.Amount,
            EffectiveDate = changeDate,
            CalculationDetails = $"Period: {currentPeriodStart:yyyy-MM-dd} to {currentPeriodEnd:yyyy-MM-dd}, Days Used: {daysUsed}/{daysInCycle}"
        };

        _logger.LogInformation(
            "Proration calculated: Subscription={SubId}, Type={Type}, Amount={Amount}, Credit/Charge={Charge}",
            subscriptionId, prorationType, proratedAmount, result.CreditOrCharge);

        return result;
    }

    /// <summary>
    /// ProRata Algorithm - Most common, fairest method.
    /// Amount = (FullCycleAmount) * (DaysUsed / DaysInCycle)
    ///
    /// Example: $100/month, cycle = 30 days, change on day 10 = $100 * (10/30) = $33.33
    /// </summary>
    public decimal CalculateProRata(decimal fullCycleAmount, DateTime cycleStart, DateTime cycleEnd, DateTime changeDate)
    {
        if (changeDate < cycleStart)
        {
            return 0m;
        }

        var daysInCycle = (cycleEnd - cycleStart).Days;
        if (daysInCycle <= 0)
        {
            return fullCycleAmount;
        }

        // Ensure changeDate is within cycle
        var effectiveChangeDate = changeDate > cycleEnd ? cycleEnd : changeDate;
        var daysUsed = (effectiveChangeDate - cycleStart).Days + 1; // Include start day

        // Calculate with DECIMAL(18,4) precision, then round to (18,2)
        var dailyRate = fullCycleAmount / daysInCycle;
        var proratedAmount = dailyRate * daysUsed;

        // Round to 2 decimal places for currency
        return Math.Round(proratedAmount, 2);
    }

    /// <summary>
    /// FullPrice Algorithm - No adjustment.
    /// Always charge the full cycle amount regardless of when change occurs.
    /// </summary>
    public decimal CalculateFullPrice(decimal fullCycleAmount)
    {
        return fullCycleAmount;
    }

    /// <summary>
    /// OneMonth Algorithm - Grace period.
    /// Always charge one full month equivalent regardless of days remaining.
    /// Example: Downgrade on day 25 of 30-day month still charged $100 (full month).
    /// </summary>
    public decimal CalculateOneMonth(decimal monthlyAmount)
    {
        return monthlyAmount;
    }

    /// <summary>
    /// None Algorithm - Difference only.
    /// No proration, only charge the difference between old and new plan.
    /// Example: Upgrade from $50 to $100 = $50 additional charge.
    /// </summary>
    public decimal CalculateNone(decimal oldAmount, decimal newAmount)
    {
        var difference = newAmount - oldAmount;
        return difference > 0 ? difference : 0m;
    }
}
