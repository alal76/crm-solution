// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Analytics controller for external BI provider dashboards and embeds.
/// Bridges IAnalyticsPort providers (Superset/PowerBI/BuiltIn) to the frontend.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AnalyticsController : CrmControllerBase
{
    private const string AnalyticsUnavailableMessage = "Analytics provider is unavailable.";

    private readonly IProviderFactory<IAnalyticsPort> _analyticsFactory;

    public AnalyticsController(IProviderFactory<IAnalyticsPort> analyticsFactory)
    {
        _analyticsFactory = analyticsFactory ?? throw new ArgumentNullException(nameof(analyticsFactory));
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string? GetCurrentUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value; // NOSONAR

    private List<string> GetCurrentUserRoles() => // NOSONAR
        User.FindAll(ClaimTypes.Role).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

    private static Dictionary<string, string>? ParseFilters(IQueryCollection query)
    {
        var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in query)
        {
            if (!kvp.Key.StartsWith("filter_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var key = kvp.Key.Substring("filter_".Length);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            filters[key] = kvp.Value.ToString();
        }

        // S2583: filters.Count can be 0 or more depending on query input
        return filters.Count > 0 ? filters : null; // NOSONAR - S2583 false positive: Count depends on runtime query parameters
    }

    /// <summary>
    /// Gets available analytics dashboards for the current user.
    /// </summary>
    [HttpGet("dashboards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDashboards(CancellationToken cancellationToken = default)
    {
                var provider = _analyticsFactory.GetProvider();
        if (!await provider.IsAvailableAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = AnalyticsUnavailableMessage });
        }

        var userId = GetCurrentUserId();
        var roles = GetCurrentUserRoles();
        var dashboards = await provider.GetDashboardsForUserAsync(userId, roles, cancellationToken);

        var response = dashboards.Select(d => new
        {
            id = d.Id,
            name = d.Name,
            description = d.Description,
            embedUrl = d.Url,
            thumbnailUrl = d.ThumbnailUrl,
            tags = d.Tags
        });

        return Ok(response);
    }

    /// <summary>
    /// Gets a dashboard by ID.
    /// </summary>
    [HttpGet("dashboards/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDashboard(string id, CancellationToken cancellationToken = default)
    {
                var provider = _analyticsFactory.GetProvider();
        if (!await provider.IsAvailableAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = AnalyticsUnavailableMessage });
        }

        var dashboard = await provider.GetDashboardAsync(id, cancellationToken);
        if (dashboard == null)
        {
            return NotFound();
        }

        return Ok(dashboard);
    }

    /// <summary>
    /// Gets embed configuration for a dashboard.
    /// </summary>
    [HttpGet("dashboards/{id}/embed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDashboardEmbed(string id, CancellationToken cancellationToken = default)
    {
                var provider = _analyticsFactory.GetProvider();
        if (!await provider.IsAvailableAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = AnalyticsUnavailableMessage });
        }

        if (!provider.SupportsEmbedding)
        {
            return BadRequest(new { message = "Analytics provider does not support embedding." });
        }

        var request = new EmbedRequest
        {
            EmbedType = "dashboard",
            ResourceId = id,
            UserId = GetCurrentUserId(),
            UserEmail = GetCurrentUserEmail(),
            Roles = GetCurrentUserRoles(),
            Filters = ParseFilters(Request.Query),
            HideHeader = true
        };

        var embed = await provider.GetEmbedAsync(request, cancellationToken);

        return Ok(new
        {
            embedUrl = embed.EmbedUrl,
            token = embed.Token,
            expiresAt = embed.ExpiresAt,
            dashboardId = id,
            provider = provider.ProviderName
        });
    }

    /// <summary>
    /// Gets available charts/widgets.
    /// </summary>
    [HttpGet("charts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetCharts([FromQuery] string? dashboardId = null, CancellationToken cancellationToken = default)
    {
                var provider = _analyticsFactory.GetProvider();
        if (!await provider.IsAvailableAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = AnalyticsUnavailableMessage });
        }

        var charts = await provider.GetChartsAsync(dashboardId, cancellationToken);
        return Ok(charts);
    }

    /// <summary>
    /// Gets embed configuration for a chart.
    /// </summary>
    [HttpGet("charts/{id}/embed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetChartEmbed(string id, CancellationToken cancellationToken = default)
    {
                var provider = _analyticsFactory.GetProvider();
        if (!await provider.IsAvailableAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = AnalyticsUnavailableMessage });
        }

        if (!provider.SupportsEmbedding)
        {
            return BadRequest(new { message = "Analytics provider does not support embedding." });
        }

        var embed = await provider.GetChartEmbedAsync(id, ParseFilters(Request.Query), cancellationToken);
        return Ok(new
        {
            embedUrl = embed.EmbedUrl,
            token = embed.Token,
            expiresAt = embed.ExpiresAt,
            chartId = id,
            provider = provider.ProviderName
        });
    }
}
