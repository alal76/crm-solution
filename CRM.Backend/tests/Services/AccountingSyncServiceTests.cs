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

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for AccountingSyncService (REV-STUB-002).
///
/// AccountingSyncService resolves the active provider (QuickBooks/Xero) via
/// IProviderConfigurationService and delegates account/invoice sync to the already-real,
/// already-tested IQuickBooksService/IXeroService (both mocked here — their own HTTP
/// behavior is covered by QuickBooksServiceTests/XeroServiceTests). SyncPaymentAsync makes
/// its own direct HTTP call, which IS mocked at the HTTP layer here via a fake handler
/// behind IHttpClientFactory — no real network calls are made anywhere in this file.
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

    // ── SyncAccountAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAccountAsync_DelegatesToQuickBooks_WhenQuickBooksConfigured()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.SyncAccountAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var svc = BuildService(configService, quickBooks: quickBooks);

        var result = await svc.SyncAccountAsync(42);

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("QuickBooks");
        result.ExternalId.Should().Be("42");
        quickBooks.Verify(q => q.SyncAccountAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncAccountAsync_ReturnsFailure_WhenQuickBooksSyncFails()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.SyncAccountAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var svc = BuildService(configService, quickBooks: quickBooks);

        var result = await svc.SyncAccountAsync(42);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SyncInvoiceAsync_DelegatesToXero_WhenOnlyXeroConfigured()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: false, xeroConfigured: true);
        var xero = new Mock<IXeroService>();
        xero.Setup(x => x.SyncInvoiceAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var svc = BuildService(configService, xero: xero);

        var result = await svc.SyncInvoiceAsync(7);

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("Xero");
    }

    // ── TestConnectionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_ReturnsTrue_WhenQuickBooksConnected()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuickBooksConnectionStatus { IsConnected = true });

        var svc = BuildService(configService, quickBooks: quickBooks);

        var connected = await svc.TestConnectionAsync();

        connected.Should().BeTrue();
    }

    // ── SyncPaymentAsync — real HTTP layer (mocked) ────────────────────────

    [Fact]
    public async Task SyncPaymentAsync_ReturnsSuccess_WhenQuickBooksReturns200()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var tokenStore = new IntegrationTokenStore();
        tokenStore.Set("qb:access_token", "test-access-token");
        tokenStore.Set("qb:realm_id", "test-realm-id");

        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, """{"Payment":{"Id":"pay_1"}}""");
        var svc = BuildService(configService, tokenStore: tokenStore, handler: handler);

        var result = await svc.SyncPaymentAsync("pay_1");

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("QuickBooks");
        result.ExternalId.Should().Be("pay_1");
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString().Should().Contain("/payment/pay_1");
        handler.LastRequest.Headers.Authorization!.Parameter.Should().Be("test-access-token");
    }

    [Fact]
    public async Task SyncPaymentAsync_ReturnsFailure_WhenNotConnected()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        // No tokens set in the store => not connected.
        var svc = BuildService(configService, tokenStore: new IntegrationTokenStore());

        var result = await svc.SyncPaymentAsync("pay_1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not connected");
    }

    [Fact]
    public async Task SyncPaymentAsync_ReturnsFailure_WhenHttpReturnsError()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var tokenStore = new IntegrationTokenStore();
        tokenStore.Set("qb:access_token", "test-access-token");
        tokenStore.Set("qb:realm_id", "test-realm-id");

        var handler = new TestHttpMessageHandler(HttpStatusCode.NotFound, """{"Fault":{"Error":[{"Message":"Object Not Found"}]}}""");
        var svc = BuildService(configService, tokenStore: tokenStore, handler: handler);

        var result = await svc.SyncPaymentAsync("missing-payment");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("404");
    }

    // ── RunBatchSyncAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task RunBatchSyncAsync_ReturnsErrorAndNoProcessing_WhenNotConnected()
    {
        var configService = ConfigServiceMock(quickBooksConfigured: true);
        var quickBooks = new Mock<IQuickBooksService>();
        quickBooks.Setup(q => q.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuickBooksConnectionStatus { IsConnected = false });

        var svc = BuildService(configService, quickBooks: quickBooks);

        var result = await svc.RunBatchSyncAsync();

        result.TotalProcessed.Should().Be(0);
        result.Errors.Should().ContainSingle(e => e.Contains("not connected"));
    }

    // ── Test HTTP handler ───────────────────────────────────────────────────

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public HttpRequestMessage? LastRequest { get; private set; }

        public TestHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }
}
