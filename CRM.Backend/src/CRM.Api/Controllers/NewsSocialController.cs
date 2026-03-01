// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for News and Social Media feeds.
///
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Fetching news articles about a company/account
/// - Fetching social media posts from linked accounts
/// - Combined feeds for 360° account view
///
/// TECHNICAL VIEW:
/// - Uses INewsSocialService for external API integration
/// - Integrates with NewsAPI.org for news articles
/// - Integrates with Twitter/X API for tweets
/// - Integrates with LinkedIn API for company posts
/// - Caches results to reduce API calls
///
/// API ROUTES:
/// - GET    /api/news-social/{accountId}          - Get feeds for account
/// - GET    /api/news-social/news?companyName=...  - Get news only
/// - GET    /api/news-social/status                - Check API configuration status
/// - POST   /api/news-social/refresh/{accountId}  - Force refresh feeds
/// </summary>
[ApiController]
[Route("api/news-social")]
[Authorize]
public class NewsSocialController : CrmControllerBase
{
    private readonly INewsSocialService _newsSocialService;
    private readonly ILogger<NewsSocialController> _logger;

    public NewsSocialController(
        INewsSocialService newsSocialService,
        ILogger<NewsSocialController> logger)
    {
        _newsSocialService = newsSocialService;
        _logger = logger;
    }

    /// <summary>
    /// Get API configuration status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetStatus()
    {
        return Ok(new
        {
            newsApiConfigured = _newsSocialService.IsNewsApiConfigured(),
            socialApiConfigured = _newsSocialService.IsSocialApiConfigured(),
            message = !_newsSocialService.IsNewsApiConfigured() && !_newsSocialService.IsSocialApiConfigured()
                ? "No external APIs are configured. Please configure NewsAPI and/or social media API keys in settings."
                : "External APIs are configured and ready."
        });
    }

    /// <summary>
    /// Get news and social feeds for an account
    /// </summary>
    [HttpGet("{accountId:int}")]
    [ProducesResponseType(typeof(NewsSocialFeedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NewsSocialFeedResponse>> GetFeedsForAccount(
        int accountId,
        [FromQuery] int maxNewsItems = 10,
        [FromQuery] int maxSocialItems = 10,
        CancellationToken cancellationToken = default)
    {
                var request = new NewsSocialFeedRequest
        {
            AccountId = accountId,
            MaxNewsItems = maxNewsItems,
            MaxSocialItems = maxSocialItems,
            RefreshCache = false
        };

        var response = await _newsSocialService.GetFeedsAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Force refresh feeds for an account (bypass cache)
    /// </summary>
    [HttpPost("refresh/{accountId:int}")]
    [ProducesResponseType(typeof(NewsSocialFeedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NewsSocialFeedResponse>> RefreshFeeds(
        int accountId,
        [FromQuery] int maxNewsItems = 10,
        [FromQuery] int maxSocialItems = 10,
        CancellationToken cancellationToken = default)
    {
                var request = new NewsSocialFeedRequest
        {
            AccountId = accountId,
            MaxNewsItems = maxNewsItems,
            MaxSocialItems = maxSocialItems,
            RefreshCache = true
        };

        var response = await _newsSocialService.GetFeedsAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get news for a specific company name
    /// </summary>
    [HttpGet("news")]
    [ProducesResponseType(typeof(List<NewsItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<NewsItemDto>>> GetNews(
        [FromQuery] string companyName,
        [FromQuery] int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return BadRequest("Company name is required");
        }

                var news = await _newsSocialService.GetNewsAsync(companyName, maxItems, cancellationToken);
        return Ok(news);
    }

    /// <summary>
    /// Get social feeds for given handles/URLs
    /// </summary>
    [HttpGet("social")]
    [ProducesResponseType(typeof(List<SocialFeedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SocialFeedDto>>> GetSocialFeeds(
        [FromQuery] string? linkedInUrl,
        [FromQuery] string? twitterHandle,
        [FromQuery] string? facebookUrl,
        [FromQuery] int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
                var feeds = await _newsSocialService.GetSocialFeedsAsync(
            linkedInUrl, twitterHandle, facebookUrl, maxItems, cancellationToken);
        return Ok(feeds);
    }

    /// <summary>
    /// Analyze sentiment of text
    /// </summary>
    [HttpPost("sentiment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> AnalyzeSentiment(
        [FromBody] SentimentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required");
        }

        try
        {
            var sentiment = await _newsSocialService.AnalyzeSentimentAsync(request.Text, cancellationToken);
            return Ok(new { sentiment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return Ok(new { sentiment = "neutral" });
        }
    }
}

public class SentimentRequest
{
    public string Text { get; set; } = string.Empty;
}
