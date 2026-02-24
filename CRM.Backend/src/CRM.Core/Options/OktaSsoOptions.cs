// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Options;

/// <summary>
/// Configuration options for Okta Enterprise SSO integration.
/// TODO-AUTH-003: Okta Enterprise SSO Provider
/// </summary>
public class OktaSsoOptions
{
    /// <summary>
    /// Gets or sets the Okta domain (e.g., "dev-123456.okta.com").
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Okta application Client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Okta application Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth redirect URI for callback.
    /// </summary>
    public string RedirectUri { get; set; } = "https://localhost:5001/auth/callback/okta";

    /// <summary>
    /// Gets or sets the OAuth scopes (space-separated).
    /// Default: "openid profile email groups"
    /// </summary>
    public string Scopes { get; set; } = "openid profile email groups";

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to enable automatic user provisioning from Okta.
    /// </summary>
    public bool EnableAutoProvisioning { get; set; } = true;

    /// <summary>
    /// Gets or sets the default user group ID for auto-provisioned users.
    /// </summary>
    public int? DefaultUserGroupId { get; set; }

    /// <summary>
    /// Gets or sets the Okta API token for admin operations.
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>
    /// Gets the Okta OAuth authorize endpoint URL.
    /// </summary>
    public string AuthorizeUrl => $"https://{Domain}/oauth2/default/v1/authorize";

    /// <summary>
    /// Gets the Okta OAuth token endpoint URL.
    /// </summary>
    public string TokenUrl => $"https://{Domain}/oauth2/default/v1/token";

    /// <summary>
    /// Gets the Okta UserInfo endpoint URL.
    /// </summary>
    public string UserInfoUrl => $"https://{Domain}/oauth2/default/v1/userinfo";

    /// <summary>
    /// Gets the Okta logout endpoint URL.
    /// </summary>
    public string LogoutUrl => $"https://{Domain}/oauth2/default/v1/logout";

    /// <summary>
    /// Gets the Okta JWKS endpoint URL for token validation.
    /// </summary>
    public string JwksUrl => $"https://{Domain}/oauth2/default/v1/keys";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(Domain))
            return (false, "OktaSsoOptions.Domain is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "OktaSsoOptions.ClientId is required");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "OktaSsoOptions.ClientSecret is required");

        if (string.IsNullOrWhiteSpace(RedirectUri))
            return (false, "OktaSsoOptions.RedirectUri is required");

        return (true, string.Empty);
    }
}
