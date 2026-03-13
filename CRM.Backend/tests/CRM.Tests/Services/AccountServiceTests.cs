// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for AccountService (TCOV Wave-A).</summary>
public class AccountServiceTests
{
    private readonly Mock<IRepository<Account>> _mockAccountRepo;
    private readonly Mock<IRepository<AccountContact>> _mockAccountContactRepo;
    private readonly Mock<IContactsService> _mockContactsService;
    private readonly Mock<IContactInfoService> _mockContactInfoService;
    private readonly Mock<IRepository<Address>> _mockAddressRepo;
    private readonly Mock<IRepository<ContactDetail>> _mockContactDetailRepo;
    private readonly Mock<IRepository<SocialAccount>> _mockSocialAccountRepo;
    private readonly Mock<IRepository<ContactInfoLink>> _mockContactInfoLinkRepo;
    private readonly Mock<IRepository<CRM.Core.Entities.EntityTag>> _mockEntityTagRepo;
    private readonly Mock<IRepository<CRM.Core.Entities.CustomField>> _mockCustomFieldRepo;
    private readonly Mock<INormalizationService> _mockNormalizationService;
    private readonly Mock<IEntityEventDispatcher> _mockDispatcher;
    private readonly Mock<IPreferencesService> _mockPreferencesService;
    private readonly Mock<IDuplicateDetectionService> _mockDuplicates;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<AccountService>> _mockLogger;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _mockAccountRepo = new Mock<IRepository<Account>>();
        _mockAccountContactRepo = new Mock<IRepository<AccountContact>>();
        _mockContactsService = new Mock<IContactsService>();
        _mockContactInfoService = new Mock<IContactInfoService>();
        _mockAddressRepo = new Mock<IRepository<Address>>();
        _mockContactDetailRepo = new Mock<IRepository<ContactDetail>>();
        _mockSocialAccountRepo = new Mock<IRepository<SocialAccount>>();
        _mockContactInfoLinkRepo = new Mock<IRepository<ContactInfoLink>>();
        _mockEntityTagRepo = new Mock<IRepository<CRM.Core.Entities.EntityTag>>();
        _mockCustomFieldRepo = new Mock<IRepository<CRM.Core.Entities.CustomField>>();
        _mockNormalizationService = new Mock<INormalizationService>();
        _mockDispatcher = new Mock<IEntityEventDispatcher>();
        _mockPreferencesService = new Mock<IPreferencesService>();
        _mockDuplicates = new Mock<IDuplicateDetectionService>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<AccountService>>();

        // Duplicate detection returns no duplicates
        _mockDuplicates
            .Setup(d => d.CheckForDuplicatesAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string?>>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        // Event dispatcher completes without side effects
        _mockDispatcher
            .Setup(e => e.DispatchEntityEventAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<WorkflowTriggerType>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // MapToDto normalization stubs — return null so account fields are used as fallback
        _mockNormalizationService
            .Setup(n => n.GetPrimaryEmailAsync(It.IsAny<ContactInfoOwnerType>(), It.IsAny<int>()))
            .ReturnsAsync((string?)null);
        _mockNormalizationService
            .Setup(n => n.GetPrimaryPhoneAsync(It.IsAny<ContactInfoOwnerType>(), It.IsAny<int>()))
            .ReturnsAsync((string?)null);
        _mockNormalizationService
            .Setup(n => n.GetPrimaryFaxAsync(It.IsAny<ContactInfoOwnerType>(), It.IsAny<int>()))
            .ReturnsAsync((string?)null);
        _mockNormalizationService
            .Setup(n => n.GetTagsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string?)null);
        _mockNormalizationService
            .Setup(n => n.GetCustomFieldsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string?)null);
        _mockNormalizationService
            .Setup(n => n.GetPrimarySocialAccountAsync(
                It.IsAny<ContactInfoOwnerType>(), It.IsAny<int>(), It.IsAny<SocialNetwork>()))
            .ReturnsAsync((string?)null);

        // MapToDto address stub — return empty list
        _mockContactInfoService
            .Setup(c => c.GetAddressesAsync(It.IsAny<EntityType>(), It.IsAny<int>()))
            .ReturnsAsync(new List<LinkedAddressDto>());

        // Preferences stub
        _mockPreferencesService
            .Setup(p => p.GetAccountDefaultsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreferencesDto());

        // Junction repo — returns empty for MapToDto contact lookup
        _mockAccountContactRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<AccountContact, bool>>()))
            .ReturnsAsync(new List<AccountContact>());

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
            _mockDispatcher.Object,
            _mockPreferencesService.Object,
            _mockDuplicates.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    // ------------------------------------------------------------------
    // GetAccountByIdAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
    {
        var account = new Account { Id = 1, FirstName = "Jane", LastName = "Doe", IsDeleted = false };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var result = await _service.GetAccountByIdAsync(1);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnNull_WhenAccountNotFound()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Account?)null);

        var result = await _service.GetAccountByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnNull_WhenAccountIsDeleted()
    {
        var account = new Account { Id = 2, FirstName = "Deleted", LastName = "Account", IsDeleted = true };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);

        var result = await _service.GetAccountByIdAsync(2);

        result.Should().BeNull();
    }

    // ------------------------------------------------------------------
    // GetAllAccountsAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAllAccountsAsync_ShouldReturnActiveAccounts()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "A", IsDeleted = false },
            new() { Id = 2, FirstName = "Bob", LastName = "B", IsDeleted = false }
        };
        // GetAllAccountsAsync calls GetAllAsync() then filters in-memory
        _mockAccountRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(accounts);

        var result = await _service.GetAllAccountsAsync();

        result.Should().HaveCount(2);
    }

    // ------------------------------------------------------------------
    // DeleteAccountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task DeleteAccountAsync_ShouldSoftDelete_WhenAccountExists()
    {
        var account = new Account { Id = 5, FirstName = "Target", IsDeleted = false };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);
        _mockAccountRepo.Setup(r => r.UpdateAsync(account)).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var result = await _service.DeleteAccountAsync(5);

        result.Should().BeTrue();
        account.IsDeleted.Should().BeTrue();
        _mockAccountRepo.Verify(r => r.UpdateAsync(account), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldReturnFalse_WhenAccountNotFound()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Account?)null);

        var result = await _service.DeleteAccountAsync(999);

        result.Should().BeFalse();
        _mockAccountRepo.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }
}
