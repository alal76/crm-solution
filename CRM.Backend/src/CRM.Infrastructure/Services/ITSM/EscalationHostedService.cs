// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
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
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();

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
            .Where(i => i.State != IncidentState.Closed &&
                       i.State != IncidentState.Resolved &&
                       i.State != IncidentState.Cancelled)
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
                if (slaInstance.ResponseDueAt.HasValue && !slaInstance.ResponseActualAt.HasValue)
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
            .Where(r => r.Status == ServiceRequestStatus.Open || r.Status == ServiceRequestStatus.InProgress)
            .Where(r => r.ResolutionDueDate.HasValue && r.ResolutionDueDate < now)
            .ToListAsync();

        foreach (var request in overdueRequests)
        {
            // Log overdue service request for escalation tracking
            _logger.LogInformation(
                "Service request {Id} is overdue. Due date was {DueDate:g}.",
                request.Id, request.ResolutionDueDate);
            count++;
        }

        return count;
    }

    private async Task EscalateIncidentAsync(ICrmDbContext context, Incident incident, string reason)
    {
        var oldLevel = incident.EscalationLevel;
        incident.EscalationLevel++;
        incident.ModifiedAt = DateTime.UtcNow;

        // Add activity comment
        var comment = new IncidentComment
        {
            IncidentId = incident.IncidentId,
            Comment = $"[Auto-Escalation] Level {oldLevel} → {incident.EscalationLevel}. Reason: {reason}",
            IsInternal = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = 1 // System user
        };

        context.IncidentComments.Add(comment);

        _logger.LogInformation(
            "Escalated incident {IncidentNumber} from level {OldLevel} to {NewLevel}: {Reason}",
            incident.Number, oldLevel, incident.EscalationLevel, reason);

        // Send notification to escalation contacts
        await SendEscalationNotificationAsync(context, incident, oldLevel, reason);
    }

    /// <summary>
    /// Sends escalation notification to the appropriate contacts based on escalation level.
    /// Uses INotificationPort for multi-channel notifications (email, SMS, etc.).
    /// </summary>
    private async Task SendEscalationNotificationAsync(ICrmDbContext context, Incident incident, int previousLevel, string reason)
    {
        try
        {
            // Get escalation contacts based on level and assignment group
            var escalationContacts = await GetEscalationContactsAsync(context, incident);

            if (!escalationContacts.Any())
            {
                _logger.LogWarning(
                    "No escalation contacts found for incident {IncidentNumber} at level {Level}",
                    incident.Number, incident.EscalationLevel);
                return;
            }

            // Resolve notification port from DI
            using var scope = _serviceProvider.CreateScope();
            var notificationPort = scope.ServiceProvider.GetService<CRM.Core.Ports.Output.Providers.INotificationPort>();

            if (notificationPort == null)
            {
                _logger.LogWarning("INotificationPort not available, escalation notifications will not be sent");
                return;
            }

            // Send email notification to each escalation contact
            foreach (var contact in escalationContacts)
            {
                var emailRequest = new CRM.Core.Ports.Output.Providers.EmailNotificationRequest
                {
                    To = contact.Email,
                    ToName = contact.Name,
                    Subject = $"[ESCALATION L{incident.EscalationLevel}] Incident {incident.Number}: {incident.ShortDescription}",
                    Body = BuildEscalationEmailBody(incident, previousLevel, reason),
                    IsHtml = true,
                };

                var result = await notificationPort.SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Escalation notification sent to {Email} for incident {IncidentNumber}",
                        contact.Email, incident.Number);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send escalation notification to {Email} for incident {IncidentNumber}: {Error}",
                        contact.Email, incident.Number, result.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending escalation notification for incident {IncidentNumber}", incident.Number);
        }
    }

    /// <summary>
    /// Gets escalation contacts based on incident escalation level and assignment group.
    /// </summary>
    private async Task<List<EscalationContact>> GetEscalationContactsAsync(ICrmDbContext context, Incident incident)
    {
        var contacts = new List<EscalationContact>();

        // Level 1: Assigned technician + team lead
        // Level 2: Team manager + service owner
        // Level 3: Department head + IT director
        // Level 4+: Executive / CIO

        // Get assignee if exists
        if (incident.AssignedToId.HasValue)
        {
            var assignee = await context.Users
                .Where(u => u.Id == incident.AssignedToId && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (assignee != null && incident.EscalationLevel == 1)
            {
                contacts.Add(new EscalationContact
                {
                    Email = assignee.Email,
                    Name = $"{assignee.FirstName} {assignee.LastName}",
                    Role = "Assigned Technician",
                });
            }
        }

        // Get assignment group members for manager escalation
        if (incident.AssignmentGroupId.HasValue && incident.EscalationLevel >= 2)
        {
            var groupManagers = await context.UserGroupMembers
                .Where(ugm => ugm.UserGroupId == incident.AssignmentGroupId)
                .Join(
                    context.Users.Where(u => !u.IsDeleted),
                    ugm => ugm.UserId,
                    u => u.Id,
                    (ugm, u) => u)
                .ToListAsync();

            // Find users with manager role (Role == 3 or check for admin in group)
            foreach (var user in groupManagers.Where(u => u.Role >= 2).Take(2))
            {
                contacts.Add(new EscalationContact
                {
                    Email = user.Email,
                    Name = $"{user.FirstName} {user.LastName}",
                    Role = incident.EscalationLevel >= 3 ? "Manager" : "Team Lead",
                });
            }
        }

        // For highest escalation levels, try to find system admins
        if (incident.EscalationLevel >= 3)
        {
            var admins = await context.Users
                .Where(u => u.Role >= 3 && !u.IsDeleted)
                .Take(2)
                .ToListAsync();

            foreach (var admin in admins)
            {
                if (!contacts.Any(c => c.Email == admin.Email))
                {
                    contacts.Add(new EscalationContact
                    {
                        Email = admin.Email,
                        Name = $"{admin.FirstName} {admin.LastName}",
                        Role = "IT Director",
                    });
                }
            }
        }

        return contacts;
    }

    /// <summary>
    /// Builds the HTML email body for escalation notification.
    /// </summary>
    private static string BuildEscalationEmailBody(Incident incident, int previousLevel, string reason)
    {
        var priority = GetPriorityLabel((int)incident.Impact, (int)incident.Urgency);
        var urgencyColor = (int)incident.Impact switch
        {
            <= 2 => "#dc3545",
            3 => "#ffc107",
            _ => "#28a745"
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .header {{ background-color: {urgencyColor}; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .details {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .label {{ font-weight: bold; color: #555; }}
        .action {{ background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>⚠️ Incident Escalated</h1>
        <p>Level {previousLevel} → Level {incident.EscalationLevel}</p>
    </div>
    <div class='content'>
        <h2>Incident: {incident.Number}</h2>
        <p><strong>{incident.ShortDescription}</strong></p>

        <div class='details'>
            <p><span class='label'>Priority:</span> {priority}</p>
            <p><span class='label'>Impact:</span> P{(int)incident.Impact}</p>
            <p><span class='label'>Urgency:</span> P{(int)incident.Urgency}</p>
            <p><span class='label'>State:</span> {incident.State}</p>
            <p><span class='label'>Opened:</span> {incident.OpenedAt:yyyy-MM-dd HH:mm} UTC</p>
        </div>

        <h3>Escalation Reason</h3>
        <p>{reason}</p>

        <h3>Description</h3>
        <p>{incident.Description ?? "No description provided."}</p>

        <p>Please review and take appropriate action.</p>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gets priority label based on impact and urgency matrix.
    /// </summary>
    private static string GetPriorityLabel(int impact, int urgency)
    {
        var priorityValue = (impact + urgency) / 2;
        return priorityValue switch
        {
            1 => "P1 - Critical",
            2 => "P2 - High",
            3 => "P3 - Medium",
            _ => "P4 - Low",
        };
    }

    /// <summary>
    /// Represents an escalation contact for notifications.
    /// </summary>
    private sealed class EscalationContact
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
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
