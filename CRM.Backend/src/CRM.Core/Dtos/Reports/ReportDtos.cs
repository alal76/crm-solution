// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Dtos.Reports;

/// <summary>
/// DTO representing a report definition.
/// </summary>
public class ReportDefinitionDto
{
    /// <summary>Gets or sets the report ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the report name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the report description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the report category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the folder ID containing this report.</summary>
    public int? FolderId { get; set; }

    /// <summary>Gets or sets the ID of the user who created this report.</summary>
    public int CreatedById { get; set; }

    /// <summary>Gets or sets whether this is a standard (system) report.</summary>
    public bool IsStandard { get; set; }

    /// <summary>Gets or sets when the report was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new report definition.
/// </summary>
public class CreateReportDefinitionDto
{
    /// <summary>Gets or sets the report name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the report description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the report query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the report category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the folder ID to place this report in.</summary>
    public int? FolderId { get; set; }
}

/// <summary>
/// DTO for updating an existing report definition.
/// </summary>
public class UpdateReportDefinitionDto
{
    /// <summary>Gets or sets the report ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the report name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the report description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the report query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the report category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the folder ID to place this report in.</summary>
    public int? FolderId { get; set; }
}

/// <summary>
/// DTO representing the result of a report execution.
/// </summary>
public class ReportExecutionResultDto
{
    /// <summary>Gets or sets the report ID that was executed.</summary>
    public int ReportId { get; set; }

    /// <summary>Gets or sets the execution ID.</summary>
    public int ExecutionId { get; set; }

    /// <summary>Gets or sets the column names in the result set.</summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>Gets or sets the data rows as dictionaries of column name to value.</summary>
    public List<Dictionary<string, object>> Data { get; set; } = new();

    /// <summary>Gets or sets when the report was executed.</summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>Gets or sets the number of rows returned.</summary>
    public int RowCount { get; set; }

    /// <summary>Gets or sets the execution time in milliseconds.</summary>
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// DTO for report execution parameters.
/// </summary>
public class ReportParametersDto
{
    /// <summary>Gets or sets the start date filter.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Gets or sets the end date filter.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets additional filters as key-value pairs.</summary>
    public Dictionary<string, object> Filters { get; set; } = new();
}

/// <summary>
/// DTO representing a report schedule.
/// </summary>
public class ReportScheduleDto
{
    /// <summary>Gets or sets the schedule ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the report ID to schedule.</summary>
    public int ReportId { get; set; }

    /// <summary>Gets or sets the frequency (Daily, Weekly, Monthly).</summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>Gets or sets the time of day to run the report.</summary>
    public TimeOnly Time { get; set; }

    /// <summary>Gets or sets the list of recipient email addresses.</summary>
    public List<string> Recipients { get; set; } = new();

    /// <summary>Gets or sets the export format (PDF, Excel, CSV).</summary>
    public string ExportFormat { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the schedule is enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets when the next run is scheduled.</summary>
    public DateTime? NextRunAt { get; set; }

    /// <summary>Gets or sets when the last run occurred.</summary>
    public DateTime? LastRunAt { get; set; }
}

/// <summary>
/// DTO for creating a new report schedule.
/// </summary>
public class CreateReportScheduleDto
{
    /// <summary>Gets or sets the report ID to schedule.</summary>
    public int ReportId { get; set; }

    /// <summary>Gets or sets the frequency (Daily, Weekly, Monthly).</summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>Gets or sets the time of day to run the report.</summary>
    public TimeOnly Time { get; set; }

    /// <summary>Gets or sets the list of recipient email addresses.</summary>
    public List<string> Recipients { get; set; } = new();

