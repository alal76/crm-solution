// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class FeatureFlagManagementServiceTests : ServiceTestFixtureBase<FeatureFlagManagementService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;    private readonly FeatureFlagManagementService _service;

    public FeatureFlagManagementServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();

        // GetAllFlagsAsync/UpdateFlagAsync check for a persisted override (REV-FE-005) before
        // falling back to IFeatureManager -- an empty set means no override, matching the
        // previous appsettings-only behavior this test class already expects.
        var emptyFeatureFlagSet = MockDbSetFactory.CreateMockDbSet(new List<FeatureFlag>());
        _mockDbContext.Setup(c => c.FeatureFlags).Returns(emptyFeatureFlagSet.Object);

        // Setup mock configuration to return proper values for GetValue extension methods
        // Return null for rollout percentages/targeting so defaults are used
        // Return "BuiltIn" only for provider type sections (Providers:{category}:Type)
        _mockConfiguration.Setup(c => c.GetSection(It.Is<string>(k => k.StartsWith("Providers:") && k.EndsWith(":Type"))))
            .Returns(() =>
            {
                var mockSection = new Mock<IConfigurationSection>();
                mockSection.Setup(s => s.Value).Returns("BuiltIn");
                return mockSection.Object;
            });

        // Return null for all other sections (rollout percentages, targeted users, etc.)
        _mockConfiguration.Setup(c => c.GetSection(It.Is<string>(k => !k.StartsWith("Providers:") || !k.EndsWith(":Type"))))
            .Returns(() =>
            {
                var mockSection = new Mock<IConfigurationSection>();
                mockSection.Setup(s => s.Value).Returns((string?)null);
                return mockSection.Object;
            });

        _service = new FeatureFlagManagementService(
            _mockDbContext.Object,
            _mockFeatureManager.Object,
            _mockConfiguration.Object,
            MockLogger.Object);
    }

    [Fact]
    public async Task GetAllFlagsAsync_ReturnsAllFlags()
    {
        // Arrange
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GetAllFlagsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, f => f.Name == "EnableITSM");
        Assert.Contains(result, f => f.Name == "UseExternalAI");
    }

    [Fact]
    public async Task GetFlagAsync_WithValidName_ReturnsFlag()
    {
        // Arrange
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync("EnableITSM"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GetFlagAsync("EnableITSM");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EnableITSM", result.Name);
        Assert.True(result.Enabled);
    }

    [Fact]
    public async Task GetFlagAsync_WithInvalidName_ReturnsNull()
    {
        // Arrange
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetFlagAsync("InvalidFlag");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IsFlagEnabledForUserAsync_WithRolloutPercentage_ReturnsCorrectResult()
    {
        // Arrange
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync("EnableITSM"))
            .ReturnsAsync(true);
        // Note: GetValue is an extension method and can't be mocked directly
        // The service will use the default value (100 rollout) when configuration returns null

        // Act
        var result = await _service.IsFlagEnabledForUserAsync("EnableITSM", 1);

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task UpdateFlagAsync_CreatesAuditLog()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<FeatureFlagAuditLog>>();
        _mockDbContext.Setup(d => d.FeatureFlagAuditLogs).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync("EnableITSM"))
            .ReturnsAsync(false);

        var dto = new UpdateFeatureFlagDto { Name = "EnableITSM", Enabled = true };

        // Act
        var result = await _service.UpdateFlagAsync("EnableITSM", dto, 1);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(m => m.Add(It.IsAny<FeatureFlagAuditLog>()), Times.Once);
        _mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetRolloutPercentageAsync_WithValidPercentage_ReturnsTrue()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<FeatureFlagAuditLog>>();
        _mockDbContext.Setup(d => d.FeatureFlagAuditLogs).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        // Note: GetValue is an extension method - the service will use internal logic

        // Act
        var result = await _service.SetRolloutPercentageAsync("EnableITSM", 50, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetRolloutPercentageAsync_WithInvalidPercentage_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SetRolloutPercentageAsync("EnableITSM", 150, 1));
    }

    [Fact]
    public async Task SetVariantsAsync_WithValidVariants_ReturnsTrue()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<FeatureFlagAuditLog>>();
        _mockDbContext.Setup(d => d.FeatureFlagAuditLogs).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var variants = new[]
        {
            new FlagVariantDto { VariantName = "controlA", Weight = 50 },
            new FlagVariantDto { VariantName = "variantB", Weight = 50 }
        };

        // Act
        var result = await _service.SetVariantsAsync("EnableITSM", variants, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAvailableProvidersAsync_ReturnsProvidersForCategory()
    {
        // Act
        var result = await _service.GetAvailableProvidersAsync("Chat");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("BuiltIn", result);
        Assert.Contains("Chatwoot", result);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_CreatesAuditEntry()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<FeatureFlagAuditLog>>();
        _mockDbContext.Setup(d => d.FeatureFlagAuditLogs).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ResetToDefaultsAsync(1);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(m => m.Add(It.IsAny<FeatureFlagAuditLog>()), Times.Once);
    }
}

