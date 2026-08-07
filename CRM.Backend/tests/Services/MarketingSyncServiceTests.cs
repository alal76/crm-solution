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
/// Unit tests for MarketingSyncService (REV-STUB-003).
///
/// Contact sync delegates to IMailchimpService/IHubSpotService (mocked here; their own HTTP
/// behavior is covered by MailchimpServiceTests/HubSpotServiceTests). The new capabilities
/// (list enumeration, campaign metrics, subscriber import, segment sync) make their own
/// direct HTTP calls to Mailchimp, which are mocked at the HTTP layer via a fake handler
/// behind IHttpClientFactory — no real network calls are made anywhere in this file.
///
/// MANDATORY: Written after verifying source for:
///   Class: MarketingSyncService, Namespace: CRM.Infrastructure.Services.Integrations
///   Constructor: (IProviderConfigurationService, IMailchimpService, IHubSpotService,
///                 IHttpClientFactory, ILogger)
/// </summary>
public class MarketingSyncServiceTests
{
    private static ProviderConfigurationDto ConfigDto(object data) => new()
    {
        Id = 1,
        ConfigurationKey = "crm.marketing.mailchimp",
        ConfigurationType = "crm",
        ConfigurationData = JsonSerializer.Serialize(data),
        IsEncrypted = false,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static Mock<IProviderConfigurationService> ConfigServiceMock(
        bool mailchimpConfigured, bool hubspotConfigured = false)
    {
        var mock = new Mock<IProviderConfigurationService>();

        mock.Setup(m => m.GetConfigurationAsync("crm.marketing.mailchimp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mailchimpConfigured
                ? ConfigDto(new { ApiKey = "abc123-us21", ServerPrefix = "us21", ListId = "list-1" })
                : null);

        mock.Setup(m => m.GetConfigurationAsync("crm.marketing.hubspot", It.IsAny<CancellationToken>()))
            .ReturnsAsync(hubspotConfigured ? ConfigDto(new { AccessToken = "hs-token" }) : null);

        return mock;
    }

    private static MarketingSyncService BuildService(
        Mock<IProviderConfigurationService> configService,
        Mock<IMailchimpService>? mailchimp = null,
        Mock<IHubSpotService>? hubspot = null,
        HttpMessageHandler? handler = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler ?? new TestHttpMessageHandler(HttpStatusCode.OK, "{}")));

        return new MarketingSyncService(
            configService.Object,
            (mailchimp ?? new Mock<IMailchimpService>()).Object,
            (hubspot ?? new Mock<IHubSpotService>()).Object,
            factory.Object,
            Mock.Of<ILogger<MarketingSyncService>>());
    }

    // ── SyncContactAsync (delegation) ──────────────────────────────────────

    [Fact]
    public async Task SyncContactAsync_DelegatesToMailchimp_WhenMailchimpConfigured()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: true);
        var mailchimp = new Mock<IMailchimpService>();
        mailchimp.Setup(m => m.SyncContactAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var svc = BuildService(configService, mailchimp: mailchimp);

        var result = await svc.SyncContactAsync(5, "list-1");

        result.Success.Should().BeTrue();
        result.Provider.Should().Be("Mailchimp");
    }

    [Fact]
    public async Task SyncContactAsync_ReturnsFailure_WhenDelegateFails()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: true);
        var mailchimp = new Mock<IMailchimpService>();
        mailchimp.Setup(m => m.SyncContactAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var svc = BuildService(configService, mailchimp: mailchimp);

        var result = await svc.SyncContactAsync(5, "list-1");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ── GetListsAsync — real HTTP layer (mocked) ───────────────────────────

    [Fact]
    public async Task GetListsAsync_ReturnsLists_WhenMailchimpReturns200()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: true);
        const string json = """
        {
          "lists": [
            { "id": "list-1", "name": "Newsletter", "stats": { "member_count": 42 } }
          ]
        }
        """;
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, json);
        var svc = BuildService(configService, handler: handler);

        var lists = await svc.GetListsAsync();

        lists.Should().ContainSingle();
        lists[0].Id.Should().Be("list-1");
        lists[0].Name.Should().Be("Newsletter");
        lists[0].MemberCount.Should().Be(42);
        handler.LastRequest!.RequestUri!.ToString().Should().Contain("us21.api.mailchimp.com");
    }

    [Fact]
    public async Task GetListsAsync_ReturnsEmpty_WhenMailchimpReturnsError()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: true);
        var handler = new TestHttpMessageHandler(HttpStatusCode.Unauthorized, """{"detail":"Invalid API key"}""");
        var svc = BuildService(configService, handler: handler);

        var lists = await svc.GetListsAsync();

        lists.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListsAsync_ReturnsEmpty_WhenProviderIsHubSpot()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: false, hubspotConfigured: true);
        var svc = BuildService(configService);

        var lists = await svc.GetListsAsync();

        lists.Should().BeEmpty();
    }

    // ── ImportSubscribersAsLeadsAsync ──────────────────────────────────────

    [Fact]
    public async Task ImportSubscribersAsLeadsAsync_ReturnsCount_WhenMailchimpReturns200()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: true);
        const string json = """{ "members": [ {"email_address":"a@x.com"}, {"email_address":"b@x.com"} ] }""";
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, json);
        var svc = BuildService(configService, handler: handler);

        var result = await svc.ImportSubscribersAsLeadsAsync("list-1");

        result.TotalImported.Should().Be(2);
    }

    // ── TestConnectionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_DelegatesToHubSpot_WhenOnlyHubSpotConfigured()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: false, hubspotConfigured: true);
        var hubspot = new Mock<IHubSpotService>();
        hubspot.Setup(h => h.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HubSpotConnectionStatus { IsConnected = true });

        var svc = BuildService(configService, hubspot: hubspot);

        var connected = await svc.TestConnectionAsync();

        connected.Should().BeTrue();
    }

    // ── SyncSegmentAsync — HubSpot not supported ───────────────────────────

    [Fact]
    public async Task SyncSegmentAsync_ReturnsFailure_WhenProviderIsHubSpot()
    {
        var configService = ConfigServiceMock(mailchimpConfigured: false, hubspotConfigured: true);
        var svc = BuildService(configService);

        var result = await svc.SyncSegmentAsync("VIP", new List<int> { 1, 2 });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Mailchimp");
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
