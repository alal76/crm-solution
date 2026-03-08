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

// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using System.Collections.Concurrent;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for automatic incident assignment based on rules.
/// </summary>
public interface IAssignmentRulesEngine
{
    /// <summary>
    /// Evaluate all assignment rules and return the best assignment for an incident.
    /// </summary>
    Task<AssignmentResult> EvaluateAsync(int incidentId);

    /// <summary>
    /// Apply assignment rules and assign the incident automatically.
    /// </summary>
    Task<AssignmentResult> AutoAssignAsync(int incidentId);

    /// <summary>
    /// Get all configured assignment rules.
    /// </summary>
    Task<List<AssignmentRule>> GetRulesAsync();

    /// <summary>
    /// Create or update an assignment rule.
    /// </summary>
    Task<AssignmentRule> SaveRuleAsync(AssignmentRule rule);

    /// <summary>
    /// Delete an assignment rule.
    /// </summary>
    Task<bool> DeleteRuleAsync(int ruleId);

    /// <summary>
    /// Test a rule against an incident without applying.
    /// </summary>
    Task<RuleTestResult> TestRuleAsync(int ruleId, int incidentId);

    /// <summary>
    /// Get workload distribution for assignment groups.
    /// </summary>
    Task<List<GroupWorkload>> GetGroupWorkloadsAsync();

    /// <summary>
    /// Get available agents in a group for assignment.
    /// </summary>
    Task<List<AvailableAgent>> GetAvailableAgentsAsync(int groupId);
}

/// <summary>
/// Result of an assignment evaluation.
/// </summary>
public class AssignmentResult
{
    public int IncidentId { get; set; }
    public bool WasAssigned { get; set; }
    public int? AssignedGroupId { get; set; }
    public string? AssignedGroupName { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public int? MatchedRuleId { get; set; }
    public string? MatchedRuleName { get; set; }
    public string AssignmentReason { get; set; } = string.Empty;
    public List<RuleEvaluation> EvaluatedRules { get; set; } = new();
}

/// <summary>
/// Evaluation result for a single rule.
/// </summary>
public class RuleEvaluation
{
    public int RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool Matched { get; set; }
    public string? FailureReason { get; set; }
    public int ConditionsMatched { get; set; }
    public int TotalConditions { get; set; }
}

/// <summary>
/// Assignment rule definition.
/// </summary>
public class AssignmentRule
{
    public int RuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } // Lower = higher priority
    public List<RuleCondition> Conditions { get; set; } = new();
    public AssignmentAction Action { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? CreatedById { get; set; }
}

/// <summary>
/// Condition for matching an incident.
/// </summary>
public class RuleCondition
{
    public int ConditionId { get; set; }
    public string Field { get; set; } = string.Empty; // e.g., "Category", "Priority", "CI.Type"
    public ConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
}

public enum ConditionOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    In,
    NotIn,
    IsNull,
    IsNotNull
}

/// <summary>
/// Action to take when rule matches.
/// </summary>
public class AssignmentAction
{
    public AssignmentType Type { get; set; }
    public int? TargetGroupId { get; set; }
    public string? TargetGroupName { get; set; }
    public int? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public bool UseRoundRobin { get; set; }
    public bool UseWorkloadBalance { get; set; }
    public bool UseSkillMatching { get; set; }
}

public enum AssignmentType
{
    AssignToGroup,
    AssignToUser,
    AssignToGroupMember // Assign to specific member of group
}

/// <summary>
/// Result of testing a rule.
/// </summary>
public class RuleTestResult
{
    public int RuleId { get; set; }
    public int IncidentId { get; set; }
    public bool WouldMatch { get; set; }
    public List<ConditionTestResult> ConditionResults { get; set; } = new();
    public string? TargetAssignment { get; set; }
}

public class ConditionTestResult
{
    public int ConditionId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string ExpectedValue { get; set; } = string.Empty;
    public string? ActualValue { get; set; }
    public bool Matched { get; set; }
}

/// <summary>
/// Workload for an assignment group.
/// </summary>
public class GroupWorkload
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TotalActiveIncidents { get; set; }
    public int UnassignedIncidents { get; set; }
    public int TotalMembers { get; set; }
    public int AvailableMembers { get; set; }
    public double AverageLoadPerMember { get; set; }
    public WorkloadStatus Status { get; set; }
}

public enum WorkloadStatus
{
    Light,
    Normal,
    Heavy,
    Overloaded
}

