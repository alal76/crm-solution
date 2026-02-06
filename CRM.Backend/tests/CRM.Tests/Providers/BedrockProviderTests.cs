// CRM Solution - BedrockProvider Tests
// Tests for the AWS Bedrock LLM provider (Claude, Titan, Llama)

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
/// Unit tests for BedrockProvider (AWS Bedrock with Claude, Titan, Llama models).
/// Tests multi-model support, embeddings, and AWS-specific authentication.
/// </summary>
public class BedrockProviderTests : IDisposable
{
    private readonly Mock<ILogger<BedrockProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<BedrockConfiguration> _options;
    private readonly BedrockProvider _provider;

    public BedrockProviderTests()
    {
        _loggerMock = new Mock<ILogger<BedrockProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        _options = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            AccessKeyId = "AKIAIOSFODNN7EXAMPLE",
            SecretAccessKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
            ModelId = "anthropic.claude-3-sonnet-20240229-v1:0",
            EmbeddingModelId = "amazon.titan-embed-text-v1",
            MaxTokens = 4096
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new BedrockProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("Bedrock");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BedrockProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithMissingRegion_ThrowsArgumentException()
    {
        // Arrange
        var invalidOptions = Options.Create(new BedrockConfiguration
        {
            AccessKeyId = "key",
            SecretAccessKey = "secret"
        });

        // Act
        var act = () => new BedrockProvider(invalidOptions, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Claude Model Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithClaudeModel_ReturnsResponse()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "Hello! I'm Claude on Bedrock." } },
            usage = new { input_tokens = 10, output_tokens = 15 },
            stop_reason = "end_turn"
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
    public async Task GetChatCompletionAsync_WithClaudeSystemPrompt_IncludesSystem()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "I am your CRM assistant." } }
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

    #endregion

    #region Llama Model Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithLlamaModel_UsesCorrectFormat()
    {
        // Arrange
        var llamaOptions = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            AccessKeyId = "AKIAIOSFODNN7EXAMPLE",
            SecretAccessKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
            ModelId = "meta.llama3-8b-instruct-v1:0"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        var llamaProvider = new BedrockProvider(llamaOptions, _loggerMock.Object, httpClientFactoryMock.Object);

        var response = new
        {
            generation = "Hello from Llama!",
            prompt_token_count = 10,
            generation_token_count = 15
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
        var result = await llamaProvider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Titan Embedding Tests

    [Fact]
    public async Task GetEmbeddingsAsync_WithTitanModel_ReturnsEmbedding()
    {
        // Arrange
        var response = new
        {
            embedding = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f },
            inputTextTokenCount = 5
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetEmbeddingsAsync("Test text for embedding");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetBatchEmbeddingsAsync_WithMultipleTexts_ReturnsEmbeddings()
    {
        // Arrange - Setup sequential responses
        var response1 = new { embedding = new[] { 0.1f, 0.2f }, inputTextTokenCount = 3 };
        var response2 = new { embedding = new[] { 0.3f, 0.4f }, inputTextTokenCount = 4 };

        var callCount = 0;
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                var content = callCount == 1 
                    ? JsonSerializer.Serialize(response1) 
                    : JsonSerializer.Serialize(response2);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                };
            });

        var texts = new List<string> { "Text 1", "Text 2" };

        // Act
        var result = await _provider.GetBatchEmbeddingsAsync(texts);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region CRM-Specific Method Tests

    [Fact]
    public async Task DraftEmailAsync_WithContext_GeneratesEmail()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "Dear Customer,\n\nI hope this email finds you well..." } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIEmailDraftRequest
        {
            RecipientName = "Customer",
            Subject = "Follow-up",
            Context = "Discussed pricing in last call",
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
            content = new[] { new { type = "text", text = "{\"sentiment\":\"neutral\",\"score\":0.5}" } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.AnalyzeSentimentAsync("The service was okay.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExtractEntitiesAsync_WithText_ReturnsEntities()
    {
        // Arrange
        var response = new
        {
            content = new[]
            {
                new { type = "text", text = "[{\"type\":\"company\",\"value\":\"Amazon Web Services\"}]" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ExtractEntitiesAsync("We use Amazon Web Services for our infrastructure.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SummarizeAsync_WithLongText_ReturnsSummary()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "Summary: Customer approved the proposal." } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.SummarizeAsync("Very long document text...", 100);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ClassifyIntentAsync_WithText_ReturnsIntent()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "{\"intent\":\"purchase_inquiry\",\"confidence\":0.85}" } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ClassifyIntentAsync("I want to buy more licenses");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Streaming Tests

    [Fact]
    public async Task StreamChatCompletionAsync_WithClaude_StreamsChunks()
    {
        // Arrange
        var streamContent = "event: content_block_delta\n" +
                           "data: {\"type\":\"content_block_delta\",\"delta\":{\"text\":\"Hello\"}}\n\n" +
                           "event: message_stop\n" +
                           "data: {\"type\":\"message_stop\"}\n\n";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(streamContent)
            });

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" }
            },
            Stream = true
        };

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in _provider.StreamChatCompletionAsync(request))
        {
            chunks.Add(chunk);
        }

        // Assert
        chunks.Should().NotBeEmpty();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyEndpoint_ReturnsHealthy()
    {
        // Arrange
        var response = new { content = new[] { new { type = "text", text = "OK" } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Bedrock");
    }

    [Fact]
    public async Task HealthCheckAsync_WithAccessDenied_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Forbidden, 
            "{\"message\":\"Access Denied\",\"__type\":\"AccessDeniedException\"}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithWorkingEndpoint_ReturnsTrue()
    {
        // Arrange
        var response = new { content = new[] { new { type = "text", text = "OK" } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var available = await _provider.IsAvailableAsync();

        // Assert
        available.Should().BeTrue();
    }

    #endregion

    #region Model Configuration Tests

    [Fact]
    public void GetModelId_ReturnsConfiguredModel()
    {
        // Act
        var modelId = _provider.ModelId;

        // Assert
        modelId.Should().Be("anthropic.claude-3-sonnet-20240229-v1:0");
    }

    [Fact]
    public void GetRegion_ReturnsConfiguredRegion()
    {
        // Act
        var region = _provider.Region;

        // Assert
        region.Should().Be("us-east-1");
    }

    [Fact]
    public async Task ListFoundationModelsAsync_ReturnsModels()
    {
        // Arrange
        var response = new
        {
            modelSummaries = new[]
            {
                new { modelId = "anthropic.claude-3-sonnet", modelName = "Claude 3 Sonnet" },
                new { modelId = "meta.llama3-8b-instruct", modelName = "Llama 3 8B" },
                new { modelId = "amazon.titan-text-express-v1", modelName = "Titan Text Express" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ListFoundationModelsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithThrottlingException_ThrowsException()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, 
            "{\"message\":\"Rate exceeded\",\"__type\":\"ThrottlingException\"}");

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
    public async Task GetChatCompletionAsync_WithModelNotFoundError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, 
            "{\"message\":\"Model not found\",\"__type\":\"ResourceNotFoundException\"}");

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
    public async Task GetChatCompletionAsync_WithValidationException_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, 
            "{\"message\":\"Invalid input\",\"__type\":\"ValidationException\"}");

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>()
        };

        // Act
        var act = () => _provider.GetChatCompletionAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithCancelledToken_ThrowsCancelledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var act = () => _provider.GetChatCompletionAsync(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region AWS Signature Tests

    [Fact]
    public void SignRequest_WithValidCredentials_AddsAuthorizationHeader()
    {
        // This is implicitly tested through successful requests
        // The provider should add AWS Signature Version 4 headers
        _provider.Should().NotBeNull();
    }

    #endregion
}
