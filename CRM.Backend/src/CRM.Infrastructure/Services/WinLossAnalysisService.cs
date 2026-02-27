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
/// Win/Loss analysis service implementation.
/// TODO-CRM003-05: Implement win/loss analysis reports
/// </summary>
public class WinLossAnalysisService : IWinLossAnalysisService
{
    private readonly ICrmDbContext _context;

    public WinLossAnalysisService(ICrmDbContext context)
    {
        _context = context;
    }

    public async Task<WinLossSummary> GetSummaryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var opportunities = await _context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync(ct);

        var wins = opportunities.Where(o => o.Stage == OpportunityStage.ClosedWon).ToList();
        var losses = opportunities.Where(o => o.Stage == OpportunityStage.ClosedLost).ToList();
        var open = opportunities.Where(o => o.IsOpen).ToList();

        var totalClosed = wins.Count + losses.Count;

        return new WinLossSummary
        {
            TotalOpportunities = opportunities.Count,
            TotalWins = wins.Count,
            TotalLosses = losses.Count,
            StillOpen = open.Count,
            WinRate = totalClosed > 0 ? Math.Round((decimal)wins.Count / totalClosed * 100, 2) : 0,
            TotalWonAmount = wins.Sum(o => o.Amount),
            TotalLostAmount = losses.Sum(o => o.Amount),
            AverageWonDealSize = wins.Any() ? wins.Average(o => o.Amount) : 0,
            AverageLostDealSize = losses.Any() ? losses.Average(o => o.Amount) : 0,
            AverageDaysToWin = wins.Any() && wins.Any(w => w.ClosedDate.HasValue)
                ? wins.Where(w => w.ClosedDate.HasValue).Average(w => (w.ClosedDate!.Value - w.CreatedAt).TotalDays)
                : 0,
            AverageDaysToLose = losses.Any() && losses.Any(l => l.ClosedDate.HasValue)
                ? losses.Where(l => l.ClosedDate.HasValue).Average(l => (l.ClosedDate!.Value - l.CreatedAt).TotalDays)
                : 0,
            FromDate = from,
            ToDate = to
        };
    }

    public async Task<IEnumerable<WinLossByReason>> GetByReasonAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var losses = await _context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted &&
                        o.Stage == OpportunityStage.ClosedLost &&
                        o.CreatedAt >= from &&
                        o.CreatedAt <= to)
            .ToListAsync(ct);

        var totalLosses = losses.Count;

        return losses
            .GroupBy(o => o.LossReasonCategory ?? LossReasonCategory.None)
            .Select(g => new WinLossByReason
            {
                ReasonCategory = g.Key,
                ReasonName = g.Key.ToString(),
                Count = g.Count(),
                TotalAmount = g.Sum(o => o.Amount),
                Percentage = totalLosses > 0 ? Math.Round((decimal)g.Count() / totalLosses * 100, 2) : 0
            })
            .OrderByDescending(r => r.Count)
            .ToList();
    }

    public async Task<IEnumerable<WinLossByCompetitor>> GetByCompetitorAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var opportunities = await _context.Opportunities
            .AsNoTracking()
            .Include(o => o.Competitors)
            .ThenInclude(c => c.Competitor)
            .Where(o => !o.IsDeleted &&
                        (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost) &&
                        o.CreatedAt >= from &&
                        o.CreatedAt <= to)
            .ToListAsync(ct);

        var byCompetitor = new Dictionary<int, WinLossByCompetitor>();

        foreach (var opp in opportunities)
        {
            foreach (var oppCompetitor in opp.Competitors)
            {
                if (!byCompetitor.ContainsKey(oppCompetitor.CompetitorId))
                {
                    byCompetitor[oppCompetitor.CompetitorId] = new WinLossByCompetitor
                    {
                        CompetitorId = oppCompetitor.CompetitorId,
                        CompetitorName = oppCompetitor.Competitor?.Name ?? "Unknown"
                    };
                }

                var entry = byCompetitor[oppCompetitor.CompetitorId];
                entry.TotalDeals++;

                if (opp.Stage == OpportunityStage.ClosedWon)
                {
                    entry.WinsAgainst++;
                    entry.TotalWonAmount += opp.Amount;
                }
                else
                {
                    entry.LossesTo++;
                    entry.TotalLostAmount += opp.Amount;
                }
            }
        }

        return byCompetitor.Values
            .Select(c =>
            {
                c.WinRate = c.TotalDeals > 0
                    ? Math.Round((decimal)c.WinsAgainst / c.TotalDeals * 100, 2)
                    : 0;
                return c;
            })
            .OrderByDescending(c => c.TotalDeals)
            .ToList();
    }

    public async Task<IEnumerable<WinRateTrend>> GetWinRateTrendsAsync(
        DateTime fromDate,
        DateTime toDate,
        string period = "month",
        CancellationToken ct = default)
    {
        var opportunities = await _context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted &&
                        (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost) &&
                        o.ClosedDate.HasValue &&
                        o.ClosedDate >= fromDate &&
                        o.ClosedDate <= toDate)
            .ToListAsync(ct);

        var trends = new List<WinRateTrend>();
        var current = fromDate;

        while (current < toDate)
        {
            var periodEnd = period.ToLower() switch
            {
                "day" => current.AddDays(1),
                "week" => current.AddDays(7),
                "quarter" => current.AddMonths(3),
                _ => current.AddMonths(1) // month
            };

            if (periodEnd > toDate) periodEnd = toDate;

            var periodOpps = opportunities
                .Where(o => o.ClosedDate >= current && o.ClosedDate < periodEnd)
                .ToList();

            var wins = periodOpps.Count(o => o.Stage == OpportunityStage.ClosedWon);
            var losses = periodOpps.Count(o => o.Stage == OpportunityStage.ClosedLost);
            var total = wins + losses;

            trends.Add(new WinRateTrend
            {
                PeriodStart = current,
                PeriodEnd = periodEnd,
                PeriodLabel = current.ToString("MMM yyyy"),
                TotalOpportunities = total,
                Wins = wins,
                Losses = losses,
                WinRate = total > 0 ? Math.Round((decimal)wins / total * 100, 2) : 0,
                WonAmount = periodOpps.Where(o => o.Stage == OpportunityStage.ClosedWon).Sum(o => o.Amount),
                LostAmount = periodOpps.Where(o => o.Stage == OpportunityStage.ClosedLost).Sum(o => o.Amount)
            });

            current = periodEnd;
        }

        return trends;
    }

    public async Task<IEnumerable<WinLossBySalesRep>> GetBySalesRepAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var opportunities = await _context.Opportunities
            .AsNoTracking()
            .Include(o => o.SalesOwner)
            .Where(o => !o.IsDeleted &&
                        o.SalesOwnerId.HasValue &&
                        (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost) &&
                        o.CreatedAt >= from &&
                        o.CreatedAt <= to)
            .ToListAsync(ct);

        return opportunities
            .GroupBy(o => o.SalesOwnerId!.Value)
            .Select(g =>
            {
                var wins = g.Count(o => o.Stage == OpportunityStage.ClosedWon);
                var losses = g.Count(o => o.Stage == OpportunityStage.ClosedLost);
                var total = wins + losses;

                return new WinLossBySalesRep
                {
                    UserId = g.Key,
                    UserName = g.First().SalesOwner?.Username ?? "Unknown",
                    TotalOpportunities = total,
                    Wins = wins,
                    Losses = losses,
                    WinRate = total > 0 ? Math.Round((decimal)wins / total * 100, 2) : 0,
                    TotalWonAmount = g.Where(o => o.Stage == OpportunityStage.ClosedWon).Sum(o => o.Amount),
                    TotalLostAmount = g.Where(o => o.Stage == OpportunityStage.ClosedLost).Sum(o => o.Amount),
                    AverageDealSize = g.Average(o => o.Amount)
                };
            })
            .OrderByDescending(r => r.WinRate)
            .ToList();
    }

    public async Task<LossAnalysisReport> GetLossAnalysisAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var losses = await _context.Opportunities
            .AsNoTracking()
            .Include(o => o.Account)
            .Include(o => o.SalesOwner)
            .Include(o => o.CompetitorWinner)
            .Where(o => !o.IsDeleted &&
                        o.Stage == OpportunityStage.ClosedLost &&
                        o.CreatedAt >= from &&
                        o.CreatedAt <= to)
            .OrderByDescending(o => o.ClosedDate ?? o.UpdatedAt)
            .ToListAsync(ct);

        var byReason = await GetByReasonAsync(fromDate, toDate, ct);
        var byCompetitor = await GetByCompetitorAsync(fromDate, toDate, ct);

        return new LossAnalysisReport
        {
            TotalLosses = losses.Count,
            TotalLostAmount = losses.Sum(o => o.Amount),
            ByReason = byReason,
            ByCompetitor = byCompetitor,
            TopLossReason = byReason.FirstOrDefault()?.ReasonName ?? "N/A",
            TopCompetitor = byCompetitor.FirstOrDefault()?.CompetitorName ?? "N/A",
            AverageDaysToLose = losses.Any() && losses.Any(l => l.ClosedDate.HasValue)
                ? losses.Where(l => l.ClosedDate.HasValue).Average(l => (l.ClosedDate!.Value - l.CreatedAt).TotalDays)
                : 0,
            RecentLosses = losses.Take(10).Select(o => new LostOpportunityDetail
            {
                OpportunityId = o.Id,
                OpportunityName = o.Name,
                AccountName = o.Account?.DisplayName ?? "Unknown",
                Amount = o.Amount,
                ClosedDate = o.ClosedDate,
                LossReasonCategory = o.LossReasonCategory,
                LossReason = o.LossReason,
                CompetitorWinner = o.CompetitorWinner?.Name,
                SalesOwner = o.SalesOwner?.Username
            }).ToList()
        };
    }

    public async Task<IEnumerable<WinLossByDealSize>> GetByDealSizeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var opportunities = await _context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted &&
                        (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost) &&
                        o.CreatedAt >= from &&
                        o.CreatedAt <= to)
            .ToListAsync(ct);

        var segments = new[]
        {
            ("< $10K", 0m, 10000m),
            ("$10K - $50K", 10000m, 50000m),
            ("$50K - $100K", 50000m, 100000m),
            ("$100K - $500K", 100000m, 500000m),
            ("> $500K", 500000m, decimal.MaxValue)
        };

        return segments.Select(seg =>
        {
            var segOpps = opportunities.Where(o => o.Amount >= seg.Item2 && o.Amount < seg.Item3).ToList();
            var wins = segOpps.Count(o => o.Stage == OpportunityStage.ClosedWon);
            var losses = segOpps.Count(o => o.Stage == OpportunityStage.ClosedLost);
            var total = wins + losses;

            return new WinLossByDealSize
            {
                Segment = seg.Item1,
                MinAmount = seg.Item2,
                MaxAmount = seg.Item3 == decimal.MaxValue ? 1000000m : seg.Item3,
                TotalDeals = total,
                Wins = wins,
                Losses = losses,
                WinRate = total > 0 ? Math.Round((decimal)wins / total * 100, 2) : 0,
                TotalWonAmount = segOpps.Where(o => o.Stage == OpportunityStage.ClosedWon).Sum(o => o.Amount),
                TotalLostAmount = segOpps.Where(o => o.Stage == OpportunityStage.ClosedLost).Sum(o => o.Amount)
            };
        }).ToList();
    }
}
