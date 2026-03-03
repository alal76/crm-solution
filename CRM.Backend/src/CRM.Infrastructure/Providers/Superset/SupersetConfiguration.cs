// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.Superset;

using Microsoft.Extensions.Options;

/// <summary>
/// Configuration options for Apache Superset analytics provider.
/// </summary>
public class SupersetConfiguration
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "Providers:Analytics:Superset";

    /// <summary>
    /// Superset server base URL.
    /// Example: "https://superset.company.com" or "http://localhost:8088"
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Username for Superset API authentication.
    /// Should be a service account with API access.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for Superset API authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Superset provider (authentication type).
    /// Options: "db" (database), "ldap", "oauth"
    /// Default: "db"
    /// </summary>
    public string Provider { get; set; } = "db";

    /// <summary>
    /// Whether to refresh the access token automatically.
    /// </summary>
    public bool AutoRefreshToken { get; set; } = true;

    /// <summary>
    /// Access token refresh interval in minutes.
    /// Default: 50 minutes (tokens typically expire at 60 minutes)
    /// </summary>
    public int TokenRefreshIntervalMinutes { get; set; } = 50;

    /// <summary>
    /// Guest token settings for embedded dashboards.
    /// </summary>
    public GuestTokenSettings GuestToken { get; set; } = new();

    /// <summary>
    /// Default row-level security filters to apply to all embedded dashboards.
    /// Key: Native filter name, Value: Expression template (use {userId} placeholder)
    /// </summary>
    public Dictionary<string, string> DefaultRlsFilters { get; set; } = new();

    /// <summary>
    /// Dashboard ID mappings from CRM dashboard names to Superset dashboard IDs.
    /// </summary>
    public Dictionary<string, int> DashboardMappings { get; set; } = new();

    /// <summary>
    /// Chart ID mappings from CRM chart names to Superset chart IDs.
    /// </summary>
    public Dictionary<string, int> ChartMappings { get; set; } = new();

    /// <summary>
    /// Timeout for API requests in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to skip SSL certificate validation (for development only).
    /// </summary>
    public bool SkipSslValidation { get; set; } = false;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>Tuple of (isValid, errorMessage)</returns>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            return (false, "Superset BaseUrl is required");
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            return (false, "Superset Username is required");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            return (false, "Superset Password is required");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var parsedUri) ||
            (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
        {
            return (false, $"Superset BaseUrl '{BaseUrl}' is not a valid URL");
        }

        return (true, null);
    }
}

/// <summary>
/// Guest token configuration for Superset embedded dashboards.
/// </summary>
public class GuestTokenSettings
{
    /// <summary>
    /// Default expiration time for guest tokens in minutes.
    /// Default: 300 (5 hours)
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 300;

    /// <summary>
    /// Allowed domains for CORS when embedding.
    /// </summary>
    public List<string> AllowedDomains { get; set; } = new();

    /// <summary>
    /// Resource types allowed in guest tokens.
    /// Default: ["dashboard"]
    /// </summary>
    public List<string> AllowedResourceTypes { get; set; } = new() { "dashboard" };

    /// <summary>
    /// Whether to allow guest tokens to access all datasets.
    /// If false, datasets must be explicitly specified.
    /// </summary>
    public bool AllowAllDatasets { get; set; } = false;
}

/// <summary>
/// Validates SupersetConfiguration on startup.
/// </summary>
public class SupersetConfigurationValidator : IValidateOptions<SupersetConfiguration>
{
    public ValidateOptionsResult Validate(string? name, SupersetConfiguration options)
    {
        var (isValid, error) = options.Validate();

        if (!isValid)
        {
            return ValidateOptionsResult.Fail(error!);
        }

        return ValidateOptionsResult.Success;
    }
}
