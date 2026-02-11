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

namespace CRM.Infrastructure.Providers.PowerBI;

/// <summary>
/// Configuration options for Power BI provider.
/// Supports both service principal and master user authentication.
/// </summary>
public class PowerBIConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Providers:Analytics:PowerBI";

    /// <summary>
    /// Azure AD tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD client/application ID for service principal authentication.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD client secret for service principal authentication.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Power BI workspace ID (also called Group ID).
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Authentication method: ServicePrincipal or MasterUser.
    /// </summary>
    public PowerBIAuthMethod AuthMethod { get; set; } = PowerBIAuthMethod.ServicePrincipal;

    /// <summary>
    /// Master user credentials (only for MasterUser auth method).
    /// </summary>
    public MasterUserCredentials? MasterUser { get; set; }

    /// <summary>
    /// Power BI API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.powerbi.com/v1.0/myorg";

    /// <summary>
    /// Azure AD authority URL.
    /// </summary>
    public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>
    /// Power BI resource URL for token acquisition.
    /// </summary>
    public string ResourceUrl { get; set; } = "https://analysis.windows.net/powerbi/api";

    /// <summary>
    /// Scopes for service principal authentication.
    /// </summary>
    public string[] Scopes => new[] { $"{ResourceUrl}/.default" };

    /// <summary>
    /// Token cache duration in minutes.
    /// </summary>
    public int TokenCacheMinutes { get; set; } = 55;

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable row-level security (RLS) filtering.
    /// </summary>
    public bool EnableRls { get; set; } = true;

    /// <summary>
    /// Default RLS role name for embedding.
    /// </summary>
    public string? DefaultRlsRole { get; set; }

    /// <summary>
    /// RLS identity claim mapping (CRM user property → RLS filter).
    /// </summary>
    public Dictionary<string, string> RlsIdentityMappings { get; set; } = new();

    /// <summary>
    /// Dashboard ID to CRM dashboard name mappings.
    /// </summary>
    public Dictionary<string, string> DashboardMappings { get; set; } = new();

    /// <summary>
    /// Report ID to CRM report name mappings.
    /// </summary>
    public Dictionary<string, string> ReportMappings { get; set; } = new();

    /// <summary>
    /// Dataset ID to CRM data source name mappings.
    /// </summary>
    public Dictionary<string, string> DatasetMappings { get; set; } = new();

    /// <summary>
    /// Embed configuration settings.
    /// </summary>
    public EmbedSettings? EmbedConfig { get; set; }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(TenantId))
            return (false, "TenantId is required");
        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "ClientId is required");
        if (string.IsNullOrWhiteSpace(WorkspaceId))
            return (false, "WorkspaceId is required");

        if (AuthMethod == PowerBIAuthMethod.ServicePrincipal)
        {
            if (string.IsNullOrWhiteSpace(ClientSecret))
                return (false, "ClientSecret is required for service principal authentication");
        }
        else if (AuthMethod == PowerBIAuthMethod.MasterUser)
        {
            if (MasterUser == null)
                return (false, "MasterUser credentials are required for master user authentication");
            if (string.IsNullOrWhiteSpace(MasterUser.Username))
                return (false, "MasterUser.Username is required");
            if (string.IsNullOrWhiteSpace(MasterUser.Password))
                return (false, "MasterUser.Password is required");
        }

        return (true, null);
    }
}

/// <summary>
/// Authentication method for Power BI.
/// </summary>
public enum PowerBIAuthMethod
{
    /// <summary>
    /// Service principal authentication (recommended for production).
    /// </summary>
    ServicePrincipal,

    /// <summary>
    /// Master user authentication (for development/testing).
    /// </summary>
    MasterUser
}

/// <summary>
/// Master user credentials for Power BI authentication.
/// </summary>
public class MasterUserCredentials
{
    /// <summary>
    /// Power BI Pro user email.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Embed settings for Power BI.
/// </summary>
public class EmbedSettings
{
    /// <summary>
    /// Allow edit mode in embedded reports.
    /// </summary>
    public bool AllowEdit { get; set; } = false;

    /// <summary>
    /// Show page navigation pane.
    /// </summary>
    public bool ShowNavContent { get; set; } = true;

    /// <summary>
    /// Show filter pane.
    /// </summary>
    public bool ShowFilterPane { get; set; } = true;

    /// <summary>
    /// Default filter settings.
    /// </summary>
    public Dictionary<string, string>? DefaultFilters { get; set; }
}

/// <summary>
/// Power BI embed configuration for frontend.
/// </summary>
public class PowerBIEmbedConfig
{
    /// <summary>
    /// Embed token for authentication.
    /// </summary>
    public string EmbedToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time.
    /// </summary>
    public DateTime TokenExpiry { get; set; }

    /// <summary>
    /// Embed URL for the report/dashboard.
    /// </summary>
    public string EmbedUrl { get; set; } = string.Empty;

    /// <summary>
    /// Report or dashboard ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of embed (Report, Dashboard, Tile, etc.).
    /// </summary>
    public string Type { get; set; } = "report";
}
