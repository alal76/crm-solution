// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for commission rules engine.
/// Handles complex commission calculations with caps, splits, and trigger logic.
/// </summary>
public interface ICommissionRulesEngine
{
    /// <summary>
    /// Calculates commission for an opportunity.
    /// Applies applicable rules, caps, splits, and trigger conditions.
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="userId">User/salesperson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Commission calculation result</returns>
    Task<CommissionCalculationResultDto> CalculateCommissionAsync(
        int opportunityId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates commission for a deal amount with specific rule.
    /// </summary>
    /// <param name="ruleId">Commission rule ID to apply</param>
    /// <param name="dealAmount">Deal amount</param>
    /// <param name="userId">User/salesperson ID</param>
    /// <param name="teamMemberIds">Team member IDs for split calculation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Commission calculation result</returns>
    Task<CommissionCalculationResultDto> CalculateWithRuleAsync(
        int ruleId,
        decimal dealAmount,
        int userId,
        int[]? teamMemberIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies commission cap to calculated amount.
    /// </summary>
    /// <param name="calculatedAmount">Initially calculated commission</param>
    /// <param name="maxCommissionAmount">Maximum allowed commission</param>
    /// <returns>Capped commission amount</returns>
    decimal ApplyCap(decimal calculatedAmount, decimal? maxCommissionAmount);

    /// <summary>
    /// Calculates split commission for team sales.
    /// </summary>
    /// <param name="totalCommission">Total commission amount</param>
    /// <param name="splitPercentages">Dictionary of user ID to split percentage</param>
    /// <returns>Dictionary of user ID to commission amount</returns>
    Dictionary<int, decimal> CalculateSplit(decimal totalCommission, Dictionary<int, decimal> splitPercentages);

    /// <summary>
    /// Checks if commission trigger conditions are met.
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="trigger">Trigger event to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if trigger conditions are met</returns>
    Task<bool> CheckTriggerConditionsAsync(
        int opportunityId,
        CRM.Core.Enums.CommissionTriggerEvent trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets applicable commission rules for a user and deal.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="dealAmount">Deal amount</param>
    /// <param name="productIds">Product IDs in the deal</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of applicable commission rules</returns>
    Task<IEnumerable<CRM.Core.Entities.CommissionRule>> GetApplicableRulesAsync(
        int userId,
        decimal dealAmount,
        int[]? productIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates tiered commission based on amount brackets.
    /// </summary>
    /// <param name="dealAmount">Deal amount</param>
    /// <param name="tiers">Tier definitions (min, max, rate)</param>
    /// <returns>Calculated tiered commission</returns>
    decimal CalculateTieredCommission(decimal dealAmount, IEnumerable<CommissionTierDto> tiers);
}

/// <summary>
/// DTO for commission tier calculation.
/// </summary>
public class CommissionTierDto
{
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal? FixedAmount { get; set; }
}
