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

namespace CRM.Core.Entities;

#region Sales Quota Enumerations

/// <summary>
/// FUNCTIONAL: Quota period type.
/// TECHNICAL: Determines quota calculation period.
/// </summary>
public enum QuotaPeriodType
{
    /// <summary>Monthly quota</summary>
    Monthly = 0,

    /// <summary>Quarterly quota</summary>
    Quarterly = 1,

    /// <summary>Annual quota</summary>
    Annual = 2,

    /// <summary>Semi-annual quota</summary>
    SemiAnnual = 3,

    /// <summary>Weekly quota</summary>
    Weekly = 4,

    /// <summary>Custom period</summary>
    Custom = 5
}

/// <summary>
/// FUNCTIONAL: What the quota measures.
/// TECHNICAL: Determines attainment calculation.
/// </summary>
public enum QuotaMetric
{
    /// <summary>Revenue/bookings</summary>
    Revenue = 0,

    /// <summary>Number of deals closed</summary>
    DealsCount = 1,

    /// <summary>Units sold</summary>
    UnitsSold = 2,

    /// <summary>New customers acquired</summary>
    NewCustomers = 3,

    /// <summary>Pipeline created</summary>
    PipelineCreated = 4,

    /// <summary>Meetings booked</summary>
    MeetingsBooked = 5,

    /// <summary>Qualified leads</summary>
    QualifiedLeads = 6,

    /// <summary>MRR/ARR added</summary>
    RecurringRevenue = 7,

    /// <summary>Calls made</summary>
    CallsMade = 8,

    /// <summary>Custom metric</summary>
    Custom = 9
}

/// <summary>
/// FUNCTIONAL: Forecast category for opportunities.
/// TECHNICAL: Used for pipeline analysis.
/// </summary>
public enum ForecastCategory
{
    /// <summary>In pipeline, uncertain</summary>
    Pipeline = 0,

    /// <summary>Best case - optimistic forecast</summary>
    BestCase = 1,

    /// <summary>Commit - high confidence forecast</summary>
    Commit = 2,

    /// <summary>Closed won - already won</summary>
    ClosedWon = 3,

    /// <summary>Omitted - excluded from forecast</summary>
    Omitted = 4,

    /// <summary>Most likely - expected outcome</summary>
    MostLikely = 5
}

#endregion

/// <summary>
/// Sales quota definition and tracking.
/// </summary>
public class SalesQuota : BaseEntity
{
    #region Identification

    /// <summary>Quota name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Quota period type</summary>
    public QuotaPeriodType PeriodType { get; set; } = QuotaPeriodType.Quarterly;

    /// <summary>What the quota measures</summary>
    public QuotaMetric Metric { get; set; } = QuotaMetric.Revenue;

    /// <summary>Period identifier (e.g., "2024-Q1")</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Fiscal year</summary>
    public int FiscalYear { get; set; }

    /// <summary>Fiscal quarter (1-4) if applicable</summary>
    public int? FiscalQuarter { get; set; }

    /// <summary>Fiscal month (1-12) if applicable</summary>
    public int? FiscalMonth { get; set; }

    #endregion

    #region Period Dates

    /// <summary>Period start date</summary>
    public DateTime PeriodStartDate { get; set; }

    /// <summary>Period end date</summary>
    public DateTime PeriodEndDate { get; set; }

    #endregion

    #region Target

    /// <summary>Quota target amount</summary>
    public decimal TargetAmount { get; set; } = 0;

    /// <summary>Currency code</summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>Stretch/accelerator target</summary>
    public decimal? StretchTargetAmount { get; set; }

    /// <summary>Minimum acceptable performance</summary>
    public decimal? MinimumTargetAmount { get; set; }

    #endregion

    #region Attainment

    /// <summary>Actual achieved amount</summary>
    public decimal ActualAmount { get; set; } = 0;

    /// <summary>Attainment percentage</summary>
    public decimal AttainmentPercent => TargetAmount > 0 ? (ActualAmount / TargetAmount * 100) : 0;

    /// <summary>Variance from target</summary>
    public decimal Variance => ActualAmount - TargetAmount;

