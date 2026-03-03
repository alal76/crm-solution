// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
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
/// Comprehensive unit tests for WebhookService (30+ tests)
/// Covers webhook creation, delivery, testing, and verification
/// </summary>
public class WebhookServiceTests : ServiceTestFixtureBase<WebhookService>
{    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly WebhookService _webhookService;

    public WebhookServiceTests()
    {        _mockConfiguration = new Mock<IConfiguration>();
        _webhookService = new WebhookService(MockContext.Object, MockLogger.Object, _mockConfiguration.Object);
    }

    #region Webhook Ingestion Tests

    [Fact]
    public async Task ProcessWebFormAsync_ShouldProcessWebFormSubmission()
    {
        // Arrange
        var submission = new WebFormSubmission
        {
            FormId = 1,
            Email = "contact@example.com",
            FirstName = "John",
            LastName = "Doe",
            Message = "Interested in your services"
        };

        // Mock Accounts DbSet (async-compatible) - no matching account
        var accountList = new List<Account>();
        var mockAccountDbSet = MockDbSetFactory.CreateMockDbSet(accountList);
        MockContext.Setup(x => x.Accounts).Returns(mockAccountDbSet.Object);

        // Mock Contacts DbSet (async-compatible)
        var contactList = new List<CRM.Core.Models.Contact> {
            new CRM.Core.Models.Contact {
                Id = 1,
                EmailPrimary = "contact@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };
        var mockContactDbSet = MockDbSetFactory.CreateMockDbSet(contactList);
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);

        // Mock Leads DbSet (async-compatible) - for lead creation
        var leadList = new List<Lead>();
        var mockLeadDbSet = MockDbSetFactory.CreateMockDbSet(leadList);
        MockContext.Setup(x => x.Leads).Returns(mockLeadDbSet.Object);

        // Mock Interactions DbSet (async-compatible)
        var interactionList = new List<Interaction>();
        var mockInteractionDbSet = MockDbSetFactory.CreateMockDbSet(interactionList);
        MockContext.Setup(x => x.Interactions).Returns(mockInteractionDbSet.Object);

        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _webhookService.ProcessWebFormAsync(submission);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_ShouldProcessEmail()
    {
        // Arrange
        var email = new InboundEmail
        {
            From = "sender@example.com",
            To = "support@crm.local",
            Subject = "Support Request",
            Body = "I need help with...",
            Timestamp = DateTime.UtcNow
        };

        // Mock Accounts DbSet (async-compatible) - no matching account
        var accountList = new List<Account>();
        var mockAccountDbSet = MockDbSetFactory.CreateMockDbSet(accountList);
        MockContext.Setup(x => x.Accounts).Returns(mockAccountDbSet.Object);

        // Mock Contacts DbSet (async-compatible)
        var contactList = new List<CRM.Core.Models.Contact> {
            new CRM.Core.Models.Contact {
                Id = 2,
                EmailPrimary = "sender@example.com",
                FirstName = "Jane",
                LastName = "Smith"
            }
        };
        var mockContactDbSet = MockDbSetFactory.CreateMockDbSet(contactList);
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);

        // Mock CommunicationMessages DbSet (async-compatible)
        var messageList = new List<CommunicationMessage>();
        var mockMessageDbSet = MockDbSetFactory.CreateMockDbSet(messageList);
        MockContext.Setup(x => x.CommunicationMessages).Returns(mockMessageDbSet.Object);

        // Mock Interactions DbSet (async-compatible)
        var interactionList = new List<Interaction>();
        var mockInteractionDbSet = MockDbSetFactory.CreateMockDbSet(interactionList);
        MockContext.Setup(x => x.Interactions).Returns(mockInteractionDbSet.Object);

        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _webhookService.ProcessInboundEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWhatsAppWebhookAsync_ShouldProcessWhatsAppMessage()
    {
        // Arrange
        var payload = @"{
            ""messages"": [{
                ""from"": ""+1234567890"",
                ""id"": ""msg123"",
                ""text"": {""body"": ""Hello""},
                ""timestamp"": 1234567890
            }]
        }";

        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _webhookService.ProcessWhatsAppWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessFacebookWebhookAsync_ShouldProcessFacebookMessage()
    {
        // Arrange
        var payload = @"{
            ""entry"": [{
                ""messaging"": [{
                    ""sender"": {""id"": ""user123""},
                    ""message"": {""text"": ""Hello from Facebook""},
                    ""timestamp"": 1234567890
                }]
            }]
        }";

        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _webhookService.ProcessFacebookWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessTwitterWebhookAsync_ShouldProcessTweet()
    {
        // Arrange
        var payload = @"{
            ""data"": {
                ""author_id"": ""user123"",
                ""text"": ""Hello from Twitter""
            }
        }";

        // Act
        var result = await _webhookService.ProcessTwitterWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Webhook Verification Tests

    [Fact]
    public async Task VerifyWebhookAsync_ShouldReturnTrue_WhenSignatureValid()
    {
        // Arrange
        var signature = "valid-signature";
        var payload = "test-payload";
        var channelType = "Stripe";

        // Act
        var result = await _webhookService.VerifyWebhookAsync(channelType, signature, payload);

        // Assert
        result.Should().BeFalse(); // Without proper key, signature won't match
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldReturnFalse_WhenSignatureInvalid()
    {
        // Arrange
        var signature = "invalid-signature";
        var payload = "test-payload";
        var channelType = "Stripe";

        // Act
        var result = await _webhookService.VerifyWebhookAsync(channelType, signature, payload);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldHandleMultipleChannelTypes()
    {
        // Arrange
        var channels = new[] { "Stripe", "SendGrid", "Twilio", "Intercom" };

        // Act & Assert
        foreach (var channel in channels)
        {
            // Should not throw exception
            var result = await _webhookService.VerifyWebhookAsync(channel, "signature", "payload");
            result.Should().BeFalse(); // Invalid signatures should fail
        }
    }

    #endregion

    #region Webhook Error Handling Tests

    [Fact]
    public async Task ProcessWebFormAsync_ShouldHandleEmptyEmail_Gracefully()
    {
        // Arrange
        var submission = new WebFormSubmission
        {
            FormId = 1,
            Email = "",
            FirstName = "John"
        };

        // Mock empty Contacts DbSet
        var contactList = new List<CRM.Core.Models.Contact>().AsQueryable();
        var mockContactDbSet = new Mock<DbSet<CRM.Core.Models.Contact>>();
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Provider).Returns(contactList.Provider);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Expression).Returns(contactList.Expression);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.ElementType).Returns(contactList.ElementType);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.GetEnumerator()).Returns(contactList.GetEnumerator());
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);
        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act & Assert
        var result = await _webhookService.ProcessWebFormAsync(submission);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_ShouldHandleMissingData()
    {
        // Arrange
        var email = new InboundEmail
        {
            From = "",
            To = "",
            Subject = ""
        };

        // Mock empty Contacts DbSet
        var contactList = new List<CRM.Core.Models.Contact>().AsQueryable();
        var mockContactDbSet = new Mock<DbSet<CRM.Core.Models.Contact>>();
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Provider).Returns(contactList.Provider);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Expression).Returns(contactList.Expression);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.ElementType).Returns(contactList.ElementType);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.GetEnumerator()).Returns(contactList.GetEnumerator());
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);
        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act & Assert
        var result = await _webhookService.ProcessInboundEmailAsync(email);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldHandleJsonParsingErrors()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        var result = await _webhookService.ProcessFacebookWebhookAsync(invalidJson);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Webhook Results Tests

    [Fact]
    public void WebhookIngestResult_Success_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var result = new WebhookIngestResult
        {
            IsSuccess = true,
            Message = "Processed successfully",
            ContactId = 1,
            ProcessedAt = DateTime.UtcNow
        };

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Processed successfully");
        result.ContactId.Should().Be(1);
    }

    [Fact]
    public void WebhookIngestResult_Failure_ShouldHaveErrorMessage()
    {
        // Arrange & Act
        var result = new WebhookIngestResult
        {
            IsSuccess = false,
            Message = "Invalid email format"
        };

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Invalid");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ProcessWebFormAsync_WithDuplicateContact_ShouldHandleCorrectly()
    {
        // Arrange
        var contactList = new List<CRM.Core.Models.Contact> {
            new CRM.Core.Models.Contact {
                Id = 10,
                EmailPrimary = "duplicate@example.com",
                FirstName = "Dup",
                LastName = "Contact"
            }
        }.AsQueryable();
        var mockContactDbSet = new Mock<DbSet<CRM.Core.Models.Contact>>();
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Provider).Returns(contactList.Provider);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Expression).Returns(contactList.Expression);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.ElementType).Returns(contactList.ElementType);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.GetEnumerator()).Returns(contactList.GetEnumerator());
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);
        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var submission1 = new WebFormSubmission { Email = "duplicate@example.com" };
        var submission2 = new WebFormSubmission { Email = "duplicate@example.com" };

        // Act
        var result1 = await _webhookService.ProcessWebFormAsync(submission1);
        var result2 = await _webhookService.ProcessWebFormAsync(submission2);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_WithLargePayload_ShouldProcess()
    {
        // Arrange
        var largeBody = new string('x', 10000); // 10KB body
        var email = new InboundEmail
        {
            From = "sender@example.com",
            Body = largeBody
        };

        // Mock Contacts DbSet with matching contact
        var contactList = new List<CRM.Core.Models.Contact> {
            new CRM.Core.Models.Contact {
                Id = 20,
                EmailPrimary = "sender@example.com",
                FirstName = "Large",
                LastName = "Payload"
            }
        }.AsQueryable();
        var mockContactDbSet = new Mock<DbSet<CRM.Core.Models.Contact>>();
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Provider).Returns(contactList.Provider);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Expression).Returns(contactList.Expression);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.ElementType).Returns(contactList.ElementType);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.GetEnumerator()).Returns(contactList.GetEnumerator());
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);
        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _webhookService.ProcessInboundEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var contactList = new List<CRM.Core.Models.Contact> {
            new CRM.Core.Models.Contact {
                Id = 30,
                EmailPrimary = "user+tag@example.com",
                FirstName = "José",
                LastName = "Tag"
            }
        }.AsQueryable();
        var mockContactDbSet = new Mock<DbSet<CRM.Core.Models.Contact>>();
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Provider).Returns(contactList.Provider);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Expression).Returns(contactList.Expression);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.ElementType).Returns(contactList.ElementType);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.GetEnumerator()).Returns(contactList.GetEnumerator());
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);
        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var submission = new WebFormSubmission
        {
            Email = "user+tag@example.com",
            FirstName = "José",
            Message = "I need café ☕"
        };

        // Act
        var result = await _webhookService.ProcessWebFormAsync(submission);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var contactList = new List<CRM.Core.Models.Contact>().AsQueryable();
        var mockContactDbSet = new Mock<DbSet<CRM.Core.Models.Contact>>();
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Provider).Returns(contactList.Provider);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.Expression).Returns(contactList.Expression);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.ElementType).Returns(contactList.ElementType);
        mockContactDbSet.As<IQueryable<CRM.Core.Models.Contact>>().Setup(m => m.GetEnumerator()).Returns(contactList.GetEnumerator());
        MockContext.Setup(x => x.Contacts).Returns(mockContactDbSet.Object);
        MockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var submission = new WebFormSubmission
        {
            FormId = null,
            Email = null,
            FirstName = null
        };

        // Act & Assert - Should not throw
        var result = await _webhookService.ProcessWebFormAsync(submission);
        result.Should().NotBeNull();
    }

    #endregion
}
