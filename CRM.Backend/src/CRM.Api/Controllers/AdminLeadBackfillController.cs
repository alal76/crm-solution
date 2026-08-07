// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Infrastructure;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin-only controller for REM-ORPHAN-003 groundwork: migrating legacy "Contacts-as-Leads"
/// data (Contact rows with ContactType = Lead) into the real Lead table.
/// This is a maintenance tool — it is never invoked automatically.
/// </summary>
[ApiController]
[Route("api/admin/lead-backfill")]
[Authorize(Roles = "Admin")]
public class AdminLeadBackfillController : CrmControllerBase
{
    private readonly ILeadBackfillService _backfillService;
    private readonly ILeadHistoryContinuityService _historyContinuityService;
    private readonly ILogger<AdminLeadBackfillController> _logger;

    public AdminLeadBackfillController(
        ILeadBackfillService backfillService,
        ILeadHistoryContinuityService historyContinuityService,
        ILogger<AdminLeadBackfillController> logger)
    {
        _backfillService = backfillService ?? throw new ArgumentNullException(nameof(backfillService));
        _historyContinuityService = historyContinuityService ?? throw new ArgumentNullException(nameof(historyContinuityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the legacy Contacts-as-Leads -> Lead table backfill.
    /// Safe by default: <paramref name="dryRun"/> defaults to true, so omitting the query
    /// parameter performs no database writes and only reports what would happen.
    /// Pass <c>?dryRun=false</c> to actually create Lead rows.
    /// </summary>
    /// <param name="dryRun">Defaults to true (no writes). Set to false to persist changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunBackfill([FromQuery] bool dryRun = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Lead backfill triggered by admin. DryRun={DryRun}", dryRun);

        var result = await _backfillService.RunAsync(dryRun, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// REM-LEAD-HISTORY-CONTINUITY: re-parents Activity/RecordComment history that was created
    /// against the OLD Contact record (EntityType == "Contact") onto its migrated Lead record
    /// (EntityType == "Lead"), so history shows up on the new Lead's timeline instead of the
    /// legacy Contact's. Only applies to Leads created via the backfill tool (Lead.ContactId
    /// is set). Safe by default: <paramref name="dryRun"/> defaults to true, so omitting the
    /// query parameter performs no database writes and only reports what would happen.
    /// Pass <c>?dryRun=false</c> to actually re-parent the history.
    /// </summary>
    /// <param name="dryRun">Defaults to true (no writes). Set to false to persist changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("history-continuity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunHistoryContinuity([FromQuery] bool dryRun = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Lead history continuity triggered by admin. DryRun={DryRun}", dryRun);

        var result = await _historyContinuityService.RunAsync(dryRun, cancellationToken);

        return Ok(result);
    }
}
