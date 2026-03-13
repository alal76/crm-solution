// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Threading.Tasks;
using CRM.Core.Options;
using CRM.Infrastructure.Services.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services.Authentication;

/// <summary>
/// Unit tests for <see cref="TotpService"/> (Authentication namespace).
/// Tests RFC 6238 TOTP setup, verification, and backup-code generation.
/// </summary>
public class TotpServiceAuthTests
{
    private static TotpService BuildService(TotpOptions? options = null)
    {
        var opts = Options.Create(options ?? new TotpOptions
        {
            IssuerName = "CRM Test",
            SetupExpirationMinutes = 10,
            BackupCodeCount = 5
        });
        return new TotpService(opts, Mock.Of<ILogger<TotpService>>());
    }

    // ------------------------------------------------------------------
    // InitializeSetupAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task InitializeSetupAsync_ReturnsNonEmptySecret()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.InitializeSetupAsync(userId: 1, userEmail: "user@test.com");

        // Assert
        result.Secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InitializeSetupAsync_ReturnsOtpauthQrCodeUrl()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.InitializeSetupAsync(userId: 1, userEmail: "user@test.com");

        // Assert
        result.QrCodeUrl.Should().StartWith("otpauth://");
        result.QrCodeUrl.Should().Contain("user%40test.com");
    }

    [Fact]
    public async Task InitializeSetupAsync_ReturnsManualEntryKey()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.InitializeSetupAsync(userId: 1, userEmail: "user@test.com");

        // Assert
        result.ManualEntryKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InitializeSetupAsync_ExpiresAtIsInFuture()
    {
        // Arrange
        var service = BuildService(new TotpOptions { SetupExpirationMinutes = 10 });

        // Act
        var result = await service.InitializeSetupAsync(userId: 42, userEmail: "a@b.com");

        // Assert — expiry should be ~10 minutes from now
        result.ExpiresAt.Should().BeAfter(System.DateTime.UtcNow);
        result.ExpiresAt.Should().BeBefore(System.DateTime.UtcNow.AddMinutes(15));
    }

    [Fact]
    public async Task InitializeSetupAsync_TwoCallsProduceDifferentSecrets()
    {
        // Arrange
        var service = BuildService();

        // Act
        var r1 = await service.InitializeSetupAsync(1, "a@test.com");
        var r2 = await service.InitializeSetupAsync(1, "a@test.com");

        // Assert — secrets are randomly generated every time
        r1.Secret.Should().NotBe(r2.Secret);
    }

    // ------------------------------------------------------------------
    // VerifySetupAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task VerifySetupAsync_WithEmptyTotp_ReturnsFalse()
    {
        // Arrange
        var service = BuildService();
        var setup = await service.InitializeSetupAsync(1, "u@test.com");

        // Act
        var result = await service.VerifySetupAsync(1, string.Empty, setup.Secret);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifySetupAsync_WithEmptySecret_ReturnsFalse()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.VerifySetupAsync(1, "123456", string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifySetupAsync_WithWrongCode_ReturnsFalse()
    {
        // Arrange
        var service = BuildService();
        var setup = await service.InitializeSetupAsync(1, "u@test.com");

        // Act — "000000" is almost certainly wrong for any valid secret
        var result = await service.VerifySetupAsync(1, "000000", setup.Secret);

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------------
    // CompleteSetupAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task CompleteSetupAsync_WithValidSecret_ReturnsBackupCodes()
    {
        // Arrange
        var service = BuildService(new TotpOptions { BackupCodeCount = 5 });
        var setup = await service.InitializeSetupAsync(1, "u@test.com");

        // Act
        var result = await service.CompleteSetupAsync(1, setup.Secret);

        // Assert
        result.Should().NotBeNull();
        result.TotalCodes.Should().Be(5);
        result.Codes.Should().HaveCount(5);
    }

    [Fact]
    public async Task CompleteSetupAsync_EachBackupCodeHasCorrectLength()
    {
        // Arrange
        var service = BuildService(new TotpOptions { BackupCodeCount = 3 });
        var setup = await service.InitializeSetupAsync(1, "u@test.com");

        // Act
        var result = await service.CompleteSetupAsync(1, setup.Secret);

        // Assert — backup codes are 8 characters per the const BackupCodeLength = 8
        foreach (var code in result.Codes)
        {
            code.Should().HaveLength(8);
        }
    }

    // ------------------------------------------------------------------
    // VerifyAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task VerifyAsync_WithIncorrectLengthCode_ReturnsInvalid()
    {
        // Arrange
        var service = BuildService();

        // Act — 5 digits instead of required 6
        var result = await service.VerifyAsync(1, "12345");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyAsync_WithEmptyCode_ReturnsInvalid()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.VerifyAsync(1, string.Empty);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