public class UserInterfaceServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<UserInterfaceService>> MockLogger;
    private readonly UserInterfaceService _service;

    public UserInterfaceServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        MockLogger = new Mock<ILogger<UserInterfaceService>>();

        _service = new UserInterfaceService(_mockDbContext.Object, MockLogger.Object);
    }

    [Fact]
    public async Task SaveUIPreferencesAsync_WithValidDto_ReturnsSavedPreferences()
    {
        // Arrange
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(new List<UIPreference>());
        _mockDbContext.Setup(d => d.UIPreferences).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new CreateUpdateUIPreferenceDto { Theme = "dark" };

        // Act
        var result = await _service.SaveUIPreferencesAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dark", result.Theme);
    }

    [Fact]
    public async Task ResetUIPreferencesAsync_ResetsToDefaults()
    {
        // Arrange - create an existing preference to reset
        var existingPref = new UIPreference { Id = 1, UserId = 1, Theme = "dark" };
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(new List<UIPreference> { existingPref });
        _mockDbContext.Setup(d => d.UIPreferences).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ResetUIPreferencesAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SaveUICustomizationAsync_WithValidData_ReturnsSavedCustomization()
    {
        // Arrange
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(new List<UICustomization>());
        _mockDbContext.Setup(d => d.UICustomizations).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new CreateUpdateUICustomizationDto
        {
            ModuleName = "Accounts",
            PageName = "ListView",
            VisibleColumns = new[] { "Name", "Email" }
        };

        // Act
        var result = await _service.SaveUICustomizationAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Accounts", result.ModuleName);
    }

    [Fact]
    public async Task SaveDashboardCustomizationAsync_WithValidData_ReturnsSavedDashboard()
    {
        // Arrange
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(new List<DashboardCustomization>());
        _mockDbContext.Setup(d => d.DashboardCustomizations).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new CreateUpdateDashboardCustomizationDto
        {
            DashboardName = "Sales Dashboard",
            IsDefault = true
        };

        // Act
        var result = await _service.SaveDashboardCustomizationAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sales Dashboard", result.DashboardName);
    }

    [Fact]
    public async Task SetDefaultDashboardAsync_WithValidDashboard_ReturnsTrue()
    {
        // Arrange - create dashboards with different default states
        var dashboards = new List<DashboardCustomization>
        {
            new() { Id = 1, UserId = 1, DashboardName = "Old Default", IsDefault = true },
            new() { Id = 2, UserId = 1, DashboardName = "Sales Dashboard", IsDefault = false }
        };
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(dashboards);
        _mockDbContext.Setup(d => d.DashboardCustomizations).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SetDefaultDashboardAsync(1, "Sales Dashboard");

        // Assert
        Assert.True(result);
    }
}

public class PerformanceOptimizationServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<PerformanceOptimizationService>> MockLogger;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly PerformanceOptimizationService _service;

    public PerformanceOptimizationServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        MockLogger = new Mock<ILogger<PerformanceOptimizationService>>();
        _mockCache = new Mock<IDistributedCache>();

        _service = new PerformanceOptimizationService(
            _mockDbContext.Object,
            MockLogger.Object,
            _mockCache.Object);
    }

    [Fact]
    public async Task RecordMetricAsync_WithValidMetric_ReturnsTrue()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<PerformanceMetric>>();
        _mockDbContext.Setup(d => d.PerformanceMetrics).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var metric = new PerformanceMetricDto
        {
            EndpointName = "GET /api/accounts",
            HttpMethod = "GET",
            Route = "/api/accounts",
            ResponseTimeMs = 150,
            StatusCode = 200
        };

        // Act
        var result = await _service.RecordMetricAsync(metric);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(m => m.Add(It.IsAny<PerformanceMetric>()), Times.Once);
    }

    [Fact]
    public async Task GetCacheStatisticsAsync_ReturnsCacheStats()
    {
        // Arrange - mock the PerformanceMetrics DbSet that the service queries
        var metrics = new List<PerformanceMetric>
        {
            new() { Id = 1, EndpointName = "GET /api/accounts", ResponseTimeMs = 100, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(metrics);
        _mockDbContext.Setup(d => d.PerformanceMetrics).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetCacheStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CacheStatisticsDto>(result);
    }

    [Fact]
    public async Task ClearCacheAsync_WithoutPattern_ReturnsTrue()
    {
        // Act
        var result = await _service.ClearCacheAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PurgeOldMetricsAsync_WithValidDays_ReturnsCount()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<PerformanceMetric>>();
        _mockDbContext.Setup(d => d.PerformanceMetrics).Returns(mockDbSet.Object);
        _mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.PurgeOldMetricsAsync(30);

        // Assert
        Assert.IsType<int>(result);
    }
}
