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
/// Unit tests for AzureOpenAIProvider.
/// MANDATORY: Written after verifying source signature:
/// Class: AzureOpenAIProvider, Namespace: CRM.Infrastructure.Providers.AI
/// Constructor: (HttpClient, IOptions&lt;AzureOpenAIConfiguration&gt;, ILogger&lt;AzureOpenAIProvider&gt;)
/// </summary>
public class AzureOpenAIProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AzureOpenAIConfiguration DefaultConfig() => new()
    {
        Endpoint = "https://myresource.openai.azure.com",
        ApiKey = "test-api-key-0000000000000000000",
        DeploymentName = "gpt-4o",
        EmbeddingDeploymentName = "text-embedding-ada-002",
        ApiVersion = "2024-02-15-preview",
        DefaultMaxTokens = 4096,
        DefaultTemperature = 0.7
    };

    private static AzureOpenAIProvider CreateProvider(
        HttpStatusCode responseStatus = HttpStatusCode.OK,
        string? responseBody = null,
        AzureOpenAIConfiguration? config = null)
    {
        var body = responseBody ?? BuildChatResponseJson("Hello from Azure!");
        var handler = new FixedResponseHandler(responseStatus, body);
        var httpClient = new HttpClient(handler);

        var options = Options.Create(config ?? DefaultConfig());
        var logger = new Mock<ILogger<AzureOpenAIProvider>>().Object;
        return new AzureOpenAIProvider(httpClient, options, logger);
    }

    private static string BuildChatResponseJson(string content, string model = "gpt-4o") =>
        JsonSerializer.Serialize(new
        {
            id = "chatcmpl-001",
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

    private static string BuildEmbeddingResponseJson() =>
        JsonSerializer.Serialize(new
        {
            model = "text-embedding-ada-002",
            data = new[]
            {
                new { index = 0, embedding = new float[] { 0.1f, 0.2f, 0.3f } }
            },
            usage = new { prompt_tokens = 5, completion_tokens = 0, total_tokens = 5 }
        });

    // ── Simple HttpMessageHandler for controlled responses ───────────────────

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
        var logger = new Mock<ILogger<AzureOpenAIProvider>>().Object;

        // Act
        var act = () => new AzureOpenAIProvider(null!, options, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        // Arrange
        var logger = new Mock<ILogger<AzureOpenAIProvider>>().Object;

        // Act
        var act = () => new AzureOpenAIProvider(new HttpClient(), null!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var options = Options.Create(DefaultConfig());

        // Act
        var act = () => new AzureOpenAIProvider(new HttpClient(), options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsAzureOpenAI()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.ProviderName.Should().Be("AzureOpenAI");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenEndpointResponds200()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenEndpointReturns401()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.Unauthorized, "{\"error\":\"invalid key\"}");

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
        var logger = new Mock<ILogger<AzureOpenAIProvider>>().Object;
        var provider = new AzureOpenAIProvider(new HttpClient(new ThrowingHandler()), options, logger);

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
        var provider = CreateProvider(HttpStatusCode.OK, BuildChatResponseJson("Azure reply here"));
        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = "Hello Azure!" }
            }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Message.Content.Should().Be("Azure reply here");
        result.Message.Role.Should().Be("assistant");
    }

    [Fact]
    public async Task ChatAsync_UsesDeploymentNameFromConfig_WhenRequestModelIsNull()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildChatResponseJson("Ok", "gpt-4o"));
        var request = new AIChatRequest
        {
            Model = null,
            Messages = new List<AIChatMessage> { new() { Role = "user", Content = "Hi" } }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Model.Should().Be("gpt-4o");
    }

    // ── GetAvailableModelsAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableModelsAsync_ReturnsDeploymentNamesFromConfig()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var models = (await provider.GetAvailableModelsAsync()).ToList();

        // Assert
        models.Should().NotBeEmpty();
        models.Should().Contain(m => m.Id == "gpt-4o");
        models.Should().Contain(m => m.Id == "text-embedding-ada-002");
    }

    // ── GetEmbeddingAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetEmbeddingAsync_ReturnsEmbeddingVector_WhenApiCallSucceeds()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildEmbeddingResponseJson());

        // Act
        var result = await provider.GetEmbeddingAsync("sample text");

        // Assert
        result.Should().NotBeNull();
        result.Embedding.Should().NotBeEmpty();
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
    public async Task GetUsageStatsAsync_ReturnsProviderName_AzureOpenAI()
    {
        // Arrange
        var provider = CreateProvider();
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        // Act
        var stats = await provider.GetUsageStatsAsync(start, end);

        // Assert
        stats.Provider.Should().Be("AzureOpenAI");
        stats.StartDate.Should().Be(start);
        stats.EndDate.Should().Be(end);
    }
}
