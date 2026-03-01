// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Public system-information endpoints that do not require authentication.
/// These endpoints expose configuration data needed by frontend forms and
/// third-party integrations.
///
/// BACK-009: Billing Timezone — GET /api/system/timezones
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController : CrmControllerBase
{
    private readonly IBillingTimezoneService _timezoneService;
    private readonly ILogger<SystemController> _logger;

    /// <summary>Initialises a new instance of <see cref="SystemController"/>.</summary>
    public SystemController(
        IBillingTimezoneService timezoneService,
        ILogger<SystemController> logger)
    {
        _timezoneService = timezoneService ?? throw new ArgumentNullException(nameof(timezoneService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/system/timezones
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all timezones supported by the current server runtime,
    /// sorted by base UTC offset then by timezone ID.
    ///
    /// This endpoint is unauthenticated so that the frontend can populate
    /// timezone picker controls before the user logs in.
    ///
    /// BACK-009: Billing Timezone
    /// </summary>
    /// <returns>List of <see cref="TimezoneInfoDto"/> records.</returns>
    /// <response code="200">Sorted list of supported timezones.</response>
    [HttpGet("timezones")]
    [ProducesResponseType(typeof(IReadOnlyList<TimezoneInfoDto>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedTimezones()
    {
        var timezones = _timezoneService.GetSupportedTimezones();
        _logger.LogDebug("GetSupportedTimezones returned {Count} entries.", timezones.Count);
        return Ok(timezones);
    }
}
