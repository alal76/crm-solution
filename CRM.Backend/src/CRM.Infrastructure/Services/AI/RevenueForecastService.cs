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
/// Generates revenue forecasts from weighted pipeline data and historical trends.
/// Implements TODO-AI-10.
/// </summary>
public class RevenueForecastService : IRevenueForecastService
{
    // Stage probability weights for weighted pipeline calculation
    private static readonly Dictionary<OpportunityStage, double> StageProbabilities = new()
    {
        [OpportunityStage.Discovery]    = 0.10,
        [OpportunityStage.Qualification] = 0.25,
        [OpportunityStage.Proposal]     = 0.50,
        [OpportunityStage.Negotiation]  = 0.75,
        [OpportunityStage.ClosedWon]    = 1.00,
        [OpportunityStage.ClosedLost]   = 0.00
    };

    private readonly ICrmDbContext _db;
    private readonly ILogger<RevenueForecastService> _logger;

    public RevenueForecastService(ICrmDbContext db, ILogger<RevenueForecastService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<RevenueForecastDto> ForecastRevenueAsync(int months = 6, CancellationToken ct = default)
    {
        months = Math.Clamp(months, 1, 24);

        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endDate = startDate.AddMonths(months);

        // Fetch all relevant opportunities
        var opportunities = await _db.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted &&
                        o.Stage != OpportunityStage.ClosedLost &&
                        (o.ExpectedCloseDate == null || o.ExpectedCloseDate < endDate))
            .Select(o => new
            {
                o.Amount,
                o.Stage,
                o.Probability,
                o.ExpectedCloseDate,
                o.ClosedDate
            })
            .ToListAsync(ct);

        var forecastMonths = new List<ForecastMonthDto>();
        decimal totalForecast = 0;

        for (int i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var label = monthStart.ToString("yyyy-MM");

            // Already closed this month
            var closed = opportunities
                .Where(o => o.Stage == OpportunityStage.ClosedWon &&
                            o.ClosedDate.HasValue &&
                            o.ClosedDate >= monthStart &&
                            o.ClosedDate < monthEnd)
                .Sum(o => o.Amount);

            // Pipeline expected to close this month (weighted)
            var pipeline = opportunities
                .Where(o => o.Stage != OpportunityStage.ClosedWon &&
                            o.Stage != OpportunityStage.ClosedLost &&
                            o.ExpectedCloseDate.HasValue &&
                            o.ExpectedCloseDate >= monthStart &&
                            o.ExpectedCloseDate < monthEnd)
                .Sum(o =>
                {
                    var stagePct = StageProbabilities.TryGetValue(o.Stage, out var p) ? p : 0.1;
                    var manualPct = o.Probability / 100.0;
                    var blended = (stagePct + manualPct) / 2.0;
                    return o.Amount * (decimal)blended;
                });

            var monthForecast = closed + pipeline;
            totalForecast += monthForecast;

            forecastMonths.Add(new ForecastMonthDto
            {
                Month = label,
                ForecastedRevenue = monthForecast,
                ConfidenceLow = monthForecast * 0.75m,
                ConfidenceHigh = monthForecast * 1.25m,
                PipelineRevenue = pipeline,
                ClosedRevenue = closed
            });
        }

        _logger.LogDebug("Revenue forecast generated: {Months} months, total={Total:C}",
            months, totalForecast);

        return new RevenueForecastDto
        {
            Months = forecastMonths.ToArray(),
            TotalForecastedRevenue = totalForecast,
            OverallConfidencePct = 70,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
