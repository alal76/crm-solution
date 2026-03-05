// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SK Plugin unit tests — AccountPlugin
// MANDATORY TEST RULE: All method signatures verified against actual source before writing.
// Source files read:
//   AccountPlugin.cs — KernelFunctions: GetAccount, SearchAccounts, GetAccountHealth,
//                      GetRelatedContacts, UpdateAccount, AddAccountNote
//   IAccountService.cs — signatures confirmed
//   AccountDto.cs — AccountDto, UpdateAccountDto fields confirmed (Company: string?, Website: string?)
//   CrmPluginBase.cs — SuccessResult({error:false,data:...}), ErrorResult({error:true,...})

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="AccountPlugin"/>.
/// KernelFunctions tested: GetAccount, SearchAccounts, GetAccountHealth,
///   GetRelatedContacts, UpdateAccount, AddAccountNote
/// </summary>
public class AccountPluginTests
{
    private readonly Mock<IAccountService> _accountService = new(MockBehavior.Loose);
    private readonly Mock<ICrmDbContext> _context = new(MockBehavior.Loose);
    private readonly Mock<ILogger<AccountPlugin>> _logger = new();
    private readonly AccountPlugin _sut;

    public AccountPluginTests()
    {
        _sut = new AccountPlugin(_accountService.Object, _context.Object, _logger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Property / Constructor tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PluginName_ShouldBe_Account()
    {
        _sut.PluginName.Should().Be("Account");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAccountServiceIsNull()
    {
        var act = () => new AccountPlugin(null!, _context.Object, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("accountService");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContextIsNull()
    {
        var act = () => new AccountPlugin(_accountService.Object, null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new AccountPlugin(_accountService.Object, _context.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAccountAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccountAsync_ShouldReturnSuccessJson_WhenAccountExists()
    {
        var accountDto = new AccountDto
        {
            Id = 1,
            Company = "Acme Corp",
            Email = "info@acme.com",
            Phone = "555-1234"
        };
        _accountService.Setup(s => s.GetAccountByIdAsync(1)).ReturnsAsync(accountDto);

        var result = await _sut.GetAccountAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnErrorJson_WhenAccountNotFound()
    {
        _accountService.Setup(s => s.GetAccountByIdAsync(99)).ReturnsAsync((AccountDto?)null);

        var result = await _sut.GetAccountAsync(99);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _accountService.Setup(s => s.GetAccountByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB timeout"));

        var result = await _sut.GetAccountAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchAccountsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAccountsAsync_ShouldReturnSuccessJson_WithCountAndAccounts()
    {
        var accounts = new List<AccountDto>
        {
            new() { Id = 1, Company = "Acme Corp", Email = "a@acme.com" },
            new() { Id = 2, Company = "Beta Ltd",  Email = "b@beta.com" }
        };
        _accountService.Setup(s => s.SearchAccountsAsync("acme")).ReturnsAsync(accounts);

        var result = await _sut.SearchAccountsAsync("acme");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task SearchAccountsAsync_ShouldRespectMaxResults()
    {
        var accounts = Enumerable.Range(1, 15)
            .Select(i => new AccountDto { Id = i, Company = $"Company{i}" })
            .ToList();
        _accountService.Setup(s => s.SearchAccountsAsync(It.IsAny<string>())).ReturnsAsync(accounts);

        var result = await _sut.SearchAccountsAsync("company", maxResults: 3);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task SearchAccountsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _accountService.Setup(s => s.SearchAccountsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Search error"));

        var result = await _sut.SearchAccountsAsync("test");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAccountHealthAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccountHealthAsync_ShouldReturnSuccessJson_WhenAccountExists()
    {
        var accountDto = new AccountDto { Id = 5, Company = "Health Co", Email = "h@co.com" };
        _accountService.Setup(s => s.GetAccountByIdAsync(5)).ReturnsAsync(accountDto);

        var result = await _sut.GetAccountHealthAsync(5);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("accountId").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task GetAccountHealthAsync_ShouldReturnErrorJson_WhenAccountNotFound()
    {
        _accountService.Setup(s => s.GetAccountByIdAsync(77)).ReturnsAsync((AccountDto?)null);

        var result = await _sut.GetAccountHealthAsync(77);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAccountHealthAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _accountService.Setup(s => s.GetAccountByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Health check error"));

        var result = await _sut.GetAccountHealthAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetRelatedContactsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRelatedContactsAsync_ShouldReturnSuccessJson_WithContactCount()
    {
        var contacts = new List<AccountContactDto>
        {
            new() { ContactId = 10, AccountId = 2 },
            new() { ContactId = 11, AccountId = 2 }
        };
        _accountService.Setup(s => s.GetAccountContactsAsync(2)).ReturnsAsync(contacts);

        var result = await _sut.GetRelatedContactsAsync(2);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("accountId").GetInt32().Should().Be(2);
        data.GetProperty("count").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task GetRelatedContactsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _accountService.Setup(s => s.GetAccountContactsAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Contacts load error"));

        var result = await _sut.GetRelatedContactsAsync(3);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateAccountAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnSuccessJson_WhenFieldIsValid()
    {
        // UpdateAccountDto has 'Company' (string?) — valid field for reflection
        var updatedDto = new AccountDto { Id = 1, Company = "NewName Corp" };
        _accountService
            .Setup(s => s.UpdateAccountAsync(1, It.IsAny<UpdateAccountDto>()))
            .ReturnsAsync(updatedDto);

        var result = await _sut.UpdateAccountAsync(1, "Company", "NewName Corp");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("updated").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnErrorJson_WhenFieldNameIsUnknown()
    {
        var result = await _sut.UpdateAccountAsync(1, "NonExistentField", "value");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("Unknown field");
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnErrorJson_WhenAccountNotFound()
    {
        _accountService
            .Setup(s => s.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<UpdateAccountDto>()))
            .ReturnsAsync((AccountDto?)null);

        var result = await _sut.UpdateAccountAsync(999, "Company", "Orphan Corp");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _accountService
            .Setup(s => s.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<UpdateAccountDto>()))
            .ThrowsAsync(new Exception("Update failed"));

        var result = await _sut.UpdateAccountAsync(1, "Company", "Fail Corp");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AddAccountNoteAsync — uses _context.Notes + SaveChanges
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAccountNoteAsync_ShouldReturnSuccessJson_WhenAccountExists()
    {
        var accountDto = new AccountDto { Id = 7, Company = "Note Corp", Email = "n@corp.com" };
        _accountService.Setup(s => s.GetAccountByIdAsync(7)).ReturnsAsync(accountDto);

        var mockNotes = MockDbSetFactory.CreateMockDbSet(new List<Note>());
        _context.Setup(c => c.Notes).Returns(mockNotes.Object);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.AddAccountNoteAsync(7, "Important account note.");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        result.Should().Contain("Important account note");
    }

    [Fact]
    public async Task AddAccountNoteAsync_ShouldReturnErrorJson_WhenAccountNotFound()
    {
        _accountService.Setup(s => s.GetAccountByIdAsync(66)).ReturnsAsync((AccountDto?)null);

        var result = await _sut.AddAccountNoteAsync(66, "Note for missing account");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task AddAccountNoteAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _accountService.Setup(s => s.GetAccountByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Note creation failed"));

        var result = await _sut.AddAccountNoteAsync(1, "Failing note");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }
}
