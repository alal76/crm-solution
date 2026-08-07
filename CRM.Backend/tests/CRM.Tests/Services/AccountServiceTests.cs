// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
    private readonly Mock<IAccountContactService> _mockAccountContactService;
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
        _mockAccountContactService = new Mock<IAccountContactService>();

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
            _mockLogger.Object,
            _mockAccountContactService.Object);
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

    // ------------------------------------------------------------------
    // SearchAccountsAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task SearchAccountsAsync_ShouldMatchByCompany_AndExcludeDeleted()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, FirstName = "", LastName = "", Email = "", Company = "Acme Corp", IsDeleted = false },
            new() { Id = 2, FirstName = "", LastName = "", Email = "", Company = "Other Co", IsDeleted = false },
            new() { Id = 3, FirstName = "", LastName = "", Email = "", Company = "Acme Corp", IsDeleted = true }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.SearchAccountsAsync("Acme")).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task SearchAccountsAsync_ShouldMatchAcrossNameEmailAndCompanyFields()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, FirstName = "Zephyr", LastName = "", Email = "", Company = "", IsDeleted = false },
            new() { Id = 2, FirstName = "", LastName = "Zephyr", Email = "", Company = "", IsDeleted = false },
            new() { Id = 3, FirstName = "", LastName = "", Email = "Zephyr@example.com", Company = "", IsDeleted = false },
            new() { Id = 4, FirstName = "", LastName = "", Email = "", Company = "NoMatch", IsDeleted = false }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.SearchAccountsAsync("Zephyr")).ToList();

        result.Should().HaveCount(3);
        result.Select(r => r.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    // ------------------------------------------------------------------
    // GetIndividualAccountsAsync / GetOrganizationAccountsAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetIndividualAccountsAsync_ShouldReturnOnlyIndividualNonDeleted()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, Category = AccountCategory.Individual, IsDeleted = false },
            new() { Id = 2, Category = AccountCategory.Organization, IsDeleted = false },
            new() { Id = 3, Category = AccountCategory.Individual, IsDeleted = true }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.GetIndividualAccountsAsync()).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetOrganizationAccountsAsync_ShouldReturnOnlyOrganizationNonDeleted()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, Category = AccountCategory.Individual, IsDeleted = false },
            new() { Id = 2, Category = AccountCategory.Organization, IsDeleted = false },
            new() { Id = 3, Category = AccountCategory.Organization, IsDeleted = true }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.GetOrganizationAccountsAsync()).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(2);
    }

    // ------------------------------------------------------------------
    // GetAccountsByAssignedUserAsync / GetAccountsByLifecycleStageAsync / GetAccountsByPriorityAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAccountsByAssignedUserAsync_ShouldFilterByUser_AndExcludeDeleted()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, AssignedToUserId = 10, IsDeleted = false },
            new() { Id = 2, AssignedToUserId = 20, IsDeleted = false },
            new() { Id = 3, AssignedToUserId = 10, IsDeleted = true }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.GetAccountsByAssignedUserAsync(10)).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAccountsByLifecycleStageAsync_ShouldFilterByStage()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, LifecycleStage = AccountLifecycleStage.Active, IsDeleted = false },
            new() { Id = 2, LifecycleStage = AccountLifecycleStage.Lead, IsDeleted = false }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.GetAccountsByLifecycleStageAsync(AccountLifecycleStage.Active)).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAccountsByPriorityAsync_ShouldFilterByPriority()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, Priority = AccountPriority.High, IsDeleted = false },
            new() { Id = 2, Priority = AccountPriority.Medium, IsDeleted = false },
            new() { Id = 3, Priority = AccountPriority.High, IsDeleted = true }
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .Returns<Func<Account, bool>>(predicate => Task.FromResult<IEnumerable<Account>>(accounts.Where(predicate).ToList()));

        var result = (await _service.GetAccountsByPriorityAsync(AccountPriority.High)).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
    }

    // ------------------------------------------------------------------
    // CreateAccountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateAccountAsync_HappyPath_CreatesAccountAndMaterializesContactDetails()
    {
        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-123-4567",
            Category = AccountCategory.Individual,
            ShippingSameAsBilling = false
        };

        _mockAccountRepo.Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>())).ReturnsAsync(new List<Account>());
        _mockAccountRepo.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        _mockContactDetailRepo.Setup(r => r.AddAsync(It.IsAny<ContactDetail>())).Returns(Task.CompletedTask);
        _mockContactDetailRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        _mockContactInfoLinkRepo.Setup(r => r.AddAsync(It.IsAny<ContactInfoLink>())).Returns(Task.CompletedTask);
        _mockContactInfoLinkRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var result = await _service.CreateAccountAsync(dto);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");

        _mockAccountRepo.Verify(r => r.AddAsync(It.Is<Account>(a => a.FirstName == "John" && a.Email == "john@example.com")), Times.Once);
        _mockContactDetailRepo.Verify(r => r.AddAsync(It.Is<ContactDetail>(cd => cd.DetailType == ContactDetailType.Email && cd.Value == "john@example.com")), Times.Once);
        _mockContactDetailRepo.Verify(r => r.AddAsync(It.Is<ContactDetail>(cd => cd.DetailType == ContactDetailType.Phone && cd.Value == "555-123-4567")), Times.Once);
        _mockDispatcher.Verify(e => e.DispatchEntityEventAsync(
            "Account", It.IsAny<int>(), WorkflowTriggerType.OnCreate,
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAccountAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var dto = new CreateAccountDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "existing@example.com"
        };
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account> { new() { Id = 1, Email = "existing@example.com" } });

        Func<Task> act = async () => await _service.CreateAccountAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*existing@example.com*already exists*");
        _mockAccountRepo.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccountAsync_InvalidPhoneFormat_ThrowsInvalidOperationException()
    {
        var dto = new CreateAccountDto
        {
            FirstName = "John",
            LastName = "Doe",
            Phone = "555@invalid#phone"
        };

        Func<Task> act = async () => await _service.CreateAccountAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid phone format*");
        _mockAccountRepo.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // UpdateAccountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnNull_WhenAccountNotFound()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Account?)null);

        var result = await _service.UpdateAccountAsync(999, new UpdateAccountDto());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnNull_WhenAccountIsDeleted()
    {
        var account = new Account { Id = 1, IsDeleted = true };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var result = await _service.UpdateAccountAsync(1, new UpdateAccountDto());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldThrow_WhenEmailBelongsToDifferentAccount()
    {
        var account = new Account { Id = 1, Email = "old@example.com", IsDeleted = false };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);
        _mockAccountRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<Account, bool>>()))
            .ReturnsAsync(new List<Account> { new() { Id = 2, Email = "new@example.com" } });

        var dto = new UpdateAccountDto { Email = "new@example.com" };

        Func<Task> act = async () => await _service.UpdateAccountAsync(1, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*new@example.com*already exists*");
        _mockAccountRepo.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldThrow_WhenPhoneFormatInvalid()
    {
        var account = new Account { Id = 1, IsDeleted = false };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var dto = new UpdateAccountDto { Phone = "invalid###phone" };

        Func<Task> act = async () => await _service.UpdateAccountAsync(1, dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid phone format*");
        _mockAccountRepo.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldSkipDuplicateCheck_AndUpdateFields_WhenEmailUnchanged()
    {
        var account = new Account { Id = 1, Email = "same@example.com", Company = "OldCo", IsDeleted = false };
        _mockAccountRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);
        _mockAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _mockAccountRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        _mockContactInfoLinkRepo.Setup(r => r.FindAsync(It.IsAny<Func<ContactInfoLink, bool>>())).ReturnsAsync(new List<ContactInfoLink>());
        _mockContactInfoLinkRepo.Setup(r => r.AddAsync(It.IsAny<ContactInfoLink>())).Returns(Task.CompletedTask);
        _mockContactInfoLinkRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        _mockContactDetailRepo.Setup(r => r.AddAsync(It.IsAny<ContactDetail>())).Returns(Task.CompletedTask);
        _mockContactDetailRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateAccountDto { Email = "same@example.com", Company = "NewCo" };

        var result = await _service.UpdateAccountAsync(1, dto);

        result.Should().NotBeNull();
        account.Company.Should().Be("NewCo");
        account.LastActivityDate.Should().NotBeNull();
        // The duplicate-email guard queries the account repo only when the email actually changes.
        _mockAccountRepo.Verify(r => r.FindAsync(It.IsAny<Func<Account, bool>>()), Times.Never);
        _mockDispatcher.Verify(e => e.DispatchEntityEventAsync(
            "Account", 1, WorkflowTriggerType.OnUpdate,
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ------------------------------------------------------------------
    // Address management (GetAccountAddressesAsync, GetPrimary*AddressAsync, SetPrimary*AddressAsync)
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAccountAddressesAsync_ShouldDelegateToContactInfoService()
    {
        var addresses = new List<LinkedAddressDto> { new() { Id = 1, Line1 = "123 Main St", AddressType = "Billing" } };
        _mockContactInfoService.Setup(c => c.GetAddressesAsync(EntityType.Account, 5)).ReturnsAsync(addresses);

        var result = await _service.GetAccountAddressesAsync(5);

        result.Should().BeEquivalentTo(addresses);
    }

    [Fact]
    public async Task GetPrimaryBillingAddressAsync_ShouldReturnAddressMarkedPrimary()
    {
        var addresses = new List<LinkedAddressDto>
        {
            new() { Id = 1, AddressType = "Billing", IsPrimary = false, Line1 = "A" },
            new() { Id = 2, AddressType = "Billing", IsPrimary = true, Line1 = "B" },
            new() { Id = 3, AddressType = "Shipping", IsPrimary = true, Line1 = "C" }
        };
        _mockContactInfoService.Setup(c => c.GetAddressesAsync(EntityType.Account, 5)).ReturnsAsync(addresses);

        var result = await _service.GetPrimaryBillingAddressAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetPrimaryBillingAddressAsync_ShouldFallBackToFirstMatch_WhenNonePrimary()
    {
        var addresses = new List<LinkedAddressDto>
        {
            new() { Id = 1, AddressType = "Billing", IsPrimary = false, Line1 = "A" },
            new() { Id = 2, AddressType = "Billing", IsPrimary = false, Line1 = "B" }
        };
        _mockContactInfoService.Setup(c => c.GetAddressesAsync(EntityType.Account, 5)).ReturnsAsync(addresses);

        var result = await _service.GetPrimaryBillingAddressAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetPrimaryBillingAddressAsync_ShouldReturnNull_WhenNoBillingAddressesExist()
    {
        var addresses = new List<LinkedAddressDto>
        {
            new() { Id = 1, AddressType = "Shipping", IsPrimary = true }
        };
        _mockContactInfoService.Setup(c => c.GetAddressesAsync(EntityType.Account, 5)).ReturnsAsync(addresses);

        var result = await _service.GetPrimaryBillingAddressAsync(5);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrimaryShippingAddressAsync_ShouldOnlyMatchShippingType()
    {
        var addresses = new List<LinkedAddressDto>
        {
            new() { Id = 1, AddressType = "Billing", IsPrimary = true },
            new() { Id = 2, AddressType = "Shipping", IsPrimary = true }
        };
        _mockContactInfoService.Setup(c => c.GetAddressesAsync(EntityType.Account, 7)).ReturnsAsync(addresses);

        var result = await _service.GetPrimaryShippingAddressAsync(7);

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    [Fact]
    public async Task SetPrimaryBillingAddressAsync_ShouldCallContactInfoServiceWithBillingType()
    {
        _mockContactInfoService
            .Setup(c => c.SetPrimaryAddressAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AddressType>()))
            .Returns(Task.CompletedTask);

        await _service.SetPrimaryBillingAddressAsync(3, 99);

        _mockContactInfoService.Verify(c => c.SetPrimaryAddressAsync(EntityType.Account, 3, 99, AddressType.Billing), Times.Once);
    }

    [Fact]
    public async Task SetPrimaryShippingAddressAsync_ShouldCallContactInfoServiceWithShippingType()
    {
        _mockContactInfoService
            .Setup(c => c.SetPrimaryAddressAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AddressType>()))
            .Returns(Task.CompletedTask);

        await _service.SetPrimaryShippingAddressAsync(3, 99);

        _mockContactInfoService.Verify(c => c.SetPrimaryAddressAsync(EntityType.Account, 3, 99, AddressType.Shipping), Times.Once);
    }

    // ------------------------------------------------------------------
    // Contact-management delegation (AP-037: forwarded to IAccountContactService)
    // ------------------------------------------------------------------

    [Fact]
    public async Task LinkContactToAccountAsync_ShouldDelegateToAccountContactService()
    {
        var dto = new LinkContactToAccountDto { ContactId = 42, Role = AccountContactRole.Primary };
        var expected = new AccountContactDto { Id = 1, AccountId = 5, ContactId = 42 };
        _mockAccountContactService.Setup(s => s.LinkContactToAccountAsync(5, dto)).ReturnsAsync(expected);

        var result = await _service.LinkContactToAccountAsync(5, dto);

        result.Should().BeSameAs(expected);
        _mockAccountContactService.Verify(s => s.LinkContactToAccountAsync(5, dto), Times.Once);
    }

    [Fact]
    public async Task UnlinkContactFromAccountAsync_ShouldDelegateToAccountContactService()
    {
        _mockAccountContactService.Setup(s => s.UnlinkContactFromAccountAsync(5, 42)).ReturnsAsync(true);

        var result = await _service.UnlinkContactFromAccountAsync(5, 42);

        result.Should().BeTrue();
        _mockAccountContactService.Verify(s => s.UnlinkContactFromAccountAsync(5, 42), Times.Once);
    }

    [Fact]
    public async Task UpdateAccountContactAsync_ShouldDelegateToAccountContactService()
    {
        var dto = new UpdateAccountContactDto { Notes = "updated" };
        var expected = new AccountContactDto { Id = 1, AccountId = 5, ContactId = 42 };
        _mockAccountContactService.Setup(s => s.UpdateAccountContactAsync(5, 42, dto)).ReturnsAsync(expected);

        var result = await _service.UpdateAccountContactAsync(5, 42, dto);

        result.Should().BeSameAs(expected);
        _mockAccountContactService.Verify(s => s.UpdateAccountContactAsync(5, 42, dto), Times.Once);
    }

    [Fact]
    public async Task GetAccountContactsAsync_ShouldDelegateToAccountContactService()
    {
        var expected = new List<AccountContactDto> { new() { Id = 1, AccountId = 5 } };
        _mockAccountContactService.Setup(s => s.GetAccountContactsAsync(5)).ReturnsAsync(expected);

        var result = await _service.GetAccountContactsAsync(5);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task SetPrimaryContactAsync_ShouldDelegateToAccountContactService()
    {
        _mockAccountContactService.Setup(s => s.SetPrimaryContactAsync(5, 42)).ReturnsAsync(true);

        var result = await _service.SetPrimaryContactAsync(5, 42);

        result.Should().BeTrue();
        _mockAccountContactService.Verify(s => s.SetPrimaryContactAsync(5, 42), Times.Once);
    }

    [Fact]
    public async Task GetDirectContactsAsync_ShouldDelegateToAccountContactService()
    {
        var expected = new List<object> { new { Id = 1 } };
        _mockAccountContactService.Setup(s => s.GetDirectContactsAsync(5)).ReturnsAsync(expected);

        var result = await _service.GetDirectContactsAsync(5);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AssignContactToAccountAsync_ShouldDelegateToAccountContactService()
    {
        _mockAccountContactService.Setup(s => s.AssignContactToAccountAsync(5, 42)).ReturnsAsync(true);

        var result = await _service.AssignContactToAccountAsync(5, 42);

        result.Should().BeTrue();
        _mockAccountContactService.Verify(s => s.AssignContactToAccountAsync(5, 42), Times.Once);
    }

    [Fact]
    public async Task UnassignContactFromAccountAsync_ShouldDelegateToAccountContactService()
    {
        _mockAccountContactService.Setup(s => s.UnassignContactFromAccountAsync(5, 42)).ReturnsAsync(true);

        var result = await _service.UnassignContactFromAccountAsync(5, 42);

        result.Should().BeTrue();
        _mockAccountContactService.Verify(s => s.UnassignContactFromAccountAsync(5, 42), Times.Once);
    }
}
