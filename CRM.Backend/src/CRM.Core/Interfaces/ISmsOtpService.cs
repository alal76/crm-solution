// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service for SMS-based One-Time Password delivery and verification.
/// Uses Twilio for SMS delivery.
/// </summary>
public interface ISmsOtpService
{
    /// <summary>
    /// Send an OTP code via SMS to the specified phone number.
    /// </summary>
    Task<SmsOtpResult> SendOtpAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify an OTP code received via SMS.
    /// </summary>
    Task<bool> VerifyOtpAsync(
        string phoneNumber,
        string code,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an OTP is still valid (not expired).
    /// </summary>
    Task<bool> IsOtpValidAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining attempts before lockout.
    /// </summary>
    Task<int> GetRemainingAttemptsAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend OTP to phone number (respects rate limiting).
    /// </summary>
    Task<SmsOtpResult> ResendOtpAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of SMS OTP send operation.
/// </summary>
public class SmsOtpResult
{
    /// <summary>Gets a value indicating whether the send was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Gets the Twilio SID for tracking.</summary>
    public string? MessageSid { get; set; }

    /// <summary>Gets the error message if send failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets the expiration time of the OTP.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets the number of remaining attempts before lockout.</summary>
    public int RemainingAttempts { get; set; }
}
