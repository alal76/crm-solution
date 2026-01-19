namespace CRM.Core.Dtos;

/// <summary>
/// DTO for 2FA setup response
/// </summary>
public class TwoFactorSetupResponse
{
    public string QrCodeUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public List<string> BackupCodes { get; set; } = new();
}