    /// <summary>Amount needed to reach target</summary>
    public decimal GapToTarget => Math.Max(0, TargetAmount - ActualAmount);

    /// <summary>Is quota achieved</summary>
    public bool IsAchieved => ActualAmount >= TargetAmount;

    #endregion

    #region Breakdown

    /// <summary>New business amount</summary>
    public decimal? NewBusinessAmount { get; set; }

    /// <summary>Renewal amount</summary>
    public decimal? RenewalAmount { get; set; }

    /// <summary>Upsell/expansion amount</summary>
    public decimal? ExpansionAmount { get; set; }

    #endregion

    #region Relationships

    /// <summary>User ID (if individual quota)</summary>
    public int? UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Team ID (if team quota)</summary>
    public int? TeamId { get; set; }

    /// <summary>Navigation to team</summary>
    public Team? Team { get; set; }

    /// <summary>Parent quota ID (for rollup)</summary>
    public int? ParentQuotaId { get; set; }

    /// <summary>Navigation to parent quota</summary>
    public SalesQuota? ParentQuota { get; set; }

    /// <summary>Child quotas</summary>
    public ICollection<SalesQuota> ChildQuotas { get; set; } = new List<SalesQuota>();

    #endregion

    #region Audit

    /// <summary>Last refreshed date</summary>
    public DateTime? LastRefreshedAt { get; set; }

    /// <summary>Notes</summary>
    public string? Notes { get; set; }

    #endregion
}

/// <summary>
/// Sales forecast for pipeline analysis.
/// </summary>
public class SalesForecast : BaseEntity
{
    #region Identification

    /// <summary>Forecast name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Forecast period</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Period start date</summary>
    public DateTime PeriodStartDate { get; set; }

    /// <summary>Period end date</summary>
    public DateTime PeriodEndDate { get; set; }

    /// <summary>Fiscal year</summary>
    public int FiscalYear { get; set; }

    /// <summary>Fiscal quarter</summary>
    public int? FiscalQuarter { get; set; }

    /// <summary>Fiscal month</summary>
    public int? FiscalMonth { get; set; }

    #endregion

    #region Quota Context

    /// <summary>Quota amount for this period</summary>
    public decimal QuotaAmount { get; set; } = 0;

    /// <summary>Currency code</summary>
    public string CurrencyCode { get; set; } = "USD";

    #endregion

    #region Forecast Categories

    /// <summary>Already closed won</summary>
    public decimal ClosedWonAmount { get; set; } = 0;

    /// <summary>Commit forecast (high confidence)</summary>
    public decimal CommitAmount { get; set; } = 0;

    /// <summary>Best case forecast (optimistic)</summary>
    public decimal BestCaseAmount { get; set; } = 0;

    /// <summary>Pipeline amount (all open)</summary>
    public decimal PipelineAmount { get; set; } = 0;

    /// <summary>Omitted from forecast</summary>
    public decimal OmittedAmount { get; set; } = 0;

    #endregion

    #region Calculated Fields

    /// <summary>Total forecast (closed + commit)</summary>
    public decimal ForecastAmount => ClosedWonAmount + CommitAmount;

    /// <summary>Gap to quota</summary>
    public decimal GapToQuota => Math.Max(0, QuotaAmount - ForecastAmount);

    /// <summary>Coverage ratio (pipeline / gap)</summary>
    public decimal CoverageRatio => GapToQuota > 0 ? (PipelineAmount / GapToQuota) : 0;

    /// <summary>Forecast attainment %</summary>
    public decimal ForecastAttainmentPercent => QuotaAmount > 0 ? (ForecastAmount / QuotaAmount * 100) : 0;

    #endregion

    #region Deal Counts

    /// <summary>Closed won deal count</summary>
    public int ClosedWonCount { get; set; } = 0;

    /// <summary>Commit deal count</summary>
    public int CommitCount { get; set; } = 0;

    /// <summary>Best case deal count</summary>
    public int BestCaseCount { get; set; } = 0;

