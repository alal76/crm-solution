// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using CRM.Infrastructure.Providers.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="AzureOpenAIProvider"/> (TCOV-060).</summary>
public class AzureOpenAIProviderTests
{
    private readonly Mock<ILogger<AzureOpenAIProvider>> _loggerMock = new();

    private static AzureOpenAIConfiguration DefaultConfig() => new()
    {
        Endpoint = "https://my-resource.openai.azure.com",
        ApiKey = "test-key",
        DeploymentName = "gpt-4o",
        ApiVersion = "2024-02-15-preview"
    };

    private AzureOpenAIProvider Create(HttpMessageHandler? handler = null)
    {
        var client = handler is null
            ? new HttpClient { BaseAddress = new Uri("https://my-resource.openai.azure.com") }
            : new HttpClient(handler) { BaseAddress = new Uri("https://my-resource.openai.azure.com") };
        return new AzureOpenAIProvider(client, Options.Create(DefaultConfig()), _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullHttpClient_ShouldThrow()
    {
        var act = () => new AzureOpenAIProvider(null!, Options.Create(DefaultConfig()), _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_NullConfig_ShouldThrow()
    {
        var act = () => new AzureOpenAIProvider(new HttpClient(), null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new AzureOpenAIProvider(new HttpClient(), Options.Create(DefaultConfig()), null!);
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
    public void ProviderName_ShouldReturnAzureOpenAI()
    {
        Create().ProviderName.Should().Be("AzureOpenAI");
    }

    // ─── Availability ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task IsAvailableAsync_WhenServerUnreachable_ShouldReturnFalse()
    {
        var result = await Create().IsAvailableAsync();
        result.Should().BeFalse();
    }

    // ─── Models ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetAvailableModelsAsync_ShouldIncludeDeploymentModel()
    {
        var models = (await Create().GetAvailableModelsAsync()).ToList();
        models.Should().NotBeEmpty();
        models.Should().Contain(m => m.Id == "gpt-4o");
    }

    // ─── Config Validation ────────────────────────────────────────────────────────
    [Fact]
    public void AzureOpenAIConfiguration_Validate_MissingEndpoint_ShouldReturnError()
    {
        var config = new AzureOpenAIConfiguration { Endpoint = "", DeploymentName = "gpt-4o", ApiKey = "key" };
        var (isValid, error) = config.Validate();
        isValid.Should().BeFalse();
        error.Should().Contain("Endpoint");
    }

    [Fact]
    public void AzureOpenAIConfiguration_Validate_ValidConfig_ShouldReturnValid()
    {
        var (isValid, error) = DefaultConfig().Validate();
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }
}
