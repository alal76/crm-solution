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
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DuplicateDetectionService
/// Covers: Duplicate detection, rules, merge, deduplication
/// </summary>
public class DuplicateDetectionServiceTests
{
    private readonly Mock<IRepository<Account>> _mockAccountRepository;
    private readonly Mock<IRepository<Contact>> _mockContactRepository;
    private readonly Mock<IRepository<Lead>> _mockLeadRepository;
    private readonly Mock<IRepository<DuplicateRule>> _mockRuleRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<DuplicateDetectionService>> _mockLogger;
    private readonly DuplicateDetectionService _service;

    public DuplicateDetectionServiceTests()
    {
        _mockAccountRepository = new Mock<IRepository<Account>>();
        _mockContactRepository = new Mock<IRepository<Contact>>();
        _mockLeadRepository = new Mock<IRepository<Lead>>();
        _mockRuleRepository = new Mock<IRepository<DuplicateRule>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DuplicateDetectionService>>();

        _service = new DuplicateDetectionService(
            _mockAccountRepository.Object,
            _mockContactRepository.Object,
            _mockLeadRepository.Object,
            _mockRuleRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Find Duplicates - Account Tests

    [Fact]
    public async Task FindDuplicateAccountsAsync_MatchingEmail_ReturnsDuplicates()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "test@acme.com", Company = "Acme Inc" },
            new Account { Id = 2, Email = "test@acme.com", Company = "ACME Corporation" }
        };

