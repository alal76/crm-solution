namespace CRM.Core.Dtos;

/// <summary>
/// DTO for password reset confirmation
/// </summary>
public class PasswordResetConfirm
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
