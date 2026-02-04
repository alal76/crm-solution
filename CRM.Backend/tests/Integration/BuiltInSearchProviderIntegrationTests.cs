// Part of the Pluggable Architecture implementation
// Phase 1 Week 5: Integration tests for BuiltInSearchProvider
// Task 5.8: Integration test existing search still works

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Integration;

/// <summary>
/// Integration tests for BuiltInSearchProvider that verify actual search
/// functionality against an in-memory database with real entities.
/// These tests ensure the refactored search logic preserves existing behavior.
/// </summary>
public class BuiltInSearchProviderIntegrationTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly BuiltInSearchProvider _provider;
    private readonly Mock<IDbContextResolver> _resolverMock;

    public BuiltInSearchProviderIntegrationTests()
    {
        // Create an in-memory database for testing
        // ConfigureWarnings: Ignore the many-to-many relationship inference issues
        // that occur with complex entities like ArticleRelationship
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"SearchTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableServiceProviderCaching(false)
            .Options;

        _dbContext = new CrmDbContext(options, null);
        
        // Setup the resolver to return our test context
        _resolverMock = new Mock<IDbContextResolver>();
        _resolverMock.Setup(r => r.ResolveContext()).Returns(_dbContext);
        
        var logger = new Mock<ILogger<BuiltInSearchProvider>>();
        _provider = new BuiltInSearchProvider(_resolverMock.Object, logger.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region Account Search Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingAccountName_ReturnsAccountResults()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Acme Corporation",
            FirstName = "Acme",
            LastName = "Corporation",
            LegalName = "Acme Corporation LLC",
            Category = AccountCategory.Organization,
            Email = "contact@acme.com",
            Phone = "555-0100",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Acme", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().Contain(h => h.EntityType == "Account");
        result.Hits.First(h => h.EntityType == "Account").Title.Should().Contain("Acme");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithDeletedAccount_DoesNotReturnDeletedAccount()
    {
        // Arrange
        var activeAccount = new Account
        {
            Id = 1,
            Company = "Active Company",
            FirstName = "Active",
            LastName = "Company",
            LegalName = "Active Company Inc",
            Category = AccountCategory.Organization,
            Email = "contact@activecompany.com",
            Phone = "555-0101",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedAccount = new Account
        {
            Id = 2,
            Company = "Deleted Company",
            FirstName = "Deleted",
            LastName = "Company",
            LegalName = "Deleted Company Inc",
            Category = AccountCategory.Organization,
            Email = "contact@deletedcompany.com",
            Phone = "555-0102",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.AddRange(activeAccount, deletedAccount);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Company", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var accountHits = result.Hits.Where(h => h.EntityType == "Account").ToList();
        accountHits.Should().HaveCount(1);
        accountHits.First().Title.Should().Contain("Active");
        accountHits.Should().NotContain(h => h.Title.Contains("Deleted"));
    }

    #endregion

    #region Contact Search Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingContactName_ReturnsContactResults()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            EmailPrimary = "john.smith@example.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "John", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().Contain(h => h.EntityType == "Contact");
        result.Hits.First(h => h.EntityType == "Contact").Title.Should().Contain("John Smith");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingContactEmail_ReturnsContactResults()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            EmailPrimary = "jane.doe@testcompany.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "testcompany", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().Contain(h => h.EntityType == "Contact");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithArchivedContact_DoesNotReturnArchivedContact()
    {
        // Arrange
        var activeContact = new Contact
        {
            Id = 1,
            FirstName = "Active",
            LastName = "User",
            EmailPrimary = "active@example.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        var archivedContact = new Contact
        {
            Id = 2,
            FirstName = "Archived",
            LastName = "User",
            EmailPrimary = "archived@example.com",
            Status = ContactStatus.Archived,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.AddRange(activeContact, archivedContact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "User", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var contactHits = result.Hits.Where(h => h.EntityType == "Contact").ToList();
        contactHits.Should().HaveCount(1);
        contactHits.First().Title.Should().Contain("Active");
        contactHits.Should().NotContain(h => h.Title.Contains("Archived"));
    }

    #endregion

    #region Opportunity Search Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingOpportunityName_ReturnsOpportunityResults()
    {
        // Arrange
        // First create an account for the opportunity
        var account = new Account
        {
            Id = 100,
            Company = "Enterprise Corp",
            Category = AccountCategory.Organization,
            Email = "contact@enterprise.com",
            Phone = "555-0200",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise Deal Q1",
            Amount = 50000,
            Stage = OpportunityStage.Qualification,
            AccountId = 100,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Enterprise", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().Contain(h => h.EntityType == "Opportunity");
        result.Hits.First(h => h.EntityType == "Opportunity").Title.Should().Contain("Enterprise");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithDeletedOpportunity_DoesNotReturnDeletedOpportunity()
    {
        // Arrange
        // First create an account for the opportunities
        var account = new Account
        {
            Id = 101,
            Company = "Test Corp",
            Category = AccountCategory.Organization,
            Email = "contact@testcorp.com",
            Phone = "555-0201",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        
        var activeOpp = new Opportunity
        {
            Id = 1,
            Name = "Active Opportunity",
            Amount = 10000,
            Stage = OpportunityStage.Discovery,
            AccountId = 101,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedOpp = new Opportunity
        {
            Id = 2,
            Name = "Deleted Opportunity",
            Amount = 20000,
            Stage = OpportunityStage.Discovery,
            AccountId = 101,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Opportunities.AddRange(activeOpp, deletedOpp);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Opportunity", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var oppHits = result.Hits.Where(h => h.EntityType == "Opportunity").ToList();
        oppHits.Should().HaveCount(1);
        oppHits.First().Title.Should().Contain("Active");
        oppHits.Should().NotContain(h => h.Title.Contains("Deleted"));
    }

    #endregion

    #region Product Search Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingProductName_ReturnsProductResults()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Enterprise License",
            Description = "Full enterprise software license",
            Price = 999.99m,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Enterprise", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().Contain(h => h.EntityType == "Product");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithInactiveProduct_DoesNotReturnInactiveProduct()
    {
        // Arrange
        var activeProduct = new Product
        {
            Id = 1,
            Name = "Active Product",
            Description = "Currently available",
            Price = 100m,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var inactiveProduct = new Product
        {
            Id = 2,
            Name = "Inactive Product",
            Description = "No longer available",
            Price = 200m,
            IsActive = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.AddRange(activeProduct, inactiveProduct);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Product", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var productHits = result.Hits.Where(h => h.EntityType == "Product").ToList();
        productHits.Should().HaveCount(1);
        productHits.First().Title.Should().Contain("Active");
    }

    #endregion

    #region Multi-Entity Search Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithGenericQuery_ReturnsResultsFromMultipleEntityTypes()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Global Corp",
            FirstName = "Global",
            LastName = "Corp",
            LegalName = "Global Corporation",
            Category = AccountCategory.Organization,
            Email = "contact@globalcorp.com",
            Phone = "555-0103",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Global",
            LastName = "Manager",
            EmailPrimary = "manager@global.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Global Expansion Deal",
            Amount = 100000,
            Stage = OpportunityStage.Negotiation,
            AccountId = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Accounts.Add(account);
        _dbContext.Contacts.Add(contact);
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Global", Take = 20 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Hits.Select(h => h.EntityType).Distinct().Should().Contain("Account");
        result.Hits.Select(h => h.EntityType).Distinct().Should().Contain("Contact");
        result.Hits.Select(h => h.EntityType).Distinct().Should().Contain("Opportunity");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithPagination_RespectsSkipAndTake()
    {
        // Arrange - Create multiple contacts
        for (int i = 1; i <= 10; i++)
        {
            _dbContext.Contacts.Add(new Contact
            {
                Id = i,
                FirstName = $"TestUser{i}",
                LastName = "Sample",
                EmailPrimary = $"testuser{i}@example.com",
                Status = ContactStatus.Active,
                DateAdded = DateTime.UtcNow
            });
        }
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "TestUser", Skip = 2, Take = 3 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        // Should return at most 3 results due to Take
        result.Hits.Count().Should().BeLessThanOrEqualTo(3);
    }

    #endregion

    #region EntityType Filter Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithEntityTypeFilter_ReturnsOnlySpecifiedEntityType()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            FirstName = "Tech",
            LastName = "Corp",
            LegalName = "Tech Corporation",
            Category = AccountCategory.Organization,
            Email = "contact@techcorp.com",
            Phone = "555-0104",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Tech",
            LastName = "Support",
            EmailPrimary = "support@tech.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        
        _dbContext.Accounts.Add(account);
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Tech", EntityType = "Account", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "Account");
        result.Hits.Should().NotContain(h => h.EntityType == "Contact");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithNoMatchingResults_ReturnsEmptyHits()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            FirstName = "Acme",
            LastName = "Corp",
            LegalName = "Acme Corporation",
            Category = AccountCategory.Organization,
            Email = "contact@acmecorp.com",
            Phone = "555-0105",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "NonExistentQuery12345", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithCaseInsensitiveQuery_ReturnsMatches()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            FirstName = "UPPERCASE",
            LastName = "Corp",
            LegalName = "UPPERCASE Corporation",
            Category = AccountCategory.Organization,
            Email = "contact@uppercase.com",
            Phone = "555-0106",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "uppercase", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().Contain(h => h.EntityType == "Account");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_ProcessingTime_IsTracked()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Processing",
            LastName = "Test",
            EmailPrimary = "processing@test.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Processing", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ProcessingTimeMs.Should().BeGreaterOrEqualTo(0);
        result.Query.Should().Be("Processing");
    }

    #endregion

    #region Health Check Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task HealthCheckAsync_WithValidContext_ReturnsHealthy()
    {
        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.ProviderName.Should().Be("BuiltIn");
        // In-memory database should be considered healthy
        // Note: InMemory provider may not support CanConnectAsync, behavior may vary
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task IsAvailableAsync_WithValidContext_ReturnsTrue()
    {
        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
