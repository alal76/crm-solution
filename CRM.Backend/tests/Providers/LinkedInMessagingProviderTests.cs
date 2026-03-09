// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: LinkedInMessagingProvider
//
// Verified from source before writing:
//   Class: LinkedInMessagingProvider, Namespace: CRM.Infrastructure.Providers.Messaging
//   Constructor: (IOptions<LinkedInMessagingOptions>, ILogger<LinkedInMessagingProvider>)
//   Interface: CRM.Core.Interfaces.ILinkedInMessagingProvider
//   Options: CRM.Core.Configuration.LinkedInMessagingOptions
//     (ClientId, ClientSecret, AccessToken, Enabled, MockMode)
//   IsAvailable: always false (mock-only provider, COMM-004)
//   IsMockMode: always true
//   SendMessageAsync: always logs warning and returns false (no HTTP calls)
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
/// Unit tests for <see cref="LinkedInMessagingProvider"/>.
/// COMM-004: Verifies that the mock-only stub behaves correctly.
/// </summary>
public class LinkedInMessagingProviderTests
{
    // ── Factory helpers ─────────────────────────────────────────────────────

    private static LinkedInMessagingOptions DefaultOptions() => new()
    {
        ClientId = string.Empty,
        ClientSecret = string.Empty,
        AccessToken = string.Empty,
        Enabled = false,
        MockMode = true
    };

    private static (LinkedInMessagingProvider provider, Mock<ILogger<LinkedInMessagingProvider>> loggerMock)
        CreateProvider(LinkedInMessagingOptions? options = null)
    {
        var opts = Options.Create(options ?? DefaultOptions());
        var loggerMock = new Mock<ILogger<LinkedInMessagingProvider>>();
        return (new LinkedInMessagingProvider(opts, loggerMock.Object), loggerMock);
    }

    // ── IsAvailable ──────────────────────────────────────────────────────────

    [Fact]
    public void IsAvailable_ReturnsFalse_Always()
    {
        var (provider, _) = CreateProvider();

        provider.IsAvailable.Should().BeFalse(
            "COMM-004: LinkedIn outbound messaging requires Sales Navigator; always unavailable.");
    }

    [Fact]
    public void IsAvailable_ReturnsFalse_EvenWhenOptionsAreFullyConfigured()
    {
        var opts = new LinkedInMessagingOptions
        {
            ClientId = "client_id_abc123",
            ClientSecret = "client_secret_def456",
            AccessToken = "access_token_ghi789",
            Enabled = true,
            MockMode = true
        };
        var (provider, _) = CreateProvider(opts);

        provider.IsAvailable.Should().BeFalse(
            "COMM-004: Even with full config, outbound messaging requires Sales Navigator.");
    }

    // ── IsMockMode ───────────────────────────────────────────────────────────

    [Fact]
    public void IsMockMode_ReturnsTrue_Always()
    {
        var (provider, _) = CreateProvider();

        provider.IsMockMode.Should().BeTrue(
            "COMM-004: This implementation is always mock-only.");
    }

    // ── SendMessageAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_AlwaysInMockMode()
    {
        var (provider, _) = CreateProvider();

        var result = await provider.SendMessageAsync("urn:li:person:AbCdEf", "Hello, LinkedIn!");

        result.Should().BeFalse(
            "COMM-004: Outbound messaging is not available in mock mode.");
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenFullyConfigured()
    {
        var opts = new LinkedInMessagingOptions
        {
            ClientId = "client_id_abc123",
            ClientSecret = "client_secret_def456",
            AccessToken = "access_token_ghi789",
            Enabled = true,
            MockMode = true
        };
        var (provider, _) = CreateProvider(opts);

        var result = await provider.SendMessageAsync("urn:li:person:XyZwVu", "Sales message");

        result.Should().BeFalse(
            "COMM-004: Outbound messaging is never attempted — this is a mock stub.");
    }

    [Fact]
    public async Task SendMessageAsync_LogsWarning_WhenCalled()
    {
        var (provider, loggerMock) = CreateProvider();

        await provider.SendMessageAsync("urn:li:person:AbCdEf", "Test message");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("LinkedIn outbound messaging not available")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "A warning must be logged to inform operators that outbound messaging requires Sales Navigator.");
    }

    [Fact]
    public async Task SendMessageAsync_LogsWarning_ContainsRecipientUrn()
    {
        var (provider, loggerMock) = CreateProvider();
        const string recipientUrn = "urn:li:person:TestUrn123";

        await provider.SendMessageAsync(recipientUrn, "ping");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains(recipientUrn)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "The warning log must include the recipient URN for traceability.");
    }

    [Fact]
    public async Task SendMessageAsync_WithCancellationToken_ReturnsFalse()
    {
        var (provider, _) = CreateProvider();
        using var cts = new CancellationTokenSource();

        var result = await provider.SendMessageAsync("urn:li:person:AbCdEf", "Test", cts.Token);

        result.Should().BeFalse();
    }
}
