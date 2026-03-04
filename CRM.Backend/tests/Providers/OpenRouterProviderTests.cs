// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for OpenRouterProvider.
/// MANDATORY: Written after verifying source signature:
/// Class: OpenRouterProvider, Namespace: CRM.Infrastructure.Providers.AI
/// Constructor: (HttpClient, IOptions&lt;OpenRouterConfiguration&gt;, ILogger&lt;OpenRouterProvider&gt;)
/// </summary>
public class OpenRouterProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static OpenRouterConfiguration DefaultConfig() => new()
    {
        ApiKey = "sk-or-test-key-0000000000000000000",
        BaseUrl = "https://openrouter.ai/api/v1",
        DefaultModel = "openai/gpt-4o-mini",
        DefaultMaxTokens = 4096,
        DefaultTemperature = 0.7,
        SiteName = "CRM Solution",
        SiteUrl = "https://crm.example.com"
    };

    private static OpenRouterProvider CreateProvider(
        HttpStatusCode responseStatus = HttpStatusCode.OK,
        string? responseBody = null,
        OpenRouterConfiguration? config = null)
    {
        var body = responseBody ?? BuildChatResponseJson("Hello from OpenRouter!");
        var handler = new FixedResponseHandler(responseStatus, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://openrouter.ai/api/v1") };

        var options = Options.Create(config ?? DefaultConfig());
        var logger = new Mock<ILogger<OpenRouterProvider>>().Object;
        return new OpenRouterProvider(httpClient, options, logger);
    }

    private static string BuildChatResponseJson(string content, string model = "openai/gpt-4o-mini") =>
        JsonSerializer.Serialize(new
        {
            id = "gen-001",
            model,
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new { role = "assistant", content },
                    finish_reason = "stop"
                }
            },
            usage = new { prompt_tokens = 10, completion_tokens = 5, total_tokens = 15 }
        });

    private static string BuildModelsResponseJson() =>
        JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new { id = "openai/gpt-4o-mini",  name = "GPT-4o Mini",  context_length = 128_000 },
                new { id = "anthropic/claude-3-5-sonnet", name = "Claude 3.5 Sonnet", context_length = 200_000 }
            }
        });

    private static string BuildEmbeddingResponseJson() =>
        JsonSerializer.Serialize(new
        {
            model = "openai/text-embedding-ada-002",
            data = new[]
            {
                new { index = 0, embedding = new float[] { 0.1f, 0.2f, 0.3f } }
            },
            usage = new { prompt_tokens = 5, completion_tokens = 0, total_tokens = 5 }
        });

    // ── Simple HttpMessageHandler ─────────────────────────────────────────────

    private sealed class FixedResponseHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public FixedResponseHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("Simulated network failure");
    }

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        // Arrange
        var options = Options.Create(DefaultConfig());
        var logger = new Mock<ILogger<OpenRouterProvider>>().Object;

        // Act
        var act = () => new OpenRouterProvider(null!, options, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        // Arrange
        var logger = new Mock<ILogger<OpenRouterProvider>>().Object;

        // Act
        var act = () => new OpenRouterProvider(new HttpClient(), null!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var options = Options.Create(DefaultConfig());

        // Act
        var act = () => new OpenRouterProvider(new HttpClient(), options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsOpenRouter()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.ProviderName.Should().Be("OpenRouter");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenModelsEndpointResponds200()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildModelsResponseJson());

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenModelsEndpointReturns401()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.Unauthorized, "{\"error\":\"Invalid API key\"}");

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenHttpExceptionThrown()
    {
        // Arrange
        var options = Options.Create(DefaultConfig());
        var logger = new Mock<ILogger<OpenRouterProvider>>().Object;
        var httpClient = new HttpClient(new ThrowingHandler()) { BaseAddress = new Uri("https://openrouter.ai/api/v1") };
        var provider = new OpenRouterProvider(httpClient, options, logger);

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    // ── ChatAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChatAsync_ReturnsAssistantMessage_WhenApiCallSucceeds()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildChatResponseJson("OpenRouter says hello"));
        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = "Hello OpenRouter!" }
            }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Message.Content.Should().Be("OpenRouter says hello");
        result.Message.Role.Should().Be("assistant");
    }

    [Fact]
    public async Task ChatAsync_UsesDefaultModel_WhenRequestModelIsNullOrEmpty()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildChatResponseJson("Ok", "openai/gpt-4o-mini"));
        var request = new AIChatRequest
        {
            Model = null,
            Messages = new List<AIChatMessage> { new() { Role = "user", Content = "Hi" } }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Model.Should().Be("openai/gpt-4o-mini");
    }

    // ── GetEmbeddingAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetEmbeddingAsync_ReturnsEmbeddingVector_WhenApiCallSucceeds()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildEmbeddingResponseJson());

        // Act
        var result = await provider.GetEmbeddingAsync("embed this text");

        // Assert
        result.Should().NotBeNull();
        result.Embedding.Should().NotBeEmpty();
    }

    // ── GetAvailableModelsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableModelsAsync_ReturnsModels_FromModelsEndpoint()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildModelsResponseJson());

        // Act
        var models = (await provider.GetAvailableModelsAsync()).ToList();

        // Assert
        models.Should().HaveCount(2);
        models.Should().Contain(m => m.Id == "openai/gpt-4o-mini");
        models.Should().Contain(m => m.Id == "anthropic/claude-3-5-sonnet");
    }

    // ── EstimateTokens ───────────────────────────────────────────────────────

    [Fact]
    public void EstimateTokens_ReturnsZero_ForEmptyString()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var count = provider.EstimateTokens(string.Empty);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void EstimateTokens_ReturnsCeilingOfLengthDividedByFour()
    {
        // Arrange
        var provider = CreateProvider();
        var text = "Hello!"; // 6 chars → ceil(6/4) = 2

        // Act
        var count = provider.EstimateTokens(text);

        // Assert
        count.Should().Be(2);
    }

    // ── GetUsageStatsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUsageStatsAsync_ReturnsOpenRouterProviderStats()
    {
        // Arrange
        var provider = CreateProvider();
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        // Act
        var stats = await provider.GetUsageStatsAsync(start, end);

        // Assert
        stats.Provider.Should().Be("OpenRouter");
        stats.StartDate.Should().Be(start);
        stats.EndDate.Should().Be(end);
    }
}
