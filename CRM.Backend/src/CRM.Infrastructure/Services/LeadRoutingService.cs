// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Services;

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for automatic lead routing and assignment based on configurable rules.
/// </summary>
public class LeadRoutingService : ILeadRoutingService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadRoutingService> _logger;

    public LeadRoutingService(ICrmDbContext context, ILogger<LeadRoutingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Routing Rule Management

    public async Task<IEnumerable<LeadRoutingRule>> GetAllRulesAsync(
        RoutingRuleStatus? status = null,
        int? teamId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeadRoutingRules
            .Include(r => r.Criteria)
            .Include(r => r.Targets)
            .Include(r => r.Team)
            .Where(r => !r.IsDeleted);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (teamId.HasValue)
            query = query.Where(r => r.TeamId == teamId.Value);

        return await query
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadRoutingRule?> GetRuleByIdAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.LeadRoutingRules
            .Include(r => r.Criteria)
            .Include(r => r.Targets)
            .ThenInclude(t => t.User)
            .Include(r => r.Team)
            .Include(r => r.FallbackOwner)
            .FirstOrDefaultAsync(r => r.Id == ruleId && !r.IsDeleted, cancellationToken);
    }

    public async Task<LeadRoutingRule> CreateRuleAsync(LeadRoutingRule rule, CancellationToken cancellationToken = default)
    {
        rule.CreatedAt = DateTime.UtcNow;
        _context.LeadRoutingRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created lead routing rule {RuleId}: {RuleName}", rule.Id, rule.Name);
        return rule;
    }

    public async Task<LeadRoutingRule> UpdateRuleAsync(LeadRoutingRule rule, CancellationToken cancellationToken = default)
    {
        var existing = await _context.LeadRoutingRules
            .FirstOrDefaultAsync(r => r.Id == rule.Id && !r.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Routing rule {rule.Id} not found");

        // Update fields
        existing.Name = rule.Name;
        existing.Description = rule.Description;
        existing.Status = rule.Status;
        existing.Priority = rule.Priority;
        existing.AssignmentType = rule.AssignmentType;
        existing.AssignToTeam = rule.AssignToTeam;
        existing.TeamId = rule.TeamId;
        existing.FallbackOwnerId = rule.FallbackOwnerId;
        existing.EffectiveStartDate = rule.EffectiveStartDate;
        existing.EffectiveEndDate = rule.EffectiveEndDate;
        existing.BusinessHoursOnly = rule.BusinessHoursOnly;
        existing.Timezone = rule.Timezone;
        existing.SendNotification = rule.SendNotification;
        existing.NotificationTemplateId = rule.NotificationTemplateId;
        existing.NotifyManager = rule.NotifyManager;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated lead routing rule {RuleId}: {RuleName}", rule.Id, rule.Name);
        return existing;
    }

    public async Task<bool> DeleteRuleAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _context.LeadRoutingRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && !r.IsDeleted, cancellationToken);

        if (rule == null)
            return false;

        rule.IsDeleted = true;
        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted lead routing rule {RuleId}: {RuleName}", ruleId, rule.Name);
        return true;
    }

    public async Task<LeadRoutingRule> ActivateRuleAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
        if (rule == null)
            throw new InvalidOperationException($"Routing rule {ruleId} not found");

        rule.Status = RoutingRuleStatus.Active;
        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated lead routing rule {RuleId}: {RuleName}", ruleId, rule.Name);
        return rule;
    }

    public async Task<LeadRoutingRule> DeactivateRuleAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
        if (rule == null)
            throw new InvalidOperationException($"Routing rule {ruleId} not found");

        rule.Status = RoutingRuleStatus.Inactive;
        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated lead routing rule {RuleId}: {RuleName}", ruleId, rule.Name);
        return rule;
    }

    #endregion

    #region Criteria Management

    public async Task<LeadRoutingCriteria> AddCriteriaAsync(
        int ruleId,
        LeadRoutingCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
        if (rule == null)
            throw new InvalidOperationException($"Routing rule {ruleId} not found");

        criteria.LeadRoutingRuleId = ruleId;
        criteria.CreatedAt = DateTime.UtcNow;
        _context.LeadRoutingCriteria.Add(criteria);
        await _context.SaveChangesAsync(cancellationToken);

        return criteria;
    }

    public async Task<LeadRoutingCriteria> UpdateCriteriaAsync(
        LeadRoutingCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.LeadRoutingCriteria
            .FirstOrDefaultAsync(c => c.Id == criteria.Id && !c.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Criteria {criteria.Id} not found");

        existing.CriteriaType = criteria.CriteriaType;
        existing.FieldName = criteria.FieldName;
        existing.Operator = criteria.Operator;
        existing.Value = criteria.Value;
        existing.ValueTo = criteria.ValueTo;
        existing.LogicalOperator = criteria.LogicalOperator;
        existing.Order = criteria.Order;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveCriteriaAsync(int criteriaId, CancellationToken cancellationToken = default)
    {
        var criteria = await _context.LeadRoutingCriteria
            .FirstOrDefaultAsync(c => c.Id == criteriaId && !c.IsDeleted, cancellationToken);

        if (criteria == null)
            return false;

        criteria.IsDeleted = true;
        criteria.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<LeadRoutingCriteria>> GetCriteriaAsync(
        int ruleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeadRoutingCriteria
            .Where(c => c.LeadRoutingRuleId == ruleId && !c.IsDeleted)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Target Management

    public async Task<LeadRoutingTarget> AddTargetAsync(
        int ruleId,
        LeadRoutingTarget target,
        CancellationToken cancellationToken = default)
    {
        var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
        if (rule == null)
            throw new InvalidOperationException($"Routing rule {ruleId} not found");

        target.LeadRoutingRuleId = ruleId;
        target.CreatedAt = DateTime.UtcNow;
        _context.LeadRoutingTargets.Add(target);
        await _context.SaveChangesAsync(cancellationToken);

        return target;
    }

    public async Task<LeadRoutingTarget> UpdateTargetAsync(
        LeadRoutingTarget target,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.LeadRoutingTargets
            .FirstOrDefaultAsync(t => t.Id == target.Id && !t.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Target {target.Id} not found");

        existing.UserId = target.UserId;
        existing.Weight = target.Weight;
        existing.MaxLeadsPerDay = target.MaxLeadsPerDay;
        existing.MaxLeadsPerWeek = target.MaxLeadsPerWeek;
        existing.IsActive = target.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveTargetAsync(int targetId, CancellationToken cancellationToken = default)
    {
        var target = await _context.LeadRoutingTargets
            .FirstOrDefaultAsync(t => t.Id == targetId && !t.IsDeleted, cancellationToken);

        if (target == null)
            return false;

        target.IsDeleted = true;
        target.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<LeadRoutingTarget>> GetTargetsAsync(
        int ruleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeadRoutingTargets
            .Include(t => t.User)
            .Where(t => t.LeadRoutingRuleId == ruleId && !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<TargetCapacity> GetTargetCapacityAsync(
        int targetId,
        CancellationToken cancellationToken = default)
    {
        var target = await _context.LeadRoutingTargets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == targetId && !t.IsDeleted, cancellationToken);

        if (target == null)
            throw new InvalidOperationException($"Target {targetId} not found");

        return new TargetCapacity
        {
            TargetId = target.Id,
            UserId = target.UserId,
            UserName = target.User?.Username ?? string.Empty,
            MaxLeadsPerDay = target.MaxLeadsPerDay,
            MaxLeadsPerWeek = target.MaxLeadsPerWeek,
            LeadsAssignedToday = target.LeadsAssignedToday,
            LeadsAssignedThisWeek = target.LeadsAssignedThisWeek
        };
    }

    #endregion

    #region Lead Routing

    public async Task<LeadRoutingResult> RouteLeadAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, cancellationToken);

        if (lead == null)
            return new LeadRoutingResult { Success = false, LeadId = leadId, ErrorMessage = "Lead not found" };

        // Get matching rules
        var matchingRules = await EvaluateMatchingRulesAsync(leadId, cancellationToken);
        var bestMatch = matchingRules.FirstOrDefault();

        if (bestMatch == null)
        {
            _logger.LogWarning("No matching routing rule found for lead {LeadId}", leadId);
            return new LeadRoutingResult { Success = false, LeadId = leadId, ErrorMessage = "No matching routing rule" };
        }

        return await RouteLeadWithRuleAsync(leadId, bestMatch.Id, cancellationToken);
    }

    public async Task<LeadRoutingResult> RouteLeadWithRuleAsync(
        int leadId,
        int ruleId,
        CancellationToken cancellationToken = default)
    {
        var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
        if (rule == null)
            return new LeadRoutingResult { Success = false, LeadId = leadId, ErrorMessage = $"Rule {ruleId} not found" };

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, cancellationToken);

        if (lead == null)
            return new LeadRoutingResult { Success = false, LeadId = leadId, ErrorMessage = "Lead not found" };

        // Select target based on assignment type
        var selectedTarget = await SelectTargetAsync(rule, cancellationToken);
        if (selectedTarget == null)
        {
            // Use fallback owner
            if (rule.FallbackOwnerId.HasValue)
            {
                return await AssignLeadAsync(lead, rule.FallbackOwnerId.Value, rule, cancellationToken);
            }
            return new LeadRoutingResult { Success = false, LeadId = leadId, ErrorMessage = "No available targets" };
        }

        return await AssignLeadAsync(lead, selectedTarget.UserId, rule, cancellationToken);
    }

    public async Task<IEnumerable<LeadRoutingRule>> EvaluateMatchingRulesAsync(
        int leadId,
        CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, cancellationToken);

        if (lead == null)
            return Enumerable.Empty<LeadRoutingRule>();

        var activeRules = await _context.LeadRoutingRules
            .Include(r => r.Criteria)
            .Where(r => !r.IsDeleted && r.Status == RoutingRuleStatus.Active)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var matchingRules = new List<LeadRoutingRule>();

        foreach (var rule in activeRules)
        {
            // Check effective dates
            if (rule.EffectiveStartDate.HasValue && rule.EffectiveStartDate.Value > now)
                continue;
            if (rule.EffectiveEndDate.HasValue && rule.EffectiveEndDate.Value < now)
                continue;

            // Evaluate criteria
            if (EvaluateRuleCriteria(lead, rule))
            {
                matchingRules.Add(rule);
            }
        }

        return matchingRules;
    }

    public async Task<IEnumerable<LeadRoutingResult>> RouteLeadsBatchAsync(
        IEnumerable<int> leadIds,
        CancellationToken cancellationToken = default)
    {
        var results = new List<LeadRoutingResult>();

        foreach (var leadId in leadIds)
        {
            var result = await RouteLeadAsync(leadId, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<LeadRoutingResult> RerouteLeadAsync(
        int leadId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, cancellationToken);

        if (lead == null)
            return new LeadRoutingResult { Success = false, LeadId = leadId, ErrorMessage = "Lead not found" };

        var previousOwnerId = lead.OwnerId;
        var result = await RouteLeadAsync(leadId, cancellationToken);

        if (result.Success)
        {
            // Log the rerouting
            var log = new LeadRoutingLog
            {
                LeadId = leadId,
                LeadRoutingRuleId = result.MatchedRuleId,
                AssignedToUserId = result.AssignedToUserId,
                PreviousOwnerId = previousOwnerId,
                AssignedAt = DateTime.UtcNow,
                AssignmentType = result.AssignmentType,
                Success = true,
                FailureReason = reason != null ? $"Rerouted: {reason}" : null,
                CreatedAt = DateTime.UtcNow
            };
            _context.LeadRoutingLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rerouted lead {LeadId} from user {PreviousOwner} to user {NewOwner}",
                leadId, previousOwnerId, result.AssignedToUserId);
        }

        return result;
    }

    #endregion

    #region Routing Logs

    public async Task<IEnumerable<LeadRoutingLog>> GetLeadRoutingHistoryAsync(
        int leadId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeadRoutingLogs
            .Include(l => l.LeadRoutingRule)
            .Include(l => l.AssignedToUser)
            .Where(l => l.LeadId == leadId)
            .OrderByDescending(l => l.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeadRoutingLog>> GetRuleRoutingLogsAsync(
        int ruleId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeadRoutingLogs
            .Include(l => l.Lead)
            .Include(l => l.AssignedToUser)
            .Where(l => l.LeadRoutingRuleId == ruleId);

        if (fromDate.HasValue)
            query = query.Where(l => l.AssignedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.AssignedAt <= toDate.Value);

        return await query
            .OrderByDescending(l => l.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeadRoutingLog>> GetUserRoutingLogsAsync(
        int userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeadRoutingLogs
            .Include(l => l.Lead)
            .Include(l => l.LeadRoutingRule)
            .Where(l => l.AssignedToUserId == userId);

        if (fromDate.HasValue)
            query = query.Where(l => l.AssignedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.AssignedAt <= toDate.Value);

        return await query
            .OrderByDescending(l => l.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Analytics

    public async Task<LeadRoutingStatistics> GetRuleStatisticsAsync(
        int ruleId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeadRoutingLogs.Where(l => l.LeadRoutingRuleId == ruleId);

        if (fromDate.HasValue)
            query = query.Where(l => l.AssignedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.AssignedAt <= toDate.Value);

        var logs = await query.ToListAsync(cancellationToken);

        return CalculateStatistics(logs, fromDate, toDate);
    }

    public async Task<LeadRoutingStatistics> GetOverallStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeadRoutingLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.AssignedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.AssignedAt <= toDate.Value);

        var logs = await query
            .Include(l => l.AssignedToUser)
            .ToListAsync(cancellationToken);

        return CalculateStatistics(logs, fromDate, toDate);
    }

    public async Task<ResponseTimeStatistics> GetResponseTimeStatisticsAsync(
        int? ruleId = null,
        int? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LeadRoutingLogs
            .Where(l => l.ResponseTimeSeconds.HasValue);

        if (ruleId.HasValue)
            query = query.Where(l => l.LeadRoutingRuleId == ruleId.Value);
        if (userId.HasValue)
            query = query.Where(l => l.AssignedToUserId == userId.Value);
        if (fromDate.HasValue)
            query = query.Where(l => l.AssignedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.AssignedAt <= toDate.Value);

        var responseTimes = await query
            .Select(l => l.ResponseTimeSeconds!.Value)
            .ToListAsync(cancellationToken);

        if (!responseTimes.Any())
        {
            return new ResponseTimeStatistics();
        }

        var sorted = responseTimes.OrderBy(r => r).ToList();
        var count = sorted.Count;

        return new ResponseTimeStatistics
        {
            AverageResponseTimeSeconds = sorted.Average(),
            MedianResponseTimeSeconds = sorted[count / 2],
            MinResponseTimeSeconds = sorted.Min(),
            MaxResponseTimeSeconds = sorted.Max(),
            TotalLeadsMeasured = count,
            LeadsUnderSLA = await query.CountAsync(l => l.ContactedWithinSLA == true, cancellationToken),
            LeadsOverSLA = await query.CountAsync(l => l.ContactedWithinSLA == false, cancellationToken),
            PercentileP50 = sorted[(int)(count * 0.50)],
            PercentileP90 = sorted[(int)(count * 0.90)],
            PercentileP99 = sorted[Math.Min((int)(count * 0.99), count - 1)]
        };
    }

    #endregion

    #region Capacity Management

    public async Task ResetDailyCountsAsync(CancellationToken cancellationToken = default)
    {
        var targets = await _context.LeadRoutingTargets
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var target in targets)
        {
            target.LeadsAssignedToday = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Reset daily lead counts for {Count} routing targets", targets.Count);
    }

    public async Task ResetWeeklyCountsAsync(CancellationToken cancellationToken = default)
    {
        var targets = await _context.LeadRoutingTargets
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var target in targets)
        {
            target.LeadsAssignedThisWeek = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Reset weekly lead counts for {Count} routing targets", targets.Count);
    }

    #endregion

    #region Private Methods

    private bool EvaluateRuleCriteria(Lead lead, LeadRoutingRule rule)
    {
        if (!rule.Criteria.Any())
            return true; // No criteria means rule matches all leads

        bool overallResult = true;
        string currentOperator = "AND";

        foreach (var criteria in rule.Criteria.OrderBy(c => c.Order))
        {
            bool criteriaResult = EvaluateSingleCriteria(lead, criteria);

            if (currentOperator == "AND")
                overallResult = overallResult && criteriaResult;
            else
                overallResult = overallResult || criteriaResult;

            currentOperator = criteria.LogicalOperator;
        }

        return overallResult;
    }

    private bool EvaluateSingleCriteria(Lead lead, LeadRoutingCriteria criteria)
    {
        string? leadValue = GetLeadFieldValue(lead, criteria.CriteriaType, criteria.FieldName);
        string? criteriaValue = criteria.Value;

        return criteria.Operator.ToLower() switch
        {
            "equals" => string.Equals(leadValue, criteriaValue, StringComparison.OrdinalIgnoreCase),
            "not_equals" => !string.Equals(leadValue, criteriaValue, StringComparison.OrdinalIgnoreCase),
            "contains" => leadValue?.Contains(criteriaValue ?? "", StringComparison.OrdinalIgnoreCase) == true,
            "not_contains" => leadValue?.Contains(criteriaValue ?? "", StringComparison.OrdinalIgnoreCase) != true,
            "starts_with" => leadValue?.StartsWith(criteriaValue ?? "", StringComparison.OrdinalIgnoreCase) == true,
            "ends_with" => leadValue?.EndsWith(criteriaValue ?? "", StringComparison.OrdinalIgnoreCase) == true,
            "greater_than" => CompareNumeric(leadValue, criteriaValue) > 0,
            "less_than" => CompareNumeric(leadValue, criteriaValue) < 0,
            "between" => IsInRange(leadValue, criteriaValue, criteria.ValueTo),
            "in_list" => criteriaValue?.Split(',').Any(v => string.Equals(v.Trim(), leadValue, StringComparison.OrdinalIgnoreCase)) == true,
            "is_empty" => string.IsNullOrWhiteSpace(leadValue),
            "is_not_empty" => !string.IsNullOrWhiteSpace(leadValue),
            _ => false
        };
    }

    private string? GetLeadFieldValue(Lead lead, RoutingCriteriaType criteriaType, string? fieldName)
    {
        return criteriaType switch
        {
            RoutingCriteriaType.LeadSource => lead.Source.ToString(),
            RoutingCriteriaType.LeadScore => lead.Score.ToString(),
            RoutingCriteriaType.Territory => lead.Region,
            RoutingCriteriaType.Industry => lead.CompanyName,
            RoutingCriteriaType.CompanySize => lead.CompanyName,
            RoutingCriteriaType.AnnualRevenue => lead.Score.ToString(),
            RoutingCriteriaType.Campaign => lead.CampaignId?.ToString(),
            RoutingCriteriaType.LeadStatus => lead.Status.ToString(),
            // For custom fields, would need to look up custom field value
            _ => null
        };
    }

    private int CompareNumeric(string? value1, string? value2)
    {
        if (decimal.TryParse(value1, out var num1) && decimal.TryParse(value2, out var num2))
            return num1.CompareTo(num2);
        return 0;
    }

    private bool IsInRange(string? value, string? min, string? max)
    {
        if (!decimal.TryParse(value, out var num))
            return false;
        if (!decimal.TryParse(min, out var minNum))
            return false;
        if (!decimal.TryParse(max, out var maxNum))
            return false;

        return num >= minNum && num <= maxNum;
    }

    private async Task<LeadRoutingTarget?> SelectTargetAsync(
        LeadRoutingRule rule,
        CancellationToken cancellationToken)
    {
        var availableTargets = await _context.LeadRoutingTargets
            .Include(t => t.User)
            .Where(t => t.LeadRoutingRuleId == rule.Id && !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        // Filter by capacity
        availableTargets = availableTargets
            .Where(t => (!t.MaxLeadsPerDay.HasValue || t.LeadsAssignedToday < t.MaxLeadsPerDay.Value) &&
                        (!t.MaxLeadsPerWeek.HasValue || t.LeadsAssignedThisWeek < t.MaxLeadsPerWeek.Value))
            .ToList();

        if (!availableTargets.Any())
            return null;

        return rule.AssignmentType switch
        {
            LeadAssignmentType.RoundRobin => SelectRoundRobin(availableTargets, rule),
            LeadAssignmentType.Weighted => SelectWeighted(availableTargets),
            LeadAssignmentType.Random => availableTargets[Random.Shared.Next(availableTargets.Count)],
            LeadAssignmentType.LoadBalanced => SelectLoadBalanced(availableTargets),
            _ => availableTargets.First()
        };
    }

    private LeadRoutingTarget SelectRoundRobin(List<LeadRoutingTarget> targets, LeadRoutingRule rule)
    {
        var sorted = targets.OrderBy(t => t.Id).ToList();
        var index = rule.RoundRobinPosition % sorted.Count;
        return sorted[index];
    }

    private LeadRoutingTarget SelectWeighted(List<LeadRoutingTarget> targets)
    {
        var totalWeight = targets.Sum(t => t.Weight);
        var randomValue = Random.Shared.Next(totalWeight);
        var cumulative = 0;

        foreach (var target in targets)
        {
            cumulative += target.Weight;
            if (randomValue < cumulative)
                return target;
        }

        return targets.Last();
    }

    private LeadRoutingTarget SelectLoadBalanced(List<LeadRoutingTarget> targets)
    {
        // Select target with lowest current load
        return targets.OrderBy(t => t.LeadsAssignedToday).First();
    }

    private async Task<LeadRoutingResult> AssignLeadAsync(
        Lead lead,
        int assignToUserId,
        LeadRoutingRule rule,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == assignToUserId && !u.IsDeleted, cancellationToken);

        if (user == null)
            return new LeadRoutingResult { Success = false, LeadId = lead.Id, ErrorMessage = "Assigned user not found" };

        // Update lead
        lead.OwnerId = assignToUserId;
        lead.UpdatedAt = DateTime.UtcNow;

        // Update rule round robin position
        rule.RoundRobinPosition++;
        rule.LastAssignmentDate = DateTime.UtcNow;
        rule.TotalLeadsAssigned++;

        // Update target counts
        var target = await _context.LeadRoutingTargets
            .FirstOrDefaultAsync(t => t.LeadRoutingRuleId == rule.Id && t.UserId == assignToUserId && !t.IsDeleted, cancellationToken);

        if (target != null)
        {
            target.LeadsAssignedToday++;
            target.LeadsAssignedThisWeek++;
            target.TotalLeadsAssigned++;
            target.LastAssignmentDate = DateTime.UtcNow;
        }

        // Create routing log
        var log = new LeadRoutingLog
        {
            LeadId = lead.Id,
            LeadRoutingRuleId = rule.Id,
            AssignedToUserId = assignToUserId,
            AssignedAt = DateTime.UtcNow,
            AssignmentType = rule.AssignmentType,
            Success = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.LeadRoutingLogs.Add(log);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Routed lead {LeadId} to user {UserId} via rule {RuleId}",
            lead.Id, assignToUserId, rule.Id);

        return new LeadRoutingResult
        {
            Success = true,
            LeadId = lead.Id,
            AssignedToUserId = assignToUserId,
            AssignedToUserName = user.Username,
            MatchedRuleId = rule.Id,
            MatchedRuleName = rule.Name,
            AssignmentType = rule.AssignmentType,
            RoutingLogId = log.Id
        };
    }

    private LeadRoutingStatistics CalculateStatistics(List<LeadRoutingLog> logs, DateTime? fromDate, DateTime? toDate)
    {
        var stats = new LeadRoutingStatistics
        {
            TotalLeadsRouted = logs.Count,
            SuccessfulRoutes = logs.Count(l => l.Success),
            FailedRoutes = logs.Count(l => !l.Success),
            LeadsContactedWithinSLA = logs.Count(l => l.ContactedWithinSLA == true),
            FromDate = fromDate,
            ToDate = toDate
        };

        if (logs.Any(l => l.ResponseTimeSeconds.HasValue))
        {
            stats.AverageResponseTimeSeconds = logs
                .Where(l => l.ResponseTimeSeconds.HasValue)
                .Average(l => l.ResponseTimeSeconds!.Value);
        }

        // Group by assignment type
        stats.RoutesByAssignmentType = logs
            .GroupBy(l => l.AssignmentType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by user
        stats.RoutesByUser = logs
            .Where(l => l.AssignedToUser != null)
            .GroupBy(l => l.AssignedToUser!.Username)
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    #endregion
}
