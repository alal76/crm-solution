// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EscalationRule = CRM.Core.Entities.ITSM.EscalationRule;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for managing escalation rules within SLA policies (SLA query/evaluation focused).
/// Implements IEscalationRulePolicyService (renamed from IEscalationRuleService to avoid
/// conflict with the admin CRUD interface IEscalationRuleService).
/// </summary>
public class EscalationRuleService : IEscalationRulePolicyService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<EscalationRuleService> _logger;

    public EscalationRuleService(
        ICrmDbContext dbContext,
        ILogger<EscalationRuleService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<EscalationRuleDto> Items, int TotalCount)> GetRulesAsync(EscalationRuleFilterDto filter)
    {
        try
        {
            var query = _dbContext.Set<EscalationRule>()
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            // Apply filters
            if (filter.SLAPolicyId.HasValue)
            {
                query = query.Where(r => r.SLAPolicyId == filter.SLAPolicyId);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(r => r.IsActive == filter.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(term) ||
                    (r.Description != null && r.Description.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Name)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items.Select(MapToDto), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation rules");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EscalationRuleDto?> GetRuleByIdAsync(int id)
    {
        try
        {
            var rule = await _dbContext.Set<EscalationRule>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            return rule != null ? MapToDto(rule) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation rule {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EscalationRuleDto>> GetRulesBySLAPolicyAsync(int slaPolicyId)
    {
        try
        {
            var rules = await _dbContext.Set<EscalationRule>()
                .AsNoTracking()
                .Where(r => r.SLAPolicyId == slaPolicyId && !r.IsDeleted)
                .OrderBy(r => r.AgeInMinutes)
                .ToListAsync();

            return rules.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation rules for SLA policy {SLAPolicyId}", slaPolicyId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EscalationRuleDto> CreateRuleAsync(CreateEscalationRuleDto dto, int createdById)
    {
        try
        {
            // Validate SLA Policy exists
            var policyExists = await _dbContext.ITSMSLAPolicies
                .AnyAsync(p => p.SLAPolicyId == dto.SLAPolicyId && !p.IsDeleted);

            if (!policyExists)
            {
                throw new KeyNotFoundException($"SLA Policy with ID {dto.SLAPolicyId} not found");
            }

            var rule = new EscalationRule
            {
                Name = dto.Name,
                Description = null, // DTOs don't have Description
                Priority = MapMetricTypeToPriority(dto.TriggerMetric),
                Category = null,
                Queue = null,
                AgeInMinutes = CalculateAgeFromPercent(dto.TriggerAtPercent),
                TargetType = MapEscalationTypeToTargetType(dto.EscalationType),
                TargetId = dto.ReassignToUserId ?? dto.ReassignToTeamId,
                TargetName = dto.ReassignToUserId.HasValue ? "User" : dto.ReassignToTeamId.HasValue ? "Team" : null,
                MaxAttempts = 3,
                RetryIntervalMinutes = 15,
                IsActive = dto.IsActive,
                SLAPolicyId = dto.SLAPolicyId,
                Conditions = SerializeActionConfig(dto.ActionConfig),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _dbContext.Set<EscalationRule>().Add(rule);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Escalation rule created: {RuleId} ({RuleName}) for SLA Policy {SLAPolicyId}",
                rule.Id, rule.Name, dto.SLAPolicyId);

            return MapToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating escalation rule");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EscalationRuleDto> UpdateRuleAsync(int id, UpdateEscalationRuleDto dto, int modifiedById)
    {
        try
        {
            var rule = await _dbContext.Set<EscalationRule>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (rule == null)
            {
                throw new KeyNotFoundException($"Escalation rule with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                rule.Name = dto.Name;
            }

            if (dto.IsActive.HasValue)
            {
                rule.IsActive = dto.IsActive.Value;
            }

            if (dto.TriggerAtPercent.HasValue)
            {
                rule.AgeInMinutes = CalculateAgeFromPercent(dto.TriggerAtPercent.Value);
            }

            if (dto.TriggerMetric.HasValue)
            {
                rule.Priority = MapMetricTypeToPriority(dto.TriggerMetric.Value);
            }

            if (dto.EscalationType.HasValue)
            {
                rule.TargetType = MapEscalationTypeToTargetType(dto.EscalationType.Value);
            }

            if (dto.ReassignToUserId.HasValue)
            {
                rule.TargetId = dto.ReassignToUserId.Value;
                rule.TargetName = "User";
            }
            else if (dto.ReassignToTeamId.HasValue)
            {
                rule.TargetId = dto.ReassignToTeamId.Value;
                rule.TargetName = "Team";
            }

            if (dto.ActionConfig != null)
            {
                rule.Conditions = SerializeActionConfig(dto.ActionConfig);
            }


            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation rule updated: {RuleId}", id);

            return MapToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating escalation rule {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteRuleAsync(int id)
    {
        try
        {
            var rule = await _dbContext.Set<EscalationRule>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (rule == null)
            {
                return false;
            }

            rule.IsDeleted = true;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation rule deleted: {RuleId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting escalation rule {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EscalationRuleDto> EnableRuleAsync(int id, int modifiedById)
    {
        try
        {
            var rule = await _dbContext.Set<EscalationRule>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (rule == null)
            {
                throw new KeyNotFoundException($"Escalation rule with ID {id} not found");
            }

            rule.IsActive = true;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation rule enabled: {RuleId}", id);

            return MapToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling escalation rule {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EscalationRuleDto> DisableRuleAsync(int id, int modifiedById)
    {
        try
        {
            var rule = await _dbContext.Set<EscalationRule>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (rule == null)
            {
                throw new KeyNotFoundException($"Escalation rule with ID {id} not found");
            }

            rule.IsActive = false;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Escalation rule disabled: {RuleId}", id);

            return MapToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling escalation rule {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EscalationRuleDto>> GetApplicableRulesAsync(int serviceRequestId)
    {
        try
        {
            // Get the service request to find its SLA policy
            var serviceRequest = await _dbContext.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == serviceRequestId);

            if (serviceRequest == null)
            {
                _logger.LogWarning("Service request {ServiceRequestId} not found", serviceRequestId);
                return Enumerable.Empty<EscalationRuleDto>();
            }

            // Find applicable rules - rules that match the service request criteria
            var rules = await _dbContext.Set<EscalationRule>()
                .AsNoTracking()
                .Where(r => r.IsActive && !r.IsDeleted)
                .Where(r =>
                    // Match priority
                    r.Priority == serviceRequest.Priority.ToString() ||
                    string.IsNullOrEmpty(r.Priority) ||
                    // Match category
                    (r.Category == null || r.Category == serviceRequest.CategoryId.ToString()) ||
                    // Match queue
                    (r.Queue == null || r.Queue == serviceRequest.AssignedToGroupId.ToString()))
                .OrderBy(r => r.AgeInMinutes)
                .ToListAsync();

            return rules.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicable rules for service request {ServiceRequestId}", serviceRequestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> EvaluateRulesAsync(int serviceRequestId)
    {
        try
        {
            var serviceRequest = await _dbContext.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.Id == serviceRequestId);

            if (serviceRequest == null)
            {
                _logger.LogWarning("Service request {ServiceRequestId} not found for rule evaluation", serviceRequestId);
                return false;
            }

            var applicableRules = await GetApplicableRulesAsync(serviceRequestId);

            if (!applicableRules.Any())
            {
                _logger.LogDebug("No applicable escalation rules for service request {ServiceRequestId}", serviceRequestId);
                return false;
            }

            // Calculate the age of the service request in minutes
            var ageInMinutes = (DateTime.UtcNow - serviceRequest.CreatedAt).TotalMinutes;

            foreach (var rule in applicableRules.OrderBy(r => r.TriggerAtPercent))
            {
                // Check if this rule should be triggered based on time
                int ruleAgeThreshold = CalculateAgeFromPercent(rule.TriggerAtPercent);

                if (ageInMinutes >= ruleAgeThreshold)
                {
                    // Execute the rule action
                    await ExecuteRuleActionAsync(rule, serviceRequest);

                    _logger.LogInformation(
                        "Escalation rule {RuleId} executed for service request {ServiceRequestId}",
                        rule.Id, serviceRequestId);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating escalation rules for service request {ServiceRequestId}", serviceRequestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ReorderRulesAsync(int slaPolicyId, IEnumerable<int> ruleIds)
    {
        try
        {
            var rules = await _dbContext.Set<EscalationRule>()
                .Where(r => r.SLAPolicyId == slaPolicyId && !r.IsDeleted)
                .ToListAsync();

            var ruleIdList = ruleIds.ToList();

            for (int i = 0; i < ruleIdList.Count; i++)
            {
                var rule = rules.FirstOrDefault(r => r.Id == ruleIdList[i]);
                if (rule != null)
                {
                    // Use AgeInMinutes to represent order (multiply by execution order factor)
                    rule.AgeInMinutes = (i + 1) * 10; // 10, 20, 30, ...
                }
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Escalation rules reordered for SLA policy {SLAPolicyId}. New order: {RuleIds}",
                slaPolicyId, string.Join(", ", ruleIdList));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering escalation rules for SLA policy {SLAPolicyId}", slaPolicyId);
            throw;
        }
    }

    #region Private Helper Methods

    private static EscalationRuleDto MapToDto(EscalationRule rule)
    {
        return new EscalationRuleDto
        {
            Id = rule.Id,
            SLAPolicyId = rule.SLAPolicyId ?? 0,
            SLAPolicyName = null, // Would need to join to get this
            Name = rule.Name,
            TriggerAtPercent = CalculatePercentFromAge(rule.AgeInMinutes),
            TriggerMetric = MapPriorityToMetricType(rule.Priority),
            IsActive = rule.IsActive,
            ExecutionOrder = rule.AgeInMinutes / 10, // Derive order from age
            EscalationType = MapTargetTypeToEscalationType(rule.TargetType),
            EmailRecipients = null,
            EmailTemplateId = null,
            ReassignToUserId = rule.TargetType == EscalationTargetType.User ? rule.TargetId : null,
            ReassignToUserName = rule.TargetType == EscalationTargetType.User ? rule.TargetName : null,
            ReassignToTeamId = rule.TargetType == EscalationTargetType.Group ? rule.TargetId : null,
            ReassignToTeamName = rule.TargetType == EscalationTargetType.Group ? rule.TargetName : null,
            NewPriority = null,
            WebhookUrl = null,
            ActionConfig = DeserializeActionConfig(rule.Conditions),
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt ?? DateTime.UtcNow
        };
    }

    private static string MapMetricTypeToPriority(SLAMetricType metricType)
    {
        return metricType switch
        {
            SLAMetricType.FirstResponse => "High",
            SLAMetricType.Resolution => "Medium",
            SLAMetricType.NextResponse => "Low",
            _ => "Medium"
        };
    }

    private static SLAMetricType MapPriorityToMetricType(string? priority)
    {
        return priority?.ToLower() switch
        {
            "critical" or "high" => SLAMetricType.FirstResponse,
            "medium" => SLAMetricType.Resolution,
            "low" => SLAMetricType.NextResponse,
            _ => SLAMetricType.Resolution
        };
    }

    private static EscalationTargetType MapEscalationTypeToTargetType(EscalationType escalationType)
    {
        return escalationType switch
        {
            EscalationType.ReassignUser => EscalationTargetType.User,
            EscalationType.ReassignTeam => EscalationTargetType.Group,
            EscalationType.Email => EscalationTargetType.User,
            EscalationType.IncreasePriority => EscalationTargetType.Manager,
            _ => EscalationTargetType.User
        };
    }

    private static EscalationType MapTargetTypeToEscalationType(EscalationTargetType targetType)
    {
        return targetType switch
        {
            EscalationTargetType.User => EscalationType.ReassignUser,
            EscalationTargetType.Group => EscalationType.ReassignTeam,
            EscalationTargetType.Manager => EscalationType.IncreasePriority,
            EscalationTargetType.Queue => EscalationType.ReassignTeam,
            _ => EscalationType.Email
        };
    }

    private static int CalculateAgeFromPercent(int percent)
    {
        // Assume 100% = 60 minutes (1 hour) as baseline
        // This can be adjusted based on SLA policy targets
        return (percent * 60) / 100;
    }

    private static int CalculatePercentFromAge(int ageInMinutes)
    {
        // Reverse calculation: 60 minutes = 100%
        return (ageInMinutes * 100) / 60;
    }

    private static string? SerializeActionConfig(Dictionary<string, object>? actionConfig)
    {
        if (actionConfig == null || actionConfig.Count == 0)
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Serialize(actionConfig);
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object>? DeserializeActionConfig(string? conditions)
    {
        if (string.IsNullOrWhiteSpace(conditions))
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(conditions);
        }
        catch
        {
            return null;
        }
    }

    private async Task ExecuteRuleActionAsync(EscalationRuleDto rule, ServiceRequest serviceRequest)
    {
        // Log the escalation action
        _logger.LogInformation(
            "Executing escalation rule {RuleId} ({RuleName}) for service request {ServiceRequestId}. " +
            "Action type: {EscalationType}",
            rule.Id, rule.Name, serviceRequest.Id, rule.EscalationType);

        switch (rule.EscalationType)
        {
            case EscalationType.ReassignUser:
                if (rule.ReassignToUserId.HasValue)
                {
                    serviceRequest.AssignedToUserId = rule.ReassignToUserId.Value;
                }
                break;

            case EscalationType.ReassignTeam:
                if (rule.ReassignToTeamId.HasValue)
                {
                    serviceRequest.AssignedToGroupId = rule.ReassignToTeamId.Value;
                }
                break;

            case EscalationType.IncreasePriority:
                // Increase priority (lower enum value = higher priority)
                var currentPriority = (int)serviceRequest.Priority;
                if (currentPriority > 0)
                {
                    serviceRequest.Priority = (ServiceRequestPriority)(currentPriority - 1);
                }
                break;

            case EscalationType.Email:
                // Email notification would be handled by a separate notification service
                _logger.LogInformation(
                    "Email notification triggered for service request {ServiceRequestId}",
                    serviceRequest.Id);
                break;

            case EscalationType.Webhook:
                // Webhook notification would be handled by a separate webhook service
                _logger.LogInformation(
                    "Webhook notification triggered for service request {ServiceRequestId}",
                    serviceRequest.Id);
                break;

            default:
                _logger.LogWarning(
                    "Unknown escalation type {EscalationType} for rule {RuleId}",
                    rule.EscalationType, rule.Id);
                break;
        }

        // Log escalation execution (EscalationHistory is designed for Incidents, not ServiceRequests)
        _logger.LogInformation(
            "Escalation executed for service request {ServiceRequestId} - Rule: {RuleId}, " +
            "Type: {EscalationType}, ReassignToUser: {UserId}, ReassignToTeam: {TeamId}",
            serviceRequest.Id, rule.Id, rule.EscalationType,
            rule.ReassignToUserId, rule.ReassignToTeamId);

        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