    /// <summary>Pipeline deal count</summary>
    public int PipelineCount { get; set; } = 0;

    #endregion

    #region Manager Adjustments

    /// <summary>Manager-adjusted commit amount</summary>
    public decimal? AdjustedCommitAmount { get; set; }

    /// <summary>Manager-adjusted best case</summary>
    public decimal? AdjustedBestCaseAmount { get; set; }

    /// <summary>Adjustment notes</summary>
    public string? AdjustmentNotes { get; set; }

    /// <summary>Adjusted by user ID</summary>
    public int? AdjustedById { get; set; }

    /// <summary>Adjustment date</summary>
    public DateTime? AdjustedAt { get; set; }

    #endregion

    #region Snapshot

    /// <summary>Snapshot date</summary>
    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

    /// <summary>Whether this is a submitted forecast</summary>
    public bool IsSubmitted { get; set; } = false;

    /// <summary>Submitted date</summary>
    public DateTime? SubmittedAt { get; set; }

    #endregion

    #region Relationships

    /// <summary>User ID</summary>
    public int? UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Team ID (for team rollup)</summary>
    public int? TeamId { get; set; }

    /// <summary>Navigation to team</summary>
    public Team? Team { get; set; }

    /// <summary>Related quota ID</summary>
    public int? SalesQuotaId { get; set; }

    /// <summary>Navigation to quota</summary>
    public SalesQuota? SalesQuota { get; set; }

    /// <summary>Parent forecast (for rollup)</summary>
    public int? ParentForecastId { get; set; }

    /// <summary>Navigation to parent forecast</summary>
    public SalesForecast? ParentForecast { get; set; }

    /// <summary>Child forecasts</summary>
    public ICollection<SalesForecast> ChildForecasts { get; set; } = new List<SalesForecast>();

    /// <summary>Forecast line items</summary>
    public ICollection<ForecastLineItem> LineItems { get; set; } = new List<ForecastLineItem>();

    #endregion
}

/// <summary>
/// Individual opportunity in a forecast.
/// </summary>
public class ForecastLineItem : BaseEntity
{
    /// <summary>Forecast category</summary>
    public ForecastCategory Category { get; set; }

    /// <summary>Amount</summary>
    public decimal Amount { get; set; } = 0;

    /// <summary>Close date</summary>
    public DateTime CloseDate { get; set; }

    /// <summary>Stage</summary>
    public string? Stage { get; set; }

    /// <summary>Probability</summary>
    public int Probability { get; set; }

    /// <summary>Manager override category</summary>
    public ForecastCategory? OverrideCategory { get; set; }

    /// <summary>Manager override amount</summary>
    public decimal? OverrideAmount { get; set; }

    /// <summary>Override notes</summary>
    public string? OverrideNotes { get; set; }

    /// <summary>Parent forecast ID</summary>
    public int SalesForecastId { get; set; }

    /// <summary>Navigation to forecast</summary>
    public SalesForecast? SalesForecast { get; set; }

    /// <summary>Opportunity ID</summary>
    public int OpportunityId { get; set; }

    /// <summary>Navigation to opportunity</summary>
    public Opportunity? Opportunity { get; set; }
}

/// <summary>
/// Historical forecast snapshot for trend analysis.
/// </summary>
public class ForecastHistory : BaseEntity
{
    /// <summary>Snapshot date</summary>
    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

    /// <summary>Forecast period</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>User ID</summary>
    public int? UserId { get; set; }

    /// <summary>Team ID</summary>
    public int? TeamId { get; set; }

    /// <summary>Quota amount</summary>
    public decimal QuotaAmount { get; set; }

    /// <summary>Closed won at snapshot</summary>
    public decimal ClosedWonAmount { get; set; }

    /// <summary>Commit at snapshot</summary>
    public decimal CommitAmount { get; set; }

    /// <summary>Best case at snapshot</summary>
    public decimal BestCaseAmount { get; set; }

    /// <summary>Pipeline at snapshot</summary>
    public decimal PipelineAmount { get; set; }

    /// <summary>Weeks remaining in period</summary>
    public int WeeksRemaining { get; set; }
}
