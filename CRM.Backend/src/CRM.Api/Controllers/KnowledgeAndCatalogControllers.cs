// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Linq;
using System.Security.Claims;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Knowledge Management API endpoints for managing knowledge articles and documentation.
/// </summary>
/// <remarks>
/// Knowledge Management enables the creation, curation, and sharing of knowledge articles.
/// Key features include: article CRUD, publishing workflow, feedback collection, article suggestions, and full-text search.
/// </remarks>
[ApiController]
[Route("api/itsm/knowledge")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Knowledge Management")]
public class KnowledgeController : CrmControllerBase
{
    private readonly IKnowledgeManagementService _knowledgeService;
    private readonly IArticleRecommendationService _recommendationService;
    private readonly IKCSWorkflowService _kcsService;

    public KnowledgeController(
        IKnowledgeManagementService knowledgeService,
        IArticleRecommendationService recommendationService,
        IKCSWorkflowService kcsService)
    {
        _knowledgeService = knowledgeService;
        _recommendationService = recommendationService;
        _kcsService = kcsService;
    }

    /// <summary>
    /// Create a new knowledge article.
    /// </summary>
    /// <param name="dto">Article details</param>
    /// <returns>The created article</returns>
    [HttpPost]
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeArticleDto>> CreateArticle([FromBody] CreateKnowledgeArticleDto dto)
    {
        var article = await _knowledgeService.CreateArticleAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetArticle), new { id = article.ArticleId }, article);
    }

    /// <summary>
    /// Get a knowledge article by ID.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>The article details</returns>
    [HttpGet("{id:int}")]
    [HttpGet("articles/{id:int}")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base articles accessible to unauthenticated portal users
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KnowledgeArticleDto>> GetArticle(int id)
    {
        var article = await _knowledgeService.GetArticleByIdAsync(id);
        return article == null ? NotFound() : Ok(article);
    }

    /// <summary>
    /// Search knowledge articles using full-text search.
    /// </summary>
    /// <param name="searchTerm">Search keywords</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Matching articles</returns>
    [HttpGet("search")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base search accessible to unauthenticated portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> SearchArticles(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var articles = await _knowledgeService.SearchArticlesAsync(searchTerm, pageNumber, pageSize);
        return Ok(articles);
    }

    /// <summary>
    /// Get articles pending approval/publishing.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of pending articles</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetPendingArticles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var articles = await _knowledgeService.SearchArticlesAsync(string.Empty, pageNumber, pageSize);
        var pending = articles.Where(article => article.PublishingState != PublishingState.Published);
        return Ok(pending);
    }

    /// <summary>
    /// Update an existing knowledge article.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <param name="dto">Updated article data</param>
    /// <returns>The updated article</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KnowledgeArticleDto>> UpdateArticle(int id, [FromBody] CreateKnowledgeArticleDto dto)
    {
        var article = await _knowledgeService.UpdateArticleAsync(id, dto, GetCurrentUserId());
        return Ok(article);
    }

    /// <summary>
    /// Publish a knowledge article.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id:int}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PublishArticle(int id)
    {
        await _knowledgeService.PublishArticleAsync(id, GetCurrentUserId());
        return Ok();
    }

    /// <summary>
    /// Retire a knowledge article.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id:int}/retire")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RetireArticle(int id)
    {
        await _knowledgeService.RetireArticleAsync(id, GetCurrentUserId());
        return Ok();
    }

    /// <summary>
    /// Submit feedback on a knowledge article.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <param name="dto">Feedback details</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:int}/feedback")]
    [AllowAnonymous] // NOSONAR - S4834: Public product catalog accessible to unauthenticated portal/landing page users
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddFeedback(int id, [FromBody] AddFeedbackDto dto)
    {
        var result = await _knowledgeService.SubmitFeedbackAsync(id, GetCurrentUserId(), dto.Helpful, dto.Comments);
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Get suggested articles based on incident description.
    /// </summary>
    /// <param name="incidentDescription">Description text to match against</param>
    /// <returns>List of suggested articles</returns>
    [HttpGet("suggestions")]
    [AllowAnonymous] // NOSONAR - S4834: Public product catalog categories accessible without authentication
    [ProducesResponseType(typeof(IEnumerable<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetSuggestedArticles([FromQuery] string incidentDescription)
    {
        var articles = await _knowledgeService.GetSuggestedArticlesAsync(incidentDescription);
        return Ok(articles);
    }

    /// <summary>
    /// Get most popular articles by view/feedback count.
    /// </summary>
    /// <param name="count">Number of articles to return (default: 10)</param>
    /// <returns>List of popular articles</returns>
    [HttpGet("popular")]
    [HttpGet("articles/popular")]
    [AllowAnonymous] // NOSONAR - S4834: Public service catalog accessible to unauthenticated customer portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetPopularArticles([FromQuery] int count = 10)
    {
        var articles = await _knowledgeService.GetPopularArticlesAsync(count);
        return Ok(articles);
    }

    /// <summary>
    /// Get recently published articles.
    /// </summary>
    /// <param name="count">Number of articles to return (default: 10)</param>
    /// <returns>List of recent articles</returns>
    [HttpGet("recent")]
    [HttpGet("articles/recent")]
    [AllowAnonymous] // NOSONAR - S4834: Public service catalog search accessible without authentication
    [ProducesResponseType(typeof(IEnumerable<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetRecentArticles([FromQuery] int count = 10)
    {
        var articles = await _knowledgeService.GetRecentArticlesAsync(count);
        return Ok(articles);
    }

    /// <summary>
    /// List or search knowledge articles via /articles path.
    /// </summary>
    /// <param name="search">Optional search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Matching articles</returns>
    [HttpGet("articles")]
    [AllowAnonymous] // NOSONAR - S4834: Public service catalog items accessible to unauthenticated portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> ListArticles(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var articles = await _knowledgeService.SearchArticlesAsync(search ?? string.Empty, pageNumber, pageSize);
        return Ok(articles);
    }

    /// <summary>
    /// Get all knowledge article categories.
    /// </summary>
    /// <returns>List of category names</returns>
    [HttpGet("categories")]
    [AllowAnonymous] // NOSONAR - S4834: Public service catalog item details accessible without authentication
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _knowledgeService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get version history for a knowledge article.
    /// TODO-SD002-010: GET /api/knowledgearticles/{id}/versions endpoint
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>List of article versions</returns>
    [HttpGet("{id:int}/versions")]
    [ProducesResponseType(typeof(IEnumerable<ArticleVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ArticleVersionDto>>> GetArticleVersions(int id)
    {
        var article = await _knowledgeService.GetArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        var versions = await _knowledgeService.GetArticleVersionsAsync(id);
        return Ok(versions);
    }

    /// <summary>
    /// Restore a previous version of a knowledge article.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <param name="versionId">The version ID to restore</param>
    /// <returns>The restored article</returns>
    [HttpPost("{id:int}/restore/{versionId:int}")]
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeArticleDto>> RestoreArticleVersion(int id, int versionId)
    {
        var article = await _knowledgeService.RestoreArticleVersionAsync(id, versionId, GetCurrentUserId());
        return article == null ? NotFound() : Ok(article);
    }

    // ────────────────────────────────────────────────────────────────
    // Article recommendations (IArticleRecommendationService)
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get article recommendations for an incident based on category and popularity/helpfulness ranking.
    /// </summary>
    /// <param name="incidentId">The incident ID</param>
    /// <returns>List of recommended articles with relevance scores</returns>
    [HttpGet("incidents/{incidentId:int}/article-recommendations")]
    [AllowAnonymous] // NOSONAR - S4834: Recommendations surface only public article metadata, consistent with the existing /suggestions endpoint
    [ProducesResponseType(typeof(IEnumerable<ArticleRecommendation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArticleRecommendation>>> GetArticleRecommendations(int incidentId)
    {
        var recommendations = await _recommendationService.GetRecommendationsAsync(incidentId);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get trending knowledge articles based on view count and recent activity.
    /// </summary>
    /// <param name="count">Number of trending articles to return (default: 10)</param>
    /// <returns>List of trending articles with trend direction</returns>
    [HttpGet("articles/trending")]
    [AllowAnonymous] // NOSONAR - S4834: Public trending article list, consistent with the existing /popular and /recent endpoints
    [ProducesResponseType(typeof(IEnumerable<TrendingArticle>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TrendingArticle>>> GetTrendingArticles([FromQuery] int count = 10)
    {
        var articles = await _recommendationService.GetTrendingArticlesAsync(count);
        return Ok(articles);
    }

    // ────────────────────────────────────────────────────────────────
    // KCS (Knowledge-Centered Service) workflow (IKCSWorkflowService)
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Start a KCS capture session for an incident.
    /// </summary>
    /// <param name="incidentId">The incident ID</param>
    /// <returns>The started capture session</returns>
    [HttpPost("incidents/{incidentId:int}/kcs/capture-session")]
    [ProducesResponseType(typeof(KCSSession), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KCSSession>> StartKCSCaptureSession(int incidentId)
    {
        try
        {
            var session = await _kcsService.StartCaptureSessionAsync(incidentId, GetCurrentUserId());
            return Ok(session);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Create a KCS draft article from a resolved incident.
    /// </summary>
    /// <param name="incidentId">The incident ID</param>
    /// <returns>The generated draft article</returns>
    [HttpPost("incidents/{incidentId:int}/kcs/draft")]
    [ProducesResponseType(typeof(KCSDraftArticle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KCSDraftArticle>> CreateKCSDraft(int incidentId)
    {
        try
        {
            var draft = await _kcsService.CreateDraftFromIncidentAsync(incidentId, GetCurrentUserId());
            return Ok(draft);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Submit a knowledge article for KCS review.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>The created review request</returns>
    [HttpPost("{id:int}/kcs/submit-review")]
    [ProducesResponseType(typeof(KCSReviewRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KCSReviewRequest>> SubmitKCSReview(int id)
    {
        try
        {
            var reviewRequest = await _kcsService.SubmitForReviewAsync(id, GetCurrentUserId());
            return Ok(reviewRequest);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Record a review decision (approve/reject/etc.) for a KCS review request.
    /// </summary>
    /// <param name="reviewRequestId">The review request ID</param>
    /// <param name="dto">The review decision and optional feedback</param>
    /// <returns>The review result</returns>
    [HttpPost("kcs/reviews/{reviewRequestId:int}/decide")]
    [ProducesResponseType(typeof(KCSReviewResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KCSReviewResult>> DecideKCSReview(int reviewRequestId, [FromBody] KCSReviewDecisionRequest dto)
    {
        try
        {
            var result = await _kcsService.ReviewArticleAsync(reviewRequestId, GetCurrentUserId(), dto.Decision, dto.Feedback);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get KCS metrics for an agent over a date range.
    /// </summary>
    /// <param name="agentId">The agent (user) ID</param>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>Agent KCS metrics</returns>
    [HttpGet("kcs/agents/{agentId:int}/metrics")]
    [ProducesResponseType(typeof(KCSAgentMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<KCSAgentMetrics>> GetKCSAgentMetrics(int agentId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var metrics = await _kcsService.GetAgentMetricsAsync(agentId, fromDate, toDate);
        return Ok(metrics);
    }

    /// <summary>
    /// Get the KCS lifecycle status for a knowledge article.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>The article's lifecycle information</returns>
    [HttpGet("{id:int}/kcs/lifecycle")]
    [ProducesResponseType(typeof(KCSArticleLifecycle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KCSArticleLifecycle>> GetKCSArticleLifecycle(int id)
    {
        try
        {
            var lifecycle = await _kcsService.GetArticleLifecycleAsync(id);
            return Ok(lifecycle);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Flag a knowledge article for KCS review or retirement.
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <param name="dto">The flag type and reason</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:int}/kcs/flag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> FlagKCSArticle(int id, [FromBody] KCSFlagRequest dto)
    {
        var result = await _kcsService.FlagArticleAsync(id, dto.Flag, GetCurrentUserId(), dto.Reason);
        return result ? Ok() : NotFound();
    }

    /// <summary>
    /// Get the KCS coaching queue for the current reviewer.
    /// </summary>
    /// <returns>List of coaching queue items</returns>
    [HttpGet("kcs/coaching-queue")]
    [ProducesResponseType(typeof(List<KCSCoachingItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<KCSCoachingItem>>> GetKCSCoachingQueue()
    {
        var queue = await _kcsService.GetCoachingQueueAsync(GetCurrentUserId());
        return Ok(queue);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1"); // NOSONAR
}

/// <summary>
/// Service Catalog API endpoints.
/// </summary>
/// <remarks>
/// The Service Catalog provides a self-service portal where users can browse and request IT services,
/// hardware, software, and other predefined catalog items. Supports categories, featured items,
/// request creation, and request tracking.
/// </remarks>
[ApiController]
[Route("api/itsm/catalog")]
[Authorize]
[Produces("application/json")]
[Tags("ITSM - Service Catalog")]
public class CatalogController : CrmControllerBase
{
    private readonly IServiceCatalogService _catalogService;
    private readonly ICatalogApprovalService _approvalService;
    private readonly ICatalogFulfillmentService _fulfillmentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogController"/> class.
    /// </summary>
    /// <param name="catalogService">The service catalog service.</param>
    /// <param name="approvalService">The catalog request approval workflow service.</param>
    /// <param name="fulfillmentService">The catalog request fulfillment automation service.</param>
    public CatalogController(
        IServiceCatalogService catalogService,
        ICatalogApprovalService approvalService,
        ICatalogFulfillmentService fulfillmentService)
    {
        _catalogService = catalogService;
        _approvalService = approvalService;
        _fulfillmentService = fulfillmentService;
    }

    /// <summary>
    /// Get all catalog items, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <returns>List of catalog items</returns>
    [HttpGet("items")]
    [AllowAnonymous] // NOSONAR - S4834: Public FAQ articles accessible to unauthenticated portal and landing page users
    [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetCatalogItems([FromQuery] int? categoryId)
    {
        var items = await _catalogService.GetCatalogItemsAsync(categoryId, null);
        return Ok(items);
    }

    /// <summary>
    /// Get a specific catalog item by ID.
    /// </summary>
    /// <param name="id">The catalog item ID</param>
    /// <returns>The catalog item details</returns>
    [HttpGet("items/{id}")]
    [AllowAnonymous] // NOSONAR - S4834: Public FAQ categories accessible without authentication
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CatalogItemDto>> GetCatalogItem(int id)
    {
        var item = await _catalogService.GetCatalogItemByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create a new catalog service request.
    /// </summary>
    /// <param name="dto">The catalog request details</param>
    /// <returns>The created request ID</returns>
    [HttpPost("requests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateCatalogRequest([FromBody] CreateCatalogRequestDto dto)
    {
        var requestId = await _catalogService.CreateCatalogRequestAsync(dto, GetCurrentUserId());
        return Ok(new { requestId });
    }

    /// <summary>
    /// Get all catalog requests submitted by the current user.
    /// </summary>
    /// <returns>List of the current user's catalog requests</returns>
    [HttpGet("my-requests")]
    [ProducesResponseType(typeof(IEnumerable<CatalogRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CatalogRequest>>> GetMyRequests()
    {
        var requests = await _catalogService.GetMyRequestsAsync(GetCurrentUserId());
        return Ok(requests);
    }

    /// <summary>
    /// Search catalog items by term.
    /// </summary>
    /// <param name="searchTerm">The search term to filter catalog items</param>
    /// <returns>List of matching catalog items</returns>
    [HttpGet("search")]
    [AllowAnonymous] // NOSONAR - S4834: Public announcement accessible to unauthenticated portal users
    [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> SearchCatalog([FromQuery(Name = "term")] string? searchTerm)
    {
        var items = await _catalogService.SearchCatalogAsync(searchTerm ?? string.Empty);
        return Ok(items);
    }

    /// <summary>
    /// Get featured catalog items.
    /// </summary>
    /// <returns>List of featured catalog items</returns>
    [HttpGet("featured")]
    [AllowAnonymous] // NOSONAR - S4834: Public announcements list accessible without authentication
    [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetFeaturedItems()
    {
        var items = await _catalogService.GetCatalogItemsAsync(null, true);
        return Ok(items);
    }

    /// <summary>
    /// Get all catalog categories.
    /// </summary>
    /// <returns>List of catalog categories with item counts</returns>
    [HttpGet("categories")]
    [AllowAnonymous] // NOSONAR - S4834: Public announcement details accessible without authentication
    [ProducesResponseType(typeof(IEnumerable<CatalogCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CatalogCategoryDto>>> GetCategories()
    {
        var categories = await _catalogService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Create a catalog request on behalf of another user.
    /// </summary>
    /// <param name="dto">The request details including the target user</param>
    /// <returns>The created request ID</returns>
    [HttpPost("requests/for-others")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateRequestForOthers([FromBody] CreateCatalogRequestForOthersRequest dto)
    {
        var createDto = new CRM.Core.Interfaces.ITSM.CreateCatalogRequestForOthersDto
        {
            CatalogItemId = dto.CatalogItemId,
            RequestedForUserId = dto.RequestedForUserId,
            Notes = dto.Notes,
            FormData = dto.FormData
        };
        var requestId = await _catalogService.CreateCatalogRequestForOthersAsync(createDto, GetCurrentUserId());
        return Ok(new { requestId });
    }

    /// <summary>
    /// Get a specific catalog request by ID.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <returns>The catalog request details</returns>
    [HttpGet("requests/{requestId}")]
    [ProducesResponseType(typeof(CatalogRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CatalogRequest>> GetRequestById(int requestId)
    {
        var request = await _catalogService.GetRequestByIdAsync(requestId);
        return request == null ? NotFound() : Ok(request);
    }

    /// <summary>
    /// Cancel a pending catalog request.
    /// </summary>
    /// <param name="requestId">The catalog request ID to cancel</param>
    /// <returns>Success or failure</returns>
    [HttpPatch("requests/{requestId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelRequest(int requestId)
    {
        var result = await _catalogService.CancelRequestAsync(requestId, GetCurrentUserId());
        return result ? Ok() : BadRequest("Unable to cancel request");
    }

    // ────────────────────────────────────────────────────────────────
    // Catalog request approval workflow
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Submit a catalog request for approval.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <returns>The created (or auto-approved) approval workflow</returns>
    [HttpPost("requests/{requestId}/approval/submit")]
    [ProducesResponseType(typeof(ApprovalWorkflow), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalWorkflow>> SubmitForApproval(int requestId)
    {
        try
        {
            var workflow = await _approvalService.SubmitForApprovalAsync(requestId, GetCurrentUserId());
            return Ok(workflow);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get the current approval status for a catalog request.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <returns>The approval workflow, if one exists</returns>
    [HttpGet("requests/{requestId}/approval/status")]
    [ProducesResponseType(typeof(ApprovalWorkflow), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalWorkflow>> GetApprovalStatus(int requestId)
    {
        var workflow = await _approvalService.GetApprovalStatusAsync(requestId);
        return workflow == null ? NotFound() : Ok(workflow);
    }

    /// <summary>
    /// Get the approval action history for a catalog request.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <returns>List of approval actions taken</returns>
    [HttpGet("requests/{requestId}/approval/history")]
    [ProducesResponseType(typeof(IEnumerable<ApprovalAction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApprovalAction>>> GetApprovalHistory(int requestId)
    {
        var history = await _approvalService.GetApprovalHistoryAsync(requestId);
        return Ok(history);
    }

    /// <summary>
    /// Withdraw a pending approval request.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <param name="dto">Withdrawal reason</param>
    /// <returns>Success or failure</returns>
    [HttpPost("requests/{requestId}/approval/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> WithdrawApproval(int requestId, [FromBody] ReasonRequest dto)
    {
        var result = await _approvalService.WithdrawApprovalAsync(requestId, GetCurrentUserId(), dto.Reason);
        return result ? Ok() : BadRequest("Unable to withdraw approval");
    }

    /// <summary>
    /// Process an approval decision (approve/reject/etc.) for a workflow stage.
    /// </summary>
    /// <param name="workflowId">The approval workflow ID</param>
    /// <param name="dto">The decision and optional comments</param>
    /// <returns>The recorded approval action</returns>
    [HttpPost("approval/workflows/{workflowId}/process")]
    [ProducesResponseType(typeof(ApprovalAction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApprovalAction>> ProcessApproval(int workflowId, [FromBody] ProcessApprovalRequest dto)
    {
        try
        {
            var action = await _approvalService.ProcessApprovalAsync(workflowId, GetCurrentUserId(), dto.Decision, dto.Comments);
            return Ok(action);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Escalate an overdue approval workflow.
    /// </summary>
    /// <param name="workflowId">The approval workflow ID</param>
    /// <param name="dto">Escalation reason</param>
    /// <returns>Success or failure</returns>
    [HttpPost("approval/workflows/{workflowId}/escalate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> EscalateApproval(int workflowId, [FromBody] ReasonRequest dto)
    {
        var result = await _approvalService.EscalateApprovalAsync(workflowId, dto.Reason);
        return result ? Ok() : NotFound();
    }

    /// <summary>
    /// Get pending approvals assigned to the current user.
    /// </summary>
    /// <returns>List of pending approval items</returns>
    [HttpGet("approval/pending")]
    [ProducesResponseType(typeof(IEnumerable<PendingServiceRequestApproval>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PendingServiceRequestApproval>>> GetPendingApprovals()
    {
        var pending = await _approvalService.GetPendingApprovalsAsync(GetCurrentUserId());
        return Ok(pending);
    }

    /// <summary>
    /// Get the approval rule configuration for a catalog item.
    /// </summary>
    /// <param name="catalogItemId">The catalog item ID</param>
    /// <returns>The approval rule</returns>
    [HttpGet("items/{catalogItemId}/approval-rule")]
    [ProducesResponseType(typeof(CatalogApprovalRule), StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogApprovalRule>> GetApprovalRule(int catalogItemId)
    {
        var rule = await _approvalService.GetApprovalRuleAsync(catalogItemId);
        return Ok(rule);
    }

    // ────────────────────────────────────────────────────────────────
    // Catalog request fulfillment automation
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Start fulfillment of an approved catalog request.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <returns>The fulfillment workflow</returns>
    [HttpPost("requests/{requestId}/fulfillment/start")]
    [ProducesResponseType(typeof(FulfillmentWorkflow), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FulfillmentWorkflow>> StartFulfillment(int requestId)
    {
        try
        {
            var workflow = await _fulfillmentService.StartFulfillmentAsync(requestId);
            return Ok(workflow);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get the current fulfillment status for a catalog request.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <returns>The fulfillment workflow, if one exists</returns>
    [HttpGet("requests/{requestId}/fulfillment")]
    [ProducesResponseType(typeof(FulfillmentWorkflow), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FulfillmentWorkflow>> GetFulfillmentStatus(int requestId)
    {
        var workflow = await _fulfillmentService.GetFulfillmentStatusAsync(requestId);
        return workflow == null ? NotFound() : Ok(workflow);
    }

    /// <summary>
    /// Cancel fulfillment of a catalog request.
    /// </summary>
    /// <param name="requestId">The catalog request ID</param>
    /// <param name="dto">Cancellation reason</param>
    /// <returns>Success or failure</returns>
    [HttpPost("requests/{requestId}/fulfillment/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelFulfillment(int requestId, [FromBody] ReasonRequest dto)
    {
        var result = await _fulfillmentService.CancelFulfillmentAsync(requestId, dto.Reason, GetCurrentUserId());
        return result ? Ok() : BadRequest("Unable to cancel fulfillment");
    }

    /// <summary>
    /// Complete a fulfillment task.
    /// </summary>
    /// <param name="taskId">The fulfillment task ID</param>
    /// <param name="dto">Optional completion notes</param>
    /// <returns>The completed task</returns>
    [HttpPost("fulfillment/tasks/{taskId}/complete")]
    [ProducesResponseType(typeof(FulfillmentTask), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FulfillmentTask>> CompleteFulfillmentTask(int taskId, [FromBody] CompleteFulfillmentTaskRequest dto)
    {
        try
        {
            var task = await _fulfillmentService.CompleteTaskAsync(taskId, GetCurrentUserId(), dto.Notes);
            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Execute an automated fulfillment task.
    /// </summary>
    /// <param name="taskId">The fulfillment task ID</param>
    /// <returns>The automation execution result</returns>
    [HttpPost("fulfillment/tasks/{taskId}/automate")]
    [ProducesResponseType(typeof(AutomationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutomationResult>> ExecuteFulfillmentAutomation(int taskId)
    {
        try
        {
            var result = await _fulfillmentService.ExecuteAutomationAsync(taskId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get the fulfillment template for a catalog item.
    /// </summary>
    /// <param name="catalogItemId">The catalog item ID</param>
    /// <returns>The fulfillment template</returns>
    [HttpGet("items/{catalogItemId}/fulfillment-template")]
    [ProducesResponseType(typeof(FulfillmentTemplate), StatusCodes.Status200OK)]
    public async Task<ActionResult<FulfillmentTemplate>> GetFulfillmentTemplate(int catalogItemId)
    {
        var template = await _fulfillmentService.GetFulfillmentTemplateAsync(catalogItemId);
        return Ok(template);
    }

    /// <summary>
    /// Create or update the fulfillment template for a catalog item.
    /// </summary>
    /// <param name="catalogItemId">The catalog item ID</param>
    /// <param name="template">The template definition</param>
    /// <returns>The saved template</returns>
    [HttpPut("items/{catalogItemId}/fulfillment-template")]
    [ProducesResponseType(typeof(FulfillmentTemplate), StatusCodes.Status200OK)]
    public async Task<ActionResult<FulfillmentTemplate>> SaveFulfillmentTemplate(int catalogItemId, [FromBody] FulfillmentTemplate template)
    {
        template.CatalogItemId = catalogItemId;
        var saved = await _fulfillmentService.SaveFulfillmentTemplateAsync(template);
        return Ok(saved);
    }

    /// <summary>
    /// Get fulfillment metrics for a date range.
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>Fulfillment metrics</returns>
    [HttpGet("fulfillment/metrics")]
    [ProducesResponseType(typeof(FulfillmentMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<FulfillmentMetrics>> GetFulfillmentMetrics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var metrics = await _fulfillmentService.GetMetricsAsync(fromDate, toDate);
        return Ok(metrics);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1"); // NOSONAR
}

public class CreateCatalogRequestForOthersRequest
{
    public int CatalogItemId { get; set; }
    public int RequestedForUserId { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, string>? FormData { get; set; }
}

public class CatalogCategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Generic reason payload used by catalog approval/fulfillment actions
/// (withdraw approval, escalate approval, cancel fulfillment).
/// </summary>
public class ReasonRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request body for processing an approval decision on a workflow stage.
/// </summary>
public class ProcessApprovalRequest
{
    public ApprovalDecision Decision { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Request body for completing a fulfillment task.
/// </summary>
public class CompleteFulfillmentTaskRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Service Level Agreement (SLA) Management API endpoints.
/// </summary>
/// <remarks>
/// SLA Management tracks and enforces service level commitments for incidents and service requests.
/// Key features include: SLA policies, breach detection, compliance metrics, pause/resume capabilities, and dashboard analytics.
/// </remarks>
[ApiController]
[Route("api/itsm/sla")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - SLA Management")]
public class SLAController : CrmControllerBase
{
    private readonly ISLAService _slaService;

    public SLAController(ISLAService slaService)
    {
        _slaService = slaService;
    }

    /// <summary>
    /// Create a new SLA policy.
    /// </summary>
    /// <param name="dto">Policy details</param>
    /// <returns>The created policy</returns>
    [HttpPost("policies")]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SLAPolicyDto>> CreatePolicy([FromBody] SLAPolicyDto dto)
    {
        var policy = await _slaService.CreateSLAPolicyAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetPolicy), new { id = policy.SLAPolicyId }, policy);
    }

    /// <summary>
    /// Get all SLA policies.
    /// </summary>
    /// <param name="targetType">Optional filter by target type (0=Incident, 1=ServiceRequest)</param>
    /// <returns>List of SLA policies</returns>
    [HttpGet("policies")]
    [ProducesResponseType(typeof(IEnumerable<SLAPolicyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SLAPolicyDto>>> GetPolicies([FromQuery] int? targetType)
    {
        var type = targetType.HasValue ? (SLATargetType?)targetType.Value : null;
        var policies = await _slaService.GetSLAPoliciesAsync(type);
        return Ok(policies);
    }

    /// <summary>
    /// Get an SLA policy by ID.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <returns>The policy details</returns>
    [HttpGet("policies/{id}")]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SLAPolicyDto>> GetPolicy(int id)
    {
        var policies = await _slaService.GetSLAPoliciesAsync(null);
        var policy = policies.FirstOrDefault(p => p.SLAPolicyId == id);
        return policy == null ? NotFound() : Ok(policy);
    }

    /// <summary>
    /// Get the active SLA instance for a target.
    /// </summary>
    /// <param name="targetId">Target entity ID (incident/request ID)</param>
    /// <param name="targetType">Target type (0=Incident, 1=ServiceRequest)</param>
    /// <returns>Active SLA instance</returns>
    [HttpGet("instances/{targetId}/{targetType}")]
    [ProducesResponseType(typeof(SLAInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SLAInstanceDto>> GetActiveSLA(int targetId, int targetType)
    {
        var sla = await _slaService.GetSLAInstanceAsync(targetId, (SLATargetType)targetType);
        return sla == null ? NotFound() : Ok(sla);
    }

    /// <summary>
    /// Get all breached SLAs.
    /// </summary>
    /// <returns>List of breached SLA instances</returns>
    [HttpGet("breached")]
    [ProducesResponseType(typeof(IEnumerable<SLAInstanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SLAInstanceDto>>> GetBreachedSLAs()
    {
        var slas = await _slaService.GetBreachedSLAsAsync();
        return Ok(slas);
    }

    /// <summary>
    /// Manually trigger SLA breach check.
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("check-breaches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CheckSLABreaches()
    {
        await _slaService.CheckSLABreachesAsync();
        return Ok();
    }

    /// <summary>
    /// Pause SLA tracking for a target (e.g., waiting for customer response).
    /// </summary>
    /// <param name="targetId">Target entity ID</param>
    /// <param name="targetType">Target type (0=Incident, 1=ServiceRequest)</param>
    /// <param name="dto">Pause reason</param>
    /// <returns>Success status</returns>
    [HttpPost("instances/{targetId}/{targetType}/pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PauseSLA(int targetId, int targetType, [FromBody] PauseSLADto dto)
    {
        await _slaService.PauseSLAAsync(targetId, (SLATargetType)targetType, dto.Reason);
        return Ok();
    }

    /// <summary>
    /// Resume SLA tracking for a paused target.
    /// </summary>
    /// <param name="targetId">Target entity ID</param>
    /// <param name="targetType">Target type (0=Incident, 1=ServiceRequest)</param>
    /// <returns>Success status</returns>
    [HttpPost("instances/{targetId}/{targetType}/resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResumeSLA(int targetId, int targetType)
    {
        await _slaService.ResumeSLAAsync(targetId, (SLATargetType)targetType);
        return Ok();
    }

    /// <summary>
    /// Get SLA dashboard with summary statistics.
    /// </summary>
    /// <returns>Dashboard data with active, breached, and at-risk SLAs</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(SLADashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SLADashboardDto>> GetDashboard()
    {
        var dashboard = await _slaService.GetSLADashboardAsync();
        return Ok(dashboard);
    }

    /// <summary>
    /// Get SLAs that are at risk of breaching.
    /// </summary>
    /// <param name="thresholdMinutes">Time threshold in minutes (default: 30)</param>
    /// <returns>List of at-risk SLA instances</returns>
    [HttpGet("at-risk")]
    [ProducesResponseType(typeof(IEnumerable<SLAInstanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SLAInstanceDto>>> GetAtRiskSLAs([FromQuery] int thresholdMinutes = 30)
    {
        var slas = await _slaService.GetAtRiskSLAsAsync(thresholdMinutes);
        return Ok(slas);
    }

    /// <summary>
    /// Get SLA compliance metrics for a date range.
    /// </summary>
    /// <param name="startDate">Start date (default: 30 days ago)</param>
    /// <param name="endDate">End date (default: now)</param>
    /// <returns>SLA metrics including compliance rates and averages</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(SLAMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SLAMetricsDto>> GetMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        var metrics = await _slaService.GetSLAMetricsAsync(start, end);
        return Ok(metrics);
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1"); // NOSONAR
}

public class SLADashboardDto
{
    public int TotalActiveSLAs { get; set; }
    public int BreachedCount { get; set; }
    public int AtRiskCount { get; set; }
    public int OnTrackCount { get; set; }
    public double OverallComplianceRate { get; set; }
    public IEnumerable<SLAInstanceDto> RecentBreaches { get; set; } = new List<SLAInstanceDto>();
    public IEnumerable<SLAInstanceDto> AtRiskItems { get; set; } = new List<SLAInstanceDto>();
}

public class SLAMetricsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalIncidents { get; set; }
    public int TotalBreaches { get; set; }
    public double ResponseComplianceRate { get; set; }
    public double ResolutionComplianceRate { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public double AverageResolutionTimeMinutes { get; set; }
    public Dictionary<int, double> ComplianceByPriority { get; set; } = new();
}

public class AddFeedbackDto
{
    public bool Helpful { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Request body for recording a KCS review decision.
/// </summary>
public class KCSReviewDecisionRequest
{
    public KCSReviewDecision Decision { get; set; }
    public string? Feedback { get; set; }
}

/// <summary>
/// Request body for flagging a knowledge article via the KCS workflow.
/// </summary>
public class KCSFlagRequest
{
    public KCSFlag Flag { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PauseSLADto
{
    public string Reason { get; set; } = string.Empty;
}
