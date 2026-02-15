// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using CRM.Core.Features;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for feature flag service integration.
/// Tests that feature flags load and function correctly.
/// </summary>
public class FeatureFlagServiceTests
{
    [Fact]
    public async Task ProviderFlags_WhenAllBuiltIn_ReturnsFalse()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:UseExternalChat", "false" },
                { "FeatureManagement:UseExternalSearch", "false" },
                { "FeatureManagement:UseExternalNotifications", "false" },
                { "FeatureManagement:UseExternalAnalytics", "false" },
                { "FeatureManagement:UseExternalSignatures", "false" },
                { "FeatureManagement:UseExternalAI", "false" },
                { "FeatureManagement:UseExternalIntegrations", "false" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));

        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act & Assert
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations));
    }

    [Fact]
    public async Task ProviderFlags_WhenAllExternal_ReturnsTrue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:UseExternalChat", "true" },
                { "FeatureManagement:UseExternalSearch", "true" },
                { "FeatureManagement:UseExternalNotifications", "true" },
                { "FeatureManagement:UseExternalAnalytics", "true" },
                { "FeatureManagement:UseExternalSignatures", "true" },
                { "FeatureManagement:UseExternalAI", "true" },
                { "FeatureManagement:UseExternalIntegrations", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));

        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act & Assert
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations));
    }

    [Fact]
    public async Task ModuleFlags_WhenConfigured_LoadsCorrectly()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:EnableITSM", "true" },
                { "FeatureManagement:EnableMarketing", "true" },
                { "FeatureManagement:EnableCustomerPortal", "false" },
                { "FeatureManagement:EnablePartnerPortal", "false" },
                { "FeatureManagement:EnableKnowledgeBase", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));

        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act & Assert
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.EnableITSM));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.EnableMarketing));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.EnableCustomerPortal));
        Assert.False(await featureManager.IsEnabledAsync(FeatureFlags.EnablePartnerPortal));
        Assert.True(await featureManager.IsEnabledAsync(FeatureFlags.EnableKnowledgeBase));
    }

    [Fact]
    public async Task AllFeatureNames_AreValid()
    {
        // Arrange
        var featureNames = new[]
        {
            FeatureFlags.UseExternalChat,
            FeatureFlags.UseExternalSearch,
            FeatureFlags.UseExternalNotifications,
            FeatureFlags.UseExternalAnalytics,
            FeatureFlags.UseExternalSignatures,
            FeatureFlags.UseExternalAI,
            FeatureFlags.UseExternalIntegrations,
            FeatureFlags.EnableITSM,
            FeatureFlags.EnableMarketing,
            FeatureFlags.EnableCustomerPortal,
            FeatureFlags.EnablePartnerPortal,
            FeatureFlags.EnableKnowledgeBase
        };

        // Act & Assert - All names should not contain colons (not allowed by Microsoft.FeatureManagement)
        foreach (var name in featureNames)
        {
            Assert.DoesNotContain(":", name);
        }
    }
}
