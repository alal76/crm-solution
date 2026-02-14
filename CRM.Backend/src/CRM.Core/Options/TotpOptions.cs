namespace CRM.Core.Options;

/// <summary>
/// Configuration options for TOTP (RFC 6238) setup and backup codes.
/// </summary>
public class TotpOptions
{
    /// <summary>
    /// Gets or sets the issuer name displayed in authenticator apps.
    /// </summary>
    public string IssuerName { get; set; } = "CRM Solution";

    /// <summary>
    /// Gets or sets the number of minutes before a setup secret expires.
    /// </summary>
    public int SetupExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of backup codes generated per user.
    /// </summary>
    public int BackupCodeCount { get; set; } = 10;
}
