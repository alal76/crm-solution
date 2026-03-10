// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SmsOtpService (TCOV-011).
/// SmsOtpService calls TwilioClient.Init() in constructor — dummy values are acceptable for unit tests.
/// </summary>
public class SmsOtpServiceTests
{
    private static readonly SmsOtpSettings ValidSettings = new()
    {
        AccountSid = "AC00000000000000000000000000000000",
        AuthToken = "auth_token_placeholder_for_testing",
        FromPhoneNumber = "+15005550006",
        OtpExpirationSeconds = 300,
        MaxAttempts = 5,
        MaxOtpsPerHour = 3
    };

    private static IOptions<SmsOtpSettings> OptionsFor(SmsOtpSettings s) => Options.Create(s);
    private readonly Mock<ILogger<SmsOtpService>> _mockLogger = new();

    [Fact]
    public void Constructor_ShouldCreateService_WhenValidSettings()
    {
        var svc = new SmsOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        svc.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyOtpAsync_ShouldReturnFalse_WhenNoOtpStored()
    {
        var svc = new SmsOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.VerifyOtpAsync("+15555551234", "123456", 1);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_ShouldReturnFalse_WhenCodeIsEmpty()
    {
        var svc = new SmsOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.VerifyOtpAsync("+15555551234", string.Empty, 1);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOtpValidAsync_ShouldReturnFalse_WhenNoRecordExists()
    {
        var svc = new SmsOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.IsOtpValidAsync("+15555551234", 1);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_ShouldReturnFalse_WhenPhoneNumberIsEmpty()
    {
        var svc = new SmsOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.VerifyOtpAsync(string.Empty, "123456", 1);
        result.Should().BeFalse();
    }
}
