// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services.Notifications;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for SmsNotificationService (stub implementation of ISmsNotificationService).
/// Exercises phone validation, single send, bulk send, and escalation SMS helpers.
/// TODO-SD005-009: SMS notification channel for escalations.
/// </summary>
public class SmsNotificationServiceTests
{
    private readonly SmsNotificationService _service;

    public SmsNotificationServiceTests()
    {
        var logger = new Mock<ILogger<SmsNotificationService>>().Object;
        _service = new SmsNotificationService(logger);
    }

    // -------------------------------------------------------------------------
    // IsValidPhoneNumber tests
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("+14155552671", true)]
    [InlineData("+447911123456", true)]
    [InlineData("14155552671", true)]   // optional leading +
    [InlineData("12345", false)]        // too short
    [InlineData("", false)]             // empty
    [InlineData("+00000000000", false)] // leading zero not allowed
    public void IsValidPhoneNumber_ShouldReturnExpected(string number, bool expected)
    {
        // Act
        var result = _service.IsValidPhoneNumber(number);

        // Assert
        result.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // SendSmsAsync tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendSmsAsync_ShouldReturnTrue_ForValidPhoneNumber()
    {
        // Act
        var result = await _service.SendSmsAsync("+14155552671", "Test message");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnFalse_ForInvalidPhoneNumber()
    {
        // Act
        var result = await _service.SendSmsAsync("invalid", "Test message");

        // Assert
        result.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // SendBulkSmsAsync tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendBulkSmsAsync_ShouldReturnResultForEachNumber()
    {
        // Arrange
        var numbers = new[] { "+14155552671", "+447911123456", "bad" };

        // Act
        var results = await _service.SendBulkSmsAsync(numbers, "Bulk test");

        // Assert
        results.Should().HaveCount(3);
        results["+14155552671"].Should().BeTrue();
        results["+447911123456"].Should().BeTrue();
        results["bad"].Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // SendEscalationSmsAsync tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendEscalationSmsAsync_ShouldReturnTrue_ForValidNumber()
    {
        // Act
        var result = await _service.SendEscalationSmsAsync(
            "+14155552671",
            "SR-001234",
            2,
            "Database connectivity failure affecting all users");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEscalationSmsAsync_ShouldReturnFalse_ForInvalidNumber()
    {
        // Act
        var result = await _service.SendEscalationSmsAsync(
            "not-a-number",
            "SR-001234",
            1,
            "Minor display issue");

        // Assert
        result.Should().BeFalse();
    }
}
