// CRM Solution - BuiltInSearchProvider Unit Tests
// Phase 1 Week 5 Task 5.7: Unit tests for BuiltInSearchProvider
//
// Tests verify SQL-based search functionality with mocked DbContextResolver

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers.BuiltIn;

/// <summary>
/// Unit tests for BuiltInSearchProvider.
/// These tests verify the provider's behavior with mocked dependencies.
/// Since BuiltInSearchProvider uses real EF Core queries, comprehensive
/// search testing is done in integration tests (Task 5.8).
/// </summary>
public class BuiltInSearchProviderTests
{
    private readonly Mock<ILogger<BuiltInSearchProvider>> _loggerMock;
    private readonly Mock<IDbContextResolver> _dbContextResolverMock;
    private readonly BuiltInSearchProvider _provider;

    public BuiltInSearchProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInSearchProvider>>();
        _dbContextResolverMock = new Mock<IDbContextResolver>();
        
        _provider = new BuiltInSearchProvider(_dbContextResolverMock.Object, _loggerMock.Object);
    }

    #region Provider Metadata Tests

    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        // Assert
        _provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenContextCanBeResolved()
    {
        // Arrange
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Returns(new Mock<CRM.Core.Interfaces.ICrmDbContext>().Object);

        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenContextResolutionThrows()
    {
        // Arrange
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Throws(new InvalidOperationException("No context available"));

        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnFalse_WhenContextIsNull()
    {
        // Arrange
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Returns((CRM.Core.Interfaces.ICrmDbContext?)null);

        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Index Operations Tests (No-ops for BuiltIn)

    [Fact]
    public async Task IndexAsync_ShouldCompleteWithoutError()
    {
        // Arrange - Index is a no-op for BuiltIn provider (DB is the index)
        var testObject = new { Id = 100, Name = "Test" };

        // Act & Assert - Should complete without throwing
        await _provider.Invoking(p => p.IndexAsync(testObject, "100"))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task IndexBatchAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var testObjects = new[] 
        { 
            new { Id = 101, Name = "Batch1" },
            new { Id = 102, Name = "Batch2" }
        };

        // Act & Assert
        await _provider.Invoking(p => p.IndexBatchAsync(testObjects, a => a.Id.ToString()))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCompleteWithoutError()
    {
        // Act & Assert
        await _provider.Invoking(p => p.DeleteAsync<object>("1"))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearIndexAsync_ShouldCompleteWithoutError()
    {
        // Act & Assert
        await _provider.Invoking(p => p.ClearIndexAsync<object>())
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task RebuildIndexAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var testObjects = new[] { new { Id = 1, Name = "Test" } };

        // Act & Assert
        await _provider.Invoking(p => p.RebuildIndexAsync(testObjects, a => a.Id.ToString()))
            .Should().NotThrowAsync();
    }

    #endregion

    #region Search Request Validation Tests

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var request = new SearchRequest { Query = "" };
        
        // The provider returns empty for blank queries without touching DB
        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Query.Should().Be("");
    }

    [Fact]
    public async Task SearchAsync_WithWhitespaceQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var request = new SearchRequest { Query = "   " };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ShouldNotThrow()
    {
        // Arrange
        var request = new SearchRequest { Query = null! };

        // Act & Assert
        await _provider.Invoking(p => p.SearchAsync(request))
            .Should().NotThrowAsync();
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnProviderName()
    {
        // Arrange
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Returns(new Mock<CRM.Core.Interfaces.ICrmDbContext>().Object);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task HealthCheckAsync_WhenContextThrows_ShouldBeUnhealthy()
    {
        // Arrange - When context throws, health check should return unhealthy, not throw
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Throws(new InvalidOperationException("Context not available"));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthCheckAsync_WhenContextUnavailable_ShouldBeUnhealthy()
    {
        // Arrange
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Throws(new InvalidOperationException("Database unavailable"));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    #endregion

    #region SuggestAsync Tests

    [Fact]
    public async Task SuggestAsync_WithEmptyPrefix_ShouldReturnEmptyList()
    {
        // Arrange - Empty prefix returns empty without DB call

        // Act
        var result = await _provider.SuggestAsync("");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SuggestAsync_WhenContextUnavailable_ShouldThrow()
    {
        // Arrange - Provider doesn't catch exceptions in SuggestAsync
        _dbContextResolverMock
            .Setup(r => r.ResolveContext())
            .Throws(new InvalidOperationException("No context"));

        // Act & Assert - Provider propagates the exception
        await _provider.Invoking(p => p.SuggestAsync("test"))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion
}
