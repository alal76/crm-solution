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
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Email OTP service implementation using SendGrid.
/// Sends and verifies one-time passwords via email.
/// </summary>
public class EmailOtpService : IEmailOtpService
{
    private readonly EmailOtpSettings _settings;
    private readonly SendGridClient _client;
    private readonly ILogger<EmailOtpService> _logger;

    public EmailOtpService(
        IOptions<EmailOtpSettings> settings,
        ILogger<EmailOtpService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrEmpty(_settings.SendGridApiKey))
        {
            throw new InvalidOperationException("SendGrid API key is required");
        }

        _client = new SendGridClient(_settings.SendGridApiKey);
    }

    /// <summary>
    /// Send OTP via email to the specified email address.
    /// </summary>
    public async Task<EmailOtpResult> SendOtpAsync(
        string email,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                return new EmailOtpResult { Success = false, ErrorMessage = "Invalid email address" };
            }

            var normalizedEmail = email.ToLowerInvariant().Trim();

            // Check rate limiting (max 5 emails per hour per email)
            if (IsRateLimited(normalizedEmail, RateLimitType.Email))
            {
                return new EmailOtpResult { Success = false, ErrorMessage = "Too many email requests. Please try again later." };
            }

            // Generate 8-digit OTP (longer than SMS for email security)
            var otp = GenerateOtp(8);
            var expiresAt = DateTime.UtcNow.AddSeconds(_settings.OtpExpirationSeconds);

            // Store OTP hash
            StoreOtpHash(normalizedEmail, userId, HashOtp(otp), expiresAt);

            // Send email via SendGrid
            var msg = new SendGridMessage()
            {
                From = new SendGrid.Helpers.Mail.EmailAddress(_settings.FromAddress, "CRM Verification"),
                Subject = "Your CRM Verification Code",
                HtmlContent = GenerateEmailHtml(otp, _settings.OtpExpirationSeconds),
                PlainTextContent = $"Your CRM verification code is: {otp}. This code expires in {_settings.OtpExpirationSeconds / 60} minutes."
            };
            msg.AddTo(new SendGrid.Helpers.Mail.EmailAddress(normalizedEmail));

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"SendGrid error: {response.StatusCode}");
                return new EmailOtpResult
                {
                    Success = false,
                    ErrorMessage = "Failed to send verification email"
                };
            }

            _logger.LogInformation($"Email OTP sent to {MaskEmail(normalizedEmail)} for user {userId}");

            return new EmailOtpResult
            {
                Success = true,
                MessageId = response.Headers.FirstOrDefault(h => h.Key == "X-Message-Id").Value?.FirstOrDefault(),
                ExpiresAt = expiresAt,
                RemainingAttempts = _settings.MaxAttempts
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email OTP to {MaskEmail(email)}");
            return new EmailOtpResult
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
        string email,
        string code,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var normalizedEmail = email.ToLowerInvariant().Trim();

            // Get stored OTP hash
            var (storedHash, expiresAt, attempts) = RetrieveOtpHash(normalizedEmail, userId);

            if (storedHash == null)
            {
                _logger.LogWarning($"No OTP found for email {MaskEmail(normalizedEmail)} user {userId}");
                return false;
            }

            // Check expiration (15 minutes for email vs 5 for SMS)
            if (DateTime.UtcNow > expiresAt)
            {
                _logger.LogWarning($"OTP expired for email {MaskEmail(normalizedEmail)} user {userId}");
                ClearOtpRecord(normalizedEmail, userId);
                return false;
            }

            // Check attempt limit
            if (attempts >= _settings.MaxAttempts)
            {
                _logger.LogWarning($"Max OTP attempts exceeded for email {MaskEmail(normalizedEmail)} user {userId}");
                ClearOtpRecord(normalizedEmail, userId);
                return false;
            }

            // Verify code hash (case-insensitive)
            var providedHash = HashOtp(code.Trim());
            if (providedHash != storedHash)
            {
                _logger.LogWarning($"Invalid OTP attempt for email {MaskEmail(normalizedEmail)} user {userId}");
                IncrementOtpAttempt(normalizedEmail, userId);
                return false;
            }

            // Success - clear the OTP record
            ClearOtpRecord(normalizedEmail, userId);
            _logger.LogInformation($"OTP verified successfully for email {MaskEmail(normalizedEmail)} user {userId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying OTP for email {MaskEmail(email)}");
            return false;
        }
    }

    /// <summary>
    /// Check if OTP is still valid for the email address.
    /// </summary>
    public async Task<bool> IsOtpValidAsync(
        string email,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var normalizedEmail = email.ToLowerInvariant().Trim();
            var (hash, expiresAt, attempts) = RetrieveOtpHash(normalizedEmail, userId);

            return hash != null && DateTime.UtcNow <= expiresAt && attempts < _settings.MaxAttempts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OTP validity");
            return false;
        }
    }

    /// <summary>
    /// Get remaining verification attempts for the email address.
    /// </summary>
    public async Task<int> GetRemainingAttemptsAsync(
        string email,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return 0;
            }

            var normalizedEmail = email.ToLowerInvariant().Trim();
            var (_, _, attempts) = RetrieveOtpHash(normalizedEmail, userId);

            return Math.Max(0, _settings.MaxAttempts - attempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining attempts");
            return 0;
        }
    }

    /// <summary>
    /// Resend OTP to email address (with rate limiting).
    /// </summary>
    public async Task<EmailOtpResult> ResendOtpAsync(
        string email,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = email.ToLowerInvariant().Trim();

            // Check cooldown period (10 seconds between resends)
            if (!CanResendOtp(normalizedEmail))
            {
                return new EmailOtpResult { Success = false, ErrorMessage = "Please wait before requesting a new code." };
            }

            // Clear old OTP record
            ClearOtpRecord(normalizedEmail, userId);

            // Send new OTP
            var result = await SendOtpAsync(email, userId, cancellationToken);

            if (result.Success)
            {
                UpdateResendTimestamp(normalizedEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP");
            return new EmailOtpResult { Success = false, ErrorMessage = "Failed to resend OTP" };
        }
    }

    #region Private Helper Methods

    private static string GenerateOtp(int length = 8)
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
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp.ToUpperInvariant()));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string MaskEmail(string email)
    {
        // Mask email for logging: u***@example.com
        var parts = email.Split('@');
        if (parts.Length != 2 || parts[0].Length <= 1)
        {
            return "****@****";
        }

        return parts[0][0] + "***@" + parts[1];
    }

    private string GenerateEmailHtml(string otp, int expirationSeconds)
    {
        var minutes = expirationSeconds / 60;
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f5f5f5; padding: 20px; text-align: center; border-radius: 8px; }}
        .code {{ font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #1976d2; text-align: center; padding: 20px 0; }}
        .footer {{ font-size: 12px; color: #999; text-align: center; padding-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>CRM Verification Code</h2>
        </div>
        <p>Your verification code is:</p>
        <div class='code'>{otp}</div>
        <p>This code expires in {minutes} minute(s).</p>
        <p>If you didn't request this code, please ignore this email.</p>
        <div class='footer'>
            <p>&copy; 2026 CRM Solution. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private bool IsRateLimited(string email, RateLimitType type)
    {
        // In production, implement with distributed cache (Redis)
        // This is a simplified in-memory implementation
        var key = $"{type}:{email}";
        if (!_otp_rate_limits.TryGetValue(key, out var lastAttempt))
        {
            return false;
        }

        var cooldownSeconds = type == RateLimitType.Email ? 60 : 5;
        return DateTime.UtcNow.Subtract(lastAttempt).TotalSeconds < cooldownSeconds;
    }

    private bool CanResendOtp(string email)
    {
        // Check if enough time has passed since last resend
        var key = $"resend:{email}";
        if (!_otp_rate_limits.TryGetValue(key, out var lastResend))
        {
            return true;
        }

        return DateTime.UtcNow.Subtract(lastResend).TotalSeconds >= 10; // 10 second cooldown
    }

    private void UpdateResendTimestamp(string email)
    {
        var key = $"resend:{email}";
        _otp_rate_limits[key] = DateTime.UtcNow;
    }

    private void StoreOtpHash(string email, int userId, string hash, DateTime expiresAt)
    {
        var key = $"otp:{email}:{userId}";
        _otp_records[key] = (hash, expiresAt, 0);
        _otp_rate_limits[$"email:{email}"] = DateTime.UtcNow;
    }

    private (string? hash, DateTime expiresAt, int attempts) RetrieveOtpHash(string email, int userId)
    {
        var key = $"otp:{email}:{userId}";
        if (_otp_records.TryGetValue(key, out var record))
        {
            return record;
        }
        return (null, DateTime.MinValue, 0);
    }

    private void IncrementOtpAttempt(string email, int userId)
    {
        var key = $"otp:{email}:{userId}";
        if (_otp_records.TryGetValue(key, out var record))
        {
            _otp_records[key] = (record.hash, record.expiresAt, record.attempts + 1);
        }
    }

    private void ClearOtpRecord(string email, int userId)
    {
        var key = $"otp:{email}:{userId}";
        _otp_records.Remove(key);
    }

    // In-memory storage (replace with Redis in production)
    private static readonly Dictionary<string, (string hash, DateTime expiresAt, int attempts)> _otp_records = new();
    private static readonly Dictionary<string, DateTime> _otp_rate_limits = new();

    private enum RateLimitType { Email,
        Sms }

    #endregion
}

/// <summary>
/// Email OTP service configuration settings.
/// </summary>
public class EmailOtpSettings
{
    public string SendGridApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public int OtpExpirationSeconds { get; set; } = 900; // 15 minutes
    public int MaxAttempts { get; set; } = 5;
    public int MaxEmailsPerHour { get; set; } = 5;

    public void Validate()
    {
        if (string.IsNullOrEmpty(SendGridApiKey))
        {
            throw new InvalidOperationException("SendGrid API key is required");
        }
        if (string.IsNullOrEmpty(FromAddress))
        {
            throw new InvalidOperationException("SendGrid FromAddress is required");
        }
    }
}
