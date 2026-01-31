namespace CRM.Core.Entities.Reports;

#region Report Enumerations

/// <summary>
/// Type of report.
/// </summary>
public enum ReportType
{
    /// <summary>Tabular data report</summary>
    Table = 0,
    
    /// <summary>Bar chart</summary>
    BarChart = 1,
    
    /// <summary>Line chart</summary>
    LineChart = 2,
    
    /// <summary>Pie chart</summary>
    PieChart = 3,
    
    /// <summary>Funnel chart</summary>
    FunnelChart = 4,
    
    /// <summary>KPI/Scorecard</summary>
    KPI = 5,
    
    /// <summary>Matrix/pivot report</summary>
    Matrix = 6,
    
    /// <summary>Gauge chart</summary>
    Gauge = 7,
    
    /// <summary>Scatter plot</summary>
    ScatterPlot = 8,
    
    /// <summary>Area chart</summary>
    AreaChart = 9,
    
    /// <summary>Heat map</summary>
    HeatMap = 10,
    
    /// <summary>Combo chart</summary>
    ComboChart = 11,
    
    /// <summary>Map/Geo visualization</summary>
    Map = 12,
    
    /// <summary>Summary cards</summary>
    SummaryCards = 13
}

/// <summary>
/// Report data source type.
/// </summary>
public enum ReportDataSource
{
    /// <summary>Leads</summary>
    Leads = 0,
    
    /// <summary>Opportunities</summary>
    Opportunities = 1,
    
    /// <summary>Customers</summary>
    Customers = 2,
    
    /// <summary>Contacts</summary>
    Contacts = 3,
    
    /// <summary>Activities</summary>
    Activities = 4,
    
    /// <summary>Sales Performance</summary>
    SalesPerformance = 5,
    
    /// <summary>Pipeline</summary>
    Pipeline = 6,
    
    /// <summary>Revenue</summary>
    Revenue = 7,
    
    /// <summary>Support Cases</summary>
    SupportCases = 8,
    
    /// <summary>Marketing Campaigns</summary>
    MarketingCampaigns = 9,
    
    /// <summary>Products</summary>
    Products = 10,
    
    /// <summary>Quotes</summary>
    Quotes = 11,
    
    /// <summary>Orders</summary>
    Orders = 12,
    
    /// <summary>Invoices</summary>
    Invoices = 13,
    
    /// <summary>Custom query</summary>
    CustomQuery = 99
}

/// <summary>
/// Time period for report data.
/// </summary>
public enum ReportTimePeriod
{
    /// <summary>Today</summary>
    Today = 0,
    
    /// <summary>Yesterday</summary>
    Yesterday = 1,
    
    /// <summary>This week</summary>
    ThisWeek = 2,
    
    /// <summary>Last week</summary>
    LastWeek = 3,
    
    /// <summary>This month</summary>
    ThisMonth = 4,
    
    /// <summary>Last month</summary>
    LastMonth = 5,
    
    /// <summary>This quarter</summary>
    ThisQuarter = 6,
    
    /// <summary>Last quarter</summary>
    LastQuarter = 7,
    
    /// <summary>This year</summary>
    ThisYear = 8,
    
    /// <summary>Last year</summary>
    LastYear = 9,
    
    /// <summary>Last 7 days</summary>
    Last7Days = 10,
    
    /// <summary>Last 30 days</summary>
    Last30Days = 11,
    
    /// <summary>Last 90 days</summary>
    Last90Days = 12,
    
    /// <summary>Last 365 days</summary>
    Last365Days = 13,
    
    /// <summary>All time</summary>
    AllTime = 14,
    
    /// <summary>Custom date range</summary>
    Custom = 99
}

/// <summary>
/// Report status.
/// </summary>
public enum ReportStatus
{
    /// <summary>Draft - being edited</summary>
    Draft = 0,
    
    /// <summary>Active and available</summary>
    Active = 1,
    
    /// <summary>Archived</summary>
    Archived = 2,
    
    /// <summary>Disabled</summary>
    Disabled = 3
}

/// <summary>
/// Report access level.
/// </summary>
public enum ReportAccessLevel
{
    /// <summary>Private - only owner can view</summary>
    Private = 0,
    
    /// <summary>Team - team members can view</summary>
    Team = 1,
    
    /// <summary>Department - department can view</summary>
    Department = 2,
    
    /// <summary>Organization - everyone can view</summary>
    Organization = 3,
    
    /// <summary>Public - including external users</summary>
    Public = 4
}

#endregion

/// <summary>
/// Report definition for the visual report builder.
/// </summary>
public class ReportDefinition : BaseEntity
{
    #region Identification
    
    /// <summary>Report name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Report description</summary>
    public string? Description { get; set; }
    
