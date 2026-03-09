// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.ComponentModel;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using KBArticle = CRM.Core.Entities.KnowledgeBase.KnowledgeArticle;
using KBArticleStatus = CRM.Core.Entities.KnowledgeBase.ArticleStatus;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for Knowledge Base article operations.
/// Provides AI-accessible functions for searching and retrieving knowledge articles
/// across both ITSM and General Knowledge Bases (KB-018).
/// </summary>
public class KnowledgeBasePlugin : CrmPluginBase
{
    private readonly ICrmDbContext _context;
    private readonly IUnifiedKnowledgeSearchService? _unifiedSearchService;

    /// <inheritdoc />
    public override string PluginName => "KnowledgeBase";

    /// <inheritdoc />
    public override string Description => "Search and retrieve knowledge base articles — find how-to guides, FAQs, troubleshooting steps, and best practices.";

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeBasePlugin"/> class.
    /// </summary>
    /// <param name="context">The database context for querying knowledge articles.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="unifiedSearchService">Optional unified search service for cross-KB queries (KB-018).</param>
    public KnowledgeBasePlugin(
        ICrmDbContext context,
        ILogger<KnowledgeBasePlugin> logger,
        IUnifiedKnowledgeSearchService? unifiedSearchService = null) : base(logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _unifiedSearchService = unifiedSearchService;
    }

    #region Read Operations

    /// <summary>
    /// Searches knowledge base articles by keyword across both ITSM and General KBs.
    /// When the unified search service is available, delegates to it for cross-KB results.
    /// Otherwise falls back to ITSM-only search.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <param name="maxResults">Maximum number of results to return. Defaults to 10.</param>
    /// <returns>A JSON array of matching knowledge article summaries.</returns>
    [KernelFunction("SearchArticles")]
    [Description("Search knowledge base articles by keyword in title, description, or body content. Searches both ITSM and General knowledge bases.")]
    public async Task<string> SearchArticlesAsync(
        [Description("Keyword to search for in articles")] string keyword,
        [Description("Maximum number of results to return")] int maxResults = 10)
    {
        try
        {
            if (_unifiedSearchService != null)
            {
                var results = await _unifiedSearchService.SearchAsync(keyword, maxResults, KnowledgeSource.All);
                var items = results.Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Summary,
                    Source = r.Source.ToString(),
                    r.Slug,
                    r.RelevanceScore,
                    r.ViewCount,
                    r.Category,
                    r.UpdatedAt
                }).ToList();
                return SuccessResult(new { totalFound = items.Count, articles = items });
            }

            // Fallback: ITSM-only search when unified service is not available
            var articles = await _context.ITSMKnowledgeArticles
                .Where(a => !a.IsDeleted
                    && a.PublishingState == PublishingState.Published
                    && (a.Title.Contains(keyword)
                        || (a.ShortDescription != null && a.ShortDescription.Contains(keyword))
                        || a.ArticleBody.Contains(keyword)))
                .OrderByDescending(a => a.ViewCount)
                .Take(maxResults)
                .Select(a => new
                {
                    a.ArticleId,
                    a.Number,
                    a.Title,
                    a.ShortDescription,
                    ArticleType = a.ArticleType.ToString(),
                    a.ViewCount,
                    a.HelpfulCount,
                    a.PublishedDate,
                    a.Tags
                })
                .ToListAsync();

            return SuccessResult(new { totalFound = articles.Count, articles });
        }
        catch (Exception ex)
        {
            return ErrorResult("SearchArticles", ex.Message);
        }
    }

    /// <summary>
    /// Searches General Knowledge Base articles by keyword in title, summary, or content.
    /// Only returns published, non-deleted articles.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <param name="maxResults">Maximum number of results to return. Defaults to 10.</param>
    /// <returns>A JSON array of matching General KB article summaries.</returns>
    [KernelFunction("SearchGeneralKBArticles")]
    [Description("Search General Knowledge Base articles by keyword. These are non-ITSM articles such as how-to guides, FAQs, and documentation.")]
    public async Task<string> SearchGeneralKBArticlesAsync(
        [Description("Keyword to search for in articles")] string keyword,
        [Description("Maximum number of results to return")] int maxResults = 10)
    {
        try
        {
            var articles = await _context.KnowledgeArticles
                .Where(a => !a.IsDeleted
                    && a.Status == KBArticleStatus.Published
                    && (a.Title.Contains(keyword)
                        || (a.Summary != null && a.Summary.Contains(keyword))
                        || a.Content.Contains(keyword)))
                .OrderByDescending(a => a.ViewCount)
                .Take(maxResults)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Summary,
                    a.Slug,
                    ArticleType = a.ArticleType.ToString(),
                    a.ViewCount,
                    a.PublishedAt
                })
                .ToListAsync();

            return SuccessResult(new { totalFound = articles.Count, articles });
        }
        catch (Exception ex)
        {
            return ErrorResult("SearchGeneralKBArticles", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a specific knowledge base article by its ID.
    /// </summary>
    /// <param name="articleId">The article ID to retrieve.</param>
    /// <returns>A JSON object with the full article details including body content.</returns>
    [KernelFunction("GetArticle")]
    [Description("Get a specific knowledge base article by its ID, including the full body content.")]
    public async Task<string> GetArticleAsync(
        [Description("The ID of the knowledge article to retrieve")] int articleId)
    {
        try
        {
            var article = await _context.ITSMKnowledgeArticles
                .Where(a => a.ArticleId == articleId && !a.IsDeleted)
                .Select(a => new
                {
                    a.ArticleId,
                    a.Number,
                    a.Title,
                    a.ShortDescription,
                    a.ArticleBody,
                    ArticleType = a.ArticleType.ToString(),
                    PublishingState = a.PublishingState.ToString(),
                    a.ViewCount,
                    a.HelpfulCount,
                    a.NotHelpfulCount,
                    a.Tags,
                    a.PublishedDate,
                    a.Version,
                    a.IsInternal,
                    a.IsExternal,
                    a.IsPublic,
                    a.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (article == null)
            {
                return SuccessResult(new { found = false, message = $"Knowledge article {articleId} not found" });
            }

            return SuccessResult(article);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetArticle", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the most popular knowledge base articles by view count.
    /// </summary>
    /// <param name="maxResults">Maximum number of results to return. Defaults to 10.</param>
    /// <returns>A JSON array of the most popular published articles.</returns>
    [KernelFunction("GetPopularArticles")]
    [Description("Get the most popular knowledge base articles ranked by view count.")]
    public async Task<string> GetPopularArticlesAsync(
        [Description("Maximum number of articles to return")] int maxResults = 10)
    {
        try
        {
            var articles = await _context.ITSMKnowledgeArticles
                .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
                .OrderByDescending(a => a.ViewCount)
                .Take(maxResults)
                .Select(a => new
                {
                    a.ArticleId,
                    a.Number,
                    a.Title,
                    a.ShortDescription,
                    ArticleType = a.ArticleType.ToString(),
                    a.ViewCount,
                    a.HelpfulCount,
                    a.Tags
                })
                .ToListAsync();

            return SuccessResult(articles);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetPopularArticles", ex.Message);
        }
    }

    #endregion
}
