// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Superset;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for SupersetProvider.
/// Tests authentication flow, dashboard/chart operations, embed token generation,
/// and error-handling paths.
/// MANDATORY: Written after verifying source for:
///   Class: SupersetProvider, Namespace: CRM.Infrastructure.Providers.Superset
///   Constructor: (HttpClient, IOptions&lt;SupersetConfiguration&gt;, ILogger&lt;SupersetProvider&gt;)
///   Auth: two-step CSRF + login via REST API.
/// </summary>
public class SupersetProviderTests
{
    // ── Sequenced HTTP helper ───────────────────────────────────────────────

    /// <summary>
    /// Returns responses from a queue so that multi-step HTTP flows can be tested.
    /// </summary>
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

    private static SupersetConfiguration DefaultConfig() => new()
    {
        BaseUrl = "http://superset.test",
        Username = "admin",
        Password = "superset123",
        Provider = "db",
        TokenRefreshIntervalMinutes = 50,
        TimeoutSeconds = 30
    };

    /// <summary>
    /// Standard auth sequence: csrf → login.
    /// </summary>
    private static (HttpStatusCode, string)[] AuthSequence() => new[]
    {
        (HttpStatusCode.OK, """{"result":"csrf-test-token"}"""),
        (HttpStatusCode.OK, """{"access_token":"test-access-token","refresh_token":"test-refresh"}""")
    };

    private static SupersetProvider CreateProvider(
        SupersetConfiguration? config = null,
        params (HttpStatusCode StatusCode, string Body)[] responses)
    {
        var effectiveConfig = config ?? DefaultConfig();
        var httpClient = new HttpClient(new SequencedHandler(responses));
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<SupersetProvider>>();
        return new SupersetProvider(httpClient, options, logger.Object);
    }

    /// <summary>Returns a provider pre-seeded with auth + one additional response.</summary>
    private static SupersetProvider CreateWithAuth(
        string apiResponseBody,
        HttpStatusCode apiStatusCode = HttpStatusCode.OK,
        SupersetConfiguration? config = null)
    {
        var authSeq = AuthSequence();
        var all = new[]
        {
            authSeq[0],
            authSeq[1],
            (apiStatusCode, apiResponseBody)
        };
        return CreateProvider(config, all);
    }

    // ── Provider metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsSuperset()
    {
        // No HTTP calls expected for metadata
        var provider = CreateProvider(responses: AuthSequence());
        provider.ProviderName.Should().Be("Superset");
    }

    [Fact]
    public void SupportsEmbedding_ReturnsTrue()
    {
        var provider = CreateProvider(responses: AuthSequence());
        provider.SupportsEmbedding.Should().BeTrue();
    }

    // ── IsAvailableAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenAuthSucceeds()
    {
        var provider = CreateProvider(responses: AuthSequence());

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenCsrfEndpointFails()
    {
        var provider = CreateProvider(responses:
        [
            (HttpStatusCode.Unauthorized, """{"message":"Unauthorized"}""")
        ]);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenLoginFails()
    {
        var provider = CreateProvider(responses:
        [
            (HttpStatusCode.OK, """{"result":"csrf-token"}"""),
            (HttpStatusCode.Unauthorized, """{"message":"Invalid credentials"}""")
        ]);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── GetDashboardsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardsAsync_ReturnsEmpty_WhenNoDashboardsInResponse()
    {
        var provider = CreateWithAuth("""{"count":0,"result":[]}""");

        var dashboards = await provider.GetDashboardsAsync();

        dashboards.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardsAsync_ReturnsMappedDashboards_WhenResultsPresent()
    {
        const string body = """
            {
              "count": 2,
              "result": [
                {"id":1,"dashboard_title":"Sales Overview","description":"Sales desc","slug":"sales","url":"/dashboard/1"},
                {"id":2,"dashboard_title":"Support KPIs","description":null,"slug":null,"url":"/dashboard/2"}
              ]
            }
            """;

        var provider = CreateWithAuth(body);

        var dashboards = (await provider.GetDashboardsAsync()).ToList();

        dashboards.Should().HaveCount(2);
        dashboards[0].Id.Should().Be("1");
        dashboards[0].Name.Should().Be("Sales Overview");
        dashboards[0].CanEmbed.Should().BeTrue();
        dashboards[1].Id.Should().Be("2");
        dashboards[1].Name.Should().Be("Support KPIs");
    }

    // ── GetDashboardAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardAsync_ReturnsNull_WhenIdIsNotNumeric()
    {
        var provider = CreateProvider(responses: AuthSequence());
        // Auth happens on first call; non-numeric ID should short-circuit without HTTP call
        var result = await provider.GetDashboardAsync("not-a-number");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsNull_WhenHttpReturnsNotFound()
    {
        var provider = CreateWithAuth("""{"message":"Not found"}""", HttpStatusCode.NotFound);

        var result = await provider.GetDashboardAsync("99");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsDashboardInfo_WhenFound()
    {
        const string body = """
            {"result":{"id":42,"dashboard_title":"My Dashboard","description":"Desc","slug":"my-dash","url":"/dashboard/42"}}
            """;

        var provider = CreateWithAuth(body);

        var result = await provider.GetDashboardAsync("42");

        result.Should().NotBeNull();
        result!.Id.Should().Be("42");
        result.Name.Should().Be("My Dashboard");
        result.CanEmbed.Should().BeTrue();
    }

    // ── GetChartsAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetChartsAsync_ReturnsCharts_WhenApiRespondsSuccessfully()
    {
        const string body = """
            {
              "count": 1,
              "result": [
                {"id":10,"slice_name":"Revenue Trend","description":"Rev chart","viz_type":"line","width":600,"height":400}
              ]
            }
            """;

        var provider = CreateWithAuth(body);

        var charts = (await provider.GetChartsAsync()).ToList();

        charts.Should().HaveCount(1);
        charts[0].Id.Should().Be("10");
        charts[0].Name.Should().Be("Revenue Trend");
        charts[0].ChartType.Should().Be("line");
        charts[0].Width.Should().Be(600);
        charts[0].Height.Should().Be(400);
    }

    // ── GetChartEmbedAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetChartEmbedAsync_ReturnsError_WhenChartIdIsNotNumeric()
    {
        var provider = CreateProvider(responses: AuthSequence());

        var result = await provider.GetChartEmbedAsync("not-an-id");

        result.EmbedType.Should().Be("error");
        result.Config.Should().ContainKey("error");
    }

    [Fact]
    public async Task GetChartEmbedAsync_ReturnsIframe_WhenChartIdIsNumeric()
    {
        var provider = CreateProvider(responses: AuthSequence());

        var result = await provider.GetChartEmbedAsync("7");

        result.EmbedType.Should().Be("iframe");
        result.EmbedUrl.Should().Contain("slice_id=7");
        result.Height.Should().Be(400);
    }

    // ── ExecuteReportAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteReportAsync_ReturnsFailure_WhenReportIdIsNotNumeric()
    {
        var provider = CreateProvider(responses: AuthSequence());

        var result = await provider.ExecuteReportAsync("not-numeric");

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Invalid report/chart ID");
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsFailure_WhenApiReturnsError()
    {
        var provider = CreateWithAuth("""{"message":"Not found"}""", HttpStatusCode.NotFound);

        var result = await provider.ExecuteReportAsync("5");

        result.Success.Should().BeFalse();
    }
}