    /// <summary>Gets or sets the export format (PDF, Excel, CSV).</summary>
    public string ExportFormat { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing report schedule.
/// </summary>
public class UpdateReportScheduleDto
{
    /// <summary>Gets or sets the schedule ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the frequency (Daily, Weekly, Monthly).</summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>Gets or sets the time of day to run the report.</summary>
    public TimeOnly Time { get; set; }

    /// <summary>Gets or sets the list of recipient email addresses.</summary>
    public List<string> Recipients { get; set; } = new();

    /// <summary>Gets or sets the export format (PDF, Excel, CSV).</summary>
    public string ExportFormat { get; set; } = string.Empty;
}

/// <summary>
/// DTO representing a report folder.
/// </summary>
public class ReportFolderDto
{
    /// <summary>Gets or sets the folder ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the folder name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the folder description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent folder ID for nested folders.</summary>
    public int? ParentId { get; set; }

    /// <summary>Gets or sets the number of reports in this folder.</summary>
    public int ReportCount { get; set; }
}

/// <summary>
/// DTO for creating a new report folder.
/// </summary>
public class CreateReportFolderDto
{
    /// <summary>Gets or sets the folder name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the folder description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent folder ID for nested folders.</summary>
    public int? ParentId { get; set; }
}

/// <summary>
/// DTO for updating an existing report folder.
/// </summary>
public class UpdateReportFolderDto
{
    /// <summary>Gets or sets the folder ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the folder name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the folder description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent folder ID for nested folders.</summary>
    public int? ParentId { get; set; }
}

/// <summary>
/// DTO representing report execution history.
/// </summary>
public class ReportExecutionHistoryDto
{
    /// <summary>Gets or sets the execution history ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the report ID that was executed.</summary>
    public int ReportId { get; set; }

    /// <summary>Gets or sets when the report was executed.</summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>Gets or sets the execution status (Success, Failed, Cancelled).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of rows returned.</summary>
    public int? RowCount { get; set; }

    /// <summary>Gets or sets the execution time in milliseconds.</summary>
    public long? ExecutionTimeMs { get; set; }

    /// <summary>Gets or sets who triggered the execution (user email or 'Scheduled').</summary>
    public string TriggeredBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for sharing a report with users and groups.
/// </summary>
public class ShareReportDto
{
    /// <summary>Gets or sets the list of user IDs to share with.</summary>
    public List<int> UserIds { get; set; } = new();

    /// <summary>Gets or sets the list of group IDs to share with.</summary>
    public List<int> GroupIds { get; set; } = new();

    /// <summary>Gets or sets the permission level (View, Edit, Admin). Defaults to "View".</summary>
    public string Permission { get; set; } = "View";
}

/// <summary>
/// DTO representing the sharing configuration of a report.
/// </summary>
public class ReportSharingDto
{
    /// <summary>Gets or sets the users the report is shared with.</summary>
    public List<UserDto> Users { get; set; } = new();

    /// <summary>Gets or sets the groups the report is shared with.</summary>
    public List<UserGroupDto> Groups { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Cohort Analysis (TODO-RPT-07)
// ---------------------------------------------------------------------------

/// <summary>Cohort grouping granularity.</summary>
public enum ReportCohortType
{
    /// <summary>Monthly cohorts.</summary>
    Monthly = 0,

    /// <summary>Quarterly cohorts.</summary>
    Quarterly = 1
}

/// <summary>Metric measured per cohort period.</summary>
public enum CohortMetricType
{
    /// <summary>Retention rate over time.</summary>
    Retention = 0,

    /// <summary>Revenue generated over time.</summary>
    Revenue = 1,

    /// <summary>Activity (interactions) over time.</summary>
    Activity = 2
}

/// <summary>Dimension used for customer segmentation.</summary>
public enum SegmentBy
{
    /// <summary>Segment by industry vertical.</summary>
    Industry = 0,

    /// <summary>Segment by geographic region.</summary>
    Region = 1,

    /// <summary>Segment by revenue band.</summary>
    Revenue = 2,

    /// <summary>Segment by lifecycle stage.</summary>
    Lifecycle = 3
}

/// <summary>
/// Request DTO for cohort analysis.
/// </summary>
public class CohortAnalysisRequestDto
{
    /// <summary>Gets or sets the cohort window start date.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the cohort window end date.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Gets or sets whether to group cohorts monthly or quarterly.</summary>
    public ReportCohortType CohortType { get; set; } = ReportCohortType.Monthly;

    /// <summary>Gets or sets which metric to measure per period.</summary>
    public CohortMetricType MetricType { get; set; } = CohortMetricType.Retention;
}

/// <summary>
/// A single cohort row in the analysis matrix.
/// </summary>
public class CohortRowDto
{
    /// <summary>Label for this cohort (e.g. "Jan 2025").</summary>
    public string CohortLabel { get; set; } = string.Empty;

    /// <summary>Number of customers in the cohort at period 0.</summary>
    public int InitialCount { get; set; }

    /// <summary>Per-period metric values (retention %, revenue, or activity count).</summary>
    public List<decimal> Values { get; set; } = new();
}

/// <summary>
/// Full cohort analysis result.
/// </summary>
public class CohortAnalysisDto
{
    /// <summary>Period labels (e.g. "Month +0", "Month +1", …).</summary>
    public List<string> Periods { get; set; } = new();

    /// <summary>One row per cohort.</summary>
    public List<CohortRowDto> Cohorts { get; set; } = new();
}

/// <summary>
/// Criteria for customer segmentation.
/// </summary>
public class SegmentationCriteria
{
    /// <summary>Dimension to segment customers by.</summary>
    public SegmentBy SegmentBy { get; set; } = SegmentBy.Industry;

    /// <summary>Optional start of the date range for activity filtering.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Optional end of the date range for activity filtering.</summary>
    public DateTime? EndDate { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// JSON schema versioning for report query payloads can be added for API versioning support
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Typed filter operator for structured report queries (V2 schema).
/// </summary>
public enum FilterOperator
{
    /// <summary>Exact equality.</summary>
    Equals = 0,
    /// <summary>Not equal.</summary>
    NotEquals = 1,
    /// <summary>Contains substring (case-insensitive).</summary>
    Contains = 2,
    /// <summary>Greater than.</summary>
    GreaterThan = 3,
    /// <summary>Less than.</summary>
    LessThan = 4,
    /// <summary>Between two values (inclusive).</summary>
    Between = 5,
    /// <summary>Value is in a list.</summary>
    In = 6,
    /// <summary>Field is null or not set.</summary>
    IsNull = 7,
}

/// <summary>
/// A single structured filter descriptor used in V2 report queries.
/// </summary>
public class ReportFilterDescriptor
{
    /// <summary>The field / column name to filter on.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>The comparison operator.</summary>
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;

    /// <summary>Primary value (or lower bound for Between).</summary>
    public object? Value { get; set; }

    /// <summary>Upper bound value — used only when Operator is Between.</summary>
    public object? ValueTo { get; set; }
}

/// <summary>
/// A sort descriptor for report queries.
/// </summary>
public class ReportSortDescriptor
{
    /// <summary>Field to sort by.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>True for ascending; false for descending.</summary>
    public bool Ascending { get; set; } = true;
}

/// <summary>
/// Versioned report query payload (TODO-AI005-FE-002).
/// Supports V1 (legacy flat filters) and V2 (structured filter array).
/// </summary>
public class ReportQueryDto
{
    /// <summary>
    /// Schema version of this payload.
    /// Consumers should call MigrateReportQueryAsync if this is below the
    /// current supported version.
    /// </summary>
    public CRM.Core.Enums.ReportQuerySchemaVersion SchemaVersion { get; set; }
        = CRM.Core.Enums.ReportQuerySchemaVersion.V2;

    // ── V1 legacy fields (flat) ──────────────────────────────────────────────

    /// <summary>[V1] Optional start date filter.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>[V1] Optional end date filter.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>[V1] Free-form filter map (field → value strings).</summary>
    public Dictionary<string, object>? Filters { get; set; }

    // ── V2 structured fields ─────────────────────────────────────────────────

    /// <summary>[V2] Typed filter descriptors replacing the flat Filters map.</summary>
    public List<ReportFilterDescriptor>? FilterGroups { get; set; }

    /// <summary>[V2] Ordered list of sort descriptors.</summary>
    public List<ReportSortDescriptor>? SortDescriptors { get; set; }

    /// <summary>[V2] Column visibility map (column name → visible).</summary>
    public Dictionary<string, bool>? ColumnVisibility { get; set; }

    // ── Shared fields ────────────────────────────────────────────────────────

    /// <summary>Maximum number of rows to return (0 = unlimited).</summary>
    public int Limit { get; set; } = 0;

    /// <summary>Zero-based page offset for paginated results.</summary>
    public int PageOffset { get; set; } = 0;
}

/// <summary>
/// A single customer segment summary.
/// </summary>
public class CustomerSegmentDto
{
    /// <summary>Segment name (e.g. "Technology", "Northeast").</summary>
    public string SegmentName { get; set; } = string.Empty;

    /// <summary>Number of customers in this segment.</summary>
    public int CustomerCount { get; set; }

    /// <summary>Average revenue per customer in the segment.</summary>
    public decimal AverageRevenue { get; set; }

    /// <summary>Retention rate for the segment (0–100).</summary>
    public decimal RetentionRate { get; set; }
}

// ───────────────────────────────────────────────────────────────────────────
// Report Templates Marketplace (REV-FE-003)
// ───────────────────────────────────────────────────────────────────────────

/// <summary>
/// DTO representing a report template in the Report Templates Marketplace.
/// Field shape matches the frontend `ReportTemplate` interface in
/// CRM.Frontend/src/pages/ReportTemplatesPage.tsx exactly (camelCase over the wire).
/// </summary>
public class ReportTemplateDto
{
    /// <summary>Gets or sets the template ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the template description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the marketplace category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author display name — the linked user's full name when the
    /// template has an <c>AuthorUserId</c>, otherwise the stored <c>AuthorDisplayName</c>.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets the average rating (0-5).</summary>
    public decimal Rating { get; set; }

    /// <summary>Gets or sets the number of times this template has been applied.</summary>
    public int Downloads { get; set; }

    /// <summary>Gets or sets the search/filter tags.</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Gets or sets the optional preview image URL.</summary>
    public string? PreviewImage { get; set; }

    /// <summary>Gets or sets the saved report configuration, deserialized from JSON storage.</summary>
    public Dictionary<string, object> ReportConfig { get; set; } = new();

    /// <summary>Gets or sets when the template was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO returned by POST /api/reports/templates/{id}/apply. Carries the report
/// configuration for the frontend to hand off to the report designer, along with
/// the updated download count.
/// </summary>
public class ApplyReportTemplateResultDto
{
    /// <summary>Gets or sets the template ID that was applied.</summary>
    public int TemplateId { get; set; }

    /// <summary>Gets or sets the template name (used as the default report name in the designer).</summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets the report configuration to seed the designer with.</summary>
    public Dictionary<string, object> ReportConfig { get; set; } = new();

    /// <summary>Gets or sets the updated (post-increment) download count.</summary>
    public int Downloads { get; set; }
}
