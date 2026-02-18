// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Commission Rule Evaluation Service - Advanced commission calculations with tiers, caps,
/// minimums, clawback, split commissions, and trigger-based rules.
/// 
/// Features:
/// - Tiered commission rates based on deal amount
/// - Commission caps and minimums per deal
/// - Clawback on deal churn
/// - Split commissions across multiple sales reps
/// - Trigger-based rules (deal close, payment received, subscription renewal)
/// 
/// SPEC: PHASE 5 - Commission Rules Advanced (20 hours)
/// </summary>
public interface ICommissionRuleEvaluationService
{
    /// <summary>
    /// Evaluate commission rule for a deal and sales rep based on trigger event.
    /// Determines applicable commission rate, tier, and amount.
    /// </summary>
    /// <param name="dealId">Opportunity ID</param>
    /// <param name="salesRepId">Sales representative ID</param>
    /// <param name="trigger">Trigger event (DealClose, PaymentReceived, SubscriptionRenewal)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Commission evaluation result with amount and breakdown</returns>
    Task<CommissionEvaluationDto> EvaluateCommissionRuleAsync(
        int dealId,
        int salesRepId,
        string trigger,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculate tiered commission based on deal amount and rule configuration.
    /// </summary>
    /// <param name="amount">Deal amount in base currency</param>
    /// <param name="ruleId">Commission rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tiered commission calculation with tier breakdown</returns>
    Task<TieredCommissionDto> CalculateTieredCommissionAsync(
        decimal amount,
        int ruleId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Apply caps and minimums to base commission amount.
    /// </summary>
    /// <param name="baseCommission">Base commission amount before caps</param>
    /// <param name="ruleId">Commission rule ID for cap/minimum lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Final commission after applying caps and minimums</returns>
    Task<decimal> ApplyCapsAndMinimumsAsync(
        decimal baseCommission,
        int ruleId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Process split commission across multiple sales representatives.
    /// </summary>
    /// <param name="dealId">Opportunity ID</param>
    /// <param name="repIds">List of sales rep IDs</param>
    /// <param name="splitPercentages">Percentages for each rep (must sum to 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Split commission allocation per rep</returns>
    Task<List<SplitCommissionDto>> ProcessSplitCommissionAsync(
        int dealId,
        List<int> repIds,
        List<decimal> splitPercentages,
        CancellationToken cancellationToken);

    /// <summary>
    /// Apply clawback penalty for deal churn (recovery of commissions paid out).
    /// </summary>
    /// <param name="dealId">Opportunity ID</param>
    /// <param name="originalCommission">Original commission amount paid</param>
    /// <param name="clawbackPercentage">Percentage to claw back (e.g., 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Clawback amount owed back</returns>
    Task<ClawbackDto> ApplyClawbackAsync(
        int dealId,
        decimal originalCommission,
        decimal clawbackPercentage,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get commission history for a sales representative in a date range.
    /// </summary>
    /// <param name="salesRepId">Sales rep ID</param>
    /// <param name="startDate">Period start date</param>
    /// <param name="endDate">Period end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of commission transactions in period</returns>
    Task<List<CommissionHistoryDto>> GetCommissionHistoryAsync(
        int salesRepId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get commission summary for a sales rep (total earned, pending, approved, paid).
    /// </summary>
    /// <param name="salesRepId">Sales rep ID</param>
    /// <param name="period">Period to summarize (YearMonth e.g., "2026-02")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Commission summary with breakdown</returns>
    Task<CommissionSummaryDto> GetCommissionSummaryAsync(
        int salesRepId,
        string period,
        CancellationToken cancellationToken);
}

/// <summary>
/// Commission evaluation result with breakdown of calculation.
/// </summary>
public class CommissionEvaluationDto
{
    public int DealId { get; set; }
    public int SalesRepId { get; set; }
    public decimal DealAmount { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public decimal BaseCommissionRate { get; set; }
    public decimal TierCommissionRate { get; set; }
    public decimal BaseCommissionAmount { get; set; }
    public decimal TierCommissionAmount { get; set; }
    public decimal FinalCommissionAmount { get; set; }
    public string TierName { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
}

/// <summary>
/// Tiered commission calculation result.
/// </summary>
public class TieredCommissionDto
{
    public decimal Amount { get; set; }
    public int RuleId { get; set; }
    public List<CommissionTierCalculationDto> Tiers { get; set; } = new();
    public decimal TotalCommission { get; set; }
}

/// <summary>
/// Individual commission tier for calculation results.
/// </summary>
public class CommissionTierCalculationDto
{
    public int TierNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal CommissionAmount { get; set; }
}

/// <summary>
/// Split commission allocation.
/// </summary>
public class SplitCommissionDto
{
    public int SalesRepId { get; set; }
    public string SalesRepName { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
}

/// <summary>
/// Clawback penalty details.
/// </summary>
public class ClawbackDto
{
    public int DealId { get; set; }
    public int SalesRepId { get; set; }
    public decimal OriginalCommission { get; set; }
    public decimal ClawbackPercentage { get; set; }
    public decimal ClawbackAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ClawbackDate { get; set; }
}

/// <summary>
/// Commission history entry for a sales rep.
/// </summary>
public class CommissionHistoryDto
{
    public int CommissionId { get; set; }
    public int DealId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Commission summary for a period.
/// </summary>
public class CommissionSummaryDto
{
    public int SalesRepId { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal TotalEarned { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalApproved { get; set; }
    public decimal TotalPaid { get; set; }
    public int DealCount { get; set; }
}
