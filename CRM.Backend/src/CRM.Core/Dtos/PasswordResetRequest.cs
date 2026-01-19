namespace CRM.Core.Dtos;

/// <summary>
/// DTO for password reset request
/// </summary>
public class PasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
}
