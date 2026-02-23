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

using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing admin configurations (Sales and Service Desk settings)
/// 
/// HEXAGONAL ARCHITECTURE:
/// - Implements IAdminConfigurationService (primary/driving port)
/// - Uses ICrmDbContext (secondary/driven port)
/// </summary>
public class AdminConfigurationService : IAdminConfigurationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AdminConfigurationService> _logger;

    public AdminConfigurationService(
        ICrmDbContext context,
        ILogger<AdminConfigurationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Commission Rules

    public async Task<IEnumerable<CommissionRuleDto>> GetCommissionRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = await _context.CommissionRules
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            return rules.Select(MapCommissionRuleToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving commission rules");
            throw;
        }
    }

    public async Task<CommissionRuleDto?> GetCommissionRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.CommissionRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return rule == null ? null : MapCommissionRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving commission rule {id}");
            throw;
        }
    }

    public async Task<CommissionRuleDto> CreateCommissionRuleAsync(CreateCommissionRuleDto dto, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = new CommissionRule
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.RuleType,
                BaseRate = dto.Rate,
                MinAmount = dto.MinAmount,
                MaxAmount = dto.MaxAmount,
                ApplicableProductIds = JsonSerializer.Serialize(new List<int>()),
                ApplicableUserIds = JsonSerializer.Serialize(new List<int>()),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
            };

            _context.CommissionRules.Add(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Commission rule '{rule.Name}' created by user {createdByUserId}");
            return MapCommissionRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating commission rule");
            throw;
        }
    }

    public async Task<CommissionRuleDto?> UpdateCommissionRuleAsync(int id, UpdateCommissionRuleDto dto, int? modifiedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.CommissionRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Name))
                rule.Name = dto.Name;
            if (dto.RuleType.HasValue)
                rule.Type = dto.RuleType.Value;
            if (dto.Rate.HasValue)
                rule.BaseRate = dto.Rate.Value;
            if (dto.MinAmount.HasValue)
                rule.MinAmount = dto.MinAmount;
            if (dto.MaxAmount.HasValue)
                rule.MaxAmount = dto.MaxAmount;
            if (!string.IsNullOrEmpty(dto.Description))
                rule.Description = dto.Description;
            if (dto.IsActive.HasValue)
                rule.IsActive = dto.IsActive.Value;

            rule.UpdatedAt = DateTime.UtcNow;

            _context.CommissionRules.Update(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Commission rule '{rule.Name}' updated by user {modifiedByUserId}");
            return MapCommissionRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating commission rule {id}");
            throw;
        }
    }

    public async Task<bool> DeleteCommissionRuleAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.CommissionRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
                return false;

            rule.IsDeleted = true;
            rule.UpdatedAt = DateTime.UtcNow;

            _context.CommissionRules.Update(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Commission rule '{rule.Name}' soft-deleted by user {deletedByUserId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting commission rule {id}");
            throw;
        }
    }

    #endregion

    #region Discount Rules

    public async Task<IEnumerable<DiscountRuleDto>> GetDiscountRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = await _context.DiscountRules
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            return rules.Select(MapDiscountRuleToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discount rules");
            throw;
        }
    }

    public async Task<DiscountRuleDto?> GetDiscountRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.DiscountRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return rule == null ? null : MapDiscountRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving discount rule {id}");
            throw;
        }
    }

    public async Task<DiscountRuleDto> CreateDiscountRuleAsync(CreateDiscountRuleDto dto, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = new DiscountRule
            {
                Name = dto.Name,
                Description = dto.Description,
                DiscountType = dto.Type,
                DiscountValue = dto.Value,
                MinQuantity = dto.MinQuantity,
                MaxQuantity = null,
                MinOrderAmount = dto.MinOrderAmount,
                PromotionalCode = null,
                ValidFrom = dto.EffectiveDate,
                ValidUntil = dto.ExpiryDate,
                ApplicableProductIds = JsonSerializer.Serialize(new List<int>()),
                ApplicableUserIds = JsonSerializer.Serialize(new List<int>()),
                CumulativeWithOther = dto.IsCumulative,
                MaxDiscountValue = dto.MaxDiscount,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
            };

            _context.DiscountRules.Add(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Discount rule '{rule.Name}' created by user {createdByUserId}");
            return MapDiscountRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discount rule");
            throw;
        }
    }

    public async Task<DiscountRuleDto?> UpdateDiscountRuleAsync(int id, UpdateDiscountRuleDto dto, int? modifiedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.DiscountRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Name))
                rule.Name = dto.Name;
            if (dto.Type.HasValue)
                rule.DiscountType = dto.Type.Value;
            if (dto.Value.HasValue)
                rule.DiscountValue = dto.Value.Value;
            if (dto.MinOrderAmount.HasValue)
                rule.MinOrderAmount = dto.MinOrderAmount;
            if (dto.MinQuantity.HasValue)
                rule.MinQuantity = dto.MinQuantity;
            if (dto.MaxDiscount.HasValue)
                rule.MaxDiscountValue = dto.MaxDiscount;
            if (dto.EffectiveDate.HasValue)
                rule.ValidFrom = dto.EffectiveDate.Value;
            if (dto.ExpiryDate.HasValue)
                rule.ValidUntil = dto.ExpiryDate;
            if (dto.IsActive.HasValue)
                rule.IsActive = dto.IsActive.Value;
            if (dto.IsCumulative.HasValue)
                rule.CumulativeWithOther = dto.IsCumulative.Value;

            rule.UpdatedAt = DateTime.UtcNow;

            _context.DiscountRules.Update(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Discount rule '{rule.Name}' updated by user {modifiedByUserId}");
            return MapDiscountRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating discount rule {id}");
            throw;
        }
    }

    public async Task<bool> DeleteDiscountRuleAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.DiscountRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
                return false;

            rule.IsDeleted = true;
            rule.UpdatedAt = DateTime.UtcNow;

            _context.DiscountRules.Update(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Discount rule '{rule.Name}' soft-deleted by user {deletedByUserId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting discount rule {id}");
            throw;
        }
    }

    #endregion

    #region SLA Policies

    public async Task<IEnumerable<SLAPolicyDto>> GetSLAPoliciesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await _context.SLAPolicies
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            return policies.Select(MapSLAPolicyToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SLA policies");
            throw;
        }
    }

    public async Task<SLAPolicyDto?> GetSLAPolicyByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await _context.SLAPolicies
                .Where(p => !p.IsDeleted && p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return policy == null ? null : MapSLAPolicyToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving SLA policy {id}");
            throw;
        }
    }

    public async Task<SLAPolicyDto> CreateSLAPolicyAsync(CreateSLAPolicyDto request, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = new CRM.Core.Entities.SLAPolicy
            {
                Name = request.Name,
                Description = request.Description,
                Priority = Enum.Parse<ServicePriority>(request.Priority),
                InitialResponseTimeMinutes = request.InitialResponseTimeMinutes,
                ResolutionTimeMinutes = request.ResolutionTimeMinutes,
                WorkingHoursOnly = request.WorkingHoursOnly,
                EscalationPath = JsonSerializer.Serialize(request.EscalationPathUserIds),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _context.SLAPolicies.Add(policy);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"SLA policy '{policy.Name}' created by user {createdByUserId}");
            return MapSLAPolicyToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SLA policy");
            throw;
        }
    }

    public async Task<SLAPolicyDto?> UpdateSLAPolicyAsync(int id, UpdateSLAPolicyDto request, int? modifiedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await _context.SLAPolicies
                .Where(p => !p.IsDeleted && p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (policy == null)
                return null;

            if (!string.IsNullOrEmpty(request.Name))
                policy.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                policy.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Priority))
                policy.Priority = Enum.Parse<ServicePriority>(request.Priority);
            if (request.InitialResponseTimeMinutes.HasValue)
                policy.InitialResponseTimeMinutes = request.InitialResponseTimeMinutes.Value;
            if (request.ResolutionTimeMinutes.HasValue)
                policy.ResolutionTimeMinutes = request.ResolutionTimeMinutes.Value;
            if (request.WorkingHoursOnly.HasValue)
                policy.WorkingHoursOnly = request.WorkingHoursOnly.Value;
            if (request.EscalationPathUserIds != null)
                policy.EscalationPath = JsonSerializer.Serialize(request.EscalationPathUserIds);
            if (request.IsActive.HasValue)
                policy.IsActive = request.IsActive.Value;

            policy.UpdatedAt = DateTime.UtcNow;

            _context.SLAPolicies.Update(policy);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"SLA policy '{policy.Name}' updated by user {modifiedByUserId}");
            return MapSLAPolicyToDto(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating SLA policy {id}");
            throw;
        }
    }

    public async Task<bool> DeleteSLAPolicyAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await _context.SLAPolicies
                .Where(p => !p.IsDeleted && p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (policy == null)
                return false;

            policy.IsDeleted = true;
            policy.UpdatedAt = DateTime.UtcNow;

            _context.SLAPolicies.Update(policy);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"SLA policy '{policy.Name}' soft-deleted by user {deletedByUserId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting SLA policy {id}");
            throw;
        }
    }

    #endregion

    #region Escalation Rules

    public async Task<IEnumerable<EscalationRuleDto>> GetEscalationRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = await _context.ITSMEscalationRules
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            return rules.Select(MapEscalationRuleToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalation rules");
            throw;
        }
    }

    public async Task<EscalationRuleDto?> GetEscalationRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.ITSMEscalationRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return rule == null ? null : MapEscalationRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving escalation rule {id}");
            throw;
        }
    }

    public async Task<EscalationRuleDto> CreateEscalationRuleAsync(CreateEscalationRuleDto request, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: ITSM.EscalationRule schema differs from root EscalationRule.
            // Mapping: Condition→Conditions, ConditionMetric→Priority, ThresholdValue→AgeInMinutes,
            // EscalateToGroupId/UserId→TargetId+TargetType. SendNotification not available on ITSM.EscalationRule.
            var rule = new CRM.Core.Entities.ITSM.EscalationRule
            {
                Name = request.Name,
                Description = request.Description,
                Conditions = request.Condition,
                Priority = request.ConditionMetric,
                AgeInMinutes = request.ThresholdValue,
                TargetType = request.EscalateToGroupId.HasValue
                    ? CRM.Core.Entities.ITSM.EscalationTargetType.Group
                    : CRM.Core.Entities.ITSM.EscalationTargetType.User,
                TargetId = request.EscalateToGroupId ?? request.EscalateToUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _context.ITSMEscalationRules.Add(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Escalation rule '{rule.Name}' created by user {createdByUserId}");
            return MapEscalationRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating escalation rule");
            throw;
        }
    }

    public async Task<EscalationRuleDto?> UpdateEscalationRuleAsync(int id, UpdateEscalationRuleDto request, int? modifiedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.ITSMEscalationRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
                return null;

            if (!string.IsNullOrEmpty(request.Name))
                rule.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                rule.Description = request.Description;
            // TODO: ITSM.EscalationRule uses Conditions/Priority/AgeInMinutes instead of Condition/ConditionMetric/ThresholdValue
            if (!string.IsNullOrEmpty(request.Condition))
                rule.Conditions = request.Condition;
            if (!string.IsNullOrEmpty(request.ConditionMetric))
                rule.Priority = request.ConditionMetric;
            if (request.ThresholdValue.HasValue)
                rule.AgeInMinutes = request.ThresholdValue.Value;
            if (request.EscalateToUserId.HasValue)
            {
                rule.TargetId = request.EscalateToUserId;
                rule.TargetType = CRM.Core.Entities.ITSM.EscalationTargetType.User;
            }
            if (request.EscalateToGroupId.HasValue)
            {
                rule.TargetId = request.EscalateToGroupId;
                rule.TargetType = CRM.Core.Entities.ITSM.EscalationTargetType.Group;
            }
            // TODO: SendNotification not available on ITSM.EscalationRule - skipped
            if (request.IsActive.HasValue)
                rule.IsActive = request.IsActive.Value;

            rule.UpdatedAt = DateTime.UtcNow;

            _context.ITSMEscalationRules.Update(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Escalation rule '{rule.Name}' updated by user {modifiedByUserId}");
            return MapEscalationRuleToDto(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating escalation rule {id}");
            throw;
        }
    }

    public async Task<bool> DeleteEscalationRuleAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await _context.ITSMEscalationRules
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (rule == null)
                return false;

            rule.IsDeleted = true;
            rule.UpdatedAt = DateTime.UtcNow;

            _context.ITSMEscalationRules.Update(rule);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Escalation rule '{rule.Name}' soft-deleted by user {deletedByUserId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting escalation rule {id}");
            throw;
        }
    }

    #endregion

    #region Service Queues

    public async Task<IEnumerable<ServiceQueueDto>> GetServiceQueuesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var queues = await _context.ServiceQueues
                .Where(q => !q.IsDeleted)
                .OrderBy(q => q.Priority) // TODO: ITSM.ServiceQueue uses Priority instead of DisplayOrder
                .ToListAsync(cancellationToken);

            return queues.Select(MapServiceQueueToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service queues");
            throw;
        }
    }

    public async Task<ServiceQueueDto?> GetServiceQueueByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var queue = await _context.ServiceQueues
                .Where(q => !q.IsDeleted && q.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return queue == null ? null : MapServiceQueueToDto(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving service queue {id}");
            throw;
        }
    }

    public async Task<ServiceQueueDto> CreateServiceQueueAsync(CreateServiceQueueDto request, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: ITSM.ServiceQueue lacks RoutingType, AssignedUserIds, AssignedGroupIds, SkillRequirements, DisplayOrder.
            // These extended fields are stored as JSON in RoutingConfiguration pending a schema migration.
            var queue = new CRM.Core.Entities.ITSM.ServiceQueue
            {
                Name = request.Name,
                Description = request.Description,
                RoutingConfiguration = JsonSerializer.Serialize(new
                {
                    RoutingType = request.RoutingType,
                    AssignedUserIds = request.AssignedUserIds,
                    AssignedGroupIds = request.AssignedGroupIds,
                    SkillRequirements = request.SkillRequirements,
                    DisplayOrder = request.DisplayOrder
                }),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _context.ServiceQueues.Add(queue);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Service queue '{queue.Name}' created by user {createdByUserId}");
            return MapServiceQueueToDto(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service queue");
            throw;
        }
    }

    public async Task<ServiceQueueDto?> UpdateServiceQueueAsync(int id, UpdateServiceQueueDto request, int? modifiedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queue = await _context.ServiceQueues
                .Where(q => !q.IsDeleted && q.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (queue == null)
                return null;

            if (!string.IsNullOrEmpty(request.Name))
                queue.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                queue.Description = request.Description;
            // TODO: ITSM.ServiceQueue lacks RoutingType, AssignedUserIds, AssignedGroupIds, SkillRequirements, DisplayOrder.
            // Update the RoutingConfiguration JSON field with any changed extended properties.
            if (request.RoutingType != null || request.AssignedUserIds != null ||
                request.AssignedGroupIds != null || request.SkillRequirements != null || request.DisplayOrder.HasValue)
            {
                var existingConfig = string.IsNullOrEmpty(queue.RoutingConfiguration)
                    ? new System.Collections.Generic.Dictionary<string, object?>()
                    : JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(queue.RoutingConfiguration)
                      ?? new System.Collections.Generic.Dictionary<string, object?>();
                if (request.RoutingType != null) existingConfig["RoutingType"] = (object?)request.RoutingType;
                if (request.AssignedUserIds != null) existingConfig["AssignedUserIds"] = (object?)request.AssignedUserIds;
                if (request.AssignedGroupIds != null) existingConfig["AssignedGroupIds"] = (object?)request.AssignedGroupIds;
                if (request.SkillRequirements != null) existingConfig["SkillRequirements"] = (object?)request.SkillRequirements;
                if (request.DisplayOrder.HasValue) existingConfig["DisplayOrder"] = (object?)request.DisplayOrder.Value;
                queue.RoutingConfiguration = JsonSerializer.Serialize(existingConfig);
            }
            if (request.IsActive.HasValue)
                queue.IsActive = request.IsActive.Value;

            queue.UpdatedAt = DateTime.UtcNow;

            _context.ServiceQueues.Update(queue);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Service queue '{queue.Name}' updated by user {modifiedByUserId}");
            return MapServiceQueueToDto(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating service queue {id}");
            throw;
        }
    }

    public async Task<bool> DeleteServiceQueueAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queue = await _context.ServiceQueues
                .Where(q => !q.IsDeleted && q.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (queue == null)
                return false;

            queue.IsDeleted = true;
            queue.UpdatedAt = DateTime.UtcNow;

            _context.ServiceQueues.Update(queue);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Service queue '{queue.Name}' soft-deleted by user {deletedByUserId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting service queue {id}");
            throw;
        }
    }

    #endregion

    #region Configuration Overview

    public async Task<AdminConfigurationDto> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sales = await GetSalesConfigAsync(cancellationToken);
            var serviceDesk = await GetServiceDeskConfigAsync(cancellationToken);

            return new AdminConfigurationDto
            {
                SalesConfig = sales,
                ServiceDeskConfig = serviceDesk,
                NotificationConfig = new NotificationAdminConfigDto(),
                CustomConfigurations = new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin configuration");
            throw;
        }
    }

    public async Task<SalesAdminConfigDto> GetSalesConfigAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var commissionRules = await GetCommissionRulesAsync(cancellationToken);
            var discountRules = await GetDiscountRulesAsync(cancellationToken);

            return new SalesAdminConfigDto
            {
                CommissionRules = commissionRules.ToList(),
                DiscountRules = discountRules.ToList(),
                DefaultCommissionPercentage = 5m,
                MaxDiscountPercentage = 30m,
                RequireApprovalForDiscounts = true,
                RequireApprovalForOrders = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales configuration");
            throw;
        }
    }

    public async Task<ServiceDeskAdminConfigDto> GetServiceDeskConfigAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var slaPolicies = await GetSLAPoliciesAsync(cancellationToken);
            var escalationRules = await GetEscalationRulesAsync(cancellationToken);
            var queues = await GetServiceQueuesAsync(cancellationToken);

            return new ServiceDeskAdminConfigDto
            {
                SLAPolicies = slaPolicies.ToList(),
                EscalationRules = escalationRules.ToList(),
                ServiceQueues = queues.ToList(),
                AutoAssignRequests = true,
                DefaultPriorityMinutes = 120,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service desk configuration");
            throw;
        }
    }

    #endregion

    #region Mapping Helpers

    private CommissionRuleDto MapCommissionRuleToDto(CommissionRule entity)
    {
        return new CommissionRuleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SaleType = "Standard",
            RuleType = entity.Type.ToString(),
            Rate = entity.BaseRate,
            MinAmount = entity.MinAmount,
            MaxAmount = entity.MaxAmount,
            EffectiveDate = entity.CreatedAt,
            ExpiryDate = null,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private DiscountRuleDto MapDiscountRuleToDto(DiscountRule entity)
    {
        return new DiscountRuleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.DiscountType.ToString(),
            Value = entity.DiscountValue,
            MinOrderAmount = entity.MinOrderAmount,
            MinQuantity = entity.MinQuantity,
            CustomerTier = null,
            ProductCategory = null,
            MaxDiscount = entity.MaxDiscountValue,
            EffectiveDate = entity.ValidFrom,
            ExpiryDate = entity.ValidUntil,
            IsActive = entity.IsActive,
            IsCumulative = entity.CumulativeWithOther,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private SLAPolicyDto MapSLAPolicyToDto(CRM.Core.Entities.SLAPolicy entity)
    {
        return new SLAPolicyDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Priority = entity.Priority.ToString(),
            InitialResponseTimeMinutes = entity.InitialResponseTimeMinutes,
            ResolutionTimeMinutes = entity.ResolutionTimeMinutes,
            WorkingHoursOnly = entity.WorkingHoursOnly,
            EscalationPath = entity.EscalationPath,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private EscalationRuleDto MapEscalationRuleToDto(CRM.Core.Entities.ITSM.EscalationRule entity)
    {
        // TODO: ITSM.EscalationRule has different schema. Mapping approximation:
        // Conditions→Condition, Priority→ConditionMetric, AgeInMinutes→ThresholdValue,
        // TargetId→EscalateToUserId or EscalateToGroupId based on TargetType
        return new EscalationRuleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Condition = entity.Conditions ?? string.Empty,
            ConditionMetric = entity.Priority,
            ThresholdValue = entity.AgeInMinutes,
            EscalateToUserId = entity.TargetType == CRM.Core.Entities.ITSM.EscalationTargetType.User ? entity.TargetId : null,
            EscalateToGroupId = entity.TargetType == CRM.Core.Entities.ITSM.EscalationTargetType.Group ? entity.TargetId : null,
            SendNotification = true, // TODO: not available on ITSM.EscalationRule
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private ServiceQueueDto MapServiceQueueToDto(CRM.Core.Entities.ITSM.ServiceQueue entity)
    {
        // TODO: ITSM.ServiceQueue stores routing config as JSON in RoutingConfiguration.
        // Extract DisplayOrder, RoutingType, AssignedUserIds, AssignedGroupIds, SkillRequirements from JSON.
        System.Text.Json.JsonElement? config = null;
        if (!string.IsNullOrEmpty(entity.RoutingConfiguration))
            config = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(entity.RoutingConfiguration);
        return new ServiceQueueDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            RoutingType = config.HasValue && config.Value.TryGetProperty("RoutingType", out var rt) ? rt.GetString() ?? string.Empty : string.Empty,
            AssignedUserIds = config.HasValue && config.Value.TryGetProperty("AssignedUserIds", out var au)
                ? JsonSerializer.Deserialize<List<int>>(au.GetRawText()) ?? new()
                : new(),
            AssignedGroupIds = config.HasValue && config.Value.TryGetProperty("AssignedGroupIds", out var ag)
                ? JsonSerializer.Deserialize<List<int>>(ag.GetRawText()) ?? new()
                : new(),
            SkillRequirements = config.HasValue && config.Value.TryGetProperty("SkillRequirements", out var sr)
                ? JsonSerializer.Deserialize<List<string>>(sr.GetRawText()) ?? new()
                : new(),
            DisplayOrder = config.HasValue && config.Value.TryGetProperty("DisplayOrder", out var dOrd) && dOrd.TryGetInt32(out int doVal) ? doVal : 0,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    #endregion
}
