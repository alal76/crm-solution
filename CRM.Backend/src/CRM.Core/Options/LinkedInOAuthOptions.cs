// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Options;

/// <summary>
/// Configuration options for LinkedIn OAuth 2.0 provider.
/// </summary>
public class LinkedInOAuthOptions
{
    /// <summary>
    /// Gets or sets the LinkedIn application Client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the LinkedIn application Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth scopes (space-separated).
    /// Default: "r_liteprofile r_emailaddress"
    /// </summary>
    public string Scopes { get; set; } = "r_liteprofile r_emailaddress";

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the LinkedIn OAuth authorize endpoint URL.
    /// </summary>
    public const string AuthorizeUrl = "https://www.linkedin.com/oauth/v2/authorization";

    /// <summary>
    /// Gets the LinkedIn OAuth token endpoint URL.
    /// </summary>
    public const string TokenUrl = "https://www.linkedin.com/oauth/v2/accessToken";

    /// <summary>
    /// Gets the LinkedIn API profile endpoint URL.
    /// </summary>
    public const string ProfileUrl = "https://api.linkedin.com/v2/me";

    /// <summary>
    /// Gets the LinkedIn API email endpoint URL.
    /// </summary>
    public const string EmailUrl = "https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "LinkedInOAuthOptions.ClientId is required");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "LinkedInOAuthOptions.ClientSecret is required");

        return (true, string.Empty);
    }
}
