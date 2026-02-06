// CRM Solution - Adapter Registry Tests
// Tests for the central provider health monitoring registry

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Infrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Factories;

/// <summary>
/// Unit tests for AdapterRegistry.
/// Tests health monitoring, status tracking, and metrics for pluggable providers.
/// </summary>
public class AdapterRegistryTests
{
    private readonly Mock<ILogger<AdapterRegistry>> _mockLogger;

    public AdapterRegistryTests()
    {
        _mockLogger = new Mock<ILogger<AdapterRegistry>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_CreatesRegistry()
    {
        // Act
        var registry = CreateRegistry();

        // Assert
        registry.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AdapterRegistry(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region Register Tests

    [Fact]
    public void Register_WithValidParameters_AddsAdapter()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act
        registry.Register("Search", "Meilisearch", isActive: true);

        // Assert
        var adapters = registry.GetAllAdapters();
        adapters.Should().ContainSingle();
        adapters.First().Category.Should().Be("Search");
        adapters.First().ProviderName.Should().Be("Meilisearch");
        adapters.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public void Register_MultipleAdapters_AllAreTracked()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act
        registry.Register("Search", "Meilisearch", isActive: true);
        registry.Register("Search", "Algolia", isActive: false);
        registry.Register("Chat", "Chatwoot", isActive: true);

        // Assert
        var adapters = registry.GetAllAdapters().ToList();
        adapters.Should().HaveCount(3);
    }

    [Fact]
    public void Register_SameAdapterTwice_UpdatesExisting()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch", isActive: false);

        // Act
        registry.Register("Search", "Meilisearch", isActive: true);

        // Assert
        var adapters = registry.GetAllAdapters();
        adapters.Should().ContainSingle();
        adapters.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public void Register_SetsRegisteredAtTimestamp()
    {
        // Arrange
        var registry = CreateRegistry();
        var beforeRegister = DateTime.UtcNow;

        // Act
        registry.Register("Search", "Meilisearch");

        // Assert
        var adapter = registry.GetAllAdapters().First();
        adapter.RegisteredAt.Should().BeOnOrAfter(beforeRegister);
        adapter.RegisteredAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Register_SetsDefaultStatusToUnknown()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act
        registry.Register("Search", "Meilisearch");

        // Assert
        var adapter = registry.GetAllAdapters().First();
        adapter.Status.Should().Be(AdapterStatus.Unknown);
    }

    #endregion

    #region SetActive Tests

    [Fact]
    public void SetActive_DeactivatesOtherProvidersInCategory()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch", isActive: true);
        registry.Register("Search", "Algolia", isActive: false);
        registry.Register("Chat", "Chatwoot", isActive: true);

        // Act
        registry.SetActive("Search", "Algolia");

        // Assert
        var meilisearch = registry.GetAdapter("Search", "Meilisearch");
        var algolia = registry.GetAdapter("Search", "Algolia");
        var chatwoot = registry.GetAdapter("Chat", "Chatwoot");

        meilisearch!.IsActive.Should().BeFalse();
        algolia!.IsActive.Should().BeTrue();
        chatwoot!.IsActive.Should().BeTrue(); // Different category, unchanged
    }

    [Fact]
    public void SetActive_WithUnregisteredProvider_DoesNothing()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch", isActive: true);

        // Act
        registry.SetActive("Search", "NonExistent");

        // Assert
        var meilisearch = registry.GetAdapter("Search", "Meilisearch");
        meilisearch!.IsActive.Should().BeFalse(); // Still deactivated all in category
    }

    #endregion

    #region UpdateHealth Tests

    [Fact]
    public void UpdateHealth_WithHealthyResult_SetsStatusToHealthy()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        var healthResult = HealthCheckResult.Healthy("All good");

