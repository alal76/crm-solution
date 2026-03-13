// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
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
        _mockLogger = new Mock<ILogger<UsersController>>();

        _controller = new UsersController(
            _mockUserRepository.Object,
            _mockProfileRepository.Object,
            _mockDepartmentRepository.Object,
            _mockContactsService.Object,
            _mockUserService.Object,
            _mockDbContext.Object,
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
}
