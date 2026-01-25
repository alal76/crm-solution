// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

namespace CRM.Core.Entities;

/// <summary>
/// Represents a widget on a dashboard
/// </summary>
public class DashboardWidget : BaseEntity
{
    /// <summary>
    /// Dashboard this widget belongs to
    /// </summary>
    public int DashboardId { get; set; }
    
    /// <summary>
    /// Navigation property to dashboard
    /// </summary>
    public virtual Dashboard? Dashboard { get; set; }
    
    /// <summary>
    /// Title displayed on the widget
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional subtitle or description
    /// </summary>
    public string? Subtitle { get; set; }
    
    /// <summary>
    /// Type of widget
    /// </summary>
    public WidgetType WidgetType { get; set; } = WidgetType.StatCard;
    
    /// <summary>
    /// Data source identifier (e.g., "customers.count", "opportunities.pipeline")
    /// </summary>
    public string DataSource { get; set; } = string.Empty;
    
    /// <summary>
    /// Position in grid - row index (0-based)
    /// </summary>
    public int RowIndex { get; set; } = 0;
    
    /// <summary>
    /// Position in grid - column index (0-based)
    /// </summary>
    public int ColumnIndex { get; set; } = 0;
    
    /// <summary>
    /// Number of columns this widget spans (1-4)
    /// </summary>
    public int ColumnSpan { get; set; } = 1;
    
    /// <summary>
    /// Number of rows this widget spans (1-4)
    /// </summary>
    public int RowSpan { get; set; } = 1;
    
    /// <summary>
    /// Widget display order within the dashboard
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this widget is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// Icon name for the widget (Material Icon)
    /// </summary>
    public string? IconName { get; set; }
    
    /// <summary>
    /// Primary color for the widget (hex)
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Background color or gradient
    /// </summary>
    public string? BackgroundColor { get; set; }
    
    /// <summary>
    /// Navigation link when widget is clicked
    /// </summary>
    public string? NavigationLink { get; set; }
    
    /// <summary>
    /// JSON configuration for chart options, filters, etc.
    /// </summary>
    public string? ConfigJson { get; set; }
    
    /// <summary>
    /// Whether to show a trend indicator
    /// </summary>
    public bool ShowTrend { get; set; } = false;
    
    /// <summary>
    /// Comparison period for trend (days)
    /// </summary>
    public int TrendPeriodDays { get; set; } = 30;
    
    /// <summary>
    /// Widget-specific refresh interval in seconds (0 = use dashboard default)
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 0;
}

/// <summary>
/// Types of dashboard widgets
/// </summary>
public enum WidgetType
{
    /// <summary>Simple stat card with value and icon</summary>
    StatCard = 0,
    /// <summary>Line chart for trends</summary>
    LineChart = 1,
    /// <summary>Bar chart for comparisons</summary>
    BarChart = 2,
    /// <summary>Pie or donut chart for distributions</summary>
    PieChart = 3,
    /// <summary>Data table with rows</summary>
    DataTable = 4,
    /// <summary>List of recent activities</summary>
    ActivityList = 5,
    /// <summary>List of tasks</summary>
    TaskList = 6,
    /// <summary>Pipeline funnel visualization</summary>
    PipelineFunnel = 7,
    /// <summary>Progress indicator/gauge</summary>
    ProgressGauge = 8,
    /// <summary>Map visualization</summary>
    MapWidget = 9,
    /// <summary>Calendar view</summary>
    CalendarWidget = 10,
    /// <summary>Custom HTML/Markdown content</summary>
    CustomContent = 11,
    /// <summary>Leaderboard ranking</summary>
    Leaderboard = 12,
    /// <summary>KPI with target comparison</summary>
    KPICard = 13,
    /// <summary>Area chart</summary>
    AreaChart = 14,
    /// <summary>Stacked bar chart</summary>
    StackedBarChart = 15
}
