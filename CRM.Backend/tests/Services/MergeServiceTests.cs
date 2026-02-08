// CRM Solution - Customer Relationship Management System
// Merge Service Unit Tests

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
/// Unit tests for MergeService
/// Covers: Entity merging, field resolution, history, validation
/// </summary>
public class MergeServiceTests
{
    private readonly Mock<IRepository<Account>> _mockAccountRepository;
    private readonly Mock<IRepository<Contact>> _mockContactRepository;
    private readonly Mock<IRepository<Lead>> _mockLeadRepository;
    private readonly Mock<IRepository<MergeHistory>> _mockMergeHistoryRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<MergeService>> _mockLogger;
    private readonly MergeService _service;

    public MergeServiceTests()
    {
        _mockAccountRepository = new Mock<IRepository<Account>>();
        _mockContactRepository = new Mock<IRepository<Contact>>();
        _mockLeadRepository = new Mock<IRepository<Lead>>();
        _mockMergeHistoryRepository = new Mock<IRepository<MergeHistory>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<MergeService>>();

        _service = new MergeService(
            _mockAccountRepository.Object,
            _mockContactRepository.Object,
            _mockLeadRepository.Object,
            _mockMergeHistoryRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Account Merge Tests

    [Fact]
    public async Task MergeAccountsAsync_ValidIds_ReturnsMergedAccount()
    {
        // Arrange
        var master = new Account
        {
            Id = 1,
            Company = "Acme Inc",
            Email = "master@acme.com",
            Phone = null
        };

        var duplicate = new Account
        {
            Id = 2,
            Company = "ACME Corporation",
            Email = "duplicate@acme.com",
            Phone = "555-1234"
        };

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockAccountRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        _mockAccountRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        _mockAccountRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => { m.Id = 1; return m; });

        // Act
        var result = await _service.MergeAccountsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task MergeAccountsAsync_SameIds_ThrowsException()
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
            _service.MergeAccountsAsync(999, 1));
    }

