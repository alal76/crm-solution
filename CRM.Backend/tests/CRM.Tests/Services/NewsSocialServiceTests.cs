// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for NewsSocialService - News API, Twitter, and LinkedIn integration
/// </summary>
public class NewsSocialServiceTests
{
    private readonly Mock<ILogger<NewsSocialService>> _mockLogger;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILLMService> _mockLlmService;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;

    public NewsSocialServiceTests()
    {
        _mockLogger = new Mock<ILogger<NewsSocialService>>();
        _mockAccountService = new Mock<IAccountService>();
        _mockCache = new Mock<IDistributedCache>();
        _mockLlmService = new Mock<ILLMService>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private NewsSocialService CreateService(NewsSocialOptions? options = null)
    {
        options ??= new NewsSocialOptions
        {
            NewsApi = new NewsApiOptions { ApiKey = "test-news-api-key", Enabled = true },
            Twitter = new TwitterApiOptions { BearerToken = "test-twitter-bearer", Enabled = true },
            LinkedIn = new LinkedInApiOptions { AccessToken = "test-linkedin-token", Enabled = true },
            EnableSentimentAnalysis = false,
            CacheMinutes = 15
        };

        return new NewsSocialService(
            _mockLogger.Object,
            Options.Create(options),
            _mockAccountService.Object,
            _httpClient,
            _mockCache.Object,
            _mockLlmService.Object);
    }

    #region Configuration Tests

    [Fact]
    public void IsNewsApiConfigured_ShouldReturnTrue_WhenApiKeyIsValid()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.IsNewsApiConfigured();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNewsApiConfigured_ShouldReturnFalse_WhenApiKeyIsEmpty()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            NewsApi = new NewsApiOptions { ApiKey = "", Enabled = true }
        };
        var service = CreateService(options);

