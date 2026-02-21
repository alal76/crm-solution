// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service for Email-based One-Time Password delivery and verification.
/// Uses SendGrid for email delivery.
/// </summary>
public interface IEmailOtpService
{
    /// <summary>
    /// Send an OTP code via email to the specified address.
    /// </summary>
    Task<EmailOtpResult> SendOtpAsync(
        string emailAddress,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify an OTP code received via email.
    /// </summary>
    Task<bool> VerifyOtpAsync(
        string emailAddress,
        string code,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an OTP is still valid (not expired).
    /// </summary>
    Task<bool> IsOtpValidAsync(
        string emailAddress,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining attempts before lockout.
    /// </summary>
    Task<int> GetRemainingAttemptsAsync(
        string emailAddress,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend OTP to email address (respects rate limiting).
    /// </summary>
    Task<EmailOtpResult> ResendOtpAsync(
        string emailAddress,
        int userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of Email OTP send operation.
/// </summary>
public class EmailOtpResult
{
    /// <summary>Gets a value indicating whether the send was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Gets the SendGrid message ID for tracking.</summary>
    public string? MessageId { get; set; }

    /// <summary>Gets the error message if send failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets the expiration time of the OTP.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets the number of remaining attempts before lockout.</summary>
    public int RemainingAttempts { get; set; }
}
