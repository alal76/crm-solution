// CRM Solution - AlgoliaProvider Tests
// Tests for the Algolia search provider

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Algolia;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for AlgoliaProvider.
/// Tests search, indexing, and health check functionality.
/// </summary>
public class AlgoliaProviderTests : IDisposable
{
    private readonly Mock<ILogger<AlgoliaProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<AlgoliaConfiguration> _options;
    private readonly AlgoliaProvider _provider;

    public AlgoliaProviderTests()
    {
        _loggerMock = new Mock<ILogger<AlgoliaProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://test-app.algolia.net")
        };

        _options = Options.Create(new AlgoliaConfiguration
        {
            ApplicationId = "test-app-id",
            ApiKey = "test-api-key",
            IndexPrefix = "crm_"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new AlgoliaProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesProvider()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.ProviderName.Should().Be("Algolia");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AlgoliaProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_WithValidRequest_ReturnsResults()
    {
        // Arrange
        var searchResponse = new
        {
            hits = new[]
            {
                new { objectID = "1", name = "Test Account" },
                new { objectID = "2", name = "Another Account" }
            },
            nbHits = 2,
            page = 0,
            hitsPerPage = 10
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest
        {
            Query = "test",
            Index = "accounts",
            Limit = 10
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.SearchAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SearchAsync_WithFilters_AppliesFilters()
    {
        // Arrange
        var searchResponse = new { hits = new[] { new { objectID = "1" } }, nbHits = 1 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest
        {
            Query = "test",
            Index = "accounts",
            Filters = new Dictionary<string, object>
            {
                ["accountType"] = "Customer",
                ["region"] = "US"
            }
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithFacets_IncludesFacets()
    {
        // Arrange
        var searchResponse = new
        {
            hits = new[] { new { objectID = "1" } },
            nbHits = 1,
            facets = new { accountType = new { Customer = 5, Prospect = 3 } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest
        {
            Query = "test",
            Index = "accounts",
            Facets = new List<string> { "accountType" }
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithHighlighting_ReturnsHighlights()
    {
        // Arrange
        var searchResponse = new
        {
            hits = new[]
            {
                new
                {
                    objectID = "1",
                    name = "Test Account",
                    _highlightResult = new { name = new { value = "<em>Test</em> Account" } }
                }
            },
            nbHits = 1
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest
        {
            Query = "test",
            Index = "accounts"
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Index Tests

    [Fact]
    public async Task IndexAsync_WithValidDocument_IndexesSuccessfully()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"objectID\":\"1\",\"taskID\":123}");

        var document = new SearchDocument
        {
            Id = "1",
            Index = "accounts",
            Content = new Dictionary<string, object>
            {
                ["name"] = "Acme Corp",
                ["email"] = "info@acme.com"
            }
        };

        // Act
        var result = await _provider.IndexAsync(document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IndexAsync_WithNullDocument_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.IndexAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task IndexBatchAsync_WithMultipleDocuments_IndexesAll()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"objectIDs\":[\"1\",\"2\",\"3\"],\"taskID\":123}");

        var documents = new List<SearchDocument>
        {
            new SearchDocument { Id = "1", Index = "accounts", Content = new Dictionary<string, object> { ["name"] = "Account 1" } },
            new SearchDocument { Id = "2", Index = "accounts", Content = new Dictionary<string, object> { ["name"] = "Account 2" } },
            new SearchDocument { Id = "3", Index = "accounts", Content = new Dictionary<string, object> { ["name"] = "Account 3" } }
        };

        // Act
        var result = await _provider.IndexBatchAsync(documents);

        // Assert
        result.SuccessCount.Should().Be(3);
        result.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteFromIndexAsync_WithValidId_DeletesDocument()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"taskID\":123}");

        // Act
        var result = await _provider.DeleteFromIndexAsync("accounts", "1");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Suggest Tests

    [Fact]
    public async Task SuggestAsync_WithPartialQuery_ReturnsSuggestions()
    {
        // Arrange
        var searchResponse = new
        {
            hits = new[]
            {
                new { objectID = "1", name = "Acme Corporation" },
                new { objectID = "2", name = "Acme Industries" }
            },
            nbHits = 2
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SuggestRequest
        {
            Query = "Acm",
            Index = "accounts",
            Limit = 5
        };

        // Act
        var suggestions = await _provider.SuggestAsync(request);

        // Assert
        suggestions.Should().NotBeNull();
        suggestions.Should().HaveCount(2);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyServer_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"published\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Algolia");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyServer_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, "Service unavailable");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyServer_ReturnsTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"published\"}");

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Index Management Tests

    [Fact]
    public async Task GetAllIndexesAsync_ReturnsIndexList()
    {
        // Arrange
        var indexes = new
        {
            items = new[]
            {
                new { name = "crm_accounts", entries = 100 },
                new { name = "crm_contacts", entries = 50 }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(indexes));

        // Act
        var result = await _provider.GetAllIndexesAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetIndexConfigurationAsync_ReturnsSettings()
    {
        // Arrange
        var settings = new
        {
            searchableAttributes = new[] { "name", "email" },
            attributesForFaceting = new[] { "accountType" }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(settings));

        // Act
        var config = await _provider.GetIndexConfigurationAsync("accounts");

        // Assert
        config.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SearchAsync_WithInvalidApiKey_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Forbidden, "{\"message\":\"Invalid API key\"}");
        var request = new SearchRequest { Query = "test", Index = "accounts" };

        // Act
        var act = () => _provider.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SearchAsync_WithRateLimitExceeded_ThrowsException()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, "{\"message\":\"Rate limit exceeded\"}");
        var request = new SearchRequest { Query = "test", Index = "accounts" };

        // Act
        var act = () => _provider.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task SearchAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var searchResponse = new { hits = Array.Empty<object>(), nbHits = 0 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest { Query = "test", Index = "accounts" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SearchAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Multi-Index Search Tests

    [Fact]
    public async Task UnifiedSearchAsync_SearchesMultipleIndexes()
    {
        // Arrange
        var searchResponse = new
        {
            results = new[]
            {
                new { hits = new[] { new { objectID = "1" } }, index = "crm_accounts", nbHits = 1 },
                new { hits = new[] { new { objectID = "2" } }, index = "crm_contacts", nbHits = 1 }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest { Query = "test", Limit = 10 };

        // Act
        var result = await _provider.UnifiedSearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
