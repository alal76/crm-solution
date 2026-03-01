// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin endpoints for search analytics (INFRA-10).
/// Provides insights into popular queries, zero-result searches, and performance.
/// </summary>
[ApiController]
[Route("api/admin/search-analytics")]
[Authorize(Roles = "Admin,Manager")]
public class AdminSearchAnalyticsController : CrmControllerBase
{
    private readonly ISearchAnalyticsService _analytics;

    public AdminSearchAnalyticsController(ISearchAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    /// <summary>Gets the most popular search queries.</summary>
    [HttpGet("popular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularQueries(
        [FromQuery] int top = 20,
        [FromQuery] DateTime? since = null,
        CancellationToken ct = default)
    {
        var queries = await _analytics.GetPopularQueriesAsync(top, since, ct);
        return Ok(queries);
    }

    /// <summary>Gets search queries that returned zero results.</summary>
    [HttpGet("zero-results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetZeroResultQueries(
        [FromQuery] int top = 20,
        [FromQuery] DateTime? since = null,
        CancellationToken ct = default)
    {
        var queries = await _analytics.GetZeroResultQueriesAsync(top, since, ct);
        return Ok(queries);
    }

    /// <summary>Gets overall search performance metrics.</summary>
    [HttpGet("performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics(
        [FromQuery] DateTime? since = null,
        CancellationToken ct = default)
    {
        var metrics = await _analytics.GetPerformanceMetricsAsync(since, ct);
        return Ok(metrics);
    }

    /// <summary>Gets search counts broken down by entity type.</summary>
    [HttpGet("by-entity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntityType(
        [FromQuery] DateTime? since = null,
        CancellationToken ct = default)
    {
        var breakdown = await _analytics.GetSearchesByEntityTypeAsync(since, ct);
        return Ok(breakdown);
    }

    /// <summary>Gets hourly/daily search volume between two dates.</summary>
    [HttpGet("volume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSearchVolume(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var resolvedFrom = from ?? DateTime.UtcNow.AddDays(-7);
        var resolvedTo = to ?? DateTime.UtcNow;

        if (resolvedFrom > resolvedTo)
            return BadRequest(new { message = "from must be before to." });

        var volume = await _analytics.GetSearchVolumeAsync(resolvedFrom, resolvedTo, ct);
        return Ok(volume);
    }

    /// <summary>Gets a combined summary of all search analytics.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? since = null,
        CancellationToken ct = default)
    {
        var popularTask = _analytics.GetPopularQueriesAsync(10, since, ct);
        var zeroTask = _analytics.GetZeroResultQueriesAsync(10, since, ct);
        var perfTask = _analytics.GetPerformanceMetricsAsync(since, ct);
        var byTypeTask = _analytics.GetSearchesByEntityTypeAsync(since, ct);

        await Task.WhenAll(popularTask, zeroTask, perfTask, byTypeTask);

        return Ok(new
        {
            popularQueries = popularTask.Result,
            zeroResultQueries = zeroTask.Result,
            performance = perfTask.Result,
            byEntityType = byTypeTask.Result
        });
    }
}
