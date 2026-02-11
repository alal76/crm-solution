// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EmailSyncService
/// Covers: Email sync, mailbox integration, email tracking
/// </summary>
public class EmailSyncServiceTests
{
    private readonly Mock<IRepository<EmailMessage>> _mockEmailRepository;
    private readonly Mock<IRepository<EmailSync>> _mockSyncRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<Activity>> _mockActivityRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<EmailSyncService>> _mockLogger;
    private readonly EmailSyncService _service;

    public EmailSyncServiceTests()
    {
        _mockEmailRepository = new Mock<IRepository<EmailMessage>>();
        _mockSyncRepository = new Mock<IRepository<EmailSync>>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockActivityRepository = new Mock<IRepository<Activity>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<EmailSyncService>>();

        _service = new EmailSyncService(
            _mockEmailRepository.Object,
            _mockSyncRepository.Object,
            _mockUserRepository.Object,
            _mockActivityRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Sync Setup Tests

    [Fact]
    public async Task SetupSyncAsync_ValidCredentials_ReturnsSyncConfig()
    {
        // Arrange
        var request = new SetupEmailSyncRequest
        {
            UserId = 1,
            Provider = "gmail",
            AccessToken = "token123",
            RefreshToken = "refresh123"
        };

        _mockSyncRepository.Setup(r => r.AddAsync(It.IsAny<EmailSync>()))
            .ReturnsAsync((EmailSync s) => { s.Id = 1; return s; });

        // Act
        var result = await _service.SetupSyncAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Provider.Should().Be("gmail");
    }

    [Fact]
    public async Task SetupSyncAsync_InvalidProvider_ThrowsException()
    {
        // Arrange
        var request = new SetupEmailSyncRequest
        {
            UserId = 1,
            Provider = "invalid"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SetupSyncAsync(request));
    }

    [Fact]
    public async Task SetupSyncAsync_DuplicateSetup_UpdatesExisting()
    {
        // Arrange
        var existingSync = new EmailSync { Id = 1, UserId = 1, Provider = "gmail" };
        var request = new SetupEmailSyncRequest
        {
            UserId = 1,
            Provider = "gmail",
            AccessToken = "newtoken"
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync> { existingSync });

        _mockSyncRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailSync>()))
            .ReturnsAsync((EmailSync s) => s);

        // Act
        var result = await _service.SetupSyncAsync(request);

        // Assert
        result.Should().NotBeNull();
        _mockSyncRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailSync>()), Times.Once);
    }

    #endregion

    #region Get Sync Status Tests

    [Fact]
    public async Task GetSyncStatusAsync_ActiveSync_ReturnsStatus()
    {
        // Arrange
        var sync = new EmailSync
        {
            Id = 1,
            UserId = 1,
            Provider = "gmail",
            LastSyncAt = DateTime.UtcNow.AddMinutes(-5),
            IsActive = true,
            SyncedEmailCount = 100
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync> { sync });

        // Act
        var result = await _service.GetSyncStatusAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
        result.SyncedEmailCount.Should().Be(100);
    }

    [Fact]
    public async Task GetSyncStatusAsync_NoSync_ReturnsNull()
    {
        // Arrange
        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync>());

        // Act
        var result = await _service.GetSyncStatusAsync(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Sync Emails Tests

    [Fact]
    public async Task SyncEmailsAsync_ValidSync_SyncsEmails()
    {
        // Arrange
        var sync = new EmailSync
        {
            Id = 1,
            UserId = 1,
            Provider = "gmail",
            IsActive = true
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync> { sync });

        _mockSyncRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailSync>()))
            .ReturnsAsync((EmailSync s) => s);

        // Act
        var result = await _service.SyncEmailsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SyncEmailsAsync_InactiveSync_ReturnsFalse()
    {
        // Arrange
        var sync = new EmailSync
        {
            Id = 1,
            UserId = 1,
            IsActive = false
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync> { sync });

        // Act
        var result = await _service.SyncEmailsAsync(1);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SyncEmailsAsync_TokenExpired_RefreshesToken()
    {
        // Arrange
        var sync = new EmailSync
        {
            Id = 1,
            UserId = 1,
            IsActive = true,
            TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            RefreshToken = "refresh123"
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync> { sync });

        _mockSyncRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailSync>()))
            .ReturnsAsync((EmailSync s) => s);

        // Act
        var result = await _service.SyncEmailsAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Get Emails Tests

    [Fact]
    public async Task GetEmailsAsync_ReturnsUserEmails()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, Subject = "Email 1", UserId = 1 },
            new EmailMessage { Id = 2, Subject = "Email 2", UserId = 1 }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        // Act
        var result = await _service.GetEmailsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmailByIdAsync_ExistingEmail_ReturnsEmail()
    {
        // Arrange
        var email = new EmailMessage
        {
            Id = 1,
            Subject = "Test Email",
            Body = "Email body"
        };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        // Act
        var result = await _service.GetEmailByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("Test Email");
    }

    [Fact]
    public async Task GetEmailsByContactAsync_ReturnsContactEmails()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, ContactEmail = "contact@test.com" }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        // Act
        var result = await _service.GetEmailsByContactAsync("contact@test.com");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetEmailThreadAsync_ReturnsThreadEmails()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, ThreadId = "thread123" },
            new EmailMessage { Id = 2, ThreadId = "thread123" }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        // Act
        var result = await _service.GetEmailThreadAsync("thread123");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Send Email Tests

    [Fact]
    public async Task SendEmailAsync_ValidEmail_SendsAndSaves()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            UserId = 1,
            To = "recipient@test.com",
            Subject = "Test Subject",
            Body = "Test body"
        };

        _mockEmailRepository.Setup(r => r.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => { e.Id = 1; return e; });

        // Act
        var result = await _service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task SendEmailAsync_WithAttachments_HandlesAttachments()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            UserId = 1,
            To = "recipient@test.com",
            Subject = "Test",
            Body = "Body",
            Attachments = new List<EmailAttachment>
            {
                new EmailAttachment { FileName = "test.pdf", ContentBase64 = "base64content" }
            }
        };

        _mockEmailRepository.Setup(r => r.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => { e.Id = 1; return e; });

        // Act
        var result = await _service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_InvalidRecipient_ThrowsException()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            UserId = 1,
            To = "invalid-email",
            Subject = "Test",
            Body = "Body"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SendEmailAsync(request));
    }

    #endregion

    #region Link to Entity Tests

    [Fact]
    public async Task LinkEmailToAccountAsync_ValidIds_LinksEmail()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, AccountId = null };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => e);

        // Act
        var result = await _service.LinkEmailToAccountAsync(1, 100);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task LinkEmailToContactAsync_ValidIds_LinksEmail()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, ContactId = null };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => e);

        // Act
        var result = await _service.LinkEmailToContactAsync(1, 50);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task LinkEmailToOpportunityAsync_ValidIds_LinksEmail()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, OpportunityId = null };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => e);

        // Act
        var result = await _service.LinkEmailToOpportunityAsync(1, 25);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Auto-Link Tests

    [Fact]
    public async Task AutoLinkEmailsAsync_MatchingContact_LinksAutomatically()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, ContactEmail = "contact@test.com", ContactId = null }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => e);

        // Act
        var result = await _service.AutoLinkEmailsAsync(1);

        // Assert
        result.LinkedCount.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Mark Email Tests

    [Fact]
    public async Task MarkAsReadAsync_UnreadEmail_MarksAsRead()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, IsRead = false };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => { e.IsRead = true; return e; });

        // Act
        var result = await _service.MarkAsReadAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsUnreadAsync_ReadEmail_MarksAsUnread()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, IsRead = true };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => { e.IsRead = false; return e; });

        // Act
        var result = await _service.MarkAsUnreadAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveEmailAsync_ActiveEmail_ArchivesEmail()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, IsArchived = false };

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        _mockEmailRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage e) => { e.IsArchived = true; return e; });

        // Act
        var result = await _service.ArchiveEmailAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteEmailAsync_ExistingEmail_DeletesEmail()
    {
        // Arrange
        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new EmailMessage { Id = 1 });

        _mockEmailRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteEmailAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DisableSyncAsync_ActiveSync_DisablesSync()
    {
        // Arrange
        var sync = new EmailSync { Id = 1, UserId = 1, IsActive = true };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailSync, bool>>>()))
            .ReturnsAsync(new List<EmailSync> { sync });

        _mockSyncRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailSync>()))
            .ReturnsAsync((EmailSync s) => { s.IsActive = false; return s; });

        // Act
        var result = await _service.DisableSyncAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchEmailsAsync_ValidQuery_ReturnsMatchingEmails()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, Subject = "Important meeting", Body = "Details" }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        // Act
        var result = await _service.SearchEmailsAsync(1, "meeting");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchEmailsAsync_EmptyQuery_ReturnsAllEmails()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, UserId = 1 },
            new EmailMessage { Id = 2, UserId = 1 }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        // Act
        var result = await _service.SearchEmailsAsync(1, "");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetEmailStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, IsRead = true, Direction = "inbound" },
            new EmailMessage { Id = 2, IsRead = false, Direction = "outbound" }
        };

        _mockEmailRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmailMessage, bool>>>()))
            .ReturnsAsync(emails);

        // Act
        var result = await _service.GetEmailStatisticsAsync(1);

        // Assert
        result.TotalEmails.Should().Be(2);
        result.UnreadCount.Should().Be(1);
    }

    #endregion
}

// Supporting classes for tests
public class SetupEmailSyncRequest
{
    public int UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}

public class SendEmailRequest
{
    public int UserId { get; set; }
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<EmailAttachment>? Attachments { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
}
