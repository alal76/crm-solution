// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SYS009-003: Unit tests for provider-aware navigation with feature flags.

using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Tests for <see cref="NavigationConfigService"/> provider-aware feature flag navigation (TODO-SYS009-003).
/// Verifies that ITSM and Marketing nav items are conditionally included based on feature flags.
/// </summary>
public class NavigationConfigServiceTests
{
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ICrmDbContext> _mockContext;

    public NavigationConfigServiceTests()
    {
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockContext = new Mock<ICrmDbContext>();

        // By default all features are disabled; individual tests override as needed
        _mockFeatureManager
            .Setup(fm => fm.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
    }

    private NavigationConfigService CreateService(IConfiguration? config = null)
    {
        config ??= new ConfigurationBuilder().Build();
        return new NavigationConfigService(
            _mockFeatureManager.Object,
            config,
            _mockContext.Object,
            NullLogger<NavigationConfigService>.Instance);
    }

    [Fact]
    public async Task GetAvailableNavItemsAsync_ExcludesItsmItems_WhenItsmFeatureDisabled()
    {
        // EnableITSM = false (default mock setup)
        var service = CreateService();

        var items = (await service.GetAvailableNavItemsAsync()).ToList();

        items.Should().NotContain(n => n.Category == "itsm",
            "ITSM navigation items must be hidden when EnableITSM feature flag is false");
    }

    [Fact]
    public async Task GetAvailableNavItemsAsync_IncludesItsmItems_WhenItsmFeatureEnabled()
    {
        _mockFeatureManager
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableITSM))
            .ReturnsAsync(true);

        var service = CreateService();

        var items = (await service.GetAvailableNavItemsAsync()).ToList();

        items.Should().Contain(n => n.Category == "itsm",
            "ITSM navigation items must be visible when EnableITSM feature flag is true");
    }

    [Fact]
    public async Task GetAvailableNavItemsAsync_ExcludesMarketingItems_WhenMarketingDisabled()
    {
        // EnableMarketing = false (default mock setup)
        var service = CreateService();

        var items = (await service.GetAvailableNavItemsAsync()).ToList();

        items.Should().NotContain(n => n.Category == "marketing",
            "Marketing navigation items must be hidden when EnableMarketing feature flag is false");
    }

    [Fact]
    public async Task GetAvailableNavItemsAsync_IncludesMarketingItems_WhenMarketingEnabled()
    {
        _mockFeatureManager
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableMarketing))
            .ReturnsAsync(true);

        var service = CreateService();

        var items = (await service.GetAvailableNavItemsAsync()).ToList();

        items.Should().Contain(n => n.Category == "marketing",
            "Marketing navigation items must be visible when EnableMarketing feature flag is true");
    }

    [Fact]
    public async Task GetNavigationConfigAsync_AlwaysIncludesCoreNavItems()
    {
        // Core items like Dashboard/Accounts should always be present regardless of feature flags
        var service = CreateService();

        var config = await service.GetNavigationConfigAsync();

        config.NavItems.Should().Contain(n => n.Id == "dashboard");
        config.NavItems.Should().Contain(n => n.Id == "accounts");
        config.NavItems.Should().Contain(n => n.Id == "contacts");
    }
}
