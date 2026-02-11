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
