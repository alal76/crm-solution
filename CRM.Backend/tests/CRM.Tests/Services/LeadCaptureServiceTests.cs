// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LeadCaptureService (BACK-006).
/// Covers token generation, validation, revocation, and lead capture from form submissions.
/// </summary>
public class LeadCaptureServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDb;
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly Mock<ILogger<LeadCaptureService>> _mockLogger;

    public LeadCaptureServiceTests()
    {
        _mockDb = new Mock<ICrmDbContext>();
        _mockLeadService = new Mock<ILeadService>();
        _mockLogger = new Mock<ILogger<LeadCaptureService>>();
    }

    private LeadCaptureService CreateService()
    {
        return new LeadCaptureService(
            _mockDb.Object,
            _mockLeadService.Object,
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

    // ─── GenerateFormTokenAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GenerateFormTokenAsync_ShouldReturnToken_WhenFormNameProvided()
    {
        // Arrange
        var svc = CreateService();
        var uniqueName = $"test-form-{Guid.NewGuid()}";

        // Act
        var result = await svc.GenerateFormTokenAsync(uniqueName);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateFormTokenAsync_ShouldReturnFormNameInResult_WhenFormNameProvided()
    {
        // Arrange
        var svc = CreateService();
        var uniqueName = $"Contact-Form-{Guid.NewGuid()}";

        // Act
        var result = await svc.GenerateFormTokenAsync(uniqueName);

        // Assert
        result.FormName.Should().Be(uniqueName);
    }

    [Fact]
    public async Task GenerateFormTokenAsync_ShouldSetExpiryBasedOnHours_WhenExpiresInHoursProvided()
    {
        // Arrange
        var svc = CreateService();
        const int hours = 48;
        var before = DateTime.UtcNow;

        // Act
        var result = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}", expiresInHours: hours);

        // Assert
        result.ExpiresAt.Should().BeAfter(before.AddHours(hours - 1));
        result.ExpiresAt.Should().BeBefore(before.AddHours(hours + 1));
    }

    [Fact]
    public async Task GenerateFormTokenAsync_ShouldIncludeEmbedCode_WhenTokenGenerated()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var result = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}");

        // Assert
        result.EmbedCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateFormTokenAsync_ShouldSetCampaignId_WhenCampaignIdProvided()
    {
        // Arrange
        var svc = CreateService();
        const int campaignId = 42;

        // Act
        var result = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}", campaignId: campaignId);

        // Assert
        result.CampaignId.Should().Be(campaignId);
    }

    [Fact]
    public async Task GenerateFormTokenAsync_ShouldProduceUniqueTokens_ForEachCall()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var result1 = await svc.GenerateFormTokenAsync($"form-a-{Guid.NewGuid()}");
        var result2 = await svc.GenerateFormTokenAsync($"form-b-{Guid.NewGuid()}");

        // Assert
        result1.Token.Should().NotBe(result2.Token);
    }

    // ─── ValidateFormTokenAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ValidateFormTokenAsync_ShouldReturnTrue_WhenTokenIsValid()
    {
        // Arrange
        var svc = CreateService();
        var token = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}");

        // Act
        var isValid = await svc.ValidateFormTokenAsync(token.Token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFormTokenAsync_ShouldReturnFalse_WhenTokenDoesNotExist()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var isValid = await svc.ValidateFormTokenAsync("non-existent-token-xyz-999");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFormTokenAsync_ShouldReturnFalse_WhenTokenIsExpired()
    {
        // Arrange
        var svc = CreateService();

        // Generate token that expires immediately (0 hours — already expired)
        var token = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}", expiresInHours: -1);

        // Act
        var isValid = await svc.ValidateFormTokenAsync(token.Token);

        // Assert
        isValid.Should().BeFalse();
    }

    // ─── RevokeTokenAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeTokenAsync_ShouldInvalidateToken_WhenCalledWithValidToken()
    {
        // Arrange
        var svc = CreateService();
        var token = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}");

        // Pre-condition: token is valid
        (await svc.ValidateFormTokenAsync(token.Token)).Should().BeTrue();

        // Act
        await svc.RevokeTokenAsync(token.Token);

        // Assert: token is no longer valid after revocation
        var isStillValid = await svc.ValidateFormTokenAsync(token.Token);
        isStillValid.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_ShouldNotThrow_WhenTokenDoesNotExist()
    {
        // Arrange
        var svc = CreateService();

        // Act & Assert: revoking an unknown token should not throw
        var act = async () => await svc.RevokeTokenAsync("unknown-token-that-does-not-exist");
        await act.Should().NotThrowAsync();
    }

    // ─── GetActiveTokensAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveTokensAsync_ShouldReturnToken_WhenTokenIsGenerated()
    {
        // Arrange
        var svc = CreateService();
        var uniqueName = $"active-form-{Guid.NewGuid()}";
        var token = await svc.GenerateFormTokenAsync(uniqueName);

        // Act
        var activeTokens = (await svc.GetActiveTokensAsync()).ToList();

        // Assert
        activeTokens.Should().Contain(t => t.Token == token.Token);
    }

    [Fact]
    public async Task GetActiveTokensAsync_ShouldNotReturnRevokedToken_WhenTokenRevoked()
    {
        // Arrange
        var svc = CreateService();
        var token = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}");
        await svc.RevokeTokenAsync(token.Token);

        // Act
        var activeTokens = (await svc.GetActiveTokensAsync()).ToList();

        // Assert
        activeTokens.Should().NotContain(t => t.Token == token.Token);
    }

    // ─── CaptureLeadFromFormAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CaptureLeadFromFormAsync_ShouldReturnSuccess_WhenValidTokenAndNewEmail()
    {
        // Arrange
        var svc = CreateService();
        var token = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}");

        _mockLeadService
            .Setup(s => s.CheckDuplicateAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (int?)null, (string?)null));
        _mockLeadService
            .Setup(s => s.CreateAsync(It.IsAny<Lead>()))
            .ReturnsAsync(42);

        var request = new LeadCaptureRequest
        {
            Token = token.Token,
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@example.com",
            Company = "ACME Corp"
        };

        // Act
        var result = await svc.CaptureLeadFromFormAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.LeadId.Should().Be(42);
        result.IsDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task CaptureLeadFromFormAsync_ShouldReturnFailure_WhenInvalidToken()
    {
        // Arrange
        var svc = CreateService();

        var request = new LeadCaptureRequest
        {
            Token = "totally-invalid-token",
            Email = "lead@example.com"
        };

        // Act
        var result = await svc.CaptureLeadFromFormAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CaptureLeadFromFormAsync_ShouldReturnDuplicateResult_WhenEmailAlreadyExists()
    {
        // Arrange
        var svc = CreateService();
        var token = await svc.GenerateFormTokenAsync($"form-{Guid.NewGuid()}");

        _mockLeadService
            .Setup(s => s.CheckDuplicateAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, 7, "Email"));

        var request = new LeadCaptureRequest
        {
            Token = token.Token,
            Email = "existing@example.com"
        };

        // Act
        var result = await svc.CaptureLeadFromFormAsync(request);

        // Assert
        result.IsDuplicate.Should().BeTrue();
        result.ExistingLeadId.Should().Be(7);
    }
}
