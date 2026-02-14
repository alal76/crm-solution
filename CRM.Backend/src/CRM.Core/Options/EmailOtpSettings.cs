namespace CRM.Core.Options;

/// <summary>
/// Configuration options for Email OTP service using SendGrid.
/// </summary>
public class EmailOtpSettings
{
    /// <summary>
    /// Gets or sets the SendGrid API key.
    /// Available at https://app.sendgrid.com/settings/api_keys
    /// </summary>
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender email address.
    /// Must be verified in SendGrid sender identity settings.
    /// </summary>
    public string FromAddress { get; set; } = "noreply@crm.local";

    /// <summary>
    /// Gets or sets the sender display name.
    /// Default: "CRM System"
    /// </summary>
    public string FromName { get; set; } = "CRM System";

    /// <summary>
    /// Gets or sets the OTP expiration time in seconds.
    /// Default: 900 (15 minutes)
    /// Email OTP has longer expiration than SMS to allow for inbox delays.
    /// </summary>
    public int OtpExpirationSeconds { get; set; } = 900;

    /// <summary>
    /// Gets or sets the OTP code length (number of digits).
    /// Default: 8 (8-digit OTP for email is more secure than SMS 6-digit)
    /// </summary>
    public int OtpLength { get; set; } = 8;

    /// <summary>
    /// Gets or sets the maximum number of verification attempts allowed.
    /// Default: 5
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of OTP emails that can be sent per hour per recipient.
    /// Default: 5
    /// Higher than SMS (3) because email is less likely to be abused for SMS flooding.
    /// </summary>
    public int MaxEmailsPerHour { get; set; } = 5;

    /// <summary>
    /// Gets or sets the storage type for OTP data.
    /// Options: "Memory" (in-process), "Redis", "Database"
    /// Default: "Memory" (TODO: Switch to Redis/Database for production)
    /// </summary>
    public string StorageType { get; set; } = "Memory";

    /// <summary>
    /// Gets or sets the email template name for OTP delivery.
    /// Default: "OTPVerification"
    /// Must match SendGrid dynamic template name in appsettings.
    /// </summary>
    public string TemplateName { get; set; } = "OTPVerification";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(SendGridApiKey))
            return (false, "EmailOtpSettings.SendGridApiKey is required");

        if (string.IsNullOrWhiteSpace(FromAddress))
            return (false, "EmailOtpSettings.FromAddress is required");

        if (!FromAddress.Contains("@"))
            return (false, "EmailOtpSettings.FromAddress must be a valid email address");

        if (OtpLength < 4 || OtpLength > 12)
            return (false, "EmailOtpSettings.OtpLength must be between 4 and 12");

        if (MaxAttempts < 1 || MaxAttempts > 10)
            return (false, "EmailOtpSettings.MaxAttempts must be between 1 and 10");

        if (MaxEmailsPerHour < 1 || MaxEmailsPerHour > 20)
            return (false, "EmailOtpSettings.MaxEmailsPerHour must be between 1 and 20");

        if (OtpExpirationSeconds < 300 || OtpExpirationSeconds > 3600)
            return (false, "EmailOtpSettings.OtpExpirationSeconds must be between 300 and 3600");

        return (true, string.Empty);
    }
}
