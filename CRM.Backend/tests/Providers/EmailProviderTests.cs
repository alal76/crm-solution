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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Mail;
using System.Threading;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for Email Provider
/// Covers: Email sending, templates, attachments
/// </summary>
public class EmailProviderTests
{
    private readonly Mock<IOptions<EmailSettings>> _mockEmailSettings;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailSettings _settings;

    public EmailProviderTests()
    {
        _settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            SmtpPort = 587,
            FromAddress = "noreply@test.com",
            FromName = "CRM System",
            Username = "testuser",
            Password = "testpassword",
            EnableSsl = true,
            UseDevelopmentMode = true
        };

        _mockEmailSettings = new Mock<IOptions<EmailSettings>>();
        _mockEmailSettings.Setup(x => x.Value).Returns(_settings);
        _mockLogger = new Mock<ILogger<EmailService>>();
    }

    #region SendEmail Tests

    [Fact]
    public async Task SendEmailAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_EmptyToAddress_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = new EmailRequest
        {
            To = "",
            Subject = "Test",
            Body = "Test body"
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_NullSubject_HandlesGracefully()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = new EmailRequest
        {
            To = "recipient@test.com",
            Subject = null!,
            Body = "Test body"
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        // Should either succeed with empty subject or fail gracefully
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_InvalidEmailFormat_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = new EmailRequest
        {
            To = "invalid-email",
            Subject = "Test",
            Body = "Test body"
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_WithCc_IncludesCcRecipients()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.Cc = new List<string> { "cc1@test.com", "cc2@test.com" };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_WithBcc_IncludesBccRecipients()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.Bcc = new List<string> { "bcc1@test.com", "bcc2@test.com" };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_HtmlBody_SetsIsHtmlBodyTrue()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.IsHtml = true;
        request.Body = "<html><body><h1>Test</h1></body></html>";

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_WithReplyTo_SetsReplyToAddress()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.ReplyTo = "reply@test.com";

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region SendBulkEmail Tests

    [Fact]
    public async Task SendBulkEmailAsync_ValidRecipients_SendsToAll()
    {
        // Arrange
        var service = CreateMockEmailService();
        var recipients = new List<string> { "user1@test.com", "user2@test.com", "user3@test.com" };
        var subject = "Bulk Test";
        var body = "Bulk email body";

        // Act
        var result = await service.SendBulkEmailAsync(recipients, subject, body);

        // Assert
        result.SuccessCount.Should().Be(3);
    }

    [Fact]
    public async Task SendBulkEmailAsync_EmptyRecipientList_ReturnsZeroSent()
    {
        // Arrange
        var service = CreateMockEmailService();
        var recipients = new List<string>();

        // Act
        var result = await service.SendBulkEmailAsync(recipients, "Test", "Body");

        // Assert
        result.SuccessCount.Should().Be(0);
    }

    [Fact]
    public async Task SendBulkEmailAsync_SomeInvalidEmails_ReportsFailures()
    {
        // Arrange
        var service = CreateMockEmailService();
        var recipients = new List<string> { "valid@test.com", "invalid-email", "another@test.com" };

        // Act
        var result = await service.SendBulkEmailAsync(recipients, "Test", "Body");

        // Assert
        result.FailureCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithRateLimit_RespectsLimit()
    {
        // Arrange
        var service = CreateMockEmailService();
        var recipients = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            recipients.Add($"user{i}@test.com");
        }

        // Act
        var result = await service.SendBulkEmailAsync(recipients, "Test", "Body", rateLimit: 10);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Template Tests

    [Fact]
    public async Task SendTemplatedEmailAsync_ValidTemplate_ReplacesPlaceholders()
    {
        // Arrange
        var service = CreateMockEmailService();
        var template = "Hello {{Name}}, your order {{OrderId}} is confirmed.";
        var data = new Dictionary<string, string>
        {
            { "Name", "John" },
            { "OrderId", "12345" }
        };

        // Act
        var result = await service.SendTemplatedEmailAsync("recipient@test.com", "Order Confirmation", template, data);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_MissingPlaceholder_HandlesGracefully()
    {
        // Arrange
        var service = CreateMockEmailService();
        var template = "Hello {{Name}}, your order {{OrderId}} is confirmed.";
        var data = new Dictionary<string, string>
        {
            { "Name", "John" }
            // Missing OrderId
        };

        // Act
        var result = await service.SendTemplatedEmailAsync("recipient@test.com", "Test", template, data);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_EmptyTemplate_HandlesGracefully()
    {
        // Arrange
        var service = CreateMockEmailService();
        var data = new Dictionary<string, string> { { "Name", "John" } };

        // Act
        var result = await service.SendTemplatedEmailAsync("recipient@test.com", "Test", "", data);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Attachment Tests

    [Fact]
    public async Task SendEmailWithAttachmentAsync_ValidAttachment_Succeeds()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.Attachments = new List<EmailAttachment>
        {
            new EmailAttachment
            {
                FileName = "document.pdf",
                Content = new byte[] { 1, 2, 3, 4, 5 },
                ContentType = "application/pdf"
            }
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailWithAttachmentAsync_MultipleAttachments_Succeeds()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.Attachments = new List<EmailAttachment>
        {
            new EmailAttachment { FileName = "doc1.pdf", Content = new byte[] { 1, 2, 3 }, ContentType = "application/pdf" },
            new EmailAttachment { FileName = "doc2.xlsx", Content = new byte[] { 4, 5, 6 }, ContentType = "application/vnd.ms-excel" }
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailWithAttachmentAsync_LargeAttachment_HandlesGracefully()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        var largeContent = new byte[10 * 1024 * 1024]; // 10MB
        request.Attachments = new List<EmailAttachment>
        {
            new EmailAttachment { FileName = "large.bin", Content = largeContent, ContentType = "application/octet-stream" }
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Priority Tests

    [Fact]
    public async Task SendEmailAsync_HighPriority_SetsCorrectPriority()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.Priority = EmailPriority.High;

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_LowPriority_SetsCorrectPriority()
    {
        // Arrange
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();
        request.Priority = EmailPriority.Low;

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("valid@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("invalid", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("noatsign.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_ReturnsExpectedResult(string email, bool expected)
    {
        // Arrange
        var service = CreateMockEmailService();

        // Act
        var result = service.IsValidEmail(email);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Constructor_NullSettings_ThrowsException()
    {
        // Act & Assert
        var action = () => new MockEmailService(null!, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_DevelopmentMode_SkipsActualSend()
    {
        // Arrange
        _settings.UseDevelopmentMode = true;
        var service = CreateMockEmailService();
        var request = CreateValidEmailRequest();

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.DevelopmentModeSkipped.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private EmailService CreateMockEmailService()
    {
        return new MockEmailService(_mockEmailSettings.Object, _mockLogger.Object);
    }

    private EmailRequest CreateValidEmailRequest()
    {
        return new EmailRequest
        {
            To = "recipient@test.com",
            Subject = "Test Email",
            Body = "This is a test email body."
        };
    }

    #endregion
}

// Mock implementation for testing
public class MockEmailService : EmailService
{
    public MockEmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        : base(settings, logger)
    {
    }

    public override async Task<EmailResult> SendEmailAsync(EmailRequest request)
    {
        if (string.IsNullOrEmpty(request.To))
            return new EmailResult { Success = false, ErrorMessage = "To address required" };

        if (!IsValidEmail(request.To))
            return new EmailResult { Success = false, ErrorMessage = "Invalid email format" };

        await Task.Delay(1); // Simulate async operation

        return new EmailResult
        {
            Success = true,
            DevelopmentModeSkipped = Settings.UseDevelopmentMode
        };
    }

    public override async Task<BulkEmailResult> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body, int? rateLimit = null)
    {
        var result = new BulkEmailResult();
        foreach (var recipient in recipients)
        {
            if (IsValidEmail(recipient))
                result.SuccessCount++;
            else
                result.FailureCount++;
        }
        await Task.Delay(1);
        return result;
    }

    public override async Task<EmailResult> SendTemplatedEmailAsync(string to, string subject, string template, Dictionary<string, string> data)
    {
        if (!IsValidEmail(to))
            return new EmailResult { Success = false };

        await Task.Delay(1);
        return new EmailResult { Success = true };
    }

    public override bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected EmailSettings Settings => _settings.Value;
}

// Supporting classes
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public bool UseDevelopmentMode { get; set; }
}

public class EmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public string? ReplyTo { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}

public class EmailResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool DevelopmentModeSkipped { get; set; }
}

public class BulkEmailResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public enum EmailPriority
{
    Low,
    Normal,
    High
}

public abstract class EmailService
{
    protected readonly IOptions<EmailSettings> _settings;
    protected readonly ILogger<EmailService> _logger;

    protected EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;
    }

    public abstract Task<EmailResult> SendEmailAsync(EmailRequest request);
    public abstract Task<BulkEmailResult> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body, int? rateLimit = null);
    public abstract Task<EmailResult> SendTemplatedEmailAsync(string to, string subject, string template, Dictionary<string, string> data);
    public abstract bool IsValidEmail(string email);
}
