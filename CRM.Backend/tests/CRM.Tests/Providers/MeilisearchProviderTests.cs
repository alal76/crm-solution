// CRM Solution - MeilisearchProvider Tests
// Tests for the Meilisearch search provider

using System;
using System.Collections.Generic;
using System.Linq;
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
using CRM.Infrastructure.Providers.Meilisearch;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for MeilisearchProvider.
/// Tests search, indexing, suggestions, and health check functionality.
/// </summary>
public class MeilisearchProviderTests : IDisposable
{
    private readonly Mock<ILogger<MeilisearchProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<MeilisearchConfiguration> _options;
    private readonly MeilisearchProvider _provider;

    public MeilisearchProviderTests()
    {
        _loggerMock = new Mock<ILogger<MeilisearchProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:7700")
        };

        _options = Options.Create(new MeilisearchConfiguration
        {
            Url = "http://localhost:7700",
            ApiKey = "test-api-key",
            IndexPrefix = "crm_"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new MeilisearchProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("Meilisearch");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MeilisearchProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

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
                new { id = "1", name = "Test Account", type = "account" },
                new { id = "2", name = "Test Contact", type = "contact" }
            },
            totalHits = 2,
            processingTimeMs = 5
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
    public async Task SearchAsync_WithEmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        var searchResponse = new { hits = Array.Empty<object>(), totalHits = 0 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest { Query = "", Index = "accounts" };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithFilters_AppliesFilters()
    {
        // Arrange
        var searchResponse = new { hits = new[] { new { id = "1" } }, totalHits = 1 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest
        {
            Query = "test",
            Index = "accounts",
            Filters = new Dictionary<string, object>
            {
                ["accountType"] = "Customer",
                ["isActive"] = true
            }
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithPagination_RespectsLimitAndOffset()
    {
        // Arrange
        var searchResponse = new
        {
            hits = new[] { new { id = "3" }, new { id = "4" } },
            totalHits = 10
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest
        {
            Query = "test",
            Index = "accounts",
            Limit = 2,
            Offset = 2
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task SearchAsync_WithServerError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server error");
        var request = new SearchRequest { Query = "test", Index = "accounts" };

        // Act
        var act = () => _provider.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

    #region Unified Search Tests

    [Fact]
    public async Task UnifiedSearchAsync_SearchesMultipleIndexes()
    {
        // Arrange
        var searchResponse = new
        {
            hits = new[]
            {
                new { id = "1", _index = "accounts" },
                new { id = "2", _index = "contacts" }
            },
            totalHits = 2
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest { Query = "test", Limit = 10 };

        // Act
        var result = await _provider.UnifiedSearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Index Tests

    [Fact]
    public async Task IndexAsync_WithValidDocument_IndexesSuccessfully()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted, "{\"taskUid\":123}");

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
        SetupHttpResponse(HttpStatusCode.Accepted, "{\"taskUid\":123}");

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
    public async Task IndexBatchAsync_WithEmptyList_ReturnsZeroCounts()
    {
        // Act
        var result = await _provider.IndexBatchAsync(new List<SearchDocument>());

        // Assert
        result.SuccessCount.Should().Be(0);
        result.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteFromIndexAsync_WithValidId_DeletesDocument()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted, "{\"taskUid\":123}");

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
                new { id = "1", name = "Acme Corporation" },
                new { id = "2", name = "Acme Industries" }
            },
            totalHits = 2
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

    [Fact]
    public async Task SuggestAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.SuggestAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyServer_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"available\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Meilisearch");
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
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"available\"}");

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public async Task GetIndexConfigurationAsync_ReturnsConfiguration()
    {
        // Arrange
        var indexInfo = new { uid = "crm_accounts", primaryKey = "id", numberOfDocuments = 100 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(indexInfo));

        // Act
        var config = await _provider.GetIndexConfigurationAsync("accounts");

        // Assert
        config.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllIndexesAsync_ReturnsIndexList()
    {
        // Arrange
        var indexes = new
        {
            results = new[]
            {
                new { uid = "crm_accounts", primaryKey = "id" },
                new { uid = "crm_contacts", primaryKey = "id" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(indexes));

        // Act
        var result = await _provider.GetAllIndexesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("crm_accounts");
        result.Should().Contain("crm_contacts");
    }

    [Fact]
    public async Task CreateIndexAsync_CreatesNewIndex()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted, "{\"taskUid\":123}");

        // Act
        var result = await _provider.CreateIndexAsync("new_index", "id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteIndexAsync_DeletesIndex()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted, "{\"taskUid\":123}");

        // Act
        var result = await _provider.DeleteIndexAsync("old_index");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task SearchAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var searchResponse = new { hits = Array.Empty<object>(), totalHits = 0 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(searchResponse));

        var request = new SearchRequest { Query = "test", Index = "accounts" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SearchAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IndexAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Accepted, "{\"taskUid\":123}");
        var document = new SearchDocument { Id = "1", Index = "accounts", Content = new Dictionary<string, object>() };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.IndexAsync(document, cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SearchAsync_WithNotFoundIndex_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "{\"message\":\"Index not found\"}");
        var request = new SearchRequest { Query = "test", Index = "nonexistent" };

        // Act
        var act = () => _provider.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SearchAsync_WithUnauthorized_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"message\":\"Invalid API key\"}");
        var request = new SearchRequest { Query = "test", Index = "accounts" };

        // Act
        var act = () => _provider.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion
}
