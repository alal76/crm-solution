// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.BackgroundServices;

/// <summary>
/// Polls every 5 minutes to advance due nurture sequence steps.
/// Uses a scoped service scope so EF Core DbContext lifetime is respected.
/// </summary>
public class NurtureSequenceBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NurtureSequenceBackgroundService> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

    /// <summary>Initializes a new instance of NurtureSequenceBackgroundService.</summary>
    public NurtureSequenceBackgroundService(IServiceScopeFactory scopeFactory, ILogger<NurtureSequenceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NurtureSequenceBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sequenceService = scope.ServiceProvider.GetRequiredService<IEmailSequenceService>();
                await sequenceService.ProcessDueStepsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031 // Intentional broad catch to prevent background service crash
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing nurture sequence steps");
            }
#pragma warning restore CA1031

            await Task.Delay(PollInterval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("NurtureSequenceBackgroundService stopped");
    }
}
