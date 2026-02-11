// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Infrastructure.Providers.DocuSign;

/// <summary>
/// Configuration options for DocuSign e-signature provider.
/// Supports both OAuth JWT and individual user consent flows.
/// </summary>
public class DocuSignConfiguration
{
    /// <summary>
    /// Gets or sets the DocuSign Integration Key (Client ID).
    /// </summary>
    public string IntegrationKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DocuSign User ID (GUID) for JWT auth.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DocuSign Account ID.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the RSA private key for JWT authentication.
    /// Can be a file path or PEM-formatted key content.
    /// </summary>
    public string RsaPrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth base path (e.g., https://account-d.docusign.com for demo).
    /// </summary>
    public string OAuthBasePath { get; set; } = "https://account-d.docusign.com";

    /// <summary>
    /// Gets or sets the API base path (e.g., https://demo.docusign.net/restapi for demo).
    /// </summary>
    public string ApiBasePath { get; set; } = "https://demo.docusign.net/restapi";

    /// <summary>
    /// Gets or sets the environment (demo or production).
    /// </summary>
    public string Environment { get; set; } = "demo";

    /// <summary>
    /// Gets or sets the webhook secret for Connect webhook validation.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Connect webhook URL (optional, for outbound webhooks).
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default sender email address.
    /// </summary>
    public string DefaultSenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default sender name.
    /// </summary>
    public string DefaultSenderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default expiration days for envelopes.
    /// </summary>
    public int DefaultExpirationDays { get; set; } = 14;

    /// <summary>
    /// Gets or sets the default reminder days.
    /// </summary>
    public int DefaultReminderDays { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to use embedded signing.
    /// </summary>
    public bool EnableEmbeddedSigning { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout in seconds for API calls.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the JWT token expiration in hours (max 1 hour per DocuSign).
    /// </summary>
    public int JwtExpirationHours { get; set; } = 1;

    /// <summary>
    /// Gets or sets the OAuth scopes required (space-separated).
    /// </summary>
    public string OAuthScopes { get; set; } = "signature impersonation";

    /// <summary>
    /// Gets the API base URL for the configured environment.
    /// </summary>
    public string GetApiBaseUrl()
    {
        if (Environment.Equals("production", StringComparison.OrdinalIgnoreCase))
        {
            return "https://na4.docusign.net/restapi";
        }
        return "https://demo.docusign.net/restapi";
    }

    /// <summary>
    /// Gets the OAuth base URL for the configured environment.
    /// </summary>
    public string GetOAuthBaseUrl()
    {
        if (Environment.Equals("production", StringComparison.OrdinalIgnoreCase))
        {
            return "https://account.docusign.com";
        }
        return "https://account-d.docusign.com";
    }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>A tuple indicating validity and any error message.</returns>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(IntegrationKey))
        {
            return (false, "DocuSign Integration Key is required");
        }

        if (string.IsNullOrWhiteSpace(UserId))
        {
            return (false, "DocuSign User ID is required");
        }

        if (string.IsNullOrWhiteSpace(AccountId))
        {
            return (false, "DocuSign Account ID is required");
        }

        if (string.IsNullOrWhiteSpace(RsaPrivateKey))
        {
            return (false, "DocuSign RSA Private Key is required for JWT authentication");
        }

        if (!Environment.Equals("demo", StringComparison.OrdinalIgnoreCase) &&
            !Environment.Equals("production", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "DocuSign Environment must be 'demo' or 'production'");
        }

        if (DefaultExpirationDays < 1 || DefaultExpirationDays > 999)
        {
            return (false, "DefaultExpirationDays must be between 1 and 999");
        }

        if (TimeoutSeconds < 1 || TimeoutSeconds > 300)
        {
            return (false, "TimeoutSeconds must be between 1 and 300");
        }

        return (true, null);
    }

    /// <summary>
    /// Gets the RSA private key bytes from the configuration.
    /// Handles both file paths and embedded PEM content.
    /// </summary>
    public byte[] GetRsaPrivateKeyBytes()
    {
        // Check if it's a file path
        if (File.Exists(RsaPrivateKey))
        {
            return File.ReadAllBytes(RsaPrivateKey);
        }

        // Otherwise, treat as PEM content
        return System.Text.Encoding.UTF8.GetBytes(RsaPrivateKey);
    }
}
