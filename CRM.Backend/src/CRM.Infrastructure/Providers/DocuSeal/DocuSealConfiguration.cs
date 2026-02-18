// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Providers.DocuSeal;

/// <summary>
/// Configuration settings for DocuSeal e-signature integration.
/// DocuSeal is an open-source alternative to DocuSign for document signing.
///
/// Configuration example in appsettings.json:
/// {
///   "Providers": {
///     "Signatures": {
///       "Type": "DocuSeal",
///       "DocuSeal": {
///         "Url": "https://docuseal.company.com",
///         "ApiKey": "your-api-key",
///         "WebhookSecret": "your-webhook-secret",
///         "DefaultExpirationDays": 30,
///         "EnableEmbedSigning": true
///       }
///     }
///   }
/// }
/// </summary>
public class DocuSealConfiguration
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Providers:Signatures:DocuSeal";

    /// <summary>
    /// Base URL of the DocuSeal instance.
    /// Example: https://docuseal.company.com or http://localhost:3001
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// API key for DocuSeal authentication.
    /// Found in DocuSeal admin settings.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret for validating webhook signatures.
    /// Used to verify webhook payloads originate from DocuSeal.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Default expiration days for signature requests.
    /// </summary>
    public int DefaultExpirationDays { get; set; } = 30;

    /// <summary>
    /// Enable embedded signing (iframe-based signing experience).
    /// When false, signers receive email links.
    /// </summary>
    public bool EnableEmbedSigning { get; set; } = true;

    /// <summary>
    /// Return URL after signing completion (for embedded signing).
    /// </summary>
    public string? SigningReturnUrl { get; set; }

    /// <summary>
    /// Webhook endpoint URL for DocuSeal to send events to.
    /// Usually: https://your-crm-api/api/webhooks/docuseal
    /// </summary>
    public string? WebhookEndpoint { get; set; }

    /// <summary>
    /// Timeout for HTTP requests to DocuSeal in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable request/response logging for debugging.
    /// Should be false in production.
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Maximum file size for documents in bytes.
    /// Default: 25MB
    /// </summary>
    public long MaxDocumentSizeBytes { get; set; } = 25 * 1024 * 1024;

    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            return (false, "DocuSeal URL is required");
        }

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
        {
            return (false, $"DocuSeal URL '{Url}' is not a valid URI");
        }

        if (uri.Scheme != "http" && uri.Scheme != "https")
        {
            return (false, "DocuSeal URL must use http or https scheme");
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            return (false, "DocuSeal API key is required");
        }

        if (DefaultExpirationDays < 1 || DefaultExpirationDays > 365)
        {
            return (false, "DefaultExpirationDays must be between 1 and 365");
        }

        if (TimeoutSeconds < 5 || TimeoutSeconds > 120)
        {
            return (false, "TimeoutSeconds must be between 5 and 120");
        }

        return (true, null);
    }

    /// <summary>
    /// Gets the base API URL without trailing slash.
    /// </summary>
    public string GetApiBaseUrl()
    {
        var url = Url.TrimEnd('/');
        return $"{url}/api";
    }
}
