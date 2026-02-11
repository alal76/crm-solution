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

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Account Repository
/// Covers: Account-specific queries, relationships, statistics
/// </summary>
public class AccountRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<Account>> _mockDbSet;
    private readonly Mock<ILogger<AccountRepository>> _mockLogger;
    private readonly AccountRepository _repository;

    public AccountRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<Account>>();
        _mockLogger = new Mock<ILogger<AccountRepository>>();

        _mockContext.Setup(c => c.Set<Account>()).Returns(_mockDbSet.Object);
        _repository = new AccountRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByEmail Tests

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsAccount()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "test@example.com", Company = "Test Co" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        // Arrange
        var accounts = new List<Account>().AsQueryable();
        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetByEmailAsync("notfound@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsAccount()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "Test@Example.com", Company = "Test Co" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetByOwner Tests

    [Fact]
    public async Task GetByOwnerAsync_HasAccounts_ReturnsOwnerAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, OwnerId = 1, Company = "Account 1" },
            new Account { Id = 2, OwnerId = 1, Company = "Account 2" },
            new Account { Id = 3, OwnerId = 2, Company = "Account 3" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByOwnerAsync_NoAccounts_ReturnsEmpty()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, OwnerId = 2, Company = "Account 1" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIndustry Tests

    [Fact]
    public async Task GetByIndustryAsync_HasMatches_ReturnsAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Industry = "Technology", Company = "Tech Co" },
            new Account { Id = 2, Industry = "Technology", Company = "Tech Inc" },
            new Account { Id = 3, Industry = "Healthcare", Company = "Health Co" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetByIndustryAsync("Technology");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetWithContacts Tests

    [Fact]
    public async Task GetWithContactsAsync_HasContacts_ReturnsAccountWithContacts()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Test Co",
            AccountContacts = new List<AccountContact>
            {
                new AccountContact { ContactId = 1 },
                new AccountContact { ContactId = 2 }
            }
        };

        var accounts = new List<Account> { account }.AsQueryable();
        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetWithContactsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.AccountContacts.Should().HaveCount(2);
    }

    #endregion

    #region GetWithOpportunities Tests

    [Fact]
    public async Task GetWithOpportunitiesAsync_HasOpportunities_ReturnsAccountWithOpportunities()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Test Co",
            Opportunities = new List<Opportunity>
            {
                new Opportunity { Id = 1, Name = "Opp 1" },
                new Opportunity { Id = 2, Name = "Opp 2" }
            }
        };

        var accounts = new List<Account> { account }.AsQueryable();
        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetWithOpportunitiesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Opportunities.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByCompanyName_ReturnsMatches()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Company = "Acme Corporation" },
            new Account { Id = 2, Company = "Acme Industries" },
            new Account { Id = 3, Company = "Beta Corp" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.SearchAsync("Acme");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ByEmail_ReturnsMatches()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "contact@acme.com", Company = "Acme" },
            new Account { Id = 2, Email = "info@beta.com", Company = "Beta" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.SearchAsync("acme");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAll()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Company = "Acme" },
            new Account { Id = 2, Company = "Beta" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.SearchAsync("");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByIndustryAsync_ReturnsIndustryCounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Industry = "Technology" },
            new Account { Id = 2, Industry = "Technology" },
            new Account { Id = 3, Industry = "Healthcare" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetCountByIndustryAsync();

        // Assert
        result.Should().ContainKey("Technology");
        result["Technology"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Status = "Active" },
            new Account { Id = 2, Status = "Active" },
            new Account { Id = 3, Status = "Inactive" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result.Should().ContainKey("Active");
        result["Active"].Should().Be(2);
    }

    [Fact]
    public async Task GetTotalRevenueAsync_CalculatesTotal()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, AnnualRevenue = 100000 },
            new Account { Id = 2, AnnualRevenue = 200000 },
            new Account { Id = 3, AnnualRevenue = 150000 }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetTotalRevenueAsync();

        // Assert
        result.Should().Be(450000);
    }

    #endregion

    #region Hierarchy Tests

    [Fact]
    public async Task GetChildAccountsAsync_HasChildren_ReturnsChildren()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, ParentAccountId = null, Company = "Parent" },
            new Account { Id = 2, ParentAccountId = 1, Company = "Child 1" },
            new Account { Id = 3, ParentAccountId = 1, Company = "Child 2" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetChildAccountsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetParentAccountAsync_HasParent_ReturnsParent()
    {
        // Arrange
        var parent = new Account { Id = 1, Company = "Parent" };
        var child = new Account { Id = 2, ParentAccountId = 1, Company = "Child" };

        var accounts = new List<Account> { parent, child }.AsQueryable();
        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetParentAccountAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.Company.Should().Be("Parent");
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecentAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Account { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Account { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-30) }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyUpdatedAsync_ReturnsUpdatedAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, UpdatedAt = DateTime.UtcNow.AddHours(-1) },
            new Account { Id = 2, UpdatedAt = DateTime.UtcNow.AddDays(-2) },
            new Account { Id = 3, UpdatedAt = DateTime.UtcNow.AddDays(-10) }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.GetRecentlyUpdatedAsync(3);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public async Task FindDuplicatesAsync_HasDuplicates_ReturnsDuplicates()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "same@example.com", Company = "Acme" },
            new Account { Id = 2, Email = "same@example.com", Company = "Acme Corp" }
        }.AsQueryable();

        SetupMockDbSet(accounts);

        // Act
        var result = await _repository.FindDuplicatesAsync("same@example.com", "Acme");

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<Account> data)
    {
        _mockDbSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting classes
public class Account
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string Company { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string Status { get; set; } = "Active";
    public int? OwnerId { get; set; }
    public int? ParentAccountId { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public List<AccountContact> AccountContacts { get; set; } = new();
    public List<Opportunity> Opportunities { get; set; } = new();
}

public class AccountContact
{
    public int AccountId { get; set; }
    public int ContactId { get; set; }
}

public class Opportunity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