        // Act
        registry.UpdateHealth("Search", "Meilisearch", healthResult);

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.Status.Should().Be(AdapterStatus.Healthy);
        adapter.LastHealthMessage.Should().Be("All good");
    }

    [Fact]
    public void UpdateHealth_WithDegradedResult_SetsStatusToDegraded()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        var healthResult = HealthCheckResult.Degraded("Performance degraded");

        // Act
        registry.UpdateHealth("Search", "Meilisearch", healthResult);

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.Status.Should().Be(AdapterStatus.Degraded);
        adapter.FailureCount.Should().Be(1);
    }

    [Fact]
    public void UpdateHealth_WithUnhealthyResult_SetsStatusToUnhealthy()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        var healthResult = HealthCheckResult.Unhealthy("Service unavailable");

        // Act
        registry.UpdateHealth("Search", "Meilisearch", healthResult);

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.Status.Should().Be(AdapterStatus.Unhealthy);
        adapter.FailureCount.Should().Be(1);
        adapter.LastFailureTime.Should().NotBeNull();
    }

    [Fact]
    public void UpdateHealth_IncrementsHealthCheckCount()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Healthy());
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Healthy());
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Healthy());

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.HealthCheckCount.Should().Be(3);
    }

    [Fact]
    public void UpdateHealth_SetsLastHealthCheckTimestamp()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Healthy());

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.LastHealthCheck.Should().NotBeNull();
        adapter.LastHealthCheck!.Value.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void UpdateHealth_WithUnregisteredProvider_DoesNothing()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act & Assert (should not throw)
        registry.UpdateHealth("Search", "NonExistent", HealthCheckResult.Healthy());
    }

    #endregion

    #region RecordSuccess Tests

    [Fact]
    public void RecordSuccess_IncrementsSuccessCount()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(200));

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.SuccessCount.Should().Be(2);
    }

    [Fact]
    public void RecordSuccess_AccumulatesTotalOperationTime()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(200));

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.TotalOperationTime.TotalMilliseconds.Should().Be(300);
    }

    [Fact]
    public void RecordSuccess_SetsLastOperationTime()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        var beforeRecord = DateTime.UtcNow;

        // Act
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.LastOperationTime.Should().NotBeNull();
        adapter.LastOperationTime!.Value.Should().BeOnOrAfter(beforeRecord);
    }

    #endregion

    #region RecordFailure Tests

    [Fact]
    public void RecordFailure_IncrementsFailureCount()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        registry.RecordFailure("Search", "Meilisearch", "Connection timeout");
        registry.RecordFailure("Search", "Meilisearch", "Server error");

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.FailureCount.Should().Be(2);
    }

    [Fact]
    public void RecordFailure_SetsLastFailureInfo()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        var beforeRecord = DateTime.UtcNow;

        // Act
        registry.RecordFailure("Search", "Meilisearch", "Connection timeout");

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.LastFailureTime.Should().NotBeNull();
        adapter.LastFailureMessage.Should().Be("Connection timeout");
    }

    #endregion

    #region GetAdaptersByCategory Tests

    [Fact]
    public void GetAdaptersByCategory_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.Register("Search", "Algolia");
        registry.Register("Chat", "Chatwoot");

        // Act
        var searchAdapters = registry.GetAdaptersByCategory("Search").ToList();

        // Assert
        searchAdapters.Should().HaveCount(2);
        searchAdapters.All(a => a.Category == "Search").Should().BeTrue();
    }

    [Fact]
    public void GetAdaptersByCategory_IsCaseInsensitive()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        var adapters = registry.GetAdaptersByCategory("SEARCH").ToList();

        // Assert
        adapters.Should().ContainSingle();
    }

    [Fact]
    public void GetAdaptersByCategory_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        var adapters = registry.GetAdaptersByCategory("NonExistent").ToList();

        // Assert
        adapters.Should().BeEmpty();
    }

    #endregion

    #region GetActiveAdapter Tests

    [Fact]
    public void GetActiveAdapter_ReturnsActiveAdapterForCategory()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch", isActive: false);
        registry.Register("Search", "Algolia", isActive: true);

        // Act
        var active = registry.GetActiveAdapter("Search");

        // Assert
        active.Should().NotBeNull();
        active!.ProviderName.Should().Be("Algolia");
    }

    [Fact]
    public void GetActiveAdapter_WithNoActiveAdapter_ReturnsNull()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch", isActive: false);

        // Act
        var active = registry.GetActiveAdapter("Search");

        // Assert
        active.Should().BeNull();
    }

    #endregion

    #region GetHealthSummary Tests

    [Fact]
    public void GetHealthSummary_ReturnsCorrectCounts()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.Register("Search", "Algolia");
        registry.Register("Chat", "Chatwoot");
        registry.Register("Chat", "Intercom");
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Healthy());
        registry.UpdateHealth("Search", "Algolia", HealthCheckResult.Degraded());
        registry.UpdateHealth("Chat", "Chatwoot", HealthCheckResult.Unhealthy());
        // Chat:Intercom remains Unknown

        // Act
        var summary = registry.GetHealthSummary();

        // Assert
        summary.TotalAdapters.Should().Be(4);
        summary.HealthyCount.Should().Be(1);
        summary.DegradedCount.Should().Be(1);
        summary.UnhealthyCount.Should().Be(1);
        summary.UnknownCount.Should().Be(1);
    }

    [Fact]
    public void GetHealthSummary_ReturnsActiveAdaptersList()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch", isActive: true);
        registry.Register("Chat", "Chatwoot", isActive: true);
        registry.Register("Chat", "Intercom", isActive: false);

        // Act
        var summary = registry.GetHealthSummary();

        // Assert
        summary.ActiveAdapters.Should().HaveCount(2);
        summary.ActiveAdapters.Should().Contain("Search:Meilisearch");
        summary.ActiveAdapters.Should().Contain("Chat:Chatwoot");
    }

    [Fact]
    public void GetHealthSummary_IsOverallHealthy_WhenNoFailures()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.Register("Chat", "Chatwoot");
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Healthy());
        registry.UpdateHealth("Chat", "Chatwoot", HealthCheckResult.Healthy());

        // Act
        var summary = registry.GetHealthSummary();

        // Assert
        summary.IsOverallHealthy.Should().BeTrue();
    }

    [Fact]
    public void GetHealthSummary_IsNotOverallHealthy_WhenDegraded()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.UpdateHealth("Search", "Meilisearch", HealthCheckResult.Degraded());

        // Act
        var summary = registry.GetHealthSummary();

        // Assert
        summary.IsOverallHealthy.Should().BeFalse();
    }

    #endregion

    #region HealthCheckAllAsync Tests

    [Fact]
    public async Task HealthCheckAllAsync_ChecksAllRegisteredAdapters()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.Register("Chat", "Chatwoot");

        Func<string, string, Task<HealthCheckResult>> healthCheckFunc =
            (category, provider) => Task.FromResult(HealthCheckResult.Healthy($"{category}:{provider} OK"));

        // Act
        var results = await registry.HealthCheckAllAsync(healthCheckFunc);

        // Assert
        results.Should().HaveCount(2);
        results.Values.All(r => r.Status == HealthStatus.Healthy).Should().BeTrue();
    }

    [Fact]
    public async Task HealthCheckAllAsync_UpdatesAdapterHealth()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        Func<string, string, Task<HealthCheckResult>> healthCheckFunc =
            (_, _) => Task.FromResult(HealthCheckResult.Healthy());

        // Act
        await registry.HealthCheckAllAsync(healthCheckFunc);

        // Assert
        var adapter = registry.GetAdapter("Search", "Meilisearch");
        adapter!.Status.Should().Be(AdapterStatus.Healthy);
        adapter.HealthCheckCount.Should().Be(1);
    }

    [Fact]
    public async Task HealthCheckAllAsync_HandlesExceptions()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        Func<string, string, Task<HealthCheckResult>> healthCheckFunc =
            (_, _) => throw new Exception("Connection failed");

        // Act
        var results = await registry.HealthCheckAllAsync(healthCheckFunc);

        // Assert
        results.Values.Single().Status.Should().Be(HealthStatus.Unhealthy);
        results.Values.Single().Description.Should().Contain("Connection failed");
    }

    #endregion

    #region AdapterInfo Metrics Tests

    [Fact]
    public void AdapterInfo_AverageOperationTimeMs_CalculatesCorrectly()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(200));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(300));

        // Act
        var adapter = registry.GetAdapter("Search", "Meilisearch");

        // Assert
        adapter!.AverageOperationTimeMs.Should().Be(200);
    }

    [Fact]
    public void AdapterInfo_AverageOperationTimeMs_WithNoOperations_ReturnsZero()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        var adapter = registry.GetAdapter("Search", "Meilisearch");

        // Assert
        adapter!.AverageOperationTimeMs.Should().Be(0);
    }

    [Fact]
    public void AdapterInfo_SuccessRate_CalculatesCorrectly()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));
        registry.RecordSuccess("Search", "Meilisearch", TimeSpan.FromMilliseconds(100));
        registry.RecordFailure("Search", "Meilisearch");

        // Act
        var adapter = registry.GetAdapter("Search", "Meilisearch");

        // Assert
        adapter!.SuccessRate.Should().Be(75); // 3/4 = 75%
    }

    [Fact]
    public void AdapterInfo_SuccessRate_WithNoOperations_ReturnsZero()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.Register("Search", "Meilisearch");

        // Act
        var adapter = registry.GetAdapter("Search", "Meilisearch");

        // Assert
        adapter!.SuccessRate.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private AdapterRegistry CreateRegistry()
    {
        return new AdapterRegistry(_mockLogger.Object);
    }

    #endregion
}
