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
/// Background service for automatic closure of resolved tickets after a configurable waiting period.
/// This gives users time to reopen if the resolution wasn't satisfactory.
/// </summary>
public class AutoCloseHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoCloseHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    // Configurable: days to wait before auto-closing resolved items
    private const int DefaultAutoCloseDaysIncident = 3;
    private const int DefaultAutoCloseDaysServiceRequest = 5;
    private const int DefaultAutoCloseDaysChange = 7;

    public AutoCloseHostedService(
        IServiceProvider serviceProvider,
        ILogger<AutoCloseHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto-Close Service starting...");

        // Initial delay to allow application startup
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAutoClosuresAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto-close service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Auto-Close Service stopped");
    }

    private async Task ProcessAutoClosuresAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContextResolver = scope.ServiceProvider.GetRequiredService<IDbContextResolver>();
        var context = dbContextResolver.ResolveContext();

        var now = DateTime.UtcNow;
        var closedCount = 0;

        // Auto-close resolved incidents
        closedCount += await AutoCloseIncidentsAsync(context, now);

        // Auto-close resolved service requests
        closedCount += await AutoCloseServiceRequestsAsync(context, now);

        // Auto-close completed changes
        closedCount += await AutoCloseChangesAsync(context, now);

        // Auto-close resolved problems
        closedCount += await AutoCloseProblemsAsync(context, now);

        if (closedCount > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("Auto-closed {Count} resolved items", closedCount);
        }
    }

    private async Task<int> AutoCloseIncidentsAsync(ICrmDbContext context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysIncident);
        var count = 0;

        // Find resolved incidents older than the auto-close threshold
        var resolvedIncidents = await context.Incidents
            .Where(i => i.Status == IncidentState.Resolved)
            .Where(i => i.ResolvedAt.HasValue && i.ResolvedAt < cutoffDate)
            .ToListAsync();

        foreach (var incident in resolvedIncidents)
        {
            incident.Status = IncidentState.Closed;
            incident.ClosedAt = now;
            incident.UpdatedAt = now;

            // Add auto-close comment
            var comment = new IncidentComment
            {
                IncidentId = incident.IncidentId,
                CommentText = $"[Auto-Closed] Incident automatically closed after {DefaultAutoCloseDaysIncident} days in Resolved status without user reopening.",
                IsInternal = true,
                CreatedAt = now,
                CreatedById = 1 // System user
            };

            context.IncidentComments.Add(comment);

            _logger.LogDebug("Auto-closed incident {IncidentNumber}", incident.Number);
            count++;
        }

        return count;
    }

    private async Task<int> AutoCloseServiceRequestsAsync(ICrmDbContext context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysServiceRequest);
        var count = 0;

        // Find fulfilled/completed service requests
        var completedRequests = await context.ServiceRequests
            .Where(r => r.Status == "Completed" || r.Status == "Fulfilled")
            .Where(r => r.UpdatedAt.HasValue && r.UpdatedAt < cutoffDate)
            .ToListAsync();

        foreach (var request in completedRequests)
        {
            request.Status = "Closed";
            request.UpdatedAt = now;

            _logger.LogDebug("Auto-closed service request {Id}", request.Id);
            count++;
        }

        return count;
    }

    private async Task<int> AutoCloseChangesAsync(ICrmDbContext context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysChange);
        var count = 0;

        // Find implemented changes that need to be closed
        var implementedChanges = await context.Changes
            .Where(c => c.Status == ChangeState.Implemented)
            .Where(c => c.ActualEnd.HasValue && c.ActualEnd < cutoffDate)
            .ToListAsync();

        foreach (var change in implementedChanges)
        {
            change.Status = ChangeState.Closed;
            change.ClosedAt = now;
            change.UpdatedAt = now;

            // Add work note about auto-closure
            var note = new ChangeNote
            {
                ChangeRequestId = change.ChangeRequestId,
                NoteText = $"[Auto-Closed] Change request automatically closed after {DefaultAutoCloseDaysChange} days in Implemented status.",
                IsInternal = true,
                CreatedAt = now,
                CreatedById = 1 // System user
            };

            context.ITSMChangeNotes.Add(note);

            _logger.LogDebug("Auto-closed change {ChangeNumber}", change.Number);
            count++;
        }

        return count;
    }

    private async Task<int> AutoCloseProblemsAsync(ICrmDbContext context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysChange);
        var count = 0;

        // Find resolved problems with no open linked incidents
        var resolvedProblems = await context.ITSMProblems
            .Where(p => p.Status == ProblemStatus.Resolved || p.Status == ProblemStatus.KnownError)
            .Where(p => p.ResolvedAt.HasValue && p.ResolvedAt < cutoffDate)
            .ToListAsync();

        foreach (var problem in resolvedProblems)
        {
            // Check if there are any open incidents still linked
            var hasOpenIncidents = await context.Incidents
                .AnyAsync(i => i.ProblemId == problem.ProblemId &&
                              i.Status != IncidentState.Closed &&
                              i.Status != IncidentState.Resolved);

            if (hasOpenIncidents)
            {
                _logger.LogDebug("Problem {ProblemNumber} has open incidents, skipping auto-close",
                    problem.Number);
                continue;
            }

            problem.Status = ProblemStatus.Closed;
            problem.ClosedAt = now;
            problem.UpdatedAt = now;

            // Add closure note
            var note = new ProblemNote
            {
                ProblemId = problem.ProblemId,
                NoteText = $"[Auto-Closed] Problem automatically closed after {DefaultAutoCloseDaysChange} days in Resolved/Known Error status with no open linked incidents.",
                IsInternal = true,
                CreatedAt = now,
                CreatedById = 1 // System user
            };

            context.ITSMProblemNotes.Add(note);

            _logger.LogDebug("Auto-closed problem {ProblemNumber}", problem.Number);
            count++;
        }

        return count;
    }
}

/// <summary>
/// Configuration options for auto-close behavior.
/// Can be loaded from appsettings.json.
/// </summary>
public class AutoCloseOptions
{
    public const string SectionName = "ITSM:AutoClose";

    /// <summary>
    /// Whether auto-close is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Days to wait before auto-closing resolved incidents.
    /// </summary>
    public int IncidentDays { get; set; } = 3;

    /// <summary>
    /// Days to wait before auto-closing completed service requests.
    /// </summary>
    public int ServiceRequestDays { get; set; } = 5;

    /// <summary>
    /// Days to wait before auto-closing implemented changes.
    /// </summary>
    public int ChangeDays { get; set; } = 7;

    /// <summary>
    /// Days to wait before auto-closing resolved problems.
    /// </summary>
    public int ProblemDays { get; set; } = 7;

    /// <summary>
    /// How often to check for items to auto-close (in minutes).
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 60;
}


#endif
