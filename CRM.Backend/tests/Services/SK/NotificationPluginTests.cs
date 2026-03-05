// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the NotificationPlugin Semantic Kernel plugin.
/// </summary>
public class NotificationPluginTests
{
    private readonly Mock<INotificationPort> _notificationPortMock;
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<NotificationPlugin>> _loggerMock;
    private readonly NotificationPlugin _sut;

    public NotificationPluginTests()
    {
        _notificationPortMock = new Mock<INotificationPort>();
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<NotificationPlugin>>();
        _sut = new NotificationPlugin(_notificationPortMock.Object, _dbContextMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNotificationPortIsNull()
    {
        var act = () => new NotificationPlugin(null!, _dbContextMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("notificationPort");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
    {
        var act = () => new NotificationPlugin(_notificationPortMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new NotificationPlugin(_notificationPortMock.Object, _dbContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_Notification()
    {
        _sut.PluginName.Should().Be("Notification");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetNotificationHistoryAsync Tests

    [Fact]
    public async Task GetNotificationHistoryAsync_ShouldReturnSuccessJson_WhenMessagesExist()
    {
        var messages = new List<CommunicationMessage>
        {
            new CommunicationMessage
            {
                Id = 1,
                Subject = "Order confirmation",
                Body = "Your order has been placed.",
                ChannelType = ChannelType.Email,
                Direction = MessageDirection.Outbound,
                Status = MessageStatus.Sent,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(messages);
        _dbContextMock.Setup(c => c.CommunicationMessages).Returns(mockSet.Object);

        var result = await _sut.GetNotificationHistoryAsync(25);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalReturned").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetNotificationHistoryAsync_ShouldFilterByDirection_WhenDirectionProvided()
    {
        var messages = new List<CommunicationMessage>
        {
            new CommunicationMessage { Id = 1, Subject = "Out", ChannelType = ChannelType.Email, Direction = MessageDirection.Outbound, Status = MessageStatus.Sent, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new CommunicationMessage { Id = 2, Subject = "In", ChannelType = ChannelType.Email, Direction = MessageDirection.Inbound, Status = MessageStatus.Sent, IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(messages);
        _dbContextMock.Setup(c => c.CommunicationMessages).Returns(mockSet.Object);

        var result = await _sut.GetNotificationHistoryAsync(25, "Outbound");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalReturned").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetNotificationHistoryAsync_ShouldReturnErrorJson_WhenDbSetThrows()
    {
        _dbContextMock
            .Setup(c => c.CommunicationMessages)
            .Throws(new InvalidOperationException("DB failed"));

        var result = await _sut.GetNotificationHistoryAsync(25);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetNotificationHistory");
    }

    [Fact]
    public async Task GetNotificationHistoryAsync_ShouldReturnEmptyResult_WhenNoMessages()
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(new List<CommunicationMessage>());
        _dbContextMock.Setup(c => c.CommunicationMessages).Returns(mockSet.Object);

        var result = await _sut.GetNotificationHistoryAsync(25);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalReturned").GetInt32().Should().Be(0);
    }

    #endregion

    #region GetNotificationStatsAsync Tests

    [Fact]
    public async Task GetNotificationStatsAsync_ShouldReturnSuccessJson_WithStats()
    {
        var messages = new List<CommunicationMessage>
        {
            new CommunicationMessage { Id = 1, ChannelType = ChannelType.Email, Direction = MessageDirection.Outbound, Status = MessageStatus.Sent, IsDeleted = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new CommunicationMessage { Id = 2, ChannelType = ChannelType.SMS, Direction = MessageDirection.Outbound, Status = MessageStatus.Delivered, IsDeleted = false, CreatedAt = DateTime.UtcNow.AddDays(-2) }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(messages);
        _dbContextMock.Setup(c => c.CommunicationMessages).Returns(mockSet.Object);

        var result = await _sut.GetNotificationStatsAsync(7);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalMessages").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task GetNotificationStatsAsync_ShouldReturnErrorJson_WhenDbSetThrows()
    {
        _dbContextMock
            .Setup(c => c.CommunicationMessages)
            .Throws(new Exception("Stats query failed"));

        var result = await _sut.GetNotificationStatsAsync(7);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetNotificationStats");
    }

    #endregion

    #region SendNotificationAsync Tests

    [Fact]
    public async Task SendNotificationAsync_ShouldReturnSuccessJson_WhenSendSucceeds()
    {
        var notifResult = new NotificationResult
        {
            Success = true,
            MessageId = "msg-001",
            Provider = "BuiltIn"
        };
        _notificationPortMock
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var result = await _sut.SendNotificationAsync(
            "user@example.com",
            "Test Subject",
            "<p>Hello</p>");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("messageId").GetString().Should().Be("msg-001");
    }

    [Fact]
    public async Task SendNotificationAsync_ShouldReturnSuccessJson_WithSuccessFalse_WhenProviderFails()
    {
        var notifResult = new NotificationResult
        {
            Success = false,
            Error = "SMTP not configured",
            Provider = "BuiltIn"
        };
        _notificationPortMock
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var result = await _sut.SendNotificationAsync("test@example.com", "Sub", "Body");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task SendNotificationAsync_ShouldReturnErrorJson_WhenPortThrows()
    {
        _notificationPortMock
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection refused"));

        var result = await _sut.SendNotificationAsync("a@b.com", "Subject", "Body");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("SendNotification");
    }

    #endregion
}
