// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// TCOV2-D01 — AccountsController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Api.Hubs;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AccountsController (TCOV2-D01).
/// Tests HTTP contract only — not business logic.
/// [Authorize] is on the controller but is not exercised in unit tests (middleware concern).
/// </summary>
public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IContactInfoService> _mockContactInfoService;
    private readonly Mock<ILogger<AccountsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mockAccountService = new Mock<IAccountService>();
        _mockContactInfoService = new Mock<IContactInfoService>();
        _mockLogger = new Mock<ILogger<AccountsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _controller = new AccountsController(
            _mockAccountService.Object,
            _mockContactInfoService.Object,
            _mockLogger.Object,
            _mockNotificationService.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
            }
        };
    }

    private static AccountDto MakeAccountDto(int id = 1) => new()
    {
        Id = id,
        FirstName = "Test",
        LastName = $"Account {id}",
        RowVersion = null
    };

    // ── GetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAccountsExist()
    {
        // Arrange
        var accounts = new List<AccountDto> { MakeAccountDto(1), MakeAccountDto(2) };
        _mockAccountService.Setup(s => s.GetAllAccountsAsync()).ReturnsAsync(accounts);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeEquivalentTo(accounts);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenNoAccounts()
    {
        _mockAccountService.Setup(s => s.GetAllAccountsAsync()).ReturnsAsync(new List<AccountDto>());

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        _mockAccountService.Verify(s => s.GetAllAccountsAsync(), Times.Once);
    }

    // ── GetById ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenAccountExists()
    {
        // Arrange
        var dto = MakeAccountDto(42);
        _mockAccountService.Setup(s => s.GetAccountByIdAsync(42)).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(42);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        _mockAccountService.Setup(s => s.GetAccountByIdAsync(99)).ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _controller.GetById(99);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── GetIndividuals ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetIndividuals_ShouldReturnOk()
    {
        var accounts = new List<AccountDto> { MakeAccountDto(1) };
        _mockAccountService.Setup(s => s.GetIndividualAccountsAsync()).ReturnsAsync(accounts);

        var result = await _controller.GetIndividuals();

        result.Should().BeOfType<OkObjectResult>();
        _mockAccountService.Verify(s => s.GetIndividualAccountsAsync(), Times.Once);
    }

    // ── GetOrganizations ────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrganizations_ShouldReturnOk()
    {
        _mockAccountService.Setup(s => s.GetOrganizationAccountsAsync()).ReturnsAsync(new List<AccountDto>());

        var result = await _controller.GetOrganizations();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Search ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ShouldReturnOk_WithResults()
    {
        var accounts = new List<AccountDto> { MakeAccountDto(1) };
        _mockAccountService.Setup(s => s.SearchAccountsAsync("Acme")).ReturnsAsync(accounts);

        var result = await _controller.Search("Acme");

        result.Should().BeOfType<OkObjectResult>();
    }
}
