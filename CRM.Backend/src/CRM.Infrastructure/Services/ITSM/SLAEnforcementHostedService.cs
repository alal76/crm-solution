// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Background service that runs continuously to monitor and enforce SLA agreements.
/// Runs every 1 minute to check for SLA breaches and send notifications.
/// </summary>
public class SLAEnforcementHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SLAEnforcementHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public SLAEnforcementHostedService(IServiceProvider serviceProvider, ILogger<SLAEnforcementHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA Enforcement Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var slaService = scope.ServiceProvider.GetRequiredService<ISLAService>();
                    
                    // Check SLA breaches every minute
                    await slaService.CheckSLABreachesAsync();
                    
                    _logger.LogDebug("SLA breach check completed at {Timestamp}", DateTime.UtcNow);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SLA Enforcement Background Service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SLA enforcement service");
                // Continue running despite errors
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("SLA Enforcement Background Service stopped");
    }
}
