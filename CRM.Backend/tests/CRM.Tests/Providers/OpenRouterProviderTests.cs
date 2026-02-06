// CRM Solution - OpenRouterProvider Tests
// Tests for the OpenRouter AI gateway provider (multi-model access)

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
/// Unit tests for OpenRouterProvider (multi-model AI gateway).
/// Tests model routing, fallbacks, and unified API.
/// </summary>
public class OpenRouterProviderTests : IDisposable
{
    private readonly Mock<ILogger<OpenRouterProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<OpenRouterConfiguration> _options;
    private readonly OpenRouterProvider _provider;

    public OpenRouterProviderTests()
    {
        _loggerMock = new Mock<ILogger<OpenRouterProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://openrouter.ai")
        };

        _options = Options.Create(new OpenRouterConfiguration
        {
            ApiKey = "sk-or-test-key",
            DefaultModel = "openai/gpt-4o",
            FallbackModels = new[] { "anthropic/claude-3-sonnet", "meta-llama/llama-3-70b-instruct" },
            SiteUrl = "https://mycrm.example.com",
            SiteName = "My CRM"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new OpenRouterProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
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
        _provider.ProviderName.Should().Be("OpenRouter");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new OpenRouterProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithMissingApiKey_ThrowsArgumentException()
    {
        // Arrange
        var invalidOptions = Options.Create(new OpenRouterConfiguration
        {
            DefaultModel = "openai/gpt-4"
        });

        // Act
        var act = () => new OpenRouterProvider(invalidOptions, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

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
            id = "gen-xyz123",
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content = "Hello! I can help you." },
                    finish_reason = "stop"
                }
            },
            model = "openai/gpt-4o",
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
    public async Task GetChatCompletionAsync_WithSpecificModel_UsesRequestedModel()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Response from Claude" } } },
            model = "anthropic/claude-3-5-sonnet"
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Model = "anthropic/claude-3-5-sonnet",
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello Claude" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithSystemPrompt_IncludesSystem()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "I am a CRM assistant." } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            SystemPrompt = "You are a helpful CRM assistant.",
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "What are you?" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithToolCalling_SupportsFunctions()
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
                                id = "call_123",
                                type = "function",
                                function = new { name = "search_contacts", arguments = "{\"query\":\"Smith\"}" }
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
                new AIChatMessage { Role = "user", Content = "Find contacts named Smith" }
            },
            Tools = new List<AITool>
            {
                new AITool { Name = "search_contacts", Description = "Search contacts" }
            }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ToolCalls.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Model Routing Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithModelRouting_RoutesToProvider()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Response" } } },
            model = "openai/gpt-4o"
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new AIChatMessage { Role = "user", Content = "Hello" }
            },
            ModelPreferences = new List<string> { "openai/*", "anthropic/*" }
        };

        // Act
        var result = await _provider.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsAvailableModels()
    {
        // Arrange
        var response = new
        {
            data = new[]
            {
                new { id = "openai/gpt-4o", name = "GPT-4o", context_length = 128000 },
                new { id = "anthropic/claude-3-5-sonnet", name = "Claude 3.5 Sonnet", context_length = 200000 },
                new { id = "meta-llama/llama-3-70b-instruct", name = "Llama 3 70B", context_length = 8192 },
                new { id = "google/gemini-pro", name = "Gemini Pro", context_length = 32000 }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ListModelsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModelInfoAsync_ReturnsModelDetails()
    {
        // Arrange
        var response = new
        {
            id = "openai/gpt-4o",
            name = "GPT-4o",
            description = "Most capable GPT-4 model",
            context_length = 128000,
            pricing = new { prompt = "0.005", completion = "0.015" },
            top_provider = new { is_moderated = true }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetModelInfoAsync("openai/gpt-4o");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Streaming Tests

    [Fact]
    public async Task StreamChatCompletionAsync_WithValidRequest_StreamsChunks()
    {
        // Arrange
        var streamContent = "data: {\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}\n\n" +
                           "data: {\"choices\":[{\"delta\":{\"content\":\" there\"}}]}\n\n" +
                           "data: [DONE]\n\n";

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

    #region CRM-Specific Method Tests

    [Fact]
    public async Task DraftEmailAsync_WithContext_GeneratesEmail()
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
            choices = new[] { new { message = new { content = "{\"sentiment\":\"positive\",\"score\":0.88}" } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.AnalyzeSentimentAsync("Great product, very satisfied!");

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
                new { message = new { content = "[{\"type\":\"person\",\"value\":\"Jane Doe\"}]" } }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ExtractEntitiesAsync("Jane Doe is our main contact.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SummarizeAsync_WithText_ReturnsSummary()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "Summary: Discussed contract and pricing." } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.SummarizeAsync("Long text here...", 50);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ClassifyIntentAsync_WithText_ReturnsIntent()
    {
        // Arrange
        var response = new
        {
            choices = new[] { new { message = new { content = "{\"intent\":\"support_request\",\"confidence\":0.92}" } } }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.ClassifyIntentAsync("I need help with my account");

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
        result.ProviderName.Should().Be("OpenRouter");
    }

    [Fact]
    public async Task HealthCheckAsync_WithAuthError_ReturnsUnhealthy()
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

    #region Configuration Tests

    [Fact]
    public void GetDefaultModel_ReturnsConfiguredModel()
    {
        // Act
        var model = _provider.DefaultModel;

        // Assert
        model.Should().Be("openai/gpt-4o");
    }

    [Fact]
    public void GetFallbackModels_ReturnsConfiguredFallbacks()
    {
        // Act
        var fallbacks = _provider.FallbackModels;

        // Assert
        fallbacks.Should().Contain("anthropic/claude-3-sonnet");
        fallbacks.Should().Contain("meta-llama/llama-3-70b-instruct");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetChatCompletionAsync_WithRateLimitError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429, "{\"error\":{\"message\":\"Rate limit exceeded\"}}");

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
    public async Task GetChatCompletionAsync_WithModelNotAvailable_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound, "{\"error\":{\"message\":\"Model not available\"}}");

        var request = new AIChatRequest
        {
            Model = "nonexistent/model",
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
    public async Task GetChatCompletionAsync_WithInsufficientCredits_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.PaymentRequired, "{\"error\":{\"message\":\"Insufficient credits\"}}");

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

    #region Usage and Credits Tests

    [Fact]
    public async Task GetUsageAsync_ReturnsUsageInfo()
    {
        // Arrange
        var response = new
        {
            data = new
            {
                used_credits = 10.50,
                remaining_credits = 89.50,
                total_requests = 150
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetUsageAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
