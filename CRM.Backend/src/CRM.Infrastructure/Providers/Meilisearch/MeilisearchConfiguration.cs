// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.Meilisearch;

/// <summary>
/// Configuration options for the Meilisearch search provider.
/// Bind to "Providers:Search:Meilisearch" section in appsettings.json.
/// </summary>
public class MeilisearchConfiguration
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Providers:Search:Meilisearch";

    /// <summary>
    /// The Meilisearch server URL (e.g., "http://localhost:7700" or "https://ms-xxx.meilisearch.io").
    /// </summary>
    public string Url { get; set; } = "http://localhost:7700";

    /// <summary>
    /// The Meilisearch API key for authentication.
    /// For development, this can be the master key; for production, use a search-only key for reads.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Prefix to add to all index names (e.g., "crm_prod_" for "crm_prod_accounts").
    /// Useful for multi-tenant or environment isolation.
    /// </summary>
    public string IndexPrefix { get; set; } = "crm_";

    /// <summary>
    /// Default number of results to return per page.
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Maximum number of results to return per page.
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable highlighting in search results.
    /// </summary>
    public bool EnableHighlighting { get; set; } = true;

    /// <summary>
    /// Number of retries for failed requests.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether to automatically sync entities on create/update/delete.
    /// When false, manual reindexing is required.
    /// </summary>
    public bool AutoSyncEnabled { get; set; } = true;

    /// <summary>
    /// Batch size for bulk indexing operations.
    /// </summary>
    public int BatchSize { get; set; } = 1000;
}

// MeilisearchIndexConfig class is defined in MeilisearchIndexConfig.cs
