// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

using CRM.Core.Entities;

/// <summary>
/// Service for automatic lead routing and assignment based on configurable rules.
/// </summary>
public interface ILeadRoutingService
{
    #region Routing Rule Management

    /// <summary>
    /// Get all routing rules with optional filtering.
    /// </summary>
    Task<IEnumerable<LeadRoutingRule>> GetAllRulesAsync(
        RoutingRuleStatus? status = null,
        int? teamId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific routing rule by ID.
    /// </summary>
    Task<LeadRoutingRule?> GetRuleByIdAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new routing rule.
    /// </summary>
    Task<LeadRoutingRule> CreateRuleAsync(LeadRoutingRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing routing rule.
    /// </summary>
    Task<LeadRoutingRule> UpdateRuleAsync(LeadRoutingRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a routing rule (soft delete).
    /// </summary>
    Task<bool> DeleteRuleAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a routing rule.
    /// </summary>
    Task<LeadRoutingRule> ActivateRuleAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a routing rule.
    /// </summary>
    Task<LeadRoutingRule> DeactivateRuleAsync(int ruleId, CancellationToken cancellationToken = default);

    #endregion

    #region Criteria Management

    /// <summary>
    /// Add criteria to a routing rule.
    /// </summary>
    Task<LeadRoutingCriteria> AddCriteriaAsync(
        int ruleId,
        LeadRoutingCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update criteria on a routing rule.
    /// </summary>
    Task<LeadRoutingCriteria> UpdateCriteriaAsync(
        LeadRoutingCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove criteria from a routing rule.
    /// </summary>
    Task<bool> RemoveCriteriaAsync(int criteriaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all criteria for a rule.
    /// </summary>
    Task<IEnumerable<LeadRoutingCriteria>> GetCriteriaAsync(
        int ruleId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Target Management

    /// <summary>
    /// Add a target user to a routing rule.
    /// </summary>
    Task<LeadRoutingTarget> AddTargetAsync(
        int ruleId,
        LeadRoutingTarget target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a routing target.
    /// </summary>
    Task<LeadRoutingTarget> UpdateTargetAsync(
        LeadRoutingTarget target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a target from a routing rule.
    /// </summary>
    Task<bool> RemoveTargetAsync(int targetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all targets for a rule.
    /// </summary>
    Task<IEnumerable<LeadRoutingTarget>> GetTargetsAsync(
        int ruleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available capacity for a target user.
    /// </summary>
    Task<TargetCapacity> GetTargetCapacityAsync(
        int targetId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Lead Routing

    /// <summary>
    /// Route a lead through the routing rules and assign to appropriate owner.
    /// </summary>
    Task<LeadRoutingResult> RouteLeadAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Route a lead using specific routing rule (bypass matching).
    /// </summary>
    Task<LeadRoutingResult> RouteLeadWithRuleAsync(
        int leadId,
        int ruleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluate which rules match a lead (without routing).
    /// </summary>
    Task<IEnumerable<LeadRoutingRule>> EvaluateMatchingRulesAsync(
        int leadId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Route multiple leads in batch.
    /// </summary>
    Task<IEnumerable<LeadRoutingResult>> RouteLeadsBatchAsync(
        IEnumerable<int> leadIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-route a lead (for reassignment).
    /// </summary>
    Task<LeadRoutingResult> RerouteLeadAsync(
        int leadId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Routing Logs

    /// <summary>
    /// Get routing history for a lead.
    /// </summary>
    Task<IEnumerable<LeadRoutingLog>> GetLeadRoutingHistoryAsync(
        int leadId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get routing logs for a rule.
    /// </summary>
    Task<IEnumerable<LeadRoutingLog>> GetRuleRoutingLogsAsync(
        int ruleId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get routing logs for a user.
    /// </summary>
    Task<IEnumerable<LeadRoutingLog>> GetUserRoutingLogsAsync(
        int userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Analytics

    /// <summary>
    /// Get routing statistics for a rule.
    /// </summary>
    Task<LeadRoutingStatistics> GetRuleStatisticsAsync(
        int ruleId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overall routing statistics.
    /// </summary>
    Task<LeadRoutingStatistics> GetOverallStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get response time statistics for routed leads.
    /// </summary>
    Task<ResponseTimeStatistics> GetResponseTimeStatisticsAsync(
        int? ruleId = null,
        int? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Capacity Management

    /// <summary>
    /// Reset daily lead counts for all targets.
    /// </summary>
    Task ResetDailyCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset weekly lead counts for all targets.
    /// </summary>
    Task ResetWeeklyCountsAsync(CancellationToken cancellationToken = default);

    #endregion
}

#region Supporting Types

/// <summary>
/// Result of a lead routing operation.
/// </summary>
public class LeadRoutingResult
{
    public bool Success { get; set; }
    public int LeadId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int? MatchedRuleId { get; set; }
    public string? MatchedRuleName { get; set; }
    public LeadAssignmentType AssignmentType { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime RoutedAt { get; set; } = DateTime.UtcNow;
    public int? RoutingLogId { get; set; }
}

/// <summary>
/// Target user capacity information.
/// </summary>
public class TargetCapacity
{
    public int TargetId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int? MaxLeadsPerDay { get; set; }
    public int? MaxLeadsPerWeek { get; set; }
    public int LeadsAssignedToday { get; set; }
    public int LeadsAssignedThisWeek { get; set; }
    public int RemainingDailyCapacity => MaxLeadsPerDay.HasValue ? MaxLeadsPerDay.Value - LeadsAssignedToday : int.MaxValue;
    public int RemainingWeeklyCapacity => MaxLeadsPerWeek.HasValue ? MaxLeadsPerWeek.Value - LeadsAssignedThisWeek : int.MaxValue;
    public bool HasCapacity => RemainingDailyCapacity > 0 && RemainingWeeklyCapacity > 0;
}

/// <summary>
/// Lead routing statistics.
/// </summary>
public class LeadRoutingStatistics
{
    public int TotalLeadsRouted { get; set; }
    public int SuccessfulRoutes { get; set; }
    public int FailedRoutes { get; set; }
    public double SuccessRate => TotalLeadsRouted > 0 ? (double)SuccessfulRoutes / TotalLeadsRouted * 100 : 0;
    public double AverageResponseTimeSeconds { get; set; }
    public int LeadsContactedWithinSLA { get; set; }
    public double SLAComplianceRate => SuccessfulRoutes > 0 ? (double)LeadsContactedWithinSLA / SuccessfulRoutes * 100 : 0;
    public Dictionary<LeadAssignmentType, int> RoutesByAssignmentType { get; set; } = new();
    public Dictionary<string, int> RoutesByUser { get; set; } = new();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Response time statistics for routed leads.
/// </summary>
public class ResponseTimeStatistics
{
    public double AverageResponseTimeSeconds { get; set; }
    public double MedianResponseTimeSeconds { get; set; }
    public double MinResponseTimeSeconds { get; set; }
    public double MaxResponseTimeSeconds { get; set; }
    public int TotalLeadsMeasured { get; set; }
    public int LeadsUnderSLA { get; set; }
    public int LeadsOverSLA { get; set; }
    public double PercentileP50 { get; set; }
    public double PercentileP90 { get; set; }
    public double PercentileP99 { get; set; }
}

#endregion
