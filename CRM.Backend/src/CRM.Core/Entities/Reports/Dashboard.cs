// This file extends the existing Dashboard entities with report-specific functionality
// The main Dashboard and DashboardWidget entities are in CRM.Core.Entities namespace

namespace CRM.Core.Entities.Reports;

// Note: Dashboard and DashboardWidget already exist in CRM.Core.Entities
// This file contains additional report-specific enums and helper entities

#region Report Dashboard Enumerations

/// <summary>
/// Dashboard category for organization.
/// </summary>
public enum DashboardCategory
{
    /// <summary>Personal dashboard</summary>
    Personal = 0,
    
    /// <summary>Team dashboard</summary>
    Team = 1,
    
    /// <summary>Executive/leadership dashboard</summary>
    Executive = 2,
    
    /// <summary>Operations dashboard</summary>
    Operations = 3,
    
    /// <summary>Sales dashboard</summary>
    Sales = 4,
    
    /// <summary>Marketing dashboard</summary>
    Marketing = 5,
    
    /// <summary>Service dashboard</summary>
    Service = 6,
    
    /// <summary>Home dashboard</summary>
    Home = 7
}

/// <summary>
/// Widget type on dashboard.
/// </summary>
public enum WidgetType
{
    /// <summary>Report widget</summary>
    Report = 0,
    
    /// <summary>KPI metric widget</summary>
    KPI = 1,
    
    /// <summary>Chart widget</summary>
    Chart = 2,
    
    /// <summary>Activity feed widget</summary>
    ActivityFeed = 3,
    
    /// <summary>Task list widget</summary>
    TaskList = 4,
    
    /// <summary>Pipeline widget</summary>
    Pipeline = 5,
    
    /// <summary>Leaderboard widget</summary>
    Leaderboard = 6,
    
    /// <summary>Calendar widget</summary>
    Calendar = 7,
    
    /// <summary>News/announcement widget</summary>
    News = 8,
    
    /// <summary>Quick actions widget</summary>
    QuickActions = 9,
    
    /// <summary>AI insights widget</summary>
    AIInsights = 10,
    
    /// <summary>Goal progress widget</summary>
    GoalProgress = 11,
    
    /// <summary>Iframe/embed widget</summary>
    Embed = 12,
    
    /// <summary>Text/markdown widget</summary>
    Text = 13
}

/// <summary>
/// Widget size.
/// </summary>
public enum WidgetSize
{
    /// <summary>Small (1x1)</summary>
    Small = 0,
    
    /// <summary>Medium (2x1)</summary>
    Medium = 1,
    
    /// <summary>Large (2x2)</summary>
    Large = 2,
    
    /// <summary>Wide (4x1)</summary>
    Wide = 3,
    
    /// <summary>Tall (1x2)</summary>
    Tall = 4,
    
    /// <summary>Full width (4x2)</summary>
    FullWidth = 5,
    
    /// <summary>Custom size</summary>
    Custom = 99
}

/// <summary>
/// Dashboard refresh interval.
/// </summary>
public enum DashboardRefreshInterval
{
    /// <summary>No auto-refresh</summary>
    None = 0,
    
    /// <summary>Every minute</summary>
    OneMinute = 1,
    
    /// <summary>Every 5 minutes</summary>
    FiveMinutes = 5,
    
    /// <summary>Every 15 minutes</summary>
    FifteenMinutes = 15,
    
    /// <summary>Every 30 minutes</summary>
    ThirtyMinutes = 30,
    
    /// <summary>Every hour</summary>
    OneHour = 60,
    
    /// <summary>Every 4 hours</summary>
    FourHours = 240,
    
    /// <summary>Daily</summary>
    Daily = 1440
}

#endregion

/// <summary>
/// Report widget configuration - links reports to dashboard widgets.
/// This is used to store report-specific widget settings.
/// </summary>
public class ReportWidgetConfig : BaseEntity
{
    /// <summary>Dashboard widget ID</summary>
    public int DashboardWidgetId { get; set; }
    
    /// <summary>Navigation to dashboard widget</summary>
    public DashboardWidget? DashboardWidget { get; set; }
    
    /// <summary>Report definition ID</summary>
    public int ReportDefinitionId { get; set; }
    
    /// <summary>Navigation to report</summary>
    public ReportDefinition? ReportDefinition { get; set; }
    
    /// <summary>Override time period</summary>
    public ReportTimePeriod? TimePeriod { get; set; }
    
    /// <summary>Override filters (JSON)</summary>
    public string? FiltersOverrideJson { get; set; }
    
    /// <summary>Chart type override</summary>
    public ReportType? ChartTypeOverride { get; set; }
    
    /// <summary>Show legend</summary>
    public bool ShowLegend { get; set; } = true;
    
    /// <summary>Show data labels</summary>
    public bool ShowDataLabels { get; set; } = true;
    
    /// <summary>Auto refresh with dashboard</summary>
    public bool AutoRefresh { get; set; } = true;
}
