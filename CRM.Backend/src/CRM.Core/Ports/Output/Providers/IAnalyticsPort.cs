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

namespace CRM.Core.Ports.Output.Providers;

#region Analytics Port Interface

/// <summary>
/// Output port for analytics and business intelligence operations.
/// Enables embedded dashboards and reporting from external BI platforms.
/// Implementations: BuiltIn (basic), Superset, Metabase, Power BI, Looker, QuickSight.
/// </summary>
public interface IAnalyticsPort
{
    /// <summary>
    /// Gets the unique identifier for this analytics provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the analytics provider is properly configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether this provider supports dashboard embedding.
    /// </summary>
    bool SupportsEmbedding { get; }

    #region Dashboard Operations

    /// <summary>
    /// Gets a list of available dashboards.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available dashboards.</returns>
    Task<IEnumerable<DashboardInfo>> GetDashboardsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific dashboard by ID.
    /// </summary>
    /// <param name="dashboardId">The dashboard ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dashboard information if found.</returns>
    Task<DashboardInfo?> GetDashboardAsync(string dashboardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dashboards accessible by a specific user/role.
    /// </summary>
    /// <param name="userId">CRM user ID.</param>
    /// <param name="roles">User roles for permission filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered list of dashboards.</returns>
    Task<IEnumerable<DashboardInfo>> GetDashboardsForUserAsync(int userId, IEnumerable<string>? roles = null, CancellationToken cancellationToken = default);

    #endregion

    #region Embedding Operations

    /// <summary>
    /// Generates an embed URL or token for a dashboard.
    /// </summary>
    /// <param name="request">Embed request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embed configuration for frontend.</returns>
    Task<EmbedResult> GetEmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a guest/anonymous embed token.
    /// </summary>
    /// <param name="dashboardId">Dashboard to embed.</param>
    /// <param name="filters">Row-level security filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Guest embed token.</returns>
    Task<string> GetGuestTokenAsync(string dashboardId, Dictionary<string, string>? filters = null, CancellationToken cancellationToken = default);

    #endregion

    #region Chart/Widget Operations

    /// <summary>
    /// Gets available charts/widgets.
    /// </summary>
    /// <param name="dashboardId">Optional dashboard filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of charts.</returns>
    Task<IEnumerable<ChartInfo>> GetChartsAsync(string? dashboardId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets embed configuration for a specific chart.
    /// </summary>
    /// <param name="chartId">The chart ID.</param>
    /// <param name="filters">Data filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chart embed configuration.</returns>
    Task<EmbedResult> GetChartEmbedAsync(string chartId, Dictionary<string, string>? filters = null, CancellationToken cancellationToken = default);

    #endregion

    #region Query Operations (for BuiltIn provider)

    /// <summary>
    /// Executes a predefined report query.
    /// </summary>
    /// <param name="reportId">The report identifier.</param>
    /// <param name="parameters">Report parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Report data.</returns>
    Task<ReportResult> ExecuteReportAsync(string reportId, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available predefined reports.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available reports.</returns>
    Task<IEnumerable<ReportInfo>> GetReportsAsync(string? category = null, CancellationToken cancellationToken = default);

    #endregion

    #region Data Source Operations

    /// <summary>
    /// Gets configured data sources in the analytics platform.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of data sources.</returns>
    Task<IEnumerable<DataSourceInfo>> GetDataSourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers a data refresh for a data source.
    /// </summary>
    /// <param name="dataSourceId">The data source ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Refresh job status.</returns>
    Task<RefreshJobStatus> RefreshDataSourceAsync(string dataSourceId, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Gets the health status of the analytics provider.
    /// </summary>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Analytics DTOs

/// <summary>
/// Dashboard information.
/// </summary>
public class DashboardInfo
{
    /// <summary>
    /// Provider-assigned dashboard ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Dashboard display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dashboard description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Dashboard category/folder.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Thumbnail image URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Direct URL to dashboard (for linking).
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Whether embedding is supported.
    /// </summary>
    public bool CanEmbed { get; set; }

    /// <summary>
    /// Dashboard owner.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last modified timestamp.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Tags/labels.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Chart count in dashboard.
    /// </summary>
    public int? ChartCount { get; set; }
}

/// <summary>
/// Chart/widget information.
/// </summary>
public class ChartInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ChartType { get; set; } // bar, line, pie, table, etc.
    public string? DashboardId { get; set; }
    public bool CanEmbed { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

/// <summary>
/// Embed request.
/// </summary>
public class EmbedRequest
{
    /// <summary>
    /// Type of embed (dashboard, chart, report).
    /// </summary>
    public string EmbedType { get; set; } = "dashboard";

    /// <summary>
    /// Dashboard or chart ID.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// CRM user ID for permissions.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// User email for SSO.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// User roles.
    /// </summary>
    public List<string>? Roles { get; set; }

    /// <summary>
    /// Row-level security filters.
    /// </summary>
    public Dictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// Hide dashboard header/navigation.
    /// </summary>
    public bool HideHeader { get; set; } = true;

    /// <summary>
    /// Hide filter controls.
    /// </summary>
    public bool HideFilters { get; set; }

    /// <summary>
    /// Token expiry in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;
}

/// <summary>
/// Embed result for frontend rendering.
/// </summary>
public class EmbedResult
{
    /// <summary>
    /// Embed type (iframe, sdk, token).
    /// </summary>
    public string EmbedType { get; set; } = "iframe";

    /// <summary>
    /// Embed URL (for iframe embedding).
    /// </summary>
    public string? EmbedUrl { get; set; }

    /// <summary>
    /// Embed token (for SDK embedding).
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Token expiry time.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Additional configuration for SDK.
    /// </summary>
    public Dictionary<string, object>? Config { get; set; }

    /// <summary>
    /// Suggested iframe height.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Suggested iframe width.
    /// </summary>
    public int? Width { get; set; }
}

/// <summary>
/// Report information.
/// </summary>
public class ReportInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<ReportParameter>? Parameters { get; set; }
    public List<string>? OutputFormats { get; set; } // json, csv, pdf, excel
}

/// <summary>
/// Report parameter definition.
/// </summary>
public class ReportParameter
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, date, select
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    public List<object>? Options { get; set; }
}

/// <summary>
/// Report execution result.
/// </summary>
public class ReportResult
{
    public string ReportId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime ExecutedAt { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RowCount { get; set; }
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Data source information.
/// </summary>
public class DataSourceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; } // mysql, postgresql, etc.
    public DateTime? LastRefreshed { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Data refresh job status.
/// </summary>
public class RefreshJobStatus
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // running, completed, failed
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
}

#endregion
