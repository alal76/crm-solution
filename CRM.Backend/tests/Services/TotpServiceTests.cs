// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for TotpService.
/// TotpService is a stateless, parameterless utility class implementing ITotpService.
/// Real API: GenerateSecret(), VerifyCode(secret, code), GetQrCodeUrl(secret, email, issuer),
///           GenerateBackupCodes(count = 10).
/// </summary>
public class TotpServiceTests
{
    private readonly TotpService _service;

    public TotpServiceTests()
    {
        // TotpService is parameterless — no mocks needed
        _service = new TotpService();
    }

    #region GenerateSecret Tests

    [Fact]
    public void GenerateSecret_ReturnsNonEmptyString()
    {
        var secret = _service.GenerateSecret();
        secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSecret_ReturnsValidBase64()
    {
        var secret = _service.GenerateSecret();

        // Should not throw — valid Base64
        var bytes = Convert.FromBase64String(secret);
        bytes.Should().NotBeNull();
    }

    [Fact]
    public void GenerateSecret_Produces32RandomBytes()
    {
        var secret = _service.GenerateSecret();
        var bytes = Convert.FromBase64String(secret);

        // 32 bytes source → 44 Base64 characters (with padding)
        bytes.Should().HaveCount(32);
    }

    [Fact]
    public void GenerateSecret_ReturnsUniqueValues()
    {
        var secret1 = _service.GenerateSecret();
        var secret2 = _service.GenerateSecret();
        var secret3 = _service.GenerateSecret();

        secret1.Should().NotBe(secret2);
        secret2.Should().NotBe(secret3);
        secret1.Should().NotBe(secret3);
    }

    #endregion

    #region VerifyCode Tests

    [Fact]
    public void VerifyCode_NullSecret_ReturnsFalse()
    {
        var result = _service.VerifyCode(null!, "123456");
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyCode_EmptySecret_ReturnsFalse()
    {
        var result = _service.VerifyCode("", "123456");
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyCode_NullCode_ReturnsFalse()
    {
        var secret = _service.GenerateSecret();
        var result = _service.VerifyCode(secret, null!);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyCode_EmptyCode_ReturnsFalse()
    {
        var secret = _service.GenerateSecret();
        var result = _service.VerifyCode(secret, "");
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345")]      // Too short (5 digits)
    [InlineData("1234567")]    // Too long (7 digits)
    [InlineData("abcdef")]     // Non-numeric
    [InlineData("12345a")]     // Mixed alpha-numeric
    public void VerifyCode_InvalidFormat_ReturnsFalse(string code)
    {
        var secret = _service.GenerateSecret();
        var result = _service.VerifyCode(secret, code);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyCode_CorrectCurrentCode_ReturnsTrue()
    {
        // Generate a secret and compute the current TOTP manually
        var secret = _service.GenerateSecret();
        var currentCode = ComputeTotp(secret, DateTimeOffset.UtcNow);

        var result = _service.VerifyCode(secret, currentCode);
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_RandomWrongCode_ReturnsFalse()
    {
        var secret = _service.GenerateSecret();

        // Try several random 6-digit codes; at least one should fail.
        // The current window covers 3 possible codes max, so "999999" is overwhelmingly likely to be invalid.
        var result = _service.VerifyCode(secret, "999999");
        // Note: There is a ~0.0003% chance this is actually the current code.
        // If flaky, use a deterministic approach. For practical purposes this is fine.
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyCode_SameSecretAndCode_IsConsistentWithinWindow()
    {
        var secret = _service.GenerateSecret();
        var code = ComputeTotp(secret, DateTimeOffset.UtcNow);

        // Calling verify twice in the same time window should return the same result
        var result1 = _service.VerifyCode(secret, code);
        var result2 = _service.VerifyCode(secret, code);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    #endregion

    #region GetQrCodeUrl Tests

    [Fact]
    public void GetQrCodeUrl_ReturnsOtpauthScheme()
    {
        var secret = _service.GenerateSecret();
        var url = _service.GetQrCodeUrl(secret, "user@example.com", "CRM");

        url.Should().StartWith("otpauth://totp/");
    }

    [Fact]
    public void GetQrCodeUrl_ContainsSecret()
    {
        var secret = _service.GenerateSecret();
        var url = _service.GetQrCodeUrl(secret, "user@example.com", "CRM");

        // The secret should appear URI-escaped in the URL
        var encodedSecret = Uri.EscapeDataString(secret);
        url.Should().Contain($"secret={encodedSecret}");
    }

    [Fact]
    public void GetQrCodeUrl_ContainsEmail()
    {
        var secret = _service.GenerateSecret();
        var email = "user@example.com";
        var url = _service.GetQrCodeUrl(secret, email, "CRM");

        var encodedEmail = Uri.EscapeDataString(email);
        url.Should().Contain(encodedEmail);
    }

    [Fact]
    public void GetQrCodeUrl_ContainsIssuer()
    {
        var secret = _service.GenerateSecret();
        var url = _service.GetQrCodeUrl(secret, "user@example.com", "CRM App");

        var encodedIssuer = Uri.EscapeDataString("CRM App");
        url.Should().Contain($"issuer={encodedIssuer}");
    }

    [Fact]
    public void GetQrCodeUrl_MatchesExpectedFormat()
    {
        var secret = _service.GenerateSecret();
        var email = "test@crm.local";
        var issuer = "MyCRM";

        var url = _service.GetQrCodeUrl(secret, email, issuer);

        var expected = $"otpauth://totp/{Uri.EscapeDataString(email)}?secret={Uri.EscapeDataString(secret)}&issuer={Uri.EscapeDataString(issuer)}";
        url.Should().Be(expected);
    }

    #endregion

    #region GenerateBackupCodes Tests

    [Fact]
    public void GenerateBackupCodes_DefaultCount_Returns10Codes()
    {
        var codes = _service.GenerateBackupCodes();
        codes.Should().HaveCount(10);
    }

    [Fact]
    public void GenerateBackupCodes_CustomCount_ReturnsRequestedNumber()
    {
        var codes = _service.GenerateBackupCodes(5);
        codes.Should().HaveCount(5);
    }

    [Fact]
    public void GenerateBackupCodes_ZeroCount_ReturnsEmptyList()
    {
        var codes = _service.GenerateBackupCodes(0);
        codes.Should().BeEmpty();
    }

    [Fact]
    public void GenerateBackupCodes_AllCodesAre16CharUppercaseHex()
    {
        var codes = _service.GenerateBackupCodes(10);

        foreach (var code in codes)
        {
            code.Should().HaveLength(16);
            // BitConverter.ToString produces uppercase hex
            code.Should().MatchRegex("^[0-9A-F]{16}$");
        }
    }

    [Fact]
    public void GenerateBackupCodes_AllCodesAreUnique()
    {
        var codes = _service.GenerateBackupCodes(20);
        codes.Distinct().Should().HaveCount(codes.Count);
    }

    [Fact]
    public void GenerateBackupCodes_ReturnsList()
    {
        var codes = _service.GenerateBackupCodes(3);
        codes.Should().BeOfType<List<string>>();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Computes the TOTP code for a given secret and time, matching TotpService's algorithm.
    /// Uses HMACSHA1, 30-second time step, 6-digit codes.
    /// </summary>
    private static string ComputeTotp(string base64Secret, DateTimeOffset time)
    {
        var secretBytes = Convert.FromBase64String(base64Secret);
        var unixTime = time.ToUnixTimeSeconds();
        var timeWindow = unixTime / 30; // 30-second time step

        using var hmac = new HMACSHA1(secretBytes);
        var timeBytes = BitConverter.GetBytes(timeWindow);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        var hash = hmac.ComputeHash(timeBytes);
        var offset = hash[hash.Length - 1] & 0x0f;
        var truncated = (hash[offset] & 0x7f) << 24
            | (hash[offset + 1] & 0xff) << 16
            | (hash[offset + 2] & 0xff) << 8
            | (hash[offset + 3] & 0xff);
        var totp = truncated % 1_000_000; // 10^6 for 6 digits

        return totp.ToString("D6"); // Zero-padded to 6 digits
    }

    #endregion
}
