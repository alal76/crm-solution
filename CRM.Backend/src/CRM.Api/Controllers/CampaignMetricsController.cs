// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing campaign metrics.
/// Provides endpoints for campaign analytics and performance analysis.
/// </summary>
[ApiController]
[Route("api/campaign-metrics")]
[Authorize]
public class CampaignMetricsController : CrmControllerBase
{
    private readonly ICampaignMetricService _service;
    private readonly ILogger<CampaignMetricsController> _logger;

    /// <summary>
    /// Initializes a new instance of the CampaignMetricsController.
    /// </summary>
    public CampaignMetricsController(
        ICampaignMetricService service,
        ILogger<CampaignMetricsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new campaign metric record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignMetric), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CampaignMetric metric,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Creating campaign metric for campaign: campaignId={CampaignId}", metric.CampaignId);
        var result = await _service.CreateAsync(metric, cancellationToken);
        return CreatedAtAction(nameof(GetMetrics), new { id = result.CampaignId }, result);
    }

    /// <summary>
    /// Gets metrics for a specific campaign.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMetrics(
        int id,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Getting metrics for campaign: id={CampaignId}", id);
        var result = await _service.GetMetricsAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { message = $"Campaign with id {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Analyzes campaign performance and generates insights.
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(CampaignAnalysisResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Analyze(
        [FromBody] CampaignAnalysisDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Analyzing campaign: campaignId={CampaignId}", dto.CampaignId);
        var result = await _service.AnalyzeAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Generates a preview of campaign metrics before execution.
    /// </summary>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(CampaignMetricsPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Preview(
        [FromBody] CampaignPreviewDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Generating campaign preview");
        var result = await _service.PreviewAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Duplicates metrics from an existing campaign to a new campaign.
    /// </summary>
    [HttpPost("duplicate")]
    [ProducesResponseType(typeof(CampaignMetricsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Duplicate(
        [FromBody] CampaignDuplicationDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Duplicating campaign metrics: sourceCampaignId={SourceCampaignId}, targetCampaignId={TargetCampaignId}",
            dto.SourceCampaignId, dto.TargetCampaignId);
        var result = await _service.DuplicateAsync(dto, cancellationToken);
        if (result == null)
            return NotFound(new { message = "Source campaign metrics not found" });

        return CreatedAtAction(nameof(GetMetrics), new { id = dto.TargetCampaignId }, result);
    }

    /// <summary>
    /// Retargets a campaign based on performance metrics.
    /// </summary>
    [HttpPost("retarget")]
    [ProducesResponseType(typeof(CampaignRetargetingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Retarget(
        [FromBody] CampaignRetargetingDto dto,
        CancellationToken cancellationToken = default)
    {
                _logger.LogInformation("Retargeting campaign: campaignId={CampaignId}", dto.CampaignId);
        var result = await _service.RetargetAsync(dto, cancellationToken);
        return Ok(result);
    }
}
