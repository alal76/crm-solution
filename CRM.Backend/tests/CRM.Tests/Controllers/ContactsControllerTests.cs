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

    // ── UpdateContact ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateContact_ShouldReturnOk_WhenContactExists()
    {
        // Arrange
        var request = new CRM.Core.Dtos.UpdateContactRequest { FirstName = "Updated" };
        var returned = MakeContactDto(3);
        _mockContactsService
            .Setup(s => s.UpdateAsync(3, It.IsAny<CRM.Core.Dtos.UpdateContactRequest>(), It.IsAny<string>()))
            .ReturnsAsync(returned);
        _mockNotificationService
            .Setup(n => n.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateContact(3, request);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(returned);
    }

    [Fact]
    public async Task UpdateContact_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange
        var request = new CRM.Core.Dtos.UpdateContactRequest { FirstName = "Updated" };
        _mockContactsService
            .Setup(s => s.UpdateAsync(999, It.IsAny<CRM.Core.Dtos.UpdateContactRequest>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Contact with ID 999 not found"));

        // Act
        var result = await _controller.UpdateContact(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── DeleteContact ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteContact_ShouldReturnOk_WhenContactExists()
    {
        // Arrange
        _mockContactsService.Setup(s => s.DeleteAsync(4)).ReturnsAsync(true);
        _mockNotificationService
            .Setup(n => n.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteContact(4);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteContact_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange
        _mockContactsService.Setup(s => s.DeleteAsync(999))
            .ThrowsAsync(new InvalidOperationException("Contact with ID 999 not found"));

        // Act
        var result = await _controller.DeleteContact(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── AddSocialMediaLink ───────────────────────────────────────────────────

    [Fact]
    public async Task AddSocialMediaLink_ShouldReturnCreated_WhenContactExists()
    {
        // Arrange
        var request = new CRM.Core.Dtos.AddSocialMediaRequest { Platform = "LinkedIn", Url = "https://linkedin.com/in/alice" };
        var returned = new SocialMediaLinkDto { Id = 1, Platform = "LinkedIn", Url = request.Url };
        _mockContactsService
            .Setup(s => s.AddSocialMediaLinkAsync(1, It.IsAny<CRM.Core.Dtos.AddSocialMediaRequest>()))
            .ReturnsAsync(returned);

        // Act
        var result = await _controller.AddSocialMediaLink(1, request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task AddSocialMediaLink_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange
        var request = new CRM.Core.Dtos.AddSocialMediaRequest { Platform = "LinkedIn", Url = "https://linkedin.com/in/alice" };
        _mockContactsService
            .Setup(s => s.AddSocialMediaLinkAsync(999, It.IsAny<CRM.Core.Dtos.AddSocialMediaRequest>()))
            .ThrowsAsync(new InvalidOperationException("Contact with ID 999 not found"));

        // Act
        var result = await _controller.AddSocialMediaLink(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── RemoveSocialMediaLink ────────────────────────────────────────────────

    [Fact]
    public async Task RemoveSocialMediaLink_ShouldReturnOk_WhenLinkExists()
    {
        // Arrange
        _mockContactsService.Setup(s => s.RemoveSocialMediaLinkAsync(10)).ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveSocialMediaLink(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveSocialMediaLink_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange
        _mockContactsService.Setup(s => s.RemoveSocialMediaLinkAsync(999))
            .ThrowsAsync(new InvalidOperationException("Social media link with ID 999 not found"));

        // Act
        var result = await _controller.RemoveSocialMediaLink(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── AssignToAccount / UnassignFromAccount ───────────────────────────────

    [Fact]
    public async Task AssignToAccount_ShouldReturnOk_WhenContactExists()
    {
        // Arrange
        _mockContactsService.Setup(s => s.AssignToAccountAsync(1, 2)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignToAccount(1, 2);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockContactsService.Verify(s => s.AssignToAccountAsync(1, 2), Times.Once);
    }

    [Fact]
    public async Task AssignToAccount_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange
        _mockContactsService.Setup(s => s.AssignToAccountAsync(999, 2))
            .ThrowsAsync(new InvalidOperationException("Contact with ID 999 not found"));

        // Act
        var result = await _controller.AssignToAccount(999, 2);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UnassignFromAccount_ShouldReturnOk_WhenContactExists()
    {
        // Arrange
        _mockContactsService.Setup(s => s.UnassignFromAccountAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UnassignFromAccount(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockContactsService.Verify(s => s.UnassignFromAccountAsync(1), Times.Once);
    }

    [Fact]
    public async Task UnassignFromAccount_ShouldReturnNotFound_WhenServiceThrows()
    {
        // Arrange
        _mockContactsService.Setup(s => s.UnassignFromAccountAsync(999))
            .ThrowsAsync(new InvalidOperationException("Contact with ID 999 not found"));

        // Act
        var result = await _controller.UnassignFromAccount(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
