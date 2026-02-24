// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing 2FA trusted devices (TODO-AUTH-019).
/// Allows users to skip 2FA on trusted devices within a trust period.
/// </summary>
public interface ITrustedDeviceService
{
    /// <summary>
    /// Registers a device as trusted for a user after successful 2FA.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Unique device identifier</param>
    /// <param name="deviceName">Friendly device name</param>
    /// <param name="userAgent">User agent string</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="fingerprintHash">Optional device fingerprint hash</param>
    /// <param name="trustDurationDays">How many days to trust the device (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created trusted device record</returns>
    Task<TrustedDevice> TrustDeviceAsync(
        int userId,
        string deviceId,
        string? deviceName,
        string? userAgent,
        string? ipAddress,
        string? fingerprintHash = null,
        int trustDurationDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a device is trusted for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="fingerprintHash">Optional fingerprint hash for additional verification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if device is trusted and not expired</returns>
    Task<bool> IsDeviceTrustedAsync(
        int userId,
        string deviceId,
        string? fingerprintHash = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last used timestamp for a trusted device.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateLastUsedAsync(
        int userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all trusted devices for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trusted devices</returns>
    Task<IEnumerable<TrustedDevice>> GetTrustedDevicesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes trust for a specific device.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeDeviceTrustAsync(
        int userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes trust for all devices of a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeAllDevicesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired trusted device records.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records removed</returns>
    Task<int> CleanupExpiredDevicesAsync(CancellationToken cancellationToken = default);
}
