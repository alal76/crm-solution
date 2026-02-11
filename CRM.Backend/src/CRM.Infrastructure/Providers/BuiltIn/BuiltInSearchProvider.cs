// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Providers.BuiltIn;

/// <summary>
/// Built-in search provider using Entity Framework Core and SQL-based search.
/// Preserves existing CRM search functionality while implementing ISearchPort.
/// </summary>
public class BuiltInSearchProvider : ISearchPort
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<BuiltInSearchProvider> _logger;

    public BuiltInSearchProvider(
        IDbContextResolver dbContextResolver,
        ILogger<BuiltInSearchProvider> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProviderName => "BuiltIn";

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _dbContextResolver.ResolveContext();
            return Task.FromResult(context != null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BuiltIn search provider availability check failed");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var hits = new List<SearchHit>();
        var query = request.Query?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            return new SearchResult
            {
                Query = request.Query ?? string.Empty,
                Hits = hits,
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var context = _dbContextResolver.ResolveContext();

        // Search across entity types based on filter or all
        var entityTypes = GetSearchableEntityTypes(request.EntityType);

        foreach (var entityType in entityTypes)
        {
            var entityHits = await SearchEntityTypeAsync(context, entityType, query, request, cancellationToken);
            hits.AddRange(entityHits);
        }

        // Sort by score and apply pagination
        var sortedHits = hits
            .OrderByDescending(h => h.Score)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        stopwatch.Stop();

        return new SearchResult
        {
            Query = request.Query ?? string.Empty,
            Hits = sortedHits,
            TotalCount = hits.Count,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <inheritdoc />
    public async Task<SearchResult<T>> SearchAsync<T>(string query, SearchOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        var normalizedQuery = query?.Trim().ToLowerInvariant() ?? string.Empty;
        var opts = options ?? new SearchOptions();

        if (string.IsNullOrEmpty(normalizedQuery) || normalizedQuery.Length < 2)
        {
            return new SearchResult<T>
            {
                Items = Enumerable.Empty<T>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var context = _dbContextResolver.ResolveContext();
        var entityType = typeof(T).Name;

        // Get the appropriate DbSet and search
        IEnumerable<T>? items = entityType switch
        {
            nameof(Account) => await SearchAccountsAsync(context, normalizedQuery, opts, cancellationToken) as IEnumerable<T>,
            nameof(Contact) => await SearchContactsAsync(context, normalizedQuery, opts, cancellationToken) as IEnumerable<T>,
            nameof(Opportunity) => await SearchOpportunitiesAsync(context, normalizedQuery, opts, cancellationToken) as IEnumerable<T>,
            nameof(Product) => await SearchProductsAsync(context, normalizedQuery, opts, cancellationToken) as IEnumerable<T>,
            nameof(KnowledgeArticle) => await SearchKnowledgeArticlesAsync(context, normalizedQuery, opts, cancellationToken) as IEnumerable<T>,
            _ => null
        };

        items ??= Enumerable.Empty<T>();
        var itemsList = items.ToList();
        var totalCount = itemsList.Count;
        var pagedItems = itemsList.Skip(opts.Skip).Take(opts.Take).ToList();

        stopwatch.Stop();

        return new SearchResult<T>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
            Page = opts.Skip / opts.Take,
            TotalPages = (int)Math.Ceiling((double)totalCount / opts.Take)
        };
    }

    /// <inheritdoc />
    public Task IndexAsync<T>(T document, string id, CancellationToken cancellationToken = default) where T : class
    {
        // BuiltIn provider uses database directly, no separate indexing needed
        _logger.LogDebug("BuiltIn search provider: IndexAsync called for {EntityType} - no-op (uses DB directly)", typeof(T).Name);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task IndexBatchAsync<T>(IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken cancellationToken = default) where T : class
    {
        // BuiltIn provider uses database directly, no separate indexing needed
        var count = documents.Count();
        _logger.LogDebug("BuiltIn search provider: IndexBatchAsync called for {Count} {EntityType} documents - no-op (uses DB directly)", count, typeof(T).Name);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : class
    {
        // BuiltIn provider uses database directly, no separate index to delete from
        _logger.LogDebug("BuiltIn search provider: DeleteAsync called for {EntityType} {Id} - no-op (uses DB directly)", typeof(T).Name, id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SearchSuggestion>> SuggestAsync(string prefix, string? indexName = null, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var suggestions = new List<SearchSuggestion>();
        var normalizedPrefix = prefix?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(normalizedPrefix) || normalizedPrefix.Length < 2)
        {
            return suggestions;
        }

        var context = _dbContextResolver.ResolveContext();

        // Get account suggestions
        var accountSuggestions = await context.Accounts
            .Where(a => !a.IsDeleted && (
                a.Company.ToLower().StartsWith(normalizedPrefix) ||
                a.FirstName.ToLower().StartsWith(normalizedPrefix) ||
                a.LastName.ToLower().StartsWith(normalizedPrefix)))
            .Take(maxResults / 2)
            .Select(a => new SearchSuggestion
            {
                Text = a.Category == AccountCategory.Organization ? a.Company : $"{a.FirstName} {a.LastName}".Trim(),
                EntityType = "Account",
                EntityId = a.Id.ToString(),
                Score = 1.0
            })
            .ToListAsync(cancellationToken);

        suggestions.AddRange(accountSuggestions);

        // Get contact suggestions - Contact uses Status != Archived (no IsDeleted property)
        var contactSuggestions = await context.Contacts
            .Where(c => c.Status != ContactStatus.Archived && (
                c.FirstName.ToLower().StartsWith(normalizedPrefix) ||
                c.LastName.ToLower().StartsWith(normalizedPrefix) ||
                (c.EmailPrimary != null && c.EmailPrimary.ToLower().StartsWith(normalizedPrefix))))
            .Take(maxResults / 2)
            .Select(c => new SearchSuggestion
            {
                Text = $"{c.FirstName} {c.LastName}".Trim(),
                EntityType = "Contact",
                EntityId = c.Id.ToString(),
                Score = 0.9
            })
            .ToListAsync(cancellationToken);

        suggestions.AddRange(contactSuggestions);

        return suggestions
            .OrderByDescending(s => s.Score)
            .Take(maxResults);
    }

    /// <inheritdoc />
    public Task ClearIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        // BuiltIn provider uses database directly, no separate index to clear
        _logger.LogDebug("BuiltIn search provider: ClearIndexAsync called for {EntityType} - no-op (uses DB directly)", typeof(T).Name);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RebuildIndexAsync<T>(IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken cancellationToken = default) where T : class
    {
        // BuiltIn provider uses database directly, no separate index to rebuild
        var count = documents.Count();
        _logger.LogDebug("BuiltIn search provider: RebuildIndexAsync called for {Count} {EntityType} documents - no-op (uses DB directly)", count, typeof(T).Name);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var context = _dbContextResolver.ResolveContext();

            // Simple connectivity check
            var canConnect = await context.Database.CanConnectAsync(cancellationToken);

            stopwatch.Stop();

            if (canConnect)
            {
                return new ProviderHealthResult
                {
                    ProviderName = ProviderName,
                    IsHealthy = true,
                    Message = "Connected",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Details = new Dictionary<string, object>
                    {
                        ["DatabaseProvider"] = context.Database.ProviderName ?? "Unknown"
                    }
                };
            }

            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = false,
                Message = "Cannot connect to database",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "BuiltIn search provider health check failed");

            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = false,
                Message = $"Error: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    #region Private Helper Methods

    private static IEnumerable<string> GetSearchableEntityTypes(string? requestedType)
    {
        var allTypes = new[] { "Account", "Contact", "Opportunity", "Product", "KnowledgeArticle" };

        if (string.IsNullOrEmpty(requestedType))
        {
            return allTypes;
        }

        return allTypes.Where(t => t.Equals(requestedType, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<List<SearchHit>> SearchEntityTypeAsync(
        ICrmDbContext context,
        string entityType,
        string query,
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        return entityType switch
        {
            "Account" => await SearchAccountHitsAsync(context, query, request.IncludeHighlights, cancellationToken),
            "Contact" => await SearchContactHitsAsync(context, query, request.IncludeHighlights, cancellationToken),
            "Opportunity" => await SearchOpportunityHitsAsync(context, query, request.IncludeHighlights, cancellationToken),
            "Product" => await SearchProductHitsAsync(context, query, request.IncludeHighlights, cancellationToken),
            "KnowledgeArticle" => await SearchKnowledgeArticleHitsAsync(context, query, request.IncludeHighlights, cancellationToken),
            _ => new List<SearchHit>()
        };
    }

    #endregion

    #region Entity-Specific Search Hit Methods

    private async Task<List<SearchHit>> SearchAccountHitsAsync(
        ICrmDbContext context,
        string query,
        bool includeHighlights,
        CancellationToken cancellationToken)
    {
        var accounts = await context.Accounts
            .Where(a => !a.IsDeleted && (
                a.Company.ToLower().Contains(query) ||
                a.FirstName.ToLower().Contains(query) ||
                a.LastName.ToLower().Contains(query) ||
                a.Email.ToLower().Contains(query)))
            .Take(50)
            .ToListAsync(cancellationToken);

        return accounts.Select(a => new SearchHit
        {
            Id = a.Id.ToString(),
            EntityType = "Account",
            Title = a.Category == AccountCategory.Organization ? a.Company : $"{a.FirstName} {a.LastName}".Trim(),
            Description = a.Email,
            Score = CalculateScore(query, a.Company, a.FirstName, a.LastName, a.Email),
            Highlights = includeHighlights ? GetHighlights(query, a.Company, a.Email) : null,
            Metadata = new Dictionary<string, object>
            {
                ["Category"] = a.Category.ToString(),
                ["Email"] = a.Email ?? string.Empty
            }
        }).ToList();
    }

    private async Task<List<SearchHit>> SearchContactHitsAsync(
        ICrmDbContext context,
        string query,
        bool includeHighlights,
        CancellationToken cancellationToken)
    {
        // Contact uses Status for filtering (no IsDeleted property)
        var contacts = await context.Contacts
            .Where(c => c.Status != ContactStatus.Archived && (
                c.FirstName.ToLower().Contains(query) ||
                c.LastName.ToLower().Contains(query) ||
                (c.EmailPrimary != null && c.EmailPrimary.ToLower().Contains(query))))
            .Take(50)
            .ToListAsync(cancellationToken);

        return contacts.Select(c => new SearchHit
        {
            Id = c.Id.ToString(),
            EntityType = "Contact",
            Title = $"{c.FirstName} {c.LastName}".Trim(),
            Description = c.EmailPrimary ?? c.Company,
            Score = CalculateScore(query, c.FirstName, c.LastName, c.EmailPrimary ?? string.Empty),
            Highlights = includeHighlights ? GetHighlights(query, $"{c.FirstName} {c.LastName}", c.EmailPrimary) : null,
            Metadata = new Dictionary<string, object>
            {
                ["Email"] = c.EmailPrimary ?? string.Empty,
                ["JobTitle"] = c.JobTitle ?? string.Empty
            }
        }).ToList();
    }

    private async Task<List<SearchHit>> SearchOpportunityHitsAsync(
        ICrmDbContext context,
        string query,
        bool includeHighlights,
        CancellationToken cancellationToken)
    {
        var opportunities = await context.Opportunities
            .Include(o => o.Account)
            .Where(o => !o.IsDeleted && (
                o.Name.ToLower().Contains(query) ||
                (o.Account != null && o.Account.Company.ToLower().Contains(query))))
            .Take(50)
            .ToListAsync(cancellationToken);

        return opportunities.Select(o => new SearchHit
        {
            Id = o.Id.ToString(),
            EntityType = "Opportunity",
            Title = o.Name,
            Description = $"${o.Amount:N0} - {o.Stage}",
            Score = CalculateScore(query, o.Name),
            Highlights = includeHighlights ? GetHighlights(query, o.Name) : null,
            Metadata = new Dictionary<string, object>
            {
                ["Stage"] = o.Stage.ToString(),
                ["Amount"] = o.Amount
            }
        }).ToList();
    }

    private async Task<List<SearchHit>> SearchProductHitsAsync(
        ICrmDbContext context,
        string query,
        bool includeHighlights,
        CancellationToken cancellationToken)
    {
        var products = await context.Products
            .Where(p => !p.IsDeleted && p.IsActive && (
                p.Name.ToLower().Contains(query) ||
                (p.ProductCode != null && p.ProductCode.ToLower().Contains(query)) ||
                (p.Description != null && p.Description.ToLower().Contains(query))))
            .Take(50)
            .ToListAsync(cancellationToken);

        return products.Select(p => new SearchHit
        {
            Id = p.Id.ToString(),
            EntityType = "Product",
            Title = p.Name,
            Description = p.Description ?? p.ProductCode,
            Score = CalculateScore(query, p.Name, p.ProductCode ?? string.Empty),
            Highlights = includeHighlights ? GetHighlights(query, p.Name, p.Description) : null,
            Metadata = new Dictionary<string, object>
            {
                ["ProductCode"] = p.ProductCode ?? string.Empty,
                ["Price"] = p.Price
            }
        }).ToList();
    }

    private async Task<List<SearchHit>> SearchKnowledgeArticleHitsAsync(
        ICrmDbContext context,
        string query,
        bool includeHighlights,
        CancellationToken cancellationToken)
    {
        var articles = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted &&
                a.PublishingState == PublishingState.Published && (
                a.Title.ToLower().Contains(query) ||
                (a.ShortDescription != null && a.ShortDescription.ToLower().Contains(query)) ||
                a.ArticleBody.ToLower().Contains(query)))
            .Take(50)
            .ToListAsync(cancellationToken);

        return articles.Select(a => new SearchHit
        {
            Id = a.ArticleId.ToString(), // KnowledgeArticle uses ArticleId as primary key
            EntityType = "KnowledgeArticle",
            Title = a.Title,
            Description = a.ShortDescription ?? TruncateText(a.ArticleBody, 200),
            Score = CalculateScore(query, a.Title, a.ShortDescription ?? string.Empty),
            Highlights = includeHighlights ? GetHighlights(query, a.Title, a.ShortDescription) : null,
            Metadata = new Dictionary<string, object>
            {
                ["Number"] = a.Number ?? string.Empty,
                ["ViewCount"] = a.ViewCount
            }
        }).ToList();
    }

    #endregion

    #region Typed Search Methods (for generic SearchAsync<T>)

    private async Task<IEnumerable<Account>> SearchAccountsAsync(
        ICrmDbContext context,
        string query,
        SearchOptions opts,
        CancellationToken cancellationToken)
    {
        return await context.Accounts
            .Where(a => !a.IsDeleted && (
                a.Company.ToLower().Contains(query) ||
                a.FirstName.ToLower().Contains(query) ||
                a.LastName.ToLower().Contains(query) ||
                a.Email.ToLower().Contains(query)))
            .OrderBy(a => a.Company)
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<Contact>> SearchContactsAsync(
        ICrmDbContext context,
        string query,
        SearchOptions opts,
        CancellationToken cancellationToken)
    {
        // Contact uses Status for filtering (no IsDeleted property)
        return await context.Contacts
            .Where(c => c.Status != ContactStatus.Archived && (
                c.FirstName.ToLower().Contains(query) ||
                c.LastName.ToLower().Contains(query) ||
                (c.EmailPrimary != null && c.EmailPrimary.ToLower().Contains(query))))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<Opportunity>> SearchOpportunitiesAsync(
        ICrmDbContext context,
        string query,
        SearchOptions opts,
        CancellationToken cancellationToken)
    {
        return await context.Opportunities
            .Include(o => o.Account)
            .Where(o => !o.IsDeleted && (
                o.Name.ToLower().Contains(query) ||
                (o.Account != null && o.Account.Company.ToLower().Contains(query))))
            .OrderByDescending(o => o.Amount)
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<Product>> SearchProductsAsync(
        ICrmDbContext context,
        string query,
        SearchOptions opts,
        CancellationToken cancellationToken)
    {
        return await context.Products
            .Where(p => !p.IsDeleted && p.IsActive && (
                p.Name.ToLower().Contains(query) ||
                (p.ProductCode != null && p.ProductCode.ToLower().Contains(query))))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<KnowledgeArticle>> SearchKnowledgeArticlesAsync(
        ICrmDbContext context,
        string query,
        SearchOptions opts,
        CancellationToken cancellationToken)
    {
        return await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted &&
                a.PublishingState == PublishingState.Published && (
                a.Title.ToLower().Contains(query) ||
                (a.ShortDescription != null && a.ShortDescription.ToLower().Contains(query)) ||
                a.ArticleBody.ToLower().Contains(query)))
            .OrderByDescending(a => a.ViewCount)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Utility Methods

    private static double CalculateScore(string query, params string[] fields)
    {
        double score = 0.0;
        var lowerQuery = query.ToLowerInvariant();

        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field)) continue;

            var lowerField = field.ToLowerInvariant();

            // Exact match (highest score)
            if (lowerField == lowerQuery)
            {
                score += 1.0;
            }
            // Starts with (high score)
            else if (lowerField.StartsWith(lowerQuery))
            {
                score += 0.8;
            }
            // Contains (lower score)
            else if (lowerField.Contains(lowerQuery))
            {
                score += 0.5;
            }
        }

        return Math.Min(score, 1.0); // Cap at 1.0
    }

    private static Dictionary<string, string>? GetHighlights(string query, params string?[] fields)
    {
        var highlights = new Dictionary<string, string>();
        var lowerQuery = query.ToLowerInvariant();

        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (string.IsNullOrEmpty(field)) continue;

            var lowerField = field.ToLowerInvariant();
            if (lowerField.Contains(lowerQuery))
            {
                // Simple highlight: wrap matched text with <em> tags
                var highlightedText = field.Replace(
                    query,
                    $"<em>{query}</em>",
                    StringComparison.OrdinalIgnoreCase);
                highlights[$"field_{i}"] = highlightedText;
            }
        }

        return highlights.Count > 0 ? highlights : null;
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        // Find the last space within the max length to avoid cutting words
        var lastSpace = text.LastIndexOf(' ', maxLength);
        if (lastSpace > maxLength / 2)
        {
            return text[..lastSpace] + "...";
        }

        return text[..maxLength] + "...";
    }

    #endregion
}
