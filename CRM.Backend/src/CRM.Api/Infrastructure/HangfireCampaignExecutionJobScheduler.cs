// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Jobs;
using Hangfire;

namespace CRM.Api.Infrastructure;

/// <summary>
/// REV-STUB-011: Hangfire-backed implementation of <see cref="ICampaignExecutionJobScheduler"/>.
/// Lives in CRM.Api (not CRM.Infrastructure) because Hangfire.Core is only referenced from
/// this project — see the "Hangfire.Core is not referenced from CRM.Infrastructure" comment
/// on <c>ContractExpirationJobExtensions</c> for the established boundary this mirrors.
/// </summary>
public class HangfireCampaignExecutionJobScheduler : ICampaignExecutionJobScheduler
{
    /// <inheritdoc />
    public string? EnqueueExecution(int campaignId)
    {
        return BackgroundJob.Enqueue<CampaignExecutionJob>(job => job.ExecuteAsync(campaignId, CancellationToken.None));
    }
}
