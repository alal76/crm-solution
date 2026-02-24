// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json.Serialization;

namespace CRM.Core.DTOs.Analytics;

/// <summary>
/// Versioned report query schema definitions.
/// Implements TODO-AI005-FE-002.
///
/// These DTOs describe the shape of an analytics report query,
/// supporting filter chains, aggregations, and chart configuration.
/// The schema is version-stamped so dashboard definitions stored
/// in the database can be migrated when the shape changes.
/// </summary>
public class ReportQuerySchema
{
    /// <summary>Schema version. Current: 2.</summary>
    public int Version { get; set; } = 2;

    /// <summary>Human-readable report title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Source entity to query (e.g. "Accounts", "Opportunities", "Leads").</summary>
    public string Entity { get; set; } = string.Empty;

    /// <summary>Columns to select / display.</summary>
    public List<ReportColumn> Columns { get; set; } = new();

    /// <summary>Filter criteria (AND-joined by default).</summary>
    public List<ReportFilter> Filters { get; set; } = new();

    /// <summary>Group-by definitions for aggregation queries.</summary>
    public List<ReportGroupBy> GroupBy { get; set; } = new();

    /// <summary>Sort order definitions.</summary>
    public List<ReportSort> SortBy { get; set; } = new();

    /// <summary>Maximum number of rows to return (0 = unlimited).</summary>
    public int Limit { get; set; }

    /// <summary>Chart / visualization settings (optional).</summary>
    public ReportChartConfig? Chart { get; set; }

    /// <summary>Date range shortcut (e.g. "Last7Days", "ThisMonth", "Custom").</summary>
    public string? DateRange { get; set; }

    /// <summary>Custom start date when DateRange is "Custom".</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Custom end date when DateRange is "Custom".</summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>A column selected in the report.</summary>
public class ReportColumn
{
    /// <summary>Field name on the source entity (e.g. "Name", "Amount", "Stage").</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Display label override.</summary>
    public string? Label { get; set; }

    /// <summary>Aggregate function: None, Count, Sum, Average, Min, Max.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AggregateFunction Aggregate { get; set; } = AggregateFunction.None;

    /// <summary>Optional format string (e.g. "C2" for currency, "P0" for percentage).</summary>
    public string? Format { get; set; }
}

/// <summary>A single filter condition.</summary>
public class ReportFilter
{
    /// <summary>Field name to filter on.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Comparison operator.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;

    /// <summary>Value to compare against (serialized as string).</summary>
    public string? Value { get; set; }

    /// <summary>Secondary value for Between operator.</summary>
    public string? ValueTo { get; set; }

    /// <summary>Logical join with the next filter: "And" or "Or".</summary>
    public string Logic { get; set; } = "And";
}

/// <summary>Group-by clause.</summary>
public class ReportGroupBy
{
    /// <summary>Field name to group by.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Optional time granularity for date fields: Day, Week, Month, Quarter, Year.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TimeGranularity? Granularity { get; set; }
}

/// <summary>Sort clause.</summary>
public class ReportSort
{
    /// <summary>Field name to sort by.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Sort direction.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

/// <summary>Chart visualization configuration.</summary>
public class ReportChartConfig
{
    /// <summary>Chart type.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartType Type { get; set; } = ChartType.Bar;

    /// <summary>Field used for the X axis / labels.</summary>
    public string? XAxisField { get; set; }

    /// <summary>Field used for the Y axis / values.</summary>
    public string? YAxisField { get; set; }

    /// <summary>Optional series grouping field (for stacked/multi-series charts).</summary>
    public string? SeriesField { get; set; }

    /// <summary>Whether to show data labels on the chart.</summary>
    public bool ShowDataLabels { get; set; }

    /// <summary>Whether to show a legend.</summary>
    public bool ShowLegend { get; set; } = true;

    /// <summary>Chart color palette name or hex array.</summary>
    public List<string>? Colors { get; set; }
}

// ─── Enums ──────────────────────────────────────────────────────

/// <summary>Available aggregate functions for report columns.</summary>
public enum AggregateFunction
{
    None = 0,
    Count = 1,
    Sum = 2,
    Average = 3,
    Min = 4,
    Max = 5
}

/// <summary>Filter comparison operators.</summary>
public enum FilterOperator
{
    Equals = 0,
    NotEquals = 1,
    GreaterThan = 2,
    GreaterThanOrEquals = 3,
    LessThan = 4,
    LessThanOrEquals = 5,
    Contains = 6,
    StartsWith = 7,
    EndsWith = 8,
    IsNull = 9,
    IsNotNull = 10,
    In = 11,
    NotIn = 12,
    Between = 13
}

/// <summary>Time granularity for date grouping.</summary>
public enum TimeGranularity
{
    Day = 0,
    Week = 1,
    Month = 2,
    Quarter = 3,
    Year = 4
}

/// <summary>Sort direction.</summary>
public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

/// <summary>Supported chart types.</summary>
public enum ChartType
{
    Bar = 0,
    Line = 1,
    Pie = 2,
    Doughnut = 3,
    Area = 4,
    Scatter = 5,
    Funnel = 6,
    Table = 7,
    KPI = 8
}
