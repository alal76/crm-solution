// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Core.Interfaces;
using CRM.Core.Ports;
using CRM.Infrastructure.Services.Integrations;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// GeoIP location service (REV-STUB-007) backed by the MaxMind GeoIP2 Precision: Insights
/// web service — a live, per-lookup REST API call rather than a locally downloaded GeoLite2
/// database.
///
/// Design choice (local DB vs. live web service): <see cref="LookupAsync"/> is called
/// synchronously in-line with login/auth-risk flows (see <see cref="IsImpossibleTravel"/>
/// callers), not in a hot per-request path, so the extra network round-trip of the web
/// service is an acceptable trade-off for this repo. It also avoids needing a background
/// job to download/refresh a multi-hundred-MB GeoLite2.mmdb file and ship the
/// <c>MaxMind.GeoIP2</c> NuGet package, which is out of scope for "wire the API". The
/// Insights endpoint (rather than the cheaper City endpoint) is used specifically because
/// it is the only MaxMind product that returns the VPN/Tor/hosting-provider traits this
/// interface exposes (<see cref="GeoLocationResult.IsVpn"/>, IsTor, IsDatacenter) — the
/// tradeoff is a materially higher per-lookup cost than City or a local GeoLite2 DB.
///
/// Credentials (MaxMind AccountId + LicenseKey) are resolved via
/// <see cref="IProviderConfigurationService"/> — the DB-backed, encrypted provider store used
/// by the Admin &gt; Providers UI (category "GeoIP", provider "MaxMind").
///
/// The Haversine distance math in <see cref="CalculateDistance"/> and the impossible-travel
/// heuristic in <see cref="IsImpossibleTravel"/> were already real (not stubbed) and are
/// left unchanged.
/// </summary>
public class GeoLocationService : IGeoLocationService
{
    private const string Category = "GeoIP";
    private const string Provider = "MaxMind";
    private const string InsightsUrlTemplate = "https://geoip.maxmind.com/geoip/v2.1/insights/{0}";

