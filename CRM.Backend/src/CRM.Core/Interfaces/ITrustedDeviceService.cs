// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

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
    Task<TrustedDeviceDto> TrustDeviceAsync(
        int userId,
        string deviceFingerprint,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null,
        int trustDurationDays = 30,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a device is trusted for a user.
    /// </summary>
    Task<bool> IsDeviceTrustedAsync(
        int userId,
        string deviceFingerprint,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the last used timestamp for a trusted device.
    /// </summary>
    Task UpdateLastUsedAsync(
        int userId,
        string deviceFingerprint,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all trusted devices for a user.
    /// </summary>
    Task<IEnumerable<TrustedDeviceDto>> GetTrustedDevicesAsync(
        int userId,
        bool includeExpired = false,
        CancellationToken ct = default);

    /// <summary>
    /// Revokes trust for a specific device.
    /// </summary>
    Task<bool> RevokeDeviceAsync(
        int userId,
        int deviceId,
        CancellationToken ct = default);

    /// <summary>
    /// Revokes trust for all devices of a user.
    /// </summary>
    Task<int> RevokeAllDevicesAsync(
        int userId,
        CancellationToken ct = default);

    /// <summary>
    /// Cleans up expired trusted device records.
    /// </summary>
    Task<int> CleanupExpiredDevicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Extends trust for a specific device.
    /// </summary>
    Task<bool> ExtendTrustAsync(
        int userId,
        string deviceFingerprint,
        int extensionDays = 30,
        CancellationToken ct = default);
}
