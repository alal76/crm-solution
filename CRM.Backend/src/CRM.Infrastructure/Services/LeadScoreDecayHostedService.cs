// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that applies score decay to inactive leads.
/// Runs periodically to check for leads that haven't had activity beyond the decay threshold
/// and reduces their scores according to configured decay rules.
///
/// Part of Marketing &amp; Sales implementation (G7 - Score Decay Background Job)
/// </summary>
public class LeadScoreDecayHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeadScoreDecayHostedService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly bool _isEnabled;

    public LeadScoreDecayHostedService(
        IServiceProvider serviceProvider,
        ILogger<LeadScoreDecayHostedService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Configuration: how often to check for decay (default: every 6 hours)
        var intervalHours = configuration.GetValue<int>("LeadScoring:DecayCheckIntervalHours", 6);
        _checkInterval = TimeSpan.FromHours(intervalHours);

        // Configuration: enable/disable decay processing
        _isEnabled = configuration.GetValue<bool>("LeadScoring:EnableDecay", true);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Lead Score Decay Service is disabled via configuration");
            return;
        }

        _logger.LogInformation("Lead Score Decay Service starting (check interval: {Interval})", _checkInterval);

        // Initial delay to allow application startup
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScoreDecayAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in lead score decay service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Lead Score Decay Service stopped");
    }

    private async Task ProcessScoreDecayAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();

        _logger.LogDebug("Starting lead score decay processing...");

        // Get all active decay rules
        var decayRules = await context.LeadScoreRules
            .Where(r => r.RuleType == LeadScoreRuleType.Decay && r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);

        if (!decayRules.Any())
        {
            _logger.LogDebug("No active decay rules configured");
            return;
        }

        var now = DateTime.UtcNow;
        var totalDecayed = 0;
        var totalPointsReduced = 0;

        foreach (var rule in decayRules)
        {
            var (count, points) = await ApplyDecayRuleAsync(context, rule, now, cancellationToken);
            totalDecayed += count;
            totalPointsReduced += points;
        }

        if (totalDecayed > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Lead score decay completed: {LeadCount} leads affected, {TotalPoints} total points reduced",
                totalDecayed, totalPointsReduced);
        }
        else
        {
            _logger.LogDebug("No leads qualified for score decay this cycle");
        }
    }

    private async Task<(int count, int totalPoints)> ApplyDecayRuleAsync(
        CrmDbContext context,
        LeadScoreRule rule,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (!rule.DecayDaysThreshold.HasValue || !rule.DecayPointsPerPeriod.HasValue)
        {
            _logger.LogWarning("Decay rule {RuleName} (ID: {RuleId}) has invalid configuration", rule.Name, rule.Id);
            return (0, 0);
        }

        var inactivityThreshold = now.AddDays(-rule.DecayDaysThreshold.Value);
        var decayPeriodDays = rule.DecayPeriodDays ?? 7;
        var pointsToDecay = Math.Abs(rule.DecayPointsPerPeriod.Value); // Always positive for calculation

        // Find leads that:
        // 1. Have a positive lead score
        // 2. Haven't had any activity since the threshold date
        // 3. Haven't been decayed too recently (based on decay period)
        var leadsToDecay = await context.Set<Lead>()
            .Where(l => !l.IsDeleted)
            .Where(l => l.LeadScore > 0)
            .Where(l => !l.LastActivityDate.HasValue || l.LastActivityDate < inactivityThreshold)
            .Where(l => !l.LastScoreDecayDate.HasValue || l.LastScoreDecayDate < now.AddDays(-decayPeriodDays))
            .Take(500) // Process in batches to avoid memory issues
            .ToListAsync(cancellationToken);

        if (!leadsToDecay.Any())
        {
            return (0, 0);
        }

        var count = 0;
        var totalPointsReduced = 0;

        foreach (var lead in leadsToDecay)
        {
            var previousScore = lead.LeadScore;
            lead.LeadScore = Math.Max(0, lead.LeadScore - pointsToDecay);
            lead.LastScoreDecayDate = now;
            lead.UpdatedAt = now;

            var pointsReduced = previousScore - lead.LeadScore;
            totalPointsReduced += pointsReduced;
            count++;

            // Log significant decay events
            if (pointsReduced > 0 && (previousScore >= 50 && lead.LeadScore < 50))
            {
                _logger.LogInformation(
                    "Lead {LeadId} score dropped below 50 due to inactivity: {OldScore} -> {NewScore}",
                    lead.Id, previousScore, lead.LeadScore);
            }

            // Create an activity record for audit trail
            var activity = new Activity
            {
                ActivityType = ActivityType.StatusChanged,
                Title = $"Score decayed by {pointsReduced} points",
                Description = $"Lead score reduced from {previousScore} to {lead.LeadScore} due to inactivity (Rule: {rule.Name})",
                EntityType = "Lead",
                EntityId = lead.Id,
                EntityName = $"{lead.FirstName} {lead.LastName}",
                IsSystem = true,
                ActivityDate = now,
                CreatedAt = now,
                OldValue = previousScore.ToString(),
                NewValue = lead.LeadScore.ToString(),
                Source = "ScoreDecayService"
            };

            context.Activities.Add(activity);
        }

        _logger.LogDebug(
            "Applied decay rule '{RuleName}': {Count} leads, {Points} points reduced",
            rule.Name, count, totalPointsReduced);

        return (count, totalPointsReduced);
    }
}

/// <summary>
/// Extension to add LastScoreDecayDate to Lead entity if not present.
/// This should be added to the Lead entity class.
/// </summary>
public static class LeadScoreDecayExtensions
{
    /// <summary>
    /// Register the decay service in DI
    /// </summary>
    public static IServiceCollection AddLeadScoreDecayService(this IServiceCollection services)
    {
        services.AddHostedService<LeadScoreDecayHostedService>();
        return services;
    }
}
