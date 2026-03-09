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
/// Unit tests for XeroService — INT-001.
/// </summary>
public class XeroServiceTests
{
    private static XeroService BuildService(
        XeroOptions? opts = null,
        IntegrationTokenStore? tokenStore = null)
    {
        var options = Options.Create(opts ?? new XeroOptions
        {
            Enabled = true,
            ClientId = "xero-client-id",
            ClientSecret = "xero-client-secret",
            RedirectUri = "http://localhost:5000/api/integrations/xero/callback"
        });

        var db = new Mock<ICrmDbContext>().Object;
        var logger = new Mock<ILogger<XeroService>>().Object;
        var httpClient = new HttpClient();
        var store = tokenStore ?? new IntegrationTokenStore();

        return new XeroService(options, db, logger, httpClient, store);
    }

    // ------------------------------------------------------------------ //
    //  GetAuthorizationUrl
    // ------------------------------------------------------------------ //

    [Fact]
    public void GetAuthorizationUrl_ReturnsCorrectUrl_WithScopes()
    {
        // Arrange
        var svc = BuildService();

        // Act
        var url = svc.GetAuthorizationUrl("xero-state-abc");

        // Assert
        url.Should().StartWith("https://login.xero.com/identity/connect/authorize");
        url.Should().Contain("client_id=xero-client-id");
        url.Should().Contain("response_type=code");
        url.Should().Contain("scope=");
        url.Should().Contain("accounting.contacts");
        url.Should().Contain("state=xero-state-abc");
    }

    [Fact]
    public void GetAuthorizationUrl_IncludesAccountingTransactionsScope()
    {
        // Arrange
        var svc = BuildService();

        // Act
        var url = svc.GetAuthorizationUrl("state");

        // Assert
        url.Should().Contain("accounting.transactions");
    }

    // ------------------------------------------------------------------ //
    //  SyncContactAsync — disabled / not connected guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncContactAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new XeroOptions { Enabled = false });

        // Act
        var result = await svc.SyncContactAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncContactAsync_ReturnsFalse_WhenNotConnected()
    {
        // Arrange — enabled but no tokens
        var svc = BuildService(new XeroOptions { Enabled = true });

        // Act
        var result = await svc.SyncContactAsync(10);

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
        var svc = BuildService(new XeroOptions { Enabled = false });

        // Act
        var result = await svc.SyncInvoiceAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncInvoiceAsync_ReturnsFalse_WhenNotConnected()
    {
        // Arrange — enabled but no tokens stored
        var svc = BuildService(new XeroOptions { Enabled = true });

        // Act
        var result = await svc.SyncInvoiceAsync(55);

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
        status.TenantId.Should().BeNull();
        status.TokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStatus_ReturnsConnected_WhenTokensPresent()
    {
        // Arrange
        var store = new IntegrationTokenStore();
        store.Set("xero:access_token", "xero_tok_xyz");
        store.Set("xero:tenant_id", "tenant-456");
        store.Set("xero:expires_at", DateTime.UtcNow.AddMinutes(30).ToString("O"));

        var svc = BuildService(tokenStore: store);

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.IsConnected.Should().BeTrue();
        status.TenantId.Should().Be("tenant-456");
        status.TokenExpiresAt.Should().NotBeNull();
    }
}
