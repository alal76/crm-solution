// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.Auth;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for OktaSsoService (BACK-001).
/// Covers URL building, logout URL generation, and configuration validation.
/// </summary>
public class OktaSsoServiceTests
{
    private const string DefaultDomain = "dev-123456.okta.com";
    private const string DefaultClientId = "test-client-id";
    private const string DefaultClientSecret = "test-client-secret";
    private const string DefaultRedirectUri = "https://app.example.com/callback";

    private readonly Mock<ILogger<OktaSsoService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public OktaSsoServiceTests()
    {
        _mockLogger = new Mock<ILogger<OktaSsoService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());
    }

    private OktaSsoService CreateService(string domain = DefaultDomain)
    {
        var options = Options.Create(new OktaSsoOptions
        {
            Domain = domain,
            ClientId = DefaultClientId,
            ClientSecret = DefaultClientSecret,
            RedirectUri = DefaultRedirectUri,
            AuthorizationServerId = "default",
            Scopes = "openid profile email"
        });

        return new OktaSsoService(_mockLogger.Object, options, _mockHttpClientFactory.Object);
    }

    // ─── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidConfiguration()
    {
        // Act
        var svc = CreateService();

        // Assert
        svc.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldLogWarning_WhenDomainIsEmpty()
    {
        // Arrange
        var domain = string.Empty;

        // Act — should not throw, just log warning
        var act = () => CreateService(domain);

        act.Should().NotThrow();
        _mockLogger.Invocations.Should().NotBeEmpty();
    }

    // ─── GetAuthorizationUrl ──────────────────────────────────────────────────

    [Fact]
    public void GetAuthorizationUrl_ShouldReturnUrlContainingClientId_WhenConfigured()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var url = svc.GetAuthorizationUrl("state-123", "code-challenge-abc");

        // Assert
        url.Should().Contain(DefaultClientId);
    }

    [Fact]
    public void GetAuthorizationUrl_ShouldReturnUrlContainingDomain_WhenConfigured()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var url = svc.GetAuthorizationUrl("state-abc", "challenge-xyz");

        // Assert
        url.Should().Contain(DefaultDomain);
    }

    [Fact]
    public void GetAuthorizationUrl_ShouldIncludeState_WhenProvided()
    {
        // Arrange
        var svc = CreateService();
        const string state = "csrf-random-state";

        // Act
        var url = svc.GetAuthorizationUrl(state, "challenge-abc");

        // Assert
        url.Should().Contain(Uri.EscapeDataString(state));
    }

    [Fact]
    public void GetAuthorizationUrl_ShouldIncludeCodeChallenge_WhenProvided()
    {
        // Arrange
        var svc = CreateService();
        const string codeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        // Act
        var url = svc.GetAuthorizationUrl("state-xyz", codeChallenge);

        // Assert
        url.Should().Contain(Uri.EscapeDataString(codeChallenge));
    }

    [Fact]
    public void GetAuthorizationUrl_ShouldThrowInvalidOperationException_WhenDomainNotConfigured()
    {
        // Arrange
        var svc = CreateService(domain: string.Empty);

        // Act & Assert
        var act = () => svc.GetAuthorizationUrl("state", "challenge");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Okta*");
    }

    [Fact]
    public void GetAuthorizationUrl_ShouldReturnHttpsUrl_WhenConfigured()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var url = svc.GetAuthorizationUrl("state", "challenge");

        // Assert
        url.Should().StartWith("https://");
    }

    // ─── GetLogoutUrl ─────────────────────────────────────────────────────────

    [Fact]
    public void GetLogoutUrl_ShouldNotThrow_WhenDomainNotConfigured()
    {
        // Arrange — service with empty domain
        var svc = CreateService(domain: string.Empty);

        // Act — GetLogoutUrl should gracefully return (not throw; domain validation is
        // enforced on the authorization side, not the logout side for this provider)
        var act = () => svc.GetLogoutUrl("id-token", "https://app.example.com");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GetLogoutUrl_ShouldContainDomain_WhenConfigured()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var url = svc.GetLogoutUrl("id-token-hint", "https://app.example.com/loggedout");

        // Assert
        url.Should().Contain(DefaultDomain);
    }

    [Fact]
    public void GetLogoutUrl_ShouldIncludeIdTokenHint_WhenProvided()
    {
        // Arrange
        var svc = CreateService();
        const string idToken = "eyJ.fake.token";

        // Act
        var url = svc.GetLogoutUrl(idToken, "https://app.example.com");

        // Assert
        url.Should().Contain(Uri.EscapeDataString(idToken));
    }

    [Fact]
    public void GetLogoutUrl_ShouldReturnValidUrl_WhenIdTokenIsNull()
    {
        // Arrange
        var svc = CreateService();

        // Act
        var url = svc.GetLogoutUrl(null, "https://app.example.com");

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain(DefaultDomain);
    }
}
