namespace CRM.Core.Dtos;

/// <summary>
/// DTO for OAuth login request
/// </summary>
public class OAuthLoginRequest
{
    public string Provider { get; set; } = string.Empty; // google, github, microsoft
    public string Token { get; set; } = string.Empty; // ID token or access token from provider
}
