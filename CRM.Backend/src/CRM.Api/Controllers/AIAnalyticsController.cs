// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for AI-powered analytics, semantic search, lead scoring,
/// opportunity scoring, custom dashboards, and custom reports.
/// Phase 7 of the remediation plan.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AIAnalyticsController : CrmControllerBase
{
    private readonly IAIKnowledgeSearchService _kbSearch;
    private readonly IAILeadScoringService _leadScoring;
    private readonly IAIOpportunityScoringService _opportunityScoring;
    private readonly IDashboardBuilderService _dashboardBuilder;
    private readonly IReportBuilderService _reportBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIAnalyticsController"/> class.
    /// </summary>
    public AIAnalyticsController(
        IAIKnowledgeSearchService kbSearch,
        IAILeadScoringService leadScoring,
        IAIOpportunityScoringService opportunityScoring,
        IDashboardBuilderService dashboardBuilder,
        IReportBuilderService reportBuilder)
    {
        _kbSearch = kbSearch;
        _leadScoring = leadScoring;
        _opportunityScoring = opportunityScoring;
        _dashboardBuilder = dashboardBuilder;
        _reportBuilder = reportBuilder;
    }

    // =========================================================================
    // 7.1 AI-Powered Knowledge Base Search
    // =========================================================================

    /// <summary>
    /// Perform AI-powered semantic search over knowledge base articles.
    /// Falls back to keyword search when AI provider is unavailable.
    /// </summary>
    [HttpPost("kb-search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SemanticSearch([FromBody] KbSearchRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
        {
            return BadRequest("Query is required.");
        }

        var results = await _kbSearch.SemanticSearchAsync(request.Query, request.TopK, ct);
        return Ok(results);
    }

    /// <summary>
    /// Reindex all published knowledge base articles for semantic search.
    /// </summary>
    [HttpPost("kb-reindex")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReindexKnowledgeBase(CancellationToken ct)
    {
        await _kbSearch.ReindexAllAsync(ct);
        return Ok(new { status = "reindex_complete", timestamp = DateTime.UtcNow });
    }

    // =========================================================================
    // 7.2 Enhanced Lead Scoring
    // =========================================================================

    /// <summary>
    /// Score a single lead using weighted multi-factor analysis.
    /// Optionally enriched with AI sentiment analysis.
    /// </summary>
    [HttpPost("leads/{id:int}/score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScoreLead(int id, CancellationToken ct)
    {
        var result = await _leadScoring.ScoreLeadAsync(id, ct);
        if (result == null)
        {
            return NotFound($"Lead with ID {id} not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Score all active leads in bulk. Updates lead records with new scores.
    /// </summary>
    [HttpPost("leads/score-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ScoreAllLeads(CancellationToken ct)
    {
        var result = await _leadScoring.ScoreAllLeadsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Get the current lead scoring weight configuration.
    /// </summary>
    [HttpGet("leads/scoring-weights")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetScoringWeights()
    {
        var weights = _leadScoring.GetScoringWeights();
        return Ok(weights);
    }

    // =========================================================================
    // 7.3 Predictive Opportunity Scoring
    // =========================================================================

    /// <summary>
    /// Score a single opportunity for win probability prediction.
    /// </summary>
    [HttpPost("opportunities/{id:int}/score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScoreOpportunity(int id, CancellationToken ct)
    {
        var result = await _opportunityScoring.ScoreOpportunityAsync(id, ct);
        if (result == null)
        {
            return NotFound($"Opportunity with ID {id} not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Score all open opportunities in bulk.
    /// </summary>
    [HttpPost("opportunities/score-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ScoreAllOpportunities(CancellationToken ct)
    {
        var results = await _opportunityScoring.ScoreAllOpenAsync(ct);
        return Ok(results);
    }

    /// <summary>
    /// Get historical win rates by stage for calibration.
    /// </summary>
    [HttpGet("opportunities/win-rates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistoricalWinRates(CancellationToken ct)
    {
        var rates = await _opportunityScoring.GetHistoricalWinRatesAsync(ct);
        return Ok(rates);
    }

    // =========================================================================
    // 7.4 Custom Dashboard Builder
    // =========================================================================

    /// <summary>
    /// Get all dashboards for the current user.
    /// </summary>
    [HttpGet("dashboards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboards([FromQuery] int userId = 0, CancellationToken ct = default)
    {
        var dashboards = await _dashboardBuilder.GetDashboardsAsync(userId, ct);
        return Ok(dashboards);
    }

    /// <summary>
    /// Get a specific dashboard by ID.
    /// </summary>
    [HttpGet("dashboards/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboard(string id, CancellationToken ct)
    {
        var dashboard = await _dashboardBuilder.GetDashboardAsync(id, ct);
        if (dashboard == null)
        {
            return NotFound($"Dashboard '{id}' not found.");
        }
        return Ok(dashboard);
    }

    /// <summary>
    /// Create a new custom dashboard.
    /// </summary>
    [HttpPost("dashboards")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDashboard([FromBody] CustomDashboard dashboard, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dashboard?.Name))
        {
            return BadRequest("Dashboard name is required.");
        }

        // Ensure Widgets is never null to prevent NullReferenceException in service
        dashboard.Widgets ??= new List<DashboardWidget>();

        var created = await _dashboardBuilder.CreateDashboardAsync(dashboard, ct);
        return CreatedAtAction(nameof(GetDashboard), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing dashboard.
    /// </summary>
    [HttpPut("dashboards/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDashboard(string id, [FromBody] CustomDashboard dashboard, CancellationToken ct)
    {
        dashboard.Id = id;
        var updated = await _dashboardBuilder.UpdateDashboardAsync(dashboard, ct);
        if (updated == null)
        {
            return NotFound($"Dashboard '{id}' not found.");
        }
        return Ok(updated);
    }

    /// <summary>
    /// Delete a dashboard.
    /// </summary>
    [HttpDelete("dashboards/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDashboard(string id, CancellationToken ct)
    {
        var deleted = await _dashboardBuilder.DeleteDashboardAsync(id, ct);
        if (!deleted)
        {
            return NotFound($"Dashboard '{id}' not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Get the catalog of available widget types.
    /// </summary>
    [HttpGet("dashboards/widgets/catalog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetWidgetCatalog()
    {
        var catalog = _dashboardBuilder.GetAvailableWidgets();
        return Ok(catalog);
    }

    /// <summary>
    /// Get live data for a specific widget.
    /// </summary>
    [HttpGet("dashboards/widgets/{widgetId}/data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWidgetData(string widgetId, CancellationToken ct)
    {
        var data = await _dashboardBuilder.GetWidgetDataAsync(widgetId, ct);
        if (data == null)
        {
            return NotFound($"Widget '{widgetId}' not found.");
        }
        return Ok(data);
    }

    // =========================================================================
    // 7.5 Report Designer
    // =========================================================================

    /// <summary>
    /// Get all report definitions for a user.
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports([FromQuery] int userId = 0, CancellationToken ct = default)
    {
        var reports = await _reportBuilder.GetReportsAsync(userId, ct);
        return Ok(reports);
    }

    /// <summary>
    /// Get a specific report definition by ID.
    /// </summary>
    [HttpGet("reports/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(string id, CancellationToken ct)
    {
        var report = await _reportBuilder.GetReportAsync(id, ct);
        if (report == null)
        {
            return NotFound($"Report '{id}' not found.");
        }
        return Ok(report);
    }

    /// <summary>
    /// Create a new report definition.
    /// </summary>
    [HttpPost("reports")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReport([FromBody] ReportDefinition report, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(report?.Name))
        {
            return BadRequest("Report name is required.");
        }

        var created = await _reportBuilder.CreateReportAsync(report, ct);
        return CreatedAtAction(nameof(GetReport), new { id = created.Id }, created);
    }

    /// <summary>
    /// Execute a report and return the result rows.
    /// </summary>
    [HttpPost("reports/{id}/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateReport(string id, CancellationToken ct)
    {
        var result = await _reportBuilder.ExecuteReportAsync(id, ct);
        if (result == null)
        {
            return NotFound($"Report '{id}' not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Export a report as CSV file.
    /// </summary>
    [HttpGet("reports/{id}/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportReportCsv(string id, CancellationToken ct)
    {
        var csv = await _reportBuilder.ExportToCsvAsync(id, ct);
        if (csv == null)
        {
            return NotFound($"Report '{id}' not found.");
        }
        return File(csv, "text/csv", $"report-{id}.csv");
    }

    /// <summary>
    /// Get available entity sources for report building.
    /// </summary>
    [HttpGet("reports/sources")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetReportSources()
    {
        var sources = _reportBuilder.GetAvailableSources();
        return Ok(sources);
    }

    // ── 7.6 Account Health & Risk ──────────────────────────────────────────

    /// <summary>Returns accounts flagged as at-risk based on activity and engagement metrics.</summary>
    [HttpGet("accounts/at-risk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAtRiskAccounts([FromQuery] int limit = 20)
    {
        return Ok(new { items = Array.Empty<object>(), totalCount = 0, message = "AI at-risk account analysis requires scoring model configuration." });
    }

    /// <summary>Gets an AI-generated health score for an account.</summary>
    [HttpGet("accounts/{id:int}/health-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAccountHealthScore(int id)
    {
        return Ok(new { accountId = id, healthScore = 0, factors = Array.Empty<object>(), message = "Health score calculation pending AI model integration." });
    }

    /// <summary>Runs AI analysis on an account.</summary>
    [HttpPost("accounts/{id:int}/analyze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AnalyzeAccount(int id)
    {
        return Ok(new { accountId = id, analysis = (object?)null, message = "Account analysis pending AI model integration." });
    }

    // ── 7.7 Opportunity Intelligence ───────────────────────────────────────

    /// <summary>Returns an AI-generated risk report for all open opportunities.</summary>
    [HttpGet("opportunities/risk-report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetOpportunityRiskReport([FromQuery] int limit = 20)
    {
        return Ok(new { items = Array.Empty<object>(), totalCount = 0, message = "Opportunity risk report pending AI model integration." });
    }

    /// <summary>Returns AI-generated recommendations for an opportunity.</summary>
    [HttpGet("opportunities/{id:int}/recommendations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetOpportunityRecommendations(int id)
    {
        return Ok(new { opportunityId = id, recommendations = Array.Empty<object>(), message = "Recommendations pending AI model integration." });
    }

    /// <summary>Runs AI analysis on an opportunity.</summary>
    [HttpPost("opportunities/{id:int}/analyze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AnalyzeOpportunity(int id)
    {
        return Ok(new { opportunityId = id, analysis = (object?)null, message = "Opportunity analysis pending AI model integration." });
    }

    /// <summary>Returns AI-estimated win probability for an opportunity.</summary>
    [HttpPost("opportunities/{id:int}/win-probability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetWinProbability(int id)
    {
        return Ok(new { opportunityId = id, winProbability = 0.0, confidence = 0.0, factors = Array.Empty<object>(), message = "Win probability calculation pending AI model integration." });
    }
}

/// <summary>
/// Request model for knowledge base semantic search.
/// </summary>
public class KbSearchRequest
{
    /// <summary>Natural language query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Maximum number of results to return (default: 10).</summary>
    public int TopK { get; set; } = 10;
}
