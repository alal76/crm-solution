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

namespace CRM.Api.Controllers;

/// <summary>
/// Analytics controller for external BI provider dashboards and embeds.
/// Bridges IAnalyticsPort providers (Superset/PowerBI/BuiltIn) to the frontend.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IProviderFactory<IAnalyticsPort> _analyticsFactory;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IProviderFactory<IAnalyticsPort> analyticsFactory, ILogger<AnalyticsController> logger)
    {
        _analyticsFactory = analyticsFactory ?? throw new ArgumentNullException(nameof(analyticsFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string? GetCurrentUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value;

    private List<string> GetCurrentUserRoles() =>
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

        return filters.Count > 0 ? filters : null;
    }

    /// <summary>
    /// Gets available analytics dashboards for the current user.
    /// </summary>
    [HttpGet("dashboards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDashboards(CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _analyticsFactory.GetProvider();
            if (!await provider.IsAvailableAsync(cancellationToken))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Analytics provider is unavailable." });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics dashboards");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to retrieve analytics dashboards." });
        }
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
        try
        {
            var provider = _analyticsFactory.GetProvider();
            if (!await provider.IsAvailableAsync(cancellationToken))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Analytics provider is unavailable." });
            }

            var dashboard = await provider.GetDashboardAsync(id, cancellationToken);
            if (dashboard == null)
            {
                return NotFound();
            }

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics dashboard {DashboardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to retrieve analytics dashboard." });
        }
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
        try
        {
            var provider = _analyticsFactory.GetProvider();
            if (!await provider.IsAvailableAsync(cancellationToken))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Analytics provider is unavailable." });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics embed for dashboard {DashboardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to generate analytics embed." });
        }
    }

    /// <summary>
    /// Gets available charts/widgets.
    /// </summary>
    [HttpGet("charts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetCharts([FromQuery] string? dashboardId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _analyticsFactory.GetProvider();
            if (!await provider.IsAvailableAsync(cancellationToken))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Analytics provider is unavailable." });
            }

            var charts = await provider.GetChartsAsync(dashboardId, cancellationToken);
            return Ok(charts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics charts");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to retrieve analytics charts." });
        }
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
        try
        {
            var provider = _analyticsFactory.GetProvider();
            if (!await provider.IsAvailableAsync(cancellationToken))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Analytics provider is unavailable." });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics embed for chart {ChartId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to generate analytics chart embed." });
        }
    }
}
