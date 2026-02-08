// CRM Solution - Customer Relationship Management System
// Contact Repository Unit Tests

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

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Contact Repository
/// Covers: Contact-specific queries, account relationships, search
/// </summary>
public class ContactRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<Contact>> _mockDbSet;
    private readonly Mock<ILogger<ContactRepository>> _mockLogger;
    private readonly ContactRepository _repository;

    public ContactRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<Contact>>();
        _mockLogger = new Mock<ILogger<ContactRepository>>();

        _mockContext.Setup(c => c.Set<Contact>()).Returns(_mockDbSet.Object);
        _repository = new ContactRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByEmail Tests

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsContact()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Email = "john@example.com", FirstName = "John" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetByEmailAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var contacts = new List<Contact>().AsQueryable();
        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetByEmailAsync("notfound@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailsAsync_MultipleEmails_ReturnsMatches()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Email = "john@example.com" },
            new Contact { Id = 2, Email = "jane@example.com" },
            new Contact { Id = 3, Email = "bob@example.com" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        var emails = new[] { "john@example.com", "jane@example.com" };

        // Act
        var result = await _repository.GetByEmailsAsync(emails);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAccount Tests

    [Fact]
    public async Task GetByAccountAsync_HasContacts_ReturnsAccountContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, AccountId = 1, FirstName = "John" },
            new Contact { Id = 2, AccountId = 1, FirstName = "Jane" },
            new Contact { Id = 3, AccountId = 2, FirstName = "Bob" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByAccountAsync_NoContacts_ReturnsEmpty()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, AccountId = 2, FirstName = "John" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPrimaryContactAsync_HasPrimary_ReturnsPrimary()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, AccountId = 1, IsPrimary = false },
            new Contact { Id = 2, AccountId = 1, IsPrimary = true }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetPrimaryContactAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe" },
            new Contact { Id = 2, FirstName = "Jane", LastName = "Doe" },
            new Contact { Id = 3, FirstName = "Bob", LastName = "Smith" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.SearchAsync("Doe");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ByPhone_ReturnsMatches()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", Phone = "555-1234" },
            new Contact { Id = 2, FirstName = "Jane", Phone = "555-5678" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.SearchAsync("555-1234");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAll()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John" },
            new Contact { Id = 2, FirstName = "Jane" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.SearchAsync("");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetByTitleAsync_HasMatches_ReturnsContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Title = "CEO" },
            new Contact { Id = 2, Title = "CEO" },
            new Contact { Id = 3, Title = "CTO" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetByTitleAsync("CEO");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDepartmentAsync_HasMatches_ReturnsContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Department = "Sales" },
            new Contact { Id = 2, Department = "Sales" },
            new Contact { Id = 3, Department = "Marketing" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetByDepartmentAsync("Sales");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveContactsAsync_ReturnsActiveOnly()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, IsActive = true },
            new Contact { Id = 2, IsActive = true },
            new Contact { Id = 3, IsActive = false }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetActiveContactsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByAccountAsync_ReturnsCounts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, AccountId = 1 },
            new Contact { Id = 2, AccountId = 1 },
            new Contact { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetCountByAccountAsync();

        // Assert
        result.Should().ContainKey(1);
        result[1].Should().Be(2);
    }

    [Fact]
    public async Task GetContactsAddedInPeriodAsync_ReturnsCount()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Contact { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new Contact { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-45) }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        var start = DateTime.UtcNow.AddDays(-30);
        var end = DateTime.UtcNow;

        // Act
        var result = await _repository.GetContactsAddedInPeriodAsync(start, end);

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public async Task FindDuplicatesAsync_ByEmail_ReturnsDuplicates()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Email = "same@example.com", FirstName = "John" },
            new Contact { Id = 2, Email = "same@example.com", FirstName = "Johnny" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.FindDuplicatesAsync("same@example.com");

        // Assert
        result.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task FindPotentialDuplicatesAsync_SimilarNames_ReturnsPotentialMatches()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Smith" },
            new Contact { Id = 2, FirstName = "Jon", LastName = "Smith" }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.FindPotentialDuplicatesAsync("John", "Smith");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public async Task GetWithAccountAsync_ReturnsContactWithAccount()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "John",
            Account = new ContactAccount { Id = 1, Company = "Acme" }
        };

        var contacts = new List<Contact> { contact }.AsQueryable();
        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetWithAccountAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Account.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWithActivitiesAsync_ReturnsContactWithActivities()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "John",
            Activities = new List<ContactActivity>
            {
                new ContactActivity { Id = 1, Type = "Call" },
                new ContactActivity { Id = 2, Type = "Email" }
            }
        };

        var contacts = new List<Contact> { contact }.AsQueryable();
        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetWithActivitiesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Activities.Should().HaveCount(2);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyContactedAsync_ReturnsRecentContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, LastContactedAt = DateTime.UtcNow.AddDays(-1) },
            new Contact { Id = 2, LastContactedAt = DateTime.UtcNow.AddDays(-10) },
            new Contact { Id = 3, LastContactedAt = DateTime.UtcNow.AddDays(-60) }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetRecentlyContactedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetNeverContactedAsync_ReturnsNeverContacted()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, LastContactedAt = DateTime.UtcNow },
            new Contact { Id = 2, LastContactedAt = null },
            new Contact { Id = 3, LastContactedAt = null }
        }.AsQueryable();

        SetupMockDbSet(contacts);

        // Act
        var result = await _repository.GetNeverContactedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkUpdateAccountAsync_UpdatesContacts()
    {
        // Arrange
        var contactIds = new[] { 1, 2, 3 };
        var newAccountId = 10;

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkUpdateAccountAsync(contactIds, newAccountId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task BulkDeleteAsync_DeletesContacts()
    {
        // Arrange
        var contactIds = new[] { 1, 2, 3 };

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkDeleteAsync(contactIds);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<Contact> data)
    {
        _mockDbSet.As<IQueryable<Contact>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<Contact>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<Contact>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<Contact>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting classes
public class Contact
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public int? AccountId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ContactAccount? Account { get; set; }
    public List<ContactActivity> Activities { get; set; } = new();
}

public class ContactAccount
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
}

public class ContactActivity
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
}
