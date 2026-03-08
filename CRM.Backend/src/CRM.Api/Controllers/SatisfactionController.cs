// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// REST endpoints for CSAT / NPS / CES customer satisfaction surveys.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SatisfactionController : CrmControllerBase
{
    private readonly ISatisfactionService _service;
    private readonly ILogger<SatisfactionController> _logger;

    public SatisfactionController(ISatisfactionService service, ILogger<SatisfactionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/satisfaction
    [HttpGet]
    public async Task<IActionResult> GetSurveys(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetSurveysAsync(page, pageSize, entityType, ct);
        return Ok(result);
    }

    // GET /api/satisfaction/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var survey = await _service.GetSurveyByIdAsync(id, ct);
        return survey is null ? NotFound() : Ok(survey);
    }

    // POST /api/satisfaction — create a new survey
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSatisfactionSurveyDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _service.CreateSurveyAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // POST /api/satisfaction/respond — submit a response (public, no auth)
    [HttpPost("respond")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitResponse(
        [FromBody] SubmitSatisfactionResponseDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _service.SubmitResponseAsync(dto, ct);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Response submission rejected: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/satisfaction/metrics
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? entityType = null,
        CancellationToken ct = default)
    {
        var metrics = await _service.GetMetricsAsync(from, to, entityType, ct);
        return Ok(metrics);
    }

    // GET /api/satisfaction/nps
    [HttpGet("nps")]
    public async Task<IActionResult> GetNPS(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var score = await _service.GetNPSScoreAsync(from, to, ct);
        return Ok(new { npsScore = score });
    }

    // GET /api/satisfaction/csat
    [HttpGet("csat")]
    public async Task<IActionResult> GetCSAT(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var score = await _service.GetCSATScoreAsync(from, to, ct);
        return Ok(new { csatScore = score });
    }

    // GET /api/satisfaction/csat/summary
    [HttpGet("csat/summary")]
    public async Task<IActionResult> GetCSATSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var score = await _service.GetCSATScoreAsync(from, to, ct);
        var metrics = await _service.GetMetricsAsync(from, to, null, ct);
        return Ok(new { csatScore = score, totalResponses = metrics?.TotalResponses ?? 0, period = new { from, to } });
    }

    // GET /api/satisfaction/nps/summary
    [HttpGet("nps/summary")]
    public async Task<IActionResult> GetNPSSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var score = await _service.GetNPSScoreAsync(from, to, ct);
        var metrics = await _service.GetMetricsAsync(from, to, null, ct);
        return Ok(new { npsScore = score, totalResponses = metrics?.TotalResponses ?? 0, period = new { from, to } });
    }

    // GET /api/satisfaction/nps/trend
    [HttpGet("nps/trend")]
    public async Task<IActionResult> GetNPSTrend(
        [FromQuery] int months = 6,
        CancellationToken ct = default)
    {
        var trends = new List<object>();
        var now = DateTime.UtcNow;
        for (int i = months - 1; i >= 0; i--)
        {
            var from = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var to = from.AddMonths(1).AddTicks(-1);
            var score = await _service.GetNPSScoreAsync(from, to, ct);
            trends.Add(new { month = from.ToString("yyyy-MM"), npsScore = score });
        }
        return Ok(new { trends, months });
    }
}
