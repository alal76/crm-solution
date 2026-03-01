// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the Source-Available License (see LICENSE) as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Security.Claims;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.ServiceDeskService.Controllers;

/// <summary>
/// API controller for managing knowledge base articles and categories.
/// </summary>
[ApiController]
[Route("api/knowledge")]
[Authorize]
public class KnowledgeBaseController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(CrmDbContext context, ILogger<KnowledgeBaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #region Articles CRUD

    /// <summary>
    /// Get all articles with pagination and filtering
    /// </summary>
    [HttpGet("articles")]
    public async Task<IActionResult> GetArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? status = null,
        [FromQuery] string? visibility = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? articleType = null)
    {
        try
        {
            var query = _context.KnowledgeArticles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Where(a => !a.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ArticleStatus>(status, true, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(visibility) && Enum.TryParse<ArticleVisibility>(visibility, true, out var visibilityEnum))
            {
                query = query.Where(a => a.Visibility == visibilityEnum);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(articleType) && Enum.TryParse<ArticleType>(articleType, true, out var typeEnum))
            {
                query = query.Where(a => a.ArticleType == typeEnum);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchLower) ||
                    (a.Summary != null && a.Summary.ToLower().Contains(searchLower)) ||
                    (a.Keywords != null && a.Keywords.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync();
            var articles = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.ArticleNumber,
                    a.Title,
                    a.Slug,
                    a.Summary,
                    Status = a.Status.ToString(),
                    StatusValue = (int)a.Status,
                    Visibility = a.Visibility.ToString(),
                    VisibilityValue = (int)a.Visibility,
                    ArticleType = a.ArticleType.ToString(),
                    ArticleTypeValue = (int)a.ArticleType,
                    a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : null,
                    a.AuthorUserId,
                    AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : null,
                    a.ViewCount,
                    a.HelpfulCount,
                    a.NotHelpfulCount,
                    a.HelpfulnessScore,
                    a.AverageRating,
                    a.RatingCount,
                    a.PublishedAt,
                    a.ExpiresAt,
                    a.Version,
                    a.LanguageCode,
                    a.TagsJson,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = articles,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge articles");
            return StatusCode(500, "An error occurred while retrieving articles");
        }
    }

    /// <summary>
    /// Get article by ID
    /// </summary>
    [HttpGet("articles/{id}")]
    public async Task<IActionResult> GetArticle(int id)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.LastUpdatedByUser)
                .Include(a => a.ApprovedByUser)
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    a.ArticleNumber,
                    a.Title,
                    a.Slug,
                    a.Summary,
                    a.Content,
                    a.ContentFormat,
                    a.PlainTextContent,
                    Status = a.Status.ToString(),
                    StatusValue = (int)a.Status,
                    Visibility = a.Visibility.ToString(),
                    VisibilityValue = (int)a.Visibility,
                    ArticleType = a.ArticleType.ToString(),
                    ArticleTypeValue = (int)a.ArticleType,
                    a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : null,
                    a.AuthorUserId,
                    AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : null,
                    a.LastUpdatedByUserId,
                    LastUpdatedByName = a.LastUpdatedByUser != null ? $"{a.LastUpdatedByUser.FirstName} {a.LastUpdatedByUser.LastName}" : null,
                    a.ApprovedByUserId,
                    ApprovedByName = a.ApprovedByUser != null ? $"{a.ApprovedByUser.FirstName} {a.ApprovedByUser.LastName}" : null,
                    a.ViewCount,
                    a.UniqueVisitorCount,
                    a.HelpfulCount,
                    a.NotHelpfulCount,
                    a.HelpfulnessScore,
                    a.AverageRating,
                    a.RatingCount,
                    a.CaseDeflectionCount,
                    a.SearchImpressionCount,
                    a.SearchClickCount,
                    a.AvgTimeOnPageSeconds,
                    a.PublishedAt,
                    a.ExpiresAt,
                    a.ReviewDate,
                    a.Version,
                    a.LanguageCode,
                    a.ParentArticleId,
                    a.TagsJson,
                    a.Keywords,
                    a.ProductsJson,
                    a.TableOfContentsJson,
                    a.AttachmentsJson,
                    a.VideoUrl,
                    a.MetaTitle,
                    a.MetaDescription,
                    a.CanonicalUrl,
                    a.AISummary,
                    a.RelatedArticleIdsJson,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (article == null)
                return NotFound(new { message = $"Article with ID {id} not found" });

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article {ArticleId}", id);
            return StatusCode(500, "An error occurred while retrieving the article");
        }
    }

    /// <summary>
    /// Get article by slug (public)
    /// </summary>
    [HttpGet("articles/slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetArticleBySlug(string slug)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Where(a => a.Slug == slug && a.Status == ArticleStatus.Published && !a.IsDeleted)
                .Select(a => new
                {
                    a.Id,
                    a.ArticleNumber,
                    a.Title,
                    a.Slug,
                    a.Summary,
                    a.Content,
                    a.ContentFormat,
                    ArticleType = a.ArticleType.ToString(),
                    a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : null,
                    AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : null,
                    a.ViewCount,
                    a.HelpfulCount,
                    a.NotHelpfulCount,
                    a.AverageRating,
                    a.PublishedAt,
                    a.Version,
                    a.TagsJson,
                    a.VideoUrl,
                    a.TableOfContentsJson,
                    a.AttachmentsJson,
                    a.RelatedArticleIdsJson
                })
                .FirstOrDefaultAsync();

            if (article == null)
                return NotFound(new { message = "Article not found" });

            // Increment view count
            await _context.KnowledgeArticles
                .Where(a => a.Id == article.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article by slug {Slug}", slug);
            return StatusCode(500, "An error occurred while retrieving the article");
        }
    }

    /// <summary>
    /// Create a new article
    /// </summary>
    [HttpPost("articles")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var article = new KnowledgeArticle
            {
                ArticleNumber = $"KB{DateTime.UtcNow:yyyyMMddHHmmss}",
                Title = request.Title,
                Slug = GenerateSlug(request.Slug ?? request.Title),
                Summary = request.Summary,
                Content = request.Content,
                ContentFormat = request.ContentFormat ?? "html",
                Status = Enum.TryParse<ArticleStatus>(request.Status, true, out var status) ? status : ArticleStatus.Draft,
                Visibility = Enum.TryParse<ArticleVisibility>(request.Visibility, true, out var visibility) ? visibility : ArticleVisibility.Internal,
                ArticleType = Enum.TryParse<ArticleType>(request.ArticleType, true, out var type) ? type : ArticleType.HowTo,
                CategoryId = request.CategoryId,
                AuthorUserId = userId.Value,
                TagsJson = request.Tags,
                Keywords = request.Keywords,
                ProductsJson = request.Products,
                VideoUrl = request.VideoUrl,
                MetaTitle = request.MetaTitle,
                MetaDescription = request.MetaDescription,
                LanguageCode = request.LanguageCode ?? "en",
                ExpiresAt = request.ExpiresAt,
                ReviewDate = request.ReviewDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (article.Status == ArticleStatus.Published)
            {
                article.PublishedAt = DateTime.UtcNow;
            }

            _context.KnowledgeArticles.Add(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created knowledge article {ArticleId} - {Title}", article.Id, article.Title);
            return CreatedAtAction(nameof(GetArticle), new { id = article.Id },
                new { article.Id, article.ArticleNumber, article.Title, article.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge article");
            return StatusCode(500, "An error occurred while creating the article");
        }
    }

    /// <summary>
    /// Update an article
    /// </summary>
    [HttpPut("articles/{id}")]
    public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleRequest request)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (article == null)
                return NotFound(new { message = $"Article with ID {id} not found" });

            var userId = GetCurrentUserId();

            // Update fields
            if (request.Title != null) article.Title = request.Title;
            if (request.Slug != null) article.Slug = GenerateSlug(request.Slug);
            if (request.Summary != null) article.Summary = request.Summary;
            if (request.Content != null) article.Content = request.Content;
            if (request.ContentFormat != null) article.ContentFormat = request.ContentFormat;

            if (request.Status != null && Enum.TryParse<ArticleStatus>(request.Status, true, out var status))
            {
                if (status == ArticleStatus.Published && article.Status != ArticleStatus.Published)
                {
                    article.PublishedAt = DateTime.UtcNow;
                }
                article.Status = status;
            }

            if (request.Visibility != null && Enum.TryParse<ArticleVisibility>(request.Visibility, true, out var visibility))
                article.Visibility = visibility;

            if (request.ArticleType != null && Enum.TryParse<ArticleType>(request.ArticleType, true, out var type))
                article.ArticleType = type;

            if (request.CategoryId.HasValue) article.CategoryId = request.CategoryId;
            if (request.Tags != null) article.TagsJson = request.Tags;
            if (request.Keywords != null) article.Keywords = request.Keywords;
            if (request.Products != null) article.ProductsJson = request.Products;
            if (request.VideoUrl != null) article.VideoUrl = request.VideoUrl;
            if (request.MetaTitle != null) article.MetaTitle = request.MetaTitle;
            if (request.MetaDescription != null) article.MetaDescription = request.MetaDescription;
            if (request.LanguageCode != null) article.LanguageCode = request.LanguageCode;
            if (request.ExpiresAt.HasValue) article.ExpiresAt = request.ExpiresAt;
            if (request.ReviewDate.HasValue) article.ReviewDate = request.ReviewDate;

            article.LastUpdatedByUserId = userId;
            article.Version++;
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated knowledge article {ArticleId}", id);
            return Ok(new { message = "Article updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return StatusCode(500, "An error occurred while updating the article");
        }
    }

    /// <summary>
    /// Delete an article (soft delete)
    /// </summary>
    [HttpDelete("articles/{id}")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (article == null)
                return NotFound(new { message = $"Article with ID {id} not found" });

            article.IsDeleted = true;
            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted knowledge article {ArticleId}", id);
            return Ok(new { message = "Article deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return StatusCode(500, "An error occurred while deleting the article");
        }
    }

    #endregion

    #region Article Actions

    /// <summary>
    /// Publish an article
    /// </summary>
    [HttpPost("articles/{id}/publish")]
    public async Task<IActionResult> PublishArticle(int id)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (article == null)
                return NotFound(new { message = $"Article with ID {id} not found" });

            article.Status = ArticleStatus.Published;
            article.PublishedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Published knowledge article {ArticleId}", id);
            return Ok(new { message = "Article published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing article {ArticleId}", id);
            return StatusCode(500, "An error occurred while publishing the article");
        }
    }

    /// <summary>
    /// Archive an article
    /// </summary>
    [HttpPost("articles/{id}/archive")]
    public async Task<IActionResult> ArchiveArticle(int id)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (article == null)
                return NotFound(new { message = $"Article with ID {id} not found" });

            article.Status = ArticleStatus.Archived;
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Archived knowledge article {ArticleId}", id);
            return Ok(new { message = "Article archived successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving article {ArticleId}", id);
            return StatusCode(500, "An error occurred while archiving the article");
        }
    }

    /// <summary>
    /// Record helpful/not helpful feedback
    /// </summary>
    [HttpPost("articles/{id}/feedback")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitFeedback(int id, [FromBody] ArticleFeedbackRequest request)
    {
        try
        {
            var article = await _context.KnowledgeArticles
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (article == null)
                return NotFound(new { message = $"Article with ID {id} not found" });

            if (request.IsHelpful)
                article.HelpfulCount++;
            else
                article.NotHelpfulCount++;

            if (request.Rating.HasValue)
            {
                var totalRating = (article.AverageRating ?? 0) * article.RatingCount;
                article.RatingCount++;
                article.AverageRating = (totalRating + request.Rating.Value) / article.RatingCount;
            }

            // Save feedback record
            var feedback = new ArticleFeedback
            {
                KnowledgeArticleId = id,
                IsHelpful = request.IsHelpful,
                Rating = request.Rating,
                Comment = request.Comment,
                UserId = GetCurrentUserId(),
                SessionId = request.SessionId,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<ArticleFeedback>().Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback recorded. Thank you!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording feedback for article {ArticleId}", id);
            return StatusCode(500, "An error occurred while recording feedback");
        }
    }

    #endregion

    #region Categories

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _context.KnowledgeCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Slug,
                    c.Icon,
                    c.DisplayOrder,
                    c.ParentCategoryId,
                    ArticleCount = c.Articles.Count(a => a.Status == ArticleStatus.Published && !a.IsDeleted)
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            var category = await _context.KnowledgeCategories
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Slug,
                    c.Icon,
                    c.DisplayOrder,
                    c.IsActive,
                    c.ParentCategoryId,
                    ArticleCount = c.Articles.Count(a => a.Status == ArticleStatus.Published && !a.IsDeleted)
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    /// <summary>
    /// Create a category
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var maxOrder = await _context.KnowledgeCategories
                .Where(c => !c.IsDeleted)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

            var category = new KnowledgeCategory
            {
                Name = request.Name,
                Description = request.Description,
                Slug = GenerateSlug(request.Slug ?? request.Name),
                Icon = request.Icon,
                DisplayOrder = request.DisplayOrder ?? maxOrder + 1,
                IsActive = request.IsActive,
                ParentCategoryId = request.ParentCategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.KnowledgeCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created knowledge category {CategoryId} - {Name}", category.Id, category.Name);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
                new { category.Id, category.Name, category.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge category");
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Update a category
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _context.KnowledgeCategories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found" });

            if (request.Name != null) category.Name = request.Name;
            if (request.Description != null) category.Description = request.Description;
            if (request.Slug != null) category.Slug = GenerateSlug(request.Slug);
            if (request.Icon != null) category.Icon = request.Icon;
            if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;
            category.IsActive = request.IsActive ?? category.IsActive;
            if (request.ParentCategoryId.HasValue) category.ParentCategoryId = request.ParentCategoryId;

            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated knowledge category {CategoryId}", id);
            return Ok(new { message = "Category updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the category");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var category = await _context.KnowledgeCategories
                .Include(c => c.Articles)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found" });

            // Check if category has articles
            if (category.Articles.Any(a => !a.IsDeleted))
                return BadRequest(new { message = "Cannot delete category with articles. Move or delete articles first." });

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted knowledge category {CategoryId}", id);
            return Ok(new { message = "Category deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the category");
        }
    }

    #endregion

    #region Search

    /// <summary>
    /// Search articles (public)
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchArticles([FromQuery] string query, [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            var searchLower = query.ToLower();
            var articles = await _context.KnowledgeArticles
                .Include(a => a.Category)
                .Where(a => a.Status == ArticleStatus.Published &&
                           a.Visibility == ArticleVisibility.Public &&
                           !a.IsDeleted &&
                           (a.Title.ToLower().Contains(searchLower) ||
                            (a.Summary != null && a.Summary.ToLower().Contains(searchLower)) ||
                            (a.Keywords != null && a.Keywords.ToLower().Contains(searchLower)) ||
                            (a.PlainTextContent != null && a.PlainTextContent.ToLower().Contains(searchLower))))
                .OrderByDescending(a => a.HelpfulCount)
                .ThenByDescending(a => a.ViewCount)
                .Take(limit)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Slug,
                    a.Summary,
                    CategoryName = a.Category != null ? a.Category.Name : null,
                    a.ViewCount,
                    a.HelpfulCount
                })
                .ToListAsync();

            // Record search impressions
            var articleIds = articles.Select(a => a.Id).ToList();
            await _context.KnowledgeArticles
                .Where(a => articleIds.Contains(a.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.SearchImpressionCount, a => a.SearchImpressionCount + 1));

            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching articles");
            return StatusCode(500, "An error occurred while searching");
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get knowledge base statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _context.KnowledgeArticles
                .Where(a => !a.IsDeleted)
                .GroupBy(a => 1)
                .Select(g => new
                {
                    TotalArticles = g.Count(),
                    PublishedArticles = g.Count(a => a.Status == ArticleStatus.Published),
                    DraftArticles = g.Count(a => a.Status == ArticleStatus.Draft),
                    TotalViews = g.Sum(a => a.ViewCount),
                    TotalHelpfulVotes = g.Sum(a => a.HelpfulCount),
                    TotalNotHelpfulVotes = g.Sum(a => a.NotHelpfulCount),
                    AverageHelpfulnessScore = g.Average(a => a.HelpfulnessScore),
                    ArticlesNeedingReview = g.Count(a => a.Status == ArticleStatus.NeedsUpdate),
                    ExpiredArticles = g.Count(a => a.ExpiresAt.HasValue && a.ExpiresAt < DateTime.UtcNow)
                })
                .FirstOrDefaultAsync();

            var categoryCount = await _context.KnowledgeCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .CountAsync();

            return Ok(new
            {
                TotalArticles = stats?.TotalArticles ?? 0,
                PublishedArticles = stats?.PublishedArticles ?? 0,
                DraftArticles = stats?.DraftArticles ?? 0,
                TotalViews = stats?.TotalViews ?? 0,
                TotalHelpfulVotes = stats?.TotalHelpfulVotes ?? 0,
                TotalNotHelpfulVotes = stats?.TotalNotHelpfulVotes ?? 0,
                AverageHelpfulnessScore = stats?.AverageHelpfulnessScore ?? 0,
                ArticlesNeedingReview = stats?.ArticlesNeedingReview ?? 0,
                ExpiredArticles = stats?.ExpiredArticles ?? 0,
                TotalCategories = categoryCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge base statistics");
            return StatusCode(500, "An error occurred while retrieving statistics");
        }
    }

    #endregion

    #region Helper Methods

    private string GenerateSlug(string title)
    {
        var slug = title.ToLower()
            .Replace(" ", "-")
            .Replace("_", "-");
        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1));
        // Remove duplicate hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1));
        return slug.Trim('-');
    }

    #endregion
}

#region Request DTOs

public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ContentFormat { get; set; }
    public string? Status { get; set; }
    public string? Visibility { get; set; }
    public string? ArticleType { get; set; }
    public int? CategoryId { get; set; }
    public string? Tags { get; set; }
    public string? Keywords { get; set; }
    public string? Products { get; set; }
    public string? VideoUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? LanguageCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReviewDate { get; set; }
}

public class UpdateArticleRequest
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? ContentFormat { get; set; }
    public string? Status { get; set; }
    public string? Visibility { get; set; }
    public string? ArticleType { get; set; }
    public int? CategoryId { get; set; }
    public string? Tags { get; set; }
    public string? Keywords { get; set; }
    public string? Products { get; set; }
    public string? VideoUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? LanguageCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReviewDate { get; set; }
}

public class ArticleFeedbackRequest
{
    public bool IsHelpful { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public string? SessionId { get; set; }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public int? ParentCategoryId { get; set; }
}

#endregion
