// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.BuiltIn;

/// <summary>
/// Defines searchable entities and their field configurations.
/// Used by both BuiltIn and external search providers for consistent indexing.
/// </summary>
public static class SearchIndexDefinitions
{
    /// <summary>
    /// All defined index configurations.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, SearchIndexConfig> Indexes = new Dictionary<string, SearchIndexConfig>
    {
        ["accounts"] = new SearchIndexConfig
        {
            EntityType = "Account",
            DisplayName = "Accounts",
            SearchableFields = new[] { "Company", "FirstName", "LastName", "Email", "Phone", "Website", "City", "State" },
            FilterableFields = new[] { "Category", "LifecycleStage", "Priority", "Industry", "AssignedUserId" },
            SortableFields = new[] { "Company", "CreatedAt", "ModifiedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "Company|FirstName+LastName",
                ["description"] = "Email",
                ["subtitle"] = "City+State"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted"
        },

        ["contacts"] = new SearchIndexConfig
        {
            EntityType = "Contact",
            DisplayName = "Contacts",
            SearchableFields = new[] { "FirstName", "LastName", "Email", "Phone", "MobilePhone", "JobTitle", "Company" },
            FilterableFields = new[] { "Status", "AccountId", "AssignedUserId", "LeadSource" },
            SortableFields = new[] { "LastName", "FirstName", "CreatedAt", "ModifiedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "FirstName+LastName",
                ["description"] = "Email",
                ["subtitle"] = "JobTitle+Company"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted"
        },

        ["opportunities"] = new SearchIndexConfig
        {
            EntityType = "Opportunity",
            DisplayName = "Opportunities",
            SearchableFields = new[] { "Name", "Description" },
            FilterableFields = new[] { "Stage", "AccountId", "OwnerId", "CloseDate" },
            SortableFields = new[] { "Name", "Amount", "CloseDate", "Probability", "CreatedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "Name",
                ["description"] = "Stage",
                ["subtitle"] = "Amount"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted"
        },

        ["products"] = new SearchIndexConfig
        {
            EntityType = "Product",
            DisplayName = "Products",
            SearchableFields = new[] { "Name", "ProductCode", "Description", "Category" },
            FilterableFields = new[] { "Category", "IsActive", "ProductFamily" },
            SortableFields = new[] { "Name", "Price", "CreatedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "Name",
                ["description"] = "Description",
                ["subtitle"] = "ProductCode+Price"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted",
            AdditionalFilters = new Dictionary<string, object>
            {
                ["IsActive"] = true
            }
        },

        ["knowledge_articles"] = new SearchIndexConfig
        {
            EntityType = "KnowledgeArticle",
            DisplayName = "Knowledge Articles",
            SearchableFields = new[] { "Title", "ShortDescription", "ArticleBody", "Keywords" },
            FilterableFields = new[] { "CategoryId", "ArticleType", "PublishingState", "IsInternal" },
            SortableFields = new[] { "Title", "ViewCount", "PublishedDate", "ModifiedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "Title",
                ["description"] = "ShortDescription",
                ["subtitle"] = "Number"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted",
            AdditionalFilters = new Dictionary<string, object>
            {
                ["PublishingState"] = "Published"
            }
        },

        ["leads"] = new SearchIndexConfig
        {
            EntityType = "Lead",
            DisplayName = "Leads",
            SearchableFields = new[] { "FirstName", "LastName", "Email", "Company", "Phone" },
            FilterableFields = new[] { "Status", "Source", "AssignedUserId", "Rating" },
            SortableFields = new[] { "LastName", "Company", "CreatedAt", "Score" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "FirstName+LastName",
                ["description"] = "Company",
                ["subtitle"] = "Email"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted"
        },

        ["incidents"] = new SearchIndexConfig
        {
            EntityType = "Incident",
            DisplayName = "Incidents",
            SearchableFields = new[] { "Number", "ShortDescription", "Description" },
            FilterableFields = new[] { "State", "Priority", "AssignedGroupId", "CategoryId" },
            SortableFields = new[] { "Number", "Priority", "CreatedAt", "ResolvedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "Number",
                ["description"] = "ShortDescription",
                ["subtitle"] = "State+Priority"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted"
        },

        ["catalog_items"] = new SearchIndexConfig
        {
            EntityType = "CatalogItem",
            DisplayName = "Service Catalog",
            SearchableFields = new[] { "Name", "ShortDescription", "Description" },
            FilterableFields = new[] { "CategoryId", "IsActive", "FulfillmentGroupId" },
            SortableFields = new[] { "Name", "PopularityIndex", "CreatedAt" },
            DisplayFieldMappings = new Dictionary<string, string>
            {
                ["title"] = "Name",
                ["description"] = "ShortDescription",
                ["subtitle"] = "Price"
            },
            PrimaryKeyField = "Id",
            SoftDeleteField = "IsDeleted",
            AdditionalFilters = new Dictionary<string, object>
            {
                ["IsActive"] = true
            }
        }
    };

    /// <summary>
    /// Gets index configuration for an entity type.
    /// </summary>
    public static SearchIndexConfig? GetIndexConfig(string entityType)
    {
        var key = entityType.ToLowerInvariant() switch
        {
            "account" => "accounts",
            "contact" => "contacts",
            "opportunity" => "opportunities",
            "product" => "products",
            "knowledgearticle" => "knowledge_articles",
            "lead" => "leads",
            "incident" => "incidents",
            "catalogitem" => "catalog_items",
            _ => entityType.ToLowerInvariant()
        };

        return Indexes.TryGetValue(key, out var config) ? config : null;
    }

    /// <summary>
    /// Gets all index names.
    /// </summary>
    public static IEnumerable<string> GetAllIndexNames() => Indexes.Keys;
}

/// <summary>
/// Configuration for a search index.
/// </summary>
public class SearchIndexConfig
{
    /// <summary>
    /// The entity type name (e.g., "Account", "Contact").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name for the index.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Fields that should be full-text searchable.
    /// </summary>
    public string[] SearchableFields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Fields that can be used for filtering (exact match).
    /// </summary>
    public string[] FilterableFields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Fields that can be used for sorting.
    /// </summary>
    public string[] SortableFields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Mappings for display fields (title, description, subtitle).
    /// Format: "FieldName" or "Field1+Field2" for concatenation or "Field1|Field2" for fallback.
    /// </summary>
    public Dictionary<string, string> DisplayFieldMappings { get; set; } = new();

    /// <summary>
    /// The primary key field name.
    /// </summary>
    public string PrimaryKeyField { get; set; } = "Id";

    /// <summary>
    /// The soft delete field name (null if hard delete).
    /// </summary>
    public string? SoftDeleteField { get; set; }

    /// <summary>
    /// Additional default filters to apply (e.g., IsActive = true).
    /// </summary>
    public Dictionary<string, object> AdditionalFilters { get; set; } = new();

    /// <summary>
    /// Custom ranking rules for this index.
    /// </summary>
#pragma warning disable SA1011 // Closing square bracket should be followed by a space
    public string[]? RankingRules { get; set; }
#pragma warning restore SA1011

    /// <summary>
    /// Fields to use for synonym matching.
    /// </summary>
#pragma warning disable SA1011 // Closing square bracket should be followed by a space
    public string[]? SynonymFields { get; set; }
#pragma warning restore SA1011
}

/// <summary>
/// Extension methods for search index operations.
/// </summary>
public static class SearchIndexExtensions
{
    /// <summary>
    /// Builds a Meilisearch-compatible index name.
    /// </summary>
    public static string ToMeilisearchIndexName(this SearchIndexConfig config, string prefix = "crm_")
        => $"{prefix}{config.EntityType.ToLowerInvariant()}";

    /// <summary>
    /// Builds an Algolia-compatible index name.
    /// </summary>
    public static string ToAlgoliaIndexName(this SearchIndexConfig config, string prefix = "crm_", string environment = "prod")
        => $"{prefix}{environment}_{config.EntityType.ToLowerInvariant()}";

    /// <summary>
    /// Gets the display title for an entity based on field mappings.
    /// </summary>
    public static string GetDisplayTitle(this SearchIndexConfig config, Func<string, string?> fieldGetter)
    {
        if (!config.DisplayFieldMappings.TryGetValue("title", out var mapping))
            return string.Empty;

        return ParseDisplayMapping(mapping, fieldGetter);
    }

    /// <summary>
    /// Gets the display description for an entity based on field mappings.
    /// </summary>
    public static string GetDisplayDescription(this SearchIndexConfig config, Func<string, string?> fieldGetter)
    {
        if (!config.DisplayFieldMappings.TryGetValue("description", out var mapping))
            return string.Empty;

        return ParseDisplayMapping(mapping, fieldGetter);
    }

    private static string ParseDisplayMapping(string mapping, Func<string, string?> fieldGetter)
    {
        // Handle fallback syntax (Field1|Field2)
        if (mapping.Contains('|'))
        {
            var parts = mapping.Split('|');
            foreach (var part in parts)
            {
                var value = GetConcatenatedValue(part.Trim(), fieldGetter);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }

        return GetConcatenatedValue(mapping, fieldGetter);
    }

    private static string GetConcatenatedValue(string mapping, Func<string, string?> fieldGetter)
    {
        // Handle concatenation syntax (Field1+Field2)
        if (mapping.Contains('+'))
        {
            var parts = mapping.Split('+');
            var values = parts
                .Select(p => fieldGetter(p.Trim()))
                .Where(v => !string.IsNullOrEmpty(v));
            return string.Join(" ", values);
        }

        return fieldGetter(mapping) ?? string.Empty;
    }
}
