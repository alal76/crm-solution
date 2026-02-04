// Temporarily disabled - requires entity alignment refactoring
#if ITSM_ADVANCED
// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Background service for automatic escalation of incidents and tickets based on SLA thresholds.
/// Runs periodically to check for items that need escalation.
/// </summary>
public class EscalationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EscalationHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public EscalationHostedService(
        IServiceProvider serviceProvider,
        ILogger<EscalationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Escalation Service starting...");

        // Initial delay to allow application startup
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEscalationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in escalation service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Escalation Service stopped");
    }

    private async Task ProcessEscalationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContextResolver = scope.ServiceProvider.GetRequiredService<IDbContextResolver>();
        var context = dbContextResolver.ResolveContext();

        var now = DateTime.UtcNow;
        var escalationsProcessed = 0;

        // Process incident escalations
        escalationsProcessed += await ProcessIncidentEscalationsAsync(context, now);

        // Process service request escalations
        escalationsProcessed += await ProcessServiceRequestEscalationsAsync(context, now);

        if (escalationsProcessed > 0)
        {
            _logger.LogInformation("Processed {Count} escalations", escalationsProcessed);
        }
    }

    private async Task<int> ProcessIncidentEscalationsAsync(ICrmDbContext context, DateTime now)
    {
        var count = 0;

        // Get escalation rules (time-based thresholds)
        var escalationRules = GetEscalationRules();

        // Find incidents that need escalation based on SLA breach or time thresholds
        var activeIncidents = await context.Incidents
            .Where(i => i.Status != IncidentState.Closed && 
                       i.Status != IncidentState.Resolved &&
                       i.Status != IncidentState.Cancelled)
            .Include(i => i.AssignedTo)
            .ToListAsync();

        foreach (var incident in activeIncidents)
        {
            var shouldEscalate = false;
            var escalationReason = string.Empty;

            // Check SLA-based escalation
            var slaInstance = await context.ITSMSLAInstances
                .FirstOrDefaultAsync(s => s.TargetId == incident.IncidentId && 
                                          s.TargetType == SLATargetType.Incident &&
                                          s.State == SLAState.Active);

            if (slaInstance != null)
            {
                // Response SLA at risk (80% of time elapsed)
                if (slaInstance.ResponseDueAt.HasValue && !slaInstance.ResponseMetAt.HasValue)
                {
                    var responsePercentage = CalculateTimePercentage(incident.CreatedAt, slaInstance.ResponseDueAt.Value, now);
                    if (responsePercentage >= 80 && incident.EscalationLevel == 0)
                    {
                        shouldEscalate = true;
                        escalationReason = $"Response SLA at {responsePercentage:F0}% - approaching breach";
                    }
                }

                // Resolution SLA at risk
                if (slaInstance.ResolutionDueAt.HasValue)
                {
                    var resolutionPercentage = CalculateTimePercentage(incident.CreatedAt, slaInstance.ResolutionDueAt.Value, now);
                    
                    // Escalate at different levels based on percentage
                    if (resolutionPercentage >= 100 && incident.EscalationLevel < 3)
                    {
                        shouldEscalate = true;
                        escalationReason = "Resolution SLA BREACHED";
                    }
                    else if (resolutionPercentage >= 90 && incident.EscalationLevel < 2)
                    {
                        shouldEscalate = true;
                        escalationReason = $"Resolution SLA at {resolutionPercentage:F0}% - critical";
                    }
                    else if (resolutionPercentage >= 75 && incident.EscalationLevel < 1)
                    {
                        shouldEscalate = true;
                        escalationReason = $"Resolution SLA at {resolutionPercentage:F0}% - warning";
                    }
                }
            }

            // Time-based escalation (priority-specific thresholds)
            if (!shouldEscalate)
            {
                var rule = escalationRules.FirstOrDefault(r => r.Priority == incident.Priority);
                if (rule != null)
                {
                    var age = now - incident.CreatedAt;
                    var currentLevel = incident.EscalationLevel;

                    if (currentLevel < rule.EscalationLevels.Count)
                    {
                        var threshold = rule.EscalationLevels[currentLevel];
                        if (age.TotalMinutes >= threshold.ThresholdMinutes)
                        {
                            shouldEscalate = true;
                            escalationReason = $"Incident age ({age.TotalHours:F1} hours) exceeds escalation threshold";
                        }
                    }
                }
            }

            if (shouldEscalate)
            {
                await EscalateIncidentAsync(context, incident, escalationReason);
                count++;
            }
        }

        if (count > 0)
        {
            await context.SaveChangesAsync();
        }

        return count;
    }

    private async Task<int> ProcessServiceRequestEscalationsAsync(ICrmDbContext context, DateTime now)
    {
        // Similar logic for service requests
        // For now, a simplified version
        var count = 0;

        var overdueRequests = await context.ServiceRequests
            .Where(r => r.Status == "Open" || r.Status == "InProgress")
            .Where(r => r.DueDate.HasValue && r.DueDate < now)
            .ToListAsync();

        foreach (var request in overdueRequests)
        {
            // Add work note about being overdue
            var note = new Core.Entities.ActivityNote
            {
                Content = $"[Auto-Escalation] Service request is overdue. Due date was {request.DueDate:g}.",
                CreatedAt = now,
                CreatedBy = "System",
                NoteType = Core.Entities.NoteType.Activity,
                IsInternal = true
            };

            // Note: Would need to add note to the request
            _logger.LogInformation("Service request {Id} is overdue", request.Id);
            count++;
        }

        return count;
    }

    private async Task EscalateIncidentAsync(ICrmDbContext context, Incident incident, string reason)
    {
        var oldLevel = incident.EscalationLevel;
        incident.EscalationLevel++;
        incident.UpdatedAt = DateTime.UtcNow;

        // Add activity comment
        var comment = new IncidentComment
        {
            IncidentId = incident.IncidentId,
            CommentText = $"[Auto-Escalation] Level {oldLevel} → {incident.EscalationLevel}. Reason: {reason}",
            IsInternal = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = 1 // System user
        };

        context.IncidentComments.Add(comment);

        _logger.LogInformation(
            "Escalated incident {IncidentNumber} from level {OldLevel} to {NewLevel}: {Reason}",
            incident.Number, oldLevel, incident.EscalationLevel, reason);

        // TODO: Send notification to escalation contacts
        // await SendEscalationNotificationAsync(incident, reason);
    }

    private static double CalculateTimePercentage(DateTime start, DateTime end, DateTime current)
    {
        var total = (end - start).TotalMinutes;
        var elapsed = (current - start).TotalMinutes;
        return total > 0 ? (elapsed / total) * 100 : 100;
    }

    private static List<EscalationRule> GetEscalationRules()
    {
        // Default escalation rules by priority
        return new List<EscalationRule>
        {
            new()
            {
                Priority = 1, // P1 - Critical
                EscalationLevels = new List<EscalationLevel>
                {
                    new() { Level = 1, ThresholdMinutes = 30 },
                    new() { Level = 2, ThresholdMinutes = 60 },
                    new() { Level = 3, ThresholdMinutes = 120 }
                }
            },
            new()
            {
                Priority = 2, // P2 - High
                EscalationLevels = new List<EscalationLevel>
                {
                    new() { Level = 1, ThresholdMinutes = 120 },
                    new() { Level = 2, ThresholdMinutes = 240 },
                    new() { Level = 3, ThresholdMinutes = 480 }
                }
            },
            new()
            {
                Priority = 3, // P3 - Medium
                EscalationLevels = new List<EscalationLevel>
                {
                    new() { Level = 1, ThresholdMinutes = 480 },
                    new() { Level = 2, ThresholdMinutes = 1440 },
                    new() { Level = 3, ThresholdMinutes = 2880 }
                }
            },
            new()
            {
                Priority = 4, // P4 - Low
                EscalationLevels = new List<EscalationLevel>
                {
                    new() { Level = 1, ThresholdMinutes = 1440 },
                    new() { Level = 2, ThresholdMinutes = 4320 },
                    new() { Level = 3, ThresholdMinutes = 10080 }
                }
            }
        };
    }

    private class EscalationRule
    {
        public int Priority { get; set; }
        public List<EscalationLevel> EscalationLevels { get; set; } = new();
    }

    private class EscalationLevel
    {
        public int Level { get; set; }
        public int ThresholdMinutes { get; set; }
    }
}


#endif
