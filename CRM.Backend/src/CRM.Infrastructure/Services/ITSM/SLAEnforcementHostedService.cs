// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

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
        _logger.LogInformation("SLA Enforcement Background Service started at {Timestamp}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
                    var escalationRuleService = scope.ServiceProvider.GetRequiredService<IEscalationRuleService>();

                    // Check for breached SLAs and trigger escalations
                    await CheckSLABreachesAsync(dbContext, escalationRuleService, stoppingToken);

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
                _logger.LogError(ex, "Error in SLA enforcement service at {Timestamp}", DateTime.UtcNow);
                // Continue running despite errors
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("SLA Enforcement Background Service stopped at {Timestamp}", DateTime.UtcNow);
    }

    /// <summary>
    /// Check for breached SLAs and trigger escalations.
    /// </summary>
    private async Task CheckSLABreachesAsync(
        ICrmDbContext dbContext,
        IEscalationRuleService escalationRuleService,
        CancellationToken cancellationToken)
    {
        try
        {
            // Find all active SLA instances that have not been marked as breached
            var breachedSlAs = await dbContext.SLAInstances
                .Where(si => !si.IsDeleted && 
                        si.Status != CRM.Core.Entities.KnowledgeBase.SLAStatus.Breached &&
                        si.Status != CRM.Core.Entities.KnowledgeBase.SLAStatus.Met &&
                        (si.DueAt < DateTime.UtcNow || si.WarningAt < DateTime.UtcNow))
                .Include(si => si.ServiceRequest)
                .Include(si => si.SLAPolicy)
                .ThenInclude(sp => sp.EscalationRules)
                .ToListAsync(cancellationToken);

            foreach (var slaInstance in breachedSlAs)
            {
                try
                {
                    // Check if SLA due date has passed
                    if (slaInstance.DueAt < DateTime.UtcNow)
                    {
                        // Update SLA status to Breached
                        slaInstance.Status = CRM.Core.Entities.KnowledgeBase.SLAStatus.Breached;
                        slaInstance.WasBreached = true;
                        slaInstance.BreachedAt = DateTime.UtcNow;
                        slaInstance.MinutesOverSla = (int)(DateTime.UtcNow - slaInstance.DueAt).TotalMinutes;

                        dbContext.SLAInstances.Update(slaInstance);
                        await dbContext.SaveChangesAsync(cancellationToken);

                        _logger.LogWarning(
                            "SLA breached for service request {ServiceRequestId} at {BreachedAt}",
                            slaInstance.ServiceRequestId, slaInstance.BreachedAt);

                        // Trigger escalation rules
                        if (slaInstance.ServiceRequest != null)
                        {
                            await escalationRuleService.EvaluateRulesAsync(
                                slaInstance.ServiceRequestId);

                            _logger.LogInformation(
                                "Escalation rules evaluated for service request {ServiceRequestId}",
                                slaInstance.ServiceRequestId);
                        }
                    }
                    // Check if warning threshold has been reached
                    else if (slaInstance.WarningAt < DateTime.UtcNow && slaInstance.Status == CRM.Core.Entities.KnowledgeBase.SLAStatus.OnTrack)
                    {
                        // Update SLA status to AtRisk
                        slaInstance.Status = CRM.Core.Entities.KnowledgeBase.SLAStatus.AtRisk;
                        dbContext.SLAInstances.Update(slaInstance);
                        await dbContext.SaveChangesAsync(cancellationToken);

                        _logger.LogWarning(
                            "SLA at risk for service request {ServiceRequestId}",
                            slaInstance.ServiceRequestId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing SLA instance {SLAInstanceId} for service request {ServiceRequestId}",
                        slaInstance.Id, slaInstance.ServiceRequestId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SLA breaches");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SLA Enforcement Background Service stopping");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("SLA Enforcement Background Service stopped");
    }
}
