// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Dtos.KnowledgeBase;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IKnowledgeBaseService"/> using EF Core.
/// Manages the general Knowledge Base module with a
/// Draft → InReview → Published → Archived state machine.
/// Uses <c>context.KnowledgeArticles</c> (KnowledgeBase namespace, not ITSM).
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(ICrmDbContext dbContext, ILogger<KnowledgeBaseService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    public async Task<PagedResultDto<KnowledgeBaseArticleDto>> GetAllAsync(
        int page, int pageSize, string? search, int? categoryId, string? status,
        CancellationToken ct = default)
    {
        var query = _dbContext.KnowledgeArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Title.Contains(term) ||
                (a.Summary != null && a.Summary.Contains(term)) ||
                a.Content.Contains(term) ||
                (a.Keywords != null && a.Keywords.Contains(term)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ArticleStatus>(status, ignoreCase: true, out var statusEnum))
        {
            query = query.Where(a => a.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResultDto<KnowledgeBaseArticleDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // =========================================================================
    // GetByIdAsync / GetBySlugAsync
    // =========================================================================

    public async Task<KnowledgeBaseArticleDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var article = await _dbContext.KnowledgeArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);

        if (article is null) return null;

        // Increment view count
        article.ViewCount++;
        article.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(article);
    }

    public async Task<KnowledgeBaseArticleDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var article = await _dbContext.KnowledgeArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == slug && !a.IsDeleted, ct);

        if (article is null) return null;

        // Increment view count
        article.ViewCount++;
        article.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(article);
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    public async Task<KnowledgeBaseArticleDto> CreateAsync(
        CreateKnowledgeBaseArticleDto dto, int authorId, CancellationToken ct = default)
    {
        var slug = await GenerateUniqueSlugAsync(dto.Slug ?? dto.Title, ct);

        var article = new KnowledgeArticle
        {
            ArticleNumber = await GenerateArticleNumberAsync(ct),
            Title = dto.Title,
            Summary = dto.Summary,
            Slug = slug,
            Content = dto.Content,
            ContentFormat = dto.ContentFormat,
            ArticleType = dto.ArticleType,
            Status = dto.Status,
            Visibility = dto.Visibility,
            CategoryId = dto.CategoryId,
            TagsJson = dto.Tags,
            Keywords = dto.Keywords,
            IsFeatured = dto.IsFeatured,
            ExpiresAt = dto.ExpiresAt,
            ReviewDate = dto.ReviewDate,
            LanguageCode = dto.LanguageCode,
            ProductsJson = dto.ProductsJson,
            AuthorUserId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.KnowledgeArticles.Add(article);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created knowledge-base article {Number} (id={Id})",
            article.ArticleNumber, article.Id);

        return MapToDto(article);
    }

    // =========================================================================
    // UpdateAsync
    // =========================================================================

    public async Task<KnowledgeBaseArticleDto> UpdateAsync(
        int id, UpdateKnowledgeBaseArticleDto dto, CancellationToken ct = default)
    {
        var article = await _dbContext.KnowledgeArticles.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Knowledge article {id} not found");

        if (article.IsDeleted)
            throw new KeyNotFoundException($"Knowledge article {id} not found");

        if (dto.Title is not null) article.Title = dto.Title;
        if (dto.Summary is not null) article.Summary = dto.Summary;
        if (dto.Content is not null) article.Content = dto.Content;
        if (dto.ContentFormat is not null) article.ContentFormat = dto.ContentFormat;
        if (dto.ArticleType.HasValue) article.ArticleType = dto.ArticleType.Value;
        if (dto.Status.HasValue) article.Status = dto.Status.Value;
        if (dto.Visibility.HasValue) article.Visibility = dto.Visibility.Value;
        if (dto.CategoryId.HasValue) article.CategoryId = dto.CategoryId.Value;
        if (dto.Tags is not null) article.TagsJson = dto.Tags;
        if (dto.Keywords is not null) article.Keywords = dto.Keywords;
        if (dto.IsFeatured.HasValue) article.IsFeatured = dto.IsFeatured.Value;
        if (dto.ExpiresAt.HasValue) article.ExpiresAt = dto.ExpiresAt.Value;
        if (dto.ReviewDate.HasValue) article.ReviewDate = dto.ReviewDate.Value;
        if (dto.ProductsJson is not null) article.ProductsJson = dto.ProductsJson;

        if (dto.Slug is not null && dto.Slug != article.Slug)
        {
            article.Slug = await GenerateUniqueSlugAsync(dto.Slug, ct, excludeId: id);
        }

        article.UpdatedAt = DateTime.UtcNow;
        article.Version++;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Updated knowledge-base article {Id}", id);

        return MapToDto(article);
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var article = await _dbContext.KnowledgeArticles.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Knowledge article {id} not found");

        article.IsDeleted = true;
        article.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted (soft) knowledge-base article {Id}", id);
    }

    // =========================================================================
    // State machine: Publish / Archive
    // =========================================================================

    public async Task<KnowledgeBaseArticleDto> PublishAsync(int id, CancellationToken ct = default)
    {
        var article = await RequireArticleAsync(id, ct);

        if (article.Status == ArticleStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived article. Restore it to Draft first.");

        article.Status = ArticleStatus.Published;
        article.PublishedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Published knowledge-base article {Id}", id);

        return MapToDto(article);
    }

    public async Task<KnowledgeBaseArticleDto> ArchiveAsync(int id, CancellationToken ct = default)
    {
        var article = await RequireArticleAsync(id, ct);

        article.Status = ArticleStatus.Archived;
        article.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Archived knowledge-base article {Id}", id);

        return MapToDto(article);
    }

    // =========================================================================
    // Feedback
    // =========================================================================

    public async Task SubmitFeedbackAsync(int id, KnowledgeBaseFeedbackDto feedback, CancellationToken ct = default)
    {
        var article = await RequireArticleAsync(id, ct);

        var fb = new ArticleFeedback
        {
            KnowledgeArticleId = id,
            IsHelpful = feedback.IsHelpful ?? (feedback.Rating >= 3),
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            UserId = feedback.UserId,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ArticleFeedbacks.Add(fb);

        // Update aggregate counters
        if (fb.IsHelpful)
            article.HelpfulCount++;
        else
            article.NotHelpfulCount++;

        if (feedback.Rating.HasValue)
        {
            var newTotal = article.AverageRating.GetValueOrDefault() * article.RatingCount + feedback.Rating.Value;
            article.RatingCount++;
            article.AverageRating = newTotal / article.RatingCount;
        }

        article.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
    }

    // =========================================================================
    // Categories
    // =========================================================================

    public async Task<IEnumerable<KnowledgeCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _dbContext.KnowledgeCategories
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        // Build article counts in one query
        var counts = await _dbContext.KnowledgeArticles
            .Where(a => !a.IsDeleted && a.CategoryId != null)
            .GroupBy(a => a.CategoryId)
            .Select(g => new { CategoryId = g.Key!.Value, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);

        return categories.Select(c => new KnowledgeCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            Icon = c.Icon,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ParentId = c.ParentCategoryId,
            ArticleCount = counts.TryGetValue(c.Id, out var cnt) ? cnt : 0
        });
    }

    public async Task<KnowledgeCategoryDto> CreateCategoryAsync(
        CreateKnowledgeCategoryDto dto, CancellationToken ct = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? SlugFrom(dto.Name) : SlugFrom(dto.Slug);

        var category = new KnowledgeCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Slug = slug,
            Icon = dto.Icon,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            ParentCategoryId = dto.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.KnowledgeCategories.Add(category);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created knowledge-base category {Name} (id={Id})", category.Name, category.Id);

        return new KnowledgeCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            Icon = category.Icon,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ParentId = category.ParentCategoryId,
            ArticleCount = 0
        };
    }

    public async Task<KnowledgeCategoryDto> UpdateCategoryAsync(
        int id, UpdateKnowledgeCategoryDto dto, CancellationToken ct = default)
    {
        var category = await _dbContext.KnowledgeCategories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Knowledge category {id} not found");

        if (category.IsDeleted)
            throw new KeyNotFoundException($"Knowledge category {id} not found");

        if (dto.Name is not null) category.Name = dto.Name;
        if (dto.Description is not null) category.Description = dto.Description;
        if (dto.Slug is not null) category.Slug = SlugFrom(dto.Slug);
        if (dto.Icon is not null) category.Icon = dto.Icon;
        if (dto.DisplayOrder.HasValue) category.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsActive.HasValue) category.IsActive = dto.IsActive.Value;
        if (dto.ParentId.HasValue) category.ParentCategoryId = dto.ParentId.Value;

        category.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Updated knowledge-base category {Id}", id);

        return new KnowledgeCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            Icon = category.Icon,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ParentId = category.ParentCategoryId,
            ArticleCount = 0
        };
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        var category = await _dbContext.KnowledgeCategories.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Knowledge category {id} not found");

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted (soft) knowledge-base category {Id}", id);
    }

    // =========================================================================
    // Popular / Recent / By Product
    // =========================================================================

    public async Task<IEnumerable<KnowledgeBaseArticleDto>> GetPopularAsync(
        int count = 10, CancellationToken ct = default)
    {
        var articles = await _dbContext.KnowledgeArticles
            .Include(a => a.Category)
            .Where(a => !a.IsDeleted && a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.ViewCount)
            .Take(count)
            .ToListAsync(ct);

        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<KnowledgeBaseArticleDto>> GetRecentAsync(
        int count = 10, CancellationToken ct = default)
    {
        var articles = await _dbContext.KnowledgeArticles
            .Include(a => a.Category)
            .Where(a => !a.IsDeleted && a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<KnowledgeBaseArticleDto>> GetByProductAsync(
        int productId, CancellationToken ct = default)
    {
        var productIdStr = productId.ToString();
        var articles = await _dbContext.KnowledgeArticles
            .Include(a => a.Category)
            .Where(a => !a.IsDeleted &&
                        a.Status == ArticleStatus.Published &&
                        a.ProductsJson != null &&
                        a.ProductsJson.Contains(productIdStr))
            .OrderByDescending(a => a.ViewCount)
            .ToListAsync(ct);

        return articles.Select(MapToDto);
    }

    // =========================================================================
    // Case deflection tracking
    // =========================================================================

    public async Task TrackCaseDeflectionAsync(
        int articleId, int? serviceRequestId, CancellationToken ct = default)
    {
        var article = await _dbContext.KnowledgeArticles.FindAsync(new object[] { articleId }, ct);

        if (article is null || article.IsDeleted) return;

        article.CaseDeflectionCount++;
        article.UpdatedAt = DateTime.UtcNow;

        if (serviceRequestId.HasValue)
        {
            var link = new ServiceRequestArticle
            {
                ServiceRequestId = serviceRequestId.Value,
                KnowledgeArticleId = articleId,
                DeflectedCase = true,
                LinkedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.ServiceRequestArticles.Add(link);
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private async Task<KnowledgeArticle> RequireArticleAsync(int id, CancellationToken ct)
    {
        var article = await _dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);

        return article ?? throw new KeyNotFoundException($"Knowledge article {id} not found");
    }

    private async Task<string> GenerateArticleNumberAsync(CancellationToken ct)
    {
        var count = await _dbContext.KnowledgeArticles.CountAsync(ct);
        return $"KB{(count + 1):D7}";
    }

    /// <summary>
    /// Generates a unique URL slug. Appends a numeric suffix if there is a collision.
    /// </summary>
    private async Task<string> GenerateUniqueSlugAsync(
        string source, CancellationToken ct, int? excludeId = null)
    {
        var baseSlug = SlugFrom(source);
        var slug = baseSlug;
        var suffix = 2;

        while (true)
        {
            var query = _dbContext.KnowledgeArticles
                .Where(a => a.Slug == slug && !a.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            if (!await query.AnyAsync(ct))
                return slug;

            slug = $"{baseSlug}-{suffix++}";
        }
    }

    private static string SlugFrom(string text)
    {
        var slug = text.Trim().ToLowerInvariant();
        // Replace non-alphanumeric sequences with a hyphen
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = slug.Trim('-');
        return slug.Length > 200 ? slug[..200] : slug;
    }

    // =========================================================================
    // Mapping
    // =========================================================================

    private static KnowledgeBaseArticleDto MapToDto(KnowledgeArticle a) => new()
    {
        Id = a.Id,
        ArticleNumber = a.ArticleNumber,
        Title = a.Title,
        Summary = a.Summary,
        Slug = a.Slug,
        Content = a.Content,
        ContentFormat = a.ContentFormat,
        ArticleType = (int)a.ArticleType,
        Status = (int)a.Status,
        Visibility = (int)a.Visibility,
        CategoryId = a.CategoryId,
        CategoryName = a.Category?.Name,
        Tags = a.TagsJson,
        Keywords = a.Keywords,
        AuthorUserId = a.AuthorUserId,
        AuthorName = a.Author is not null ? $"{a.Author.FirstName} {a.Author.LastName}".Trim() : null,
        ViewCount = a.ViewCount,
        HelpfulCount = a.HelpfulCount,
        NotHelpfulCount = a.NotHelpfulCount,
        AverageRating = a.AverageRating,
        RatingCount = a.RatingCount,
        CaseDeflectionCount = a.CaseDeflectionCount,
        IsFeatured = a.IsFeatured,
        PublishedAt = a.PublishedAt,
        ExpiresAt = a.ExpiresAt,
        ReviewDate = a.ReviewDate,
        Version = a.Version,
        LanguageCode = a.LanguageCode,
        ProductsJson = a.ProductsJson,
        RelatedArticleIdsJson = a.RelatedArticleIdsJson,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
