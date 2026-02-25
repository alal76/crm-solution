// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Tests for the EnableWebhooks / UseWebhooks feature flags (TODO-INT001-15).
/// Verifies:
///   1. FeatureFlags.EnableWebhooks and FeatureFlags.UseWebhooks constants have expected values.
///   2. When EnableWebhooks = true in config, IFeatureManager.IsEnabledAsync returns true.
///   3. When EnableWebhooks = false in config, IFeatureManager.IsEnabledAsync returns false.
///
/// MANDATORY: Written after reading FeatureFlags.cs.
/// Class: static FeatureFlags, Namespace: CRM.Core.Features
/// </summary>
public class WebhookFeatureFlagTests
{
    // ── Constant values ───────────────────────────────────────────────────────

    [Fact]
    public void EnableWebhooks_Constant_HasExpectedValue()
    {
        FeatureFlags.EnableWebhooks.Should().Be("EnableWebhooks");
    }

    [Fact]
    public void UseWebhooks_Constant_HasExpectedValue()
    {
        FeatureFlags.UseWebhooks.Should().Be("UseWebhooks");
    }

    // ── Feature management evaluation ────────────────────────────────────────

    [Fact]
    public async Task FeatureManager_EnableWebhooksTrue_IsEnabled()
    {
        var featureManager = BuildFeatureManager(enableWebhooks: true);

        var result = await featureManager.IsEnabledAsync(FeatureFlags.EnableWebhooks);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task FeatureManager_EnableWebhooksFalse_IsDisabled()
    {
        var featureManager = BuildFeatureManager(enableWebhooks: false);

        var result = await featureManager.IsEnabledAsync(FeatureFlags.EnableWebhooks);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task FeatureManager_UseWebhooksTrue_IsEnabled()
    {
        var featureManager = BuildFeatureManager(useWebhooks: true);

        var result = await featureManager.IsEnabledAsync(FeatureFlags.UseWebhooks);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task FeatureManager_UseWebhooksFalse_IsDisabled()
    {
        var featureManager = BuildFeatureManager(useWebhooks: false);

        var result = await featureManager.IsEnabledAsync(FeatureFlags.UseWebhooks);

        result.Should().BeFalse();
    }

    // ── Both flags can be controlled independently ──────────────────────────

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task BothWebhookFlags_CanBeControlledIndependently(bool enableWebhooks, bool useWebhooks)
    {
        var featureManager = BuildFeatureManager(enableWebhooks, useWebhooks);

        var enabledResult = await featureManager.IsEnabledAsync(FeatureFlags.EnableWebhooks);
        var useResult = await featureManager.IsEnabledAsync(FeatureFlags.UseWebhooks);

        enabledResult.Should().Be(enableWebhooks);
        useResult.Should().Be(useWebhooks);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IFeatureManager BuildFeatureManager(bool enableWebhooks = true, bool useWebhooks = true)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"FeatureManagement:{FeatureFlags.EnableWebhooks}"] = enableWebhooks.ToString().ToLower(),
                [$"FeatureManagement:{FeatureFlags.UseWebhooks}"] = useWebhooks.ToString().ToLower()
            })
            .Build();

        var services = new ServiceCollection();
        services.AddFeatureManagement(config.GetSection("FeatureManagement"));

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IFeatureManager>();
    }
}
