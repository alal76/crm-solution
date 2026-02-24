// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Stub implementation of GeoIP location service (TODO-AUTH-024).
/// In production, integrate with a real GeoIP service (MaxMind, IP2Location, etc.).
/// </summary>
public class GeoLocationService : IGeoLocationService
{
    private readonly ILogger<GeoLocationService> _logger;

    // Known VPN/datacenter IP ranges (simplified for demo)
    private static readonly HashSet<string> KnownVpnProviders = new()
    {
        "NordVPN", "ExpressVPN", "Surfshark", "CyberGhost", "Private Internet Access"
    };

    public GeoLocationService(ILogger<GeoLocationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GeoLocationResult?> LookupAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Stub implementation - returns mock data
        // In production, call a real GeoIP API

        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "127.0.0.1" || ipAddress == "::1")
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

        _logger.LogDebug("GeoIP lookup for {IpAddress} (stub implementation)", ipAddress);

        // Return mock data for demo purposes
        await Task.Delay(1, cancellationToken); // Simulate async call

        return new GeoLocationResult
        {
            IpAddress = ipAddress,
            CountryCode = "US",
            CountryName = "United States",
            RegionCode = "CA",
            RegionName = "California",
            City = "San Francisco",
            PostalCode = "94102",
            Latitude = 37.7749,
            Longitude = -122.4194,
            Timezone = "America/Los_Angeles",
            Isp = "Internet Service Provider",
            IsVpn = false,
            IsTor = false,
            IsDatacenter = false,
            AccuracyRadius = 10
        };
    }

    /// <inheritdoc />
    public async Task<bool> IsVpnOrProxyAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Stub implementation
        // In production, check against VPN/proxy databases

        var location = await LookupAsync(ipAddress, cancellationToken);
        return location?.IsVpn ?? false;
    }

    /// <inheritdoc />
    public async Task<bool> IsTorExitNodeAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Stub implementation
        // In production, check against Tor exit node list

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

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
