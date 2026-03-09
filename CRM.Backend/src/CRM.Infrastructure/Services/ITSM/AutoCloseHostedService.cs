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
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
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

    private async Task<int> AutoCloseIncidentsAsync(ICrmDbContext _context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysIncident);
        var count = 0;

        // Find resolved incidents older than the auto-close threshold
        var resolvedIncidents = await _context.Incidents
            .Where(i => i.State == IncidentState.Resolved)
            .Where(i => i.ResolvedAt.HasValue && i.ResolvedAt < cutoffDate)
            .ToListAsync();

        foreach (var incident in resolvedIncidents)
        {
            incident.State = IncidentState.Closed;
            incident.ClosedAt = now;
            incident.ModifiedAt = now;

            // Add auto-close comment
            var comment = new IncidentComment
            {
                IncidentId = incident.IncidentId,
                Comment = $"[Auto-Closed] Incident automatically closed after {DefaultAutoCloseDaysIncident} days in Resolved status without user reopening.",
                IsInternal = true,
                CreatedAt = now,
                CreatedById = 1 // System user
            };

            _context.IncidentComments.Add(comment);

            _logger.LogDebug("Auto-closed incident {IncidentNumber}", incident.Number);
            count++;
        }

        return count;
    }

    private async Task<int> AutoCloseServiceRequestsAsync(ICrmDbContext _context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysServiceRequest);
        var count = 0;

        // Find fulfilled/completed service requests
        var completedRequests = await _context.ServiceRequests
            .Where(r => r.Status == ServiceRequestStatus.Resolved)
            .Where(r => r.UpdatedAt.HasValue && r.UpdatedAt < cutoffDate)
            .ToListAsync();

        foreach (var request in completedRequests)
        {
            request.Status = ServiceRequestStatus.Closed;
            request.UpdatedAt = now;

            _logger.LogDebug("Auto-closed service request {Id}", request.Id);
            count++;
        }

        return count;
    }

    private async Task<int> AutoCloseChangesAsync(ICrmDbContext _context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysChange);
        var count = 0;

        // Find implemented changes that need to be closed
        var implementedChanges = await _context.Changes
            .Where(c => c.State == ChangeState.Implemented)
            .Where(c => c.ActualEndDate.HasValue && c.ActualEndDate < cutoffDate)
            .ToListAsync();

        foreach (var change in implementedChanges)
        {
            change.State = ChangeState.Closed;
            change.ModifiedAt = now;

            // Add work note about auto-closure
            var comment = new ChangeComment
            {
                ChangeId = change.ChangeId,
                Comment = $"[Auto-Closed] Change request automatically closed after {DefaultAutoCloseDaysChange} days in Implemented status.",
                IsInternal = true,
                CreatedAt = now,
                CreatedById = 1 // System user
            };

            _context.ChangeComments.Add(comment);

            _logger.LogDebug("Auto-closed change {ChangeNumber}", change.Number);
            count++;
        }

        return count;
    }

    private async Task<int> AutoCloseProblemsAsync(ICrmDbContext _context, DateTime now)
    {
        var cutoffDate = now.AddDays(-DefaultAutoCloseDaysChange);
        var count = 0;

        // Find resolved problems with no open linked incidents
        var resolvedProblems = await _context.Problems
            .Where(p => p.State == ProblemState.Resolved || p.State == ProblemState.KnownError)
            .Where(p => p.ResolvedAt.HasValue && p.ResolvedAt < cutoffDate)
            .ToListAsync();

        foreach (var problem in resolvedProblems)
        {
            // Check if there are any open incidents still linked
            var hasOpenIncidents = await _context.Incidents
                .AnyAsync(i => i.ProblemId == problem.ProblemId &&
                              i.State != IncidentState.Closed &&
                              i.State != IncidentState.Resolved);

            if (hasOpenIncidents)
            {
                _logger.LogDebug("Problem {ProblemNumber} has open incidents, skipping auto-close",
                    problem.Number);
                continue;
            }

            problem.State = ProblemState.Closed;
            problem.ClosedAt = now;
            problem.ModifiedAt = now;

            // Add closure note
            var comment = new ProblemComment
            {
                ProblemId = problem.ProblemId,
                Comment = $"[Auto-Closed] Problem automatically closed after {DefaultAutoCloseDaysChange} days in Resolved/Known Error status with no open linked incidents.",
                IsInternal = true,
                CreatedAt = now,
                CreatedById = 1 // System user
            };

            _context.ProblemComments.Add(comment);

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


