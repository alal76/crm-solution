// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Providers.Meilisearch;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Factories;

/// <summary>
/// Unit tests for provider factories verifying:
/// - Correct provider listing (GetAvailableProviders)
/// - Active provider name resolution based on feature flags
/// - AdapterRegistry health monitoring functionality
/// </summary>
public class ProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly Mock<ILogger<SearchProviderFactory>> _searchLoggerMock;
    private readonly Mock<ILogger<ChatProviderFactory>> _chatLoggerMock;
    private readonly Mock<ILogger<NotificationProviderFactory>> _notificationLoggerMock;
    private readonly Mock<ILogger<AnalyticsProviderFactory>> _analyticsLoggerMock;
    private readonly Mock<ILogger<SignatureProviderFactory>> _signatureLoggerMock;
    private readonly Mock<ILogger<AIProviderFactory>> _aiLoggerMock;
    private readonly Mock<ILogger<IntegrationProviderFactory>> _integrationLoggerMock;
    private readonly IConfiguration _configuration;

    public ProviderFactoryTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _searchLoggerMock = new Mock<ILogger<SearchProviderFactory>>();
        _chatLoggerMock = new Mock<ILogger<ChatProviderFactory>>();
        _notificationLoggerMock = new Mock<ILogger<NotificationProviderFactory>>();
        _analyticsLoggerMock = new Mock<ILogger<AnalyticsProviderFactory>>();
        _signatureLoggerMock = new Mock<ILogger<SignatureProviderFactory>>();
        _aiLoggerMock = new Mock<ILogger<AIProviderFactory>>();
        _integrationLoggerMock = new Mock<ILogger<IntegrationProviderFactory>>();

        var configData = new Dictionary<string, string?>
        {
            ["Providers:Search:Type"] = "Meilisearch",
            ["Providers:Chat:Type"] = "Chatwoot",
            ["Providers:Notifications:Type"] = "Novu",
            ["Providers:Analytics:Type"] = "Superset",
            ["Providers:Signatures:Type"] = "DocuSeal",
            ["Providers:AI:Type"] = "Ollama",
            ["Providers:Integrations:Type"] = "n8n"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    #region SearchProviderFactory Tests

    [Fact]
    public void SearchProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new SearchProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _searchLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.Search.BuiltIn);
        providers.Should().Contain(ProviderTypes.Search.Meilisearch);
        providers.Should().Contain(ProviderTypes.Search.Algolia);
        providers.Should().Contain(ProviderTypes.Search.Typesense);
        providers.Should().Contain(ProviderTypes.Search.Elasticsearch);
        providers.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void SearchProviderFactory_GetActiveProviderName_WhenExternalEnabled_ReturnsConfiguredProvider()
    {
        // Arrange — factory reads UseExternalSearch from IConfiguration, not IFeatureManager
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureManagement:UseExternalSearch"] = "true",
                ["Providers:Search:Type"] = "Meilisearch"
            })
            .Build();

        var factory = new SearchProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            config,
            _searchLoggerMock.Object);

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Meilisearch");
    }

    [Fact]
    public void SearchProviderFactory_GetActiveProviderName_WhenExternalDisabled_ReturnsBuiltIn()
    {
        // Arrange
        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSearch))
            .ReturnsAsync(false);

        var factory = new SearchProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _searchLoggerMock.Object);

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Search.BuiltIn);
    }

    [Fact]
    public void SearchProviderFactory_GetProvider_ReturnsMeilisearchProvider_WhenUseExternalSearchEnabled()
    {
        // Arrange — separate config with UseExternalSearch=true so the factory routes to Meilisearch
        var configData = new Dictionary<string, string?>
        {
            ["FeatureManagement:UseExternalSearch"] = "true",
            ["Providers:Search:Type"] = "Meilisearch"
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var meilisearchProvider = new MeilisearchProvider(
            Options.Create(new MeilisearchConfiguration
            {
                Url = "http://localhost:7700",
                ApiKey = "test-master-key",
                IndexPrefix = "test_"
            }),
            new Mock<ILogger<MeilisearchProvider>>().Object);

        // ProviderResolution.ResolveByTypeName resolves by concrete type via IServiceProvider.GetService()
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(MeilisearchProvider)))
            .Returns(meilisearchProvider);

        var factory = new SearchProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            config,
            _searchLoggerMock.Object);

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().BeOfType<MeilisearchProvider>();
        provider.ProviderName.Should().Be("Meilisearch");
    }

    #endregion

    #region ChatProviderFactory Tests

    [Fact]
    public void ChatProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new ChatProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _chatLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.Chat.BuiltIn);
        providers.Should().Contain(ProviderTypes.Chat.Chatwoot);
        providers.Should().Contain(ProviderTypes.Chat.Intercom);
        providers.Should().Contain(ProviderTypes.Chat.Zendesk);
        providers.Should().Contain(ProviderTypes.Chat.Freshchat);
    }

    [Fact]
    public void ChatProviderFactory_GetActiveProviderName_WhenExternalEnabled_ReturnsConfiguredProvider()
    {
        // Arrange — factory reads UseExternalChat from IConfiguration, not IFeatureManager
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureManagement:UseExternalChat"] = "true",
                ["Providers:Chat:Type"] = "Chatwoot"
            })
            .Build();

        var factory = new ChatProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            config,
            _chatLoggerMock.Object);

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Chatwoot");
    }

    #endregion

    #region NotificationProviderFactory Tests

    [Fact]
    public void NotificationProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new NotificationProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _notificationLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.Notifications.BuiltIn);
        providers.Should().Contain(ProviderTypes.Notifications.Novu);
        providers.Should().Contain(ProviderTypes.Notifications.Twilio);
        providers.Should().Contain(ProviderTypes.Notifications.SendGrid);
        providers.Should().Contain(ProviderTypes.Notifications.OneSignal);
    }

    [Fact]
    public void NotificationProviderFactory_GetActiveProviderName_WhenExternalEnabled_ReturnsConfiguredProvider()
    {
        // Arrange — factory reads UseExternalNotifications from IConfiguration, not IFeatureManager
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureManagement:UseExternalNotifications"] = "true",
                ["Providers:Notifications:Type"] = "Novu"
            })
            .Build();

        var factory = new NotificationProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            config,
            _notificationLoggerMock.Object);

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Novu");
    }

    #endregion

    #region AnalyticsProviderFactory Tests

    [Fact]
    public void AnalyticsProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new AnalyticsProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _analyticsLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.Analytics.BuiltIn);
        providers.Should().Contain(ProviderTypes.Analytics.Superset);
        providers.Should().Contain(ProviderTypes.Analytics.Metabase);
        providers.Should().Contain(ProviderTypes.Analytics.PowerBI);
    }

    [Fact]
    public void AnalyticsProviderFactory_GetActiveProviderName_WhenExternalEnabled_ReturnsConfiguredProvider()
    {
        // Arrange — factory reads UseExternalAnalytics from IConfiguration, not IFeatureManager
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureManagement:UseExternalAnalytics"] = "true",
                ["Providers:Analytics:Type"] = "Superset"
            })
            .Build();

        var factory = new AnalyticsProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            config,
            _analyticsLoggerMock.Object);

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Superset");
    }

    #endregion

    #region SignatureProviderFactory Tests

    [Fact]
    public void SignatureProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new SignatureProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _signatureLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.Signatures.BuiltIn);
        providers.Should().Contain(ProviderTypes.Signatures.DocuSeal);
        providers.Should().Contain(ProviderTypes.Signatures.DocuSign);
        providers.Should().Contain(ProviderTypes.Signatures.AdobeSign);
    }

    #endregion

    #region AIProviderFactory Tests

    [Fact]
    public void AIProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new AIProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _aiLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.AI.Ollama);
        providers.Should().Contain(ProviderTypes.AI.OpenAI);
        providers.Should().Contain(ProviderTypes.AI.AzureOpenAI);
        providers.Should().Contain(ProviderTypes.AI.Anthropic);
    }

    #endregion

    #region IntegrationProviderFactory Tests

    [Fact]
    public void IntegrationProviderFactory_GetAvailableProviders_ReturnsAllOptions()
    {
        // Arrange
        var factory = new IntegrationProviderFactory(
            _serviceProviderMock.Object,
            _featureManagerMock.Object,
            _configuration,
            _integrationLoggerMock.Object);

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().Contain(ProviderTypes.Integrations.BuiltIn);
        providers.Should().Contain(ProviderTypes.Integrations.N8n);
        providers.Should().Contain(ProviderTypes.Integrations.Zapier);
    }

    #endregion

    #region AdapterRegistry Tests

    [Fact]
    public void AdapterRegistry_Register_AddsAdapter()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        // Act
        registry.Register("Search", "Meilisearch", isActive: true);

        // Assert
        var adapters = registry.GetAdaptersByCategory("Search");
        adapters.Should().ContainSingle();
        adapters.First().ProviderName.Should().Be("Meilisearch");
        adapters.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public void AdapterRegistry_Register_MultipleAdaptersInCategory()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        // Act
        registry.Register("Search", "BuiltIn", isActive: false);
        registry.Register("Search", "Meilisearch", isActive: true);
        registry.Register("Search", "Algolia", isActive: false);

        // Assert
        var adapters = registry.GetAdaptersByCategory("Search").ToList();
        adapters.Should().HaveCount(3);
        adapters.Select(a => a.ProviderName).Should().Contain(new[] { "BuiltIn", "Meilisearch", "Algolia" });
    }

    [Fact]
    public void AdapterRegistry_GetAdaptersByCategory_WhenCategoryNotFound_ReturnsEmpty()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        // Act
        var adapters = registry.GetAdaptersByCategory("NonExistentCategory");

        // Assert
        adapters.Should().BeEmpty();
    }

    [Fact]
    public void AdapterRegistry_GetActiveAdapter_ReturnsActiveForCategory()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "BuiltIn", isActive: false);
        registry.Register("Search", "Meilisearch", isActive: true);
        registry.Register("Chat", "Chatwoot", isActive: true);

        // Act
        var searchActive = registry.GetActiveAdapter("Search");
        var chatActive = registry.GetActiveAdapter("Chat");

        // Assert
        searchActive.Should().NotBeNull();
        searchActive!.ProviderName.Should().Be("Meilisearch");
        chatActive.Should().NotBeNull();
        chatActive!.ProviderName.Should().Be("Chatwoot");
    }

    [Fact]
    public void AdapterRegistry_GetActiveAdapter_WhenNoActive_ReturnsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "BuiltIn", isActive: false);
        registry.Register("Search", "Meilisearch", isActive: false);

        // Act
        var active = registry.GetActiveAdapter("Search");

        // Assert
        active.Should().BeNull();
    }

    [Fact]
    public void AdapterRegistry_GetAdapter_ReturnsSpecificAdapter()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "BuiltIn", isActive: false);
        registry.Register("Search", "Meilisearch", isActive: true);

        // Act
        var adapter = registry.GetAdapter("Search", "BuiltIn");

        // Assert
        adapter.Should().NotBeNull();
        adapter!.ProviderName.Should().Be("BuiltIn");
        adapter.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AdapterRegistry_SetActive_ActivatesAndDeactivatesOthers()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "BuiltIn", isActive: true);
        registry.Register("Search", "Meilisearch", isActive: false);
        registry.Register("Search", "Algolia", isActive: false);

        // Act
        registry.SetActive("Search", "Meilisearch");

        // Assert
        var adapters = registry.GetAdaptersByCategory("Search").ToList();
        adapters.First(a => a.ProviderName == "BuiltIn").IsActive.Should().BeFalse();
        adapters.First(a => a.ProviderName == "Meilisearch").IsActive.Should().BeTrue();
        adapters.First(a => a.ProviderName == "Algolia").IsActive.Should().BeFalse();
    }

    [Fact]
    public void AdapterRegistry_GetHealthSummary_ReturnsCorrectStats()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "BuiltIn", isActive: false);
        registry.Register("Search", "Meilisearch", isActive: true);
        registry.Register("Chat", "Chatwoot", isActive: true);
        registry.Register("Notifications", "Novu", isActive: true);

        // Act
        var summary = registry.GetHealthSummary();

        // Assert
        summary.Should().NotBeNull();
        summary.TotalAdapters.Should().Be(4);
        summary.ActiveAdapters.Should().HaveCount(3);
    }

    [Fact]
    public void AdapterRegistry_RecordSuccess_UpdatesMetrics()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "Meilisearch", isActive: true);

        // Act
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(50));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter.Should().NotBeNull();
        adapter!.SuccessCount.Should().Be(2);
        adapter.TotalOperationTime.TotalMilliseconds.Should().Be(150); // 50 + 100
    }

    [Fact]
    public void AdapterRegistry_RecordFailure_UpdatesMetrics()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "Meilisearch", isActive: true);

        // Act
        registry.RecordFailure("Search", "Meilisearch", "Connection timeout");

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter.Should().NotBeNull();
        adapter!.FailureCount.Should().Be(1);
        adapter.LastFailureTime.Should().NotBeNull();
        adapter.LastFailureMessage.Should().Be("Connection timeout");
    }

    [Fact]
    public void AdapterRegistry_UpdateHealth_SetsAdapterStatus()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "Meilisearch", isActive: true);

        // Act
        var healthResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("All systems operational");
        registry.UpdateHealth("Search", "Meilisearch", healthResult);

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter.Should().NotBeNull();
        adapter!.Status.Should().Be(AdapterStatus.Healthy);
        adapter.LastHealthMessage.Should().Be("All systems operational");
        adapter.LastHealthCheck.Should().NotBeNull();
    }

    [Fact]
    public void AdapterRegistry_UpdateHealth_DegradedStatus()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AdapterRegistry>>();
        var registry = new AdapterRegistry(loggerMock.Object);

        registry.Register("Search", "Meilisearch", isActive: true);

        // Act
        var healthResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("High latency detected");
        registry.UpdateHealth("Search", "Meilisearch", healthResult);

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.Status.Should().Be(AdapterStatus.Degraded);
    }

    #endregion
}
