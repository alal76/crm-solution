// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Options;
using CRM.Infrastructure.Services.Auth;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for BiometricAuthService (BACK-003).
/// Verifies registration options, user validation, and credential exclusion.
/// </summary>
public class BiometricAuthServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDb;
    private readonly Mock<IWebAuthnService> _mockWebAuthn;
    private readonly Mock<ILogger<BiometricAuthService>> _mockLogger;
    private readonly WebAuthnOptions _webAuthnOptions;

    public BiometricAuthServiceTests()
    {
        _mockDb = new Mock<ICrmDbContext>();
        _mockWebAuthn = new Mock<IWebAuthnService>();
        _mockLogger = new Mock<ILogger<BiometricAuthService>>();

        _webAuthnOptions = new WebAuthnOptions
        {
            RelyingPartyId = "localhost",
            RelyingPartyName = "CRM Solution",
            TimeoutSeconds = 60,
            AttestationConveyance = "direct",
            UserVerificationPreference = "required",
            ChallengeExpirationMinutes = 10
        };
    }

    private BiometricAuthService CreateService()
    {
        return new BiometricAuthService(
            _mockDb.Object,
            _mockWebAuthn.Object,
            Options.Create(_webAuthnOptions),
            _mockLogger.Object);
    }

    // ─── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        // Act
        var svc = CreateService();

        // Assert
        svc.Should().NotBeNull();
    }

    // ─── GetRegistrationOptionsAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetRegistrationOptionsAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var emptyUsers = new List<User>();
        var mockUsersSet = MockDbSetFactory.CreateMockDbSet(emptyUsers);
        _mockDb.Setup(d => d.Users).Returns(mockUsersSet.Object);

        var emptyCredentials = new List<WebAuthnCredential>();
        var mockCredSet = MockDbSetFactory.CreateMockDbSet(emptyCredentials);
        _mockDb.Setup(d => d.WebAuthnCredentials).Returns(mockCredSet.Object);

        var svc = CreateService();

        // Act & Assert
        var act = async () => await svc.GetRegistrationOptionsAsync(userId: 9999);
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*9999*");
    }

    [Fact]
    public async Task GetRegistrationOptionsAsync_ShouldReturnOptions_WhenUserExists()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Email = "alice@example.com", FirstName = "Alice", LastName = "Smith" }
        };
        var mockUsersSet = MockDbSetFactory.CreateMockDbSet(users);
        _mockDb.Setup(d => d.Users).Returns(mockUsersSet.Object);

        var credentials = new List<WebAuthnCredential>();
        var mockCredSet = MockDbSetFactory.CreateMockDbSet(credentials);
        _mockDb.Setup(d => d.WebAuthnCredentials).Returns(mockCredSet.Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetRegistrationOptionsAsync(userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("1");
        result.UserName.Should().Be("alice@example.com");
        result.RelyingPartyId.Should().Be(_webAuthnOptions.RelyingPartyId);
        result.RelyingPartyName.Should().Be(_webAuthnOptions.RelyingPartyName);
        result.AuthenticatorAttachment.Should().Be("platform");
    }

    [Fact]
    public async Task GetRegistrationOptionsAsync_ShouldIncludeUserDisplayName_WhenFirstNameAvailable()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 2, Email = "bob@example.com", FirstName = "Bob", LastName = "Jones" }
        };
        _mockDb.Setup(d => d.Users)
               .Returns(MockDbSetFactory.CreateMockDbSet(users).Object);
        _mockDb.Setup(d => d.WebAuthnCredentials)
               .Returns(MockDbSetFactory.CreateMockDbSet(new List<WebAuthnCredential>()).Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetRegistrationOptionsAsync(userId: 2);

        // Assert
        result.UserDisplayName.Should().Be("Bob Jones");
    }

    [Fact]
    public async Task GetRegistrationOptionsAsync_ShouldExcludeExistingCredentials_WhenCredentialsRegistered()
    {
        // Arrange
        const int userId = 3;
        var users = new List<User>
        {
            new() { Id = userId, Email = "charlie@example.com", FirstName = "Charlie" }
        };
        _mockDb.Setup(d => d.Users)
               .Returns(MockDbSetFactory.CreateMockDbSet(users).Object);

        const string existingCredentialId = "existing-cred-id-abc123";
        var credentials = new List<WebAuthnCredential>
        {
            new() { Id = 10, UserId = userId, CredentialId = existingCredentialId, IsRevoked = false }
        };
        _mockDb.Setup(d => d.WebAuthnCredentials)
               .Returns(MockDbSetFactory.CreateMockDbSet(credentials).Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetRegistrationOptionsAsync(userId);

        // Assert
        result.ExcludeCredentials.Should().Contain(existingCredentialId);
    }

    [Fact]
    public async Task GetRegistrationOptionsAsync_ShouldNotExcludeRevokedCredentials_WhenCredentialIsRevoked()
    {
        // Arrange
        const int userId = 4;
        var users = new List<User>
        {
            new() { Id = userId, Email = "dave@example.com" }
        };
        _mockDb.Setup(d => d.Users)
               .Returns(MockDbSetFactory.CreateMockDbSet(users).Object);

        const string revokedCredId = "revoked-cred-id";
        var credentials = new List<WebAuthnCredential>
        {
            new() { Id = 20, UserId = userId, CredentialId = revokedCredId, IsRevoked = true }
        };
        _mockDb.Setup(d => d.WebAuthnCredentials)
               .Returns(MockDbSetFactory.CreateMockDbSet(credentials).Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetRegistrationOptionsAsync(userId);

        // Assert — revoked credentials should NOT be in the exclude list
        result.ExcludeCredentials.Should().NotContain(revokedCredId);
    }

    [Fact]
    public async Task GetRegistrationOptionsAsync_ShouldUseConfiguredTimeout_WhenOptionsSet()
    {
        // Arrange
        const int userId = 5;
        _webAuthnOptions.TimeoutSeconds = 120;

        var users = new List<User> { new() { Id = userId, Email = "eve@example.com" } };
        _mockDb.Setup(d => d.Users)
               .Returns(MockDbSetFactory.CreateMockDbSet(users).Object);
        _mockDb.Setup(d => d.WebAuthnCredentials)
               .Returns(MockDbSetFactory.CreateMockDbSet(new List<WebAuthnCredential>()).Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetRegistrationOptionsAsync(userId);

        // Assert
        result.TimeoutMs.Should().Be(120 * 1000);
    }
}
