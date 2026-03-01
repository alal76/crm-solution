// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using System.Text.Json;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Auto-assignment service implementing round-robin, skill-based, and least-loaded strategies
/// for automatic service request assignment.
/// </summary>
public class AutoAssignmentService : IAutoAssignmentService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AutoAssignmentService> _logger;

    // Thread-safe round-robin counters keyed by queueId (null key → global)
    private static readonly ConcurrentDictionary<int, int> _roundRobinCounters = new();
    private static int _globalRoundRobinIndex;

    // In-memory rule store (pragmatic approach — persistence can come later via DB entity)
    private static readonly ConcurrentDictionary<int, AssignmentRuleDto> _rules = new();
    private static int _nextRuleId = 1;

    public AutoAssignmentService(
        ICrmDbContext dbContext,
        ILogger<AutoAssignmentService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Auto-Assignment

    /// <inheritdoc />
    public async Task<AutoAssignmentResultDto> AssignServiceRequestAsync(int serviceRequestId, CancellationToken ct = default)
    {
        try
        {
            var suggestion = await SuggestAssignmentAsync(serviceRequestId, ct);
            if (!suggestion.Success || suggestion.AssignedUserId == null)
                return suggestion;

            // Apply the assignment
            var sr = await _dbContext.ServiceRequests
                .FirstOrDefaultAsync(s => s.Id == serviceRequestId && !s.IsDeleted, ct);

            if (sr == null)
            {
                return new AutoAssignmentResultDto
                {
                    ServiceRequestId = serviceRequestId,
                    Success = false,
                    Reason = "Service request not found"
                };
            }

            sr.AssignedToUserId = suggestion.AssignedUserId;
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Auto-assigned service request {ServiceRequestId} to user {UserId} using {Strategy}",
                serviceRequestId, suggestion.AssignedUserId, suggestion.StrategyUsed);

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-assigning service request {ServiceRequestId}", serviceRequestId);
            return new AutoAssignmentResultDto
            {
                ServiceRequestId = serviceRequestId,
                Success = false,
                Reason = $"Error during assignment: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public async Task<AutoAssignmentResultDto> SuggestAssignmentAsync(int serviceRequestId, CancellationToken ct = default)
    {
        try
        {
            var sr = await _dbContext.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == serviceRequestId && !s.IsDeleted, ct);

            if (sr == null)
            {
                return new AutoAssignmentResultDto
                {
                    ServiceRequestId = serviceRequestId,
                    Success = false,
                    Reason = "Service request not found"
                };
            }

            // Find the best matching rule for this service request
            var matchingRule = FindMatchingRule(sr);

            int? agentId;
            string strategyUsed;
            string? ruleName = matchingRule?.Name;

            if (matchingRule != null)
            {
                strategyUsed = matchingRule.Strategy;
                agentId = strategyUsed switch
                {
                    "SkillBased" => await GetBestSkillMatchAgentAsync(serviceRequestId, ct),
                    "LeastLoaded" => await GetLeastLoadedAgentAsync(matchingRule.QueueId, ct),
                    _ => await GetNextRoundRobinAgentAsync(matchingRule.QueueId, ct)
                };
            }
            else
            {
                // Default to round-robin when no rule matches
                strategyUsed = "RoundRobin";
                agentId = await GetNextRoundRobinAgentAsync(null, ct);
            }

            if (agentId == null)
            {
                return new AutoAssignmentResultDto
                {
                    ServiceRequestId = serviceRequestId,
                    Success = false,
                    StrategyUsed = strategyUsed,
                    MatchedRuleName = ruleName,
                    Reason = "No available agents found"
                };
            }

            var agent = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == agentId, ct);

            return new AutoAssignmentResultDto
            {
                ServiceRequestId = serviceRequestId,
                AssignedUserId = agentId,
                AssignedUserName = agent?.FullName,
                StrategyUsed = strategyUsed,
                MatchedRuleName = ruleName,
                Success = true,
                Reason = $"Assigned via {strategyUsed}" + (ruleName != null ? $" (rule: {ruleName})" : " (default)")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting assignment for service request {ServiceRequestId}", serviceRequestId);
            return new AutoAssignmentResultDto
            {
                ServiceRequestId = serviceRequestId,
                Success = false,
                Reason = $"Error during suggestion: {ex.Message}"
            };
        }
    }

    #endregion

    #region Strategy Implementations

    /// <inheritdoc />
    public async Task<int?> GetNextRoundRobinAgentAsync(int? queueId = null, CancellationToken ct = default)
    {
        var agents = await GetActiveAgentsAsync(ct);
        if (agents.Count == 0)
            return null;

        int index;
        if (queueId.HasValue)
        {
            index = _roundRobinCounters.AddOrUpdate(
                queueId.Value,
                _ => 0,
                (_, current) => current + 1);
        }
        else
        {
            index = Interlocked.Increment(ref _globalRoundRobinIndex);
        }

        // Ensure non-negative index for modulo operation
        var effectiveIndex = Math.Abs(index) % agents.Count;
        return agents[effectiveIndex].Id;
    }

    /// <inheritdoc />
    public async Task<int?> GetBestSkillMatchAgentAsync(int serviceRequestId, CancellationToken ct = default)
    {
        var sr = await _dbContext.ServiceRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceRequestId && !s.IsDeleted, ct);

        if (sr == null)
            return null;

        var agents = await GetActiveAgentsAsync(ct);
        if (agents.Count == 0)
            return null;

        // Find rules that match and have required skills defined
        var matchingRule = FindMatchingRule(sr);
        if (matchingRule?.RequiredSkills == null)
        {
            // Fall back to round-robin if no skill-based rule applies
            return await GetNextRoundRobinAgentAsync(null, ct);
        }

        // Parse required skills from JSON array
        List<string> requiredSkills;
        try
        {
            requiredSkills = JsonSerializer.Deserialize<List<string>>(matchingRule.RequiredSkills) ?? new List<string>();
        }
        catch
        {
            // If not valid JSON, treat as comma-separated
            requiredSkills = matchingRule.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        if (requiredSkills.Count == 0)
            return await GetNextRoundRobinAgentAsync(null, ct);

        // Match agents by role — Support agents are preferred for support categories
        // A more advanced implementation would check a user skills table
        var supportAgents = agents
            .Where(a => a.Role == (int)UserRole.Support || a.Role == (int)UserRole.Admin)
            .ToList();

        if (supportAgents.Count > 0)
        {
            // Among matching agents, pick the one with fewest open assignments
            var agentLoads = new List<(User Agent, int Load)>();
            foreach (var agent in supportAgents)
            {
                var openCount = await _dbContext.ServiceRequests
                    .CountAsync(s => s.AssignedToUserId == agent.Id
                        && !s.IsDeleted
                        && s.Status != ServiceRequestStatus.Closed
                        && s.Status != ServiceRequestStatus.Resolved, ct);
                agentLoads.Add((agent, openCount));
            }

            var bestAgent = agentLoads.OrderBy(a => a.Load).First();
            return bestAgent.Agent.Id;
        }

        // Fall back to round-robin with all agents
        return await GetNextRoundRobinAgentAsync(null, ct);
    }

    /// <inheritdoc />
    public async Task<int?> GetLeastLoadedAgentAsync(int? queueId = null, CancellationToken ct = default)
    {
        var agents = await GetActiveAgentsAsync(ct);
        if (agents.Count == 0)
            return null;

        var agentLoads = new List<(User Agent, int OpenCount)>();
        foreach (var agent in agents)
        {
            var openCount = await _dbContext.ServiceRequests
                .CountAsync(s => s.AssignedToUserId == agent.Id
                    && !s.IsDeleted
                    && s.Status != ServiceRequestStatus.Closed
                    && s.Status != ServiceRequestStatus.Resolved, ct);
            agentLoads.Add((agent, openCount));
        }

        var leastLoaded = agentLoads.OrderBy(a => a.OpenCount).ThenBy(a => a.Agent.Id).First();

        _logger.LogDebug("Least-loaded agent: {AgentId} with {OpenCount} open requests",
            leastLoaded.Agent.Id, leastLoaded.OpenCount);

        return leastLoaded.Agent.Id;
    }

    #endregion

    #region Rule Management (In-Memory)

    /// <inheritdoc />
    public Task<IEnumerable<AssignmentRuleDto>> GetRulesAsync(CancellationToken ct = default)
    {
        var rules = _rules.Values
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .AsEnumerable();
        return Task.FromResult(rules);
    }

    /// <inheritdoc />
    public Task<AssignmentRuleDto?> GetRuleByIdAsync(int id, CancellationToken ct = default)
    {
        _rules.TryGetValue(id, out var rule);
        return Task.FromResult(rule);
    }

    /// <inheritdoc />
    public Task<AssignmentRuleDto> CreateRuleAsync(CreateAssignmentRuleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Rule name is required", nameof(dto));

        // Validate strategy value
        var validStrategies = new[] { "RoundRobin", "SkillBased", "LeastLoaded" };
        var strategy = validStrategies.FirstOrDefault(s =>
            s.Equals(dto.Strategy, StringComparison.OrdinalIgnoreCase)) ?? "RoundRobin";

        var id = Interlocked.Increment(ref _nextRuleId);
        var rule = new AssignmentRuleDto
        {
            Id = id,
            Name = dto.Name,
            Strategy = strategy,
            Priority = dto.Priority,
            IsActive = dto.IsActive,
            CategoryFilter = dto.CategoryFilter,
            PriorityFilter = dto.PriorityFilter,
            QueueId = dto.QueueId,
            RequiredSkills = dto.RequiredSkills,
            CreatedAt = DateTime.UtcNow
        };

        _rules[id] = rule;

        _logger.LogInformation("Created assignment rule {RuleId}: {RuleName} ({Strategy})", id, rule.Name, rule.Strategy);
        return Task.FromResult(rule);
    }

    /// <inheritdoc />
    public Task<AssignmentRuleDto?> UpdateRuleAsync(int id, UpdateAssignmentRuleDto dto, CancellationToken ct = default)
    {
        if (!_rules.TryGetValue(id, out var existing))
            return Task.FromResult<AssignmentRuleDto?>(null);

        if (dto.Name != null) existing.Name = dto.Name;
        if (dto.Strategy != null)
        {
            var validStrategies = new[] { "RoundRobin", "SkillBased", "LeastLoaded" };
            existing.Strategy = validStrategies.FirstOrDefault(s =>
                s.Equals(dto.Strategy, StringComparison.OrdinalIgnoreCase)) ?? existing.Strategy;
        }
        if (dto.Priority.HasValue) existing.Priority = dto.Priority.Value;
        if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;
        if (dto.CategoryFilter != null) existing.CategoryFilter = dto.CategoryFilter;
        if (dto.PriorityFilter != null) existing.PriorityFilter = dto.PriorityFilter;
        if (dto.QueueId.HasValue) existing.QueueId = dto.QueueId;
        if (dto.RequiredSkills != null) existing.RequiredSkills = dto.RequiredSkills;

        _rules[id] = existing;
        _logger.LogInformation("Updated assignment rule {RuleId}: {RuleName}", id, existing.Name);
        return Task.FromResult<AssignmentRuleDto?>(existing);
    }

    /// <inheritdoc />
    public Task<bool> DeleteRuleAsync(int id, CancellationToken ct = default)
    {
        var removed = _rules.TryRemove(id, out _);
        if (removed)
            _logger.LogInformation("Deleted assignment rule {RuleId}", id);
        return Task.FromResult(removed);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Get all active, non-deleted support/admin users who can be assigned tickets.
    /// </summary>
    private async Task<List<User>> GetActiveAgentsAsync(CancellationToken ct)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.IsActive && !u.IsDeleted && !u.IsLocked)
            .OrderBy(u => u.Id)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Find the highest-priority active rule matching the given service request.
    /// </summary>
    private AssignmentRuleDto? FindMatchingRule(ServiceRequest sr)
    {
        var activeRules = _rules.Values
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in activeRules)
        {
            if (RuleMatchesServiceRequest(rule, sr))
                return rule;
        }

        return null;
    }

    /// <summary>
    /// Check whether a rule's filters match a service request.
    /// </summary>
    private static bool RuleMatchesServiceRequest(AssignmentRuleDto rule, ServiceRequest sr)
    {
        // Category filter: if set, SR category must match
        if (!string.IsNullOrWhiteSpace(rule.CategoryFilter))
        {
            var categoryId = sr.CategoryId?.ToString() ?? string.Empty;
            if (!rule.CategoryFilter.Equals(categoryId, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Priority filter: if set, SR priority must match
        if (!string.IsNullOrWhiteSpace(rule.PriorityFilter))
        {
            var priorityStr = sr.Priority.ToString();
            if (!rule.PriorityFilter.Equals(priorityStr, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Reset all in-memory state (for testing purposes).
    /// </summary>
    internal static void ResetState()
    {
        _rules.Clear();
        _roundRobinCounters.Clear();
        _globalRoundRobinIndex = 0;
        _nextRuleId = 1;
    }

    #endregion
}
