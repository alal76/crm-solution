// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Providers.Meilisearch;

/// <summary>
/// Configuration class for Meilisearch index schemas.
/// Defines filterable and sortable attributes for each entity type.
/// TODO-SD002-012: Meilisearch index schema for KB articles.
/// </summary>
public static class SearchIndexConfiguration
{
    /// <summary>
    /// Gets the index configuration for Knowledge Base articles.
    /// </summary>
    public static MeilisearchIndexConfig KnowledgeArticles => new()
    {
        IndexName = "knowledge_articles",
        PrimaryKey = "articleId",
        SearchableAttributes = new List<string>
        {
            "title",
            "shortDescription",
            "articleBody",
            "tags",
            "authorName",
            "categoryName"
        },
        FilterableAttributes = new List<string>
        {
            "articleId",
            "categoryId",
            "categoryName",
            "articleType",
            "publishingState",
            "tags",
            "isInternal",
            "isExternal",
            "isPublic",
            "authorId",
            "viewCount"
        },
        SortableAttributes = new List<string>
        {
            "createdAt",
            "publishedDate",
            "viewCount",
            "helpfulCount",
            "title"
        },
        DisplayedAttributes = new List<string>
        {
            "articleId",
            "title",
            "shortDescription",
            "categoryName",
            "articleType",
            "publishingState",
            "tags",
            "authorName",
            "createdAt",
            "publishedDate",
            "viewCount",
            "helpfulCount"
        },
        RankingRules = new List<string>
        {
            "words",
            "typo",
            "proximity",
            "attribute",
            "sort",
            "exactness",
            "viewCount:desc" // Popular articles ranked higher
        }
    };

    /// <summary>
    /// Gets the index configuration for Accounts.
    /// </summary>
    public static MeilisearchIndexConfig Accounts => new()
    {
        IndexName = "accounts",
        PrimaryKey = "id",
        SearchableAttributes = new List<string>
        {
            "name",
            "accountNumber",
            "industry",
            "website",
            "description"
        },
        FilterableAttributes = new List<string>
        {
            "id",
            "accountType",
            "industry",
            "status",
            "ownerId",
            "parentAccountId"
        },
        SortableAttributes = new List<string>
        {
            "name",
            "createdAt",
            "modifiedAt"
        }
    };

    /// <summary>
    /// Gets the index configuration for Contacts.
    /// </summary>
    public static MeilisearchIndexConfig Contacts => new()
    {
        IndexName = "contacts",
        PrimaryKey = "id",
        SearchableAttributes = new List<string>
        {
            "firstName",
            "lastName",
            "email",
            "phone",
            "title",
            "accountName"
        },
        FilterableAttributes = new List<string>
        {
            "id",
            "accountId",
            "ownerId",
            "status"
        },
        SortableAttributes = new List<string>
        {
            "lastName",
            "firstName",
            "createdAt"
        }
    };

    /// <summary>
    /// Gets the index configuration for Products.
    /// </summary>
    public static MeilisearchIndexConfig Products => new()
    {
        IndexName = "products",
        PrimaryKey = "id",
        SearchableAttributes = new List<string>
        {
            "name",
            "productCode",
            "description",
            "categoryName"
        },
        FilterableAttributes = new List<string>
        {
            "id",
            "categoryId",
            "isActive",
            "price",
            "productFamily"
        },
        SortableAttributes = new List<string>
        {
            "name",
            "price",
            "createdAt"
        }
    };

    /// <summary>
    /// Gets the index configuration for Leads.
    /// </summary>
    public static MeilisearchIndexConfig Leads => new()
    {
        IndexName = "leads",
        PrimaryKey = "id",
        SearchableAttributes = new List<string>
        {
            "firstName",
            "lastName",
            "company",
            "email",
            "title"
        },
        FilterableAttributes = new List<string>
        {
            "id",
            "status",
            "source",
            "ownerId",
            "rating"
        },
        SortableAttributes = new List<string>
        {
            "createdAt",
            "lastName",
            "company"
        }
    };

    /// <summary>
    /// Gets the index configuration for Service Requests.
    /// </summary>
    public static MeilisearchIndexConfig ServiceRequests => new()
    {
        IndexName = "service_requests",
        PrimaryKey = "id",
        SearchableAttributes = new List<string>
        {
            "title",
            "description",
            "number",
            "resolution"
        },
        FilterableAttributes = new List<string>
        {
            "id",
            "status",
            "priority",
            "categoryId",
            "assigneeId",
            "customerId"
        },
        SortableAttributes = new List<string>
        {
            "createdAt",
            "priority",
            "status"
        }
    };

    /// <summary>
    /// Gets all index configurations.
    /// </summary>
    public static IEnumerable<MeilisearchIndexConfig> AllIndexes => new[]
    {
        KnowledgeArticles,
        Accounts,
        Contacts,
        Products,
        Leads,
        ServiceRequests
    };
}
