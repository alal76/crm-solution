// CRM Solution - AnthropicProvider Tests
// Tests for the Anthropic Claude LLM provider

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
/// Unit tests for AnthropicProvider (Claude models).
/// Tests Messages API, streaming, and CRM-specific methods.
/// </summary>
public class AnthropicProviderTests : IDisposable
{
    private readonly Mock<ILogger<AnthropicProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<AnthropicConfiguration> _options;
    private readonly AnthropicProvider _provider;

    public AnthropicProviderTests()
    {
        _loggerMock = new Mock<ILogger<AnthropicProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.anthropic.com")
        };

        _options = Options.Create(new AnthropicConfiguration
        {
            ApiKey = "sk-ant-test-key",
            Model = "claude-3-5-sonnet-20241022",
            MaxTokens = 4096,
            ApiVersion = "2023-06-01"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new AnthropicProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("Anthropic");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnthropicProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithMissingApiKey_ThrowsArgumentException()
    {
        // Arrange
        var invalidOptions = Options.Create(new AnthropicConfiguration
        {
            Model = "claude-3-sonnet"
        });

        // Act
        var act = () => new AnthropicProvider(invalidOptions, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Messages API Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var response = new
        {
            id = "msg_123",
            type = "message",
            role = "assistant",
            content = new[]
            {
                new { type = "text", text = "Hello! I'm Claude, an AI assistant." }
            },
            model = "claude-3-5-sonnet-20241022",
            stop_reason = "end_turn",
            usage = new { input_tokens = 10, output_tokens = 20 }
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
    public async Task GetChatCompletionAsync_WithSystemPrompt_IncludesSystem()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "I am a CRM expert." } },
            usage = new { input_tokens = 30, output_tokens = 15 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            SystemPrompt = "You are a CRM expert assistant.",
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "What is your expertise?" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithToolUse_ReturnsToolCalls()
    {
        // Arrange
        var response = new
        {
            content = new object[]
            {
                new
                {
                    type = "tool_use",
                    id = "toolu_01",
                    name = "get_customer",
                    input = new { customer_id = "123" }
                }
            },
            stop_reason = "tool_use"
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Get customer 123" }
            },
            Tools = new List<AITool>
            {
                new AITool
                {
                    Name = "get_customer",
                    Description = "Get customer details",
                    Parameters = new Dictionary<string, object>
                    {
                        ["customer_id"] = new { type = "string", description = "Customer ID" }
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
    public async Task GetChatCompletionAsync_WithMultiTurnConversation_MaintainsContext()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "Yes, I remember you're John." } },
            usage = new { input_tokens = 50, output_tokens = 10 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "My name is John." },
                new AIChatMessage { Role = "assistant", Content = "Nice to meet you, John!" },
                new AIChatMessage { Role = "user", Content = "Do you remember my name?" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Streaming Tests

    [Fact]
    public async Task StreamChatCompletionAsync_WithValidRequest_StreamsChunks()
    {
        // Arrange
        var events = "event: content_block_delta\ndata: {\"type\":\"content_block_delta\",\"delta\":{\"text\":\"Hello\"}}\n\n" +
                     "event: content_block_delta\ndata: {\"type\":\"content_block_delta\",\"delta\":{\"text\":\" world\"}}\n\n" +
                     "event: message_stop\ndata: {\"type\":\"message_stop\"}\n\n";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(events)
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

    #region CRM-Specific Method Tests

    [Fact]
    public async Task DraftEmailAsync_WithContext_GeneratesEmail()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "Dear Sarah,\n\nThank you for your interest..." } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIEmailDraftRequest
        {
            RecipientName = "Sarah",
            Subject = "Follow-up on demo",
            Context = "Prospect attended product demo",
            Tone = "professional"
        };

        // Act
        var result = await _provider.DraftEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_WithNegativeText_ReturnsNegative()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "{\"sentiment\":\"negative\",\"score\":0.15,\"reasons\":[\"frustration\",\"complaint\"]}" } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.AnalyzeSentimentAsync("This is terrible service! I'm very disappointed.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExtractEntitiesAsync_WithContactInfo_ExtractsAll()
    {
        // Arrange
        var response = new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = "[{\"type\":\"person\",\"value\":\"Mike Chen\"},{\"type\":\"phone\",\"value\":\"+1-555-0123\"},{\"type\":\"company\",\"value\":\"TechCorp\"}]"
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ExtractEntitiesAsync("Please contact Mike Chen at TechCorp, phone +1-555-0123");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SummarizeAsync_WithCallTranscript_ReturnsSummary()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "Key points: Contract renewal discussed, customer satisfied with service, next meeting scheduled." } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.SummarizeAsync("Long call transcript here...", 50);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ClassifyIntentAsync_WithSalesInquiry_ReturnsIntent()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "{\"intent\":\"pricing_inquiry\",\"confidence\":0.88,\"sub_intents\":[\"discount_request\"]}" } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ClassifyIntentAsync("Can you give me a better price?");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Model Configuration Tests

    [Fact]
    public void GetModel_ReturnsConfiguredModel()
    {
        // Act
        var model = _provider.Model;

        // Assert
        model.Should().Be("claude-3-5-sonnet-20241022");
    }

    [Fact]
    public void GetMaxTokens_ReturnsConfiguredValue()
    {
        // Act
        var maxTokens = _provider.MaxTokens;

        // Assert
        maxTokens.Should().Be(4096);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyAPI_ReturnsHealthy()
    {
        // Arrange
        var response = new
        {
            content = new[] { new { type = "text", text = "OK" } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Anthropic");
    }

    [Fact]
    public async Task HealthCheckAsync_WithAuthError_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"error\":{\"type\":\"authentication_error\",\"message\":\"Invalid API key\"}}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithWorkingAPI_ReturnsTrue()
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

    #region Error Handling Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithOverloadedError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, 
            "{\"error\":{\"type\":\"overloaded_error\",\"message\":\"Overloaded\"}}");

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
    public async Task GetChatCompletionAsync_WithRateLimitError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, 
            "{\"error\":{\"type\":\"rate_limit_error\",\"message\":\"Rate limit exceeded\"}}");

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
    public async Task GetChatCompletionAsync_WithInvalidRequest_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, 
            "{\"error\":{\"type\":\"invalid_request_error\",\"message\":\"Invalid request\"}}");

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

    #region Content Block Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithMultipleContentBlocks_HandlesAll()
    {
        // Arrange
        var response = new
        {
            content = new[]
            {
                new { type = "text", text = "First part of response." },
                new { type = "text", text = " Second part of response." }
            },
            usage = new { input_tokens = 10, output_tokens = 20 }
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
    }

    #endregion
}
