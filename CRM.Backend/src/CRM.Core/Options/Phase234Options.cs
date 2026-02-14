namespace CRM.Core.Options;

/// <summary>WebAuthn/FIDO2 configuration options.</summary>
public class WebAuthnOptions
{
    public string RelyingPartyId { get; set; } = "localhost";
    public string RelyingPartyName { get; set; } = "CRM Solution";
    public int TimeoutSeconds { get; set; } = 60;
    public string AttestationConveyance { get; set; } = "direct";
    public string UserVerificationPreference { get; set; } = "preferred";
    public int ChallengeExpirationMinutes { get; set; } = 10;

    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(RelyingPartyId))
            return (false, "RelyingPartyId is required");
        if (TimeoutSeconds < 30 || TimeoutSeconds > 300)
            return (false, "TimeoutSeconds must be between 30 and 300");
        return (true, string.Empty);
    }
}

/// <summary>Google OAuth configuration options.</summary>
public class GoogleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "https://localhost:5001/auth/callback/google";
    public int TimeoutSeconds { get; set; } = 30;

    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "GoogleOAuthOptions.ClientId is required");
        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "GoogleOAuthOptions.ClientSecret is required");
        if (string.IsNullOrWhiteSpace(RedirectUri))
            return (false, "GoogleOAuthOptions.RedirectUri is required");
        return (true, string.Empty);
    }
}

/// <summary>Microsoft OAuth / Azure AD configuration options.</summary>
public class MicrosoftOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "https://localhost:5001/auth/callback/microsoft";
    public string? Tenant { get; set; } = "common";
    public int TimeoutSeconds { get; set; } = 30;

    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "MicrosoftOAuthOptions.ClientId is required");
        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "MicrosoftOAuthOptions.ClientSecret is required");
        if (string.IsNullOrWhiteSpace(RedirectUri))
            return (false, "MicrosoftOAuthOptions.RedirectUri is required");
        return (true, string.Empty);
    }
}

/// <summary>GitHub OAuth configuration options.</summary>
public class GitHubOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "https://localhost:5001/auth/callback/github";
    public int TimeoutSeconds { get; set; } = 30;

    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "GitHubOAuthOptions.ClientId is required");
        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "GitHubOAuthOptions.ClientSecret is required");
        if (string.IsNullOrWhiteSpace(RedirectUri))
            return (false, "GitHubOAuthOptions.RedirectUri is required");
        return (true, string.Empty);
    }
}
