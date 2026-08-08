// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// REV-STUB-011: Abstraction that lets CRM.Infrastructure services enqueue a
/// fire-and-forget background job without CRM.Infrastructure taking a direct
/// dependency on Hangfire.Core (Hangfire packages are referenced from CRM.Api
/// only — see ContractExpirationJob's registration comment in Program.cs).
/// The concrete implementation (backed by Hangfire's BackgroundJob.Enqueue)
/// lives in CRM.Api and is registered there.
/// </summary>
public interface ICampaignExecutionJobScheduler
{
    /// <summary>
    /// Enqueues a fire-and-forget background job that will call
    /// <c>CampaignExecutionJob.ExecuteAsync(campaignId, ...)</c> to perform the
    /// actual recipient sends for the campaign. Returns immediately — the send
    /// work happens out-of-band on a Hangfire worker.
    /// </summary>
    /// <param name="campaignId">The campaign to execute.</param>
    /// <returns>The Hangfire job ID, or null if no scheduler/backing job system is available.</returns>
    string? EnqueueExecution(int campaignId);
}
