namespace CRM.Core.Dtos;

/// <summary>
/// DTO for admin password reset request
/// </summary>
public class AdminPasswordResetRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
