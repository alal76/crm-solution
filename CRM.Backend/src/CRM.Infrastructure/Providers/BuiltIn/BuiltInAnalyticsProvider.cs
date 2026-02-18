// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Providers.BuiltIn;

/// <summary>
/// Built-in analytics provider using direct database queries.
/// Provides basic dashboard statistics, pipeline reports, and predefined reports.
/// For advanced analytics and dashboards, use Superset or other external providers.
/// </summary>
public class BuiltInAnalyticsProvider : IAnalyticsPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<BuiltInAnalyticsProvider> _logger;

    // Predefined reports available in BuiltIn provider
    private static readonly Dictionary<string, ReportDefinition> _predefinedReports = new()
    {
        ["sales-pipeline"] = new ReportDefinition
        {
            Id = "sales-pipeline",
            Name = "Sales Pipeline Report",
            Description = "Opportunity pipeline analysis by stage",
            Category = "Sales",
            Parameters = new List<ReportParameter>
            {
                new() { Name = "startDate", DisplayName = "Start Date", Type = "date", Required = false },
                new() { Name = "endDate", DisplayName = "End Date", Type = "date", Required = false }
            }
        },
        ["account-summary"] = new ReportDefinition
        {
            Id = "account-summary",
            Name = "Account Summary Report",
            Description = "Overview of all accounts with key metrics",
            Category = "Accounts",
            Parameters = new List<ReportParameter>
            {
                new() { Name = "accountType", DisplayName = "Account Type", Type = "select", Required = false, Options = new List<object> { "Customer", "Prospect", "Partner" } }
            }
        },
        ["activity-report"] = new ReportDefinition
        {
            Id = "activity-report",
            Name = "Activity Report",
            Description = "Activities logged across all entities",
            Category = "Activities",
            Parameters = new List<ReportParameter>
            {
                new() { Name = "startDate", DisplayName = "Start Date", Type = "date", Required = true },
                new() { Name = "endDate", DisplayName = "End Date", Type = "date", Required = true },
                new() { Name = "activityType", DisplayName = "Activity Type", Type = "select", Required = false }
            }
        },
        ["lead-conversion"] = new ReportDefinition
        {
            Id = "lead-conversion",
            Name = "Lead Conversion Report",
            Description = "Lead to opportunity conversion analysis",
            Category = "Sales",
            Parameters = new List<ReportParameter>
            {
                new() { Name = "startDate", DisplayName = "Start Date", Type = "date", Required = false },
                new() { Name = "endDate", DisplayName = "End Date", Type = "date", Required = false }
            }
        },
        ["product-revenue"] = new ReportDefinition
        {
            Id = "product-revenue",
            Name = "Product Revenue Report",
            Description = "Revenue breakdown by product",
            Category = "Products",
            Parameters = new List<ReportParameter>
            {
                new() { Name = "year", DisplayName = "Year", Type = "number", Required = false, DefaultValue = DateTime.UtcNow.Year }
            }
        },
        ["user-activity"] = new ReportDefinition
        {
            Id = "user-activity",
            Name = "User Activity Report",
            Description = "User activity and engagement metrics",
            Category = "Users",
            Parameters = new List<ReportParameter>
            {
                new() { Name = "userId", DisplayName = "User ID", Type = "number", Required = false },
                new() { Name = "days", DisplayName = "Days Back", Type = "number", Required = false, DefaultValue = 30 }
            }
        }
    };

    // Predefined dashboards (basic stats views)
    private static readonly List<DashboardInfo> _predefinedDashboards = new()
    {
        new DashboardInfo
        {
            Id = "overview",
            Name = "Overview Dashboard",
            Description = "Key CRM metrics at a glance",
            Category = "General",
            CanEmbed = false,
            ChartCount = 6
        },
        new DashboardInfo
        {
            Id = "sales",
            Name = "Sales Dashboard",
            Description = "Sales pipeline and performance metrics",
            Category = "Sales",
            CanEmbed = false,
            ChartCount = 8
        },
        new DashboardInfo
        {
            Id = "accounts",
            Name = "Accounts Dashboard",
            Description = "Account overview and health metrics",
            Category = "Accounts",
            CanEmbed = false,
            ChartCount = 5
        },
        new DashboardInfo
        {
            Id = "activities",
            Name = "Activities Dashboard",
            Description = "Activity tracking and trends",
            Category = "Activities",
            CanEmbed = false,
            ChartCount = 4
        }
    };

    public BuiltInAnalyticsProvider(
        ICrmDbContext context,
        ILogger<BuiltInAnalyticsProvider> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderName => "BuiltIn";

    /// <inheritdoc />
    public bool SupportsEmbedding => false;

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn provider is always available if we can connect to the database
        return Task.FromResult(true);
    }

    #region Dashboard Operations

    /// <inheritdoc />
    public Task<IEnumerable<DashboardInfo>> GetDashboardsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all built-in dashboards");
        return Task.FromResult<IEnumerable<DashboardInfo>>(_predefinedDashboards);
    }

    /// <inheritdoc />
    public Task<DashboardInfo?> GetDashboardAsync(string dashboardId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dashboard: {DashboardId}", dashboardId);
        var dashboard = _predefinedDashboards.FirstOrDefault(d => d.Id == dashboardId);
        return Task.FromResult(dashboard);
    }

    /// <inheritdoc />
    public Task<IEnumerable<DashboardInfo>> GetDashboardsForUserAsync(
        int userId,
        IEnumerable<string>? roles = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dashboards for user {UserId} with roles {Roles}", userId, roles);
        // BuiltIn provider doesn't have role-based filtering - return all dashboards
        return Task.FromResult<IEnumerable<DashboardInfo>>(_predefinedDashboards);
    }

    #endregion

    #region Embedding Operations

    /// <inheritdoc />
    public Task<EmbedResult> GetEmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BuiltIn analytics provider does not support embedding. Use Superset or PowerBI for embedded dashboards.");

        return Task.FromResult(new EmbedResult
        {
            EmbedType = "unsupported",
            Config = new Dictionary<string, object>
            {
                ["error"] = "BuiltIn provider does not support embedding",
                ["suggestion"] = "Configure Superset, Metabase, or PowerBI for embedded dashboards"
            }
        });
    }

    /// <inheritdoc />
    public Task<string> GetGuestTokenAsync(
        string dashboardId,
        Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BuiltIn analytics provider does not support guest tokens");
        throw new NotSupportedException("BuiltIn analytics provider does not support guest tokens. Use Superset or PowerBI.");
    }

    #endregion

    #region Chart/Widget Operations

    /// <inheritdoc />
    public Task<IEnumerable<ChartInfo>> GetChartsAsync(string? dashboardId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting charts for dashboard: {DashboardId}", dashboardId ?? "all");

        var charts = new List<ChartInfo>
        {
            new() { Id = "account-count", Name = "Total Accounts", ChartType = "metric", DashboardId = "overview", CanEmbed = false },
            new() { Id = "contact-count", Name = "Total Contacts", ChartType = "metric", DashboardId = "overview", CanEmbed = false },
            new() { Id = "opportunity-value", Name = "Open Opportunity Value", ChartType = "metric", DashboardId = "overview", CanEmbed = false },
            new() { Id = "pipeline-chart", Name = "Pipeline by Stage", ChartType = "bar", DashboardId = "sales", CanEmbed = false },
            new() { Id = "monthly-revenue", Name = "Monthly Revenue Trend", ChartType = "line", DashboardId = "sales", CanEmbed = false },
            new() { Id = "activity-timeline", Name = "Activity Timeline", ChartType = "timeline", DashboardId = "activities", CanEmbed = false },
            new() { Id = "account-by-type", Name = "Accounts by Type", ChartType = "pie", DashboardId = "accounts", CanEmbed = false }
        };

        if (!string.IsNullOrEmpty(dashboardId))
        {
            charts = charts.Where(c => c.DashboardId == dashboardId).ToList();
        }

        return Task.FromResult<IEnumerable<ChartInfo>>(charts);
    }

    /// <inheritdoc />
    public Task<EmbedResult> GetChartEmbedAsync(
        string chartId,
        Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BuiltIn analytics provider does not support chart embedding");

        return Task.FromResult(new EmbedResult
        {
            EmbedType = "unsupported",
            Config = new Dictionary<string, object>
            {
                ["error"] = "BuiltIn provider does not support chart embedding",
                ["suggestion"] = "Configure Superset or PowerBI for embedded charts"
            }
        });
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    public async Task<ReportResult> ExecuteReportAsync(
        string reportId,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing report: {ReportId} with parameters: {Parameters}", reportId, parameters);

        var startTime = DateTime.UtcNow;

        try
        {
            return reportId.ToLowerInvariant() switch
            {
                "sales-pipeline" => await ExecuteSalesPipelineReportAsync(parameters, cancellationToken),
                "account-summary" => await ExecuteAccountSummaryReportAsync(parameters, cancellationToken),
                "activity-report" => await ExecuteActivityReportAsync(parameters, cancellationToken),
                "lead-conversion" => await ExecuteLeadConversionReportAsync(parameters, cancellationToken),
                "product-revenue" => await ExecuteProductRevenueReportAsync(parameters, cancellationToken),
                "user-activity" => await ExecuteUserActivityReportAsync(parameters, cancellationToken),
                _ => new ReportResult
                {
                    ReportId = reportId,
                    Success = false,
                    ExecutedAt = startTime,
                    ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                    Error = $"Unknown report: {reportId}"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing report {ReportId}", reportId);
            return new ReportResult
            {
                ReportId = reportId,
                Success = false,
                ExecutedAt = startTime,
                ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                Error = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<ReportInfo>> GetReportsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting reports for category: {Category}", category ?? "all");

        var reports = _predefinedReports.Values.Select(r => new ReportInfo
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Category = r.Category,
            Parameters = r.Parameters,
            OutputFormats = new List<string> { "json" }
        });

        if (!string.IsNullOrEmpty(category))
        {
            reports = reports.Where(r => r.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
        }

        return Task.FromResult(reports);
    }

    #endregion

    #region Data Source Operations

    /// <inheritdoc />
    public Task<IEnumerable<DataSourceInfo>> GetDataSourcesAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn provider uses the CRM database directly
        var dataSources = new List<DataSourceInfo>
        {
            new()
            {
                Id = "crm-database",
                Name = "CRM Database",
                Type = "mariadb",
                LastRefreshed = DateTime.UtcNow,
                Status = "connected"
            }
        };

        return Task.FromResult<IEnumerable<DataSourceInfo>>(dataSources);
    }

    /// <inheritdoc />
    public Task<RefreshJobStatus> RefreshDataSourceAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refresh requested for data source: {DataSourceId}", dataSourceId);

        // BuiltIn provider uses live database queries - no refresh needed
        return Task.FromResult(new RefreshJobStatus
        {
            JobId = Guid.NewGuid().ToString(),
            Status = "completed",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });
    }

    #endregion

    /// <inheritdoc />
    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderHealthResult
        {
            IsHealthy = true,
            ProviderName = ProviderName,
            Message = "BuiltIn analytics provider is operational",
            Details = new Dictionary<string, object>
            {
                ["dashboardCount"] = _predefinedDashboards.Count,
                ["reportCount"] = _predefinedReports.Count,
                ["supportsEmbedding"] = false
            }
        });
    }

    #region Private Report Implementations

    private async Task<ReportResult> ExecuteSalesPipelineReportAsync(
        Dictionary<string, object>? parameters,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        var query = _context.Opportunities.AsQueryable();

        // Apply date filters if provided
        if (parameters?.TryGetValue("startDate", out var startDateObj) == true && startDateObj is DateTime startDate)
        {
            query = query.Where(o => o.CreatedAt >= startDate);
        }
        if (parameters?.TryGetValue("endDate", out var endDateObj) == true && endDateObj is DateTime endDate)
        {
            query = query.Where(o => o.CreatedAt <= endDate);
        }

        var pipelineData = await query
            .GroupBy(o => o.Stage)
            .Select(g => new
            {
                Stage = g.Key,
                Count = g.Count(),
                TotalValue = g.Sum(o => o.Amount),
                WeightedValue = g.Sum(o => o.Amount * o.Probability / 100)
            })
            .ToListAsync(cancellationToken);

        var rows = pipelineData
            .Select(p => new Dictionary<string, object>
            {
                ["Stage"] = p.Stage.ToString(),
                ["Count"] = p.Count,
                ["TotalValue"] = p.TotalValue,
                ["WeightedValue"] = p.WeightedValue
            })
            .ToList();

        return new ReportResult
        {
            ReportId = "sales-pipeline",
            Success = true,
            ExecutedAt = startTime,
            ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            RowCount = rows.Count,
            Columns = new List<string> { "Stage", "Count", "TotalValue", "WeightedValue" },
            Rows = rows
        };
    }

    private async Task<ReportResult> ExecuteAccountSummaryReportAsync(
        Dictionary<string, object>? parameters,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        var query = _context.Accounts.AsQueryable();

        // Apply category filter if provided
        if (parameters?.TryGetValue("accountType", out var accountTypeObj) == true && accountTypeObj is string accountType)
        {
            if (Enum.TryParse<AccountCategory>(accountType, out var category))
            {
                query = query.Where(a => a.Category == category);
            }
        }

        var accounts = await query
            .Select(a => new
            {
                a.Id,
                Name = a.Category == AccountCategory.Individual
                    ? (a.FirstName + " " + a.LastName)
                    : a.Company,
                Category = a.Category.ToString(),
                a.Industry,
                a.Website,
                a.CreatedAt,
                ContactCount = a.AccountContacts != null ? a.AccountContacts.Count() : 0,
                OpportunityCount = a.Opportunities != null ? a.Opportunities.Count() : 0
            })
            .ToListAsync(cancellationToken);

        var rows = accounts
            .Select(a => new Dictionary<string, object>
            {
                ["Id"] = a.Id,
                ["Name"] = a.Name ?? "",
                ["Category"] = a.Category ?? "",
                ["Industry"] = a.Industry ?? "",
                ["Website"] = a.Website ?? "",
                ["CreatedAt"] = a.CreatedAt,
                ["ContactCount"] = a.ContactCount,
                ["OpportunityCount"] = a.OpportunityCount
            })
            .ToList();

        return new ReportResult
        {
            ReportId = "account-summary",
            Success = true,
            ExecutedAt = startTime,
            ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            RowCount = rows.Count,
            Columns = new List<string> { "Id", "Name", "Category", "Industry", "Website", "CreatedAt", "ContactCount", "OpportunityCount" },
            Rows = rows
        };
    }

    private async Task<ReportResult> ExecuteActivityReportAsync(
        Dictionary<string, object>? parameters,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // Default to last 30 days
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        if (parameters?.TryGetValue("startDate", out var startDateObj) == true)
        {
            if (startDateObj is DateTime sd) startDate = sd;
            else if (startDateObj is string sds && DateTime.TryParse(sds, out var parsedSd)) startDate = parsedSd;
        }
        if (parameters?.TryGetValue("endDate", out var endDateObj) == true)
        {
            if (endDateObj is DateTime ed) endDate = ed;
            else if (endDateObj is string eds && DateTime.TryParse(eds, out var parsedEd)) endDate = parsedEd;
        }

        var query = _context.Activities
            .Where(a => a.ActivityDate >= startDate && a.ActivityDate <= endDate);

        // Apply activity type filter if provided
        if (parameters?.TryGetValue("activityType", out var activityTypeObj) == true && activityTypeObj is string activityType)
        {
            query = query.Where(a => a.ActivityType.ToString() == activityType);
        }

        var activities = await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(1000) // Limit results
            .Select(a => new
            {
                a.Id,
                a.Title,
                ActivityType = a.ActivityType.ToString(),
                a.ActivityDate,
                a.EntityType,
                a.EntityId,
                a.UserName
            })
            .ToListAsync(cancellationToken);

        var rows = activities
            .Select(a => new Dictionary<string, object>
            {
                ["Id"] = a.Id,
                ["Title"] = a.Title ?? "",
                ["ActivityType"] = a.ActivityType,
                ["ActivityDate"] = a.ActivityDate,
                ["EntityType"] = a.EntityType ?? "",
                ["EntityId"] = a.EntityId ?? 0,
                ["UserName"] = a.UserName ?? ""
            })
            .ToList();

        return new ReportResult
        {
            ReportId = "activity-report",
            Success = true,
            ExecutedAt = startTime,
            ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            RowCount = rows.Count,
            Columns = new List<string> { "Id", "Title", "ActivityType", "ActivityDate", "EntityType", "EntityId", "UserName" },
            Rows = rows
        };
    }

    private async Task<ReportResult> ExecuteLeadConversionReportAsync(
        Dictionary<string, object>? parameters,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // Get lead statistics grouped by status
        var leadStats = await _context.Leads
            .GroupBy(l => l.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalLeads = leadStats.Sum(l => l.Count);
        var convertedCount = leadStats.FirstOrDefault(l => l.Status == LeadLifecycleStatus.Converted)?.Count ?? 0;
        var conversionRate = totalLeads > 0 ? (double)convertedCount / totalLeads * 100 : 0;

        var rows = leadStats
            .Select(l => new Dictionary<string, object>
            {
                ["Status"] = l.Status.ToString(),
                ["Count"] = l.Count,
                ["Percentage"] = totalLeads > 0 ? Math.Round((double)l.Count / totalLeads * 100, 2) : 0
            })
            .ToList();

        // Add summary row
        rows.Add(new Dictionary<string, object>
        {
            ["Status"] = "TOTAL",
            ["Count"] = totalLeads,
            ["Percentage"] = 100.0
        });

        return new ReportResult
        {
            ReportId = "lead-conversion",
            Success = true,
            ExecutedAt = startTime,
            ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            RowCount = rows.Count,
            Columns = new List<string> { "Status", "Count", "Percentage" },
            Rows = rows
        };
    }

    private async Task<ReportResult> ExecuteProductRevenueReportAsync(
        Dictionary<string, object>? parameters,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // Get product revenue from won opportunities
        var products = await _context.Products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Category,
                p.Price,
                // Count from opportunity line items would be ideal but simplified here
                UnitsSold = 0,
                Revenue = p.Price
            })
            .ToListAsync(cancellationToken);

        var rows = products
            .Select(p => new Dictionary<string, object>
            {
                ["Id"] = p.Id,
                ["Name"] = p.Name ?? "",
                ["Category"] = p.Category ?? "",
                ["UnitPrice"] = p.Price,
                ["UnitsSold"] = p.UnitsSold,
                ["Revenue"] = p.Revenue
            })
            .ToList();

        return new ReportResult
        {
            ReportId = "product-revenue",
            Success = true,
            ExecutedAt = startTime,
            ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            RowCount = rows.Count,
            Columns = new List<string> { "Id", "Name", "Category", "UnitPrice", "UnitsSold", "Revenue" },
            Rows = rows
        };
    }

    private async Task<ReportResult> ExecuteUserActivityReportAsync(
        Dictionary<string, object>? parameters,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        var days = 30;
        if (parameters?.TryGetValue("days", out var daysObj) == true)
        {
            if (daysObj is int d) days = d;
            else if (daysObj is long dl) days = (int)dl;
            else if (daysObj is string ds && int.TryParse(ds, out var parsedDays)) days = parsedDays;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var query = _context.Activities
            .Where(a => a.ActivityDate >= cutoffDate);

        // Filter by user if provided
        if (parameters?.TryGetValue("userId", out var userIdObj) == true)
        {
            int? userId = null;
            if (userIdObj is int ui) userId = ui;
            else if (userIdObj is long ul) userId = (int)ul;
            else if (userIdObj is string us && int.TryParse(us, out var parsedUid)) userId = parsedUid;

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }
        }

        var userActivities = await query
            .GroupBy(a => a.UserName)
            .Select(g => new
            {
                UserName = g.Key,
                ActivityCount = g.Count(),
                LastActivity = g.Max(a => a.ActivityDate)
            })
            .OrderByDescending(u => u.ActivityCount)
            .ToListAsync(cancellationToken);

        var rows = userActivities
            .Select(u => new Dictionary<string, object>
            {
                ["UserName"] = u.UserName ?? "Unknown",
                ["ActivityCount"] = u.ActivityCount,
                ["LastActivity"] = u.LastActivity
            })
            .ToList();

        return new ReportResult
        {
            ReportId = "user-activity",
            Success = true,
            ExecutedAt = startTime,
            ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            RowCount = rows.Count,
            Columns = new List<string> { "UserName", "ActivityCount", "LastActivity" },
            Rows = rows
        };
    }

    #endregion

    #region Helper Classes

    private class ReportDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public List<ReportParameter>? Parameters { get; set; }
    }

    #endregion
}