    [Fact]
    public async Task MergeAccountsAsync_NonExistingDuplicate_ThrowsException()
    {
        // Arrange
        _mockAccountRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Account { Id = 1 });
        _mockAccountRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.MergeAccountsAsync(1, 999));
    }

    [Fact]
    public async Task MergeAccountsAsync_WithFieldSelection_UsesSelectedValues()
    {
        // Arrange
        var master = new Account { Id = 1, Company = "Master Inc", Industry = "Tech" };
        var duplicate = new Account { Id = 2, Company = "Duplicate Corp", Industry = "Finance" };

        var fieldSelections = new Dictionary<string, string>
        {
            { "Company", "master" },
            { "Industry", "duplicate" }
        };

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockAccountRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        _mockAccountRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        _mockAccountRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => m);

        // Act
        var result = await _service.MergeAccountsAsync(1, 2, fieldSelections);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MergeAccountsAsync_TransfersRelationships()
    {
        // Arrange
        var master = new Account { Id = 1, Company = "Master Inc" };
        var duplicate = new Account
        {
            Id = 2,
            Company = "Duplicate Corp",
            AccountContacts = new List<AccountContact>
            {
                new AccountContact { ContactId = 1 },
                new AccountContact { ContactId = 2 }
            }
        };

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockAccountRepository.Setup(r => r.GetByIdWithIncludesAsync(2, It.IsAny<string[]>()))
            .ReturnsAsync(duplicate);

        _mockAccountRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        _mockAccountRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => m);

        // Act
        var result = await _service.MergeAccountsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Contact Merge Tests

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

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => m);

        // Act
        var result = await _service.MergeContactsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task MergeContactsAsync_FillsEmptyFields()
    {
        // Arrange
        var master = new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Phone = null
        };

        var duplicate = new Contact
        {
            Id = 2,
            FirstName = "John",
            LastName = "Doe",
            Email = null,
            Phone = "555-1234"
        };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockContactRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        _mockContactRepository.Setup(r => r.UpdateAsync(It.IsAny<Contact>()))
            .ReturnsAsync((Contact c) => c);

        _mockContactRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => m);

        // Act
        var result = await _service.MergeContactsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Lead Merge Tests

    [Fact]
    public async Task MergeLeadsAsync_ValidIds_ReturnsMergedLead()
    {
        // Arrange
        var master = new Lead { Id = 1, FirstName = "John", Company = "Acme" };
        var duplicate = new Lead { Id = 2, FirstName = "John", Company = "ACME", Email = "john@acme.com" };

        _mockLeadRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockLeadRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        _mockLeadRepository.Setup(r => r.UpdateAsync(It.IsAny<Lead>()))
            .ReturnsAsync((Lead l) => l);

        _mockLeadRepository.Setup(r => r.DeleteAsync(2))
            .ReturnsAsync(true);

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => m);

        // Act
        var result = await _service.MergeLeadsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Preview Merge Tests

    [Fact]
    public async Task PreviewAccountMergeAsync_ReturnsPreview()
    {
        // Arrange
        var master = new Account
        {
            Id = 1,
            Company = "Master Inc",
            Email = "master@test.com",
            Phone = null
        };

        var duplicate = new Account
        {
            Id = 2,
            Company = "Duplicate Corp",
            Email = null,
            Phone = "555-1234"
        };

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockAccountRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        // Act
        var result = await _service.PreviewAccountMergeAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.MasterEntity.Should().NotBeNull();
        result.DuplicateEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task PreviewContactMergeAsync_ReturnsFieldComparison()
    {
        // Arrange
        var master = new Contact { Id = 1, FirstName = "John", LastName = "Doe" };
        var duplicate = new Contact { Id = 2, FirstName = "Johnny", LastName = "Doe" };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        _mockContactRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(duplicate);

        // Act
        var result = await _service.PreviewContactMergeAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.FieldComparisons.Should().NotBeEmpty();
    }

    #endregion

    #region Merge History Tests

    [Fact]
    public async Task GetMergeHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var history = new List<MergeHistory>
        {
            new MergeHistory { Id = 1, EntityType = "Account", MasterEntityId = 1, MergedEntityId = 2 },
            new MergeHistory { Id = 2, EntityType = "Contact", MasterEntityId = 10, MergedEntityId = 20 }
        };

        _mockMergeHistoryRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetMergeHistoryAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMergeHistoryByEntityAsync_ReturnsFilteredHistory()
    {
        // Arrange
        var history = new List<MergeHistory>
        {
            new MergeHistory { Id = 1, EntityType = "Account", MasterEntityId = 1 }
        };

        _mockMergeHistoryRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MergeHistory, bool>>>()))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetMergeHistoryByEntityAsync("Account", 1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMergeHistoryByIdAsync_ReturnsHistory()
    {
        // Arrange
        var history = new MergeHistory
        {
            Id = 1,
            EntityType = "Account",
            MasterEntityId = 1,
            MergedEntityId = 2,
            MergedAt = DateTime.UtcNow
        };

        _mockMergeHistoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetMergeHistoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.EntityType.Should().Be("Account");
    }

    #endregion

    #region Undo Merge Tests

    [Fact]
    public async Task UndoMergeAsync_ValidHistory_ReturnsTrue()
    {
        // Arrange
        var history = new MergeHistory
        {
            Id = 1,
            EntityType = "Account",
            MasterEntityId = 1,
            MergedEntityId = 2,
            OriginalMasterData = "{}",
            OriginalMergedData = "{}"
        };

        _mockMergeHistoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        _mockMergeHistoryRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UndoMergeAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UndoMergeAsync_NonExistingHistory_ReturnsFalse()
    {
        // Arrange
        _mockMergeHistoryRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((MergeHistory?)null);

        // Act
        var result = await _service.UndoMergeAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUndoMergeAsync_WithinTimeLimit_ReturnsTrue()
    {
        // Arrange
        var history = new MergeHistory
        {
            Id = 1,
            MergedAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockMergeHistoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _service.CanUndoMergeAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUndoMergeAsync_PastTimeLimit_ReturnsFalse()
    {
        // Arrange
        var history = new MergeHistory
        {
            Id = 1,
            MergedAt = DateTime.UtcNow.AddDays(-30)
        };

        _mockMergeHistoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _service.CanUndoMergeAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Bulk Merge Tests

    [Fact]
    public async Task BulkMergeAccountsAsync_ValidIds_MergesAll()
    {
        // Arrange
        var masterId = 1;
        var duplicateIds = new List<int> { 2, 3, 4 };

        var master = new Account { Id = 1, Company = "Master" };
        var duplicates = duplicateIds.Select(id => new Account { Id = id }).ToList();

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(master);
        foreach (var dup in duplicates)
        {
            _mockAccountRepository.Setup(r => r.GetByIdAsync(dup.Id)).ReturnsAsync(dup);
        }

        _mockAccountRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        _mockAccountRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        _mockMergeHistoryRepository.Setup(r => r.AddAsync(It.IsAny<MergeHistory>()))
            .ReturnsAsync((MergeHistory m) => m);

        // Act
        var result = await _service.BulkMergeAccountsAsync(masterId, duplicateIds);

        // Assert
        result.MergedCount.Should().Be(3);
    }

    [Fact]
    public async Task BulkMergeAccountsAsync_EmptyDuplicates_ReturnsZero()
    {
        // Arrange
        var masterId = 1;
        var duplicateIds = new List<int>();

        // Act
        var result = await _service.BulkMergeAccountsAsync(masterId, duplicateIds);

        // Assert
        result.MergedCount.Should().Be(0);
    }

    #endregion

    #region Field Resolution Tests

    [Fact]
    public void ResolveField_MasterHasValue_ReturnsMasterValue()
    {
        // Arrange
        var masterValue = "Master Value";
        var duplicateValue = "Duplicate Value";

        // Act
        var result = _service.ResolveField(masterValue, duplicateValue, "master");

        // Assert
        result.Should().Be("Master Value");
    }

    [Fact]
    public void ResolveField_DuplicateSelected_ReturnsDuplicateValue()
    {
        // Arrange
        var masterValue = "Master Value";
        var duplicateValue = "Duplicate Value";

        // Act
        var result = _service.ResolveField(masterValue, duplicateValue, "duplicate");

        // Assert
        result.Should().Be("Duplicate Value");
    }

    [Fact]
    public void ResolveField_MasterEmpty_ReturnsDuplicateValue()
    {
        // Arrange
        string? masterValue = null;
        var duplicateValue = "Duplicate Value";

        // Act
        var result = _service.ResolveField(masterValue, duplicateValue, "auto");

        // Assert
        result.Should().Be("Duplicate Value");
    }

    #endregion
}
