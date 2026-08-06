// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http.Json;
using System.Text;
using CRM.Infrastructure.Providers.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="OllamaProvider"/> (TCOV-059).</summary>
public class OllamaProviderTests
{
    private readonly Mock<ILogger<OllamaProvider>> _loggerMock = new();

    private static OllamaConfiguration DefaultConfig() => new()
    {
        BaseUrl = "http://localhost:11434",
        DefaultModel = "llama3",
        EmbeddingModel = "nomic-embed-text"
    };

    private OllamaProvider Create(HttpMessageHandler? handler = null)
    {
        var client = handler is null
            ? new HttpClient { BaseAddress = new Uri("http://localhost:11434") }
            : new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") };
        return new OllamaProvider(client, Options.Create(DefaultConfig()), _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullHttpClient_ShouldThrow()
    {
        var act = () => new OllamaProvider(null!, Options.Create(DefaultConfig()), _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_NullConfig_ShouldThrow()
    {
        var act = () => new OllamaProvider(new HttpClient(), null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new OllamaProvider(new HttpClient(), Options.Create(DefaultConfig()), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => Create();
        act.Should().NotThrow();
    }

    // ─── Properties ─────────────────────────────────────────────────────────────
    [Fact]
    public void ProviderName_ShouldReturnOllama()
    {
        Create().ProviderName.Should().Be("Ollama");
    }

    // ─── Availability ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task IsAvailableAsync_WhenServerUnreachable_ShouldReturnFalse()
    {
        var result = await Create().IsAvailableAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenServerResponds_ShouldReturnTrue()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        var result = await Create(handler).IsAvailableAsync();
        result.Should().BeTrue();
    }

    // ─── GetAvailableModelsAsync ─────────────────────────────────────────────────
    [Fact]
    public async Task GetAvailableModelsAsync_WhenServerFails_ShouldReturnEmpty()
    {
        var result = await Create().GetAvailableModelsAsync();
        result.Should().BeEmpty();
    }
}

/// <summary>Minimal fake HttpMessageHandler for unit tests.</summary>
file sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        });
    }
}
