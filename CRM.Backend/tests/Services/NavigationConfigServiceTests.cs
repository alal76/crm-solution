// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class NavigationConfigServiceTests
{
    private readonly Mock<IFeatureManager> _featureManager = new();
    private readonly IConfiguration _configuration;
    private readonly Mock<ICrmDbContext> _context = new();
    private readonly Mock<ILogger<NavigationConfigService>> _logger = new();
    private readonly NavigationConfigService _service;

    public NavigationConfigServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Providers:Integrations:Type"] = "BuiltIn",
                ["Providers:Analytics:Type"] = "BuiltIn",
                ["Providers:Search:Type"] = "BuiltIn",
                ["Providers:Chat:Type"] = "BuiltIn",
                ["Providers:Notifications:Type"] = "BuiltIn",
                ["Providers:AI:Type"] = "Ollama",
                ["Providers:Signatures:Type"] = "BuiltIn"
            })
            .Build();

        _featureManager
            .Setup(f => f.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _service = new NavigationConfigService(
            _featureManager.Object,
            _configuration,
            _context.Object,
            _logger.Object);
    }

    private void SetupUserGroups(int userId, bool isSystemAdmin, bool canAccessSettings, bool canAccessUserManagement)
    {
        var group = new UserGroup
        {
            Id = 1,
            Name = "Test Group",
            IsActive = true,
            IsDeleted = false,
            IsSystemAdmin = isSystemAdmin,
            CanAccessSettings = canAccessSettings,
            CanAccessUserManagement = canAccessUserManagement
        };

        var memberships = new List<UserGroupMember>
        {
            new()
            {
                UserId = userId,
                UserGroupId = group.Id,
                UserGroup = group
            }
        };

        _context.Setup(c => c.UserGroupMembers)
            .Returns(MockDbSetFactory.CreateMockDbSet(memberships).Object);
    }

    [Fact]
    public async Task GetNavigationConfigAsync_ShouldIncludeCriticalAdminItems()
    {
        var config = await _service.GetNavigationConfigAsync();

        config.NavItems.Select(item => item.Id).Should().Contain(new[]
        {
            "workflow-settings",
            "workflow-monitor",
            "llm-settings",
            "integrations",
            "analytics-settings",
            "navigation-settings"
        });
    }

    [Fact]
    public async Task GetNavigationConfigForUserAsync_ShouldFilterAdminItems_WhenNoAdminAccess()
    {
        SetupUserGroups(userId: 7, isSystemAdmin: false, canAccessSettings: false, canAccessUserManagement: false);

        var config = await _service.GetNavigationConfigForUserAsync(7);

        config.NavItems.Select(item => item.Id).Should().NotContain("navigation-settings");
        config.NavItems.Select(item => item.Id).Should().Contain("dashboard");
    }
}