        _mockAccountRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.FindDuplicateAccountsAsync("test@acme.com");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindDuplicateAccountsAsync_MatchingCompanyName_ReturnsDuplicates()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Company = "Acme Inc" },
            new Account { Id = 2, Company = "Acme Incorporated" }
        };

        _mockAccountRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.FindDuplicateAccountsByNameAsync("Acme");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FindDuplicateAccountsAsync_NoDuplicates_ReturnsEmptyList()
    {
        // Arrange
        _mockAccountRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(new List<Account>());

        // Act
        var result = await _service.FindDuplicateAccountsAsync("unique@email.com");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindDuplicateAccountsAsync_MatchingPhone_ReturnsDuplicates()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Phone = "555-1234" },
            new Account { Id = 2, Phone = "5551234" }
        };

        _mockAccountRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.FindDuplicateAccountsByPhoneAsync("555-1234");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Find Duplicates - Contact Tests

    [Fact]
    public async Task FindDuplicateContactsAsync_MatchingEmail_ReturnsDuplicates()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Email = "john@acme.com", FirstName = "John", LastName = "Doe" },
            new Contact { Id = 2, Email = "john@acme.com", FirstName = "Johnny", LastName = "Doe" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.FindDuplicateContactsAsync("john@acme.com");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindDuplicateContactsAsync_MatchingName_ReturnsDuplicates()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe" },
            new Contact { Id = 2, FirstName = "John", LastName = "Doe" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.FindDuplicateContactsByNameAsync("John", "Doe");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindDuplicateContactsAsync_MatchingPhone_ReturnsDuplicates()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Phone = "555-1234" },
            new Contact { Id = 2, Phone = "(555) 123-4" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.FindDuplicateContactsByPhoneAsync("555-1234");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Find Duplicates - Lead Tests

    [Fact]
    public async Task FindDuplicateLeadsAsync_MatchingEmail_ReturnsDuplicates()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new Lead { Id = 1, Email = "lead@test.com" },
            new Lead { Id = 2, Email = "lead@test.com" }
        };

        _mockLeadRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Lead, bool>>>()))
            .ReturnsAsync(leads);

        // Act
        var result = await _service.FindDuplicateLeadsAsync("lead@test.com");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindDuplicateLeadsAsync_MatchingCompanyAndName_ReturnsDuplicates()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new Lead { Id = 1, Company = "Acme", FirstName = "John", LastName = "Doe" },
            new Lead { Id = 2, Company = "ACME Inc", FirstName = "John", LastName = "Doe" }
        };

        _mockLeadRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Lead, bool>>>()))
            .ReturnsAsync(leads);

        // Act
        var result = await _service.FindDuplicateLeadsByCompanyAsync("Acme", "John", "Doe");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Duplicate Rules Tests

    [Fact]
    public async Task GetRulesAsync_ReturnsAllRules()
    {
        // Arrange
        var rules = new List<DuplicateRule>
        {
            new DuplicateRule { Id = 1, Name = "Email Match", EntityType = "Contact", IsActive = true },
            new DuplicateRule { Id = 2, Name = "Name Match", EntityType = "Contact", IsActive = true }
        };

        _mockRuleRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(rules);

        // Act
        var result = await _service.GetRulesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRulesByEntityTypeAsync_ReturnsMatchingRules()
    {
        // Arrange
        var rules = new List<DuplicateRule>
        {
            new DuplicateRule { Id = 1, EntityType = "Contact", IsActive = true }
        };

        _mockRuleRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DuplicateRule, bool>>>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _service.GetRulesByEntityTypeAsync("Contact");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateRuleAsync_ValidRule_ReturnsCreatedRule()
    {
        // Arrange
        var createDto = new CreateDuplicateRuleDto
        {
            Name = "New Rule",
            EntityType = "Account",
            MatchFields = new List<string> { "Email", "Phone" }
        };

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<DuplicateRule>()))
            .ReturnsAsync((DuplicateRule r) => { r.Id = 1; return r; });

        // Act
        var result = await _service.CreateRuleAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task UpdateRuleAsync_ValidRule_ReturnsUpdatedRule()
    {
        // Arrange
        var existingRule = new DuplicateRule { Id = 1, Name = "Old Rule" };
        var updateDto = new UpdateDuplicateRuleDto { Id = 1, Name = "Updated Rule" };

        _mockRuleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingRule);

        _mockRuleRepository.Setup(r => r.UpdateAsync(It.IsAny<DuplicateRule>()))
            .ReturnsAsync((DuplicateRule r) => r);

        // Act
        var result = await _service.UpdateRuleAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Rule");
    }

    [Fact]
    public async Task DeleteRuleAsync_ExistingRule_ReturnsTrue()
    {
        // Arrange
        _mockRuleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new DuplicateRule { Id = 1 });

        _mockRuleRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteRuleAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateRuleAsync_InactiveRule_ActivatesRule()
    {
        // Arrange
        var rule = new DuplicateRule { Id = 1, IsActive = false };

        _mockRuleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(rule);

        _mockRuleRepository.Setup(r => r.UpdateAsync(It.IsAny<DuplicateRule>()))
            .ReturnsAsync((DuplicateRule r) => { r.IsActive = true; return r; });

        // Act
        var result = await _service.ActivateRuleAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Check for Duplicates Tests

    [Fact]
    public async Task CheckForDuplicatesAsync_NewAccount_ReturnsDuplicateCheck()
    {
        // Arrange
        var accountDto = new CreateAccountDto
        {
            Company = "Acme Inc",
            Email = "info@acme.com"
        };

        var existingAccounts = new List<Account>
        {
            new Account { Id = 1, Company = "ACME", Email = "info@acme.com" }
        };

        _mockAccountRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Account, bool>>>()))
            .ReturnsAsync(existingAccounts);

        // Act
        var result = await _service.CheckForDuplicatesAsync("Account", accountDto);

        // Assert
        result.Should().NotBeNull();
        result.HasDuplicates.Should().BeTrue();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_NewContact_NoDuplicates_ReturnsClean()
    {
        // Arrange
        var contactDto = new CreateContactDto
        {
            FirstName = "Unique",
            LastName = "Person",
            Email = "unique@test.com"
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(new List<Contact>());

        // Act
        var result = await _service.CheckForDuplicatesAsync("Contact", contactDto);

        // Assert
        result.HasDuplicates.Should().BeFalse();
    }

    #endregion

    #region Merge Tests

    [Fact]
    public async Task MergeAccountsAsync_ValidIds_ReturnsMergedAccount()
    {
        // Arrange
        var master = new Account { Id = 1, Company = "Acme Inc", Email = "master@acme.com" };
        var duplicate = new Account { Id = 2, Company = "ACME", Phone = "555-1234" };

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockAccountRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        _mockAccountRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        _mockAccountRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        // Act
        var result = await _service.MergeAccountsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task MergeContactsAsync_ValidIds_ReturnsMergedContact()
    {
        // Arrange
        var master = new Contact { Id = 1, FirstName = "John", LastName = "Doe" };
        var duplicate = new Contact { Id = 2, FirstName = "Johnny", LastName = "Doe", Phone = "555-1234" };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockContactRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        _mockContactRepository.Setup(r => r.UpdateAsync(It.IsAny<Contact>()))
            .ReturnsAsync((Contact c) => c);

        _mockContactRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        // Act
        var result = await _service.MergeContactsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MergeAccountsAsync_SameId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.MergeAccountsAsync(1, 1));
    }

    [Fact]
    public async Task MergeAccountsAsync_NonExistingMaster_ThrowsException()
    {
        // Arrange
        _mockAccountRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.MergeAccountsAsync(999, 2));
    }

    #endregion

    #region Match Score Tests

    [Fact]
    public void CalculateMatchScore_ExactEmailMatch_ReturnsHighScore()
    {
        // Arrange
        var contact1 = new Contact { Email = "john@test.com" };
        var contact2 = new Contact { Email = "john@test.com" };

        // Act
        var score = _service.CalculateMatchScore(contact1, contact2);

        // Assert
        score.Should().BeGreaterThan(80);
    }

    [Fact]
    public void CalculateMatchScore_SimilarName_ReturnsMediumScore()
    {
        // Arrange
        var contact1 = new Contact { FirstName = "John", LastName = "Doe" };
        var contact2 = new Contact { FirstName = "Jon", LastName = "Doe" };

        // Act
        var score = _service.CalculateMatchScore(contact1, contact2);

        // Assert
        score.Should().BeGreaterThan(50);
    }

    [Fact]
    public void CalculateMatchScore_NoMatch_ReturnsLowScore()
    {
        // Arrange
        var contact1 = new Contact { FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        var contact2 = new Contact { FirstName = "Jane", LastName = "Smith", Email = "jane@other.com" };

        // Act
        var score = _service.CalculateMatchScore(contact1, contact2);

        // Assert
        score.Should().BeLessThan(30);
    }

    [Theory]
    [InlineData("john@test.com", "john@test.com", 100)]
    [InlineData("john@test.com", "JOHN@TEST.COM", 100)]
    [InlineData("john@test.com", "jane@test.com", 50)]
    public void CalculateEmailMatchScore_VariousEmails_ReturnsExpectedScore(
        string email1, string email2, int minExpectedScore)
    {
        // Act
        var score = _service.CalculateEmailMatchScore(email1, email2);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(minExpectedScore - 10);
    }

    #endregion

    #region Batch Duplicate Detection Tests

    [Fact]
    public async Task FindAllDuplicatesAsync_EntityType_ReturnsGroupedDuplicates()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Email = "john@test.com", FirstName = "John" },
            new Contact { Id = 2, Email = "john@test.com", FirstName = "Johnny" },
            new Contact { Id = 3, Email = "jane@test.com", FirstName = "Jane" },
            new Contact { Id = 4, Email = "jane@test.com", FirstName = "Janet" }
        };

        _mockContactRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.FindAllDuplicatesAsync("Contact");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDuplicateStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Email = "john@test.com" },
            new Contact { Id = 2, Email = "john@test.com" }
        };

        _mockContactRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.GetDuplicateStatisticsAsync("Contact");

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(2);
    }

    #endregion
}
