// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Ports;
using CRM.Infrastructure.Services.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CRMConfigurationService.
/// Mocks IProviderConfigurationService and IHttpClientFactory.
/// </summary>
public class CRMConfigurationServiceTests
{
    private readonly Mock<IProviderConfigurationService> _providerConfigMock;
    private readonly Mock<ILogger<CRMConfigurationService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly CRMConfigurationService _service;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CRMConfigurationServiceTests()
    {
        _providerConfigMock = new Mock<IProviderConfigurationService>();
        _loggerMock = new Mock<ILogger<CRMConfigurationService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _service = new CRMConfigurationService(
            _providerConfigMock.Object,
            _loggerMock.Object,
            _httpClientFactoryMock.Object);
    }

    #region Helper Methods

    private static ProviderConfigurationDto CreateProviderConfigDto(
        string configKey,
        object configDataObj,
        string configType = "crm",
        string? providerName = null)
    {
        var json = JsonSerializer.Serialize(configDataObj, JsonOptions);
        return new ProviderConfigurationDto
        {
            Id = 1,
            ConfigurationKey = configKey,
            ConfigurationType = configType,
            ProviderName = providerName,
            ConfigurationData = json,
            IsEncrypted = true,
            IsActive = true,
            CanBeDisabledAtRuntime = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUserName = "admin"
        };
    }

    private void SetupAllAIProvidersNull()
    {
        foreach (var provider in new[] { "ollama", "openai", "azure", "anthropic", "bedrock", "openrouter", "gemini" })
        {
            _providerConfigMock
                .Setup(p => p.GetConfigurationAsync($"crm.ai.{provider}", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProviderConfigurationDto?)null);
        }
    }

    private void SetupAllIntegrationsNull()
    {
        var knownIntegrations = new[]
        {
            ("search", "meilisearch"), ("search", "algolia"), ("search", "elasticsearch"), ("search", "typesense"),
            ("chat", "chatwoot"), ("chat", "intercom"),
            ("notifications", "novu"), ("notifications", "twilio"), ("notifications", "sendgrid"),
            ("analytics", "superset"), ("analytics", "metabase"), ("analytics", "powerbi"),
            ("signatures", "docuseal"), ("signatures", "docusign"),
            ("workflows", "n8n"), ("workflows", "zapier")
        };

        foreach (var (type, provider) in knownIntegrations)
        {
            _providerConfigMock
                .Setup(p => p.GetConfigurationAsync($"crm.integration.{type}.{provider}", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProviderConfigurationDto?)null);
        }
    }

    private void SetupWorkerConfigNull()
    {
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.worker.config", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderConfigurationDto?)null);
    }

    private void SetupAgentsConfigNull()
    {
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.agents.config", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderConfigurationDto?)null);
    }

    private void SetupAllNull()
    {
        SetupAllAIProvidersNull();
        SetupAllIntegrationsNull();
        SetupWorkerConfigNull();
        SetupAgentsConfigNull();
    }

