// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos.KnowledgeBase;
using CRM.Core.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// General Knowledge Base API — article CRUD, search, categories, and case deflection.
/// Route: /api/knowledge
/// </summary>
[ApiController]
[Route("api/knowledge")]
[Authorize]
[Produces("application/json")]
[Tags("Knowledge Base")]
public class KnowledgeBaseController : CrmControllerBase
{
    private readonly IKnowledgeBaseService _service;
    // KB-012: unified search facade injected for /api/knowledge/search endpoint
    private readonly IUnifiedKnowledgeSearchService _unifiedSearchService;

    public KnowledgeBaseController(
        IKnowledgeBaseService service,
        IUnifiedKnowledgeSearchService unifiedSearchService)
    {
        _service = service;
        _unifiedSearchService = unifiedSearchService;
    }

    // =========================================================================
    // GET /api/knowledge/articles  — List / search articles
    // =========================================================================

    /// <summary>List or search knowledge base articles (paginated).</summary>
    [HttpGet("articles")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base accessible to portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, search, categoryId, status, ct);
        return Ok(result);
    }

    // =========================================================================
    // GET /api/knowledge/articles/{id}
    // =========================================================================

    /// <summary>Get a knowledge base article by ID.</summary>
    [HttpGet("articles/{id:int}")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base accessible to portal users
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var article = await _service.GetByIdAsync(id, ct);
        return article is null ? NotFound() : Ok(article);
    }

    // =========================================================================
    // GET /api/knowledge/articles/slug/{slug}
    // =========================================================================

    /// <summary>Get a knowledge base article by URL slug.</summary>
    [HttpGet("articles/slug/{slug}")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base accessible to portal users
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var article = await _service.GetBySlugAsync(slug, ct);
        return article is null ? NotFound() : Ok(article);
    }

    // =========================================================================
    // POST /api/knowledge/articles
    // =========================================================================

    /// <summary>Create a new knowledge base article.</summary>
    [HttpPost("articles")]
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateKnowledgeBaseArticleDto dto,
        CancellationToken ct = default)
    {
        var article = await _service.CreateAsync(dto, GetCurrentUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
    }

    // =========================================================================
    // PUT /api/knowledge/articles/{id}
    // =========================================================================

    /// <summary>Update an existing knowledge base article.</summary>
    [HttpPut("articles/{id:int}")]
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateKnowledgeBaseArticleDto dto,
        CancellationToken ct = default)
    {
        var article = await _service.UpdateAsync(id, dto, ct);
        return Ok(article);
    }

    // =========================================================================
    // DELETE /api/knowledge/articles/{id}
    // =========================================================================

    /// <summary>Soft-delete a knowledge base article.</summary>
    [HttpDelete("articles/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    // =========================================================================
    // PATCH /api/knowledge/articles/{id}/publish
    // =========================================================================

    /// <summary>Publish a knowledge base article.</summary>
    [HttpPatch("articles/{id:int}/publish")]
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct = default)
    {
        var article = await _service.PublishAsync(id, ct);
        return Ok(article);
    }

    // =========================================================================
    // PATCH /api/knowledge/articles/{id}/archive
    // =========================================================================

    /// <summary>Archive a knowledge base article.</summary>
    [HttpPatch("articles/{id:int}/archive")]
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(int id, CancellationToken ct = default)
    {
        var article = await _service.ArchiveAsync(id, ct);
        return Ok(article);
    }

    // =========================================================================
    // POST /api/knowledge/articles/{id}/feedback
    // =========================================================================

    /// <summary>Submit user feedback for a knowledge base article.</summary>
    [HttpPost("articles/{id:int}/feedback")]
    [AllowAnonymous] // NOSONAR - S4834: Anonymous feedback supported for portal visitors
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitFeedback(
        int id,
        [FromBody] KnowledgeBaseFeedbackDto feedback,
        CancellationToken ct = default)
    {
        await _service.SubmitFeedbackAsync(id, feedback, ct);
        return NoContent();
    }

    // =========================================================================
    // GET /api/knowledge/categories
    // =========================================================================

    /// <summary>Get all active knowledge base categories with article counts.</summary>
    [HttpGet("categories")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base accessible to portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct = default)
    {
        var categories = await _service.GetCategoriesAsync(ct);
        return Ok(categories);
    }

    // =========================================================================
    // POST /api/knowledge/categories
    // =========================================================================

    /// <summary>Create a new knowledge base category.</summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(KnowledgeCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateKnowledgeCategoryDto dto,
        CancellationToken ct = default)
    {
        var category = await _service.CreateCategoryAsync(dto, ct);
        return CreatedAtAction(nameof(GetCategories), new { }, category);
    }

    // =========================================================================
    // PUT /api/knowledge/categories/{id}
    // =========================================================================

    /// <summary>Update an existing knowledge base category.</summary>
    [HttpPut("categories/{id:int}")]
    [ProducesResponseType(typeof(KnowledgeCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] UpdateKnowledgeCategoryDto dto,
        CancellationToken ct = default)
    {
        var category = await _service.UpdateCategoryAsync(id, dto, ct);
        return Ok(category);
    }

    // =========================================================================
    // DELETE /api/knowledge/categories/{id}
    // =========================================================================

    /// <summary>Soft-delete a knowledge base category.</summary>
    [HttpDelete("categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct = default)
    {
        await _service.DeleteCategoryAsync(id, ct);
        return NoContent();
    }

    // =========================================================================
    // GET /api/knowledge/articles/popular
    // =========================================================================

    /// <summary>Get the most-viewed published articles.</summary>
    [HttpGet("articles/popular")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base accessible to portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopular(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var articles = await _service.GetPopularAsync(count, ct);
        return Ok(articles);
    }

    // =========================================================================
    // GET /api/knowledge/articles/recent
    // =========================================================================

    /// <summary>Get the most recently published articles.</summary>
    [HttpGet("articles/recent")]
    [AllowAnonymous] // NOSONAR - S4834: Public knowledge base accessible to portal users
    [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var articles = await _service.GetRecentAsync(count, ct);
        return Ok(articles);
    }

    // =========================================================================
    // GET /api/knowledge/articles/by-product/{productId}
    // =========================================================================

    /// <summary>Get published articles linked to a specific product.</summary>
    [HttpGet("articles/by-product/{productId:int}")]
    [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct = default)
    {
        var articles = await _service.GetByProductAsync(productId, ct);
        return Ok(articles);
    }

    // =========================================================================
    // POST /api/knowledge/articles/{id}/case-deflection
    // =========================================================================

    /// <summary>Track that an article was viewed instead of creating a ticket (case deflection).</summary>
    [HttpPost("articles/{id:int}/case-deflection")]
    [AllowAnonymous] // NOSONAR - S4834: Deflection tracking must work for anonymous portal visitors
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TrackCaseDeflection(
        int id,
        [FromQuery] int? serviceRequestId = null,
        CancellationToken ct = default)
    {
        await _service.TrackCaseDeflectionAsync(id, serviceRequestId, ct);
        return NoContent();
    }

    // =========================================================================
    // GET /api/knowledge/search  — KB-012: Unified cross-source search
    // =========================================================================

    /// <summary>
    /// Unified search across General KB and ITSM KB articles.
    /// Supports filtering by source (All, General, ITSM).
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous] // NOSONAR - S4834: Public KB search accessible to portal users
    [ProducesResponseType(typeof(IEnumerable<UnifiedKnowledgeSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int maxResults = 20,
        [FromQuery] KnowledgeSource source = KnowledgeSource.All,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required.");
        }

        var results = await _unifiedSearchService.SearchAsync(query, maxResults, source, ct);
        return Ok(results);
    }

    // =========================================================================
    // POST /api/knowledge/search/reindex  — KB-015: Meilisearch re-index trigger
    // =========================================================================

    /// <summary>
    /// Triggers a full re-index of all published General KB and ITSM KB articles
    /// into the Meilisearch <c>knowledge_articles</c> index.
    /// When Meilisearch is not configured this is a lightweight no-op (counts only).
    /// Requires Admin role.
    /// KB-015.
    /// </summary>
    [HttpPost("search/reindex")]
    [Authorize(Roles = "Admin")] // KB-015: admin-only re-index trigger
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reindex(CancellationToken ct = default)
    {
        await _unifiedSearchService.IndexAllAsync(ct);
        return Ok(new { message = "Knowledge index rebuild triggered successfully." });
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 1; // NOSONAR
    }
}