    /// <summary>Report unique identifier</summary>
    public string ReportCode { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    
    /// <summary>Report category (Sales, Marketing, Service, Custom)</summary>
    public string Category { get; set; } = "Custom";
    
    /// <summary>Tags for filtering (JSON array)</summary>
    public string? TagsJson { get; set; }
    
    #endregion
    
    #region Report Configuration
    
    /// <summary>Report type</summary>
    public ReportType ReportType { get; set; }
    
    /// <summary>Primary data source</summary>
    public ReportDataSource DataSource { get; set; }
    
    /// <summary>Report status</summary>
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    /// <summary>Access level</summary>
    public ReportAccessLevel AccessLevel { get; set; } = ReportAccessLevel.Private;
    
    #endregion
    
    #region Data Configuration
    
    /// <summary>Columns/fields to include (JSON array)</summary>
    public string ColumnsJson { get; set; } = "[]";
    
    /// <summary>Filters applied (JSON array)</summary>
    public string? FiltersJson { get; set; }
    
    /// <summary>Grouping configuration (JSON)</summary>
    public string? GroupByJson { get; set; }
    
    /// <summary>Sorting configuration (JSON array)</summary>
    public string? SortByJson { get; set; }
    
    /// <summary>Aggregations (sum, avg, count, etc.) (JSON)</summary>
    public string? AggregationsJson { get; set; }
    
    /// <summary>Custom SQL or query (for advanced reports)</summary>
    public string? CustomQuery { get; set; }
    
    /// <summary>Row limit</summary>
    public int? RowLimit { get; set; }
    
    #endregion
    
    #region Time Configuration
    
    /// <summary>Default time period</summary>
    public ReportTimePeriod TimePeriod { get; set; } = ReportTimePeriod.ThisMonth;
    
    /// <summary>Date field to use for filtering</summary>
    public string? DateField { get; set; }
    
    /// <summary>Custom start date</summary>
    public DateTime? CustomStartDate { get; set; }
    
    /// <summary>Custom end date</summary>
    public DateTime? CustomEndDate { get; set; }
    
    /// <summary>Compare to previous period</summary>
    public bool CompareToPreviousPeriod { get; set; } = false;
    
    #endregion
    
    #region Visualization Settings
    
    /// <summary>Chart configuration (JSON - colors, labels, axes)</summary>
    public string? ChartConfigJson { get; set; }
    
    /// <summary>Conditional formatting rules (JSON)</summary>
    public string? ConditionalFormattingJson { get; set; }
    
    /// <summary>Show data labels</summary>
    public bool ShowDataLabels { get; set; } = true;
    
    /// <summary>Show legend</summary>
    public bool ShowLegend { get; set; } = true;
    
    /// <summary>Show totals row</summary>
    public bool ShowTotals { get; set; } = false;
    
    #endregion
    
    #region Ownership
    
    /// <summary>Creator user ID</summary>
    public int CreatedByUserId { get; set; }
    
    /// <summary>Navigation to creator</summary>
    public User? CreatedByUser { get; set; }
    
    /// <summary>Folder ID for organization</summary>
    public int? FolderId { get; set; }
    
    /// <summary>Navigation to folder</summary>
    public ReportFolder? Folder { get; set; }
    
    #endregion
    
    #region Usage Statistics
    
    /// <summary>View count</summary>
    public int ViewCount { get; set; } = 0;
    
    /// <summary>Last viewed at</summary>
    public DateTime? LastViewedAt { get; set; }
    
    /// <summary>Favorite count</summary>
    public int FavoriteCount { get; set; } = 0;
    
    /// <summary>Export count</summary>
    public int ExportCount { get; set; } = 0;
    
    #endregion
    
    #region Relationships
    
    /// <summary>Schedules for this report</summary>
    public ICollection<ReportSchedule> Schedules { get; set; } = new List<ReportSchedule>();
    
    /// <summary>Report widget configs linking to dashboard widgets</summary>
    public ICollection<ReportWidgetConfig> WidgetConfigs { get; set; } = new List<ReportWidgetConfig>();
    
    #endregion
}

/// <summary>
/// Report folder for organizing reports.
/// </summary>
public class ReportFolder : BaseEntity
{
    /// <summary>Folder name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Folder description</summary>
    public string? Description { get; set; }
    
    /// <summary>Parent folder ID</summary>
    public int? ParentFolderId { get; set; }
    
    /// <summary>Navigation to parent folder</summary>
    public ReportFolder? ParentFolder { get; set; }
    
    /// <summary>Owner user ID</summary>
    public int OwnerUserId { get; set; }
    
    /// <summary>Navigation to owner</summary>
    public User? Owner { get; set; }
    
    /// <summary>Child folders</summary>
    public ICollection<ReportFolder> ChildFolders { get; set; } = new List<ReportFolder>();
    
    /// <summary>Reports in this folder</summary>
    public ICollection<ReportDefinition> Reports { get; set; } = new List<ReportDefinition>();
}
