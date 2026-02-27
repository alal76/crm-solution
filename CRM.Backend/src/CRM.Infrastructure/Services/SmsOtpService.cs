// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CRM.Infrastructure.Services;

/// <summary>
/// SMS OTP service implementation using Twilio.
/// Sends and verifies one-time passwords via SMS.
/// </summary>
public class SmsOtpService : ISmsOtpService
{
    private readonly SmsOtpSettings _settings;
    private readonly ILogger<SmsOtpService> _logger;

    public SmsOtpService(
        IOptions<SmsOtpSettings> settings,
        ILogger<SmsOtpService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize Twilio
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    /// <summary>
    /// Send OTP via SMS to the specified phone number.
    /// </summary>
    public async Task<SmsOtpResult> SendOtpAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate phone number
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new SmsOtpResult { Success = false, ErrorMessage = "Phone number is required" };

            // Normalize phone number
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (!IsValidPhoneNumber(normalizedPhone))
                return new SmsOtpResult { Success = false, ErrorMessage = "Invalid phone number format" };

            // Check rate limiting (max 3 SMS per hour per phone)
            if (IsRateLimited(normalizedPhone, RateLimitType.Sms))
                return new SmsOtpResult { Success = false, ErrorMessage = "Too many SMS requests. Please try again later." };

            // Generate 6-digit OTP
            var otp = GenerateOtp(6);
            var expiresAt = DateTime.UtcNow.AddSeconds(_settings.OtpExpirationSeconds);

            // Store OTP hash (never store plaintext)
            StoreOtpHash(normalizedPhone, userId, HashOtp(otp), expiresAt);

            // Send SMS via Twilio
            var message = await MessageResource.CreateAsync(
                body: $"Your CRM verification code is: {otp}. This code expires in {_settings.OtpExpirationSeconds / 60} minutes.",
                from: new Twilio.Types.PhoneNumber(_settings.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(normalizedPhone));

            _logger.LogInformation($"SMS OTP sent to {MaskPhoneNumber(normalizedPhone)} for user {userId}. SID: {message.Sid}");

            return new SmsOtpResult
            {
                Success = true,
                MessageSid = message.Sid,
                ExpiresAt = expiresAt,
                RemainingAttempts = _settings.MaxAttempts
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send SMS OTP to {MaskPhoneNumber(phoneNumber)}");
            return new SmsOtpResult
            {
                Success = false,
                ErrorMessage = "Failed to send OTP. Please try again later."
            };
        }
    }

    /// <summary>
    /// Verify the provided OTP code.
    /// </summary>
    public async Task<bool> VerifyOtpAsync(
        string phoneNumber,
        string code,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
                return false;

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            // Get stored OTP hash
            var (storedHash, expiresAt, attempts) = RetrieveOtpHash(normalizedPhone, userId);

            if (storedHash == null)
            {
                _logger.LogWarning($"No OTP found for phone {MaskPhoneNumber(normalizedPhone)} user {userId}");
                return false;
            }

            // Check expiration
            if (DateTime.UtcNow > expiresAt)
            {
                _logger.LogWarning($"OTP expired for phone {MaskPhoneNumber(normalizedPhone)} user {userId}");
                ClearOtpRecord(normalizedPhone, userId);
                return false;
            }

            // Check attempt limit
            if (attempts >= _settings.MaxAttempts)
            {
                _logger.LogWarning($"Max OTP attempts exceeded for phone {MaskPhoneNumber(normalizedPhone)} user {userId}");
                ClearOtpRecord(normalizedPhone, userId);
                return false;
            }

            // Verify code hash
            var providedHash = HashOtp(code);
            if (providedHash != storedHash)
            {
                _logger.LogWarning($"Invalid OTP attempt for phone {MaskPhoneNumber(normalizedPhone)} user {userId}");
                IncrementOtpAttempt(normalizedPhone, userId);
                return false;
            }

            // Success - clear the OTP record
            ClearOtpRecord(normalizedPhone, userId);
            _logger.LogInformation($"OTP verified successfully for phone {MaskPhoneNumber(normalizedPhone)} user {userId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying OTP for phone {MaskPhoneNumber(phoneNumber)}");
            return false;
        }
    }

    /// <summary>
    /// Check if OTP is still valid for the phone number.
    /// </summary>
    public async Task<bool> IsOtpValidAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            var (hash, expiresAt, attempts) = RetrieveOtpHash(normalizedPhone, userId);

