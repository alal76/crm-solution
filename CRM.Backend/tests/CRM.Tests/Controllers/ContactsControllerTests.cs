// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D02 — ContactsController unit tests
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
/// Unit tests for ContactsController (TCOV2-D02).
/// Tests HTTP contract only — not business logic.
/// [Authorize] attribute present; not exercised in unit tests (middleware concern).
/// </summary>
public class ContactsControllerTests
{
    private readonly Mock<IContactsService> _mockContactsService;
    private readonly Mock<ILogger<ContactsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly ContactsController _controller;

    public ContactsControllerTests()
    {
        _mockContactsService = new Mock<IContactsService>();
        _mockLogger = new Mock<ILogger<ContactsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _controller = new ContactsController(
            _mockContactsService.Object,
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

    private static ContactDto MakeContactDto(int id = 1) => new()
    {
        Id = id,
        FirstName = "Alice",
        LastName = $"User{id}",
        EmailPrimary = $"alice{id}@example.com"
    };

    // ── GetAllContacts ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllContacts_ShouldReturnOk_WithList()
    {
        // Arrange
        var contacts = new List<ContactDto> { MakeContactDto(1), MakeContactDto(2) };
        _mockContactsService.Setup(s => s.GetAllAsync()).ReturnsAsync(contacts);

        // Act
        var result = await _controller.GetAllContacts();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(contacts);
    }

    [Fact]
    public async Task GetAllContacts_CallsServiceOnce()
    {
        _mockContactsService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ContactDto>());

        await _controller.GetAllContacts();

        _mockContactsService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    // ── GetContactById ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactById_ShouldReturnOk_WhenContactExists()
    {
        // Arrange
        var dto = MakeContactDto(5);
        _mockContactsService.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetContactById(5);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var contact = ok.Value.Should().BeOfType<ContactDto>().Subject;
        contact.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetContactById_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange — ContactsService throws InvalidOperationException for not-found
        _mockContactsService.Setup(s => s.GetByIdAsync(999))
            .ThrowsAsync(new InvalidOperationException("Contact not found"));

        // Act
        var result = await _controller.GetContactById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── GetContactsByType ────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactsByType_ShouldReturnOk()
    {
        // Arrange
        var contacts = new List<ContactDto> { MakeContactDto(1) };
        _mockContactsService.Setup(s => s.GetByTypeAsync("Employee")).ReturnsAsync(contacts);

        // Act
        var result = await _controller.GetContactsByType("Employee");

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockContactsService.Verify(s => s.GetByTypeAsync("Employee"), Times.Once);
    }

    // ── CreateContact ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateContact_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CRM.Core.Dtos.CreateContactRequest
        {
            FirstName = "Bob",
            LastName = "Builder",
            EmailPrimary = "bob@example.com"
        };
        var returned = new ContactDto { Id = 10, FirstName = "Bob", LastName = "Builder" };

        _mockContactsService
            .Setup(s => s.CreateAsync(It.IsAny<CRM.Core.Dtos.CreateContactRequest>(), It.IsAny<string>()))
            .ReturnsAsync(returned);
        _mockNotificationService
            .Setup(n => n.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateContact(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = (CreatedAtActionResult)result.Result!;
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateContact_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
    {
        // Arrange
        var request = new CRM.Core.Dtos.CreateContactRequest { FirstName = string.Empty };
        _mockContactsService
            .Setup(s => s.CreateAsync(It.IsAny<CRM.Core.Dtos.CreateContactRequest>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("First name required"));

        // Act
        var result = await _controller.CreateContact(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
