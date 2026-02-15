using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class PreferencesServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<PreferencesService>> _mockLogger;
    private readonly PreferencesService _service;

    public PreferencesServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<PreferencesService>>();
        _service = new PreferencesService(_mockContext.Object, _cache, _mockLogger.Object);
    }

    private void SetupSaveChanges()
    {
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupAccounts(List<Account> accounts)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(accounts);
        _mockContext.Setup(c => c.Accounts).Returns(mockSet.Object);
    }

    private void SetupContacts(List<Contact> contacts)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(contacts);
        _mockContext.Setup(c => c.Contacts).Returns(mockSet.Object);
    }

    private void SetupPreferences(List<Preferences> preferences)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(preferences);
        _mockContext.Setup(c => c.Preferences).Returns(mockSet.Object);
    }

    [Fact]
    public async Task GetEffectivePreferencesAsync_ShouldReturnAccountDefaults_WhenUseCustomPreferencesIsFalse()
    {
        var prefs = new Preferences { Id = 1, OptInEmail = false, OptInSms = true, CreatedAt = DateTime.UtcNow };
        var account = new Account { Id = 10, PreferencesId = 1, Preferences = prefs, IsDeleted = false };
        var contact = new Contact { Id = 20, AccountId = 10, UseCustomPreferences = false };

        SetupPreferences(new List<Preferences> { prefs });
        SetupAccounts(new List<Account> { account });
        SetupContacts(new List<Contact> { contact });

        var result = await _service.GetEffectivePreferencesAsync(20, CancellationToken.None);

        result.OptInEmail.Should().BeFalse();
        result.OptInSms.Should().BeTrue();
    }

    [Fact]
    public async Task GetEffectivePreferencesAsync_ShouldReturnContactOverrides_WhenUseCustomPreferencesIsTrue()
    {
        var contactPrefs = new Preferences { Id = 2, OptInEmail = false, OptInSms = true, CreatedAt = DateTime.UtcNow };
        var contact = new Contact { Id = 21, AccountId = 10, UseCustomPreferences = true, PreferencesId = 2, Preferences = contactPrefs };

        SetupPreferences(new List<Preferences> { contactPrefs });
        SetupAccounts(new List<Account>());
        SetupContacts(new List<Contact> { contact });

        var result = await _service.GetEffectivePreferencesAsync(21, CancellationToken.None);

        result.OptInEmail.Should().BeFalse();
        result.OptInSms.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateContactPreferencesAsync_ShouldCreatePreferencesAndSetUseCustom()
    {
        var contact = new Contact { Id = 30, UseCustomPreferences = false };
        var preferences = new List<Preferences>();

        SetupPreferences(preferences);
        SetupAccounts(new List<Account>());
        SetupContacts(new List<Contact> { contact });
        SetupSaveChanges();

        var dto = new PreferencesDto
        {
            OptInEmail = false,
            OptInSms = true,
            DoNotEmailDate = DateTime.UtcNow.AddDays(1)
        };

        var result = await _service.UpdateContactPreferencesAsync(30, dto, CancellationToken.None);

        contact.UseCustomPreferences.Should().BeTrue();
        contact.Preferences.Should().NotBeNull();
        result.OptInEmail.Should().BeFalse();
        result.OptInSms.Should().BeTrue();
        preferences.Count.Should().Be(1);
    }

    [Fact]
    public async Task ResetContactToAccountAsync_ShouldClearCustomPreferencesAndSoftDeleteOverrides()
    {
        var contactPrefs = new Preferences { Id = 3, OptInEmail = false, CreatedAt = DateTime.UtcNow };
        var contact = new Contact
        {
            Id = 40,
            UseCustomPreferences = true,
            PreferencesId = 3,
            Preferences = contactPrefs
        };

        SetupPreferences(new List<Preferences> { contactPrefs });
        SetupAccounts(new List<Account>());
        SetupContacts(new List<Contact> { contact });
        SetupSaveChanges();

        var result = await _service.ResetContactToAccountAsync(40, CancellationToken.None);

        result.UseCustomPreferences.Should().BeFalse();
        result.PreferencesId.Should().BeNull();
        contactPrefs.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task BulkSetDefaultsAsync_ShouldUpdateAccountDefaultsAndResetContacts()
    {
        var accountPrefs = new Preferences { Id = 4, OptInEmail = true, CreatedAt = DateTime.UtcNow };
        var account = new Account { Id = 50, PreferencesId = 4, Preferences = accountPrefs, IsDeleted = false };
        var contactA = new Contact { Id = 51, AccountId = 50, UseCustomPreferences = false, PreferencesId = 7 };
        var contactB = new Contact { Id = 52, AccountId = 50, UseCustomPreferences = true, PreferencesId = 8 };

        SetupPreferences(new List<Preferences> { accountPrefs });
        SetupAccounts(new List<Account> { account });
        SetupContacts(new List<Contact> { contactA, contactB });
        SetupSaveChanges();

        var dto = new PreferencesDto { OptInEmail = false, OptInSms = true };
        var count = await _service.BulkSetDefaultsAsync(50, dto, CancellationToken.None);

        count.Should().Be(1);
        contactA.UseCustomPreferences.Should().BeFalse();
        contactA.PreferencesId.Should().BeNull();
        contactB.UseCustomPreferences.Should().BeTrue();
        contactB.PreferencesId.Should().Be(8);
    }
}
