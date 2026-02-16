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

        var queued = await jobs.CountAsync(j => j.Status == WorkerJobStatus.Queued, ct);
        var inProgress = await jobs.CountAsync(j => j.Status == WorkerJobStatus.InProgress, ct);
        var failed = await jobs.CountAsync(j => j.Status == WorkerJobStatus.Failed, ct);
        var pendingOutbox = await outbox.CountAsync(o => o.Status == CRM.Core.Entities.Integration.OutboxEventStatus.Pending, ct);

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            jobs = new
            {
                queued,
                inProgress,
                failed
            },
            outbox = new
            {
                pending = pendingOutbox
            }
        });
    }
}
