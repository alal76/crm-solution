// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Core.Ports;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.Integrations;

/// <summary>
/// Unit tests for <see cref="AccountingSyncService"/> (REV-STUB-002).
///
/// AccountingSyncService resolves the active provider (QuickBooks/Xero) via
/// IProviderConfigurationService and delegates account/invoice sync to
/// IQuickBooksService/IXeroService (mocked here). SyncPaymentAsync makes its own direct
/// HTTP call, mocked at the HTTP layer via a fake handler behind IHttpClientFactory — no
/// real network calls are made anywhere in this file.
///
/// MANDATORY: Written after verifying source for:
///   Class: AccountingSyncService, Namespace: CRM.Infrastructure.Services.Integrations
///   Constructor: (IProviderConfigurationService, IQuickBooksService, IXeroService,
///                 IntegrationTokenStore, ICrmDbContext, IHttpClientFactory, ILogger)
/// </summary>
public class AccountingSyncServiceTests
{
    private static ProviderConfigurationDto ConfigDto(string clientId, string clientSecret) => new()
    {
        Id = 1,
        ConfigurationKey = "crm.accounting.quickbooks",
        ConfigurationType = "crm",
        ConfigurationData = JsonSerializer.Serialize(new { ClientId = clientId, ClientSecret = clientSecret }),
        IsEncrypted = false,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static Mock<IProviderConfigurationService> ConfigServiceMock(bool quickBooksConfigured, bool xeroConfigured = false)
    {
        var mock = new Mock<IProviderConfigurationService>();

        mock.Setup(m => m.GetConfigurationAsync("crm.accounting.quickbooks", It.IsAny<CancellationToken>()))
            .ReturnsAsync(quickBooksConfigured ? ConfigDto("qb-client", "qb-secret") : null);

        mock.Setup(m => m.GetConfigurationAsync("crm.accounting.xero", It.IsAny<CancellationToken>()))
            .ReturnsAsync(xeroConfigured ? ConfigDto("xero-client", "xero-secret") : null);

        return mock;
    }

    private static AccountingSyncService BuildService(
        Mock<IProviderConfigurationService> configService,
        Mock<IQuickBooksService>? quickBooks = null,
        Mock<IXeroService>? xero = null,
        IntegrationTokenStore? tokenStore = null,
        HttpMessageHandler? handler = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler ?? new TestHttpMessageHandler(HttpStatusCode.OK, "{}")));

        return new AccountingSyncService(
            configService.Object,
            (quickBooks ?? new Mock<IQuickBooksService>()).Object,
            (xero ?? new Mock<IXeroService>()).Object,
            tokenStore ?? new IntegrationTokenStore(),
            Mock.Of<ICrmDbContext>(),
            factory.Object,
            Mock.Of<ILogger<AccountingSyncService>>());
    }

    // ──────────────────────────────────────────────────────────────────
    // SyncAccountAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAccountAsync_ReturnsSuccess_WhenQuickBooksSyncSucceeds()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.SyncAccountAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = BuildService(configService, quickBooks: quickBooks);

        var result = await service.SyncAccountAsync(accountId: 1);

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("QuickBooks");
    }

    [Fact]
    public async Task SyncAccountAsync_ReturnsFailedResult_WhenQuickBooksSyncFails()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.SyncAccountAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = BuildService(configService, quickBooks: quickBooks);

        var result = await service.SyncAccountAsync(accountId: 1);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SyncAccountAsync_IncludesProviderInResult()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var service = BuildService(configService);

        var result = await service.SyncAccountAsync(1);

        result.Provider.Should().Be("QuickBooks");
    }

    // ──────────────────────────────────────────────────────────────────
    // SyncInvoiceAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncInvoiceAsync_ReturnsFailedResult_WhenNotConfigured()
    {
        // No provider configured at all — resolution defaults to "QuickBooks" label,
        // and the mocked IQuickBooksService.SyncInvoiceAsync defaults to false.
        var configService = ConfigServiceMock(quickBooksConfigured: false, xeroConfigured: false);
        var service = BuildService(configService);

        var result = await service.SyncInvoiceAsync(invoiceId: 42);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SyncInvoiceAsync_DelegatesToXero_WhenOnlyXeroConfigured()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: false, xeroConfigured: true);
        var xero = new Mock<IXeroService>();
        xero.Setup(x => x.SyncInvoiceAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = BuildService(configService, xero: xero);

        var result = await service.SyncInvoiceAsync(invoiceId: 42);

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("Xero");
    }

    // ──────────────────────────────────────────────────────────────────
    // SyncPaymentAsync — real HTTP layer (mocked)
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncPaymentAsync_ReturnsFailedResult_WhenNotConnected()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var service = BuildService(configService, tokenStore: new IntegrationTokenStore());

        var result = await service.SyncPaymentAsync("ext-pay-001");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not connected");
    }

    [Fact]
    public async Task SyncPaymentAsync_ReturnsSuccess_WhenQuickBooksReturns200()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var tokenStore = new IntegrationTokenStore();
        tokenStore.Set("qb:access_token", "test-token");
        tokenStore.Set("qb:realm_id", "realm-1");
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, """{"Payment":{"Id":"ext-pay-001"}}""");
        var service = BuildService(configService, tokenStore: tokenStore, handler: handler);

        var result = await service.SyncPaymentAsync("ext-pay-001");

        result.Success.Should().BeTrue();
        result.ExternalId.Should().Be("ext-pay-001");
    }

    [Fact]
    public async Task SyncPaymentAsync_ReturnsFailedResult_WhenHttpReturnsError()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var tokenStore = new IntegrationTokenStore();
        tokenStore.Set("qb:access_token", "test-token");
        tokenStore.Set("qb:realm_id", "realm-1");
        var handler = new TestHttpMessageHandler(HttpStatusCode.InternalServerError, "{}");
        var service = BuildService(configService, tokenStore: tokenStore, handler: handler);

        var result = await service.SyncPaymentAsync("ext-pay-001");

        result.Success.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────────
    // GetSyncStatusAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSyncStatusAsync_ReturnsNotConnectedStatus_WhenNotConnected()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuickBooksConnectionStatus { IsConnected = false });
        var service = BuildService(configService, quickBooks: quickBooks);

        var status = await service.GetSyncStatusAsync("Account", 7);

        status.Should().NotBeNull();
        status!.Status.Should().Be("NotConnected");
        status.EntityType.Should().Be("Account");
        status.EntityId.Should().Be(7);
    }

    // ──────────────────────────────────────────────────────────────────
    // RunBatchSyncAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunBatchSyncAsync_ReturnsEmptySummaryWithError_WhenNotConnected()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuickBooksConnectionStatus { IsConnected = false });
        var service = BuildService(configService, quickBooks: quickBooks);

        var result = await service.RunBatchSyncAsync();

        result.TotalProcessed.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
        result.Errors.Should().HaveCountGreaterOrEqualTo(1);
    }

    // ──────────────────────────────────────────────────────────────────
    // TestConnectionAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_WhenNotConfigured_ReturnsFalse()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: false, xeroConfigured: false);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuickBooksConnectionStatus { IsConnected = false });
        var service = BuildService(configService, quickBooks: quickBooks);

        var connected = await service.TestConnectionAsync();

        connected.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WhenConfiguredAndConnected_ReturnsTrue()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuickBooksConnectionStatus { IsConnected = true });
        var service = BuildService(configService, quickBooks: quickBooks);

        var connected = await service.TestConnectionAsync();

        connected.Should().BeTrue();
    }

    // ── Test HTTP handler ───────────────────────────────────────────────────

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public TestHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }
}