        // Act
        var result = service.IsNewsApiConfigured();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNewsApiConfigured_ShouldReturnFalse_WhenApiKeyIsPlaceholder()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            NewsApi = new NewsApiOptions { ApiKey = "${NEWS_API_KEY}", Enabled = true }
        };
        var service = CreateService(options);

        // Act
        var result = service.IsNewsApiConfigured();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNewsApiConfigured_ShouldReturnFalse_WhenDisabled()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            NewsApi = new NewsApiOptions { ApiKey = "valid-key-12345", Enabled = false }
        };
        var service = CreateService(options);

        // Act
        var result = service.IsNewsApiConfigured();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSocialApiConfigured_ShouldReturnTrue_WhenTwitterConfigured()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            Twitter = new TwitterApiOptions { BearerToken = "valid-bearer-token", Enabled = true },
            LinkedIn = new LinkedInApiOptions { AccessToken = "", Enabled = false }
        };
        var service = CreateService(options);

        // Act
        var result = service.IsSocialApiConfigured();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSocialApiConfigured_ShouldReturnTrue_WhenLinkedInConfigured()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            Twitter = new TwitterApiOptions { BearerToken = "", Enabled = false },
            LinkedIn = new LinkedInApiOptions { AccessToken = "valid-access-token", Enabled = true }
        };
        var service = CreateService(options);

        // Act
        var result = service.IsSocialApiConfigured();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSocialApiConfigured_ShouldReturnFalse_WhenNeitherConfigured()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            Twitter = new TwitterApiOptions { BearerToken = "", Enabled = true },
            LinkedIn = new LinkedInApiOptions { AccessToken = "", Enabled = true }
        };
        var service = CreateService(options);

        // Act
        var result = service.IsSocialApiConfigured();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetFeedsAsync Tests

    [Fact]
    public async Task GetFeedsAsync_ShouldReturnCachedResponse_WhenCacheHit()
    {
        // Arrange
        var cachedResponse = new NewsSocialFeedResponse
        {
            NewsItems = new List<NewsItemDto> { new() { Title = "Cached News" } },
            LastUpdated = DateTime.UtcNow.AddMinutes(-5)
        };
        var cachedJson = JsonSerializer.Serialize(cachedResponse);
        var cachedBytes = Encoding.UTF8.GetBytes(cachedJson);

        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var service = CreateService();

        // Act
        var result = await service.GetFeedsAsync(new NewsSocialFeedRequest { CustomerId = 1, CompanyName = "Test" });

        // Assert
        result.IsFromCache.Should().BeTrue();
        result.NewsItems.Should().HaveCount(1);
        result.NewsItems.First().Title.Should().Be("Cached News");
    }

    [Fact]
    public async Task GetFeedsAsync_ShouldBypassCache_WhenRefreshCacheIsTrue()
    {
        // Arrange
        var cachedJson = JsonSerializer.Serialize(new NewsSocialFeedResponse { NewsItems = new List<NewsItemDto>() });
        var cachedBytes = Encoding.UTF8.GetBytes(cachedJson);
        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        SetupNewsApiResponse(new { articles = new List<object>() });

        var service = CreateService();

        // Act
        var result = await service.GetFeedsAsync(new NewsSocialFeedRequest
        {
            CustomerId = 1,
            CompanyName = "Test",
            RefreshCache = true
        });

        // Assert
        result.IsFromCache.Should().BeFalse();
        _mockCache.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetFeedsAsync_ShouldFetchCustomerInfo_WhenCompanyNameNotProvided()
    {
        // Arrange
        _mockAccountService.Setup(x => x.GetAccountByIdAsync(123))
            .ReturnsAsync(new AccountDto
            {
                Id = 123,
                Company = "Acme Corp",
                TwitterHandle = "@acme",
                LinkedInUrl = "https://linkedin.com/company/acme"
            });

        SetupNewsApiResponse(new { articles = new List<object>() });

        var service = CreateService();

        // Act
        await service.GetFeedsAsync(new NewsSocialFeedRequest { CustomerId = 123 });

        // Assert
        _mockAccountService.Verify(x => x.GetAccountByIdAsync(123), Times.Once);
    }

    [Fact]
    public async Task GetFeedsAsync_ShouldHandleCacheReadFailure_Gracefully()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis unavailable"));

        SetupNewsApiResponse(new { articles = new List<object>() });

        var service = CreateService();

        // Act
        var result = await service.GetFeedsAsync(new NewsSocialFeedRequest { CompanyName = "Test" });

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNullOrEmpty();
    }

    #endregion

    #region GetNewsAsync Tests

    [Fact]
    public async Task GetNewsAsync_ShouldReturnNewsItems_WhenApiReturnsResults()
    {
        // Arrange
        var newsResponse = new
        {
            status = "ok",
            totalResults = 2,
            articles = new[]
            {
                new
                {
                    title = "Company Expands Operations",
                    author = "John Doe",
                    url = "https://news.example.com/1",
                    urlToImage = "https://news.example.com/1.jpg",
                    publishedAt = DateTime.UtcNow.AddHours(-1),
                    description = "Company announces expansion",
                    source = new { id = "example", name = "Example News" }
                },
                new
                {
                    title = "Quarterly Results Released",
                    author = "Jane Smith",
                    url = "https://news.example.com/2",
                    urlToImage = (string?)null,
                    publishedAt = DateTime.UtcNow.AddHours(-2),
                    description = "Strong quarterly performance",
                    source = new { id = "finance", name = "Finance Daily" }
                }
            }
        };

        SetupNewsApiResponse(newsResponse);
        var service = CreateService();

        // Act
        var result = await service.GetNewsAsync("Acme Corp", 10);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Company Expands Operations");
        result[0].Author.Should().Be("John Doe");
        result[0].Source.Should().Be("Example News");
        result[0].Sentiment.Should().Be("neutral"); // Default without LLM
    }

    [Fact]
    public async Task GetNewsAsync_ShouldReturnEmpty_WhenApiNotConfigured()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            NewsApi = new NewsApiOptions { ApiKey = "", Enabled = false }
        };
        var service = CreateService(options);

        // Act
        var result = await service.GetNewsAsync("Acme Corp", 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNewsAsync_ShouldReturnEmpty_WhenApiReturnsError()
    {
        // Arrange
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var service = CreateService();

        // Act
        var result = await service.GetNewsAsync("Acme Corp", 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNewsAsync_ShouldRespectMaxItems()
    {
        // Arrange
        var articles = Enumerable.Range(1, 20)
            .Select(i => new
            {
                title = $"Article {i}",
                url = $"https://news.example.com/{i}",
                publishedAt = DateTime.UtcNow,
                source = new { name = "News" }
            })
            .ToArray();

        SetupNewsApiResponse(new { articles });
        var service = CreateService();

        // Act
        var result = await service.GetNewsAsync("Company", 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetNewsAsync_ShouldUrlEncodeCompanyName()
    {
        // Arrange
        string? capturedUrl = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedUrl = req.RequestUri?.ToString())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { articles = new List<object>() }))
            });

        var service = CreateService();

        // Act
        await service.GetNewsAsync("Test & Company Inc.", 10);

        // Assert
        capturedUrl.Should().Contain("Test+%26+Company+Inc.");
    }

    #endregion

    #region GetSocialFeedsAsync Tests

    [Fact]
    public async Task GetSocialFeedsAsync_ShouldReturnEmpty_WhenNoHandlesProvided()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetSocialFeedsAsync(null, null, null, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSocialFeedsAsync_ShouldFetchTwitterFeeds_WhenHandleProvided()
    {
        // Arrange
        SetupTwitterUserResponse("12345", "Acme Corp", "acme");
        SetupTwitterTweetsResponse(new[]
        {
            new { id = "t1", text = "Exciting news!", created_at = DateTime.UtcNow, public_metrics = new { like_count = 10, retweet_count = 5, reply_count = 2 } }
        });

        var service = CreateService();

        // Act
        var result = await service.GetSocialFeedsAsync(null, "@acme", null, 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].Platform.Should().Be("twitter");
        result[0].Content.Should().Be("Exciting news!");
        result[0].AuthorHandle.Should().Be("@acme");
    }

    [Fact]
    public async Task GetSocialFeedsAsync_ShouldCleanTwitterHandle()
    {
        // Arrange
        string? capturedUrl = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/users/by/username/")),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedUrl = req.RequestUri?.ToString())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { data = new { id = "123", name = "Test" } }))
            });

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/tweets")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { data = new List<object>() }))
            });

        var service = CreateService();

        // Act
        await service.GetSocialFeedsAsync(null, "@acme", null, 10);

        // Assert
        capturedUrl.Should().Contain("/acme"); // @ should be removed
        capturedUrl.Should().NotContain("/@");
    }

    [Fact]
    public async Task GetSocialFeedsAsync_ShouldReturnEmpty_WhenTwitterApiNotConfigured()
    {
        // Arrange
        var options = new NewsSocialOptions
        {
            Twitter = new TwitterApiOptions { BearerToken = "", Enabled = true }
        };
        var service = CreateService(options);

        // Act
        var result = await service.GetSocialFeedsAsync(null, "@acme", null, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSocialFeedsAsync_ShouldCalculateEngagementCount()
    {
        // Arrange
        SetupTwitterUserResponse("12345", "Acme", "acme");
        SetupTwitterTweetsResponse(new[]
        {
            new
            {
                id = "t1",
                text = "Test tweet",
                created_at = DateTime.UtcNow,
                public_metrics = new { like_count = 100, retweet_count = 50, reply_count = 25 }
            }
        });

        var service = CreateService();

        // Act
        var result = await service.GetSocialFeedsAsync(null, "@acme", null, 10);

        // Assert
        result[0].LikeCount.Should().Be(100);
        result[0].ShareCount.Should().Be(50);
        result[0].CommentCount.Should().Be(25);
        result[0].EngagementCount.Should().Be(175); // 100 + 50 + 25
    }

    [Fact]
    public async Task GetSocialFeedsAsync_ShouldBuildCorrectTwitterUrl()
    {
        // Arrange
        SetupTwitterUserResponse("12345", "Acme", "acme");
        SetupTwitterTweetsResponse(new[]
        {
            new { id = "tweet123", text = "Test", created_at = DateTime.UtcNow, public_metrics = new { like_count = 0, retweet_count = 0, reply_count = 0 } }
        });

        var service = CreateService();

        // Act
        var result = await service.GetSocialFeedsAsync(null, "@acme", null, 10);

        // Assert
        result[0].Url.Should().Be("https://twitter.com/acme/status/tweet123");
    }

    #endregion

    #region Sentiment Analysis Tests

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnPositive_WhenLLMIndicatesPositive()
    {
        // Arrange
        _mockLlmService.Setup(x => x.CompletionAsync(It.IsAny<LLMRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponse { Success = true, Content = "positive" });

        var service = CreateService();

        // Act
        var result = await service.AnalyzeSentimentAsync("Great news! Company profits soar!");

        // Assert
        result.Should().Be("positive");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnNegative_WhenLLMIndicatesNegative()
    {
        // Arrange
        _mockLlmService.Setup(x => x.CompletionAsync(It.IsAny<LLMRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponse { Success = true, Content = "This is negative sentiment" });

        var service = CreateService();

        // Act
        var result = await service.AnalyzeSentimentAsync("Company faces massive layoffs");

        // Assert
        result.Should().Be("negative");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnNeutral_WhenLLMResponseUnclear()
    {
        // Arrange
        _mockLlmService.Setup(x => x.CompletionAsync(It.IsAny<LLMRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponse { Success = true, Content = "uncertain" });

        var service = CreateService();

        // Act
        var result = await service.AnalyzeSentimentAsync("Company releases quarterly report");

        // Assert
        result.Should().Be("neutral");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnNeutral_WhenLLMNotAvailable()
    {
        // Arrange
        var service = new NewsSocialService(
            _mockLogger.Object,
            Options.Create(new NewsSocialOptions()),
            _mockAccountService.Object,
            _httpClient,
            _mockCache.Object,
            llmService: null);

        // Act
        var result = await service.AnalyzeSentimentAsync("Test text");

        // Assert
        result.Should().Be("neutral");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldReturnNeutral_WhenTextIsEmpty()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.AnalyzeSentimentAsync("");

        // Assert
        result.Should().Be("neutral");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ShouldHandleLLMError_Gracefully()
    {
        // Arrange
        _mockLlmService.Setup(x => x.CompletionAsync(It.IsAny<LLMRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("LLM service unavailable"));

        var service = CreateService();

        // Act
        var result = await service.AnalyzeSentimentAsync("Test text");

        // Assert
        result.Should().Be("neutral");
    }

    #endregion

    #region Helper Methods

    private void SetupNewsApiResponse(object response)
    {
        var json = JsonSerializer.Serialize(response);
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("newsapi.org")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }

    private void SetupTwitterUserResponse(string id, string name, string username)
    {
        var response = new { data = new { id, name, username } };
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/users/by/username/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response))
            });
    }

    private void SetupTwitterTweetsResponse(object[] tweets)
    {
        var response = new { data = tweets };
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/tweets")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response))
            });
    }

    #endregion
}
