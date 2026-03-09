// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: TwitterMessagingProvider
//
// Verified from source before writing:
//   Class: TwitterMessagingProvider, Namespace: CRM.Infrastructure.Providers.Messaging
//   Constructor: (IOptions<TwitterMessagingOptions>, ILogger<TwitterMessagingProvider>)
//   Interface: CRM.Core.Interfaces.ITwitterMessagingProvider
//   Options: CRM.Core.Configuration.TwitterMessagingOptions
//     (BearerToken, ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret, Enabled, MockMode)
//   IsAvailable: always false (mock-only provider, COMM-003)
//   IsMockMode: always true
//   SendDirectMessageAsync: always logs warning and returns false (no HTTP calls)
//   No HttpClient dependency: mock provider requires no network access
using CRM.Core.Configuration;
using CRM.Infrastructure.Providers.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="TwitterMessagingProvider"/>.
/// COMM-003: Verifies that the mock-only stub behaves correctly.
/// </summary>
public class TwitterMessagingProviderTests
{
    // ── Factory helpers ─────────────────────────────────────────────────────

    private static TwitterMessagingOptions DefaultOptions() => new()
    {
        BearerToken = string.Empty,
        ConsumerKey = string.Empty,
        ConsumerSecret = string.Empty,
        AccessToken = string.Empty,
        AccessTokenSecret = string.Empty,
        Enabled = false,
        MockMode = true
    };

    private static (TwitterMessagingProvider provider, Mock<ILogger<TwitterMessagingProvider>> loggerMock)
        CreateProvider(TwitterMessagingOptions? options = null)
    {
        var opts = Options.Create(options ?? DefaultOptions());
        var loggerMock = new Mock<ILogger<TwitterMessagingProvider>>();
        return (new TwitterMessagingProvider(opts, loggerMock.Object), loggerMock);
    }

    // ── IsAvailable ──────────────────────────────────────────────────────────

    [Fact]
    public void IsAvailable_ReturnsFalse_Always()
    {
        var (provider, _) = CreateProvider();

        provider.IsAvailable.Should().BeFalse(
            "COMM-003: Twitter outbound DMs require paid API tier; always unavailable.");
    }

    [Fact]
    public void IsAvailable_ReturnsFalse_EvenWhenOptionsAreFullyConfigured()
    {
        var opts = new TwitterMessagingOptions
        {
            BearerToken = "AAAA_bearer_token",
            ConsumerKey = "consumer_key_123",
            ConsumerSecret = "consumer_secret_456",
            AccessToken = "access_token_789",
            AccessTokenSecret = "access_token_secret_abc",
            Enabled = true,
            MockMode = true
        };
        var (provider, _) = CreateProvider(opts);

        provider.IsAvailable.Should().BeFalse(
            "COMM-003: Even with full config, outbound DMs require paid API tier.");
    }

    // ── IsMockMode ───────────────────────────────────────────────────────────

    [Fact]
    public void IsMockMode_ReturnsTrue_Always()
    {
        var (provider, _) = CreateProvider();

        provider.IsMockMode.Should().BeTrue(
            "COMM-003: This implementation is always mock-only.");
    }

    // ── SendDirectMessageAsync ───────────────────────────────────────────────

    [Fact]
    public async Task SendDirectMessageAsync_ReturnsFalse_AlwaysInMockMode()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.SendDirectMessageAsync("123456789", "Hello, world!");

        result.Should().BeFalse(
            "COMM-003: Outbound DMs are not available in mock mode.");
    }

    [Fact]
    public async Task SendDirectMessageAsync_ReturnsFalse_WhenFullyConfigured()
    {
        var opts = new TwitterMessagingOptions
        {
            BearerToken = "AAAA_bearer_token",
            ConsumerKey = "consumer_key_123",
            ConsumerSecret = "consumer_secret_456",
            AccessToken = "access_token_789",
            AccessTokenSecret = "access_token_secret_abc",
            Enabled = true,
            MockMode = true
        };
        var (provider, _) = CreateProvider(opts);

        var result = await provider.SendDirectMessageAsync("987654321", "Test DM");

        result.Should().BeFalse(
            "COMM-003: Outbound DMs are never attempted — this is a mock stub.");
    }

    [Fact]
    public async Task SendDirectMessageAsync_LogsWarning_WhenCalled()
    {
        var (provider, loggerMock) = CreateProvider();

        await provider.SendDirectMessageAsync("123456789", "Test message");

        // Verify a warning was logged (level Warning)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("Twitter outbound DMs are not available")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "A warning must be logged to inform operators that outbound DMs require a paid tier.");
    }

    [Fact]
    public async Task SendDirectMessageAsync_LogsWarning_ContainsRecipientId()
    {
        var (provider, loggerMock) = CreateProvider();
        const string recipientId = "555444333222";

        await provider.SendDirectMessageAsync(recipientId, "ping");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains(recipientId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "The warning log must include the recipient user ID for traceability.");
    }

    [Fact]
    public async Task SendDirectMessageAsync_WithCancellationToken_ReturnsFalse()
    {
        var (provider, _) = CreateProvider();
        using var cts = new CancellationTokenSource();

        var result = await provider.SendDirectMessageAsync("123456789", "Test", cts.Token);

        result.Should().BeFalse();
    }
}
