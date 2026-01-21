namespace CRM.Core.Dtos;

/// <summary>
/// DTO for enabling 2FA with secret and backup codes
/// </summary>
public class TwoFactorEnableRequest
{
    /// <summary>
    /// TOTP secret key
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// Backup codes for recovery
    /// </summary>
    public List<string> BackupCodes { get; set; } = new();
}
