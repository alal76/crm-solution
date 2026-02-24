// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Options;

/// <summary>
/// Configuration options for generic OpenID Connect provider support.
/// TODO-AUTH-004: Generic OIDC Provider Support
/// </summary>
public class OpenIdConnectOptions
{
    /// <summary>
    /// Gets or sets a unique provider name/identifier.
    /// </summary>
    public string ProviderName { get; set; } = "oidc";

    /// <summary>
    /// Gets or sets the display name for the provider.
    /// </summary>
    public string DisplayName { get; set; } = "OpenID Connect";

    /// <summary>
    /// Gets or sets the OIDC discovery endpoint (e.g., "https://issuer/.well-known/openid-configuration").
    /// </summary>
    public string MetadataAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OIDC issuer URL.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application Client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth redirect URI for callback.
    /// </summary>
    public string RedirectUri { get; set; } = "https://localhost:5001/auth/callback/oidc";

    /// <summary>
    /// Gets or sets the OAuth scopes (space-separated).
    /// Default: "openid profile email"
    /// </summary>
    public string Scopes { get; set; } = "openid profile email";

    /// <summary>
    /// Gets or sets the response type for OIDC flow.
    /// Default: "code" (authorization code flow)
    /// </summary>
    public string ResponseType { get; set; } = "code";

    /// <summary>
    /// Gets or sets the response mode.
    /// Default: "form_post"
    /// </summary>
    public string ResponseMode { get; set; } = "form_post";

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to use PKCE (Proof Key for Code Exchange).
    /// </summary>
    public bool UsePkce { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the issuer claim.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the audience claim.
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the token lifetime.
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom claim mapping for email.
    /// </summary>
    public string EmailClaimType { get; set; } = "email";

    /// <summary>
    /// Gets or sets the custom claim mapping for name.
    /// </summary>
    public string NameClaimType { get; set; } = "name";

    /// <summary>
    /// Gets or sets the custom claim mapping for groups/roles.
    /// </summary>
    public string GroupsClaimType { get; set; } = "groups";

    /// <summary>
    /// Gets or sets whether to enable automatic user provisioning.
    /// </summary>
    public bool EnableAutoProvisioning { get; set; } = true;

    /// <summary>
    /// Gets or sets the default user group ID for auto-provisioned users.
    /// </summary>
    public int? DefaultUserGroupId { get; set; }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(Authority) && string.IsNullOrWhiteSpace(MetadataAddress))
            return (false, "OpenIdConnectOptions.Authority or MetadataAddress is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "OpenIdConnectOptions.ClientId is required");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "OpenIdConnectOptions.ClientSecret is required");

        return (true, string.Empty);
    }
}
