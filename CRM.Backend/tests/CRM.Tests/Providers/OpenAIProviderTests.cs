// CRM Solution - OpenAIProvider Tests
// Tests for the OpenAI LLM provider (direct API)

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
/// Unit tests for OpenAIProvider (direct OpenAI API).
/// Tests chat completions, embeddings, and CRM-specific methods.
/// </summary>
public class OpenAIProviderTests : IDisposable
{
    private readonly Mock<ILogger<OpenAIProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<OpenAIConfiguration> _options;
    private readonly OpenAIProvider _provider;

    public OpenAIProviderTests()
    {
        _loggerMock = new Mock<ILogger<OpenAIProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.openai.com")
        };

        _options = Options.Create(new OpenAIConfiguration
        {
            ApiKey = "sk-test-api-key",
            Model = "gpt-4o",
            EmbeddingModel = "text-embedding-ada-002",
            MaxTokens = 4096,
            OrganizationId = "org-test"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new OpenAIProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OpenAIProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithMissingApiKey_ThrowsArgumentException()
    {
        // Arrange
        var invalidOptions = Options.Create(new OpenAIConfiguration
        {
            Model = "gpt-4"
        });

        // Act
        var act = () => new OpenAIProvider(invalidOptions, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

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
            id = "chatcmpl-abc123",
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new { role = "assistant", content = "Hello! How can I help you?" },
                    finish_reason = "stop"
                }
            },
            usage = new { prompt_tokens = 10, completion_tokens = 15, total_tokens = 25 }
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
    public async Task GetChatCompletionAsync_WithMultipleMessages_MaintainsConversation()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Your order status is..." } } },
            usage = new { total_tokens = 100 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" },
                new AIChatMessage { Role = "assistant", Content = "Hi there!" },
                new AIChatMessage { Role = "user", Content = "What's my order status?" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithJSONMode_RequestsStructuredOutput()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "{\"key\":\"value\"}" } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Return JSON" }
            },
            ResponseFormat = "json_object"
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithToolChoice_ForcesToolUsage()
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
                                id = "call_xyz",
                                type = "function",
                                function = new { name = "search_customers", arguments = "{\"query\":\"Acme\"}" }
                            }
                        }
                    }
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Find Acme company" }
            },
            Tools = new List<AITool>
            {
                new AITool { Name = "search_customers", Description = "Search customers" }
            },
            ToolChoice = "required"
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Streaming Tests

    [Fact]
    public async Task StreamChatCompletionAsync_WithValidRequest_ReturnsStream()
    {
        // Arrange
        var chunk1 = "data: {\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}\n\n";
        var chunk2 = "data: {\"choices\":[{\"delta\":{\"content\":\" world\"}}]}\n\n";
        var chunk3 = "data: [DONE]\n\n";
        var streamContent = chunk1 + chunk2 + chunk3;

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

    #region Embedding Tests

    [Fact]
    public async Task GetEmbeddingsAsync_WithText_ReturnsVector()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { embedding = new[] { 0.01f, 0.02f, 0.03f, 0.04f } }
            },
            model = "text-embedding-ada-002",
            usage = new { prompt_tokens = 5, total_tokens = 5 }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetEmbeddingsAsync("Test embedding text");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetBatchEmbeddingsAsync_WithTexts_ReturnsVectors()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { index = 0, embedding = new[] { 0.1f, 0.2f } },
                new { index = 1, embedding = new[] { 0.3f, 0.4f } }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetBatchEmbeddingsAsync(new List<string> { "Text 1", "Text 2" });

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region CRM-Specific Method Tests

    [Fact]
    public async Task DraftEmailAsync_WithRequest_GeneratesEmail()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Dear John,\n\nThank you for your inquiry..." } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIEmailDraftRequest
        {
            RecipientName = "John",
            Subject = "Re: Product inquiry",
            Context = "Customer asked about pricing",
            Tone = "friendly"
        };

        // Act
        var result = await _provider.DraftEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_WithPositiveText_ReturnsPositive()
    {
        // Arrange
        var response = new
        {
            choices = new[]
            {
                new { message = new { content = "{\"sentiment\":\"positive\",\"score\":0.95,\"confidence\":0.98}" } }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.AnalyzeSentimentAsync("I absolutely love this product!");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExtractEntitiesAsync_WithBusinessText_ExtractsEntities()
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
                        content = "[{\"type\":\"person\",\"value\":\"Sarah Johnson\"},{\"type\":\"email\",\"value\":\"sarah@acme.com\"}]"
                    }
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ExtractEntitiesAsync("Contact Sarah Johnson at sarah@acme.com");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SummarizeAsync_WithMeetingNotes_ReturnsSummary()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Meeting covered contract terms and next steps." } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.SummarizeAsync("Long meeting notes here...", 100);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ClassifyIntentAsync_WithSupportQuery_ReturnsIntent()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "{\"intent\":\"technical_support\",\"confidence\":0.91}" } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ClassifyIntentAsync("My software keeps crashing");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateResponseSuggestionsAsync_WithConversation_ReturnsSuggestions()
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
                        content = "[\"I'd be happy to help with that.\",\"Let me check that for you.\",\"Could you provide more details?\"]"
                    }
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var conversationHistory = new List<AIChatMessage>
        {
            new AIChatMessage { Role = "user", Content = "I have a problem" }
        };

        // Act
        var result = await _provider.GenerateResponseSuggestionsAsync(conversationHistory, 3);

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
        model.Should().Be("gpt-4o");
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsAvailableModels()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { id = "gpt-4", created = 1687882410 },
                new { id = "gpt-4-turbo", created = 1699987654 },
                new { id = "gpt-3.5-turbo", created = 1677610602 }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ListModelsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyAPI_ReturnsHealthy()
    {
        // Arrange
        var response = new { choices = new[] { new { message = new { content = "OK" } } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public async Task HealthCheckAsync_WithInvalidApiKey_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized, "{\"error\":{\"message\":\"Invalid API key\"}}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithWorkingAPI_ReturnsTrue()
    {
        // Arrange
        var response = new { choices = new[] { new { message = new { content = "OK" } } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var available = await _provider.IsAvailableAsync();

        // Assert
        available.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithRateLimitError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, "{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_error\"}}");

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
    public async Task GetChatCompletionAsync_WithServerError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "{\"error\":{\"message\":\"Server error\"}}");

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
    public async Task GetChatCompletionAsync_WithContextLengthExceeded_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, 
            "{\"error\":{\"message\":\"Context length exceeded\",\"type\":\"invalid_request_error\"}}");

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = new string('a', 100000) }
            }
        };

        // Act
        var act = () => _provider.GetChatCompletionAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithCancelledToken_ThrowsOperationCancelledException()
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
}
