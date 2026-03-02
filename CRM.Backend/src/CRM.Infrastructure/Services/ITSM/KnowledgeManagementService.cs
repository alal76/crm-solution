// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

public class KnowledgeManagementService : IKnowledgeManagementService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<KnowledgeManagementService> _logger;

    public KnowledgeManagementService(IDbContextResolver dbContextResolver, ILogger<KnowledgeManagementService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<KnowledgeArticleDto> CreateArticleAsync(CreateKnowledgeArticleDto dto, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = new KnowledgeArticle
        {
            Number = await GenerateArticleNumberAsync(context),
            Title = dto.Title,
            ShortDescription = dto.ShortDescription,
            ArticleBody = dto.ArticleBody,
            ArticleType = dto.ArticleType,
            CategoryId = dto.CategoryId,
            IsInternal = dto.IsInternal,
            IsExternal = !dto.IsInternal,
            IsPublic = false,
            PublishingState = PublishingState.Draft,
            AuthorId = createdById,
            OwnerId = createdById,
            CreatedAt = DateTime.UtcNow
        };

        context.ITSMKnowledgeArticles.Add(article);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created knowledge article {ArticleNumber}", article.Number);
        return MapToDto(article);
    }

    public async Task<KnowledgeArticleDto?> GetArticleByIdAsync(int articleId)
    {
        var context = _dbContextResolver.ResolveContext();
        var article = await context.ITSMKnowledgeArticles
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && !a.IsDeleted);

        if (article != null)
        {
            article.ViewCount++;
            await context.SaveChangesAsync();
        }

        return article == null ? null : MapToDto(article);
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> SearchArticlesAsync(string searchTerm, int pageNumber, int pageSize)
    {
        var context = _dbContextResolver.ResolveContext();
        var query = context.ITSMKnowledgeArticles
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(a => a.Title.Contains(searchTerm) ||
                                    (a.ShortDescription != null && a.ShortDescription.Contains(searchTerm)) ||
                                    a.ArticleBody.Contains(searchTerm));
        }

        var articles = await query
            .OrderByDescending(a => a.ViewCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return articles.Select(MapToDto);
    }

    public async Task<KnowledgeArticleDto> UpdateArticleAsync(int articleId, CreateKnowledgeArticleDto dto, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();
        var article = await context.ITSMKnowledgeArticles.FindAsync(articleId);

        if (article == null || article.IsDeleted)
        {
            throw new KeyNotFoundException($"Article {articleId} not found");
        }

        article.Title = dto.Title;
        article.ShortDescription = dto.ShortDescription;
        article.ArticleBody = dto.ArticleBody;
        article.ArticleType = dto.ArticleType;
        article.CategoryId = dto.CategoryId;
        article.IsInternal = dto.IsInternal;
        article.IsExternal = !dto.IsInternal;
        article.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return MapToDto(article);
    }

    public async Task<bool> PublishArticleAsync(int articleId, int publisherId)
    {
        var context = _dbContextResolver.ResolveContext();
        var article = await context.ITSMKnowledgeArticles.FindAsync(articleId);

        if (article == null || article.IsDeleted)
        {
            return false;
        }

        article.PublishingState = PublishingState.Published;
        article.PublishedById = publisherId;
        article.PublishedDate = DateTime.UtcNow;
        article.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        _logger.LogInformation("Published article {ArticleNumber}", article.Number);

        return true;
    }

    public async Task<bool> RetireArticleAsync(int articleId, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();
        var article = await context.ITSMKnowledgeArticles.FindAsync(articleId);

        if (article == null || article.IsDeleted)
        {
            return false;
        }

        article.PublishingState = PublishingState.Retired;
        article.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        _logger.LogInformation("Retired article {ArticleNumber}", article.Number);

        return true;
    }

    public async Task<bool> SubmitFeedbackAsync(int articleId, int? userId, bool isHelpful, string? comment)
    {
        var context = _dbContextResolver.ResolveContext();

        var feedback = new ArticleFeedback
        {
            ArticleId = articleId,
            UserId = userId,
            IsHelpful = isHelpful,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        context.ITSMArticleFeedback.Add(feedback);

        var article = await context.ITSMKnowledgeArticles.FindAsync(articleId);
        if (article != null)
        {
            if (isHelpful)
            {
                article.HelpfulCount++;
            }
            else
            {
                article.NotHelpfulCount++;
            }
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetSuggestedArticlesAsync(string incidentDescription)
    {
        var context = _dbContextResolver.ResolveContext();

        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "is", "a", "an", "to", "for", "of", "in", "on", "at",
            "and", "or", "not", "it", "this", "that", "with", "from", "by",
            "are", "was", "were", "be", "been", "has", "have", "had", "do",
            "does", "did", "will", "would", "could", "should", "can", "may",
            "my", "our", "your", "its", "but", "if", "so", "no", "we", "i"
        };

        var keywords = (incidentDescription?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
            .Where(k => k.Length > 2 && !stopWords.Contains(k))
            .Select(k => k.ToLowerInvariant())
            .Distinct()
            .ToArray();

        if (keywords.Length == 0)
        {
            // No meaningful keywords — fall back to top 5 most viewed published articles
            var fallbackArticles = await context.ITSMKnowledgeArticles
                .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
                .OrderByDescending(a => a.ViewCount)
                .Take(5)
                .ToListAsync();

            return fallbackArticles.Select(MapToDto);
        }

        // Fetch candidate articles matching ANY keyword in Title, ShortDescription, or ArticleBody
        var candidates = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .Where(a => keywords.Any(kw =>
                a.Title.ToLower().Contains(kw) ||
                (a.ShortDescription != null && a.ShortDescription.ToLower().Contains(kw)) ||
                a.ArticleBody.ToLower().Contains(kw)))
            .OrderByDescending(a => a.ViewCount)
            .Take(10)
            .ToListAsync();

        // Score by counting how many distinct keywords match, then take top 5
        var scored = candidates
            .Select(a => new
            {
                Article = a,
                Score = keywords.Count(kw =>
                    a.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    (a.ShortDescription != null && a.ShortDescription.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    a.ArticleBody.Contains(kw, StringComparison.OrdinalIgnoreCase))
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Article.ViewCount)
            .Take(5)
            .Select(x => x.Article);

        return scored.Select(MapToDto);
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetPopularArticlesAsync(int count)
    {
        var context = _dbContextResolver.ResolveContext();

        var articles = await context.ITSMKnowledgeArticles
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .OrderByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.HelpfulCount)
            .Take(count)
            .ToListAsync();

        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetRecentArticlesAsync(int count)
    {
        var context = _dbContextResolver.ResolveContext();

        var articles = await context.ITSMKnowledgeArticles
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .OrderByDescending(a => a.PublishedDate ?? a.CreatedAt)
            .Take(count)
            .ToListAsync();

        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var context = _dbContextResolver.ResolveContext();

        var categories = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .Select(a => a.CategoryId.ToString())
            .Distinct()
            .ToListAsync();

        // Return predefined categories (could be loaded from a separate categories table)
        return new[]
        {
            "How-To",
            "Troubleshooting",
            "FAQ",
            "Reference",
            "Best Practices",
            "Known Issues",
            "Release Notes"
        };
    }

    private async Task<string> GenerateArticleNumberAsync(ICrmDbContext context)
    {
        var lastArticle = await context.ITSMKnowledgeArticles
            .OrderByDescending(a => a.ArticleId)
            .FirstOrDefaultAsync();

        var nextNumber = lastArticle != null ? lastArticle.ArticleId + 1 : 1;
        return $"KB{nextNumber:D7}";
    }

    private KnowledgeArticleDto MapToDto(KnowledgeArticle article)
    {
        return new KnowledgeArticleDto
        {
            ArticleId = article.ArticleId,
            Number = article.Number,
            Title = article.Title,
            ShortDescription = article.ShortDescription,
            ArticleBody = article.ArticleBody,
            ArticleType = article.ArticleType,
            PublishingState = article.PublishingState,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.Username,
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            NotHelpfulCount = article.NotHelpfulCount,
            PublishedDate = article.PublishedDate
        };
    }

    /// <summary>
    /// Get version history for a knowledge article.
    /// </summary>
    public async Task<IEnumerable<ArticleVersionDto>> GetArticleVersionsAsync(int articleId)
    {
        var context = _dbContextResolver.ResolveContext();

        var versions = await context.ArticleVersions
            .Include(v => v.ChangedBy)
            .Where(v => v.ArticleId == articleId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} version(s) for article {ArticleId}", versions.Count, articleId);

        return versions.Select(v => new ArticleVersionDto
        {
            Id = v.Id,
            ArticleId = v.ArticleId,
            VersionNumber = v.VersionNumber,
            Title = v.Title,
            Content = v.Content,
            ChangedById = v.ChangedById,
            ChangedByName = v.ChangedBy?.Username,
            ChangedAt = v.ChangedAt,
            ChangeNote = v.ChangeNote
        });
    }

    /// <summary>
    /// Restore a previous version of a knowledge article.
    /// </summary>
    public async Task<KnowledgeArticleDto?> RestoreArticleVersionAsync(int articleId, int versionId, int modifiedById)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && !a.IsDeleted);

        if (article == null)
        {
            return null;
        }

        var version = await context.Set<ArticleVersion>()
            .FirstOrDefaultAsync(v => v.Id == versionId && v.ArticleId == articleId);

        if (version == null)
        {
            return null;
        }

        // Save current state as a new version before restoring
        var currentVersion = new ArticleVersion
        {
            ArticleId = articleId,
            VersionNumber = article.Version + 1,
            Title = article.Title,
            Content = article.ArticleBody,
            ChangedById = modifiedById,
            ChangedAt = DateTime.UtcNow,
            ChangeNote = $"Before restore to v{version.VersionNumber}"
        };
        context.Set<ArticleVersion>().Add(currentVersion);

        // Restore the selected version
        article.Title = version.Title;
        article.ArticleBody = version.Content;
        article.Version = article.Version + 2;
        article.ModifiedAt = DateTime.UtcNow;

        // Create a new version entry for the restored content
        var restoredVersion = new ArticleVersion
        {
            ArticleId = articleId,
            VersionNumber = article.Version,
            Title = version.Title,
            Content = version.Content,
            ChangedById = modifiedById,
            ChangedAt = DateTime.UtcNow,
            ChangeNote = $"Restored from v{version.VersionNumber}"
        };
        context.Set<ArticleVersion>().Add(restoredVersion);

        await context.SaveChangesAsync();

        _logger.LogInformation("Restored article {ArticleId} to version {VersionNumber}", articleId, version.VersionNumber);
        return MapToDto(article);
    }
}