            return hash != null && DateTime.UtcNow <= expiresAt && attempts < _settings.MaxAttempts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OTP validity");
            return false;
        }
    }

    /// <summary>
    /// Get remaining verification attempts for the phone number.
    /// </summary>
    public async Task<int> GetRemainingAttemptsAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return 0;

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            var (_, _, attempts) = RetrieveOtpHash(normalizedPhone, userId);

            return Math.Max(0, _settings.MaxAttempts - attempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining attempts");
            return 0;
        }
    }

    /// <summary>
    /// Resend OTP to phone number (with rate limiting).
    /// </summary>
    public async Task<SmsOtpResult> ResendOtpAsync(
        string phoneNumber,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            // Check cooldown period (5 seconds between resends)
            if (!CanResendOtp(normalizedPhone))
                return new SmsOtpResult { Success = false, ErrorMessage = "Please wait before requesting a new code." };

            // Clear old OTP record
            ClearOtpRecord(normalizedPhone, userId);

            // Send new OTP
            var result = await SendOtpAsync(phoneNumber, userId, cancellationToken);

            if (result.Success)
                UpdateResendTimestamp(normalizedPhone);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP");
            return new SmsOtpResult { Success = false, ErrorMessage = "Failed to resend OTP" };
        }
    }

    #region Private Helper Methods

    private static string GenerateOtp(int length = 6)
    {
        var otp = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            otp.Append(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10));
        }
        return otp.ToString();
    }

    private string HashOtp(string otp)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove common formatting characters
        var normalized = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[\s\-\.\(\)]", "");
        // Add + if not present and doesn't start with +1
        if (!normalized.StartsWith("+"))
            normalized = "+" + normalized;
        return normalized;
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        // Basic validation: phone number should be 10-15 digits (E.164 format)
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");
        return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        // Mask phone number for logging: +1234*****890
        if (phoneNumber.Length <= 6)
            return "****";
        return phoneNumber.Substring(0, 6) + "*****" + phoneNumber.Substring(phoneNumber.Length - 3);
    }

    private bool IsRateLimited(string phoneNumber, RateLimitType type)
    {
        // In production, implement with distributed cache (Redis)
        // This is a simplified in-memory implementation
        var key = $"{type}:{phoneNumber}";
        if (!_otp_rate_limits.TryGetValue(key, out var lastAttempt))
            return false;

        var cooldownSeconds = type == RateLimitType.Sms ? 60 : 5;
        return DateTime.UtcNow.Subtract(lastAttempt).TotalSeconds < cooldownSeconds;
    }

    private bool CanResendOtp(string phoneNumber)
    {
        // Check if enough time has passed since last resend
        var key = $"resend:{phoneNumber}";
        if (!_otp_rate_limits.TryGetValue(key, out var lastResend))
            return true;

        return DateTime.UtcNow.Subtract(lastResend).TotalSeconds >= 5; // 5 second cooldown
    }

    private void UpdateResendTimestamp(string phoneNumber)
    {
        var key = $"resend:{phoneNumber}";
        _otp_rate_limits[key] = DateTime.UtcNow;
    }

    private void StoreOtpHash(string phoneNumber, int userId, string hash, DateTime expiresAt)
    {
        var key = $"otp:{phoneNumber}:{userId}";
        _otp_records[key] = (hash, expiresAt, 0);
        _otp_rate_limits[$"sms:{phoneNumber}"] = DateTime.UtcNow;
    }

    private (string? hash, DateTime expiresAt, int attempts) RetrieveOtpHash(string phoneNumber, int userId)
    {
        var key = $"otp:{phoneNumber}:{userId}";
        if (_otp_records.TryGetValue(key, out var record))
            return record;
        return (null, DateTime.MinValue, 0);
    }

    private void IncrementOtpAttempt(string phoneNumber, int userId)
    {
        var key = $"otp:{phoneNumber}:{userId}";
        if (_otp_records.TryGetValue(key, out var record))
        {
            _otp_records[key] = (record.hash, record.expiresAt, record.attempts + 1);
        }
    }

    private void ClearOtpRecord(string phoneNumber, int userId)
    {
        var key = $"otp:{phoneNumber}:{userId}";
        _otp_records.Remove(key);
    }

    // In-memory storage (replace with Redis in production)
    private static readonly Dictionary<string, (string hash, DateTime expiresAt, int attempts)> _otp_records = new();
    private static readonly Dictionary<string, DateTime> _otp_rate_limits = new();

    private enum RateLimitType { Sms,
        Email }

    #endregion
}

/// <summary>
/// SMS OTP service configuration settings.
/// </summary>
public class SmsOtpSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
    public int OtpExpirationSeconds { get; set; } = 300; // 5 minutes
    public int MaxAttempts { get; set; } = 5;
    public int MaxOtpsPerHour { get; set; } = 3;

    public void Validate()
    {
        if (string.IsNullOrEmpty(AccountSid))
            throw new InvalidOperationException("Twilio AccountSid is required");
        if (string.IsNullOrEmpty(AuthToken))
            throw new InvalidOperationException("Twilio AuthToken is required");
        if (string.IsNullOrEmpty(FromPhoneNumber))
            throw new InvalidOperationException("Twilio FromPhoneNumber is required");
    }
}
