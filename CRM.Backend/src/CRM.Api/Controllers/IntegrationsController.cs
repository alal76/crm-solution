// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Ports.Input;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Integration management endpoints — accounting, marketing, linkedin, scheduling.
/// Implements TODO-INT-08, TODO-INT-09, TODO-INT-10, TODO-INT-11.
/// </summary>
[ApiController]
[Route("api/integrations")]
[Authorize]
public class IntegrationsController : CrmControllerBase
{
    private readonly IAccountingSyncService _accountingSync;
    private readonly IMarketingSyncService _marketingSync;
    private readonly ILinkedInSalesNavService _linkedIn;
    private readonly ISchedulingIntegrationService _scheduling;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        IAccountingSyncService accountingSync,
        IMarketingSyncService marketingSync,
        ILinkedInSalesNavService linkedIn,
        ISchedulingIntegrationService scheduling,
        ILogger<IntegrationsController> logger)
    {
        _accountingSync = accountingSync;
        _marketingSync = marketingSync;
        _linkedIn = linkedIn;
        _scheduling = scheduling;
        _logger = logger;
    }

    // ------------------------------------------------------------------ //
    //  TODO-INT-08: Accounting Sync (QuickBooks / Xero)
    // ------------------------------------------------------------------ //

    /// <summary>Tests the accounting integration connection.</summary>
    [HttpGet("accounting/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestAccounting(CancellationToken ct)
    {
        var ok = await _accountingSync.TestConnectionAsync(ct);
        return Ok(new { connected = ok });
    }

    /// <summary>Syncs a CRM account to the accounting platform.</summary>
    [HttpPost("accounting/sync/account/{accountId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncAccount(int accountId, CancellationToken ct)
    {
                var result = await _accountingSync.SyncAccountAsync(accountId, ct);
        return Ok(result);
    }

    /// <summary>Syncs an invoice to the accounting platform.</summary>
    [HttpPost("accounting/sync/invoice/{invoiceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncInvoice(int invoiceId, CancellationToken ct)
    {
                var result = await _accountingSync.SyncInvoiceAsync(invoiceId, ct);
        return Ok(result);
    }

    /// <summary>Runs a full batch sync of all accounts and invoices.</summary>
    [HttpPost("accounting/sync/batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchSync(CancellationToken ct)
    {
                var result = await _accountingSync.RunBatchSyncAsync(ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-INT-09: Marketing Sync (Mailchimp / HubSpot)
    // ------------------------------------------------------------------ //

    /// <summary>Returns all marketing lists from the provider.</summary>
    [HttpGet("marketing/lists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarketingLists(CancellationToken ct)
    {
        var lists = await _marketingSync.GetListsAsync(ct);
        return Ok(lists);
    }

    /// <summary>Syncs a CRM contact to the specified marketing list.</summary>
    [HttpPost("marketing/sync/contact/{contactId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncContact(int contactId, [FromQuery] string listId, CancellationToken ct)
    {
                var result = await _marketingSync.SyncContactAsync(contactId, listId, ct);
        return Ok(result);
    }

    /// <summary>Imports marketing list subscribers as CRM leads.</summary>
    [HttpPost("marketing/import/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportSubscribers(string listId, CancellationToken ct)
    {
                var result = await _marketingSync.ImportSubscribersAsLeadsAsync(listId, ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-INT-10: LinkedIn Sales Navigator
    // ------------------------------------------------------------------ //

    /// <summary>Gets a LinkedIn profile by profile URL.</summary>
    [HttpGet("linkedin/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLinkedInProfile([FromQuery] string url, CancellationToken ct)
    {
                var profile = await _linkedIn.GetProfileAsync(url, ct);
        return profile is null ? NotFound(new { message = "Profile not found" }) : Ok(profile);
    }

    /// <summary>Enriches a CRM contact with LinkedIn data.</summary>
    [HttpPost("linkedin/enrich/contact/{contactId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EnrichContact(int contactId, [FromQuery] string profileUrl, CancellationToken ct)
    {
                var result = await _linkedIn.EnrichContactAsync(contactId, profileUrl, ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-INT-11: Scheduling Integration (Calendly / Cal.com)
    // ------------------------------------------------------------------ //

    /// <summary>Returns scheduling links for all users or a specific user.</summary>
    [HttpGet("scheduling/links")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedulingLinks([FromQuery] int? userId, CancellationToken ct)
    {
        var links = await _scheduling.GetSchedulingLinksAsync(userId, ct);
        return Ok(links);
    }

    /// <summary>Creates a new scheduling link for the specified user.</summary>
    [HttpPost("scheduling/links")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSchedulingLink(
        [FromBody] CreateSchedulingLinkRequest request, CancellationToken ct)
    {
                var link = await _scheduling.CreateSchedulingLinkAsync(request, ct);
        return Ok(link);
    }

    /// <summary>Returns upcoming meetings for a user within the specified date range.</summary>
    [HttpGet("scheduling/meetings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingMeetings(
        [FromQuery] int? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var meetings = await _scheduling.GetUpcomingMeetingsAsync(
            userId,
            from ?? DateTime.UtcNow,
            to ?? DateTime.UtcNow.AddDays(30),
            ct);
        return Ok(meetings);
    }

    /// <summary>Tests the scheduling provider connection.</summary>
    [HttpGet("scheduling/test")]
    public async Task<IActionResult> TestScheduling(CancellationToken ct)
    {
        var ok = await _scheduling.TestConnectionAsync(ct);
        return Ok(new { connected = ok });
    }
}
