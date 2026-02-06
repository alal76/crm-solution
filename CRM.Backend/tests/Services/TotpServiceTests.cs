// CRM Solution - Customer Relationship Management System
// TOTP (Two-Factor Authentication) Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for TotpService
/// Covers: TOTP generation, validation, setup, backup codes
/// </summary>
public class TotpServiceTests
{
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<ILogger<TotpService>> _mockLogger;
    private readonly TotpService _service;

    public TotpServiceTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockLogger = new Mock<ILogger<TotpService>>();

        _service = new TotpService(
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    #region Secret Generation Tests

    [Fact]
    public void GenerateSecret_ReturnsNonEmptySecret()
    {
        // Act
        var secret = _service.GenerateSecret();

        // Assert
        secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSecret_ReturnsBase32EncodedSecret()
    {
        // Act
        var secret = _service.GenerateSecret();

        // Assert
        secret.Should().MatchRegex("^[A-Z2-7]+$");
    }

    [Fact]
    public void GenerateSecret_ReturnsUniqueSecrets()
    {
        // Act
        var secret1 = _service.GenerateSecret();
        var secret2 = _service.GenerateSecret();
        var secret3 = _service.GenerateSecret();

        // Assert
        secret1.Should().NotBe(secret2);
        secret2.Should().NotBe(secret3);
        secret1.Should().NotBe(secret3);
    }

    [Fact]
    public void GenerateSecret_ReturnsSufficientLength()
    {
        // Act
        var secret = _service.GenerateSecret();

        // Assert
        secret.Length.Should().BeGreaterThanOrEqualTo(16);
    }

    #endregion

    #region QR Code Generation Tests

    [Fact]
    public void GenerateQrCodeUri_ValidInput_ReturnsUri()
    {
        // Arrange
        var email = "user@example.com";
        var secret = _service.GenerateSecret();

        // Act
        var uri = _service.GenerateQrCodeUri(email, secret);

        // Assert
        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain(email);
    }

    [Fact]
    public void GenerateQrCodeUri_IncludesIssuer()
    {
        // Arrange
        var email = "user@example.com";
        var secret = _service.GenerateSecret();

        // Act
        var uri = _service.GenerateQrCodeUri(email, secret, "CRM App");

        // Assert
        uri.Should().Contain("issuer=CRM%20App");
    }

    [Fact]
    public void GenerateQrCodeUri_NullEmail_ThrowsArgumentNullException()
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GenerateQrCodeUri(null!, secret));
    }

