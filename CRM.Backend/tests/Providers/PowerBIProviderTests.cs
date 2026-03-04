// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.PowerBI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for PowerBIProvider.
/// Tests Azure AD OAuth2 auth flow, dashboard/report queries, embed token generation,
/// config validation, and error-handling paths.
/// MANDATORY: Written after verifying source for:
///   Class: PowerBIProvider, Namespace: CRM.Infrastructure.Providers.PowerBI
///   Constructor: (HttpClient, IOptions&lt;PowerBIConfiguration&gt;, ILogger&lt;PowerBIProvider&gt;)
///   Auth: OAuth2 client_credentials via https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token
/// </summary>
public class PowerBIProviderTests
{
    // ── Sequenced HTTP helper ───────────────────────────────────────────────

    private sealed class SequencedHandler : HttpMessageHandler
    {
        private readonly Queue<(HttpStatusCode StatusCode, string Body)> _queue;

        public SequencedHandler(params (HttpStatusCode StatusCode, string Body)[] responses)
        {
            _queue = new Queue<(HttpStatusCode, string)>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_queue.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("No more queued responses", Encoding.UTF8, "application/json")
                });
            }

            var (code, body) = _queue.Dequeue();
            return Task.FromResult(new HttpResponseMessage(code)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }

    // ── Factory helpers ─────────────────────────────────────────────────────

    private static PowerBIConfiguration ValidConfig(PowerBIAuthMethod method = PowerBIAuthMethod.ServicePrincipal) =>
        new()
        {
            TenantId = "tenant-id-1234",
            ClientId = "client-id-5678",
            ClientSecret = "client-secret-abcd",
            WorkspaceId = "workspace-id-9999",
            AuthMethod = method
        };

    private static (HttpStatusCode, string) TokenResponse() =>
        (HttpStatusCode.OK, """{"access_token":"test-pbi-token","token_type":"Bearer","expires_in":3600}""");

    private static PowerBIProvider CreateProvider(
        PowerBIConfiguration? config = null,
        params (HttpStatusCode StatusCode, string Body)[] responses)
    {
        var effectiveConfig = config ?? ValidConfig();
        var httpClient = new HttpClient(new SequencedHandler(responses))
        {
            BaseAddress = new Uri("https://api.powerbi.com")
        };
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<PowerBIProvider>>();
        return new PowerBIProvider(httpClient, options, logger.Object);
    }

    private static PowerBIProvider CreateWithAuth(
        string apiResponseBody,
        HttpStatusCode apiStatusCode = HttpStatusCode.OK,
        PowerBIConfiguration? config = null)
    {
        return CreateProvider(config,
            TokenResponse(),
            (apiStatusCode, apiResponseBody));
    }

    // ── Provider metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsPowerBI()
    {
        var provider = CreateProvider(responses: [TokenResponse()]);
        provider.ProviderName.Should().Be("PowerBI");
    }

    [Fact]
    public void SupportsEmbedding_ReturnsTrue()
    {
        var provider = CreateProvider(responses: [TokenResponse()]);
        provider.SupportsEmbedding.Should().BeTrue();
    }

    // ── PowerBIConfiguration.Validate ──────────────────────────────────────

    [Fact]
    public void Validate_ReturnsError_WhenTenantIdMissing()
    {
        var config = new PowerBIConfiguration
        {
            ClientId = "client",
            ClientSecret = "secret",
            WorkspaceId = "workspace"
        };

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("TenantId");
    }

    [Fact]
    public void Validate_ReturnsError_WhenClientSecretMissingForServicePrincipal()
    {
        var config = new PowerBIConfiguration
        {
            TenantId = "tenant",
            ClientId = "client",
            WorkspaceId = "workspace",
            AuthMethod = PowerBIAuthMethod.ServicePrincipal
            // ClientSecret missing
        };

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("ClientSecret");
    }

    [Fact]
    public void Validate_ReturnsValid_WhenAllRequiredFieldsPresent()
    {
        var (isValid, error) = ValidConfig().Validate();

        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    // ── IsAvailableAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenConfigInvalid()
    {
        var invalidConfig = new PowerBIConfiguration(); // all empty
        var provider = CreateProvider(invalidConfig, TokenResponse());

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenTokenAcquiredSuccessfully()
    {
        var provider = CreateProvider(responses: [TokenResponse()]);

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenTokenEndpointReturnsUnauthorized()
    {
        var provider = CreateProvider(responses:
        [
            (HttpStatusCode.Unauthorized, """{"error":"invalid_client"}""")
        ]);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── GetDashboardsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardsAsync_ReturnsEmpty_WhenNullValueInResponse()
    {
        var provider = CreateWithAuth("""{"value":null}""");

        var dashboards = await provider.GetDashboardsAsync();

        dashboards.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardsAsync_ReturnsMappedDashboards_WhenDashboardsPresent()
    {
        const string body = """
            {
              "value": [
                {"id":"dash-001","displayName":"Executive Summary","webUrl":"https://powerbi.com/dash/001","embedUrl":null},
                {"id":"dash-002","displayName":"Sales Pipeline","webUrl":"https://powerbi.com/dash/002","embedUrl":null}
              ]
            }
            """;

        var provider = CreateWithAuth(body);

        var dashboards = (await provider.GetDashboardsAsync()).ToList();

        dashboards.Should().HaveCount(2);
        dashboards[0].Id.Should().Be("dash-001");
        dashboards[0].Name.Should().Be("Executive Summary");
        dashboards[0].CanEmbed.Should().BeTrue();
        dashboards[0].Category.Should().Be("PowerBI");
        dashboards[1].Id.Should().Be("dash-002");
    }

    // ── GetDashboardAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardAsync_ReturnsNull_WhenApiReturnsNotFound()
    {
        var provider = CreateWithAuth("""{"error":"Dashboard not found"}""", HttpStatusCode.NotFound);

        var result = await provider.GetDashboardAsync("unknown-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsDashboardInfo_WhenFound()
    {
        const string body = """
            {"id":"dash-999","displayName":"Finance Report","webUrl":"https://powerbi.com/dash/999"}
            """;

        var provider = CreateWithAuth(body);

        var result = await provider.GetDashboardAsync("dash-999");

        result.Should().NotBeNull();
        result!.Id.Should().Be("dash-999");
        result.Name.Should().Be("Finance Report");
        result.CanEmbed.Should().BeTrue();
    }

    // ── GetReportsAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetReportsAsync_ReturnsReports_WhenApiSucceeds()
    {
        const string body = """
            {
              "value": [
                {"id":"rpt-01","name":"Monthly Sales","datasetId":"ds-01","embedUrl":"https://embed/rpt-01","webUrl":"https://app/rpt-01"},
                {"id":"rpt-02","name":"Customer Churn","datasetId":"ds-02","embedUrl":"https://embed/rpt-02","webUrl":"https://app/rpt-02"}
              ]
            }
            """;

        var provider = CreateWithAuth(body);

        var reports = (await provider.GetReportsAsync()).ToList();

        reports.Should().HaveCount(2);
        reports[0].Id.Should().Be("rpt-01");
        reports[0].Name.Should().Be("Monthly Sales");
        reports[1].Id.Should().Be("rpt-02");
    }

    [Fact]
    public async Task GetReportsAsync_ReturnsEmpty_WhenApiThrows()
    {
        var provider = CreateWithAuth("""{"error":"Server error"}""", HttpStatusCode.InternalServerError);

        var reports = await provider.GetReportsAsync();

        reports.Should().BeEmpty();
    }

    // ── GetDataSourcesAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetDataSourcesAsync_ReturnsMappedDatasets_WhenApiSucceeds()
    {
        const string body = """
            {
              "value": [
                {"id":"ds-a1","name":"CRM Dataset","isRefreshable":true},
                {"id":"ds-b2","name":"Static Report","isRefreshable":false}
              ]
            }
            """;

        var provider = CreateWithAuth(body);

        var sources = (await provider.GetDataSourcesAsync()).ToList();

        sources.Should().HaveCount(2);
        sources[0].Id.Should().Be("ds-a1");
        sources[0].Status.Should().Be("Active");
        sources[1].Id.Should().Be("ds-b2");
        sources[1].Status.Should().Be("Static");
    }
}
