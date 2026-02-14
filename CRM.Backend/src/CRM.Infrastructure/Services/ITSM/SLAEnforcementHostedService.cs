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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SLA Enforcement Background Service stopping");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("SLA Enforcement Background Service stopped");
    }
}
