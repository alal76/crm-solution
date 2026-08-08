// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D04 — UsersController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for UsersController (TCOV2-D04).
/// Tests HTTP contract only.
/// [Authorize] attribute present but not exercised here.
/// Note: UsersController uses IRepository&lt;User&gt;, IUserService, IContactsService,
/// ICrmDbContext — all mocked via Moq.
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<UserProfile>> _mockProfileRepository;
    private readonly Mock<IRepository<Department>> _mockDepartmentRepository;
    private readonly Mock<IContactsService> _mockContactsService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IEmailDigestService> _mockEmailDigestService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockProfileRepository = new Mock<IRepository<UserProfile>>();
        _mockDepartmentRepository = new Mock<IRepository<Department>>();
        _mockContactsService = new Mock<IContactsService>();
        _mockUserService = new Mock<IUserService>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockEmailDigestService = new Mock<IEmailDigestService>();
        _mockLogger = new Mock<ILogger<UsersController>>();

        _controller = new UsersController(
            _mockUserRepository.Object,
            _mockProfileRepository.Object,
            _mockDepartmentRepository.Object,
            _mockContactsService.Object,
            _mockUserService.Object,
            _mockDbContext.Object,
            _mockEmailDigestService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
            }
        };
    }

    private static UserDto MakeUserDto(int id = 1) => new()
    {
        Id = id,
        Email = $"user{id}@example.com",
        FirstName = "Test",
        LastName = "User"
    };

    private static User MakeUser(int id = 1) => new()
    {
        Id = id,
        Email = $"user{id}@example.com",
        FirstName = "Test",
        LastName = "User",
        Username = $"user{id}",
        PasswordHash = "hash",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // ── CreateUser ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "", FirstName = "A", LastName = "B", Password = "pass" };

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenFirstNameIsEmpty()
    {
        var request = new CreateUserRequest { Email = "a@b.com", FirstName = "", LastName = "B", Password = "pass" };

        var result = await _controller.CreateUser(request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenLastNameIsEmpty()
    {
        var request = new CreateUserRequest { Email = "a@b.com", FirstName = "A", LastName = "", Password = "pass" };

        var result = await _controller.CreateUser(request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "Secure@123",
            RoleId = 2
        };
        var returned = MakeUserDto(10);
        _mockUserService.Setup(s => s.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .ReturnsAsync(returned);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = (CreatedAtActionResult)result.Result!;
        created.StatusCode.Should().Be(201);
    }

    // ── GetUsers ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WithUserList()
    {
        // Arrange — use InMemory CrmDbContext so Set<User>() returns a real queryable
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"UsersTest_{Guid.NewGuid()}")
            .Options;
        var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
        await using var inMemCtx = new CrmDbContext(opts, cfg);

        inMemCtx.Users.AddRange(MakeUser(1), MakeUser(2));
        await inMemCtx.SaveChangesAsync();

        _mockDbContext.Setup(d => d.Set<User>()).Returns(inMemCtx.Set<User>());
        _mockContactsService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ContactDto>());

        // Act
        var result = await _controller.GetUsers();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetUserById — DTO field mapping (REM-FGAP-008) ─────────────────────────

    [Fact]
    public async Task GetUserById_ShouldMapPasswordNeverSetAndCommissionPlanId_FromEntity()
    {
        // Arrange
        var user = MakeUser(5);
        user.PasswordNeverSet = true;
        user.CommissionPlanId = 42;

        _mockUserRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(5);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var dto = (UserDto)((OkObjectResult)result.Result!).Value!;
        dto.PasswordNeverSet.Should().BeTrue();
        dto.CommissionPlanId.Should().Be(42);
    }

    [Fact]
    public async Task GetUserById_ShouldMapPasswordNeverSetFalseAndNullCommissionPlanId_WhenUnset()
    {
        // Arrange
        var user = MakeUser(6);
        user.PasswordNeverSet = false;
        user.CommissionPlanId = null;

        _mockUserRepository.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(6);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var dto = (UserDto)((OkObjectResult)result.Result!).Value!;
        dto.PasswordNeverSet.Should().BeFalse();
        dto.CommissionPlanId.Should().BeNull();
    }

    // ── Email Digest (REV-FE-002) ───────────────────────────────────────────

    [Fact]
    public async Task GetMyEmailDigest_ShouldReturnConfig_FromService()
    {
        var expected = new EmailDigestConfigDto { Enabled = true, Frequency = "weekly", DayOfWeek = 1, TimeOfDay = "09:00", Timezone = "UTC" };
        _mockEmailDigestService
            .Setup(s => s.GetConfigAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyEmailDigest(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        var dto = (EmailDigestConfigDto)((OkObjectResult)result.Result!).Value!;
        dto.Frequency.Should().Be("weekly");
        dto.DayOfWeek.Should().Be(1);
    }

    [Fact]
    public async Task GetMyEmailDigest_ShouldReturnUnauthorized_WhenNoUserClaim()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        var result = await _controller.GetMyEmailDigest(CancellationToken.None);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateMyEmailDigest_ShouldSaveViaService_AndReturnResult()
    {
        var request = new EmailDigestConfigDto { Enabled = true, Frequency = "daily", TimeOfDay = "08:00", Timezone = "UTC" };
        var saved = new EmailDigestConfigDto { Enabled = true, Frequency = "daily", TimeOfDay = "08:00", Timezone = "UTC" };
        _mockEmailDigestService
            .Setup(s => s.UpdateConfigAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);

        var result = await _controller.UpdateMyEmailDigest(request, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        _mockEmailDigestService.Verify(s => s.UpdateConfigAsync(1, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMyEmailDigestPreview_ShouldReturnOk_WhenSendSucceeds()
    {
        var user = MakeUser(1);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockEmailDigestService
            .Setup(s => s.GetConfigAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailDigestConfigDto { Enabled = true, Frequency = "daily", TimeOfDay = "08:00", Timezone = "UTC" });
        _mockEmailDigestService
            .Setup(s => s.SendDigestAsync(user, It.IsAny<EmailDigestConfig>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SendMyEmailDigestPreview(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SendMyEmailDigestPreview_ShouldReturnBadGateway_WhenSendFails()
    {
        var user = MakeUser(1);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockEmailDigestService
            .Setup(s => s.GetConfigAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailDigestConfigDto { Enabled = true, Frequency = "daily", TimeOfDay = "08:00", Timezone = "UTC" });
        _mockEmailDigestService
            .Setup(s => s.SendDigestAsync(user, It.IsAny<EmailDigestConfig>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.SendMyEmailDigestPreview(CancellationToken.None);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status502BadGateway);
    }

    [Fact]
    public async Task SendMyEmailDigestPreview_ShouldReturnNotFound_WhenUserMissing()
    {
        _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        var result = await _controller.SendMyEmailDigestPreview(CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
