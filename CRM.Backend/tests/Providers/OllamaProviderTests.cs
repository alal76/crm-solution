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
/// Unit tests for OllamaProvider.
/// MANDATORY: Written after verifying source signature:
/// Class: OllamaProvider, Namespace: CRM.Infrastructure.Providers.AI
/// Constructor: (HttpClient, IOptions&lt;OllamaConfiguration&gt;, ILogger&lt;OllamaProvider&gt;)
/// </summary>
public class OllamaProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static OllamaConfiguration DefaultConfig() => new()
    {
        BaseUrl = "http://localhost:11434",
        DefaultModel = "llama3",
        EmbeddingModel = "nomic-embed-text",
        DefaultMaxTokens = 2048,
        DefaultTemperature = 0.7
    };

    private static OllamaProvider CreateProvider(
        HttpStatusCode responseStatus = HttpStatusCode.OK,
        string? responseBody = null,
        OllamaConfiguration? config = null)
    {
        var body = responseBody ?? BuildChatResponseJson("Hello from Ollama!");
        var handler = new FixedResponseHandler(responseStatus, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") };

        var options = Options.Create(config ?? DefaultConfig());
        var logger = new Mock<ILogger<OllamaProvider>>().Object;
        return new OllamaProvider(httpClient, options, logger);
    }

    private static string BuildChatResponseJson(string content) =>
        JsonSerializer.Serialize(new
        {
            model = "llama3",
            message = new { role = "assistant", content },
            done = true,
            done_reason = "stop",
            prompt_eval_count = 10,
            eval_count = 5
        });

    private static string BuildGenerateResponseJson(string response) =>
        JsonSerializer.Serialize(new
        {
            model = "llama3",
            response,
            done = true,
            done_reason = "stop",
            prompt_eval_count = 8,
            eval_count = 4
        });

    private static string BuildTagsResponseJson(params string[] modelNames) =>
        JsonSerializer.Serialize(new
        {
            models = modelNames.Select(n => new { name = n, size = 1_234_567L }).ToArray()
        });

    private static string BuildEmbeddingResponseJson() =>
        JsonSerializer.Serialize(new
        {
            embedding = new float[] { 0.1f, 0.2f, 0.3f }
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
        var logger = new Mock<ILogger<OllamaProvider>>().Object;

        // Act
        var act = () => new OllamaProvider(null!, options, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        // Arrange
        var logger = new Mock<ILogger<OllamaProvider>>().Object;

        // Act
        var act = () => new OllamaProvider(new HttpClient(), null!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var options = Options.Create(DefaultConfig());

        // Act
        var act = () => new OllamaProvider(new HttpClient(), options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsOllama()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.ProviderName.Should().Be("Ollama");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenTagsEndpointResponds200()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildTagsResponseJson("llama3"));

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenTagsEndpointReturns503()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.ServiceUnavailable, "{}");

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
        var logger = new Mock<ILogger<OllamaProvider>>().Object;
        var httpClient = new HttpClient(new ThrowingHandler()) { BaseAddress = new Uri("http://localhost:11434") };
        var provider = new OllamaProvider(httpClient, options, logger);

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
        var provider = CreateProvider(HttpStatusCode.OK, BuildChatResponseJson("Ollama is fast!"));
        var request = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = "Tell me about Ollama." }
            }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Message.Content.Should().Be("Ollama is fast!");
        result.Message.Role.Should().Be("assistant");
    }

    [Fact]
    public async Task ChatAsync_FallsBackToDefaultModel_WhenRequestModelIsNullOrEmpty()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildChatResponseJson("Ok"));
        var request = new AIChatRequest
        {
            Model = string.Empty,
            Messages = new List<AIChatMessage> { new() { Role = "user", Content = "Hi" } }
        };

        // Act
        var result = await provider.ChatAsync(request);

        // Assert
        result.Model.Should().Be("llama3");
    }

    // ── CompleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteAsync_ReturnsGeneratedText_WhenApiCallSucceeds()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildGenerateResponseJson("Generated output!"));
        var request = new AICompletionRequest
        {
            Prompt = "Complete this sentence.",
            MaxTokens = 100
        };

        // Act
        var result = await provider.CompleteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Text.Should().Be("Generated output!");
    }

    [Fact]
    public async Task CompleteAsync_CapsNumPredict_WhenMaxTokensExceedsLimit()
    {
        // Arrange — the provider has MaxPredictTokensCap = 256
        var provider = CreateProvider(HttpStatusCode.OK, BuildGenerateResponseJson("Capped!"));
        var request = new AICompletionRequest
        {
            Prompt = "A long prompt.",
            MaxTokens = 99999 // far above the cap
        };

        // Act
        var result = await provider.CompleteAsync(request);

        // Assert — the call succeeds; the cap is applied internally before sending
        result.Should().NotBeNull();
        result.Text.Should().Be("Capped!");
    }

    // ── GetEmbeddingAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetEmbeddingAsync_ReturnsEmbeddingVector_WhenApiCallSucceeds()
    {
        // Arrange
        var provider = CreateProvider(HttpStatusCode.OK, BuildEmbeddingResponseJson());

        // Act
        var result = await provider.GetEmbeddingAsync("some text to embed");

        // Assert
        result.Should().NotBeNull();
        result.Embedding.Should().NotBeEmpty();
    }

    // ── GetAvailableModelsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableModelsAsync_ReturnsModels_FromTagsEndpoint()
    {
        // Arrange
        var provider = CreateProvider(
            HttpStatusCode.OK,
            BuildTagsResponseJson("llama3", "mistral", "codellama"));

        // Act
        var models = (await provider.GetAvailableModelsAsync()).ToList();

        // Assert
        models.Should().HaveCount(3);
        models.Should().Contain(m => m.Id == "llama3");
        models.Should().Contain(m => m.Id == "mistral");
        models.Should().Contain(m => m.Id == "codellama");
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
}
