// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Constants;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Workers;

public class WorkerHost : BackgroundService
{
    private readonly ICrmDbContext _context;
    private readonly IWorkerQueue _workerQueue;
    private readonly ILogger<WorkerHost> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

    public WorkerHost(
        ICrmDbContext context,
        IWorkerQueue workerQueue,
        IHostApplicationLifetime lifetime,
        ILogger<WorkerHost> logger)
    {
        _context = context;
        _workerQueue = workerQueue;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker host started");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (await HandleControlStateAsync(stoppingToken))
            {
                return;
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("Worker host stopped");
    }

    private async Task<bool> HandleControlStateAsync(CancellationToken ct)
    {
        var settings = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        if (settings == null)
        {
            return false;
        }

        var state = string.IsNullOrWhiteSpace(settings.WorkerControlState)
            ? WorkerControlStates.Running
            : settings.WorkerControlState;

        if (string.Equals(state, WorkerControlStates.StopRequested, StringComparison.OrdinalIgnoreCase))
        {
            await UpdateControlStateAsync(WorkerControlStates.Stopped, ct);
            _logger.LogWarning("Worker host stopping due to admin request");
            _lifetime.StopApplication();
            return true;
        }

        if (string.Equals(state, WorkerControlStates.RestartRequested, StringComparison.OrdinalIgnoreCase))
        {
            await UpdateControlStateAsync(WorkerControlStates.Running, ct);
            _logger.LogWarning("Worker host restarting due to admin request");
            _lifetime.StopApplication();
            return true;
        }

        return false;
    }

    private async Task UpdateControlStateAsync(string state, CancellationToken ct)
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync(ct);
        if (settings == null)
        {
            return;
        }

        settings.WorkerControlState = state;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}
