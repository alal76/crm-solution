// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Lead Aging Alert Service (TODO-CRM002-07)
/// Identifies stale leads that need attention.
/// </summary>
public class LeadAgingAlertService : ILeadAgingAlertService
{
    private readonly ICrmDbContext _dbContext;

    public LeadAgingAlertService(ICrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Lead>> GetStaleLeadsAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-daysThreshold);

        return await _dbContext.Leads
            .Where(l => !l.IsDeleted)
            .Where(l => l.Status != LeadLifecycleStatus.Converted && l.Status != LeadLifecycleStatus.Disqualified)
            .Where(l => l.LastActivityDate == null || l.LastActivityDate < thresholdDate)
            .OrderBy(l => l.LastActivityDate ?? l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetStaleLeadsByOwnerAsync(int ownerId, int daysThreshold, CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-daysThreshold);

        return await _dbContext.Leads
            .Where(l => !l.IsDeleted && l.OwnerId == ownerId)
            .Where(l => l.Status != LeadLifecycleStatus.Converted && l.Status != LeadLifecycleStatus.Disqualified)
            .Where(l => l.LastActivityDate == null || l.LastActivityDate < thresholdDate)
            .OrderBy(l => l.LastActivityDate ?? l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadAgingStatistics> GetAgingStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var openLeads = await _dbContext.Leads
            .Where(l => !l.IsDeleted)
            .Where(l => l.Status != LeadLifecycleStatus.Converted && l.Status != LeadLifecycleStatus.Disqualified)
            .Select(l => new { l.Id, AgeDate = l.LastActivityDate ?? l.CreatedAt })
            .ToListAsync(cancellationToken);

        var stats = new LeadAgingStatistics
        {
            TotalOpenLeads = openLeads.Count
        };

        if (openLeads.Count == 0)
        {
            return stats;
        }

        foreach (var lead in openLeads)
        {
            var ageDays = (now - lead.AgeDate).TotalDays;
            
            if (ageDays < 7)
            {
                stats.Under7Days++;
            }
            else if (ageDays < 15)
            {
                stats.Days7To14++;
            }
            else if (ageDays < 31)
            {
                stats.Days15To30++;
            }
            else if (ageDays < 61)
            {
                stats.Days31To60++;
            }
            else
            {
                stats.Over60Days++;
            }
        }

        stats.AverageAgeDays = openLeads.Average(l => (now - l.AgeDate).TotalDays);

        return stats;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LeadAgingAlertDto>> GetStaledLeadsAsync(
        int staleDaysThreshold,
        CancellationToken ct = default)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-staleDaysThreshold);
        var criticalDays = Math.Max(30, staleDaysThreshold * 2);

        var staleLeads = await _dbContext.Leads
            .Where(l => !l.IsDeleted)
            .Where(l => l.Status != LeadLifecycleStatus.Converted && l.Status != LeadLifecycleStatus.Disqualified)
            .Where(l => l.LastActivityDate == null || l.LastActivityDate < thresholdDate)
            .OrderBy(l => l.LastActivityDate ?? l.CreatedAt)
            .Select(l => new
            {
                l.Id,
                l.FirstName,
                l.LastName,
                l.OwnerId,
                l.LastActivityDate,
                l.CreatedAt
            })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        return staleLeads.Select(l =>
        {
            var refDate = l.LastActivityDate ?? l.CreatedAt;
            var daysSince = (int)(now - refDate).TotalDays;
            return new LeadAgingAlertDto
            {
                LeadId = l.Id,
                LeadName = $"{l.FirstName} {l.LastName}".Trim(),
                AssignedToUserId = l.OwnerId,
                DaysSinceLastActivity = daysSince,
                LastActivityDate = l.LastActivityDate,
                StalenessLevel = daysSince >= criticalDays ? "Critical" : "Warning"
            };
        }).ToList();
    }
}
