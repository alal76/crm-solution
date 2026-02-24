// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Providers.Meilisearch;

/// <summary>
/// Configuration class for a Meilisearch index.
/// Defines the schema, filterable, sortable, and ranking rules for an index.
/// </summary>
public class MeilisearchIndexConfig
{
    /// <summary>
    /// Gets or sets the name of the index.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary key field name.
    /// </summary>
    public string PrimaryKey { get; set; } = "id";

    /// <summary>
    /// Gets or sets the list of searchable attributes in order of importance.
    /// The first attribute has the highest priority in search ranking.
    /// </summary>
    public List<string> SearchableAttributes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of filterable attributes.
    /// These attributes can be used in filter expressions.
    /// </summary>
    public List<string> FilterableAttributes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of sortable attributes.
    /// These attributes can be used for sorting search results.
    /// </summary>
    public List<string> SortableAttributes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of displayed attributes.
    /// Only these attributes will be returned in search results.
    /// If empty, all attributes are displayed.
    /// </summary>
    public List<string> DisplayedAttributes { get; set; } = new();

    /// <summary>
    /// Gets or sets the ranking rules in order of priority.
    /// Default Meilisearch rules: words, typo, proximity, attribute, sort, exactness.
    /// Custom rules can be added like "fieldName:asc" or "fieldName:desc".
    /// </summary>
    public List<string> RankingRules { get; set; } = new()
    {
        "words",
        "typo",
        "proximity",
        "attribute",
        "sort",
        "exactness"
    };

    /// <summary>
    /// Gets or sets the stop words for this index.
    /// Common words that should be ignored in searches.
    /// </summary>
    public List<string> StopWords { get; set; } = new();

    /// <summary>
    /// Gets or sets the synonyms dictionary.
    /// Key is the word, value is a list of synonyms.
    /// </summary>
    public Dictionary<string, List<string>> Synonyms { get; set; } = new();

    /// <summary>
    /// Gets or sets the distinct attribute.
    /// Used for deduplication in search results.
    /// </summary>
    public string? DistinctAttribute { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of facet values returned for each facet.
    /// </summary>
    public int MaxValuesPerFacet { get; set; } = 100;

    /// <summary>
    /// Gets or sets the typo tolerance settings.
    /// </summary>
    public TypoToleranceSettings TypoTolerance { get; set; } = new();
}

/// <summary>
/// Settings for typo tolerance in search.
/// </summary>
public class TypoToleranceSettings
{
    /// <summary>
    /// Gets or sets whether typo tolerance is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum word size for one typo to be allowed.
    /// </summary>
    public int MinWordSizeForTypos { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum word size for two typos to be allowed.
    /// </summary>
    public int MinWordSizeForTwoTypos { get; set; } = 9;

    /// <summary>
    /// Gets or sets attributes where typo tolerance is disabled.
    /// </summary>
    public List<string> DisableOnAttributes { get; set; } = new();

    /// <summary>
    /// Gets or sets specific words where typo tolerance is disabled.
    /// </summary>
    public List<string> DisableOnWords { get; set; } = new();
}
