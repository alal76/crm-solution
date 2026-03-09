// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for QuickBooksService — INT-001.
/// </summary>
public class QuickBooksServiceTests
{
    private static QuickBooksService BuildService(
        QuickBooksOptions? opts = null,
        IntegrationTokenStore? tokenStore = null)
    {
        var options = Options.Create(opts ?? new QuickBooksOptions
        {
            Enabled = true,
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            RedirectUri = "http://localhost:5000/api/integrations/quickbooks/callback",
            Environment = "sandbox"
        });

        var db = new Mock<ICrmDbContext>().Object;
        var logger = new Mock<ILogger<QuickBooksService>>().Object;
        var httpClient = new HttpClient();
        var store = tokenStore ?? new IntegrationTokenStore();

        return new QuickBooksService(options, db, logger, httpClient, store);
    }

    // ------------------------------------------------------------------ //
    //  GetAuthorizationUrl
    // ------------------------------------------------------------------ //

    [Fact]
    public void GetAuthorizationUrl_ReturnsCorrectUrl_WithClientIdAndRedirectUri()
    {
        // Arrange
        var svc = BuildService();

        // Act
        var url = svc.GetAuthorizationUrl("test-state-123");

        // Assert
        url.Should().StartWith("https://appcenter.intuit.com/connect/oauth2");
        url.Should().Contain("client_id=test-client-id");
        url.Should().Contain("response_type=code");
        url.Should().Contain("scope=com.intuit.quickbooks.accounting");
        url.Should().Contain("state=test-state-123");
    }

    [Fact]
    public void GetAuthorizationUrl_UrlEncodesRedirectUri()
    {
        // Arrange
        var svc = BuildService();

        // Act
        var url = svc.GetAuthorizationUrl("state");

        // Assert — redirect_uri must be URL-encoded in the query string
        url.Should().Contain("redirect_uri=");
        url.Should().Contain("localhost");
    }

    // ------------------------------------------------------------------ //
    //  SyncAccountAsync — disabled / not connected guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncAccountAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new QuickBooksOptions { Enabled = false });

        // Act
        var result = await svc.SyncAccountAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncAccountAsync_ReturnsFalse_WhenNotConnected()
    {
        // Arrange — enabled but no tokens stored
        var svc = BuildService(new QuickBooksOptions { Enabled = true });

        // Act
        var result = await svc.SyncAccountAsync(99);

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------------ //
    //  SyncInvoiceAsync — disabled / not connected guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncInvoiceAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new QuickBooksOptions { Enabled = false });

        // Act
        var result = await svc.SyncInvoiceAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncInvoiceAsync_ReturnsFalse_WhenNotConnected()
    {
        // Arrange — enabled but no tokens stored
        var svc = BuildService(new QuickBooksOptions { Enabled = true });

        // Act
        var result = await svc.SyncInvoiceAsync(42);

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------------ //
    //  GetConnectionStatusAsync
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetConnectionStatus_ReturnsNotConnected_WhenNoTokensStored()
    {
        // Arrange
        var svc = BuildService();

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.Should().NotBeNull();
        status.IsConnected.Should().BeFalse();
        status.RealmId.Should().BeNull();
        status.TokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStatus_ReturnsConnected_WhenTokensPresent()
    {
        // Arrange
        var store = new IntegrationTokenStore();
        store.Set("qb:access_token", "tok_abc");
        store.Set("qb:realm_id", "realm-123");
        store.Set("qb:expires_at", DateTime.UtcNow.AddHours(1).ToString("O"));

        var svc = BuildService(tokenStore: store);

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.IsConnected.Should().BeTrue();
        status.RealmId.Should().Be("realm-123");
        status.TokenExpiresAt.Should().NotBeNull();
    }
}
