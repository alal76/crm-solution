// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Options;

/// <summary>
/// Configuration options for Apple OAuth 2.0 provider with JWT client assertion.
/// Implements RFC 7521 JWT Bearer Token Profiles.
/// </summary>
public class AppleOAuthOptions
{
    /// <summary>
    /// Gets or sets the Apple application Bundle ID or Services ID (Client ID).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Apple Developer Team ID.
    /// Located in https://developer.apple.com/account/#/membership
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Private Key ID from Apple Developer portal.
    /// Used to identify which key was used for JWT signing.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Private Key (PEM format) used for JWT signing.
    /// Downloaded from Apple Developer portal as *.p8 file.
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the Apple OAuth authorize endpoint URL.
    /// </summary>
    public const string AuthorizeUrl = "https://appleid.apple.com/auth/authorize";

    /// <summary>
    /// Gets the Apple OAuth token endpoint URL.
    /// </summary>
    public const string TokenUrl = "https://appleid.apple.com/auth/token";

    /// <summary>
    /// Gets the Apple JWK set URL for verifying ID tokens.
    /// </summary>
    public const string JwkSetUrl = "https://appleid.apple.com/auth/keys";

    /// <summary>
    /// Gets the required scope for Sign in with Apple.
    /// </summary>
    public const string DefaultScope = "name email";

    /// <summary>
    /// Gets the JWT algorithm required by Apple.
    /// </summary>
    public const string JwtAlgorithm = "ES256";

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool, string) Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "AppleOAuthOptions.ClientId is required");

        if (string.IsNullOrWhiteSpace(TeamId))
            return (false, "AppleOAuthOptions.TeamId is required");

        if (string.IsNullOrWhiteSpace(KeyId))
            return (false, "AppleOAuthOptions.KeyId is required");

        if (string.IsNullOrWhiteSpace(PrivateKey))
            return (false, "AppleOAuthOptions.PrivateKey is required");

        return (true, string.Empty);
    }
}
