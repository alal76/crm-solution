namespace CRM.Core.Dtos;

/// <summary>
/// DTO for 2FA login verification
/// </summary>
public class TwoFactorLoginRequest
{
    /// <summary>
    /// Temporary token received from initial login response
    /// </summary>
    public string TwoFactorToken { get; set; } = string.Empty;
    
    /// <summary>
    /// TOTP code from authenticator app or backup code
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
