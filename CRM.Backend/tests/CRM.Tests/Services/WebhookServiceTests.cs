// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WebhookService.
/// Covers web form processing, inbound email processing, webhook verification,
/// social media webhooks, and error handling.
/// </summary>
public class WebhookServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<WebhookService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly WebhookService _service;

    private readonly List<Account> _accounts;
    private readonly List<CRM.Core.Models.Contact> _contacts;
    private readonly List<Lead> _leads;
    private readonly List<Interaction> _interactions;
    private readonly List<CommunicationMessage> _messages;

    public WebhookServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<WebhookService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        _accounts = new List<Account>();
        _contacts = new List<CRM.Core.Models.Contact>();
        _leads = new List<Lead>();
        _interactions = new List<Interaction>();
        _messages = new List<CommunicationMessage>();

        SetupMockDbSets();

        _service = new WebhookService(
            _mockDbContext.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    private void SetupMockDbSets()
    {
        var mockAccounts = MockDbSetFactory.CreateMockDbSet(_accounts);
        var mockContacts = MockDbSetFactory.CreateMockDbSet(_contacts);
        var mockLeads = MockDbSetFactory.CreateMockDbSet(_leads);
        var mockInteractions = MockDbSetFactory.CreateMockDbSet(_interactions);
        var mockMessages = MockDbSetFactory.CreateMockDbSet(_messages);

        _mockDbContext.Setup(c => c.Accounts).Returns(mockAccounts.Object);
        _mockDbContext.Setup(c => c.Contacts).Returns(mockContacts.Object);
        _mockDbContext.Setup(c => c.Leads).Returns(mockLeads.Object);
        _mockDbContext.Setup(c => c.Interactions).Returns(mockInteractions.Object);
        _mockDbContext.Setup(c => c.CommunicationMessages).Returns(mockMessages.Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ========================================================================
    // Constructor Tests
    // ========================================================================

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDbContextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new WebhookService(null!, _mockLogger.Object, _mockConfiguration.Object));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new WebhookService(_mockDbContext.Object, null!, _mockConfiguration.Object));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConfigurationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new WebhookService(_mockDbContext.Object, _mockLogger.Object, null!));
    }

    // ========================================================================
    // ProcessWebFormAsync Tests
    // ========================================================================

    [Fact]
    public async Task ProcessWebFormAsync_ShouldReturnSuccess_WithNewLead()
    {
        // Arrange
        var submission = new WebFormSubmission
        {
            Name = "John Doe",
            Email = "john@newcompany.com",
            Phone = "555-0100",
            Subject = "Interested in CRM",
            Message = "Please contact me"
        };

        // Act
        var result = await _service.ProcessWebFormAsync(submission);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("processed successfully");
        _leads.Should().HaveCount(1);
        _leads[0].FirstName.Should().Be("John");
        _leads[0].LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task ProcessWebFormAsync_ShouldMatchExistingAccount_ByEmail()
    {
        // Arrange
        _accounts.Add(new Account { Id = 10, Email = "known@company.com", Name = "Known Co" });
        RefreshMockDbSets();

        var submission = new WebFormSubmission
        {
            Name = "Existing Customer",
            Email = "known@company.com",
            Subject = "Support request"
        };

        // Act
        var result = await _service.ProcessWebFormAsync(submission);

        // Assert
        result.Success.Should().BeTrue();
        result.AccountId.Should().Be(10);
    }

    [Fact]
    public async Task ProcessWebFormAsync_ShouldReturnSuccess_WithNullEmail()
    {
        // Arrange
        var submission = new WebFormSubmission
        {
            Name = "No Email",
            Subject = "General inquiry"
        };

        // Act
        var result = await _service.ProcessWebFormAsync(submission);

        // Assert
        result.Success.Should().BeTrue();
    }

    // ========================================================================
    // ProcessInboundEmailAsync Tests
    // ========================================================================

    [Fact]
    public async Task ProcessInboundEmailAsync_ShouldReturnSuccess_WithKnownSender()
    {
        // Arrange
        _accounts.Add(new Account { Id = 5, Email = "sender@known.com", Name = "Known Account" });
        RefreshMockDbSets();

        var email = new InboundEmail
        {
            From = "sender@known.com",
            FromName = "Known Sender",
            To = "crm@company.com",
            Subject = "Follow-up",
            TextBody = "Hi, following up on our conversation."
        };

        // Act
        var result = await _service.ProcessInboundEmailAsync(email);

        // Assert
        result.Success.Should().BeTrue();
        result.AccountId.Should().Be(5);
        result.Message.Should().Contain("processed successfully");
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_ShouldReturnSuccess_WithUnknownSender()
    {
        // Arrange
        var email = new InboundEmail
        {
            From = "unknown@example.com",
            Subject = "Hello",
            TextBody = "I'd like to know more."
        };

        // Act
        var result = await _service.ProcessInboundEmailAsync(email);

        // Assert
        result.Success.Should().BeTrue();
        result.AccountId.Should().BeNull();
    }

    // ========================================================================
    // ProcessWhatsAppWebhookAsync Tests
    // ========================================================================

    [Fact]
    public async Task ProcessWhatsAppWebhookAsync_ShouldReturnSuccess_ForValidPayload()
    {
        // Arrange
        var payload = @"{
            ""entry"": [{
                ""changes"": [{
                    ""value"": {
                        ""messages"": [{
                            ""from"": ""15551234567"",
                            ""type"": ""text"",
                            ""text"": { ""body"": ""Hello from WhatsApp"" }
                        }]
                    }
                }]
            }]
        }";

        // Act
        var result = await _service.ProcessWhatsAppWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWhatsAppWebhookAsync_ShouldReturnSuccess_ForEmptyEntries()
    {
        // Arrange - no "entry" property
        var payload = @"{}";

        // Act
        var result = await _service.ProcessWhatsAppWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("No entry");
    }

    // ========================================================================
    // ProcessFacebookWebhookAsync Tests
    // ========================================================================

    [Fact]
    public async Task ProcessFacebookWebhookAsync_ShouldReturnSuccess_ForValidPayload()
    {
        // Arrange
        var payload = @"{
            ""entry"": [{
                ""messaging"": [{
                    ""sender"": { ""id"": ""12345"" },
                    ""message"": { ""text"": ""Hello from Facebook"" }
                }]
            }]
        }";

        // Act
        var result = await _service.ProcessFacebookWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessFacebookWebhookAsync_ShouldReturnSuccess_ForEmptyPayload()
    {
        // Arrange
        var payload = @"{}";

        // Act
        var result = await _service.ProcessFacebookWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    // ========================================================================
    // ProcessTwitterWebhookAsync Tests
    // ========================================================================

    [Fact]
    public async Task ProcessTwitterWebhookAsync_ShouldReturnSuccess_ForDirectMessage()
    {
        // Arrange
        var payload = @"{
            ""direct_message_events"": [{
                ""message_create"": {
                    ""sender_id"": ""67890"",
                    ""message_data"": { ""text"": ""Hello from Twitter"" }
                }
            }]
        }";

        // Act
        var result = await _service.ProcessTwitterWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessTwitterWebhookAsync_ShouldReturnSuccess_ForTweetMention()
    {
        // Arrange
        var payload = @"{
            ""tweet_create_events"": [{
                ""text"": ""@crm Great product!"",
                ""user"": { ""id_str"": ""11111"" }
            }]
        }";

        // Act
        var result = await _service.ProcessTwitterWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    // ========================================================================
    // VerifyWebhookAsync Tests
    // ========================================================================

    [Fact]
    public async Task VerifyWebhookAsync_ShouldReturnFalse_WhenNoSecretConfigured()
    {
        // Arrange
        _mockConfiguration.Setup(c => c[It.IsAny<string>()]).Returns((string?)null);

        // Act
        var result = await _service.VerifyWebhookAsync("whatsapp", "sha256=abc", "payload");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldReturnFalse_WhenSignatureMismatch()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["Webhooks:WhatsApp:VerifyToken"]).Returns("test_secret");

        // Act
        var result = await _service.VerifyWebhookAsync("whatsapp", "sha256=invalid", "payload");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldReturnTrue_WhenSignatureMatches()
    {
        // Arrange
        var secret = "test_secret_key";
        var payload = "test_payload";

        _mockConfiguration.Setup(c => c["Webhooks:WhatsApp:VerifyToken"]).Returns(secret);

        // Compute expected signature
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        var expectedSig = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        // Act
        var result = await _service.VerifyWebhookAsync("whatsapp", expectedSig, payload);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldReturnFalse_ForUnknownChannelType()
    {
        // Act
        var result = await _service.VerifyWebhookAsync("unknown_channel", "sha256=abc", "payload");

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private void RefreshMockDbSets()
    {
        var mockAccounts = MockDbSetFactory.CreateMockDbSet(_accounts);
        var mockContacts = MockDbSetFactory.CreateMockDbSet(_contacts);
        var mockLeads = MockDbSetFactory.CreateMockDbSet(_leads);
        var mockInteractions = MockDbSetFactory.CreateMockDbSet(_interactions);
        var mockMessages = MockDbSetFactory.CreateMockDbSet(_messages);

        _mockDbContext.Setup(c => c.Accounts).Returns(mockAccounts.Object);
        _mockDbContext.Setup(c => c.Contacts).Returns(mockContacts.Object);
        _mockDbContext.Setup(c => c.Leads).Returns(mockLeads.Object);
        _mockDbContext.Setup(c => c.Interactions).Returns(mockInteractions.Object);
        _mockDbContext.Setup(c => c.CommunicationMessages).Returns(mockMessages.Object);
    }
}
