// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// AI-powered insights endpoints.
/// Implements TODO-AI-03, TODO-AI-04, TODO-AI-07, TODO-AI-08, TODO-AI-09, TODO-AI-10.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AIInsightsController : CrmControllerBase
{
    private readonly IChurnPredictionService _churnService;
    private readonly INextBestActionService _nbaService;
    private readonly IEmailSentimentService _sentimentService;
    private readonly IMeetingSummaryService _summaryService;
    private readonly IDealRiskService _dealRiskService;
    private readonly IRevenueForecastService _forecastService;

    public AIInsightsController(
        IChurnPredictionService churnService,
        INextBestActionService nbaService,
        IEmailSentimentService sentimentService,
        IMeetingSummaryService summaryService,
        IDealRiskService dealRiskService,
        IRevenueForecastService forecastService)
    {
        _churnService = churnService;
        _nbaService = nbaService;
        _sentimentService = sentimentService;
        _summaryService = summaryService;
        _dealRiskService = dealRiskService;
        _forecastService = forecastService;
    }

    // ------------------------------------------------------------------ //
    //  TODO-AI-03: Churn Prediction
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Returns churn risk prediction for the specified account.
    /// </summary>
    [HttpGet("accounts/{id:int}/churn-risk")]
    [ProducesResponseType(typeof(ChurnPredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChurnRisk(int id, CancellationToken ct)
    {
        var result = await _churnService.PredictChurnAsync(id, ct);
        return result is null ? NotFound(new { message = $"Account {id} not found" }) : Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-AI-04: Next Best Actions
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Returns prioritised next-best-action recommendations for the specified account.
    /// </summary>
    [HttpGet("accounts/{id:int}/next-best-actions")]
    [ProducesResponseType(typeof(IEnumerable<NextBestActionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNextBestActions(int id, CancellationToken ct)
    {
        var result = await _nbaService.GetRecommendationsAsync(id, ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-AI-07: Email Sentiment
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Analyses the sentiment of raw email body text.
    /// </summary>
    [HttpPost("email/sentiment")]
    [ProducesResponseType(typeof(SentimentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> AnalyzeEmailSentiment(
        [FromBody] EmailSentimentRequest request,
        CancellationToken ct)
    {
        var result = await _sentimentService.AnalyzeSentimentAsync(request.Body, ct);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-AI-08: Meeting Summary
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Generates a structured meeting summary for the specified interaction.
    /// </summary>
    [HttpPost("interactions/{id:int}/summary")]
    [ProducesResponseType(typeof(MeetingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateMeetingSummary(int id, CancellationToken ct)
    {
        var result = await _summaryService.GenerateSummaryAsync(id, ct);
        return result is null ? NotFound(new { message = $"Interaction {id} not found" }) : Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-AI-09: Deal Risk Score
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Returns the risk assessment for the specified opportunity.
    /// </summary>
    [HttpGet("opportunities/{id:int}/risk")]
    [ProducesResponseType(typeof(DealRiskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealRisk(int id, CancellationToken ct)
    {
        var result = await _dealRiskService.CalculateRiskAsync(id, ct);
        return result is null ? NotFound(new { message = $"Opportunity {id} not found" }) : Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  TODO-AI-10: Revenue Forecast
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Returns a monthly revenue forecast for the specified number of months ahead (default 6).
    /// </summary>
    [HttpGet("revenue-forecast")]
    [ProducesResponseType(typeof(RevenueForecastDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueForecast([FromQuery] int months = 6, CancellationToken ct = default)
    {
        var result = await _forecastService.ForecastRevenueAsync(months, ct);
        return Ok(result);
    }
}
