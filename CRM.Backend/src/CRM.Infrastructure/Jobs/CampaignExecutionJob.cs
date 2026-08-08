// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs;

/// <summary>
/// REV-STUB-011: Background job that performs the real recipient sends for a campaign.
/// Enqueued as a fire-and-forget job (via <see cref="ICampaignExecutionJobScheduler"/>,
/// backed by Hangfire's <c>BackgroundJob.Enqueue&lt;CampaignExecutionJob&gt;</c> in CRM.Api)
/// when <c>CampaignExecutionService.StartCampaignAsync</c> transitions a campaign to Active,
/// so a large recipient list never blocks the HTTP request that started the campaign.
///
/// Structured like <see cref="ContractExpirationJob"/>: a plain DI-resolvable class with no
/// direct dependency on Hangfire (CRM.Infrastructure does not reference Hangfire.Core —
/// only CRM.Api does), taking <see cref="IServiceProvider"/> + <see cref="ILogger{TCategoryName}"/>
/// so Hangfire's activator (which resolves job classes from the app's DI container) can
/// instantiate it.
/// </summary>
public class CampaignExecutionJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CampaignExecutionJob> _logger;

    public CampaignExecutionJob(
        IServiceProvider serviceProvider,
        ILogger<CampaignExecutionJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the real send for the given campaign by delegating to
    /// <see cref="ICampaignExecutionService.ExecuteAsync"/> inside a fresh DI scope
    /// (Hangfire jobs run outside any HTTP request scope, so a scope must be created
    /// explicitly to resolve scoped services like the DbContext).
    /// </summary>
    /// <param name="campaignId">The campaign to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CampaignExecutionJob starting for campaign {CampaignId}", campaignId);

        using var scope = _serviceProvider.CreateScope();
        var campaignExecutionService = scope.ServiceProvider.GetRequiredService<ICampaignExecutionService>();

        try
        {
            var result = await campaignExecutionService.ExecuteAsync(campaignId, cancellationToken);

            _logger.LogInformation(
                "CampaignExecutionJob completed for campaign {CampaignId}: status={Status}, success={Success}, failure={Failure}, recipients={Recipients}",
                campaignId, result.Status, result.SuccessCount, result.FailureCount, result.RecipientsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CampaignExecutionJob failed for campaign {CampaignId}", campaignId);
            throw;
        }
    }
}
