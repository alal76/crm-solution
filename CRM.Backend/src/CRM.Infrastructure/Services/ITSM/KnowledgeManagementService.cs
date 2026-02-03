// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

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
            throw new KeyNotFoundException($"Article {articleId} not found");

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
            return false;

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
            return false;

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
                article.HelpfulCount++;
            else
                article.NotHelpfulCount++;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetSuggestedArticlesAsync(string incidentDescription)
    {
        // Simplified: return top 5 most viewed articles matching keywords
        // TODO: Implement AI-powered semantic search based on incident description
        var context = _dbContextResolver.ResolveContext();
        
        var keywords = incidentDescription?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        
        var articles = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .OrderByDescending(a => a.ViewCount)
            .Take(5)
            .ToListAsync();

        return articles.Select(MapToDto);
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
}
