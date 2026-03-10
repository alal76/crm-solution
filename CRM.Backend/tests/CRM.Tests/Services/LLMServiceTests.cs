// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for LLMService (TCOV-019).</summary>
public class LLMServiceTests
{
    private static LLMService CreateService(Action<LLMProviderOptions>? configure = null)
    {
        var opts = new LLMProviderOptions();
        configure?.Invoke(opts);
        var options = Options.Create(opts);
        var logger = NullLogger<LLMService>.Instance;
        return new LLMService(logger, options);
    }

    [Fact]
    public void IsConfigured_ShouldReturnFalse_WhenOpenAIApiKeyIsEmpty()
    {
        var svc = CreateService();
        svc.IsConfigured("openai").Should().BeFalse();
    }

    [Fact]
    public void IsConfigured_ShouldReturnTrue_WhenOpenAIApiKeyIsSet()
    {
        var svc = CreateService(o => o.OpenAI.ApiKey = "sk-test-key-1234567890");
        svc.IsConfigured("openai").Should().BeTrue();
    }

    [Fact]
    public void IsConfigured_ShouldReturnFalse_ForUnknownProvider()
    {
        var svc = CreateService();
        svc.IsConfigured("nonexistent-provider-xyz").Should().BeFalse();
    }

    [Fact]
    public void GetAvailableProviders_ShouldReturnNonEmptyList()
    {
        var svc = CreateService();
        var providers = svc.GetAvailableProviders();
        providers.Should().NotBeNull();
        providers.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAvailableModels_ShouldReturnNonEmptyList()
    {
        var svc = CreateService();
        var models = svc.GetAvailableModels();
        models.Should().NotBeNull();
        models.Should().NotBeEmpty();
    }

    [Fact]
    public async Task IsConfiguredAsync_ShouldReturnFalse_WhenNoApiKeySet()
    {
        var svc = CreateService();
        var result = await svc.IsConfiguredAsync("openai");
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAvailableProviders_ShouldIncludeOpenAI()
    {
        var svc = CreateService();
        var providers = svc.GetAvailableProviders();
        providers.Should().Contain(p => p.Value != null && p.Value.ToLower().Contains("openai"));
    }
}
