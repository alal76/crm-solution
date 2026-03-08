// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.Notifications;
using CRM.Infrastructure.Services.Notifications;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SmsNotificationChannelService — Twilio delegation pattern.
/// COMM-006: Twilio SMS wiring via ISmsNotificationService.
/// </summary>
public class SmsNotificationChannelServiceTests
{
    private readonly Mock<ILogger<SmsNotificationChannelService>> _mockLogger;
    private readonly Mock<ISmsNotificationService> _mockSmsService;

    public SmsNotificationChannelServiceTests()
    {
        _mockLogger = new Mock<ILogger<SmsNotificationChannelService>>();
        _mockSmsService = new Mock<ISmsNotificationService>();
    }

    private SmsNotificationChannelService CreateServiceWithProvider()
    {
        return new SmsNotificationChannelService(_mockLogger.Object, _mockSmsService.Object);
    }

    private SmsNotificationChannelService CreateServiceWithoutProvider()
    {
        return new SmsNotificationChannelService(_mockLogger.Object);
    }

    // =========================================================================
    // SendSmsAsync — with provider
    // =========================================================================

    [Fact]
    public async Task SendSmsAsync_ShouldDelegateToSmsService_WhenProviderAvailable()
    {
        // Arrange
        _mockSmsService.Setup(s => s.SendSmsAsync("+14155552671", "Hello", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateServiceWithProvider();

        // Act
        var result = await service.SendSmsAsync("+14155552671", "Hello");

        // Assert
        result.Should().BeTrue();
        _mockSmsService.Verify(s => s.SendSmsAsync("+14155552671", "Hello", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnFalse_WhenProviderFails()
    {
        // Arrange
        _mockSmsService.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var service = CreateServiceWithProvider();

        // Act
        var result = await service.SendSmsAsync("+14155552671", "Test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnFalse_WhenProviderThrows()
    {
        // Arrange
        _mockSmsService.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Twilio error"));
        var service = CreateServiceWithProvider();

        // Act
        var result = await service.SendSmsAsync("+14155552671", "Test");

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================================
    // SendSmsAsync — without provider (fallback logging)
    // =========================================================================

    [Fact]
    public async Task SendSmsAsync_ShouldReturnTrue_WhenNoProviderConfigured()
    {
        // Arrange
        var service = CreateServiceWithoutProvider();

        // Act
        var result = await service.SendSmsAsync("+14155552671", "Hello");

        // Assert
        result.Should().BeTrue();
    }

    // =========================================================================
    // Validation
    // =========================================================================

    [Theory]
    [InlineData("+14155552671", true)]
    [InlineData("+447911123456", true)]
    [InlineData("+861391234567", true)]
    [InlineData("", false)]
    [InlineData("12345", false)]    // no + prefix
    [InlineData("+0123456789", false)] // leading zero
    public void ValidatePhoneNumber_ShouldReturnExpected(string phone, bool expected)
    {
        // Arrange
        var service = CreateServiceWithoutProvider();

        // Act
        var result = service.ValidatePhoneNumber(phone);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnFalse_WhenPhoneNumberInvalid()
    {
        // Arrange
        var service = CreateServiceWithProvider();

        // Act
        var result = await service.SendSmsAsync("invalid", "Test message");

        // Assert
        result.Should().BeFalse();
        _mockSmsService.Verify(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnFalse_WhenMessageIsEmpty()
    {
        // Arrange
        var service = CreateServiceWithProvider();

        // Act
        var result = await service.SendSmsAsync("+14155552671", "");

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================================
    // SendBulkSmsAsync
    // =========================================================================

    [Fact]
    public async Task SendBulkSmsAsync_ShouldReturnResultForEachNumber()
    {
        // Arrange
        _mockSmsService.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateServiceWithProvider();
        var numbers = new[] { "+14155552671", "+447911123456" };

        // Act
        var results = await service.SendBulkSmsAsync(numbers, "Bulk test");

        // Assert
        results.Should().HaveCount(2);
        results.Values.Should().AllSatisfy(v => v.Should().BeTrue());
    }

    [Fact]
    public async Task SendBulkSmsAsync_ShouldSkipInvalidNumbers()
    {
        // Arrange
        _mockSmsService.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateServiceWithProvider();
        var numbers = new[] { "+14155552671", "bad-number" };

        // Act
        var results = await service.SendBulkSmsAsync(numbers, "Test");

        // Assert
        results["+14155552671"].Should().BeTrue();
        results["bad-number"].Should().BeFalse();
    }

    // =========================================================================
    // GetRemainingQuotaAsync
    // =========================================================================

    [Fact]
    public async Task GetRemainingQuotaAsync_ShouldReturnNegativeOne_WhenQuotaNotTracked()
    {
        // Arrange
        var service = CreateServiceWithoutProvider();

        // Act
        var result = await service.GetRemainingQuotaAsync();

        // Assert
        result.Should().Be(-1);
    }
}
