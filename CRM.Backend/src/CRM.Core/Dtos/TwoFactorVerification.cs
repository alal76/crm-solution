namespace CRM.Core.Dtos;

/// <summary>
/// DTO for 2FA verification
/// </summary>
public class TwoFactorVerification
{
    public string Code { get; set; } = string.Empty;
    public bool RememberDevice { get; set; } = false;
}
