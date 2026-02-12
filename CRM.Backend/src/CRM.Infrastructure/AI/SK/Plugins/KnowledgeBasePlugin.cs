// -----------------------------------------------------------------------
// CRM Solution - Semantic Kernel AI Plugins
// Copyright (c) 2024-2026 Abhishek Lal (CRM Solution). All rights reserved.
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.
//
// This file is part of the CRM Solution, an enterprise-grade
// Customer Relationship Management system.
//
// Author: Abhishek Lal
// Repository: https://github.com/abhisheklal04/crm-solution
// Documentation: See /docs folder for architecture and API reference
//
// IMPORTANT: This is proprietary code. Unauthorized copying, modification,
// or distribution is strictly prohibited.
// -----------------------------------------------------------------------

#nullable enable

using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for Knowledge Base article operations.
/// Provides AI-accessible functions for searching and retrieving knowledge articles.
/// </summary>
public class KnowledgeBasePlugin : CrmPluginBase
{
    private readonly ICrmDbContext _context;

    /// <inheritdoc />
    public override string PluginName => "KnowledgeBase";

    /// <inheritdoc />
    public override string Description => "Search and retrieve knowledge base articles — find how-to guides, FAQs, troubleshooting steps, and best practices.";

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeBasePlugin"/> class.
    /// </summary>
    /// <param name="context">The database context for querying knowledge articles.</param>
    /// <param name="logger">The logger instance.</param>
    public KnowledgeBasePlugin(
        ICrmDbContext context,
        ILogger<KnowledgeBasePlugin> logger) : base(logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Read Operations

    /// <summary>
    /// Searches knowledge base articles by keyword in title, short description, or body.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <param name="maxResults">Maximum number of results to return. Defaults to 10.</param>
    /// <returns>A JSON array of matching knowledge article summaries.</returns>
    [KernelFunction("SearchArticles")]
    [Description("Search knowledge base articles by keyword in title, description, or body content.")]
    public async Task<string> SearchArticlesAsync(
        [Description("Keyword to search for in articles")] string keyword,
        [Description("Maximum number of results to return")] int maxResults = 10)
    {
        try
        {
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
