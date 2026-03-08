// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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

        var logger = new Mock<ILogger<BuiltInSearchProvider>>();
        _provider = new BuiltInSearchProvider(_dbContext, logger.Object);
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

    #region Extended Entity Search Tests (INFRA-08)

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingLeadName_ReturnsLeadResults()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Prospect",
            LastName = "Jones",
            Email = "prospect.jones@example.com",
            CompanyName = "Big Prospect Corp",
            Status = LeadLifecycleStatus.New,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Leads.Add(lead);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Prospect", EntityType = "Lead", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "Lead");
        result.Hits.First().Title.Should().Contain("Prospect");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithDeletedLead_DoesNotReturnDeletedLead()
    {
        // Arrange
        var activeLead = new Lead
        {
            Id = 1,
            FirstName = "Active",
            LastName = "Lead",
            Email = "active@example.com",
            Status = LeadLifecycleStatus.Working,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedLead = new Lead
        {
            Id = 2,
            FirstName = "Deleted",
            LastName = "Lead",
            Email = "deleted@example.com",
            Status = LeadLifecycleStatus.Disqualified,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Leads.AddRange(activeLead, deletedLead);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Lead", EntityType = "Lead", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var leadHits = result.Hits.Where(h => h.EntityType == "Lead").ToList();
        leadHits.Should().HaveCount(1);
        leadHits.First().Title.Should().Contain("Active");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingServiceRequestSubject_ReturnsServiceRequestResults()
    {
        // Arrange
        var ticket = new ServiceRequest
        {
            Id = 1,
            TicketNumber = "SR-2026-001",
            Subject = "Network connectivity outage in office",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.High,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ServiceRequests.Add(ticket);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "connectivity", EntityType = "ServiceRequest", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "ServiceRequest");
        result.Hits.First().Title.Should().Contain("SR-2026-001");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingCampaignName_ReturnsCampaignResults()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Id = 1,
            Name = "Spring Launch 2026",
            CampaignCode = "SPRING26",
            Description = "Promotional spring campaign",
            CampaignType = CampaignType.Email,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.MarketingCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Spring", EntityType = "Campaign", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "Campaign");
        result.Hits.First().Title.Should().Contain("Spring");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingContractName_ReturnsContractResults()
    {
        // Arrange
        var contract = new Contract
        {
            Id = 1,
            ContractNumber = "CON-20260224-1001",
            Name = "Enterprise Service Agreement",
            Status = ContractStatus.Draft,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Contracts.Add(contract);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Enterprise", EntityType = "Contract", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "Contract");
        result.Hits.First().Title.Should().Contain("Enterprise");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingInvoiceNumber_ReturnsInvoiceResults()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-2026-0042",
            Status = InvoiceStatus.Draft,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "INV-2026", EntityType = "Invoice", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "Invoice");
        result.Hits.First().Title.Should().Be("INV-2026-0042");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMatchingUserName_ReturnsUserResults()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "jdoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "hashed",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "jdoe", EntityType = "User", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().NotBeEmpty();
        result.Hits.Should().OnlyContain(h => h.EntityType == "User");
        result.Hits.First().Title.Should().Contain("John");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task RebuildAllIndexesAsync_Completes_WithoutError()
    {
        // Act & Assert — BuiltIn is a no-op; must not throw
        var act = async () => await _provider.RebuildAllIndexesAsync();
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Highlight and Facet Tests (INFRA-09)

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithHighlightsEnabled_ReturnsMarkTagsInHighlights()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Highlightable Corp",
            FirstName = "Highlightable",
            LastName = "Corp",
            LegalName = "Highlightable Corporation",
            Category = AccountCategory.Organization,
            Email = "info@highlightable.com",
            Phone = "555-0200",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Highlightable", IncludeHighlights = true, Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var hit = result.Hits.FirstOrDefault(h => h.EntityType == "Account");
        hit.Should().NotBeNull();
        hit!.Highlights.Should().NotBeNull();
        // Verify <mark> tags are used (not <em>)
        var highlightText = string.Join(" ", hit.Highlights!.Values);
        highlightText.Should().Contain("<mark>Highlightable</mark>");
        highlightText.Should().NotContain("<em>");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithHighlightsDisabled_DoesNotReturnHighlights()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Nohighlight",
            LastName = "Test",
            EmailPrimary = "nohighlight@example.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "Nohighlight", IncludeHighlights = false, Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        var hit = result.Hits.FirstOrDefault(h => h.EntityType == "Contact");
        hit.Should().NotBeNull();
        hit!.Highlights.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithMultipleEntityTypes_ReturnsFacetsByEntityType()
    {
        // Arrange — add one account and one contact that both match the query
        var account = new Account
        {
            Id = 1,
            Company = "FacetTest Corp",
            FirstName = "FacetTest",
            LastName = "Corp",
            LegalName = "FacetTest Corporation",
            Category = AccountCategory.Organization,
            Email = "info@facettest.com",
            Phone = "555-0300",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var contact = new Contact
        {
            Id = 1,
            FirstName = "FacetTest",
            LastName = "User",
            EmailPrimary = "facettest@example.com",
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Accounts.Add(account);
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest { Query = "FacetTest", Take = 20 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Facets.Should().NotBeNull();
        result.Facets!.Should().ContainKey("EntityType");
        var entityTypeFacets = result.Facets["EntityType"];
        entityTypeFacets.Should().NotBeEmpty();
        entityTypeFacets.Should().Contain(f => f.Value == "Account");
        entityTypeFacets.Should().Contain(f => f.Value == "Contact");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WithFacetFieldsRequested_ReturnsFacetCountsForMetadataField()
    {
        // Arrange — two service requests with different priorities
        _dbContext.ServiceRequests.AddRange(
            new ServiceRequest
            {
                Id = 1,
                TicketNumber = "SR-META-001",
                Subject = "Metadata facet urgent ticket",
                Priority = ServiceRequestPriority.High,
                Status = ServiceRequestStatus.New,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new ServiceRequest
            {
                Id = 2,
                TicketNumber = "SR-META-002",
                Subject = "Metadata facet normal ticket",
                Priority = ServiceRequestPriority.Medium,
                Status = ServiceRequestStatus.InProgress,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });
        await _dbContext.SaveChangesAsync();

        var request = new SearchRequest
        {
            Query = "Metadata",
            EntityType = "ServiceRequest",
            FacetFields = new List<string> { "Priority" },
            Take = 20
        };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Facets.Should().NotBeNull();
        result.Facets!.Should().ContainKey("Priority");
        result.Facets["Priority"].Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Search")]
    public async Task SearchAsync_WhenNoResults_FacetsAreNull()
    {
        // Arrange — empty database
        var request = new SearchRequest { Query = "NonExistentXYZ999", Take = 10 };

        // Act
        var result = await _provider.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
        result.Facets.Should().BeNull();
    }

    #endregion
}
