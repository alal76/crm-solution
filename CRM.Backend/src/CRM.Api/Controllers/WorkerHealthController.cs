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

using CRM.Core.Entities.Workers;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Worker health and stats endpoints for monitoring background processing.
/// </summary>
[ApiController]
[Route("api/workers")]
public class WorkerHealthController : ControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WorkerHealthController> _logger;

    public WorkerHealthController(ICrmDbContext context, ILogger<WorkerHealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check for worker dependencies.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        var canConnect = await _context.Database.CanConnectAsync(ct);
        var status = canConnect ? "healthy" : "degraded";

        if (!canConnect)
        {
            _logger.LogWarning("Worker health check failed: database unavailable");
        }

        var response = new
        {
            status,
            timestamp = DateTime.UtcNow
        };

        return canConnect
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Worker queue stats for operational monitoring.
    /// </summary>
    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var jobs = _context.WorkerJobs.AsNoTracking();
        var outbox = _context.OutboxEvents.AsNoTracking();
        var now = DateTime.UtcNow;

        var queued = await jobs.CountAsync(j => j.Status == WorkerJobStatus.Queued, ct);
        var inProgress = await jobs.CountAsync(j => j.Status == WorkerJobStatus.InProgress, ct);
        var completed = await jobs.CountAsync(j => j.Status == WorkerJobStatus.Completed, ct);
        var failed = await jobs.CountAsync(j => j.Status == WorkerJobStatus.Failed, ct);
        var deadLettered = await jobs.CountAsync(j => j.Status == WorkerJobStatus.DeadLettered, ct);
        var totalJobs = queued + inProgress + completed + failed + deadLettered;

        var oldestQueuedAt = await jobs
            .Where(j => j.Status == WorkerJobStatus.Queued)
            .OrderBy(j => j.CreatedAt)
            .Select(j => (DateTime?)j.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var lastFailedJobAt = await jobs
            .Where(j => j.Status == WorkerJobStatus.Failed)
            .OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt)
            .Select(j => (DateTime?)(j.UpdatedAt ?? j.CreatedAt))
            .FirstOrDefaultAsync(ct);

        var pendingOutbox = await outbox.CountAsync(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Pending, ct);
        var processingOutbox = await outbox.CountAsync(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Processing, ct);
        var completedOutbox = await outbox.CountAsync(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Completed, ct);
        var failedOutbox = await outbox.CountAsync(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Failed, ct);
        var totalOutbox = pendingOutbox + processingOutbox + completedOutbox + failedOutbox;

        var oldestPendingOutboxAt = await outbox
            .Where(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Pending)
            .OrderBy(o => o.OccurredAt)
            .Select(o => (DateTime?)o.OccurredAt)
            .FirstOrDefaultAsync(ct);

        var lastFailedOutboxAt = await outbox
            .Where(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Failed)
            .OrderByDescending(o => o.ProcessedAt ?? o.OccurredAt)
            .Select(o => (DateTime?)(o.ProcessedAt ?? o.OccurredAt))
            .FirstOrDefaultAsync(ct);

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            jobs = new
            {
                queued,
                inProgress,
                completed,
                failed,
                deadLettered,
                total = totalJobs
            },
            outbox = new
            {
                pending = pendingOutbox,
                processing = processingOutbox,
                completed = completedOutbox,
                failed = failedOutbox,
                total = totalOutbox
            },
            metrics = new
            {
                oldestQueuedAt,
                oldestQueuedAgeSeconds = AgeSeconds(oldestQueuedAt, now),
                lastFailedJobAt,
                oldestPendingOutboxAt,
                oldestPendingOutboxAgeSeconds = AgeSeconds(oldestPendingOutboxAt, now),
                lastFailedOutboxAt
            }
        });
    }

    private static double? AgeSeconds(DateTime? timestamp, DateTime now)
    {
        return timestamp.HasValue ? (now - timestamp.Value).TotalSeconds : null;
    }
}
