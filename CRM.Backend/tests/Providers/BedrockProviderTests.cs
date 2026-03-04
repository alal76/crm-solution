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
/// Unit tests for BedrockProvider.
/// MANDATORY: Written after verifying source signature:
/// Class: BedrockProvider, Namespace: CRM.Infrastructure.Providers.AI
/// Constructor: (HttpClient, IOptions&lt;BedrockConfiguration&gt;, ILogger&lt;BedrockProvider&gt;)
/// </summary>
public class BedrockProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static BedrockConfiguration DefaultConfig() => new()
    {
        Region = "us-east-1",
        AccessKeyId = "AKIAIOSFODNN7EXAMPLE",
        SecretAccessKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
        DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0",
        UseDefaultCredentials = false,
        DefaultMaxTokens = 4096,
        DefaultTemperature = 0.7
    };

    private static BedrockProvider CreateProvider(
        HttpStatusCode responseStatus = HttpStatusCode.OK,
        string? responseBody = null,
        BedrockConfiguration? config = null)
    {
        var body = responseBody ?? BuildClaudeChatResponseJson("Hello from Bedrock!");
        var handler = new FixedResponseHandler(responseStatus, body);
        var httpClient = new HttpClient(handler);

        var options = Options.Create(config ?? DefaultConfig());
        var logger = new Mock<ILogger<BedrockProvider>>().Object;
        return new BedrockProvider(httpClient, options, logger);
    }

    private static string BuildClaudeChatResponseJson(string text) =>
        JsonSerializer.Serialize(new
        {
            id = "msg_001",
            type = "message",
            role = "assistant",
            content = new[]
            {
                new { type = "text", text }
            },
            model = "anthropic.claude-3-sonnet-20240229-v1:0",
            stop_reason = "end_turn",
            usage = new { input_tokens = 10, output_tokens = 5 }
        });

    private static string BuildFoundationModelsResponseJson() =>
        JsonSerializer.Serialize(new
        {
            modelSummaries = new[]
            {
                new { modelId = "anthropic.claude-3-sonnet-20240229-v1:0", modelName = "Claude 3 Sonnet" }
            }
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
        var logger = new Mock<ILogger<BedrockProvider>>().Object;

        // Act
        var act = () => new BedrockProvider(null!, options, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        // Arrange
        var logger = new Mock<ILogger<BedrockProvider>>().Object;

        // Act
        var act = () => new BedrockProvider(new HttpClient(), null!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var options = Options.Create(DefaultConfig());

        // Act
        var act = () => new BedrockProvider(new HttpClient(), options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBedrock()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.ProviderName.Should().Be("Bedrock");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenApiResponds200()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildFoundationModelsResponseJson());

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApiReturns403()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.Forbidden, "{\"message\":\"Access denied\"}");

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
        var logger = new Mock<ILogger<BedrockProvider>>().Object;
        var provider = new BedrockProvider(new HttpClient(new ThrowingHandler()), options, logger);

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    // ── GetAvailableModelsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableModelsAsync_ReturnsSupportedModelsList()
    {
        // Arrange — no HTTP call needed; models are returned from a hardcoded list
        var provider = CreateProvider();

        // Act
        var models = (await provider.GetAvailableModelsAsync()).ToList();

        // Assert
        models.Should().NotBeEmpty();
        models.Should().Contain(m => m.Id.Contains("claude", StringComparison.OrdinalIgnoreCase)
                                  || m.Id.Contains("llama", StringComparison.OrdinalIgnoreCase)
                                  || m.Id.Contains("titan", StringComparison.OrdinalIgnoreCase));
    }

    // ── ChatAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChatAsync_WithClaudeModel_ReturnsAssistantMessage()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildClaudeChatResponseJson("Bedrock Claude reply"));
        var request = new AIChatRequest
        {
            Model = "anthropic.claude-3-sonnet-20240229-v1:0",
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = "Tell me about AWS." }
            }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Message.Content.Should().Be("Bedrock Claude reply");
        result.Message.Role.Should().Be("assistant");
    }

    [Fact]
    public async Task ChatAsync_UsesDefaultModelId_WhenRequestModelIsNull()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildClaudeChatResponseJson("Ok"));
        var request = new AIChatRequest
        {
            Model = null,
            Messages = new List<AIChatMessage> { new() { Role = "user", Content = "Hi" } }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Should().NotBeNull();
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
        var text = "AbcDef"; // 6 chars → ceil(6/4) = 2

        // Act
        var count = provider.EstimateTokens(text);

        // Assert
        count.Should().Be(2);
    }

    // ── GetUsageStatsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUsageStatsAsync_ReturnsBedrockProviderStats()
    {
        // Arrange
        var provider = CreateProvider();
        var start = DateTime.UtcNow.AddDays(-30);
        var end = DateTime.UtcNow;

        // Act
        var stats = await provider.GetUsageStatsAsync(start, end);

        // Assert
        stats.Provider.Should().Be("Bedrock");
        stats.StartDate.Should().Be(start);
        stats.EndDate.Should().Be(end);
    }
}
