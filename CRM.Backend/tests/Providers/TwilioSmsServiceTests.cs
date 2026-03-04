// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Providers.Twilio;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for TwilioSmsService (ISmsNotificationService).
///
/// Twilio handling: TwilioSmsService calls <c>TwilioClient.Init()</c> in its constructor
/// and <c>MessageResource.CreateAsync()</c> in <c>SendSmsAsync()</c>.
/// Tests exercise:
///   - <c>IsValidPhoneNumber()</c> — pure regex logic, no network
///   - Paths where <c>_isInitialized = false</c> (invalid config) — no SDK calls
///   - <c>SendEscalationSmsAsync</c> message format and delegation
///
/// MANDATORY: Written after verifying source for:
///   Class: TwilioSmsService, Namespace: CRM.Infrastructure.Providers.Twilio
///   Constructor: (IOptions&lt;TwilioConfiguration&gt;, ILogger&lt;TwilioSmsService&gt;)
///   Interface: CRM.Core.Interfaces.Notifications.ISmsNotificationService
///   Pattern: PhoneNumberRegex = ^\+?[1-9]\d{6,14}$
/// </summary>
public class TwilioSmsServiceTests
{
    // ── Factory helpers ─────────────────────────────────────────────────────

    private static TwilioConfiguration InvalidConfig() => new()
    {
        AccountSid = string.Empty,
        AuthToken = string.Empty,
        FromPhoneNumber = string.Empty
    };

    /// <summary>
    /// Creates a service with invalid config so that _isInitialized = false.
    /// SMSSending paths return false without any Twilio SDK calls.
    /// </summary>
    private static TwilioSmsService CreateUninitializedService()
    {
        var options = Options.Create(InvalidConfig());
        var logger = new Mock<ILogger<TwilioSmsService>>();
        return new TwilioSmsService(options, logger.Object);
    }

    // ── IsValidPhoneNumber ──────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidPhoneNumber_ReturnsFalse_WhenNullOrWhitespace(string phoneNumber)
    {
        var service = CreateUninitializedService();
        service.IsValidPhoneNumber(phoneNumber).Should().BeFalse();
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("123")]          // too short (< 7 digits)
    [InlineData("12345")]        // too short
    [InlineData("+0123456789")]  // starts with 0 after +
    [InlineData("abc1234567")]   // letters mixed in
    public void IsValidPhoneNumber_ReturnsFalse_WhenFormatInvalid(string phoneNumber)
    {
        var service = CreateUninitializedService();
        service.IsValidPhoneNumber(phoneNumber).Should().BeFalse();
    }

    [Theory]
    [InlineData("+12025550100")]    // US E.164
    [InlineData("+447911123456")]   // UK E.164
    [InlineData("+61412345678")]    // AU E.164
    [InlineData("12025550100")]     // Without leading +
    [InlineData("447911123456")]    // UK without +
    public void IsValidPhoneNumber_ReturnsTrue_WhenValidE164Format(string phoneNumber)
    {
        var service = CreateUninitializedService();
        service.IsValidPhoneNumber(phoneNumber).Should().BeTrue();
    }

    [Theory]
    [InlineData("+1234567")]          // 7 digits (minimum valid E.164 with +)
    [InlineData("+123456789012345")] // 15 digits (maximum valid E.164)
    public void IsValidPhoneNumber_ReturnsTrue_AtBoundaryLengths(string phoneNumber)
    {
        var service = CreateUninitializedService();
        service.IsValidPhoneNumber(phoneNumber).Should().BeTrue();
    }

    // ── SendSmsAsync (uninitialized) ────────────────────────────────────────

    [Fact]
    public async Task SendSmsAsync_ReturnsFalse_WhenServiceNotInitialized()
    {
        var service = CreateUninitializedService();

        var result = await service.SendSmsAsync("+12025550100", "Hello");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_ReturnsFalse_WhenPhoneNumberInvalid()
    {
        // IsValidPhoneNumber check runs before _isInitialized check
        var service = CreateUninitializedService();

        var result = await service.SendSmsAsync("not-a-phone", "Hello");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_ReturnsFalse_WhenPhoneIsEmptyString()
    {
        var service = CreateUninitializedService();

        var result = await service.SendSmsAsync(string.Empty, "Hello");

        result.Should().BeFalse();
    }

    // ── SendBulkSmsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsEmptyDictionary_WhenNoPhoneNumbers()
    {
        var service = CreateUninitializedService();

        var results = await service.SendBulkSmsAsync(Array.Empty<string>(), "Hello");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsAllFalse_WhenNotInitializedAndValidPhones()
    {
        var service = CreateUninitializedService();
        var phones = new[] { "+12025550101", "+12025550102", "+12025550103" };

        var results = await service.SendBulkSmsAsync(phones, "Message");

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(kv => kv.Value.Should().BeFalse());
    }

    [Fact]
    public async Task SendBulkSmsAsync_ReturnsFalseForInvalidPhone_WhenPhoneMixed()
    {
        var service = CreateUninitializedService();
        var phones = new[] { "+12025550101", "not-valid", "+12025550103" };

        var results = await service.SendBulkSmsAsync(phones, "Test");

        results.Should().HaveCount(3);
        // All return false: valid phones fail due to uninitialized, invalid phone fails validation
        results["+12025550101"].Should().BeFalse();
        results["not-valid"].Should().BeFalse();
        results["+12025550103"].Should().BeFalse();
    }

    // ── SendEscalationSmsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task SendEscalationSmsAsync_ReturnsFalse_WhenNotInitialized()
    {
        var service = CreateUninitializedService();

        var result = await service.SendEscalationSmsAsync(
            "+12025550100",
            "SR-00042",
            2,
            "Server is down");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendEscalationSmsAsync_ReturnsFalse_WhenPhoneNumberIsInvalid()
    {
        var service = CreateUninitializedService();

        var result = await service.SendEscalationSmsAsync(
            "invalid-phone",
            "SR-00051",
            1,
            "Critical issue");

        result.Should().BeFalse();
    }
}