    private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content = "")
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });

        return new HttpClient(handlerMock.Object);
    }

    #endregion

    #region GetCRMConfigAsync Tests

    [Fact]
    public async Task GetCRMConfigAsync_ShouldReturnEmptyConfig_WhenNothingConfigured()
    {
        // Arrange
        SetupAllNull();

        // Act
        var result = await _service.GetCRMConfigAsync();

        // Assert
        result.Should().NotBeNull();
        result.AIProviders.Should().BeEmpty();
        result.Integrations.Should().BeEmpty();
        result.WorkerConfig.Should().BeNull();
        result.AIAgents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCRMConfigAsync_ShouldReturnAIProviders_WhenAIConfigured()
    {
        // Arrange
        SetupAllNull(); // Start with all null, then override specific ones

        var openaiConfig = new AIProviderConfigDto
        {
            Provider = "openai",
            Enabled = true,
            ApiKey = "sk-test123",
            Model = "gpt-4"
        };
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.ai.openai", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.ai.openai", openaiConfig, providerName: "openai"));

        // Act
        var result = await _service.GetCRMConfigAsync();

        // Assert
        result.AIProviders.Should().NotBeEmpty();
        result.AIProviders.Should().Contain(p => p.Provider == "openai");
    }

    [Fact]
    public async Task GetCRMConfigAsync_ShouldReturnIntegrations_WhenIntegrationsConfigured()
    {
        // Arrange
        SetupAllNull();

        var meilisearchConfig = new IntegrationConfigDto
        {
            Type = "search",
            Provider = "meilisearch",
            Enabled = true,
            Configuration = new Dictionary<string, object>
            {
                { "url", "http://crm-meilisearch:7700" }
            }
        };
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.integration.search.meilisearch", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.integration.search.meilisearch", meilisearchConfig));

        // Act
        var result = await _service.GetCRMConfigAsync();

        // Assert
        result.Integrations.Should().NotBeEmpty();
        result.Integrations.Should().Contain(i => i.Provider == "meilisearch");
    }

    #endregion

    #region UpdateAIProviderAsync Tests

    [Fact]
    public async Task UpdateAIProviderAsync_ShouldCallProviderConfigService_WithCorrectKey()
    {
        // Arrange
        var aiConfig = new AIProviderConfigDto
        {
            Provider = "openai",
            Enabled = true,
            ApiKey = "sk-test",
            Model = "gpt-4"
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.ai.openai", aiConfig));

        // Act
        await _service.UpdateAIProviderAsync("openai", aiConfig, userId: 1);

        // Assert
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "crm.ai.openai",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAIProviderAsync_ShouldNormalizeProviderToLowerCase()
    {
        // Arrange
        var aiConfig = new AIProviderConfigDto
        {
            Provider = "Anthropic",
            Enabled = true,
            ApiKey = "sk-ant-test",
            Model = "claude-3"
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.ai.anthropic", aiConfig));

        // Act
        await _service.UpdateAIProviderAsync("Anthropic", aiConfig, userId: 1);

        // Assert — key should be lowercased
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "crm.ai.anthropic",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateIntegrationAsync Tests

    [Fact]
    public async Task UpdateIntegrationAsync_ShouldCallProviderConfigService_WithCorrectKey()
    {
        // Arrange
        var integrationConfig = new IntegrationConfigDto
        {
            Type = "search",
            Provider = "meilisearch",
            Enabled = true,
            Configuration = new Dictionary<string, object>
            {
                { "url", "http://search:7700" },
                { "apiKey", "masterKey" }
            }
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.integration.search.meilisearch", integrationConfig));

        // Act
        await _service.UpdateIntegrationAsync("search", "meilisearch", integrationConfig, userId: 1);

        // Assert
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "crm.integration.search.meilisearch",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateWorkerConfigAsync Tests

    [Fact]
    public async Task UpdateWorkerConfigAsync_ShouldCallProviderConfigService_WithCorrectKey()
    {
        // Arrange
        var workerConfig = new WorkerConfigDto
        {
            Enabled = true,
            MaxConcurrentJobs = 10,
            JobTimeoutMinutes = 60,
            RetryAttempts = 5
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.worker.config", workerConfig));

        // Act
        await _service.UpdateWorkerConfigAsync(workerConfig, userId: 1);

        // Assert
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "crm.worker.config",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAIAgentsAsync Tests

    [Fact]
    public async Task UpdateAIAgentsAsync_ShouldCallProviderConfigService_WithCorrectKey()
    {
        // Arrange
        var agents = new List<AIAgentConfigDto>
        {
            new() { Id = 1, Name = "Lead Scorer", Enabled = true },
            new() { Id = 2, Name = "Support Triage", Enabled = false }
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("crm.agents.config", agents));

        // Act
        await _service.UpdateAIAgentsAsync(agents, userId: 1);

        // Assert
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "crm.agents.config",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region TestAIProviderAsync Tests

    [Fact]
    public async Task TestAIProviderAsync_ShouldReturnFailure_WhenNoApiKey()
    {
        // Arrange — no apiKey and no apiUrl means no test URL can be determined
        var config = new AIProviderConfigDto
        {
            Provider = "bedrock",
            Enabled = true,
            ApiKey = null,
            ApiUrl = null,
            Model = "some-model"
        };

        // Bedrock requires ApiUrl to generate a test URL; with null, should fail
        SetupAllNull(); // ensure GetConfigurationAsync for the status update returns null

        // Act
        var result = await _service.TestAIProviderAsync("bedrock", config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot determine test URL");
    }

    [Fact]
    public async Task TestAIProviderAsync_ShouldReturnSuccess_WhenProviderResponds()
    {
        // Arrange
        var config = new AIProviderConfigDto
        {
            Provider = "openai",
            Enabled = true,
            ApiKey = "sk-test-key",
            Model = "gpt-4"
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, """{"data":[]}""");
        _httpClientFactoryMock
            .Setup(f => f.CreateClient("ConfigurationTest"))
            .Returns(httpClient);

        // The service tries to update test status — mock GetConfigurationAsync to return null (no status to write)
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.ai.openai", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderConfigurationDto?)null);

        // Act
        var result = await _service.TestAIProviderAsync("openai", config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("reachable");
    }

    [Fact]
    public async Task TestAIProviderAsync_ShouldReturnFailure_WhenProviderReturnsError()
    {
        // Arrange
        var config = new AIProviderConfigDto
        {
            Provider = "openai",
            Enabled = true,
            ApiKey = "sk-invalid",
            Model = "gpt-4"
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized, """{"error":"invalid key"}""");
        _httpClientFactoryMock
            .Setup(f => f.CreateClient("ConfigurationTest"))
            .Returns(httpClient);

        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.ai.openai", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderConfigurationDto?)null);

        // Act
        var result = await _service.TestAIProviderAsync("openai", config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("401");
    }

    #endregion

    #region TestIntegrationAsync Tests

    [Fact]
    public async Task TestIntegrationAsync_ShouldReturnFailure_WhenNoEndpoint()
    {
        // Arrange — no URL means no test URL
        var config = new IntegrationConfigDto
        {
            Type = "search",
            Provider = "meilisearch",
            Enabled = true,
            Configuration = new Dictionary<string, object>(),
            TestEndpoint = null
        };

        SetupAllNull();

        // Act
        var result = await _service.TestIntegrationAsync("search", "meilisearch", config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot determine test URL");
    }

    [Fact]
    public async Task TestIntegrationAsync_ShouldReturnSuccess_WhenEndpointResponds()
    {
        // Arrange
        var config = new IntegrationConfigDto
        {
            Type = "search",
            Provider = "meilisearch",
            Enabled = true,
            Configuration = new Dictionary<string, object>
            {
                { "url", "http://meilisearch:7700" }
            },
            Credentials = new Dictionary<string, string>
            {
                { "apiKey", "masterKey" }
            }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, """{"status":"available"}""");
        _httpClientFactoryMock
            .Setup(f => f.CreateClient("ConfigurationTest"))
            .Returns(httpClient);

        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync("crm.integration.search.meilisearch", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderConfigurationDto?)null);

        // Act
        var result = await _service.TestIntegrationAsync("search", "meilisearch", config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("reachable");
    }

    #endregion
}
