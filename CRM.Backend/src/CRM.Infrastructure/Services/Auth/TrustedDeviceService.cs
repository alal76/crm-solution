// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Service implementation for managing trusted devices (2FA device trust).
/// TODO-AUTH-019: Trusted Device Support
/// </summary>
public class TrustedDeviceService : ITrustedDeviceService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<TrustedDeviceService> _logger;

    public TrustedDeviceService(ICrmDbContext db, ILogger<TrustedDeviceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TrustedDeviceDto> TrustDeviceAsync(
        int userId,
        string deviceFingerprint,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null,
        int trustDurationDays = 30,
        CancellationToken ct = default)
    {
        // Check if device already trusted (update instead of creating new)
        var existingDevice = await _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceFingerprint && !d.IsDeleted, ct);

        if (existingDevice != null)
        {
            existingDevice.ExpiresAt = DateTime.UtcNow.AddDays(trustDurationDays);
            existingDevice.LastUsedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(deviceName)) existingDevice.DeviceName = deviceName;
            if (!string.IsNullOrEmpty(ipAddress)) existingDevice.IpAddress = ipAddress;
            if (!string.IsNullOrEmpty(userAgent)) existingDevice.UserAgent = userAgent;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Extended trusted device {DeviceId} for user {UserId}", existingDevice.Id, userId);

            return MapToDto(existingDevice);
        }

        // Create new trusted device
        var device = new TrustedDevice
        {
            UserId = userId,
            DeviceId = deviceFingerprint,
            DeviceName = deviceName ?? DeriveDeviceName(userAgent),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(trustDurationDays),
            CreatedAt = DateTime.UtcNow,
        };

        _db.TrustedDevices.Add(device);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Trusted device {DeviceId} created for user {UserId}, expires {Expiry}", device.Id, userId, device.ExpiresAt);

        return MapToDto(device);
    }

    /// <inheritdoc />
    public async Task<bool> IsDeviceTrustedAsync(int userId, string deviceFingerprint, CancellationToken ct = default)
    {
        return await _db.TrustedDevices
            .AnyAsync(d => d.UserId == userId
                        && d.DeviceId == deviceFingerprint
                        && !d.IsDeleted
                        && d.ExpiresAt > DateTime.UtcNow, ct);
    }

    /// <inheritdoc />
    public async Task UpdateLastUsedAsync(int userId, string deviceFingerprint, CancellationToken ct = default)
    {
        var device = await _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceFingerprint && !d.IsDeleted, ct);

        if (device != null)
        {
            device.LastUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TrustedDeviceDto>> GetTrustedDevicesAsync(int userId, bool includeExpired = false, CancellationToken ct = default)
    {
        var query = _db.TrustedDevices
            .Where(d => d.UserId == userId && !d.IsDeleted);

        if (!includeExpired)
        {
            query = query.Where(d => d.ExpiresAt > DateTime.UtcNow);
        }

        var devices = await query
            .OrderByDescending(d => d.LastUsedAt ?? d.CreatedAt)
            .ToListAsync(ct);

        return devices.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<bool> RevokeDeviceAsync(int userId, int deviceId, CancellationToken ct = default)
    {
        var device = await _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == deviceId, ct);

        if (device == null) return false;

        device.IsDeleted = true;
        device.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Revoked trusted device {DeviceId} for user {UserId}", deviceId, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<int> RevokeAllDevicesAsync(int userId, CancellationToken ct = default)
    {
        var devices = await _db.TrustedDevices
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .ToListAsync(ct);

        foreach (var device in devices)
        {
            device.IsDeleted = true;
            device.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Revoked {Count} trusted devices for user {UserId}", devices.Count, userId);
        return devices.Count;
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredDevicesAsync(CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90); // Remove devices expired more than 90 days ago

        var expiredDevices = await _db.TrustedDevices
            .Where(d => d.ExpiresAt < cutoffDate || (d.IsDeleted && d.UpdatedAt < cutoffDate))
            .ToListAsync(ct);

        foreach (var device in expiredDevices)
        {
            _db.TrustedDevices.Remove(device);
        }

        await _db.SaveChangesAsync(ct);

        if (expiredDevices.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired trusted devices", expiredDevices.Count);
        }

        return expiredDevices.Count;
    }

    /// <inheritdoc />
    public async Task<bool> ExtendTrustAsync(int userId, string deviceFingerprint, int extensionDays = 30, CancellationToken ct = default)
    {
        var device = await _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceFingerprint && !d.IsDeleted, ct);

        if (device == null) return false;

        device.ExpiresAt = DateTime.UtcNow.AddDays(extensionDays);
        device.LastUsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Extended trust for device {DeviceId} by {Days} days", device.Id, extensionDays);
        return true;
    }

    private static TrustedDeviceDto MapToDto(TrustedDevice device) => new()
    {
        Id = device.Id,
        DeviceFingerprint = device.DeviceId,
        DeviceName = device.DeviceName,
        TrustedFromIp = device.IpAddress,
        TrustedUntil = device.ExpiresAt,
        CreatedAt = device.CreatedAt,
        LastUsedAt = device.LastUsedAt,
    };

    private static string DeriveDeviceName(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown Device";

        // Simple device name derivation from user agent
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows Device";
        if (userAgent.Contains("Mac", StringComparison.OrdinalIgnoreCase)) return "Mac Device";
        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)) return "iPhone";
        if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iPad";
        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android Device";
        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux Device";

        return "Unknown Device";
    }
}
