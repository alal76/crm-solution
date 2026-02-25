// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SD002-012: Unit tests for Knowledge Base dedicated Meilisearch search index configuration.

using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.Search;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Verifies the Knowledge Base search index configuration (TODO-SD002-012).
/// Tests focus on <see cref="KnowledgeBaseIndexConfig"/> values and service construction,
/// avoiding live HTTP calls to Meilisearch.
/// </summary>
public class KnowledgeBaseSearchIndexServiceTests
{
    private static KnowledgeBaseSearchIndexService BuildService(
        IConfiguration? config = null)
    {
        var mockContext = new Mock<ICrmDbContext>();
        var mockHttpFactory = new Mock<System.Net.Http.IHttpClientFactory>();
        mockHttpFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new System.Net.Http.HttpClient());

        config ??= new ConfigurationBuilder().Build();

        return new KnowledgeBaseSearchIndexService(
            mockContext.Object,
            NullLogger<KnowledgeBaseSearchIndexService>.Instance,
            mockHttpFactory.Object,
            config);
    }

    [Fact]
    public void GetIndexConfiguration_ReturnsCorrectIndexName()
    {
        var service = BuildService();

        var config = service.GetIndexConfiguration();

        config.IndexName.Should().Be("crm_knowledge_articles",
            "Knowledge Base index must use the canonical index name 'crm_knowledge_articles'");
    }

    [Fact]
    public void GetIndexConfiguration_PrimaryKey_IsId()
    {
        var service = BuildService();

        var config = service.GetIndexConfiguration();

        config.PrimaryKey.Should().Be("id");
    }

    [Fact]
    public void GetIndexConfiguration_SearchableAttributes_ContainsTitleAndContent()
    {
        var service = BuildService();

        var config = service.GetIndexConfiguration();

        config.SearchableAttributes.Should().Contain("title")
            .And.Contain("content")
            .And.Contain("tags");
    }

    [Fact]
    public void GetIndexConfiguration_FilterableAttributes_ContainsStatusAndCategory()
    {
        var service = BuildService();

        var config = service.GetIndexConfiguration();

        config.FilterableAttributes.Should().Contain("status")
            .And.Contain("category")
            .And.Contain("isInternal");
    }

    [Fact]
    public void Constructor_ReadsUrlAndApiKey_FromConfiguration()
    {
        // Verify service can be constructed with explicit Meilisearch config
        var inMemoryConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Providers:Search:Meilisearch:Url"] = "http://test-meilisearch:7700",
                ["Providers:Search:Meilisearch:ApiKey"] = "testApiKey"
            })
            .Build();

        // Construction should succeed without throwing
        var service = BuildService(inMemoryConfig);

        service.Should().NotBeNull();
        service.GetIndexConfiguration().IndexName.Should().Be("crm_knowledge_articles");
    }
}
