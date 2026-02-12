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
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Ports.Output.Providers;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for global CRM search operations.
/// Provides AI-accessible functions for unified cross-entity search via the pluggable search provider.
/// </summary>
public class SearchPlugin : CrmPluginBase
{
    private readonly ISearchPort _searchPort;

    /// <inheritdoc />
    public override string PluginName => "Search";

    /// <inheritdoc />
    public override string Description => "Perform global search across all CRM entities — accounts, contacts, leads, opportunities, products, and knowledge articles.";

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchPlugin"/> class.
    /// </summary>
    /// <param name="searchPort">The search port for unified search operations.</param>
    /// <param name="logger">The logger instance.</param>
    public SearchPlugin(
        ISearchPort searchPort,
        ILogger<SearchPlugin> logger) : base(logger)
    {
        _searchPort = searchPort ?? throw new ArgumentNullException(nameof(searchPort));
    }

    #region Read Operations

    /// <summary>
    /// Performs a global search across all CRM entities.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="maxResults">Maximum number of results to return. Defaults to 20.</param>
    /// <returns>A JSON object containing search hits, total count, and processing time.</returns>
    [KernelFunction("GlobalSearch")]
    [Description("Search across all CRM entities (accounts, contacts, leads, opportunities, products, knowledge articles) with a single query.")]
    public async Task<string> GlobalSearchAsync(
        [Description("The search query string")] string query,
        [Description("Maximum number of results to return")] int maxResults = 20)
    {
        try
        {
            var request = new SearchRequest
            {
                Query = query,
                Take = maxResults
            };

            var result = await _searchPort.SearchAsync(request);

            return SuccessResult(new
            {
                query = result.Query,
                totalCount = result.TotalCount,
                processingTimeMs = result.ProcessingTimeMs,
                hits = result.Hits.Select(h => new
                {
                    h.EntityType,
                    h.Id,
                    h.Title,
                    h.Description,
                    h.Score,
                    h.Highlights
                })
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("GlobalSearch", ex.Message);
        }
    }

    /// <summary>
    /// Searches for entities of a specific type.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="entityType">The entity type to filter by (e.g., "Account", "Contact", "Lead", "Opportunity", "Product").</param>
    /// <param name="maxResults">Maximum number of results to return. Defaults to 20.</param>
    /// <returns>A JSON object containing filtered search hits.</returns>
    [KernelFunction("SearchByType")]
    [Description("Search for a specific entity type (Account, Contact, Lead, Opportunity, Product, KnowledgeArticle).")]
    public async Task<string> SearchByTypeAsync(
        [Description("The search query string")] string query,
        [Description("Entity type to filter by: Account, Contact, Lead, Opportunity, Product, KnowledgeArticle")] string entityType,
        [Description("Maximum number of results to return")] int maxResults = 20)
    {
        try
        {
            var request = new SearchRequest
            {
                Query = query,
                EntityType = entityType,
                Take = maxResults
            };

            var result = await _searchPort.SearchAsync(request);

            return SuccessResult(new
            {
                query = result.Query,
                entityType,
                totalCount = result.TotalCount,
                processingTimeMs = result.ProcessingTimeMs,
                hits = result.Hits.Select(h => new
                {
                    h.EntityType,
                    h.Id,
                    h.Title,
                    h.Description,
                    h.Score,
                    h.Highlights
                })
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("SearchByType", ex.Message);
        }
    }

    #endregion
}
