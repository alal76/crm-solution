// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers.ITSM;

/// <summary>
/// API controller for escalation analytics and reporting.
/// TODO-SD005-011: Escalation Analytics Reports.
/// </summary>
[ApiController]
[Route("api/escalationanalytics")]
[Produces("application/json")]
[Tags("ITSM - Escalation Analytics")]
public class EscalationAnalyticsController : ControllerBase
{
    private readonly IEscalationAnalyticsService _analyticsService;
    private readonly ILogger<EscalationAnalyticsController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="EscalationAnalyticsController"/>.
    /// </summary>
    public EscalationAnalyticsController(
        IEscalationAnalyticsService analyticsService,
        ILogger<EscalationAnalyticsController> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns a consolidated 30-day escalation analytics summary.
    /// Includes: total escalations, avg time-to-escalate by severity,
    /// escalation rate by category, top-5 request types, and resolution rate.
    /// </summary>
    /// <returns>30-day escalation analytics summary.</returns>
    /// <response code="200">Returns the escalation analytics summary.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("summary")]
    [Authorize]
    [ProducesResponseType(typeof(EscalationAnalyticsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _analyticsService.GetAnalyticsSummaryAsync(cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating escalation analytics summary");
            return StatusCode(500, new { message = "Error generating escalation analytics summary", error = ex.Message });
        }
    }

    /// <summary>
    /// Returns the full escalation dashboard for a custom date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive). Defaults to 30 days ago.</param>
    /// <param name="endDate">End date (inclusive). Defaults to today.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Full escalation dashboard.</returns>
    /// <response code="200">Returns the dashboard data.</response>
    /// <response code="400">Invalid date range.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(EscalationDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-30);

            if (start > end)
                return BadRequest(new { message = "startDate must be before endDate" });

            var dashboard = await _analyticsService.GetEscalationDashboardAsync(start, end, cancellationToken);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalation dashboard");
            return StatusCode(500, new { message = "Error retrieving escalation dashboard", error = ex.Message });
        }
    }

    /// <summary>
    /// Returns escalation counts grouped by service request category.
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Escalation statistics by category.</returns>
    /// <response code="200">Returns the escalation-by-category list.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("by-category")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<EscalationByCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCategory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-30);

            var results = await _analyticsService.GetEscalationsByCategoryAsync(start, end, cancellationToken);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalations by category");
            return StatusCode(500, new { message = "Error retrieving escalations by category", error = ex.Message });
        }
    }
}