    [Fact]
    public void GenerateQrCodeUri_NullSecret_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GenerateQrCodeUri("user@example.com", null!));
    }

    #endregion

    #region TOTP Validation Tests

    [Fact]
    public void ValidateTotp_ValidCode_ReturnsTrue()
    {
        // Arrange
        var secret = _service.GenerateSecret();
        var code = _service.GenerateCode(secret);

        // Act
        var isValid = _service.ValidateCode(secret, code);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateTotp_InvalidCode_ReturnsFalse()
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act
        var isValid = _service.ValidateCode(secret, "000000");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateTotp_EmptyCode_ReturnsFalse()
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act
        var isValid = _service.ValidateCode(secret, "");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateTotp_NullCode_ReturnsFalse()
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act
        var isValid = _service.ValidateCode(secret, null!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateTotp_WithWindowTolerance_AcceptsNearCodes()
    {
        // Arrange
        var secret = _service.GenerateSecret();
        var code = _service.GenerateCode(secret);

        // Act - Use window tolerance
        var isValid = _service.ValidateCode(secret, code, windowSize: 2);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("12345")]      // Too short
    [InlineData("1234567")]    // Too long
    [InlineData("abcdef")]     // Not numeric
    [InlineData("12345a")]     // Mixed
    public void ValidateTotp_InvalidFormat_ReturnsFalse(string code)
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act
        var isValid = _service.ValidateCode(secret, code);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Code Generation Tests

    [Fact]
    public void GenerateCode_ValidSecret_ReturnsSixDigitCode()
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act
        var code = _service.GenerateCode(secret);

        // Assert
        code.Should().HaveLength(6);
        code.Should().MatchRegex("^[0-9]{6}$");
    }

    [Fact]
    public void GenerateCode_SameSecret_ReturnsSameCode()
    {
        // Arrange
        var secret = _service.GenerateSecret();

        // Act
        var code1 = _service.GenerateCode(secret);
        var code2 = _service.GenerateCode(secret);

        // Assert - Should be same within same time window
        code1.Should().Be(code2);
    }

    [Fact]
    public void GenerateCode_NullSecret_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GenerateCode(null!));
    }

    #endregion

    #region Backup Codes Tests

    [Fact]
    public void GenerateBackupCodes_ReturnsExpectedCount()
    {
        // Act
        var codes = _service.GenerateBackupCodes(10);

        // Assert
        codes.Should().HaveCount(10);
    }

    [Fact]
    public void GenerateBackupCodes_ReturnsUniqueCodes()
    {
        // Act
        var codes = _service.GenerateBackupCodes(10);

        // Assert
        codes.Distinct().Should().HaveCount(10);
    }

    [Fact]
    public void GenerateBackupCodes_ReturnsCorrectFormat()
    {
        // Act
        var codes = _service.GenerateBackupCodes(10);

        // Assert
        foreach (var code in codes)
        {
            code.Should().MatchRegex("^[A-Z0-9]{8}$|^[a-z0-9]{8}$|^[a-z0-9]{4}-[a-z0-9]{4}$");
        }
    }

    [Fact]
    public void GenerateBackupCodes_DefaultCount_Returns10Codes()
    {
        // Act
        var codes = _service.GenerateBackupCodes();

        // Assert
        codes.Should().HaveCount(10);
    }

    [Fact]
    public void GenerateBackupCodes_ZeroCount_ReturnsEmptyList()
    {
        // Act
        var codes = _service.GenerateBackupCodes(0);

        // Assert
        codes.Should().BeEmpty();
    }

    #endregion

    #region User TOTP Setup Tests

    [Fact]
    public async Task EnableTwoFactorAsync_ValidUser_ReturnsSetupInfo()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            TwoFactorEnabled = false
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.EnableTwoFactorAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Secret.Should().NotBeNullOrEmpty();
        result.QrCodeUri.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EnableTwoFactorAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.EnableTwoFactorAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmTwoFactorAsync_ValidCode_ReturnsTrue()
    {
        // Arrange
        var secret = _service.GenerateSecret();
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            TwoFactorEnabled = false,
            TwoFactorSecret = secret
        };

        var code = _service.GenerateCode(secret);

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.TwoFactorEnabled = true; return u; });

        // Act
        var result = await _service.ConfirmTwoFactorAsync(1, code);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmTwoFactorAsync_InvalidCode_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            TwoFactorSecret = _service.GenerateSecret()
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ConfirmTwoFactorAsync(1, "000000");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DisableTwoFactorAsync_EnabledUser_DisablesTwoFactor()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            TwoFactorEnabled = true,
            TwoFactorSecret = "SECRET"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.DisableTwoFactorAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DisableTwoFactorAsync_NotEnabled_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = 1, TwoFactorEnabled = false };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.DisableTwoFactorAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Backup Code Validation Tests

    [Fact]
    public async Task ValidateBackupCodeAsync_ValidCode_ReturnsTrue()
    {
        // Arrange
        var codes = _service.GenerateBackupCodes(10);
        var user = new User
        {
            Id = 1,
            TwoFactorEnabled = true,
            BackupCodes = string.Join(",", codes)
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.ValidateBackupCodeAsync(1, codes[0]);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateBackupCodeAsync_InvalidCode_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            TwoFactorEnabled = true,
            BackupCodes = "CODE1,CODE2,CODE3"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ValidateBackupCodeAsync(1, "INVALID");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateBackupCodeAsync_UsedCode_RemovesFromList()
    {
        // Arrange
        var codes = new[] { "CODE1", "CODE2", "CODE3" };
        var user = new User
        {
            Id = 1,
            TwoFactorEnabled = true,
            BackupCodes = string.Join(",", codes)
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        await _service.ValidateBackupCodeAsync(1, "CODE1");

        // Assert
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            !u.BackupCodes!.Contains("CODE1"))), Times.Once);
    }

    [Fact]
    public async Task RegenerateBackupCodesAsync_ValidUser_ReturnsNewCodes()
    {
        // Arrange
        var user = new User { Id = 1, TwoFactorEnabled = true, BackupCodes = "OLD,CODES" };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.RegenerateBackupCodesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(10);
    }

    #endregion

    #region Check Status Tests

    [Fact]
    public async Task IsTwoFactorEnabledAsync_EnabledUser_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = 1, TwoFactorEnabled = true };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsTwoFactorEnabledAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_DisabledUser_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = 1, TwoFactorEnabled = false };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsTwoFactorEnabledAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetBackupCodesCountAsync_ReturnsCount()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            BackupCodes = "CODE1,CODE2,CODE3"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetBackupCodesCountAsync(1);

        // Assert
        result.Should().Be(3);
    }

    #endregion
}
