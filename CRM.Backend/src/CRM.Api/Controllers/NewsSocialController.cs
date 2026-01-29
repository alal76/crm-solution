// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// News & Social Media Feed Controller
// Provides endpoints to fetch real-time news and social media feeds for customers

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for News and Social Media feeds.
/// 
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Fetching news articles about a company/customer
/// - Fetching social media posts from linked accounts
/// - Combined feeds for 360° customer view
/// 
/// TECHNICAL VIEW:
/// - Uses INewsSocialService for external API integration
/// - Integrates with NewsAPI.org for news articles
/// - Integrates with Twitter/X API for tweets
/// - Integrates with LinkedIn API for company posts
/// - Caches results to reduce API calls
/// 
/// API ROUTES:
/// - GET    /api/news-social/{customerId}          - Get feeds for customer
/// - GET    /api/news-social/news?companyName=...  - Get news only
/// - GET    /api/news-social/status                - Check API configuration status
/// - POST   /api/news-social/refresh/{customerId}  - Force refresh feeds
/// </summary>
[ApiController]
[Route("api/news-social")]
[Authorize]
public class NewsSocialController : ControllerBase
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
    /// Get news and social feeds for a customer
    /// </summary>
    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<NewsSocialFeedResponse>> GetFeedsForCustomer(
        int customerId,
        [FromQuery] int maxNewsItems = 10,
        [FromQuery] int maxSocialItems = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new NewsSocialFeedRequest
            {
                CustomerId = customerId,
                MaxNewsItems = maxNewsItems,
                MaxSocialItems = maxSocialItems,
                RefreshCache = false
            };

            var response = await _newsSocialService.GetFeedsAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching feeds for customer {CustomerId}", customerId);
            return StatusCode(500, new NewsSocialFeedResponse
            {
                Error = "Failed to fetch news and social feeds"
            });
        }
    }

    /// <summary>
    /// Force refresh feeds for a customer (bypass cache)
    /// </summary>
    [HttpPost("refresh/{customerId:int}")]
    public async Task<ActionResult<NewsSocialFeedResponse>> RefreshFeeds(
        int customerId,
        [FromQuery] int maxNewsItems = 10,
        [FromQuery] int maxSocialItems = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new NewsSocialFeedRequest
            {
                CustomerId = customerId,
                MaxNewsItems = maxNewsItems,
                MaxSocialItems = maxSocialItems,
                RefreshCache = true
            };

            var response = await _newsSocialService.GetFeedsAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing feeds for customer {CustomerId}", customerId);
            return StatusCode(500, new NewsSocialFeedResponse
            {
                Error = "Failed to refresh news and social feeds"
            });
        }
    }

    /// <summary>
    /// Get news for a specific company name
    /// </summary>
    [HttpGet("news")]
    public async Task<ActionResult<List<NewsItemDto>>> GetNews(
        [FromQuery] string companyName,
        [FromQuery] int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return BadRequest("Company name is required");
        }

        try
        {
            var news = await _newsSocialService.GetNewsAsync(companyName, maxItems, cancellationToken);
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news for company {CompanyName}", companyName);
            return StatusCode(500, new List<NewsItemDto>());
        }
    }

    /// <summary>
    /// Get social feeds for given handles/URLs
    /// </summary>
    [HttpGet("social")]
    public async Task<ActionResult<List<SocialFeedDto>>> GetSocialFeeds(
        [FromQuery] string? linkedInUrl,
        [FromQuery] string? twitterHandle,
        [FromQuery] string? facebookUrl,
        [FromQuery] int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var feeds = await _newsSocialService.GetSocialFeedsAsync(
                linkedInUrl, twitterHandle, facebookUrl, maxItems, cancellationToken);
            return Ok(feeds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching social feeds");
            return StatusCode(500, new List<SocialFeedDto>());
        }
    }

    /// <summary>
    /// Analyze sentiment of text
    /// </summary>
    [HttpPost("sentiment")]
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
