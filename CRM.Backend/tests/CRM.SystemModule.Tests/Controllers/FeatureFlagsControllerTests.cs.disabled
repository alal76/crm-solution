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
using Xunit;

namespace CRM.SystemModule.Tests.Controllers;

/// <summary>
/// Unit tests for FeatureFlags controller functionality.
/// Tests feature flag API endpoints behavior.
/// </summary>
public class FeatureFlagsControllerTests
{
    [Fact]
    public async Task GetFeatureFlags_ReturnsAllFlags()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:UseExternalChat", "false" },
                { "FeatureManagement:UseExternalSearch", "false" },
                { "FeatureManagement:EnableITSM", "true" },
                { "FeatureManagement:EnableMarketing", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act
        var flags = new[]
        {
            FeatureFlags.UseExternalChat,
            FeatureFlags.UseExternalSearch,
            FeatureFlags.EnableITSM,
            FeatureFlags.EnableMarketing
        };

        var results = new List<bool>();
        foreach (var flag in flags)
        {
            results.Add(await featureManager.IsEnabledAsync(flag));
        }

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal(4, results.Count);
    }

    [Fact]
    public async Task GetFeatureFlag_WithValidName_ReturnsFlag()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:UseExternalSearch", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act
        var result = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ToggleFeatureFlag_ChangesState()
    {
        // Arrange
        var initialConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:UseExternalAI", "false" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(initialConfig.GetSection("FeatureManagement"));
        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act
        var initialState = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI);

        // Assert
        Assert.False(initialState);
    }

    [Fact]
    public async Task ProviderSelection_WithAllBuiltIn_ReturnsFalse()
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

        // Act
        var chat = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat);
        var search = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch);
        var notifications = await featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications);

        // Assert
        Assert.False(chat);
        Assert.False(search);
        Assert.False(notifications);
    }

    [Fact]
    public async Task ModuleSelection_WithMixedConfig_ReturnsCorrectValues()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "FeatureManagement:EnableITSM", "true" },
                { "FeatureManagement:EnableMarketing", "false" },
                { "FeatureManagement:EnableKnowledgeBase", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        var serviceProvider = services.BuildServiceProvider();
        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

        // Act
        var itsm = await featureManager.IsEnabledAsync(FeatureFlags.EnableITSM);
        var marketing = await featureManager.IsEnabledAsync(FeatureFlags.EnableMarketing);
        var kb = await featureManager.IsEnabledAsync(FeatureFlags.EnableKnowledgeBase);

        // Assert
        Assert.True(itsm);
        Assert.False(marketing);
        Assert.True(kb);
    }
}
