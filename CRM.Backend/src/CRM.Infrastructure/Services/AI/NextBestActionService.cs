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

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// Heuristic-based service for generating next best action recommendations.
/// Implements TODO-AI-04.
/// </summary>
public class NextBestActionService : INextBestActionService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<NextBestActionService> _logger;

    public NextBestActionService(ICrmDbContext db, ILogger<NextBestActionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<NextBestActionDto>> GetRecommendationsAsync(
        int accountId, CancellationToken ct = default)
    {
        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, ct);

        if (account is null) return Enumerable.Empty<NextBestActionDto>();

        var actions = new List<NextBestActionDto>();

        // --- Signal: Last interaction ---
        var lastInteraction = await _db.Interactions
            .AsNoTracking()
            .Where(i => i.AccountId == accountId && !i.IsDeleted)
            .OrderByDescending(i => i.InteractionDate)
            .Select(i => i.InteractionDate)
            .FirstOrDefaultAsync(ct);

        var daysSinceContact = lastInteraction == default
            ? 999
            : (DateTime.UtcNow - lastInteraction).TotalDays;

        if (daysSinceContact > 30)
        {
            actions.Add(new NextBestActionDto
            {
                ActionType = NextBestActionType.ScheduleCall,
                Priority = daysSinceContact > 90 ? 1 : 2,
                Rationale = $"No contact in {(int)daysSinceContact} days — re-engage the account.",
                DueDate = DateTime.UtcNow.AddDays(3),
                ActionLabel = "Schedule check-in call"
            });
        }

        // --- Signal: Open opportunities ---
        var openOpps = await _db.Opportunities
            .AsNoTracking()
            .Where(o => o.AccountId == accountId && !o.IsDeleted &&
                        o.Stage != OpportunityStage.ClosedWon &&
                        o.Stage != OpportunityStage.ClosedLost)
            .Select(o => new { o.Id, o.Stage, o.ExpectedCloseDate, o.Amount })
            .ToListAsync(ct);

        if (openOpps.Count == 0)
        {
            actions.Add(new NextBestActionDto
            {
                ActionType = NextBestActionType.CreateOpportunity,
                Priority = 3,
                Rationale = "No active opportunities. Consider proposing a new product or renewal.",
                DueDate = DateTime.UtcNow.AddDays(7),
                ActionLabel = "Create new opportunity"
            });
        }
        else
        {
            // Opportunity close date approaching within 14 days
            var urgentOpp = openOpps.FirstOrDefault(o =>
                o.ExpectedCloseDate.HasValue &&
                o.ExpectedCloseDate.Value <= DateTime.UtcNow.AddDays(14));

            if (urgentOpp is not null)
            {
                actions.Add(new NextBestActionDto
                {
                    ActionType = NextBestActionType.ScheduleDemo,
                    Priority = 1,
                    Rationale = "Opportunity close date approaching — push for a decision.",
                    DueDate = urgentOpp.ExpectedCloseDate,
                    ActionLabel = "Schedule closing demo / negotiation call"
                });
            }

            // High-value opportunity
            var highValue = openOpps.FirstOrDefault(o => o.Amount >= 10000);
            if (highValue is not null)
            {
                actions.Add(new NextBestActionDto
                {
                    ActionType = NextBestActionType.SendEmail,
                    Priority = 2,
                    Rationale = "High-value opportunity in pipeline — send tailored proposal.",
                    DueDate = DateTime.UtcNow.AddDays(2),
                    ActionLabel = "Send tailored proposal email"
                });
            }
        }

        // --- Signal: Open tickets ---
        var openTickets = await _db.ServiceRequests
            .AsNoTracking()
            .CountAsync(s => s.AccountId == accountId && !s.IsDeleted &&
                             (s.Status == ServiceRequestStatus.Open ||
                              s.Status == ServiceRequestStatus.InProgress), ct);

        if (openTickets >= 2)
        {
            actions.Add(new NextBestActionDto
            {
                ActionType = NextBestActionType.AssignTicket,
                Priority = 2,
                Rationale = $"{openTickets} open support tickets — prioritise resolution to retain account.",
                DueDate = DateTime.UtcNow.AddDays(1),
                ActionLabel = "Review and prioritise open tickets"
            });
        }

        // Sort by priority ascending (1 = highest)
        _logger.LogDebug("Generated {Count} next-best-action recommendations for account {AccountId}",
            actions.Count, accountId);

        return actions.OrderBy(a => a.Priority);
    }
}
