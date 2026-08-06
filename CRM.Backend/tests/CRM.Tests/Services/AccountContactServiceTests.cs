// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for AccountContactService (TCOV Wave-A).</summary>
public class AccountContactServiceTests
{
    private readonly Mock<IRepository<AccountContact>> _mockAccountContactRepo;
    private readonly Mock<IRepository<Account>> _mockAccountRepo;
    private readonly Mock<IContactsService> _mockContactsService;
    private readonly Mock<ILogger<AccountContactService>> _mockLogger;
    private readonly AccountContactService _service;

    public AccountContactServiceTests()
    {
        _mockAccountContactRepo = new Mock<IRepository<AccountContact>>();
        _mockAccountRepo = new Mock<IRepository<Account>>();
        _mockContactsService = new Mock<IContactsService>();
        _mockLogger = new Mock<ILogger<AccountContactService>>();

        _service = new AccountContactService(
            _mockAccountContactRepo.Object,
            _mockAccountRepo.Object,
            _mockContactsService.Object,
            _mockLogger.Object);
    }

    // ------------------------------------------------------------------
    // GetAccountContactsAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAccountContactsAsync_ShouldReturnEmpty_WhenNoLinksExist()
    {
        _mockAccountContactRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<AccountContact, bool>>()))
            .ReturnsAsync(new List<AccountContact>());

        var result = await _service.GetAccountContactsAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccountContactsAsync_ShouldReturnLinks_WhenLinksExist()
    {
        var links = new List<AccountContact>
        {
            new() { Id = 1, AccountId = 10, ContactId = 20, IsDeleted = false }
        };
        _mockAccountContactRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<AccountContact, bool>>()))
            .ReturnsAsync(links);

        _mockContactsService
            .Setup(c => c.GetByIdAsync(20))
            .ReturnsAsync(new ContactDto { Id = 20, FirstName = "Eve", LastName = "Adams" });

        var result = await _service.GetAccountContactsAsync(10);

        result.Should().HaveCount(1);
    }

    // ------------------------------------------------------------------
    // LinkContactToAccountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task LinkContactToAccountAsync_ShouldReturnNull_WhenAccountNotFound()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Account?)null);

        var result = await _service.LinkContactToAccountAsync(99, new LinkContactToAccountDto { ContactId = 5 });

        result.Should().BeNull();
    }

    [Fact]
    public async Task LinkContactToAccountAsync_ShouldReturnNull_WhenAccountIsDeleted()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Account { Id = 10, IsDeleted = true });

        var result = await _service.LinkContactToAccountAsync(10, new LinkContactToAccountDto { ContactId = 5 });

        result.Should().BeNull();
    }

    [Fact]
    public async Task LinkContactToAccountAsync_ShouldReturnNull_WhenContactNotFound()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Account { Id = 10, IsDeleted = false });

        // Mock IContactsService.GetByIdAsync returning null (treating as not-found)
        _mockContactsService.Setup(c => c.GetByIdAsync(5)).ReturnsAsync((ContactDto)null!);

        var result = await _service.LinkContactToAccountAsync(10, new LinkContactToAccountDto { ContactId = 5 });

        result.Should().BeNull();
    }

    [Fact]
    public async Task LinkContactToAccountAsync_ShouldReturnNull_WhenLinkAlreadyExists()
    {
        _mockAccountRepo.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Account { Id = 10, IsDeleted = false });

        _mockContactsService.Setup(c => c.GetByIdAsync(5))
            .ReturnsAsync(new ContactDto { Id = 5, FirstName = "Tom", LastName = "Black" });

        // Existing link found — should block re-linking
        _mockAccountContactRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<AccountContact, bool>>()))
            .ReturnsAsync(new List<AccountContact> { new() { AccountId = 10, ContactId = 5 } });

        var result = await _service.LinkContactToAccountAsync(10, new LinkContactToAccountDto { ContactId = 5 });

        result.Should().BeNull();
    }

    // ------------------------------------------------------------------
    // UnlinkContactFromAccountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task UnlinkContactFromAccountAsync_ShouldReturnFalse_WhenLinkNotFound()
    {
        _mockAccountContactRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<AccountContact, bool>>()))
            .ReturnsAsync(new List<AccountContact>());

        var result = await _service.UnlinkContactFromAccountAsync(10, 5);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnlinkContactFromAccountAsync_ShouldReturnTrue_WhenLinkExists()
    {
        var link = new AccountContact { Id = 1, AccountId = 10, ContactId = 5, IsDeleted = false };
        _mockAccountContactRepo
            .Setup(r => r.FindAsync(It.IsAny<Func<AccountContact, bool>>()))
            .ReturnsAsync(new List<AccountContact> { link });

        _mockAccountContactRepo.Setup(r => r.UpdateAsync(link)).Returns(Task.CompletedTask);
        _mockAccountContactRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Account load for clearing PrimaryContactId
        _mockAccountRepo.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Account { Id = 10, PrimaryContactId = null });

        var result = await _service.UnlinkContactFromAccountAsync(10, 5);

        result.Should().BeTrue();
        link.IsDeleted.Should().BeTrue();
    }
}