/// <summary>
/// Agent available for assignment.
/// </summary>
public class AvailableAgent
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int CurrentIncidentCount { get; set; }
    public bool IsAvailable { get; set; }
    public string? Shift { get; set; }
    public List<string> Skills { get; set; } = new();
    public DateTime? LastAssignmentTime { get; set; }
}

/// <summary>
/// Engine for automatic incident assignment based on configurable rules.
/// </summary>
public class AssignmentRulesEngine : IAssignmentRulesEngine
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AssignmentRulesEngine> _logger;

    // In-memory storage for rules (would be database in production)
    private static readonly List<AssignmentRule> _rules = new();
    private static readonly ConcurrentDictionary<int, int> _roundRobinIndex = new();
    private static readonly object _rulesLock = new();
    private static int _nextRuleId = 1;
    private static int _nextConditionId = 1;

    static AssignmentRulesEngine()
    {
        // Initialize default rules
        _rules.AddRange(new[]
        {
            new AssignmentRule
            {
                RuleId = _nextRuleId++,
                Name = "Network Issues to Network Team",
                Description = "Route all network-related incidents to the network support team",
                IsActive = true,
                Priority = 1,
                Conditions = new List<RuleCondition>
                {
                    new() { ConditionId = _nextConditionId++, Field = "Category", Operator = ConditionOperator.Equals, Value = "Network" }
                },
                Action = new AssignmentAction
                {
                    Type = AssignmentType.AssignToGroup,
                    TargetGroupId = 1,
                    TargetGroupName = "Network Support",
                    UseRoundRobin = true
                },
                CreatedAt = DateTime.UtcNow
            },
            new AssignmentRule
            {
                RuleId = _nextRuleId++,
                Name = "P1 Incidents to Senior Team",
                Description = "All P1 incidents go to senior support team",
                IsActive = true,
                Priority = 0, // Highest priority
                Conditions = new List<RuleCondition>
                {
                    new() { ConditionId = _nextConditionId++, Field = "Priority", Operator = ConditionOperator.Equals, Value = "1" }
                },
                Action = new AssignmentAction
                {
                    Type = AssignmentType.AssignToGroup,
                    TargetGroupId = 2,
                    TargetGroupName = "Senior Support",
                    UseWorkloadBalance = true
                },
                CreatedAt = DateTime.UtcNow
            },
            new AssignmentRule
            {
                RuleId = _nextRuleId++,
                Name = "Application Errors to App Support",
                Description = "Route application errors to the application support team",
                IsActive = true,
                Priority = 2,
                Conditions = new List<RuleCondition>
                {
                    new() { ConditionId = _nextConditionId++, Field = "Category", Operator = ConditionOperator.Contains, Value = "Application" },
                    new() { ConditionId = _nextConditionId++, Field = "SubCategory", Operator = ConditionOperator.IsNotNull, Value = "", LogicalOperator = LogicalOperator.And }
                },
                Action = new AssignmentAction
                {
                    Type = AssignmentType.AssignToGroup,
                    TargetGroupId = 3,
                    TargetGroupName = "Application Support",
                    UseRoundRobin = true
                },
                CreatedAt = DateTime.UtcNow
            },
            new AssignmentRule
            {
                RuleId = _nextRuleId++,
                Name = "VIP User Incidents",
                Description = "VIP users get dedicated support",
                IsActive = true,
                Priority = 0,
                Conditions = new List<RuleCondition>
                {
                    new() { ConditionId = _nextConditionId++, Field = "AffectedUser.IsVIP", Operator = ConditionOperator.Equals, Value = "true" }
                },
                Action = new AssignmentAction
                {
                    Type = AssignmentType.AssignToGroup,
                    TargetGroupId = 2,
                    TargetGroupName = "Senior Support",
                    UseWorkloadBalance = true
                },
                CreatedAt = DateTime.UtcNow
            },
            new AssignmentRule
            {
                RuleId = _nextRuleId++,
                Name = "Default Assignment",
                Description = "Fallback rule for unmatched incidents",
                IsActive = true,
                Priority = 100, // Lowest priority
                Conditions = new List<RuleCondition>(),
                Action = new AssignmentAction
                {
                    Type = AssignmentType.AssignToGroup,
                    TargetGroupId = 4,
                    TargetGroupName = "Help Desk",
                    UseRoundRobin = true
                },
                CreatedAt = DateTime.UtcNow
            }
        });
    }

    public AssignmentRulesEngine(
        ICrmDbContext context,
        ILogger<AssignmentRulesEngine> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AssignmentResult> EvaluateAsync(int incidentId)
    {
        var incident = await _context.Incidents
            .Include(i => i.Category)
            .Include(i => i.Subcategory)
            .Include(i => i.AssignmentGroup)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        if (incident == null)
        {
            throw new ArgumentException($"Incident {incidentId} not found");
        }

        var result = new AssignmentResult
        {
            IncidentId = incidentId,
            EvaluatedRules = new List<RuleEvaluation>()
        };

        List<AssignmentRule> rulesSnapshot;
        lock (_rulesLock) { rulesSnapshot = _rules.ToList(); }
        var activeRules = rulesSnapshot
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in activeRules)
        {
            var evaluation = EvaluateRule(rule, incident);
            result.EvaluatedRules.Add(evaluation);

            if (evaluation.Matched)
            {
                result.WasAssigned = true;
                result.MatchedRuleId = rule.RuleId;
                result.MatchedRuleName = rule.Name;
                result.AssignedGroupId = rule.Action.TargetGroupId;
                result.AssignedGroupName = rule.Action.TargetGroupName;
                result.AssignmentReason = $"Matched rule: {rule.Name}";

                // Determine specific user if needed
                if (rule.Action.Type == AssignmentType.AssignToGroupMember ||
                    rule.Action.UseRoundRobin ||
                    rule.Action.UseWorkloadBalance)
                {
                    var agent = await SelectAgentAsync(rule);
                    if (agent != null)
                    {
                        result.AssignedUserId = agent.UserId;
                        result.AssignedUserName = agent.DisplayName;
                    }
                }
                else if (rule.Action.TargetUserId.HasValue)
                {
                    result.AssignedUserId = rule.Action.TargetUserId;
                    result.AssignedUserName = rule.Action.TargetUserName;
                }

                break; // First matching rule wins
            }
        }

        _logger.LogInformation(
            "Assignment evaluation for incident {IncidentNumber}: {Result}",
            incident.Number,
            result.WasAssigned ? $"Assigned to {result.AssignedGroupName}" : "No match");

        return result;
    }

    public async Task<AssignmentResult> AutoAssignAsync(int incidentId)
    {
        var result = await EvaluateAsync(incidentId);

        if (result.WasAssigned)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);

            if (incident != null)
            {
                incident.AssignedToId = result.AssignedUserId;
                incident.AssignmentGroupId = result.AssignedGroupId;
                incident.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Auto-assigned incident {IncidentNumber} to {Group}/{User}",
                    incident.Number, result.AssignedGroupName, result.AssignedUserName ?? "Unassigned");
            }
        }

        return result;
    }

    public Task<List<AssignmentRule>> GetRulesAsync()
    {
        List<AssignmentRule> snapshot;
        lock (_rulesLock) { snapshot = _rules.ToList(); }
        return Task.FromResult(snapshot.OrderBy(r => r.Priority).ToList());
    }

    public Task<AssignmentRule> SaveRuleAsync(AssignmentRule rule)
    {
        if (rule.RuleId == 0)
        {
            rule.RuleId = Interlocked.Increment(ref _nextRuleId);
            rule.CreatedAt = DateTime.UtcNow;

            foreach (var condition in rule.Conditions)
            {
                if (condition.ConditionId == 0)
                    condition.ConditionId = Interlocked.Increment(ref _nextConditionId);
            }

            lock (_rulesLock) { _rules.Add(rule); }
        }
        else
        {
            lock (_rulesLock)
            {
                var existing = _rules.FirstOrDefault(r => r.RuleId == rule.RuleId);
                if (existing != null)
                {
                    _rules.Remove(existing);
                }

                rule.ModifiedAt = DateTime.UtcNow;
                _rules.Add(rule);
            }
        }

        _logger.LogInformation("Saved assignment rule: {Name}", rule.Name);
        return Task.FromResult(rule);
    }

    public Task<bool> DeleteRuleAsync(int ruleId)
    {
        AssignmentRule? toDelete;
        lock (_rulesLock)
        {
            toDelete = _rules.FirstOrDefault(r => r.RuleId == ruleId);
            if (toDelete != null) { _rules.Remove(toDelete); }
        }
        if (toDelete != null)
        {
            _logger.LogInformation("Deleted assignment rule: {Name}", toDelete.Name);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public async Task<RuleTestResult> TestRuleAsync(int ruleId, int incidentId)
    {
        AssignmentRule? rule;
        lock (_rulesLock) { rule = _rules.FirstOrDefault(r => r.RuleId == ruleId); }
        if (rule == null)
        {
            throw new ArgumentException($"Rule {ruleId} not found");
        }

        var incident = await _context.Incidents
            .Include(i => i.Category)
            .Include(i => i.Subcategory)
            .Include(i => i.AssignmentGroup)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        if (incident == null)
        {
            throw new ArgumentException($"Incident {incidentId} not found");
        }

        var result = new RuleTestResult
        {
            RuleId = ruleId,
            IncidentId = incidentId,
            ConditionResults = new List<ConditionTestResult>()
        };

        bool allMatched = true;
        foreach (var condition in rule.Conditions)
        {
            var actualValue = GetFieldValue(incident, condition.Field);
            var conditionMatched = EvaluateCondition(condition, actualValue);

            result.ConditionResults.Add(new ConditionTestResult
            {
                ConditionId = condition.ConditionId,
                Field = condition.Field,
                ExpectedValue = condition.Value,
                ActualValue = actualValue,
                Matched = conditionMatched
            });

            if (!conditionMatched)
                allMatched = false;
        }

        result.WouldMatch = allMatched || rule.Conditions.Count == 0;

        if (result.WouldMatch)
        {
            result.TargetAssignment = rule.Action.Type switch
            {
                AssignmentType.AssignToUser => rule.Action.TargetUserName,
                _ => rule.Action.TargetGroupName
            };
        }

        return result;
    }

    public async Task<List<GroupWorkload>> GetGroupWorkloadsAsync()
    {
        var workloads = new List<GroupWorkload>();

        // Get unique assignment groups from incidents
        var groups = await _context.Incidents
            .Where(i => i.AssignmentGroupId != null)
            .Where(i => i.State != IncidentState.Resolved && i.State != IncidentState.Closed)
            .GroupBy(i => i.AssignmentGroupId!.Value)
            .Select(g => new
            {
                GroupId = g.Key,
                TotalActive = g.Count(),
                Unassigned = g.Count(i => i.AssignedToId == null)
            })
            .ToListAsync();

        foreach (var group in groups)
        {
            var memberCount = 5; // Would come from user-group relationships
            var averageLoad = memberCount > 0 ? (double)group.TotalActive / memberCount : 0;

            workloads.Add(new GroupWorkload
            {
                GroupId = group.GroupId,
                GroupName = $"Group {group.GroupId}",
                TotalActiveIncidents = group.TotalActive,
                UnassignedIncidents = group.Unassigned,
                TotalMembers = memberCount,
                AvailableMembers = (int)(memberCount * 0.8), // Assume 80% availability
                AverageLoadPerMember = averageLoad,
                Status = averageLoad switch
                {
                    < 3 => WorkloadStatus.Light,
                    < 6 => WorkloadStatus.Normal,
                    < 10 => WorkloadStatus.Heavy,
                    _ => WorkloadStatus.Overloaded
                }
            });
        }

        return workloads;
    }

    public async Task<List<AvailableAgent>> GetAvailableAgentsAsync(int groupId)
    {
        // Get users (in production, would filter by group membership)
        var users = await _context.Users
            .Take(10)
            .ToListAsync();

        var agents = new List<AvailableAgent>();

        foreach (var user in users)
        {
            // Get current incident count
            var incidentCount = await _context.Incidents
                .CountAsync(i => i.AssignedToId == user.Id &&
                                i.State != IncidentState.Resolved &&
                                i.State != IncidentState.Closed);

            agents.Add(new AvailableAgent
            {
                UserId = user.Id,
                UserName = user.Username ?? string.Empty,
                DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                CurrentIncidentCount = incidentCount,
                IsAvailable = incidentCount < 10, // Available if under 10 incidents
                Skills = new List<string> { "General Support" }
            });
        }

        return agents.OrderBy(a => a.CurrentIncidentCount).ToList();
    }

    private RuleEvaluation EvaluateRule(AssignmentRule rule, Incident incident)
    {
        var evaluation = new RuleEvaluation
        {
            RuleId = rule.RuleId,
            RuleName = rule.Name,
            Priority = rule.Priority,
            TotalConditions = rule.Conditions.Count
        };

        // Empty conditions = match all
        if (!rule.Conditions.Any())
        {
            evaluation.Matched = true;
            evaluation.ConditionsMatched = 0;
            return evaluation;
        }

        bool allAndConditionsMatch = true;
        bool anyOrConditionMatch = false;
        bool hasOrConditions = rule.Conditions.Any(c => c.LogicalOperator == LogicalOperator.Or);

        foreach (var condition in rule.Conditions)
        {
            var actualValue = GetFieldValue(incident, condition.Field);
            var matches = EvaluateCondition(condition, actualValue);

            if (matches)
            {
                evaluation.ConditionsMatched++;
                if (condition.LogicalOperator == LogicalOperator.Or)
                    anyOrConditionMatch = true;
            }
            else if (condition.LogicalOperator == LogicalOperator.And)
            {
                allAndConditionsMatch = false;
                evaluation.FailureReason = $"Condition {condition.Field} {condition.Operator} {condition.Value} failed";
            }
        }

        evaluation.Matched = allAndConditionsMatch && (!hasOrConditions || anyOrConditionMatch);

        return evaluation;
    }

    private string? GetFieldValue(Incident incident, string field)
    {
        return field.ToLower() switch
        {
            "category" => incident.Category?.Name,
            "subcategory" => incident.Subcategory?.Name,
            "priority" => incident.Priority.ToString(),
            "impact" => incident.Impact.ToString(),
            "urgency" => incident.Urgency.ToString(),
            "status" => incident.State.ToString(),
            "title" => incident.ShortDescription,
            "description" => incident.Description,
            "assignmentgroup" => incident.AssignmentGroup?.Name,
            "affectedci.type" or "ci.type" => null, // CI nav prop not loaded
            "affectedci.criticality" or "ci.criticality" => null,
            "affecteduser.isvip" => "false", // Would check user VIP status
            _ => null
        };
    }

    private bool EvaluateCondition(RuleCondition condition, string? actualValue)
    {
        return condition.Operator switch
        {
            ConditionOperator.Equals =>
                string.Equals(actualValue, condition.Value, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals =>
                !string.Equals(actualValue, condition.Value, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.Contains =>
                actualValue?.Contains(condition.Value, StringComparison.OrdinalIgnoreCase) ?? false,
            ConditionOperator.StartsWith =>
                actualValue?.StartsWith(condition.Value, StringComparison.OrdinalIgnoreCase) ?? false,
            ConditionOperator.EndsWith =>
                actualValue?.EndsWith(condition.Value, StringComparison.OrdinalIgnoreCase) ?? false,
            ConditionOperator.In =>
                condition.Value.Split(',').Any(v =>
                    string.Equals(actualValue, v.Trim(), StringComparison.OrdinalIgnoreCase)),
            ConditionOperator.NotIn =>
                !condition.Value.Split(',').Any(v =>
                    string.Equals(actualValue, v.Trim(), StringComparison.OrdinalIgnoreCase)),
            ConditionOperator.IsNull =>
                string.IsNullOrEmpty(actualValue),
            ConditionOperator.IsNotNull =>
                !string.IsNullOrEmpty(actualValue),
            ConditionOperator.GreaterThan =>
                int.TryParse(actualValue, out var av) &&
                int.TryParse(condition.Value, out var cv) && av > cv,
            ConditionOperator.LessThan =>
                int.TryParse(actualValue, out var av2) &&
                int.TryParse(condition.Value, out var cv2) && av2 < cv2,
            _ => false
        };
    }

    private async Task<AvailableAgent?> SelectAgentAsync(AssignmentRule rule)
    {
        if (!rule.Action.TargetGroupId.HasValue)
            return null;

        var agents = await GetAvailableAgentsAsync(rule.Action.TargetGroupId.Value);
        var availableAgents = agents.Where(a => a.IsAvailable).ToList();

        if (!availableAgents.Any())
            return null;

        if (rule.Action.UseWorkloadBalance)
        {
            // Select agent with lowest workload
            return availableAgents.OrderBy(a => a.CurrentIncidentCount).FirstOrDefault();
        }

        if (rule.Action.UseRoundRobin)
        {
            // Get next agent in rotation - atomic counter increment
            var counter = _roundRobinIndex.AddOrUpdate(rule.Action.TargetGroupId.Value, 0, (_, v) => v + 1);
            var index = counter % availableAgents.Count;

            return availableAgents[index];
        }

        // Default: first available
        return availableAgents.FirstOrDefault();
    }
}
