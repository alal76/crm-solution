// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Interfaces.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Workers;

public class WorkerHost : BackgroundService
{
    private readonly IWorkerQueue _workerQueue;
    private readonly ILogger<WorkerHost> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

    public WorkerHost(IWorkerQueue workerQueue, ILogger<WorkerHost> logger)
    {
        _workerQueue = workerQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker host started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("Worker host stopped");
    }
}
