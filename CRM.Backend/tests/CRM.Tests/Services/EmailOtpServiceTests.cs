// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EmailOtpService (TCOV-010).
/// EmailOtpService creates a SendGridClient in the constructor — a non-empty API key is required.
/// </summary>
public class EmailOtpServiceTests
{
    private static readonly EmailOtpSettings ValidSettings = new()
    {
        SendGridApiKey = "SG.fake-test-api-key-for-unit-tests",
        FromAddress = "noreply@test.com",
        OtpExpirationSeconds = 300,
        MaxAttempts = 5,
        MaxEmailsPerHour = 5
    };

    private static IOptions<EmailOtpSettings> OptionsFor(EmailOtpSettings s) => Options.Create(s);

    private readonly Mock<ILogger<EmailOtpService>> _mockLogger = new();

    [Fact]
    public void Constructor_ShouldCreateService_WhenValidSettings()
    {
        var svc = new EmailOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        svc.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenApiKeyIsEmpty()
    {
        var settings = new EmailOtpSettings { SendGridApiKey = string.Empty };
        var act = () => new EmailOtpService(OptionsFor(settings), _mockLogger.Object);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task VerifyOtpAsync_ShouldReturnFalse_WhenNoOtpStored()
    {
        var svc = new EmailOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.VerifyOtpAsync("user@example.com", "123456", 1);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_ShouldReturnFalse_WhenCodeIsEmpty()
    {
        var svc = new EmailOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.VerifyOtpAsync("user@example.com", string.Empty, 1);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOtpValidAsync_ShouldReturnFalse_WhenNoOtpStored()
    {
        var svc = new EmailOtpService(OptionsFor(ValidSettings), _mockLogger.Object);
        var result = await svc.IsOtpValidAsync("user@example.com", 1);
        result.Should().BeFalse();
    }
}
