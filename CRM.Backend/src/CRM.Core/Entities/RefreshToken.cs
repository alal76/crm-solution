// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Entities;

/// <summary>
/// Stores refresh tokens in a dedicated table for multi-device support,
/// token rotation, and revocation tracking.
/// Replaces the single RefreshToken/RefreshTokenExpiry columns on User.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// The opaque refresh token string (base64-encoded random bytes).
    /// Unique and indexed for fast lookup.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the owning user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// When this refresh token expires (absolute expiration).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When this token was revoked (null if still active).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// If this token was rotated, stores the replacement token string
    /// to detect reuse of already-rotated tokens.
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Reason for revocation (e.g., "Rotated", "Logout", "AdminRevoke").
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// IP address of the client that requested this token.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent or device description of the client.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Whether this token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt != null;

    /// <summary>
    /// Whether this token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Whether this token is currently usable (not revoked and not expired).
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    // === Navigation Properties ===

    /// <summary>
    /// Navigation to the owning user.
    /// </summary>
    public virtual User? User { get; set; }
}
