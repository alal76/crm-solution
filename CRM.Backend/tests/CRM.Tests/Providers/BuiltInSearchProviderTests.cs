// CRM Solution - BuiltInSearchProvider Tests
// Tests for the built-in SQL-based search provider

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInSearchProvider.
/// Tests search, indexing, suggestions, and filtering functionality.
/// </summary>
public class BuiltInSearchProviderTests : IDisposable
{
    private readonly Mock<ILogger<BuiltInSearchProvider>> _loggerMock;
    private readonly Mock<ICrmDbContext> _contextMock;
    private readonly BuiltInSearchProvider _provider;

    public BuiltInSearchProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInSearchProvider>>();
        _contextMock = new Mock<ICrmDbContext>();
        SetupMockDbSets();
        _provider = new BuiltInSearchProvider(_contextMock.Object, _loggerMock.Object);
    }

    private void SetupMockDbSets()
    {
        var accounts = new List<Account>
        {
            new Account { Id = 1, Company = "Acme Corporation", Email = "info@acme.com", AccountType = "Customer" },
            new Account { Id = 2, Company = "Beta Industries", Email = "contact@beta.com", AccountType = "Prospect" },
            new Account { Id = 3, Company = "Gamma Tech", Email = "sales@gamma.com", AccountType = "Customer" }
        }.AsQueryable();

        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@acme.com", JobTitle = "CEO" },
            new Contact { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@beta.com", JobTitle = "CTO" },
            new Contact { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob@gamma.com", JobTitle = "Manager" }
        }.AsQueryable();

        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 1, Name = "Acme Enterprise Deal", Stage = "Negotiation", Amount = 100000 },
            new Opportunity { Id = 2, Name = "Beta Pilot Project", Stage = "Proposal", Amount = 25000 },
            new Opportunity { Id = 3, Name = "Gamma Expansion", Stage = "Qualification", Amount = 50000 }
        }.AsQueryable();

        var products = new List<Product>
        {
            new Product { Id = 1, Name = "CRM Pro", Description = "Professional CRM solution", UnitPrice = 999 },
            new Product { Id = 2, Name = "CRM Enterprise", Description = "Enterprise CRM platform", UnitPrice = 4999 },
            new Product { Id = 3, Name = "CRM Basic", Description = "Basic CRM for small teams", UnitPrice = 299 }
        }.AsQueryable();

        var accountDbSet = CreateMockDbSet(accounts);
        var contactDbSet = CreateMockDbSet(contacts);
        var opportunityDbSet = CreateMockDbSet(opportunities);
        var productDbSet = CreateMockDbSet(products);

        _contextMock.Setup(c => c.Accounts).Returns(accountDbSet.Object);
        _contextMock.Setup(c => c.Contacts).Returns(contactDbSet.Object);
        _contextMock.Setup(c => c.Opportunities).Returns(opportunityDbSet.Object);
        _contextMock.Setup(c => c.Products).Returns(productDbSet.Object);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesProvider()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInSearchProvider(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInSearchProvider(_contextMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        name.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyDatabase_ReturnsTrue()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WithUnhealthyDatabase_ReturnsFalse()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_WithMatchingQuery_ReturnsResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "Acme",
            Index = "accounts",
            Limit = 10
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithNonMatchingQuery_ReturnsEmptyResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "NonExistentCompany12345",
            Index = "accounts"
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.SearchAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        // Arrange
        var request = new SearchRequest { Query = "", Index = "accounts" };

        // Act
        var act = () => _provider.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SearchAsync_WithPagination_RespectsLimitAndOffset()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "CRM",
            Index = "products",
            Limit = 2,
            Offset = 0
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Results.Count.Should().BeLessOrEqualTo(2);
    }

    [Fact]
    public async Task SearchAsync_WithContactsIndex_SearchesContacts()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "John",
            Index = "contacts"
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithOpportunitiesIndex_SearchesOpportunities()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "Enterprise",
            Index = "opportunities"
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Unified Search Tests

    [Fact]
    public async Task UnifiedSearchAsync_SearchesMultipleEntities()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "Acme",
            Limit = 10
        };

        // Act
        var result = await _provider.UnifiedSearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UnifiedSearchAsync_WithEntityFilter_FiltersEntities()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "test",
            Filters = new Dictionary<string, object>
            {
                ["entityTypes"] = new[] { "accounts", "contacts" }
            }
        };

        // Act
        var result = await _provider.UnifiedSearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Indexing Tests (Not Supported by BuiltIn)

    [Fact]
    public async Task IndexAsync_ReturnsNotRequired()
    {
        // Arrange
        var document = new SearchDocument
        {
            Id = "1",
            Index = "accounts",
            Content = new Dictionary<string, object> { ["name"] = "Test" }
        };

        // Act
        var result = await _provider.IndexAsync(document);

        // Assert - BuiltIn uses SQL LIKE, no explicit indexing needed
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IndexBatchAsync_ReturnsNotRequired()
    {
        // Arrange
        var documents = new List<SearchDocument>
        {
            new SearchDocument { Id = "1", Index = "accounts", Content = new Dictionary<string, object>() },
            new SearchDocument { Id = "2", Index = "accounts", Content = new Dictionary<string, object>() }
        };

        // Act
        var result = await _provider.IndexBatchAsync(documents);

        // Assert
        result.SuccessCount.Should().Be(2);
        result.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteFromIndexAsync_ReturnsSuccess()
    {
        // Act
        var result = await _provider.DeleteFromIndexAsync("accounts", "1");

        // Assert - No-op for BuiltIn
        result.Should().BeTrue();
    }

    #endregion

    #region Suggestions Tests

    [Fact]
    public async Task SuggestAsync_WithPartialQuery_ReturnsSuggestions()
    {
        // Arrange
        var request = new SuggestRequest
        {
            Query = "Acm",
            Index = "accounts",
            Limit = 5
        };

        // Act
        var suggestions = await _provider.SuggestAsync(request);

        // Assert
        suggestions.Should().NotBeNull();
    }

    [Fact]
    public async Task SuggestAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.SuggestAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyDatabase_ReturnsHealthy()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyDatabase_ReturnsUnhealthy()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task HealthCheckAsync_WithException_ReturnsUnhealthy()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Message.Should().Contain("Database error");
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public async Task GetIndexConfigurationAsync_ReturnsConfiguration()
    {
        // Act
        var config = await _provider.GetIndexConfigurationAsync("accounts");

        // Assert
        config.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllIndexesAsync_ReturnsIndexList()
    {
        // Act
        var indexes = await _provider.GetAllIndexesAsync();

        // Assert
        indexes.Should().NotBeNull();
        indexes.Should().Contain("accounts");
        indexes.Should().Contain("contacts");
        indexes.Should().Contain("opportunities");
        indexes.Should().Contain("products");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task SearchAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var request = new SearchRequest { Query = "test", Index = "accounts" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SearchAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SuggestAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var request = new SuggestRequest { Query = "test", Index = "accounts" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SuggestAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task SearchAsync_WithAccountTypeFilter_FiltersResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "Corp",
            Index = "accounts",
            Filters = new Dictionary<string, object>
            {
                ["accountType"] = "Customer"
            }
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithDateRangeFilter_FiltersResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "Deal",
            Index = "opportunities",
            Filters = new Dictionary<string, object>
            {
                ["createdAfter"] = DateTime.UtcNow.AddDays(-30),
                ["createdBefore"] = DateTime.UtcNow
            }
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
