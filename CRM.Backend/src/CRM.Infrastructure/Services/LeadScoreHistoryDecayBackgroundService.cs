// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that applies a 5 % score decay every 6 hours to leads whose
/// <c>LastScoreDecayDate</c> is older than 14 days (or has never been set).
/// Records every decay event via <see cref="ILeadScoreHistoryService"/>.
/// FEAT-AISCORING: AI Lead Scoring Real-time Triggers
/// </summary>
public class LeadScoreHistoryDecayBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LeadScoreHistoryDecayBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    public LeadScoreHistoryDecayBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LeadScoreHistoryDecayBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LeadScoreHistoryDecayBackgroundService starting (interval: {Interval})", Interval);

        // Allow the application to fully start before the first run
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunDecayPassAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeadScoreHistoryDecayBackgroundService");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("LeadScoreHistoryDecayBackgroundService stopped");
    }

    private async Task RunDecayPassAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
        var historyService = scope.ServiceProvider.GetRequiredService<ILeadScoreHistoryService>();

        var now = DateTime.UtcNow;
        var threshold = now.AddDays(-14);

        // Find active leads where decay is due:
        //   - Not deleted
        //   - Not spam / closed-won / closed-lost (only New, Contacted, Qualified, Nurturing)
        //   - FitScore > 0 (never decay already-zero leads)
        //   - Have not been decayed in the last 14 days
        var dueLeads = await context.Set<Lead>()
            .AsNoTracking()
            .Where(l =>
                !l.IsDeleted &&
                l.FitScore > 0 &&
                l.Status != LeadLifecycleStatus.Converted &&
                l.Status != LeadLifecycleStatus.Disqualified &&
                (!l.LastScoreDecayDate.HasValue || l.LastScoreDecayDate < threshold))
            .Select(l => l.Id)
            .Take(500)
            .ToListAsync(ct);

        if (dueLeads.Count == 0)
        {
            _logger.LogDebug("LeadScoreHistoryDecay: no leads due for decay this cycle");
            return;
        }

        _logger.LogInformation("LeadScoreHistoryDecay: applying decay to {Count} leads", dueLeads.Count);

        foreach (var leadId in dueLeads)
        {
            try
            {
                await historyService.ApplyDecayAsync(leadId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply decay to Lead {LeadId}", leadId);
            }
        }

        _logger.LogInformation("LeadScoreHistoryDecay: completed decay pass for {Count} leads", dueLeads.Count);
    }
}
