namespace CRM.Core.Options;

/// <summary>
/// Configuration options for SMS OTP service using Twilio.
/// </summary>
public class SmsOtpSettings
{
    /// <summary>
    /// Gets or sets the Twilio Account SID.
    /// Available at https://www.twilio.com/console
    /// </summary>
    public string TwilioAccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twilio Auth Token.
    /// Available at https://www.twilio.com/console
    /// </summary>
    public string TwilioAuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twilio phone number to send SMS from.
    /// Must be in E.164 format: +15551234567
    /// </summary>
    public string FromPhoneNumber { get; set; } = "+1234567890";

    /// <summary>
    /// Gets or sets the OTP expiration time in seconds.
    /// Default: 300 (5 minutes)
    /// </summary>
    public int OtpExpirationSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the OTP code length (number of digits).
    /// Default: 6
    /// </summary>
    public int OtpLength { get; set; } = 6;

    /// <summary>
    /// Gets or sets the maximum number of verification attempts allowed.
    /// Default: 5
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of OTPs that can be sent per hour per recipient.
    /// Default: 3
    /// Helps prevent SMS flooding abuse.
    /// </summary>
    public int MaxOtpsPerHour { get; set; } = 3;

    /// <summary>
    /// Gets or sets the storage type for OTP data.
    /// Options: "Memory" (in-process), "Redis", "Database"
    /// Default: "Memory" (TODO: Switch to Redis/Database for production)
    /// </summary>
    public string StorageType { get; set; } = "Memory";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(TwilioAccountSid))
            return (false, "SmsOtpSettings.TwilioAccountSid is required");

        if (string.IsNullOrWhiteSpace(TwilioAuthToken))
            return (false, "SmsOtpSettings.TwilioAuthToken is required");

        if (string.IsNullOrWhiteSpace(FromPhoneNumber))
            return (false, "SmsOtpSettings.FromPhoneNumber is required");

        if (OtpLength < 4 || OtpLength > 8)
            return (false, "SmsOtpSettings.OtpLength must be between 4 and 8");

        if (MaxAttempts < 1 || MaxAttempts > 10)
            return (false, "SmsOtpSettings.MaxAttempts must be between 1 and 10");

        if (MaxOtpsPerHour < 1 || MaxOtpsPerHour > 10)
            return (false, "SmsOtpSettings.MaxOtpsPerHour must be between 1 and 10");

        if (OtpExpirationSeconds < 60 || OtpExpirationSeconds > 3600)
            return (false, "SmsOtpSettings.OtpExpirationSeconds must be between 60 and 3600");

        return (true, string.Empty);
    }
}
