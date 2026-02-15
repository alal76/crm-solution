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

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class SLAService : ISLAService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly IBusinessHoursCalculator _businessHoursCalculator;
    private readonly ILogger<SLAService> _logger;

    public SLAService(
        IDbContextResolver dbContextResolver,
        IBusinessHoursCalculator businessHoursCalculator,
        ILogger<SLAService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _businessHoursCalculator = businessHoursCalculator;
        _logger = logger;
    }

    public async Task<SLAPolicyDto> CreateSLAPolicyAsync(SLAPolicyDto dto, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();

        var policy = new CRM.Core.Entities.ITSM.SLAPolicy
        {
            Name = dto.Name,
            TargetType = dto.TargetType,
            P1ResponseMinutes = dto.P1ResponseMinutes,
            P1ResolutionMinutes = dto.P1ResolutionMinutes,
            UseBusinessHours = dto.UseBusinessHours,
            IsActive = dto.IsActive,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        context.ITSMSLAPolicies.Add(policy);
        await context.SaveChangesAsync();

        return MapPolicyToDto(policy);
    }

    // Interface implementation - returns Task
    async Task ISLAService.StartSLAAsync(int targetId, SLATargetType targetType, int priority)
    {
        await StartSLAInternalAsync(targetId, targetType, priority);
    }

    private async Task<SLAInstanceDto?> StartSLAInternalAsync(int targetId, SLATargetType targetType, int priority)
    {
        var context = _dbContextResolver.ResolveContext();

        // Find matching policy based on priority (P1-P4)
        var policy = await context.ITSMSLAPolicies
            .FirstOrDefaultAsync(p => p.TargetType == targetType && p.IsActive);

        if (policy == null)
        {
            _logger.LogWarning("No SLA policy found for {TargetType}", targetType);
            return null;
        }

        // Calculate due dates based on priority
        var now = DateTime.UtcNow;
        int? responseMinutes = priority switch
        {
            1 => policy.P1ResponseMinutes,
            2 => policy.P2ResponseMinutes,
            3 => policy.P3ResponseMinutes,
            4 => policy.P4ResponseMinutes,
            _ => policy.P1ResponseMinutes
        };

        int? resolutionMinutes = priority switch
        {
            1 => policy.P1ResolutionMinutes,
            2 => policy.P2ResolutionMinutes,
            3 => policy.P3ResolutionMinutes,
            4 => policy.P4ResolutionMinutes,
            _ => policy.P1ResolutionMinutes
        };

        var instance = new CRM.Core.Entities.ITSM.SLAInstance
        {
            SLAPolicyId = policy.SLAPolicyId,
            TargetId = targetId,
            TargetType = targetType,
            ResponseDueAt = responseMinutes.HasValue ? now.AddMinutes(responseMinutes.Value) : null,
            ResolutionDueAt = resolutionMinutes.HasValue ? now.AddMinutes(resolutionMinutes.Value) : null,
            State = SLAState.Active,
            CreatedAt = now
        };

        context.ITSMSLAInstances.Add(instance);
        await context.SaveChangesAsync();

        _logger.LogInformation("Started SLA instance {SLAInstanceId} for {TargetType} {TargetId}",
            instance.SLAInstanceId, targetType, targetId);

        return MapInstanceToDto(instance, policy);
    }

    async Task ISLAService.PauseSLAAsync(int targetId, SLATargetType targetType, string reason)
    {
        await PauseSLAInternalAsync(targetId, targetType, reason);
    }

    private async Task<SLAInstanceDto?> PauseSLAInternalAsync(int targetId, SLATargetType targetType, string reason)
    {
        var context = _dbContextResolver.ResolveContext();

        var instance = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .FirstOrDefaultAsync(s => s.TargetId == targetId && s.TargetType == targetType && s.State == SLAState.Active);

        if (instance == null)
            return null;

        instance.State = SLAState.Paused;
        instance.PausedAt = DateTime.UtcNow;
        instance.PauseReason = reason;
        instance.PausedAt = DateTime.UtcNow;
        instance.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        _logger.LogInformation("Paused SLA for {TargetType} {TargetId}. Reason: {Reason}", targetType, targetId, reason);

        return MapInstanceToDto(instance, instance.SLAPolicy!);
    }

    async Task ISLAService.ResumeSLAAsync(int targetId, SLATargetType targetType)
    {
        await ResumeSLAInternalAsync(targetId, targetType);
    }

    private async Task<SLAInstanceDto?> ResumeSLAInternalAsync(int targetId, SLATargetType targetType)
    {
        var context = _dbContextResolver.ResolveContext();

        var instance = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .FirstOrDefaultAsync(s => s.TargetId == targetId && s.TargetType == targetType && s.State == SLAState.Paused);

        if (instance == null || !instance.PausedAt.HasValue)
            return null;

        var pausedDuration = DateTime.UtcNow - instance.PausedAt.Value;

        // Extend due dates by paused duration
        instance.ResponseDueAt = instance.ResponseDueAt?.Add(pausedDuration);
        instance.ResolutionDueAt = instance.ResolutionDueAt?.Add(pausedDuration);

        instance.State = SLAState.Active;
        instance.PausedAt = null;
        instance.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return MapInstanceToDto(instance, instance.SLAPolicy!);
    }

    async Task ISLAService.CompleteSLAAsync(int targetId, SLATargetType targetType, bool responseComplete, bool resolutionComplete)
    {
        await CompleteSLAInternalAsync(targetId, targetType, responseComplete, resolutionComplete);
    }

    private async Task<SLAInstanceDto?> CompleteSLAInternalAsync(int targetId, SLATargetType targetType, bool responseComplete, bool resolutionComplete)
    {
        var context = _dbContextResolver.ResolveContext();

        var instance = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .FirstOrDefaultAsync(s => s.TargetId == targetId && s.TargetType == targetType &&
                                    (s.State == SLAState.Active || s.State == SLAState.Paused));

        if (instance == null)
            return null;

        var now = DateTime.UtcNow;
        instance.State = SLAState.Completed;
        instance.State = SLAState.Completed;

        // Check for breaches based on what was completed
        if (responseComplete && instance.ResponseDueAt.HasValue && now > instance.ResponseDueAt.Value)
        {
            instance.ResponseBreached = true;
        }

        if (resolutionComplete && instance.ResolutionDueAt.HasValue && now > instance.ResolutionDueAt.Value)
        {
            instance.ResolutionBreached = true;
            instance.State = SLAState.Breached;
        }

        instance.ModifiedAt = now;
        await context.SaveChangesAsync();

        return MapInstanceToDto(instance, instance.SLAPolicy!);
    }

    public async Task<IEnumerable<SLAPolicyDto>> GetSLAPoliciesAsync(SLATargetType? targetType)
    {
        var context = _dbContextResolver.ResolveContext();

        var query = context.ITSMSLAPolicies.Where(p => !p.IsDeleted && p.IsActive);

        if (targetType.HasValue)
            query = query.Where(p => p.TargetType == targetType.Value);

        var policies = await query.OrderBy(p => p.Name).ToListAsync();
        return policies.Select(MapPolicyToDto);
    }

    public async Task<SLAInstanceDto?> GetSLAInstanceAsync(int targetId, SLATargetType targetType)
    {
        var context = _dbContextResolver.ResolveContext();

        var instance = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .FirstOrDefaultAsync(s => s.TargetId == targetId && s.TargetType == targetType &&
                                     (s.State == SLAState.Active || s.State == SLAState.Paused));

        return instance == null ? null : MapInstanceToDto(instance, instance.SLAPolicy!);
    }

    public async Task<IEnumerable<SLAInstanceDto>> GetBreachedSLAsAsync()
    {
        var context = _dbContextResolver.ResolveContext();

        var now = DateTime.UtcNow;
        var instances = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .Where(s => (s.State == SLAState.Active || s.State == SLAState.Paused) &&
                       ((s.ResponseDueAt.HasValue && s.ResponseDueAt.Value < now && !s.ResponseBreached) ||
                        (s.ResolutionDueAt.HasValue && s.ResolutionDueAt.Value < now && !s.ResolutionBreached)))
            .ToListAsync();

        return instances.Select(i => MapInstanceToDto(i, i.SLAPolicy!));
    }

    public async Task CheckSLABreachesAsync()
    {
        var context = _dbContextResolver.ResolveContext();
        var now = DateTime.UtcNow;

        var activeSLAs = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .Where(s => s.State == SLAState.Active)
            .ToListAsync();

        foreach (var sla in activeSLAs)
        {
            bool updated = false;

            // Check response time breach notifications
            if (sla.ResponseDueAt.HasValue && !sla.ResponseBreached)
            {
                var timeToResponse = sla.ResponseDueAt.Value - now;
                var totalResponseTime = sla.ResponseDueAt.Value - sla.CreatedAt;

                if (now >= sla.ResponseDueAt.Value)
                {
                    sla.ResponseBreached = true;
                    sla.ModifiedAt = now;
                    updated = true;
                }
                else if (timeToResponse <= totalResponseTime * 0.5 && !sla.Response50PercentNotificationSent)
                {
                    sla.Response50PercentNotificationSent = true;
                    updated = true;
                }
                else if (timeToResponse <= totalResponseTime * 0.25 && !sla.Response75PercentNotificationSent)
                {
                    sla.Response75PercentNotificationSent = true;
                    updated = true;
                }
            }

            // Check resolution time breach
            if (sla.ResolutionDueAt.HasValue && !sla.ResolutionBreached)
            {
                var timeToResolution = sla.ResolutionDueAt.Value - now;
                var totalResolutionTime = sla.ResolutionDueAt.Value - sla.CreatedAt;

                if (now >= sla.ResolutionDueAt.Value)
                {
                    sla.ResolutionBreached = true;
                    sla.State = SLAState.Breached;
                    sla.ModifiedAt = now;
                    updated = true;
                }
                else if (timeToResolution <= totalResolutionTime * 0.5 && !sla.Resolution50PercentNotificationSent)
                {
                    sla.Resolution50PercentNotificationSent = true;
                    updated = true;
                }
                else if (timeToResolution <= totalResolutionTime * 0.25 && !sla.Resolution75PercentNotificationSent)
                {
                    sla.Resolution75PercentNotificationSent = true;
                    updated = true;
                }
            }

            if (updated)
            {
                sla.ModifiedAt = now;
            }
        }

        if (activeSLAs.Any(s => s.ModifiedAt == now))
        {
            await context.SaveChangesAsync();
        }
    }

    private DateTime CalculateDueDate(DateTime startTime, int minutes, bool businessHoursOnly, int? scheduleId)
    {
        if (!businessHoursOnly)
        {
            return startTime.AddMinutes(minutes);
        }

        // Use BusinessHoursCalculator for proper business hours calculation
        // This is a synchronous context, so we use .GetAwaiter().GetResult() safely
        // In production, consider making calling methods async
        try
        {
            return _businessHoursCalculator.AddBusinessMinutesAsync(startTime, minutes, scheduleId)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate business hours due date, falling back to simple calculation");
            return startTime.AddMinutes(minutes);
        }
    }

    private SLAPolicyDto MapPolicyToDto(CRM.Core.Entities.ITSM.SLAPolicy policy)
    {
        return new SLAPolicyDto
        {
            SLAPolicyId = policy.SLAPolicyId,
            Name = policy.Name,
            TargetType = policy.TargetType,
            P1ResponseMinutes = policy.P1ResponseMinutes,
            P1ResolutionMinutes = policy.P1ResolutionMinutes,
            UseBusinessHours = policy.UseBusinessHours,
            IsActive = policy.IsActive
        };
    }

    private SLAInstanceDto MapInstanceToDto(CRM.Core.Entities.ITSM.SLAInstance instance, CRM.Core.Entities.ITSM.SLAPolicy policy)
    {
        var now = DateTime.UtcNow;

        return new SLAInstanceDto
        {
            SLAInstanceId = instance.SLAInstanceId,
            TargetId = instance.TargetId,
            TargetType = instance.TargetType,
            ResponseDueAt = instance.ResponseDueAt,
            ResolutionDueAt = instance.ResolutionDueAt,
            ResponseBreached = instance.ResponseBreached,
            ResolutionBreached = instance.ResolutionBreached,
            State = instance.State,
            MinutesUntilResponseBreach = instance.ResponseDueAt.HasValue && !instance.ResponseBreached
                ? (int?)Math.Max(0, (instance.ResponseDueAt.Value - now).TotalMinutes)
                : null,
            MinutesUntilResolutionBreach = instance.ResolutionDueAt.HasValue && !instance.ResolutionBreached
                ? (int?)Math.Max(0, (instance.ResolutionDueAt.Value - now).TotalMinutes)
                : null
        };
    }

    public async Task<SLADashboardInfo> GetSLADashboardAsync()
    {
        var context = _dbContextResolver.ResolveContext();
        var now = DateTime.UtcNow;
        var thresholdMinutes = 30;

        var allInstances = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .Where(s => s.State == SLAState.Active || s.State == SLAState.Paused || s.State == SLAState.Breached)
            .ToListAsync();

        var breachedInstances = allInstances
            .Where(s => s.ResponseBreached || s.ResolutionBreached)
            .ToList();

        var atRiskInstances = allInstances
            .Where(s => !s.ResponseBreached && !s.ResolutionBreached &&
                       ((s.ResponseDueAt.HasValue && (s.ResponseDueAt.Value - now).TotalMinutes <= thresholdMinutes) ||
                        (s.ResolutionDueAt.HasValue && (s.ResolutionDueAt.Value - now).TotalMinutes <= thresholdMinutes)))
            .ToList();

        var onTrackInstances = allInstances
            .Where(s => !s.ResponseBreached && !s.ResolutionBreached &&
                       !atRiskInstances.Contains(s))
            .ToList();

        var totalCompleted = await context.ITSMSLAInstances
            .Where(s => s.State == SLAState.Completed)
            .CountAsync();

        var totalBreached = await context.ITSMSLAInstances
            .Where(s => s.State == SLAState.Completed && (s.ResponseBreached || s.ResolutionBreached))
            .CountAsync();

        var complianceRate = totalCompleted > 0 ? (double)(totalCompleted - totalBreached) / totalCompleted * 100 : 100.0;

        return new SLADashboardInfo
        {
            TotalActiveSLAs = allInstances.Count,
            BreachedCount = breachedInstances.Count,
            AtRiskCount = atRiskInstances.Count,
            OnTrackCount = onTrackInstances.Count,
            OverallComplianceRate = Math.Round(complianceRate, 2),
            RecentBreaches = breachedInstances.Take(5).Select(i => MapInstanceToDto(i, i.SLAPolicy!)),
            AtRiskItems = atRiskInstances.Take(5).Select(i => MapInstanceToDto(i, i.SLAPolicy!))
        };
    }

    public async Task<IEnumerable<SLAInstanceDto>> GetAtRiskSLAsAsync(int thresholdMinutes)
    {
        var context = _dbContextResolver.ResolveContext();
        var now = DateTime.UtcNow;
        var threshold = now.AddMinutes(thresholdMinutes);

        var instances = await context.ITSMSLAInstances
            .Include(s => s.SLAPolicy)
            .Where(s => (s.State == SLAState.Active || s.State == SLAState.Paused) &&
                       !s.ResponseBreached && !s.ResolutionBreached &&
                       ((s.ResponseDueAt.HasValue && s.ResponseDueAt.Value <= threshold && s.ResponseDueAt.Value > now) ||
                        (s.ResolutionDueAt.HasValue && s.ResolutionDueAt.Value <= threshold && s.ResolutionDueAt.Value > now)))
            .OrderBy(s => s.ResponseDueAt ?? s.ResolutionDueAt)
            .ToListAsync();

        return instances.Select(i => MapInstanceToDto(i, i.SLAPolicy!));
    }

    public async Task<SLAMetricsInfo> GetSLAMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var context = _dbContextResolver.ResolveContext();

        var instances = await context.ITSMSLAInstances
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
            .ToListAsync();

        var totalIncidents = instances.Count(s => s.TargetType == SLATargetType.Incident);
        var totalBreaches = instances.Count(s => s.ResponseBreached || s.ResolutionBreached);
        var responseBreaches = instances.Count(s => s.ResponseBreached);
        var resolutionBreaches = instances.Count(s => s.ResolutionBreached);

        var completedInstances = instances.Where(s => s.State == SLAState.Completed).ToList();
        var responseComplianceRate = completedInstances.Count > 0
            ? (double)(completedInstances.Count - responseBreaches) / completedInstances.Count * 100
            : 100.0;
        var resolutionComplianceRate = completedInstances.Count > 0
            ? (double)(completedInstances.Count - resolutionBreaches) / completedInstances.Count * 100
            : 100.0;

        // Calculate average times (simplified - would need actual response/resolution timestamps in production)
        var avgResponseTime = 0.0;
        var avgResolutionTime = 0.0;

        return new SLAMetricsInfo
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncidents = totalIncidents,
            TotalBreaches = totalBreaches,
            ResponseComplianceRate = Math.Round(responseComplianceRate, 2),
            ResolutionComplianceRate = Math.Round(resolutionComplianceRate, 2),
            AverageResponseTimeMinutes = avgResponseTime,
            AverageResolutionTimeMinutes = avgResolutionTime,
            ComplianceByPriority = new Dictionary<int, double>()
        };
    }
}
