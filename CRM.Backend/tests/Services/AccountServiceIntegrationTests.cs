// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// PRA-016: Re-enabled - fixed Expression<Func> vs Func and ReturnsAsync issues
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Entities.Workflow; // PRA-016

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive service-level tests for AccountService.
/// Tests exception handling, validation, business rules, and edge cases.
/// </summary>
public class AccountServiceIntegrationTests
{
    private readonly Mock<IRepository<Account>> _mockAccountRepo;
    private readonly Mock<IRepository<AccountContact>> _mockAccountContactRepo;
    private readonly Mock<IContactsService> _mockContactsService;
    private readonly Mock<IContactInfoService> _mockContactInfoService;
    private readonly Mock<IRepository<Address>> _mockAddressRepo;
    private readonly Mock<IRepository<ContactDetail>> _mockContactDetailRepo;
    private readonly Mock<IRepository<SocialAccount>> _mockSocialAccountRepo;
    private readonly Mock<IRepository<ContactInfoLink>> _mockContactInfoLinkRepo;
    private readonly Mock<IRepository<Core.Entities.EntityTag>> _mockEntityTagRepo;
    private readonly Mock<IRepository<Core.Entities.CustomField>> _mockCustomFieldRepo;
    private readonly Mock<INormalizationService> _mockNormalizationService; // PRA-016: use interface
    private readonly Mock<IEntityEventDispatcher> _mockEventDispatcher;
    private readonly Mock<IPreferencesService> _mockPreferencesService;
    private readonly Mock<IDuplicateDetectionService> _mockDuplicateDetection;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<AccountService>> _mockLogger;
    private readonly AccountService _service;

    public AccountServiceIntegrationTests()
    {
        _mockAccountRepo = new Mock<IRepository<Account>>();
        _mockAccountContactRepo = new Mock<IRepository<AccountContact>>();
        _mockContactsService = new Mock<IContactsService>();
        _mockContactInfoService = new Mock<IContactInfoService>();
        _mockAddressRepo = new Mock<IRepository<Address>>();
        _mockContactDetailRepo = new Mock<IRepository<ContactDetail>>();
        _mockSocialAccountRepo = new Mock<IRepository<SocialAccount>>();
        _mockContactInfoLinkRepo = new Mock<IRepository<ContactInfoLink>>();
        _mockEntityTagRepo = new Mock<IRepository<Core.Entities.EntityTag>>();
        _mockCustomFieldRepo = new Mock<IRepository<Core.Entities.CustomField>>();
        _mockNormalizationService = new Mock<INormalizationService>(); // PRA-016
        _mockEventDispatcher = new Mock<IEntityEventDispatcher>();
        _mockPreferencesService = new Mock<IPreferencesService>();
        _mockDuplicateDetection = new Mock<IDuplicateDetectionService>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<AccountService>>();

        // PRA-016: Global setups needed for CreateAccountAsync flow
        _mockEventDispatcher
            .Setup(d => d.DispatchEntityEventAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<WorkflowTriggerType>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockDbContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        // PRA-016: MapToDto calls GetAddressesAsync; return empty list to avoid null ArgumentNullException
        _mockContactInfoService
            .Setup(s => s.GetAddressesAsync(It.IsAny<EntityType>(), It.IsAny<int>()))
            .ReturnsAsync(new List<LinkedAddressDto>());
        // PRA-016: MapToDto accesses preferences properties directly; must return non-null
        _mockPreferencesService
            .Setup(p => p.GetAccountDefaultsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto());

        _service = new AccountService(
            _mockAccountRepo.Object,
            _mockAccountContactRepo.Object,
            _mockContactsService.Object,
            _mockContactInfoService.Object,
            _mockAddressRepo.Object,
            _mockContactDetailRepo.Object,
            _mockSocialAccountRepo.Object,
            _mockContactInfoLinkRepo.Object,
            _mockEntityTagRepo.Object,
            _mockCustomFieldRepo.Object,
            _mockNormalizationService.Object,
            _mockEventDispatcher.Object,
            _mockPreferencesService.Object,
            _mockDuplicateDetection.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    // ========================================================================
    // Exception Handling Tests for CreateAccountAsync
    // ========================================================================

    #region Duplicate Email Validation

    /// <summary>
    /// Test: CreateAccountAsync with duplicate email should throw InvalidOperationException
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingAccount = new Account { Id = 1, Email = "duplicate@test.com" };
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account> { existingAccount });
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "duplicate@test.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAccountAsync(dto));
    }

    /// <summary>
    /// Test: CreateAccountAsync with null email should succeed
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithNullEmail_Succeeds()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockAccountRepo.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());
        _mockPreferencesService.Setup(p => p.UpdateAccountPreferencesAsync(It.IsAny<int>(), It.IsAny<PreferencesDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto()); // PRA-016: method returns Task<PreferencesDto>

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = null
        };

