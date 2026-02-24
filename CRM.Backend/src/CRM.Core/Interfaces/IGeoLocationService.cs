// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for geolocation/GeoIP lookups (TODO-AUTH-024).
/// Provides location information based on IP addresses.
/// </summary>
public interface IGeoLocationService
{
    /// <summary>
    /// Looks up geographic location for an IP address.
    /// </summary>
    /// <param name="ipAddress">IP address to lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location information or null if not found</returns>
    Task<GeoLocationResult?> LookupAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an IP address is from a known VPN/proxy.
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsVpnOrProxyAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an IP address is from a known Tor exit node.
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsTorExitNodeAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates distance between two geographic points.
    /// </summary>
    /// <param name="lat1">Latitude of first point</param>
    /// <param name="lon1">Longitude of first point</param>
    /// <param name="lat2">Latitude of second point</param>
    /// <param name="lon2">Longitude of second point</param>
    /// <returns>Distance in kilometers</returns>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Detects impossible travel between two login locations.
    /// </summary>
    /// <param name="location1">First location</param>
    /// <param name="timestamp1">Timestamp of first login</param>
    /// <param name="location2">Second location</param>
    /// <param name="timestamp2">Timestamp of second login</param>
    /// <returns>True if travel between locations is impossible given the time difference</returns>
    bool IsImpossibleTravel(
        GeoLocationResult location1,
        DateTime timestamp1,
        GeoLocationResult location2,
        DateTime timestamp2);
}

/// <summary>
/// Result of geolocation lookup
/// </summary>
public class GeoLocationResult
{
    /// <summary>
    /// IP address that was looked up
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "GB")
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Country name
    /// </summary>
    public string? CountryName { get; set; }

    /// <summary>
    /// Region/State/Province code
    /// </summary>
    public string? RegionCode { get; set; }

    /// <summary>
    /// Region/State/Province name
    /// </summary>
    public string? RegionName { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Latitude
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Timezone (IANA format)
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// ISP name
    /// </summary>
    public string? Isp { get; set; }

    /// <summary>
    /// Organization/Company name
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// AS number
    /// </summary>
    public string? Asn { get; set; }

    /// <summary>
    /// Whether this is a known VPN/proxy
    /// </summary>
    public bool IsVpn { get; set; }

    /// <summary>
    /// Whether this is a known Tor exit node
    /// </summary>
    public bool IsTor { get; set; }

    /// <summary>
    /// Whether this is a datacenter/hosting IP
    /// </summary>
    public bool IsDatacenter { get; set; }

    /// <summary>
    /// Confidence score (0-100) of the location accuracy
    /// </summary>
    public int? AccuracyRadius { get; set; }
}
