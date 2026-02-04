// CRM Solution - Pluggable Architecture
// Meilisearch Provider Unit Tests
//
// These tests verify the MeilisearchProvider functionality
// using mocked Meilisearch client to ensure proper behavior.

using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Meilisearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Unit.Providers;

/// <summary>
/// Unit tests for MeilisearchProvider
/// </summary>
public class MeilisearchProviderTests
{
    private readonly Mock<ILogger<MeilisearchProvider>> _loggerMock;
    private readonly MeilisearchConfiguration _config;
    private readonly IOptions<MeilisearchConfiguration> _optionsWrapper;

    public MeilisearchProviderTests()
    {
        _loggerMock = new Mock<ILogger<MeilisearchProvider>>();
        _config = new MeilisearchConfiguration
        {
            Url = "http://localhost:7700",
            ApiKey = "test-api-key",
            IndexPrefix = "test_",
            DefaultPageSize = 20,
            MaxPageSize = 100,
            TimeoutSeconds = 30,
            EnableHighlighting = true,
            AutoSyncEnabled = false,
            BatchSize = 100
        };
        _optionsWrapper = Options.Create(_config);
    }

    #region Configuration Tests

    [Fact]
    public void Constructor_ShouldInitializeWithValidConfig()
    {
        // Act
        var provider = new MeilisearchProvider(_optionsWrapper, _loggerMock.Object);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal("Meilisearch", provider.ProviderName);
    }

    [Fact]
    public void ProviderName_ShouldReturnMeilisearch()
    {
        // Arrange
        var provider = new MeilisearchProvider(_optionsWrapper, _loggerMock.Object);

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.Equal("Meilisearch", name);
    }

    #endregion

    #region Index Name Resolution Tests

    [Theory]
    [InlineData(typeof(Account), "accounts")]
    [InlineData(typeof(Opportunity), "opportunities")]
    [InlineData(typeof(Product), "products")]
    [InlineData(typeof(KnowledgeArticle), "knowledge_articles")]
    [InlineData(typeof(Lead), "leads")]
    public void GetIndexName_ShouldReturnCorrectName(Type entityType, string expectedIndexName)
    {
        // Arrange
        var provider = new MeilisearchProvider(_optionsWrapper, _loggerMock.Object);

        // Act - Use reflection to test private method
        var method = typeof(MeilisearchProvider).GetMethod("GetIndexNameForType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var genericMethod = method!.MakeGenericMethod(entityType);
        var result = genericMethod.Invoke(provider, null) as string;

        // Assert
        Assert.Equal(expectedIndexName, result);
    }

    #endregion

    #region Search Request Validation Tests

    [Fact]
    public async Task SearchAsync_WithNullQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var provider = new MeilisearchProvider(_optionsWrapper, _loggerMock.Object);
        var request = new SearchRequest
        {
            Query = null!,
            Skip = 0,
            Take = 20
        };

        // Act
        var result = await provider.SearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Hits);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var provider = new MeilisearchProvider(_optionsWrapper, _loggerMock.Object);
        var request = new SearchRequest
        {
            Query = "   ",
            Skip = 0,
            Take = 20
        };

        // Act
        var result = await provider.SearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Hits);
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void MeilisearchConfiguration_DefaultValues_ShouldBeReasonable()
    {
        // Arrange
        var config = new MeilisearchConfiguration();

        // Assert - Verify defaults match the actual class defaults
        Assert.Equal(20, config.DefaultPageSize);
        Assert.Equal(100, config.MaxPageSize);
        Assert.Equal(30, config.TimeoutSeconds);
        Assert.True(config.EnableHighlighting);
        Assert.True(config.AutoSyncEnabled); // Default is true
        Assert.Equal(1000, config.BatchSize); // Default is 1000
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public void SearchConfig_MaxPageSize_ShouldBeRespected()
    {
        // Assert that MaxPageSize is respected in config
        Assert.Equal(100, _config.MaxPageSize);
    }

    #endregion

    #region Search Options Tests

    [Fact]
    public void SearchOptions_ShouldHaveReasonableDefaults()
    {
        // Arrange & Act
        var options = new SearchOptions();

        // Assert
        Assert.Equal(0, options.Skip);
        Assert.Equal(20, options.Take);
        Assert.Null(options.Filters);
        Assert.Null(options.SortBy);
        Assert.False(options.SortDescending);
        Assert.True(options.IncludeHighlights);
    }

    [Fact]
    public void SearchRequest_ShouldSupportFiltering()
    {
        // Arrange & Act
        var request = new SearchRequest
        {
            Query = "test",
            Filters = new Dictionary<string, string>
            {
                ["industry"] = "Technology",
                ["status"] = "Active"
            }
        };

        // Assert
        Assert.NotNull(request.Filters);
        Assert.Equal(2, request.Filters.Count);
        Assert.Equal("Technology", request.Filters["industry"]);
    }

    [Fact]
    public void SearchRequest_ShouldSupportSorting()
    {
        // Arrange & Act
        var request = new SearchRequest
        {
            Query = "test",
            SortBy = "name",
            SortDescending = true
        };

        // Assert
        Assert.Equal("name", request.SortBy);
        Assert.True(request.SortDescending);
    }

    #endregion

    #region Search Result Tests

    [Fact]
    public void SearchResult_ShouldContainAllRequiredProperties()
    {
        // Arrange & Act
        var result = new CRM.Core.Ports.Output.Providers.SearchResult
        {
            Hits = new List<SearchHit>(),
            TotalCount = 0,
            ProcessingTimeMs = 10,
            Query = "test"
        };

        // Assert
        Assert.NotNull(result.Hits);
        Assert.Empty(result.Hits);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(10, result.ProcessingTimeMs);
        Assert.Equal("test", result.Query);
    }

    [Fact]
    public void SearchHit_ShouldContainEntityInformation()
    {
        // Arrange & Act
        var hit = new SearchHit
        {
            Id = "123",
            EntityType = "Account",
            Title = "Acme Corp",
            Description = "A test company",
            Score = 0.95,
            Highlights = new Dictionary<string, string>
            {
                ["company"] = "<em>Acme</em> Corp"
            }
        };

        // Assert
        Assert.Equal("123", hit.Id);
        Assert.Equal("Account", hit.EntityType);
        Assert.Equal("Acme Corp", hit.Title);
        Assert.Equal("A test company", hit.Description);
        Assert.Equal(0.95, hit.Score);
        Assert.NotNull(hit.Highlights);
        Assert.Single(hit.Highlights);
    }

    #endregion

    #region Generic Search Result Tests

    [Fact]
    public void GenericSearchResult_ShouldContainTypedItems()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Company = "Test Account 1" },
            new Account { Id = 2, Company = "Test Account 2" }
        };

        // Act
        var result = new CRM.Core.Ports.Output.Providers.SearchResult<Account>
        {
            Items = accounts,
            TotalCount = 2,
            ProcessingTimeMs = 5
        };

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(5, result.ProcessingTimeMs);
    }

    #endregion

    #region Health Check Configuration Tests

    [Fact]
    public void MeilisearchHealthCheck_ShouldBeConfigurable()
    {
        // Arrange
        var healthCheckConfig = new MeilisearchConfiguration
        {
            Url = "http://meilisearch:7700",
            ApiKey = "health-check-key"
        };

        // Assert
        Assert.NotNull(healthCheckConfig.Url);
        Assert.NotNull(healthCheckConfig.ApiKey);
    }

    #endregion
}
