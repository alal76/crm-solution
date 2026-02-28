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
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for PortalService.
/// Covers GetMyTickets, CreateTicket, Profile management, CancelTicket and Attachments.
/// PORTAL-038, PORTAL-039 (portal service portion), PORTAL-041, PORTAL-042, PORTAL-043
/// </summary>
public class PortalServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<PortalService>> _mockLogger;
    private readonly PortalService _service;

    private readonly List<PortalUser> _portalUsers;
    private readonly List<ServiceRequest> _serviceRequests;

    public PortalServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<PortalService>>();

        _portalUsers = new List<PortalUser>();
        _serviceRequests = new List<ServiceRequest>();

        SetupMockDbSets();

        _service = new PortalService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupMockDbSets()
    {
        var mockPortalUsers = MockDbSetFactory.CreateMockDbSet(_portalUsers);
        var mockServiceRequests = MockDbSetFactory.CreateMockDbSet(_serviceRequests);

        _mockContext.Setup(c => c.PortalUsers).Returns(mockPortalUsers.Object);
        _mockContext.Setup(c => c.ServiceRequests).Returns(mockServiceRequests.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ── PORTAL-038: GetMyTicketsAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetMyTickets_ShouldReturnOnlyPortalUserTickets()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "user1@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _portalUsers.Add(new PortalUser
        {
            Id = 2,
            Email = "user2@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        _serviceRequests.Add(new ServiceRequest
        {
            Id = 1,
            Subject = "User1 Ticket",
            TicketNumber = "PT-001",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Medium,
            RequesterEmail = "user1@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _serviceRequests.Add(new ServiceRequest
        {
            Id = 2,
            Subject = "User2 Ticket",
            TicketNumber = "PT-002",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Medium,
            RequesterEmail = "user2@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        SetupMockDbSets();

        // Act
        var result = await _service.GetMyTicketsAsync(portalUserId: 1, page: 1, pageSize: 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("User1 Ticket");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetMyTickets_ShouldReturnEmpty_WhenNoTicketsExist()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "notickets@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        // No service requests added
        SetupMockDbSets();

        // Act
        var result = await _service.GetMyTicketsAsync(portalUserId: 1, page: 1, pageSize: 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetMyTickets_ShouldReturnEmpty_WhenPortalUserNotFound()
    {
        // Arrange - no users at all
        SetupMockDbSets();

        // Act
        var result = await _service.GetMyTicketsAsync(portalUserId: 99, page: 1, pageSize: 10);

        // Assert — returns an empty paged result without throwing
        result.Items.Should().BeNullOrEmpty();
        result.Page.Should().Be(1);
    }

    // ── PORTAL-039: CreateTicketAsync ─────────────────────────────────────────

    [Fact]
    public async Task CreateTicket_ShouldCreateServiceRequest_WithCorrectEntityValues()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "creator@portal.com",
            DisplayName = "Test Creator",
            PasswordHash = "hash",
            IsActive = true,
            ContactId = null,
            AccountId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalCreateTicketDto
        {
            Title = "New Bug Report",
            Description = "Something broke",
            Priority = "High"
        };

        // Act
        var result = await _service.CreateTicketAsync(portalUserId: 1, dto: dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Bug Report");
        result.Status.Should().Be("New");
        result.Priority.Should().Be("High");
        result.TicketNumber.Should().StartWith("PT-");
        _serviceRequests.Should().HaveCount(1);
        _serviceRequests.First().Subject.Should().Be("New Bug Report");
        _serviceRequests.First().RequesterEmail.Should().Be("creator@portal.com");
    }

    [Fact]
    public async Task CreateTicket_ShouldLinkToPortalUser_ViaContactId()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "linked@portal.com",
            DisplayName = "Linked User",
            PasswordHash = "hash",
            ContactId = 42,
            AccountId = 7,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        var dto = new PortalCreateTicketDto { Title = "Contact Linked Ticket", Priority = "Medium" };

        // Act
        await _service.CreateTicketAsync(portalUserId: 1, dto: dto);

        // Assert
        var created = _serviceRequests.First();
        created.ContactId.Should().Be(42);
        created.AccountId.Should().Be(7);
        created.RequesterEmail.Should().Be("linked@portal.com");
    }

    [Fact]
    public async Task CreateTicket_ShouldThrow_WhenPortalUserNotFound()
    {
        // Arrange - no users
        SetupMockDbSets();
        var dto = new PortalCreateTicketDto { Title = "Ghost Ticket" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTicketAsync(portalUserId: 999, dto: dto));
    }

    // ── PORTAL-041: Profile management ────────────────────────────────────────

    [Fact]
    public async Task GetProfile_ShouldReturnPortalUserDto()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "profile@portal.com",
            DisplayName = "John Portal",
            PasswordHash = "hash",
            IsActive = true,
            ContactId = null,
            AccountId = null,
            LastLoginAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        // Act
        var result = await _service.GetProfileAsync(portalUserId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("profile@portal.com");
        result.DisplayName.Should().Be("John Portal");
        result.IsActive.Should().BeTrue();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetProfile_ShouldThrow_WhenPortalUserNotFound()
    {
        // Arrange — no users in DB
        SetupMockDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetProfileAsync(portalUserId: 999));
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateDisplayNameAndPhone()
    {
        // Arrange
        var user = new PortalUser
        {
            Id = 1,
            Email = "update@portal.com",
            DisplayName = "OldDisplayName",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        SetupMockDbSets();

        var dto = new UpdatePortalProfileDto { DisplayName = "NewDisplayName" };

        // Act
        var result = await _service.UpdateProfileAsync(portalUserId: 1, dto: dto);

        // Assert
        result.DisplayName.Should().Be("NewDisplayName");
        user.DisplayName.Should().Be("NewDisplayName");
    }

    [Fact]
    public async Task ChangePassword_ShouldVerifyCurrentPassword_BeforeUpdating()
    {
        // Arrange
        const string originalPassword = "OldPassword@1";
        var user = new PortalUser
        {
            Id = 1,
            Email = "changepw@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(originalPassword),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        SetupMockDbSets();

        var dto = new ChangePortalPasswordDto
        {
            CurrentPassword = originalPassword,
            NewPassword = "NewPassword@2",
            ConfirmNewPassword = "NewPassword@2"
        };

        // Act
        await _service.ChangePasswordAsync(portalUserId: 1, dto: dto);

        // Assert — password hash should now verify against the new password
        BCrypt.Net.BCrypt.Verify("NewPassword@2", user.PasswordHash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify(originalPassword, user.PasswordHash).Should().BeFalse();
    }

    [Fact]
    public async Task ChangePassword_ShouldThrow_WhenCurrentPasswordIsWrong()
    {
        // Arrange
        var user = new PortalUser
        {
            Id = 1,
            Email = "wrongpw@portal.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@1"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        SetupMockDbSets();

        var dto = new ChangePortalPasswordDto
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "SomeNewPass@2",
            ConfirmNewPassword = "SomeNewPass@2"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ChangePasswordAsync(portalUserId: 1, dto: dto));
    }

    // ── PORTAL-042: CancelTicketAsync ─────────────────────────────────────────

    [Fact]
    public async Task CancelTicket_ShouldSetStatusToCancelled()
    {
        // Arrange
        var user = new PortalUser
        {
            Id = 1,
            Email = "cancel@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var ticket = new ServiceRequest
        {
            Id = 10,
            Subject = "Open Ticket",
            TicketNumber = "PT-010",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Medium,
            RequesterEmail = "cancel@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        _serviceRequests.Add(ticket);
        SetupMockDbSets();

        // Act
        await _service.CancelTicketAsync(ticketId: 10, portalUserId: 1);

        // Assert
        ticket.Status.Should().Be(ServiceRequestStatus.Cancelled);
    }

    [Fact]
    public async Task CancelTicket_ShouldThrow_WhenTicketAlreadyClosed()
    {
        // Arrange
        var user = new PortalUser
        {
            Id = 1,
            Email = "cancel2@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var ticket = new ServiceRequest
        {
            Id = 11,
            Subject = "Closed Ticket",
            TicketNumber = "PT-011",
            Status = ServiceRequestStatus.Closed,
            Priority = ServiceRequestPriority.Low,
            RequesterEmail = "cancel2@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        _serviceRequests.Add(ticket);
        SetupMockDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CancelTicketAsync(ticketId: 11, portalUserId: 1));
    }

    [Fact]
    public async Task CancelTicket_ShouldThrow_WhenUserDoesNotOwnTicket()
    {
        // Arrange — ticket belongs to different email
        var user = new PortalUser
        {
            Id = 1,
            Email = "cancel3@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var ticket = new ServiceRequest
        {
            Id = 12,
            Subject = "Someone Else Ticket",
            TicketNumber = "PT-012",
            Status = ServiceRequestStatus.Open,
            Priority = ServiceRequestPriority.Low,
            RequesterEmail = "other-person@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        _serviceRequests.Add(ticket);
        SetupMockDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CancelTicketAsync(ticketId: 12, portalUserId: 1));
    }

    // ── PORTAL-043: Attachments ────────────────────────────────────────────────

    [Fact]
    public async Task UploadAttachment_ShouldThrow_WhenFileTooLarge()
    {
        // Arrange
        _portalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "attach@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        SetupMockDbSets();

        using var smallStream = new System.IO.MemoryStream(new byte[1]);
        // 10 MB + 1 byte exceeds the limit
        const long overSizeBytes = 10_485_760 + 1;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UploadAttachmentAsync(
                ticketId: 1,
                portalUserId: 1,
                fileName: "huge-file.zip",
                contentType: "application/zip",
                fileStream: smallStream,
                fileSize: overSizeBytes));
    }

    [Fact]
    public async Task GetAttachments_ShouldReturnEmpty_WhenTicketDoesNotBelongToUser()
    {
        // Arrange — ticket owned by different user
        var user = new PortalUser
        {
            Id = 1,
            Email = "attach2@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var ticket = new ServiceRequest
        {
            Id = 20,
            Subject = "Foreign Ticket",
            TicketNumber = "PT-020",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Low,
            RequesterEmail = "not-this-user@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        _serviceRequests.Add(ticket);
        SetupMockDbSets();

        // Act
        var result = await _service.GetAttachmentsAsync(ticketId: 20, portalUserId: 1);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAttachments_ShouldReturnOnlyTicketAttachments_WhenDirectoryAbsent()
    {
        // Arrange — ticket is owned by user but attachment directory does not exist
        var user = new PortalUser
        {
            Id = 1,
            Email = "attach3@portal.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        // Use a ticket ID that definitely has no directory under wwwroot/portal-attachments
        var ticket = new ServiceRequest
        {
            Id = 999_999_999,
            Subject = "No-Dir Ticket",
            TicketNumber = "PT-NNN",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Low,
            RequesterEmail = "attach3@portal.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalUsers.Add(user);
        _serviceRequests.Add(ticket);
        SetupMockDbSets();

        // Act
        var result = await _service.GetAttachmentsAsync(ticketId: 999_999_999, portalUserId: 1);

        // Assert — no directory → empty list (no exception)
        result.Should().BeEmpty();
    }
}
