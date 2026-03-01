// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Provides score history and explanation endpoints for individual leads.
/// FEAT-AISCORING: AI Lead Scoring Real-time Triggers
/// </summary>
[ApiController]
[Route("api/leads")]
[Authorize]
[Produces("application/json")]
public class LeadScoreController : CrmControllerBase
{
    private readonly ILeadScoreHistoryService _historyService;

    public LeadScoreController(
        ILeadScoreHistoryService historyService)
    {
        _historyService = historyService;
    }

    /// <summary>
    /// Returns the score change history for a lead, ordered newest first.
    /// </summary>
    /// <param name="id">Lead primary key.</param>
    /// <param name="limit">Maximum records to return (default 20, max 100).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id}/score-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetScoreHistory(
        int id,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
                limit = Math.Clamp(limit, 1, 100);
        var history = await _historyService.GetHistoryAsync(id, limit, ct);
        return Ok(new { success = true, leadId = id, history });
    }

    /// <summary>
    /// Returns a full score explanation with component breakdown, trend, and recent history.
    /// </summary>
    /// <param name="id">Lead primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id}/score-explanation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetScoreExplanation(int id, CancellationToken ct = default)
    {
                var explanation = await _historyService.GetExplanationAsync(id, ct);
        if (explanation == null)
        {
            return NotFound(new { error = $"Lead {id} not found" });
        }

        return Ok(explanation);
    }
}
