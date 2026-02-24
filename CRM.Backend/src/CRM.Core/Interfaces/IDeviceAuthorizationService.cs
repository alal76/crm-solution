// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for OAuth Device Authorization Grant (RFC 8628) (TODO-AUTH-023).
/// Enables authentication on devices with limited input capabilities.
/// </summary>
public interface IDeviceAuthorizationService
{
    /// <summary>
    /// Initiates a device authorization request.
    /// Returns device code and user code for the device to display.
    /// </summary>
    /// <param name="clientId">Client application ID</param>
    /// <param name="scope">Requested scopes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<DeviceAuthorizationResponse> InitiateDeviceAuthorizationAsync(
        string clientId,
        string? scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls for token using device code (device side).
    /// Returns appropriate response based on authorization state.
    /// </summary>
    /// <param name="deviceCode">Device code from initiation</param>
    /// <param name="clientId">Client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<DeviceTokenResponse> PollForTokenAsync(
        string deviceCode,
        string clientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending authorization by user code (user side).
    /// </summary>
    /// <param name="userCode">User code entered by user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<DeviceAuthorizationCode?> GetPendingAuthorizationAsync(
        string userCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authorizes a device code (user side after login).
    /// </summary>
    /// <param name="userCode">User code</param>
    /// <param name="userId">Authorizing user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> AuthorizeDeviceAsync(
        string userCode,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Denies a device authorization request.
    /// </summary>
    /// <param name="userCode">User code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> DenyDeviceAsync(
        string userCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired device authorization codes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> CleanupExpiredCodesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Response from device authorization initiation
/// </summary>
public class DeviceAuthorizationResponse
{
    /// <summary>
    /// The device verification code (for polling)
    /// </summary>
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>
    /// The end-user verification code (to display to user)
    /// </summary>
    public string UserCode { get; set; } = string.Empty;

    /// <summary>
    /// The end-user verification URI
    /// </summary>
    public string VerificationUri { get; set; } = string.Empty;

    /// <summary>
    /// Verification URI with user code pre-filled (optional)
    /// </summary>
    public string? VerificationUriComplete { get; set; }

    /// <summary>
    /// Lifetime in seconds of device_code and user_code
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Minimum polling interval in seconds
    /// </summary>
    public int Interval { get; set; } = 5;
}

/// <summary>
/// Response from device token polling
/// </summary>
public class DeviceTokenResponse
{
    /// <summary>
    /// Whether the token request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error code if not successful
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Error description
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Access token if successful
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Token type (typically "Bearer")
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Token expiration in seconds
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Refresh token if issued
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Granted scope
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Standard error codes:
    /// - authorization_pending: User hasn't completed authorization yet
    /// - slow_down: Polling too frequently
    /// - access_denied: User denied the authorization
    /// - expired_token: Device code has expired
    /// </summary>
    public static class ErrorCodes
    {
        public const string AuthorizationPending = "authorization_pending";
        public const string SlowDown = "slow_down";
        public const string AccessDenied = "access_denied";
        public const string ExpiredToken = "expired_token";
    }
}
