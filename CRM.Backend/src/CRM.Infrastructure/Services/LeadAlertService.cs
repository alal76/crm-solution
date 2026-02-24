// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Lead alert service for stale and aging lead notifications.
/// TODO-CRM002-07: Add lead aging alerts and stale lead notifications
/// </summary>
public class LeadAlertService : ILeadAlertService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadAlertService> _logger;

    public LeadAlertService(ICrmDbContext context, ILogger<LeadAlertService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<StaleLeadAlert>> CheckStaleLeadsAsync(
        int staleDaysThreshold = 7,
        CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-staleDaysThreshold);

        var staleLeads = await _context.Leads
            .AsNoTracking()
            .Include(l => l.Owner)
            .Where(l => !l.IsDeleted &&
                        l.Status != LeadLifecycleStatus.Converted &&
                        l.Status != LeadLifecycleStatus.Disqualified &&
                        (l.LastContactedAt == null || l.LastContactedAt < cutoffDate))
            .ToListAsync(ct);

        return staleLeads.Select(l =>
        {
            var daysSinceContact = l.LastContactedAt.HasValue
                ? (int)(DateTime.UtcNow - l.LastContactedAt.Value).TotalDays
                : (int)(DateTime.UtcNow - l.CreatedAt).TotalDays;

            return new StaleLeadAlert
            {
                LeadId = l.Id,
                LeadName = l.FullName,
                Email = l.Email,
                Company = l.CompanyName,
                Status = l.Status,
                OwnerId = l.OwnerId,
                OwnerName = l.Owner?.UserName,
                LastContactedAt = l.LastContactedAt,
                DaysSinceLastContact = daysSinceContact,
                Score = l.Score,
                AlertLevel = GetAlertLevel(daysSinceContact, staleDaysThreshold)
            };
        }).OrderByDescending(a => a.DaysSinceLastContact).ToList();
    }

    public async Task<IEnumerable<AgingLeadAlert>> GetAgingLeadsAsync(
        int agingDaysThreshold = 14,
        CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-agingDaysThreshold);

        var agingLeads = await _context.Leads
            .AsNoTracking()
            .Include(l => l.Owner)
            .Where(l => !l.IsDeleted &&
                        l.Status == LeadLifecycleStatus.New &&
                        l.CreatedAt < cutoffDate)
            .ToListAsync(ct);

        return agingLeads.Select(l =>
        {
            var totalAgeDays = (int)(DateTime.UtcNow - l.CreatedAt).TotalDays;

            return new AgingLeadAlert
            {
                LeadId = l.Id,
                LeadName = l.FullName,
                Email = l.Email,
                Company = l.CompanyName,
                Status = l.Status,
                OwnerId = l.OwnerId,
                OwnerName = l.Owner?.UserName,
                CreatedAt = l.CreatedAt,
                DaysInCurrentStatus = totalAgeDays, // Simplified: using total age
                TotalAgeDays = totalAgeDays,
                AlertLevel = GetAlertLevel(totalAgeDays, agingDaysThreshold)
            };
        }).OrderByDescending(a => a.TotalAgeDays).ToList();
    }

    public async Task<IEnumerable<AtRiskLeadAlert>> GetAtRiskLeadsAsync(CancellationToken ct = default)
    {
        // Get leads with declining scores (score decay detected)
        var atRiskLeads = await _context.Leads
            .AsNoTracking()
            .Include(l => l.Owner)
            .Where(l => !l.IsDeleted &&
                        l.Status != LeadLifecycleStatus.Converted &&
                        l.Status != LeadLifecycleStatus.Disqualified &&
                        l.LastScoreDecayDate != null &&
                        l.Score < 50) // Low score threshold
            .ToListAsync(ct);

        return atRiskLeads.Select(l => new AtRiskLeadAlert
        {
            LeadId = l.Id,
            LeadName = l.FullName,
            Email = l.Email,
            Company = l.CompanyName,
            CurrentScore = l.Score,
            PreviousScore = l.Score + 10, // Estimated previous (actual would need history)
            ScoreDropPercent = 10,
            RiskReason = l.Score < 30 ? "Very low engagement score" : "Declining engagement",
            OwnerId = l.OwnerId,
            OwnerName = l.Owner?.UserName
        }).ToList();
    }

    public async Task<LeadAlertStatistics> GetAlertStatisticsAsync(CancellationToken ct = default)
    {
        var staleLeads = await CheckStaleLeadsAsync(7, ct);
        var agingLeads = await GetAgingLeadsAsync(14, ct);
        var atRiskLeads = await GetAtRiskLeadsAsync(ct);

        var neverContacted = await _context.Leads
            .AsNoTracking()
            .CountAsync(l => !l.IsDeleted &&
                            l.Status != LeadLifecycleStatus.Converted &&
                            l.Status != LeadLifecycleStatus.Disqualified &&
                            l.LastContactedAt == null, ct);

        var avgDaysSinceContact = staleLeads.Any()
            ? staleLeads.Average(l => l.DaysSinceLastContact)
            : 0;

        var stats = new LeadAlertStatistics
        {
            TotalStaleLeads = staleLeads.Count(),
            TotalAgingLeads = agingLeads.Count(),
            TotalAtRiskLeads = atRiskLeads.Count(),
            LeadsNeverContacted = neverContacted,
            AverageDaysSinceContact = avgDaysSinceContact,
            HighPriorityAlerts = staleLeads.Count(l => l.AlertLevel == "Critical" || l.AlertLevel == "High"),
            AlertsByOwner = staleLeads
                .Where(l => l.OwnerName != null)
                .GroupBy(l => l.OwnerName!)
                .ToDictionary(g => g.Key, g => g.Count()),
            StaleByStatus = staleLeads
                .GroupBy(l => l.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    public async Task MarkLeadContactedAsync(int leadId, CancellationToken ct = default)
    {
        var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct);
        if (lead != null)
        {
            lead.LastContactedAt = DateTime.UtcNow;
            lead.UpdatedAt = DateTime.UtcNow;
            await (_context as DbContext)!.SaveChangesAsync(ct);
            _logger.LogInformation("Lead {LeadId} marked as contacted", leadId);
        }
    }

    public async Task<int> SendStaleLeadNotificationsAsync(int staleDaysThreshold = 7, CancellationToken ct = default)
    {
        var staleLeads = await CheckStaleLeadsAsync(staleDaysThreshold, ct);
        var notificationCount = 0;

        // Group by owner for batch notifications
        var byOwner = staleLeads
            .Where(l => l.OwnerId.HasValue)
            .GroupBy(l => l.OwnerId!.Value);

        foreach (var group in byOwner)
        {
            // In a real implementation, this would send emails/notifications
            _logger.LogInformation(
                "Would send stale lead notification to user {UserId} for {Count} leads",
                group.Key,
                group.Count());
            notificationCount++;
        }

        return notificationCount;
    }

    private static string GetAlertLevel(int days, int threshold)
    {
        var ratio = (double)days / threshold;
        if (ratio >= 3) return "Critical";
        if (ratio >= 2) return "High";
        if (ratio >= 1) return "Medium";
        return "Low";
    }
}
