// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for commission management operations.
/// Handles commission calculations, tracking, and payouts.
/// </summary>
public interface ICommissionService
{
    #region CRUD Operations

    /// <summary>Gets all commissions with optional filtering.</summary>
    Task<IEnumerable<Commission>> GetAllAsync(
        int? userId = null,
        CommissionStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a commission by ID.</summary>
    Task<Commission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new commission record.</summary>
    Task<Commission> CreateAsync(Commission commission, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing commission.</summary>
    Task<Commission> UpdateAsync(Commission commission, CancellationToken cancellationToken = default);

    /// <summary>Deletes a commission (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Commission Calculation

    /// <summary>Calculates commission for a deal/opportunity.</summary>
    Task<CommissionCalculation> CalculateForDealAsync(int opportunityId, CancellationToken cancellationToken = default);

    /// <summary>Calculates commission for an order.</summary>
    Task<CommissionCalculation> CalculateForOrderAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Calculates commission for a sales rep for a period.</summary>
    Task<CommissionSummary> CalculateForPeriodAsync(int userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Recalculates commissions for a deal after changes.</summary>
    Task<Commission> RecalculateAsync(int commissionId, CancellationToken cancellationToken = default);

    #endregion

    #region Commission Plans

    /// <summary>Gets all commission plans.</summary>
    Task<IEnumerable<CommissionPlan>> GetPlansAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a commission plan by ID.</summary>
    Task<CommissionPlan?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken = default);

    /// <summary>Creates a commission plan.</summary>
    Task<CommissionPlan> CreatePlanAsync(CommissionPlan plan, CancellationToken cancellationToken = default);

    /// <summary>Updates a commission plan.</summary>
    Task<CommissionPlan> UpdatePlanAsync(CommissionPlan plan, CancellationToken cancellationToken = default);

    /// <summary>Deletes a commission plan.</summary>
    Task<bool> DeletePlanAsync(int planId, CancellationToken cancellationToken = default);

    /// <summary>Assigns a plan to a user.</summary>
    Task<bool> AssignPlanToUserAsync(int planId, int userId, DateTime? effectiveDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the active plan for a user.</summary>
    Task<CommissionPlan?> GetUserPlanAsync(int userId, CancellationToken cancellationToken = default);

    #endregion

    #region Status Management

    /// <summary>Updates commission status.</summary>
    Task<Commission> UpdateStatusAsync(int commissionId, CommissionStatus status, CancellationToken cancellationToken = default);

    /// <summary>Approves a commission for payout.</summary>
    Task<Commission> ApproveAsync(int commissionId, int approvedById, CancellationToken cancellationToken = default);

    /// <summary>Rejects a commission.</summary>
    Task<Commission> RejectAsync(int commissionId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Marks a commission as paid.</summary>
    Task<Commission> MarkAsPaidAsync(int commissionId, DateTime? paidDate = null, CancellationToken cancellationToken = default);

    /// <summary>Claws back a paid commission.</summary>
    Task<Commission> ClawbackAsync(int commissionId, string reason, CancellationToken cancellationToken = default);

    #endregion

    #region Statements

    /// <summary>Generates a commission statement for a user.</summary>
    Task<CommissionStatement> GenerateStatementAsync(int userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets commission statements for a user.</summary>
    Task<IEnumerable<CommissionStatement>> GetStatementsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets a statement by ID.</summary>
    Task<CommissionStatement?> GetStatementByIdAsync(int statementId, CancellationToken cancellationToken = default);

    /// <summary>Finalizes a statement for payout.</summary>
    Task<CommissionStatement> FinalizeStatementAsync(int statementId, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets commissions for a user.</summary>
    Task<IEnumerable<Commission>> GetByUserAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets pending commissions awaiting approval.</summary>
    Task<IEnumerable<Commission>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets commissions ready for payout.</summary>
    Task<IEnumerable<Commission>> GetReadyForPayoutAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets commission statistics.</summary>
    Task<CommissionStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets commission leaderboard.</summary>
    Task<IEnumerable<CommissionLeaderboard>> GetLeaderboardAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets commission forecast.</summary>
    Task<CommissionForecast> GetForecastAsync(int userId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Tiers

    /// <summary>Gets tiers for a commission plan.</summary>
    Task<IEnumerable<CommissionTier>> GetTiersAsync(int planId, CancellationToken cancellationToken = default);

    /// <summary>Adds a tier to a plan.</summary>
    Task<CommissionTier> AddTierAsync(int planId, CommissionTier tier, CancellationToken cancellationToken = default);

    /// <summary>Updates a tier.</summary>
    Task<CommissionTier> UpdateTierAsync(CommissionTier tier, CancellationToken cancellationToken = default);

    /// <summary>Removes a tier.</summary>
    Task<bool> RemoveTierAsync(int tierId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Commission calculation result.
/// </summary>
public class CommissionCalculation
{
    public int UserId { get; set; }
    public int? OpportunityId { get; set; }
    public int? OrderId { get; set; }
    public int? PlanId { get; set; }
    public string? PlanName { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CalculatedAmount { get; set; }
    public decimal? Accelerator { get; set; }
    public decimal FinalAmount { get; set; }
    public int? TierLevel { get; set; }
    public string? TierName { get; set; }
    public List<CommissionBreakdown> Breakdown { get; set; } = new();
}

/// <summary>
/// Commission calculation breakdown.
/// </summary>
public class CommissionBreakdown
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public decimal Result { get; set; }
}

/// <summary>
/// Commission summary for a period.
/// </summary>
public class CommissionSummary
{
    public int UserId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalClawedBack { get; set; }
    public int DealCount { get; set; }
    public decimal AverageCommission { get; set; }
    public List<Commission> Commissions { get; set; } = new();
}

/// <summary>
/// Commission statistics.
/// </summary>
public class CommissionStatistics
{
    public decimal TotalCommissions { get; set; }
    /// <summary>Alias for TotalCommissions for compatibility</summary>
    public decimal TotalAmount { get => TotalCommissions; set => TotalCommissions = value; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public int TotalRecords { get; set; }
    /// <summary>Alias for TotalRecords</summary>
    public int CommissionCount { get => TotalRecords; set => TotalRecords = value; }
    public int PendingApprovals { get; set; }
    public decimal AverageCommission { get; set; }
    public int ActivePlans { get; set; }
    public Dictionary<string, decimal> CommissionsByPlan { get; set; } = new();
}

/// <summary>
/// Commission leaderboard entry.
/// </summary>
public class CommissionLeaderboard
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalEarned { get; set; }
    public int DealCount { get; set; }
    public decimal AverageDealSize { get; set; }
}

/// <summary>
/// Commission forecast.
/// </summary>
public class CommissionForecast
{
    public int UserId { get; set; }
    public decimal CurrentEarned { get; set; }
    public decimal ForecastedEarnings { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal ExpectedFromPipeline { get; set; }
    public decimal QuotaProgress { get; set; }
    public decimal ProjectedQuotaAttainment { get; set; }
    public List<ForecastedDeal> ForecastedDeals { get; set; } = new();
}

/// <summary>
/// Forecasted deal for commission.
/// </summary>
public class ForecastedDeal
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public decimal DealValue { get; set; }
    public decimal ExpectedCommission { get; set; }
    public double Probability { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
}
