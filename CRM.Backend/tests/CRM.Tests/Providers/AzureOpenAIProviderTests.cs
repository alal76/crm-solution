// CRM Solution - AzureOpenAIProvider Tests
// Tests for the Azure OpenAI LLM provider

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
using CRM.Infrastructure.Providers.AI;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for AzureOpenAIProvider.
/// Tests Azure OpenAI deployments, chat completions, and embeddings.
/// </summary>
public class AzureOpenAIProviderTests : IDisposable
{
    private readonly Mock<ILogger<AzureOpenAIProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<AzureOpenAIConfiguration> _options;
    private readonly AzureOpenAIProvider _provider;

    public AzureOpenAIProviderTests()
    {
        _loggerMock = new Mock<ILogger<AzureOpenAIProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://test-resource.openai.azure.com")
        };

        _options = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test-resource.openai.azure.com",
            ApiKey = "test-api-key",
            DeploymentName = "gpt-4o",
            EmbeddingDeploymentName = "text-embedding-ada-002",
            ApiVersion = "2024-02-01",
            MaxTokens = 4096
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new AzureOpenAIProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{}")
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
        _provider.ProviderName.Should().Be("AzureOpenAI");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AzureOpenAIProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithMissingEndpoint_ThrowsArgumentException()
    {
        // Arrange
        var invalidOptions = Options.Create(new AzureOpenAIConfiguration
        {
            ApiKey = "key",
            DeploymentName = "deployment"
        });

        // Act
        var act = () => new AzureOpenAIProvider(invalidOptions, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Chat Completion Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithValidRequest_ReturnsCompletion()
    {
        // Arrange
        var response = new
        {
            choices = new[]
            {
                new { message = new { content = "Hello! I'm an AI assistant." } }
            },
            usage = new { prompt_tokens = 10, completion_tokens = 20, total_tokens = 30 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.GetChatCompletionAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithSystemPrompt_IncludesSystemMessage()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "I am a CRM assistant." } } },
            usage = new { prompt_tokens = 20, completion_tokens = 10, total_tokens = 30 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            SystemPrompt = "You are a helpful CRM assistant.",
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Who are you?" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithFunctionCalling_SupportsTools()
    {
        // Arrange
        var response = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = (string)null!,
                        tool_calls = new[]
                        {
                            new
                            {
                                id = "call_abc123",
                                type = "function",
                                function = new { name = "get_weather", arguments = "{\"location\":\"London\"}" }
                            }
                        }
                    },
                    finish_reason = "tool_calls"
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "What's the weather in London?" }
            },
            Tools = new List<AITool>
            {
                new AITool
                {
                    Name = "get_weather",
                    Description = "Get current weather",
                    Parameters = new Dictionary<string, object>
                    {
                        ["location"] = new { type = "string", description = "City name" }
                    }
                }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ToolCalls.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithTemperatureAndTopP_SetsParameters()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Creative output" } } },
            usage = new { total_tokens = 50 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Be creative" }
            },
            Temperature = 0.9f,
            TopP = 0.95f
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Embedding Tests

    [Fact]
    public async Task GetEmbeddingsAsync_WithValidText_ReturnsEmbedding()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { embedding = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f } }
            },
            usage = new { prompt_tokens = 5, total_tokens = 5 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetEmbeddingsAsync("Test text for embedding");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_WithEmptyText_ThrowsArgumentException()
    {
        // Act
        var act = () => _provider.GetEmbeddingsAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetBatchEmbeddingsAsync_WithMultipleTexts_ReturnsEmbeddings()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { index = 0, embedding = new[] { 0.1f, 0.2f } },
                new { index = 1, embedding = new[] { 0.3f, 0.4f } },
                new { index = 2, embedding = new[] { 0.5f, 0.6f } }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var texts = new List<string> { "Text 1", "Text 2", "Text 3" };

        // Act
        var result = await _provider.GetBatchEmbeddingsAsync(texts);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    #endregion

    #region CRM-Specific Method Tests

    [Fact]
    public async Task DraftEmailAsync_WithContext_DraftsEmail()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Dear Customer,\n\nThank you for reaching out..." } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIEmailDraftRequest
        {
            RecipientName = "John Doe",
            Subject = "Re: Your inquiry",
            Context = "Customer asked about pricing",
            Tone = "professional"
        };

        // Act
        var result = await _provider.DraftEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_WithText_ReturnsSentiment()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "{\"sentiment\":\"positive\",\"score\":0.92}" } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.AnalyzeSentimentAsync("I love your product! It's amazing!");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExtractEntitiesAsync_WithText_ReturnsEntities()
    {
        // Arrange
        var response = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "[{\"type\":\"person\",\"value\":\"John Smith\"},{\"type\":\"organization\",\"value\":\"Acme Inc\"}]"
                    }
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ExtractEntitiesAsync("John Smith from Acme Inc is our new contact.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SummarizeAsync_WithLongText_ReturnsSummary()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Customer discussed contract renewal and pricing." } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var longText = "In today's meeting, we discussed the upcoming contract renewal. The customer expressed concerns about...";

        // Act
        var result = await _provider.SummarizeAsync(longText, 50);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ClassifyIntentAsync_WithText_ReturnsIntent()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "{\"intent\":\"billing_inquiry\",\"confidence\":0.89}" } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ClassifyIntentAsync("I have a question about my invoice");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyEndpoint_ReturnsHealthy()
    {
        // Arrange
        var response = new { choices = new[] { new { message = new { content = "OK" } } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("AzureOpenAI");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnauthorized_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"error\":{\"code\":\"401\",\"message\":\"Access denied\"}}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyEndpoint_ReturnsTrue()
    {
        // Arrange
        var response = new { choices = new[] { new { message = new { content = "OK" } } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var response = new { choices = new[] { new { message = new { content = "Response" } } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Test" }
            }
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.GetChatCompletionAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithRateLimitExceeded_ReturnsError()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, "{\"error\":{\"code\":\"429\",\"message\":\"Rate limit exceeded\"}}");

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var act = () => _provider.GetChatCompletionAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithContentFilterTriggered_ReturnsError()
    {
        // Arrange
        var response = new
        {
            choices = new[]
            {
                new
                {
                    finish_reason = "content_filter",
                    message = new { content = (string)null! }
                }
            },
            error = new { code = "content_filter", message = "Content was filtered" }
        };
        SetupHttpResponse(HttpStatusCode.BadRequest, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Inappropriate content" }
            }
        };

        // Act
        var act = () => _provider.GetChatCompletionAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithQuotaExceeded_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Forbidden, "{\"error\":{\"code\":\"quota_exceeded\",\"message\":\"Quota exceeded\"}}");

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var act = () => _provider.GetChatCompletionAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void GetDeploymentName_ReturnsConfiguredDeployment()
    {
        // Act
        var deployment = _provider.DeploymentName;

        // Assert
        deployment.Should().Be("gpt-4o");
    }

    [Fact]
    public void GetEndpoint_ReturnsConfiguredEndpoint()
    {
        // Act
        var endpoint = _provider.Endpoint;

        // Assert
        endpoint.Should().Be("https://test-resource.openai.azure.com");
    }

    #endregion
}
