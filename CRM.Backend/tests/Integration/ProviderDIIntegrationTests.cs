// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.DependencyInjection;

namespace CRM.Tests.Integration;

/// <summary>
/// Integration tests for the pluggable provider architecture.
/// Tests that providers can be resolved via DI and switched via configuration.
/// </summary>
public class ProviderDIIntegrationTests
{
    /// <summary>
    /// Task 4.7: Integration test - Start with all BuiltIn providers
    /// Verifies that the DI container can be configured and all factories are available.
    /// </summary>
    [Fact]
    public void AddPluggableProviders_WithDefaultConfig_RegistersAllFactories()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalSearch", "false" },
            { "FeatureManagement:UseExternalChat", "false" },
            { "FeatureManagement:UseExternalNotifications", "false" },
            { "FeatureManagement:UseExternalAnalytics", "false" },
            { "FeatureManagement:UseExternalSignatures", "false" },
            { "FeatureManagement:UseExternalAI", "false" },
            { "FeatureManagement:UseExternalIntegrations", "false" },
            { "Providers:Search:Type", "BuiltIn" },
            { "Providers:Chat:Type", "BuiltIn" },
            { "Providers:Notifications:Type", "BuiltIn" },
            { "Providers:Analytics:Type", "BuiltIn" },
            { "Providers:Signatures:Type", "BuiltIn" },
            { "Providers:AI:Type", "BuiltIn" },
            { "Providers:Integrations:Type", "BuiltIn" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - All factories should be registered
        serviceProvider.GetService<IProviderFactory<ISearchPort>>().Should().NotBeNull();
        serviceProvider.GetService<IProviderFactory<IChatPort>>().Should().NotBeNull();
        serviceProvider.GetService<IProviderFactory<INotificationPort>>().Should().NotBeNull();
        serviceProvider.GetService<IProviderFactory<IAnalyticsPort>>().Should().NotBeNull();
        serviceProvider.GetService<IProviderFactory<ISignaturePort>>().Should().NotBeNull();
        serviceProvider.GetService<IProviderFactory<IAIPort>>().Should().NotBeNull();
        serviceProvider.GetService<IProviderFactory<IIntegrationPort>>().Should().NotBeNull();

        // Assert - AdapterRegistry should be registered
        serviceProvider.GetService<AdapterRegistry>().Should().NotBeNull();
    }

    /// <summary>
    /// Task 4.7: Verifies all factories return BuiltIn as active provider when external is disabled.
    /// </summary>
    [Fact]
    public void AllFactories_WhenExternalDisabled_ReturnBuiltInAsActive()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalSearch", "false" },
            { "FeatureManagement:UseExternalChat", "false" },
            { "FeatureManagement:UseExternalNotifications", "false" },
            { "FeatureManagement:UseExternalAnalytics", "false" },
            { "FeatureManagement:UseExternalSignatures", "false" },
            { "FeatureManagement:UseExternalAI", "false" },
            { "FeatureManagement:UseExternalIntegrations", "false" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var searchFactory = serviceProvider.GetRequiredService<IProviderFactory<ISearchPort>>();
        var chatFactory = serviceProvider.GetRequiredService<IProviderFactory<IChatPort>>();
        var notificationFactory = serviceProvider.GetRequiredService<IProviderFactory<INotificationPort>>();
        var analyticsFactory = serviceProvider.GetRequiredService<IProviderFactory<IAnalyticsPort>>();
        var signatureFactory = serviceProvider.GetRequiredService<IProviderFactory<ISignaturePort>>();
        var aiFactory = serviceProvider.GetRequiredService<IProviderFactory<IAIPort>>();
        var integrationFactory = serviceProvider.GetRequiredService<IProviderFactory<IIntegrationPort>>();

        // Assert - All should return BuiltIn (or Ollama for AI)
        searchFactory.GetActiveProviderName().Should().Be(ProviderTypes.Search.BuiltIn);
        chatFactory.GetActiveProviderName().Should().Be(ProviderTypes.Chat.BuiltIn);
        notificationFactory.GetActiveProviderName().Should().Be(ProviderTypes.Notifications.BuiltIn);
        analyticsFactory.GetActiveProviderName().Should().Be(ProviderTypes.Analytics.BuiltIn);
        signatureFactory.GetActiveProviderName().Should().Be(ProviderTypes.Signatures.BuiltIn);
        aiFactory.GetActiveProviderName().Should().Be(ProviderTypes.AI.Ollama); // AI defaults to Ollama
        integrationFactory.GetActiveProviderName().Should().Be(ProviderTypes.Integrations.BuiltIn);
    }