    private readonly IProviderConfigurationService _configService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(
        IProviderConfigurationService configService,
        IHttpClientFactory httpClientFactory,
        ILogger<GeoLocationService> logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<GeoLocationResult?> LookupAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ipAddress) || IsLoopback(ipAddress))
        {
            return new GeoLocationResult
            {
                IpAddress = ipAddress,
                CountryCode = "US",
                CountryName = "United States",
                City = "Localhost",
                Latitude = 0,
                Longitude = 0
            };
        }

        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Provider, cancellationToken);
        var accountId = ProviderConfigReader.GetValueOrDefault(fields, "AccountId");
        var licenseKey = ProviderConfigReader.GetValueOrDefault(fields, "LicenseKey");

        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(licenseKey))
        {
            _logger.LogWarning("MaxMind GeoIP is not configured (AccountId/LicenseKey missing). Configure it under Admin > Providers > GeoIP.");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(GeoLocationService));
            var url = string.Format(InsightsUrlTemplate, Uri.EscapeDataString(ipAddress));

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accountId}:{licenseKey}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // MaxMind returns 404 for IPs it has no data for (e.g. private ranges) — not an error.
                _logger.LogDebug("MaxMind has no data for {IpAddress}.", ipAddress);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("MaxMind GeoIP lookup failed for {IpAddress}: {Status} — {Error}", ipAddress, response.StatusCode, error);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseInsightsResponse(ipAddress, body);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during MaxMind GeoIP lookup for {IpAddress}", ipAddress);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsVpnOrProxyAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var location = await LookupAsync(ipAddress, cancellationToken);
        return location?.IsVpn ?? false;
    }

    /// <inheritdoc />
    public async Task<bool> IsTorExitNodeAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var location = await LookupAsync(ipAddress, cancellationToken);
        return location?.IsTor ?? false;
    }

    /// <inheritdoc />
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for distance between two points on a sphere
        const double earthRadiusKm = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    /// <inheritdoc />
    public bool IsImpossibleTravel(
        GeoLocationResult location1,
        DateTime timestamp1,
        GeoLocationResult location2,
        DateTime timestamp2)
    {
        if (location1.Latitude == null || location1.Longitude == null ||
            location2.Latitude == null || location2.Longitude == null)
        {
            return false; // Can't determine without coordinates
        }

        var distance = CalculateDistance(
            location1.Latitude.Value, location1.Longitude.Value,
            location2.Latitude.Value, location2.Longitude.Value);

        var timeDifferenceHours = Math.Abs((timestamp2 - timestamp1).TotalHours);

        // Assume max travel speed of 900 km/h (commercial airplane)
        var maxPossibleDistance = timeDifferenceHours * 900;

        // If actual distance is greater than max possible, it's impossible travel
        var isImpossible = distance > maxPossibleDistance;

        if (isImpossible)
        {
            _logger.LogWarning(
                "Impossible travel detected: {Distance:F0}km in {Hours:F1}h (max {MaxDistance:F0}km)",
                distance, timeDifferenceHours, maxPossibleDistance);
        }

        return isImpossible;
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private static bool IsLoopback(string ipAddress) =>
        ipAddress is "127.0.0.1" or "::1";

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    /// <summary>
    /// Parses a MaxMind GeoIP2 Precision Insights JSON response into <see cref="GeoLocationResult"/>.
    /// See https://dev.maxmind.com/geoip/docs/web-services/responses for the response shape.
    /// </summary>
    private static GeoLocationResult ParseInsightsResponse(string ipAddress, string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var result = new GeoLocationResult { IpAddress = ipAddress };

        if (root.TryGetProperty("country", out var country))
        {
            if (country.TryGetProperty("iso_code", out var iso)) result.CountryCode = iso.GetString();
            if (country.TryGetProperty("names", out var names) && names.TryGetProperty("en", out var en)) result.CountryName = en.GetString();
        }

        if (root.TryGetProperty("subdivisions", out var subdivisions) && subdivisions.GetArrayLength() > 0)
        {
            var first = subdivisions[0];
            if (first.TryGetProperty("iso_code", out var subIso)) result.RegionCode = subIso.GetString();
            if (first.TryGetProperty("names", out var subNames) && subNames.TryGetProperty("en", out var subEn)) result.RegionName = subEn.GetString();
        }

        if (root.TryGetProperty("city", out var city) && city.TryGetProperty("names", out var cityNames) && cityNames.TryGetProperty("en", out var cityEn))
        {
            result.City = cityEn.GetString();
        }

        if (root.TryGetProperty("postal", out var postal) && postal.TryGetProperty("code", out var postalCode))
        {
            result.PostalCode = postalCode.GetString();
        }

        if (root.TryGetProperty("location", out var location))
        {
            if (location.TryGetProperty("latitude", out var lat)) result.Latitude = lat.GetDouble();
            if (location.TryGetProperty("longitude", out var lon)) result.Longitude = lon.GetDouble();
            if (location.TryGetProperty("time_zone", out var tz)) result.Timezone = tz.GetString();
            if (location.TryGetProperty("accuracy_radius", out var acc)) result.AccuracyRadius = acc.GetInt32();
        }

        if (root.TryGetProperty("traits", out var traits))
        {
            if (traits.TryGetProperty("isp", out var isp)) result.Isp = isp.GetString();
            if (traits.TryGetProperty("organization", out var org)) result.Organization = org.GetString();
            if (traits.TryGetProperty("autonomous_system_number", out var asn)) result.Asn = asn.GetRawText();
            if (traits.TryGetProperty("is_anonymous_vpn", out var vpn)) result.IsVpn = vpn.GetBoolean();
            if (traits.TryGetProperty("is_tor_exit_node", out var tor)) result.IsTor = tor.GetBoolean();
            if (traits.TryGetProperty("is_hosting_provider", out var hosting)) result.IsDatacenter = hosting.GetBoolean();
        }

        return result;
    }
}
