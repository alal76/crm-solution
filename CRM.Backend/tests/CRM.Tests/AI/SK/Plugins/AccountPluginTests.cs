// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Plugins;

#nullable enable

/// <summary>
/// Unit tests for <see cref="AccountPlugin"/>.
/// Validates CRUD kernel functions, health score reflection, and error handling.
/// </summary>
public class AccountPluginTests
{
    #region Fields & Setup

    private readonly Mock<IAccountService> _accountServiceMock = new();
    private readonly Mock<ICrmDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<AccountPlugin>> _loggerMock = new();
    private readonly AccountPlugin _plugin;

    public AccountPluginTests()
    {
        _plugin = new AccountPlugin(
            _accountServiceMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void PluginName_ShouldReturnAccount()
    {
        _plugin.PluginName.Should().Be("Account");
    }

    [Fact]
    public void Description_ShouldNotBeEmpty()
    {
        _plugin.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullAccountService_ShouldThrow()
    {
        var act = () => new AccountPlugin(null!, _dbContextMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullDbContext_ShouldThrow()
    {
        var act = () => new AccountPlugin(_accountServiceMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new AccountPlugin(_accountServiceMock.Object, _dbContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetAccountAsync Tests

    [Fact]
    public async Task GetAccountAsync_ExistingId_ShouldReturnSuccessResult()
    {
        // Arrange
        var account = new AccountDto { Id = 1, Company = "Acme Corp", Email = "info@acme.com" };
        _accountServiceMock.Setup(s => s.GetAccountByIdAsync(1))
            .ReturnsAsync(account);

        // Act
        var result = await _plugin.GetAccountAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetAccountAsync_NonExistentId_ShouldReturnErrorResult()
    {
        // Arrange
        _accountServiceMock.Setup(s => s.GetAccountByIdAsync(999))
            .ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _plugin.GetAccountAsync(999);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SearchAccountsAsync Tests

    [Fact]
    public async Task SearchAccountsAsync_ValidQuery_ShouldReturnResults()
    {
        // Arrange
        var accounts = new List<AccountDto>
        {
            new() { Id = 1, Company = "Acme Corp" },
            new() { Id = 2, Company = "Acme Industries" }
        };
        _accountServiceMock.Setup(s => s.SearchAccountsAsync(It.IsAny<string>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _plugin.SearchAccountsAsync("Acme");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    #endregion

    #region GetAccountHealthAsync Tests

    [Fact]
    public async Task GetAccountHealthAsync_ExistingAccount_ShouldReturnHealthData()
    {
        // Arrange
        var account = new AccountDto { Id = 1, Company = "Acme Corp", AccountHealthScore = 85 };
        _accountServiceMock.Setup(s => s.GetAccountByIdAsync(1))
            .ReturnsAsync(account);

        // Act
        var result = await _plugin.GetAccountHealthAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetAccountHealthAsync_NonExistent_ShouldReturnError()
    {
        // Arrange
        _accountServiceMock.Setup(s => s.GetAccountByIdAsync(999))
            .ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _plugin.GetAccountHealthAsync(999);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetRelatedContactsAsync Tests

    [Fact]
    public async Task GetRelatedContactsAsync_ExistingAccount_ShouldReturnContacts()
    {
        // Arrange
        var contacts = new List<AccountContactDto>();
        _accountServiceMock.Setup(s => s.GetAccountContactsAsync(1))
            .ReturnsAsync(contacts);

        // Act
        var result = await _plugin.GetRelatedContactsAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        var doc = JsonDocument.Parse(result);
        doc.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    #endregion

    #region UpdateAccountAsync Tests

    [Fact]
    public async Task UpdateAccountAsync_ValidParams_ShouldReturnSuccess()
    {
        // Arrange
        var account = new AccountDto { Id = 1, Company = "Acme Corp" };
        _accountServiceMock.Setup(s => s.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<UpdateAccountDto>()))
            .ReturnsAsync(account);

        // Act
        var result = await _plugin.UpdateAccountAsync(1, "Company", "New Acme Corp");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpdateAccountAsync_NonExistentAccount_ShouldReturnError()
    {
        // Arrange
        _accountServiceMock.Setup(s => s.UpdateAccountAsync(999, It.IsAny<UpdateAccountDto>()))
            .ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _plugin.UpdateAccountAsync(999, "Company", "New Name");

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GetAccountAsync_ServiceThrows_ShouldReturnErrorResult()
    {
        // Arrange
        _accountServiceMock.Setup(s => s.GetAccountByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var result = await _plugin.GetAccountAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
