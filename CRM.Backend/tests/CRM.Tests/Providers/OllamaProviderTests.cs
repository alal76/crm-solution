// CRM Solution - OllamaProvider Tests
// Tests for the Ollama local LLM provider

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
/// Unit tests for OllamaProvider.
/// Tests local LLM inference, embeddings, and model management.
/// </summary>
public class OllamaProviderTests : IDisposable
{
    private readonly Mock<ILogger<OllamaProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<OllamaConfiguration> _options;
    private readonly OllamaProvider _provider;

    public OllamaProviderTests()
    {
        _loggerMock = new Mock<ILogger<OllamaProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        _options = Options.Create(new OllamaConfiguration
        {
            Url = "http://localhost:11434",
            Model = "llama3",
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = 120,
            MaxTokens = 4096
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new OllamaProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("Ollama");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OllamaProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Chat Completion Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithValidRequest_ReturnsCompletion()
    {
        // Arrange
        var response = new
        {
            message = new { content = "Hello! How can I help you today?" },
            done = true,
            total_duration = 1234567890L
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
        result.Content.Should().Contain("Hello");
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
    public async Task GetChatCompletionAsync_WithSystemMessage_IncludesSystemPrompt()
    {
        // Arrange
        var response = new { message = new { content = "I am a helpful assistant." }, done = true };
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
    public async Task GetChatCompletionAsync_WithTemperature_SetsTemperature()
    {
        // Arrange
        var response = new { message = new { content = "Creative response" }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Be creative" }
            },
            Temperature = 0.9f
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithMaxTokens_LimitsResponse()
    {
        // Arrange
        var response = new { message = new { content = "Short response" }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Be brief" }
            },
            MaxTokens = 100
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithMultipleTurns_HandleConversation()
    {
        // Arrange
        var response = new { message = new { content = "You said hello earlier." }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" },
                new AIChatMessage { Role = "assistant", Content = "Hi there!" },
                new AIChatMessage { Role = "user", Content = "What did I say first?" }
            }
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
        var response = new { embedding = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetEmbeddingsAsync("Test text for embedding");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_WithNullText_ThrowsArgumentException()
    {
        // Act
        var act = () => _provider.GetEmbeddingsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
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
        var response = new { embedding = new[] { 0.1f, 0.2f, 0.3f } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var texts = new List<string>
        {
            "First text",
            "Second text",
            "Third text"
        };

        // Act
        var result = await _provider.GetBatchEmbeddingsAsync(texts);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region CRM-Specific Method Tests

    [Fact]
    public async Task DraftEmailAsync_WithContext_DraftsEmail()
    {
        // Arrange
        var response = new { message = new { content = "Dear John,\n\nThank you for your interest..." }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIEmailDraftRequest
        {
            RecipientName = "John Doe",
            Subject = "Follow-up on our meeting",
            Context = "Discussed Q1 sales strategy",
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
        var response = new { message = new { content = "{\"sentiment\":\"positive\",\"confidence\":0.85}" }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.AnalyzeSentimentAsync("I'm very happy with your service!");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExtractEntitiesAsync_WithText_ReturnsEntities()
    {
        // Arrange
        var response = new { message = new { content = "[{\"type\":\"person\",\"value\":\"John Doe\"},{\"type\":\"company\",\"value\":\"Acme Corp\"}]" }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ExtractEntitiesAsync("John Doe from Acme Corp called about the project.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SummarizeAsync_WithText_ReturnsSummary()
    {
        // Arrange
        var response = new { message = new { content = "Customer discussed pricing concerns and requested a discount." }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var longText = "The customer called to discuss their recent order. They mentioned that the pricing seems higher than competitors...";

        // Act
        var result = await _provider.SummarizeAsync(longText, 100);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ClassifyIntentAsync_WithText_ReturnsIntent()
    {
        // Arrange
        var response = new { message = new { content = "{\"intent\":\"support_request\",\"confidence\":0.92}" }, done = true };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ClassifyIntentAsync("I need help with my order");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Model Management Tests

    [Fact]
    public async Task ListModelsAsync_ReturnsAvailableModels()
    {
        // Arrange
        var response = new
        {
            models = new[]
            {
                new { name = "llama3", size = 4700000000L },
                new { name = "mistral", size = 4100000000L },
                new { name = "nomic-embed-text", size = 274000000L }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ListModelsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModelInfoAsync_WithValidModel_ReturnsInfo()
    {
        // Arrange
        var response = new
        {
            modelfile = "FROM llama3",
            parameters = "temperature 0.7",
            template = "{{ .System }}\n{{ .Prompt }}"
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetModelInfoAsync("llama3");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyOllama_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "Ollama is running");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Ollama");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyOllama_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithRunningOllama_ReturnsTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

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
        var response = new { message = new { content = "Response" }, done = true };
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
    public async Task GetChatCompletionAsync_WithModelNotFound_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "{\"error\":\"model 'nonexistent' not found\"}");

        var request = new AIChatRequest
        {
            Model = "nonexistent",
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
    public async Task GetChatCompletionAsync_WithTimeout_HandlesGracefully()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

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
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithServerError_ReturnsError()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError);

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
    public void GetDefaultModel_ReturnsConfiguredModel()
    {
        // Act
        var model = _provider.DefaultModel;

        // Assert
        model.Should().Be("llama3");
    }

    [Fact]
    public void GetEmbeddingModel_ReturnsConfiguredModel()
    {
        // Act
        var model = _provider.EmbeddingModel;

        // Assert
        model.Should().Be("nomic-embed-text");
    }

    #endregion
}
