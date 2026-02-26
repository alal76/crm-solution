// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace CRM.Tests.Unit.Services;

/// <summary>
/// Unit tests for <see cref="PortalAuthService"/> — 9 scenarios.
///
/// Uses an EF Core InMemory database so real DbSet/SaveChanges semantics work.
/// IConfiguration is stubbed via NSubstitute with JWT defaults.
/// BCrypt.Net.BCrypt is used directly for producing test password hashes.
/// </summary>
public sealed class PortalAuthServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly PortalAuthService _sut;

    private const string TestPassword = "Test@Password1";

    public PortalAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var config = Substitute.For<IConfiguration>();
        // Configure JWT keys so GeneratePortalToken works
        config["Jwt:Secret"].Returns("test-secret-key-at-least-32-characters-long!");
        config["Jwt:Issuer"].Returns("TestIssuer");
        config["Jwt:Audience"].Returns("TestAudience");
        config["Jwt:ExpirationMinutes"].Returns("60");

        _context = new CrmDbContext(options, config);

        _sut = new PortalAuthService(
            _context,
            NullLogger<PortalAuthService>.Instance,
            config);
    }

    public void Dispose() => _context.Dispose();

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private PortalUser AddActiveUser(
        string email = "user@portal.test",
        string? password = null,
        bool isActive = true)
    {
        var now = DateTime.UtcNow;
        var user = new PortalUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password ?? TestPassword),
            DisplayName = "Test User",
            IsActive = isActive,
            IsEmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.PortalUsers.Add(user);
        _context.SaveChanges();
        return user;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Login
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        AddActiveUser();

        var result = await _sut.LoginAsync(new PortalLoginDto { Email = "user@portal.test", Password = TestPassword });

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be("user@portal.test");
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenUserNotFound()
    {
        var result = await _sut.LoginAsync(new PortalLoginDto { Email = "nobody@portal.test", Password = TestPassword });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenPasswordIsWrong()
    {
        AddActiveUser();

        var result = await _sut.LoginAsync(new PortalLoginDto { Email = "user@portal.test", Password = "WrongPassword!" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenAccountIsInactive()
    {
        AddActiveUser(isActive: false);

        var result = await _sut.LoginAsync(new PortalLoginDto { Email = "user@portal.test", Password = TestPassword });

        result.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Register
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ShouldCreateUser_WhenEmailIsNew()
    {
        // No portal config — registration defaults to open
        var result = await _sut.RegisterAsync(new PortalRegisterDto
        {
            Email = "new@portal.test",
            Password = TestPassword,
            DisplayName = "New User",
        });

        result.Should().NotBeNull();
        result.Email.Should().Be("new@portal.test");
        result.IsActive.Should().BeTrue();

        var persisted = await _context.PortalUsers.FirstOrDefaultAsync(u => u.Email == "new@portal.test");
        persisted.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify(TestPassword, persisted!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailAlreadyExists()
    {
        AddActiveUser(email: "dup@portal.test");

        Func<Task> act = async () => await _sut.RegisterAsync(new PortalRegisterDto
        {
            Email = "dup@portal.test",
            Password = TestPassword,
            DisplayName = "Dup User",
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ForgotPassword
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_ShouldSetResetToken_AndReturnTrue_WhenUserExists()
    {
        AddActiveUser(email: "forgot@portal.test");

        var result = await _sut.ForgotPasswordAsync("forgot@portal.test");

        result.Should().BeTrue();
        var user = await _context.PortalUsers.FirstAsync(u => u.Email == "forgot@portal.test");
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ResetPassword
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ShouldUpdateHash_AndReturnTrue_WhenTokenIsValid()
    {
        var user = AddActiveUser(email: "reset@portal.test");
        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        var result = await _sut.ResetPasswordAsync(resetToken, "NewPass@456");

        result.Should().BeTrue();
        var updated = await _context.PortalUsers.FirstAsync(u => u.Email == "reset@portal.test");
        BCrypt.Net.BCrypt.Verify("NewPass@456", updated.PasswordHash).Should().BeTrue();
        updated.PasswordResetToken.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnFalse_WhenTokenIsExpired()
    {
        var user = AddActiveUser(email: "expired@portal.test");
        var expiredToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = expiredToken;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(-1); // expired
        await _context.SaveChangesAsync();

        var result = await _sut.ResetPasswordAsync(expiredToken, "NewPass@456");

        result.Should().BeFalse();
    }
}