    /// <summary>
    /// Task 4.8: Integration test - Switch to external provider via config
    /// </summary>
    [Fact]
    public void SearchFactory_WhenExternalEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalSearch", "true" },
            { "Providers:Search:Type", "Meilisearch" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetRequiredService<IProviderFactory<ISearchPort>>();
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Search.Meilisearch);
    }

    /// <summary>
    /// Task 4.8: Verify Chat factory switches to Chatwoot when enabled
    /// </summary>
    [Fact]
    public void ChatFactory_WhenExternalEnabled_ReturnsChatwoot()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalChat", "true" },
            { "Providers:Chat:Type", "Chatwoot" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetRequiredService<IProviderFactory<IChatPort>>();
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Chat.Chatwoot);
    }

    /// <summary>
    /// Task 4.8: Verify Notification factory switches to Novu when enabled
    /// </summary>
    [Fact]
    public void NotificationFactory_WhenExternalEnabled_ReturnsNovu()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalNotifications", "true" },
            { "Providers:Notifications:Type", "Novu" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetRequiredService<IProviderFactory<INotificationPort>>();
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Notifications.Novu);
    }

    /// <summary>
    /// Task 4.8: Verify Analytics factory switches to Superset when enabled
    /// </summary>
    [Fact]
    public void AnalyticsFactory_WhenExternalEnabled_ReturnsSuperset()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalAnalytics", "true" },
            { "Providers:Analytics:Type", "Superset" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetRequiredService<IProviderFactory<IAnalyticsPort>>();
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Analytics.Superset);
    }

    /// <summary>
    /// Task 4.8: Verify multiple providers can be configured independently
    /// </summary>
    [Fact]
    public void MultipleFactories_CanBeConfiguredIndependently()
    {
        // Arrange - Mixed configuration: Search external, others BuiltIn
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalSearch", "true" },
            { "FeatureManagement:UseExternalChat", "false" },
            { "FeatureManagement:UseExternalNotifications", "true" },
            { "FeatureManagement:UseExternalAnalytics", "false" },
            { "Providers:Search:Type", "Algolia" },
            { "Providers:Notifications:Type", "Twilio" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var searchFactory = serviceProvider.GetRequiredService<IProviderFactory<ISearchPort>>();
        var chatFactory = serviceProvider.GetRequiredService<IProviderFactory<IChatPort>>();
        var notificationFactory = serviceProvider.GetRequiredService<IProviderFactory<INotificationPort>>();
        var analyticsFactory = serviceProvider.GetRequiredService<IProviderFactory<IAnalyticsPort>>();

        // Assert - Mixed results based on configuration
        searchFactory.GetActiveProviderName().Should().Be(ProviderTypes.Search.Algolia);
        chatFactory.GetActiveProviderName().Should().Be(ProviderTypes.Chat.BuiltIn);
        notificationFactory.GetActiveProviderName().Should().Be(ProviderTypes.Notifications.Twilio);
        analyticsFactory.GetActiveProviderName().Should().Be(ProviderTypes.Analytics.BuiltIn);
    }

    /// <summary>
    /// Verify AdapterRegistry is singleton and shared across all factories
    /// </summary>
    [Fact]
    public void AdapterRegistry_IsSingleton_SharedAcrossFactories()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>());

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var registry1 = serviceProvider.GetRequiredService<AdapterRegistry>();
        var registry2 = serviceProvider.GetRequiredService<AdapterRegistry>();

        // Assert - Same instance
        registry1.Should().BeSameAs(registry2);
    }

    /// <summary>
    /// Verify FeatureManager is configured and accessible
    /// </summary>
    [Fact]
    public async Task FeatureManager_IsConfigured_CanCheckFlags()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            { "FeatureManagement:UseExternalSearch", "true" },
            { "FeatureManagement:UseExternalChat", "false" }
        });

        var services = new ServiceCollection();
        // Register IConfiguration so factories can resolve it
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPluggableProviders(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();
        var searchEnabled = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch);
        var chatEnabled = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat);

        // Assert
        searchEnabled.Should().BeTrue();
        chatEnabled.Should().BeFalse();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
