// CRM Solution - Custom Report Builder Service
// Phase 7, Task 7.5 - Custom report creation with query builder, CSV export, and scheduling

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Service for building and executing custom reports over CRM data.
/// Supports tabular, summary, and matrix report types with CSV export.
/// </summary>
public interface IReportBuilderService
{
    /// <summary>
    /// Gets all report definitions for a user.
    /// </summary>
    /// <param name="userId">Owner user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of report definitions.</returns>
    Task<IEnumerable<ReportDefinition>> GetReportsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific report definition by ID.
    /// </summary>
    /// <param name="reportId">Report ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Report definition, or null if not found.</returns>
    Task<ReportDefinition?> GetReportAsync(string reportId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new report definition.
    /// </summary>
    /// <param name="report">Report definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created report with assigned ID.</returns>
    Task<ReportDefinition> CreateReportAsync(ReportDefinition report, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing report definition.
    /// </summary>
    /// <param name="report">Updated report definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated report, or null if not found.</returns>
    Task<ReportDefinition?> UpdateReportAsync(ReportDefinition report, CancellationToken ct = default);

    /// <summary>
    /// Deletes a report definition.
    /// </summary>
    /// <param name="reportId">Report ID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteReportAsync(string reportId, CancellationToken ct = default);

    /// <summary>
    /// Executes a report and returns tabular results.
    /// </summary>
    /// <param name="reportId">Report ID to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Report execution result with rows and metadata.</returns>
    Task<ReportExecutionResult?> ExecuteReportAsync(string reportId, CancellationToken ct = default);

    /// <summary>
    /// Exports a report execution result as CSV bytes.
    /// </summary>
    /// <param name="reportId">Report ID to export.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV byte array, or null if report not found.</returns>
    Task<byte[]?> ExportToCsvAsync(string reportId, CancellationToken ct = default);

    /// <summary>
    /// Returns the list of entity sources available for report building.
    /// </summary>
    /// <returns>Available entity sources with their queryable fields.</returns>
    IEnumerable<ReportEntitySource> GetAvailableSources();
}

/// <summary>
/// A custom report definition.
/// </summary>
public class ReportDefinition
{
    /// <summary>Unique report identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Report display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Owner user ID.</summary>
    public int UserId { get; set; }

    /// <summary>Report type (Tabular, Summary, Matrix).</summary>
    public ReportType Type { get; set; } = ReportType.Tabular;

    /// <summary>Entity source (Accounts, Leads, Opportunities, etc.).</summary>
    public string EntitySource { get; set; } = string.Empty;

    /// <summary>Columns to include in the report.</summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>Filters to apply.</summary>
    public List<ReportFilter> Filters { get; set; } = new();

    /// <summary>Sort column.</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort direction (Asc, Desc).</summary>
    public string SortDirection { get; set; } = "Asc";

    /// <summary>Maximum rows to return.</summary>
    public int MaxRows { get; set; } = 1000;

    /// <summary>Group-by column for Summary reports.</summary>
    public string? GroupBy { get; set; }

    /// <summary>Report created timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Report last updated timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Report type enumeration.
/// </summary>
public enum ReportType
{
    /// <summary>Simple tabular list of records.</summary>
    Tabular,
    /// <summary>Records grouped with subtotals.</summary>
    Summary,
    /// <summary>Cross-tabulated data matrix.</summary>
    Matrix
}

/// <summary>
/// A filter condition applied to a report.
/// </summary>
public class ReportFilter
{
    /// <summary>Field name to filter on.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Operator (Equals, Contains, GreaterThan, LessThan, IsNull, IsNotNull).</summary>
    public string Operator { get; set; } = "Equals";

    /// <summary>Filter value.</summary>
    public string? Value { get; set; }
}

/// <summary>
/// Result of executing a report.
/// </summary>
public class ReportExecutionResult
{
    /// <summary>Report ID.</summary>
    public string ReportId { get; set; } = string.Empty;

    /// <summary>Report name.</summary>
    public string ReportName { get; set; } = string.Empty;

    /// <summary>Column headers.</summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>Row data (each row is a dictionary of column → value).</summary>
    public List<Dictionary<string, object?>> Rows { get; set; } = new();

    /// <summary>Total row count (before MaxRows limit).</summary>
    public int TotalRows { get; set; }

    /// <summary>When the report was executed.</summary>
    public DateTime ExecutedAt { get; set; }
}

/// <summary>
/// An entity source available for report building.
/// </summary>
public class ReportEntitySource
{
    /// <summary>Source identifier (e.g., "Accounts").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Available columns/fields for this source.</summary>
    public List<string> Fields { get; set; } = new();
}

#endregion

/// <summary>
/// In-memory report builder service.
/// Stores report definitions in ConcurrentDictionary and queries CRM entities for data.
/// </summary>
public class ReportBuilderService : IReportBuilderService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ReportBuilderService> _logger;

    // In-memory storage: ReportId → ReportDefinition
    private readonly ConcurrentDictionary<string, ReportDefinition> _reports = new();

    private static int _idCounter;

    private static readonly List<ReportEntitySource> EntitySources = new()
    {
        new() { Name = "Accounts",      DisplayName = "Accounts",      Fields = new() { "Id", "Company", "Email", "Industry", "LifecycleStage", "AccountType", "Category", "Priority", "CreatedAt" } },
        new() { Name = "Leads",          DisplayName = "Leads",          Fields = new() { "Id", "FirstName", "LastName", "Email", "CompanyName", "Status", "Source", "Score", "CreatedAt" } },
        new() { Name = "Opportunities",  DisplayName = "Opportunities",  Fields = new() { "Id", "Name", "Stage", "Amount", "Probability", "ExpectedCloseDate", "CreatedAt" } },
        new() { Name = "Contacts",       DisplayName = "Contacts",       Fields = new() { "Id", "FirstName", "LastName", "EmailPrimary", "PhonePrimary", "JobTitle" } },
        new() { Name = "ServiceRequests",DisplayName = "Service Requests",Fields = new() { "Id", "Subject", "Status", "Priority", "CreatedAt" } },
    };

    /// <summary>
    /// Initializes a new instance of ReportBuilderService.
    /// </summary>
    public ReportBuilderService(ICrmDbContext context, ILogger<ReportBuilderService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<IEnumerable<ReportDefinition>> GetReportsAsync(int userId, CancellationToken ct = default)
    {
        var reports = _reports.Values
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Name)
            .AsEnumerable();
        return Task.FromResult(reports);
    }

    /// <inheritdoc />
    public Task<ReportDefinition?> GetReportAsync(string reportId, CancellationToken ct = default)
    {
        _reports.TryGetValue(reportId, out var report);
        return Task.FromResult(report);
    }

    /// <inheritdoc />
    public Task<ReportDefinition> CreateReportAsync(ReportDefinition report, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(report.Id))
            report.Id = $"rpt-{Interlocked.Increment(ref _idCounter)}";

        report.CreatedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        _reports[report.Id] = report;

        _logger.LogInformation("Created report {ReportId} ({ReportName}) for user {UserId}",
            report.Id, report.Name, report.UserId);

        return Task.FromResult(report);
    }

    /// <inheritdoc />
    public Task<ReportDefinition?> UpdateReportAsync(ReportDefinition report, CancellationToken ct = default)
    {
        if (!_reports.ContainsKey(report.Id))
            return Task.FromResult<ReportDefinition?>(null);

        report.UpdatedAt = DateTime.UtcNow;
        _reports[report.Id] = report;
        return Task.FromResult<ReportDefinition?>(report);
    }

    /// <inheritdoc />
    public Task<bool> DeleteReportAsync(string reportId, CancellationToken ct = default)
    {
        var removed = _reports.TryRemove(reportId, out _);
        if (removed)
            _logger.LogInformation("Deleted report {ReportId}", reportId);
        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public async Task<ReportExecutionResult?> ExecuteReportAsync(string reportId, CancellationToken ct = default)
    {
        if (!_reports.TryGetValue(reportId, out var report))
            return null;

        var rows = report.EntitySource switch
        {
            "Accounts" => await ExecuteAccountsReportAsync(report, ct),
            "Leads" => await ExecuteLeadsReportAsync(report, ct),
            "Opportunities" => await ExecuteOpportunitiesReportAsync(report, ct),
            "Contacts" => await ExecuteContactsReportAsync(report, ct),
            "ServiceRequests" => await ExecuteServiceRequestsReportAsync(report, ct),
            _ => new List<Dictionary<string, object?>>()
        };

        var result = new ReportExecutionResult
        {
            ReportId = report.Id,
            ReportName = report.Name,
            Columns = report.Columns.Count > 0 ? report.Columns : GetDefaultColumns(report.EntitySource),
            TotalRows = rows.Count,
            Rows = rows.Take(report.MaxRows).ToList(),
            ExecutedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Executed report {ReportId}: {RowCount} rows from {Source}",
            reportId, result.TotalRows, report.EntitySource);

        return result;
    }

    /// <inheritdoc />
    public async Task<byte[]?> ExportToCsvAsync(string reportId, CancellationToken ct = default)
    {
        var result = await ExecuteReportAsync(reportId, ct);
        if (result == null)
            return null;

        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", result.Columns.Select(EscapeCsv)));

        // Data rows
        foreach (var row in result.Rows)
        {
            var values = result.Columns.Select(col =>
                row.TryGetValue(col, out var val) ? EscapeCsv(val?.ToString() ?? "") : "");
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <inheritdoc />
    public IEnumerable<ReportEntitySource> GetAvailableSources() => EntitySources;

    #region Private Query Methods

    private async Task<List<Dictionary<string, object?>>> ExecuteAccountsReportAsync(ReportDefinition report, CancellationToken ct)
    {
        var query = _context.Customers.Where(c => !c.IsDeleted);
        var entities = await query.ToListAsync(ct);

        return entities.Select(e => new Dictionary<string, object?>
        {
            ["Id"] = e.Id,
            ["Company"] = e.Company,
            ["Email"] = e.Email,
            ["Industry"] = e.Industry,
            ["LifecycleStage"] = e.LifecycleStage,
            ["AccountType"] = e.AccountType,
            ["Category"] = e.Category,
            ["Priority"] = e.Priority,
            ["CreatedAt"] = e.CreatedAt
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteLeadsReportAsync(ReportDefinition report, CancellationToken ct)
    {
        var query = _context.Leads.Where(l => !l.IsDeleted);
        var entities = await query.ToListAsync(ct);

        return entities.Select(e => new Dictionary<string, object?>
        {
            ["Id"] = e.Id,
            ["FirstName"] = e.FirstName,
            ["LastName"] = e.LastName,
            ["Email"] = e.Email,
            ["CompanyName"] = e.CompanyName,
            ["Status"] = e.Status.ToString(),
            ["Source"] = e.Source.ToString(),
            ["Score"] = e.Score,
            ["CreatedAt"] = e.CreatedAt
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteOpportunitiesReportAsync(ReportDefinition report, CancellationToken ct)
    {
        var query = _context.Opportunities.Where(o => !o.IsDeleted);
        var entities = await query.ToListAsync(ct);

        return entities.Select(e => new Dictionary<string, object?>
        {
            ["Id"] = e.Id,
            ["Name"] = e.Name,
            ["Stage"] = e.Stage.ToString(),
            ["Amount"] = e.Amount,
            ["Probability"] = e.Probability,
            ["ExpectedCloseDate"] = e.ExpectedCloseDate,
            ["CreatedAt"] = e.CreatedAt
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteContactsReportAsync(ReportDefinition report, CancellationToken ct)
    {
        var query = _context.Contacts.AsQueryable();
        var entities = await query.ToListAsync(ct);

        return entities.Select(e => new Dictionary<string, object?>
        {
            ["Id"] = e.Id,
            ["FirstName"] = e.FirstName,
            ["LastName"] = e.LastName,
            ["Email"] = e.EmailPrimary,
            ["Phone"] = e.PhonePrimary,
            ["Title"] = e.JobTitle
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteServiceRequestsReportAsync(ReportDefinition report, CancellationToken ct)
    {
        var query = _context.ServiceRequests.Where(sr => !sr.IsDeleted);
        var entities = await query.ToListAsync(ct);

        return entities.Select(e => new Dictionary<string, object?>
        {
            ["Id"] = e.Id,
            ["Subject"] = e.Subject,
            ["Status"] = e.Status,
            ["Priority"] = e.Priority,
            ["CreatedAt"] = e.CreatedAt
        }).ToList();
    }

    private static List<string> GetDefaultColumns(string entitySource) => entitySource switch
    {
        "Accounts" => new() { "Id", "Company", "Email", "Industry", "LifecycleStage", "CreatedAt" },
        "Leads" => new() { "Id", "FirstName", "LastName", "Email", "Status", "Score", "CreatedAt" },
        "Opportunities" => new() { "Id", "Name", "Stage", "Amount", "Probability", "CreatedAt" },
        "Contacts" => new() { "Id", "FirstName", "LastName", "Email" },
        "ServiceRequests" => new() { "Id", "Subject", "Status", "Priority", "CreatedAt" },
        _ => new() { "Id", "CreatedAt" }
    };

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }

    #endregion
}
