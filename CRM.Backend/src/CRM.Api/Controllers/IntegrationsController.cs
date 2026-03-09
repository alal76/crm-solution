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
using IQuickBooksService = CRM.Core.Interfaces.IQuickBooksService;
using QuickBooksConnectionStatus = CRM.Core.Interfaces.QuickBooksConnectionStatus;
using IXeroService = CRM.Core.Interfaces.IXeroService;
using XeroConnectionStatus = CRM.Core.Interfaces.XeroConnectionStatus;
using IMailchimpService = CRM.Core.Interfaces.IMailchimpService;
using IHubSpotService = CRM.Core.Interfaces.IHubSpotService;
using ICalendlyService = CRM.Core.Interfaces.ICalendlyService;

namespace CRM.Api.Controllers;

/// <summary>
/// Integration management endpoints — accounting (QB/Xero OAuth2), marketing, linkedin, scheduling.
/// Implements INT-001 (QuickBooks/Xero), TODO-INT-08, TODO-INT-09, TODO-INT-10, TODO-INT-11.
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
    private readonly IQuickBooksService _quickBooks;
    private readonly IXeroService _xero;
    private readonly IMailchimpService _mailchimp;
    private readonly IHubSpotService _hubspot;
    private readonly ICalendlyService _calendly;

    public IntegrationsController(
        IAccountingSyncService accountingSync,
        IMarketingSyncService marketingSync,
        ILinkedInSalesNavService linkedIn,
        ISchedulingIntegrationService scheduling,
        IQuickBooksService quickBooks,
        IXeroService xero,
        IMailchimpService mailchimp,
        IHubSpotService hubspot,
        ICalendlyService calendly)
    {
        _accountingSync = accountingSync;
        _marketingSync = marketingSync;
        _linkedIn = linkedIn;
        _scheduling = scheduling;
        _quickBooks = quickBooks;
        _xero = xero;
        _mailchimp = mailchimp;
        _hubspot = hubspot;
        _calendly = calendly;
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

    // ------------------------------------------------------------------ //
    //  INT-001: QuickBooks Online OAuth2
    // ------------------------------------------------------------------ //

    /// <summary>Redirects the user to the Intuit QuickBooks OAuth2 consent screen.</summary>
    [HttpGet("quickbooks/connect")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult QuickBooksConnect()
    {
        var state = Guid.NewGuid().ToString("N");
        var url = _quickBooks.GetAuthorizationUrl(state);
        return Redirect(url);
    }

    /// <summary>
    /// Handles the Intuit OAuth2 callback, exchanges the code for tokens,
    /// then redirects to the frontend settings page.
    /// </summary>
    [HttpGet("quickbooks/callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> QuickBooksCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string realmId,
        CancellationToken ct)
    {
        var success = await _quickBooks.ExchangeCodeForTokensAsync(code, realmId, ct);
        var redirectUrl = success
            ? "/settings/integrations?qb=connected"
            : "/settings/integrations?qb=error";
        return Redirect(redirectUrl);
    }

    /// <summary>Returns the current QuickBooks connection status.</summary>
    [HttpGet("quickbooks/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuickBooksStatus(CancellationToken ct)
    {
        var status = await _quickBooks.GetConnectionStatusAsync(ct);
        return Ok(status);
    }

    /// <summary>Syncs a single CRM account to QuickBooks as a Customer.</summary>
    [HttpPost("quickbooks/sync/account/{accountId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuickBooksSyncAccount(int accountId, CancellationToken ct)
    {
        var success = await _quickBooks.SyncAccountAsync(accountId, ct);
        return Ok(new { synced = success, accountId });
    }

    /// <summary>Syncs all CRM accounts to QuickBooks as Customers.</summary>
    [HttpPost("quickbooks/sync/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuickBooksSyncAllAccounts(CancellationToken ct)
    {
        // Delegates to the generic accounting batch sync.
        // TODO (INT-001): iterate all active accounts and call SyncAccountAsync per record.
        var result = await _accountingSync.RunBatchSyncAsync(ct);
        return Ok(result);
    }

    /// <summary>Syncs a single CRM invoice to QuickBooks.</summary>
    [HttpPost("quickbooks/sync/invoice/{invoiceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuickBooksSyncInvoice(int invoiceId, CancellationToken ct)
    {
        var success = await _quickBooks.SyncInvoiceAsync(invoiceId, ct);
        return Ok(new { synced = success, invoiceId });
    }

    /// <summary>Syncs all unpaid invoices to QuickBooks.</summary>
    [HttpPost("quickbooks/sync/invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuickBooksSyncAllInvoices(CancellationToken ct)
    {
        var result = await _accountingSync.RunBatchSyncAsync(ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  INT-001: Xero OAuth2
    // ------------------------------------------------------------------ //

    /// <summary>Redirects the user to the Xero OAuth2 consent screen.</summary>
    [HttpGet("xero/connect")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult XeroConnect()
    {
        var state = Guid.NewGuid().ToString("N");
        var url = _xero.GetAuthorizationUrl(state);
        return Redirect(url);
    }

    /// <summary>
    /// Handles the Xero OAuth2 callback, exchanges the code for tokens,
    /// then redirects to the frontend settings page.
    /// </summary>
    [HttpGet("xero/callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> XeroCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string? tenantId,
        CancellationToken ct)
    {
        var resolvedTenantId = tenantId ?? string.Empty;
        var success = await _xero.ExchangeCodeForTokensAsync(code, resolvedTenantId, ct);
        var redirectUrl = success
            ? "/settings/integrations?xero=connected"
            : "/settings/integrations?xero=error";
        return Redirect(redirectUrl);
    }

    /// <summary>Returns the current Xero connection status.</summary>
    [HttpGet("xero/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> XeroStatus(CancellationToken ct)
    {
        var status = await _xero.GetConnectionStatusAsync(ct);
        return Ok(status);
    }

    /// <summary>Syncs a single CRM account to Xero as a Contact.</summary>
    [HttpPost("xero/sync/contact/{accountId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> XeroSyncContact(int accountId, CancellationToken ct)
    {
        var success = await _xero.SyncContactAsync(accountId, ct);
        return Ok(new { synced = success, accountId });
    }

    /// <summary>Syncs all CRM accounts to Xero as Contacts.</summary>
    [HttpPost("xero/sync/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> XeroSyncAllContacts(CancellationToken ct)
    {
        var result = await _accountingSync.RunBatchSyncAsync(ct);
        return Ok(result);
    }

    /// <summary>Syncs a single CRM invoice to Xero.</summary>
    [HttpPost("xero/sync/invoice/{invoiceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> XeroSyncInvoice(int invoiceId, CancellationToken ct)
    {
        var success = await _xero.SyncInvoiceAsync(invoiceId, ct);
        return Ok(new { synced = success, invoiceId });
    }

    /// <summary>Syncs all CRM invoices to Xero.</summary>
    [HttpPost("xero/sync/invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> XeroSyncAllInvoices(CancellationToken ct)
    {
        var result = await _accountingSync.RunBatchSyncAsync(ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  INT-002: Mailchimp contact-list sync
    // ------------------------------------------------------------------ //

    /// <summary>Returns the Mailchimp connection status.</summary>
    [HttpGet("mailchimp/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MailchimpStatus(CancellationToken ct)
    {
        var status = await _mailchimp.GetConnectionStatusAsync(ct);
        return Ok(status);
    }

    /// <summary>Syncs all active CRM contacts to the configured Mailchimp audience.</summary>
    [HttpPost("mailchimp/sync/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MailchimpSyncAllContacts(CancellationToken ct)
    {
        var count = await _mailchimp.SyncAllContactsAsync(ct);
        return Ok(new { synced = count });
    }

    /// <summary>Syncs a single CRM contact to Mailchimp.</summary>
    [HttpPost("mailchimp/sync/contact/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MailchimpSyncContact(int id, CancellationToken ct)
    {
        var success = await _mailchimp.SyncContactAsync(id, ct);
        return Ok(new { synced = success, contactId = id });
    }

    // ------------------------------------------------------------------ //
    //  INT-002: HubSpot bidirectional contact/deal sync
    // ------------------------------------------------------------------ //

    /// <summary>Returns the HubSpot connection status.</summary>
    [HttpGet("hubspot/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HubSpotStatus(CancellationToken ct)
    {
        var status = await _hubspot.GetConnectionStatusAsync(ct);
        return Ok(status);
    }

    /// <summary>Syncs all active CRM contacts to HubSpot.</summary>
    [HttpPost("hubspot/sync/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HubSpotSyncAllContacts(CancellationToken ct)
    {
        var count = await _hubspot.SyncAllContactsAsync(ct);
        return Ok(new { synced = count });
    }

    /// <summary>Syncs a single CRM contact to HubSpot.</summary>
    [HttpPost("hubspot/sync/contact/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HubSpotSyncContact(int id, CancellationToken ct)
    {
        var success = await _hubspot.SyncContactAsync(id, ct);
        return Ok(new { synced = success, contactId = id });
    }

    /// <summary>Syncs a CRM opportunity to HubSpot as a Deal.</summary>
    [HttpPost("hubspot/sync/deal/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HubSpotSyncDeal(int id, CancellationToken ct)
    {
        var success = await _hubspot.SyncDealAsync(id, ct);
        return Ok(new { synced = success, opportunityId = id });
    }

    // ------------------------------------------------------------------ //
    //  INT-004: Calendly Scheduling Webhook Integration
    // ------------------------------------------------------------------ //

    /// <summary>Returns Calendly integration status and configured webhook events.</summary>
    [HttpGet("calendly/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult CalendlyStatus()
    {
        var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/webhooks/calendly";
        return Ok(new
        {
            enabled = true,
            webhookUrl,
            configuredEvents = new[] { "invitee.created", "invitee.canceled" }
        });
    }

    /// <summary>Registers the CRM webhook URL with the Calendly API.</summary>
    [HttpPost("calendly/register-webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CalendlyRegisterWebhook(CancellationToken ct)
    {
        var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/webhooks/calendly";
        var success = await _calendly.RegisterWebhookAsync(webhookUrl, ct);
        return Ok(new { registered = success, webhookUrl });
    }

    /// <summary>Returns the next 10 upcoming Calendly scheduled events for the configured user.</summary>
    [HttpGet("calendly/events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CalendlyUpcomingEvents(CancellationToken ct)
    {
        var events = await _calendly.GetUpcomingEventsAsync(ct);
        return Ok(events);
    }
}
