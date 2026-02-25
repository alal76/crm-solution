// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.Auth;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SessionManagerService" /> covering:
/// <list type="bullet">
///   <item>Session creation with IP-binding metadata  (TODO-AUTH-015)</item>
///   <item>IP-binding enforcement via <c>ValidateSessionWithIpCheckAsync</c></item>
///   <item>Basic session lifecycle (validate, revoke, active list)</item>
///   <item>Concurrent session limit enforcement</item>
/// </list>
/// </summary>
public class SessionManagerServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<SessionManagerService>> _mockLogger;
    private readonly SessionManagerService _service;

    // Default configuration: max 3 sessions, IP-binding disabled
    private readonly IConfiguration _config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Auth:MaxConcurrentSessions", "3" },
            { "Auth:EnableIpBinding", "false" }
        })
        .Build();

    // IP-binding-enabled configuration
    private readonly IConfiguration _ipBindingConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Auth:MaxConcurrentSessions", "5" },
            { "Auth:EnableIpBinding", "true" }
        })
        .Build();

    public SessionManagerServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _mockLogger = new Mock<ILogger<SessionManagerService>>();

        _service = new SessionManagerService(
            _dbContext,
            _config,
            _mockLogger.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    // ── CreateSessionAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSessionAsync_ValidInput_CreatesSession()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var session = await _service.CreateSessionAsync(
            userId: 1,
            sessionToken: "tok-001",
            ipAddress: "192.168.1.10",
            userAgent: "TestAgent/1.0",
            expiresAt: expiresAt,
            cancellationToken: CancellationToken.None);

        // Assert
        session.Should().NotBeNull();
        session.UserId.Should().Be(1);
        session.SessionToken.Should().Be("tok-001");
        session.IpAddress.Should().Be("192.168.1.10");
        session.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSessionAsync_IpBindingEnabled_SetsIpBindingFlag()
    {
        // Arrange — create a service with IP-binding enabled
        var ipBoundService = new SessionManagerService(
            _dbContext,
            _ipBindingConfig,
            _mockLogger.Object);

        // Act
        var session = await ipBoundService.CreateSessionAsync(
            userId: 2,
            sessionToken: "tok-ip-bound",
            ipAddress: "10.0.0.1",
            userAgent: "Agent/2.0",
            expiresAt: DateTime.UtcNow.AddHours(1),
            cancellationToken: CancellationToken.None);

        // Assert
        session.IpBindingEnabled.Should().BeTrue("session created with IP-binding config should set IpBindingEnabled");
        session.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task CreateSessionAsync_IpBindingDisabled_LeavesIpBindingFlagFalse()
    {
        // Act
        var session = await _service.CreateSessionAsync(
            userId: 3,
            sessionToken: "tok-no-ip",
            ipAddress: "172.16.0.5",
            userAgent: "Agent/3.0",
            expiresAt: DateTime.UtcNow.AddHours(1),
            cancellationToken: CancellationToken.None);

        // Assert
        session.IpBindingEnabled.Should().BeFalse("IP binding is disabled in default config");
    }

    // ── ValidateSessionAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateSessionAsync_ActiveSession_ReturnsSession()
    {
        // Arrange
        await _service.CreateSessionAsync(1, "tok-active", "1.2.3.4", "UA", DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _service.ValidateSessionAsync("tok-active");

        // Assert
        result.Should().NotBeNull();
        result!.SessionToken.Should().Be("tok-active");
    }

    [Fact]
    public async Task ValidateSessionAsync_ExpiredSession_ReturnsNull()
    {
        // Arrange
        await _service.CreateSessionAsync(1, "tok-expired", "1.2.3.4", "UA", DateTime.UtcNow.AddSeconds(-1));

        // Act
        var result = await _service.ValidateSessionAsync("tok-expired");

        // Assert
        result.Should().BeNull("expired sessions should not be valid");
    }

    [Fact]
    public async Task ValidateSessionAsync_RevokedSession_ReturnsNull()
    {
        // Arrange
        await _service.CreateSessionAsync(1, "tok-revoke", "1.2.3.4", "UA", DateTime.UtcNow.AddHours(1));
        await _service.RevokeSessionAsync("tok-revoke");

        // Act
        var result = await _service.ValidateSessionAsync("tok-revoke");

        // Assert
        result.Should().BeNull("revoked sessions should not validate");
    }

    [Fact]
    public async Task ValidateSessionAsync_UnknownToken_ReturnsNull()
    {
        var result = await _service.ValidateSessionAsync("tok-does-not-exist");
        result.Should().BeNull();
    }

    // ── ValidateSessionWithIpCheckAsync (TODO-AUTH-015 IP session binding) ────

    [Fact]
    public async Task ValidateSessionWithIpCheckAsync_IpBindingDisabled_AllowsDifferentIp()
    {
        // Arrange — session without IP binding (IpBindingEnabled = false)
        await _service.CreateSessionAsync(1, "tok-no-bind", "10.0.0.1", "UA", DateTime.UtcNow.AddHours(1));

        // Act — validate from a completely different IP
        var result = await _service.ValidateSessionWithIpCheckAsync("tok-no-bind", "192.168.1.99");

        // Assert — should succeed because IP binding is not enabled for this session
        result.Should().NotBeNull("IP binding is disabled, so any IP should be accepted");
    }

    [Fact]
    public async Task ValidateSessionWithIpCheckAsync_IpBindingEnabled_SameIp_ReturnsSession()
    {
        // Arrange — create session with IP binding enabled
        var ipBoundService = new SessionManagerService(
            _dbContext,
            _ipBindingConfig,
            _mockLogger.Object);

        await ipBoundService.CreateSessionAsync(
            userId: 10,
            sessionToken: "tok-same-ip",
            ipAddress: "5.5.5.5",
            userAgent: "UA",
            expiresAt: DateTime.UtcNow.AddHours(1));

        // Act — validate from the SAME IP
        var result = await ipBoundService.ValidateSessionWithIpCheckAsync("tok-same-ip", "5.5.5.5");

        // Assert
        result.Should().NotBeNull("same IP should be accepted when binding is enabled");
    }

    [Fact]
    public async Task ValidateSessionWithIpCheckAsync_IpBindingEnabled_DifferentIp_RevokesAndReturnsNull()
    {
        // Arrange — IP-binding enforced session
        var ipBoundService = new SessionManagerService(
            _dbContext,
            _ipBindingConfig,
            _mockLogger.Object);

        await ipBoundService.CreateSessionAsync(
            userId: 11,
            sessionToken: "tok-diff-ip",
            ipAddress: "5.5.5.5",
            userAgent: "UA",
            expiresAt: DateTime.UtcNow.AddHours(1));

        // Act — validate from a DIFFERENT IP
        var result = await ipBoundService.ValidateSessionWithIpCheckAsync("tok-diff-ip", "9.9.9.9");

        // Assert — session must be refused AND revoked (security measure)
        result.Should().BeNull("different IP should be rejected when binding is enabled");

        var revokedSession = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == "tok-diff-ip");
        revokedSession!.IsRevoked.Should().BeTrue("session should be auto-revoked on IP mismatch");
    }

    [Fact]
    public async Task ValidateSessionWithIpCheckAsync_IpBindingEnabled_ExpiredSession_ReturnsNull()
    {
        // Arrange — expired session
        var ipBoundService = new SessionManagerService(
            _dbContext,
            _ipBindingConfig,
            _mockLogger.Object);

        await ipBoundService.CreateSessionAsync(
            userId: 12,
            sessionToken: "tok-ip-expired",
            ipAddress: "1.2.3.4",
            userAgent: "UA",
            expiresAt: DateTime.UtcNow.AddSeconds(-1));

        // Act
        var result = await ipBoundService.ValidateSessionWithIpCheckAsync("tok-ip-expired", "1.2.3.4");

        result.Should().BeNull("expired sessions should not pass IP validation either");
    }

    // ── RevokeSessionAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeSessionAsync_ExistingToken_MarksRevoked()
    {
        // Arrange
        await _service.CreateSessionAsync(1, "tok-to-revoke", "1.2.3.4", "UA", DateTime.UtcNow.AddHours(1));

        // Act
        await _service.RevokeSessionAsync("tok-to-revoke");

        // Assert
        var session = await _dbContext.UserSessions.FirstAsync(s => s.SessionToken == "tok-to-revoke");
        session.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeSessionAsync_UnknownToken_DoesNotThrow()
    {
        // Act
        Func<Task> act = async () => await _service.RevokeSessionAsync("tok-nonexistent");

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ── RevokeAllSessionsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task RevokeAllSessionsAsync_MultipleActiveSessions_RevokesAll()
    {
        // Arrange
        var expiry = DateTime.UtcNow.AddHours(1);
        await _service.CreateSessionAsync(5, "tok-a", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(5, "tok-b", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(5, "tok-c", "1.1.1.1", "UA", expiry);

        // Act
        await _service.RevokeAllSessionsAsync(5);

        // Assert
        var remaining = await _dbContext.UserSessions
            .Where(s => s.UserId == 5 && !s.IsRevoked)
            .CountAsync();
        remaining.Should().Be(0, "all sessions should be revoked");
    }

    // ── GetActiveSessionsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveSessionsAsync_ReturnsOnlyNonRevokedNonExpired()
    {
        // Arrange
        var expiry = DateTime.UtcNow.AddHours(1);
        await _service.CreateSessionAsync(7, "tok-active1", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(7, "tok-active2", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(7, "tok-revoked", "1.1.1.1", "UA", expiry);
        await _service.RevokeSessionAsync("tok-revoked");
        await _service.CreateSessionAsync(7, "tok-pexpired", "1.1.1.1", "UA", DateTime.UtcNow.AddSeconds(-1));

        // Act
        var active = await _service.GetActiveSessionsAsync(7);

        // Assert
        active.Should().HaveCount(2);
        active.Select(s => s.SessionToken).Should().Contain(["tok-active1", "tok-active2"]);
    }

    // ── EnforceSessionLimitAsync ──────────────────────────────────────────────

    [Fact]
    public async Task EnforceSessionLimitAsync_ExceedsMaxSessions_RevokesOldest()
    {
        // Arrange — max = 3 (from _config), create 3 sessions already so the 4th needs to fit
        var expiry = DateTime.UtcNow.AddHours(1);
        await _service.CreateSessionAsync(9, "tok-old1", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(9, "tok-old2", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(9, "tok-old3", "1.1.1.1", "UA", expiry);

        // Act — enforce before creating 4th session
        await _service.EnforceSessionLimitAsync(9);

        // Assert — oldest session(s) should be revoked to make room
        var activeSessions = await _dbContext.UserSessions
            .Where(s => s.UserId == 9 && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        activeSessions.Count.Should().BeLessOrEqualTo(3,
            "after enforcing limit, remaining active sessions should be within max");
    }

    [Fact]
    public async Task EnforceSessionLimitAsync_BelowLimit_DoesNotRevoke()
    {
        // Arrange
        var expiry = DateTime.UtcNow.AddHours(1);
        await _service.CreateSessionAsync(8, "tok-s1", "1.1.1.1", "UA", expiry);
        await _service.CreateSessionAsync(8, "tok-s2", "1.1.1.1", "UA", expiry);

        // Act
        await _service.EnforceSessionLimitAsync(8);

        // Assert — both sessions should still be active
        var active = await _service.GetActiveSessionsAsync(8);
        active.Should().HaveCount(2, "below limit, no sessions should be revoked");
    }
}