        // Act
        var result = await _service.CreateAccountAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
    }

    /// <summary>
    /// Test: CreateAccountAsync with empty email should succeed
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithEmptyEmail_Succeeds()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockAccountRepo.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());
        _mockPreferencesService.Setup(p => p.UpdateAccountPreferencesAsync(It.IsAny<int>(), It.IsAny<PreferencesDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto()); // PRA-016: method returns Task<PreferencesDto>

        var dto = new CreateAccountDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = ""
        };

        // Act
        var result = await _service.CreateAccountAsync(dto);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Phone Format Validation

    /// <summary>
    /// Test: CreateAccountAsync with invalid phone format should throw InvalidOperationException
    /// </summary>
    [Theory]
    [InlineData("abc-def-ghij")]
    [InlineData("555@123#4567")]
    [InlineData("not-a-phone")]
    [InlineData("###-###-####")]
    public async Task CreateAccountAsync_WithInvalidPhoneFormat_ThrowsInvalidOperationException(string invalidPhone)
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Phone = invalidPhone
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAccountAsync(dto));
    }

    /// <summary>
    /// Test: CreateAccountAsync with valid phone formats should succeed
    /// </summary>
    [Theory]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("555 123 4567")]
    [InlineData("+1-555-123-4567")]
    [InlineData("+44 20 7946 0958")]
    public async Task CreateAccountAsync_WithValidPhoneFormat_Succeeds(string validPhone)
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockAccountRepo.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());
        _mockPreferencesService.Setup(p => p.UpdateAccountPreferencesAsync(It.IsAny<int>(), It.IsAny<PreferencesDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto()); // PRA-016: method returns Task<PreferencesDto>
        _mockContactDetailRepo.Setup(r => r.AddAsync(It.IsAny<ContactDetail>())).Returns(Task.CompletedTask);
        _mockContactDetailRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016
        _mockContactInfoLinkRepo.Setup(r => r.AddAsync(It.IsAny<ContactInfoLink>())).Returns(Task.CompletedTask);
        _mockContactInfoLinkRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Phone = validPhone
        };

        // Act
        var result = await _service.CreateAccountAsync(dto);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Test: CreateAccountAsync with invalid mobile phone format should throw
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithInvalidMobilePhoneFormat_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            MobilePhone = "invalid@mobile"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAccountAsync(dto));
    }

    /// <summary>
    /// Test: CreateAccountAsync with invalid fax format should throw
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithInvalidFaxFormat_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            FaxNumber = "###INVALID###"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAccountAsync(dto));
    }

    #endregion

    #region GetAccountByIdAsync Tests

    /// <summary>
    /// Test: GetAccountByIdAsync with non-existent ID should return null
    /// </summary>
    [Fact]
    public async Task GetAccountByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Test: GetAccountByIdAsync with deleted account should return null
    /// </summary>
    [Fact]
    public async Task GetAccountByIdAsync_WithDeletedAccount_ReturnsNull()
    {
        // Arrange
        var deletedAccount = new Account
        {
            Id = 1,
            FirstName = "Deleted",
            LastName = "Account",
            IsDeleted = true
        };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deletedAccount);

        // Act
        var result = await _service.GetAccountByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Test: GetAccountByIdAsync with negative ID should return null
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public async Task GetAccountByIdAsync_WithNegativeId_ReturnsNull(int invalidId)
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.GetByIdAsync(invalidId))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Test: GetAccountByIdAsync with zero ID should return null
    /// </summary>
    [Fact]
    public async Task GetAccountByIdAsync_WithZeroId_ReturnsNull()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.GetByIdAsync(0))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountByIdAsync(0);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchAccountsAsync Tests

    /// <summary>
    /// Test: SearchAccountsAsync with null search term should handle gracefully
    /// </summary>
    [Fact]
    public async Task SearchAccountsAsync_WithNullSearchTerm_HandlesGracefully()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());

        // Act - should not throw
        var result = await _service.SearchAccountsAsync(null!);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Test: SearchAccountsAsync with empty string should return all active accounts
    /// </summary>
    [Fact]
    public async Task SearchAccountsAsync_WithEmptyString_ReturnsAccounts()
    {
        // Arrange
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());

        // Act
        var result = await _service.SearchAccountsAsync("");

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Test: SearchAccountsAsync should exclude deleted accounts
    /// </summary>
    [Fact]
    public async Task SearchAccountsAsync_ShouldExcludeDeletedAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, FirstName = "Active", IsDeleted = false },
            new Account { Id = 2, FirstName = "Deleted", IsDeleted = true }
        };
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(accounts.Where(a => !a.IsDeleted).ToList());

        // Act
        var result = await _service.SearchAccountsAsync("Test");

        // Assert
        result.Should().HaveCount(1);
        result.Should().NotContain(a => a.FirstName == "Deleted");
    }

    /// <summary>
    /// Test: SearchAccountsAsync with special characters should work
    /// </summary>
    [Fact]
    public async Task SearchAccountsAsync_WithSpecialCharacters_ReturnsMatches()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, FirstName = "José", LastName = "García", IsDeleted = false }
        };
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.SearchAccountsAsync("José");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Boundary Condition Tests

    /// <summary>
    /// Test: CreateAccountAsync with very long FirstName should be accepted or truncated
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithVeryLongFirstName_IsHandled()
    {
        // Arrange
        var longName = new string('A', 500);
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockAccountRepo.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());
        _mockPreferencesService.Setup(p => p.UpdateAccountPreferencesAsync(It.IsAny<int>(), It.IsAny<PreferencesDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto()); // PRA-016: method returns Task<PreferencesDto>

        var dto = new CreateAccountDto
        {
            FirstName = longName,
            LastName = "Test",
            Email = "test@example.com"
        };

        // Act
        var result = await _service.CreateAccountAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Test: CreateAccountAsync with negative AnnualRevenue should be accepted
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithNegativeAnnualRevenue_IsAccepted()
    {
        // Arrange - negative might represent debt or losses
        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account>());
        _mockAccountRepo.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask); // PRA-016
        _mockDuplicateDetection.Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());
        _mockPreferencesService.Setup(p => p.UpdateAccountPreferencesAsync(It.IsAny<int>(), It.IsAny<PreferencesDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto()); // PRA-016: method returns Task<PreferencesDto>

        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            AnnualRevenue = -50000m
        };

        // Act
        var result = await _service.CreateAccountAsync(dto);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
