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
using CRM.Core.Ports;
using CRM.Infrastructure.Services.Auth;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for GeoLocationService (REV-STUB-007) backed by the MaxMind GeoIP2 Precision
/// Insights web service. All HTTP calls are mocked via a fake handler behind
/// IHttpClientFactory — no real network calls are made anywhere in this file.
///
/// MANDATORY: Written after verifying source for:
///   Class: GeoLocationService, Namespace: CRM.Infrastructure.Services.Auth
///   Constructor: (IProviderConfigurationService, IHttpClientFactory, ILogger)
/// </summary>
public class GeoLocationServiceTests
{
    private static Mock<IProviderConfigurationService> ConfigServiceMock(string? accountId, string? licenseKey)
    {
        var mock = new Mock<IProviderConfigurationService>();
        mock.Setup(m => m.GetConfigurationAsync("crm.geoip.maxmind", It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountId == null
                ? null
                : new ProviderConfigurationDto
                {
                    Id = 1,
                    ConfigurationKey = "crm.geoip.maxmind",
                    ConfigurationType = "crm",
                    ConfigurationData = JsonSerializer.Serialize(new { AccountId = accountId, LicenseKey = licenseKey }),
                    IsEncrypted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        return mock;
    }

    private static GeoLocationService BuildService(
        Mock<IProviderConfigurationService> configService,
        HttpMessageHandler? handler = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler ?? new TestHttpMessageHandler(HttpStatusCode.OK, "{}")));

        return new GeoLocationService(configService.Object, factory.Object, Mock.Of<ILogger<GeoLocationService>>());
    }

    private const string InsightsResponse = """
    {
      "country": { "iso_code": "US", "names": { "en": "United States" } },
      "subdivisions": [ { "iso_code": "CA", "names": { "en": "California" } } ],
      "city": { "names": { "en": "San Francisco" } },
      "postal": { "code": "94102" },
      "location": { "latitude": 37.7749, "longitude": -122.4194, "time_zone": "America/Los_Angeles", "accuracy_radius": 10 },
      "traits": {
        "isp": "Example ISP",
        "organization": "Example Org",
        "autonomous_system_number": 12345,
        "is_anonymous_vpn": true,
        "is_tor_exit_node": false,
        "is_hosting_provider": false
      }
    }
    """;

    // ── LookupAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task LookupAsync_ReturnsLocalhostResult_WithoutHttpCall_ForLoopbackAddress()
    {
        var configService = ConfigServiceMock(accountId: null, licenseKey: null);
        var svc = BuildService(configService);

        var result = await svc.LookupAsync("127.0.0.1");

        result.Should().NotBeNull();
        result!.City.Should().Be("Localhost");
        configService.Verify(c => c.GetConfigurationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LookupAsync_ReturnsNull_WhenNotConfigured()
    {
        var configService = ConfigServiceMock(accountId: null, licenseKey: null);
        var svc = BuildService(configService);

        var result = await svc.LookupAsync("8.8.8.8");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LookupAsync_ReturnsParsedLocation_WhenMaxMindReturns200()
    {
        var configService = ConfigServiceMock("123456", "test-license-key");
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, InsightsResponse);
        var svc = BuildService(configService, handler);

        var result = await svc.LookupAsync("8.8.8.8");

        result.Should().NotBeNull();
        result!.CountryCode.Should().Be("US");
        result.City.Should().Be("San Francisco");
        result.Latitude.Should().Be(37.7749);
        result.IsVpn.Should().BeTrue();
        result.IsTor.Should().BeFalse();
        handler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Basic");
    }

    [Fact]
    public async Task LookupAsync_ReturnsNull_WhenMaxMindReturns404()
    {
        var configService = ConfigServiceMock("123456", "test-license-key");
        var handler = new TestHttpMessageHandler(HttpStatusCode.NotFound, """{"error":"not found"}""");
        var svc = BuildService(configService, handler);

        var result = await svc.LookupAsync("0.0.0.0");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LookupAsync_ReturnsNull_WhenMaxMindReturnsUnauthorized()
    {
        var configService = ConfigServiceMock("123456", "wrong-key");
        var handler = new TestHttpMessageHandler(HttpStatusCode.Unauthorized, """{"error":"bad auth"}""");
        var svc = BuildService(configService, handler);

        var result = await svc.LookupAsync("8.8.8.8");

        result.Should().BeNull();
    }

    // ── IsVpnOrProxyAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task IsVpnOrProxyAsync_ReturnsTrue_WhenTraitsIndicateVpn()
    {
        var configService = ConfigServiceMock("123456", "test-license-key");
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, InsightsResponse);
        var svc = BuildService(configService, handler);

        var isVpn = await svc.IsVpnOrProxyAsync("8.8.8.8");

        isVpn.Should().BeTrue();
    }

    // ── CalculateDistance / IsImpossibleTravel (pure math, unchanged) ──────

    [Fact]
    public void CalculateDistance_ReturnsApproximateKnownDistance_SFtoNY()
    {
        var svc = BuildService(ConfigServiceMock(null, null));

        // San Francisco -> New York is ~4,130 km great-circle distance.
        var distance = svc.CalculateDistance(37.7749, -122.4194, 40.7128, -74.0060);

        distance.Should().BeApproximately(4130, 50);
    }

    [Fact]
    public void IsImpossibleTravel_ReturnsTrue_ForImpossibleSpeed()
    {
        var svc = BuildService(ConfigServiceMock(null, null));
        var sf = new CRM.Core.Interfaces.GeoLocationResult { Latitude = 37.7749, Longitude = -122.4194 };
        var ny = new CRM.Core.Interfaces.GeoLocationResult { Latitude = 40.7128, Longitude = -74.0060 };

        var result = svc.IsImpossibleTravel(sf, DateTime.UtcNow, ny, DateTime.UtcNow.AddMinutes(5));

        result.Should().BeTrue();
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
