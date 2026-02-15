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

using System.ComponentModel.DataAnnotations.Schema;
namespace CRM.Core.Entities.AI;

#region Churn Risk Enumerations

/// <summary>
/// Churn risk level.
/// </summary>
public enum ChurnRiskLevel
{
    /// <summary>Very low churn risk (0-20%)</summary>
    VeryLow = 0,

    /// <summary>Low churn risk (20-40%)</summary>
    Low = 1,

    /// <summary>Medium churn risk (40-60%)</summary>
    Medium = 2,

    /// <summary>High churn risk (60-80%)</summary>
    High = 3,

    /// <summary>Very high churn risk (80-100%)</summary>
    Critical = 4
}

/// <summary>
/// Primary churn driver category.
/// </summary>
public enum ChurnDriverCategory
{
    /// <summary>Product quality or fit issues</summary>
    ProductQuality = 0,

    /// <summary>Price or value perception</summary>
    PriceValue = 1,

    /// <summary>Support or service issues</summary>
    ServiceQuality = 2,

    /// <summary>Competitive displacement</summary>
    Competition = 3,

    /// <summary>Lack of engagement or adoption</summary>
    Engagement = 4,

    /// <summary>Business change at customer</summary>
    BusinessChange = 5,

    /// <summary>Contract or billing issues</summary>
    ContractIssues = 6,

    /// <summary>Feature gaps</summary>
    FeatureGaps = 7,

    /// <summary>Relationship deterioration</summary>
    Relationship = 8
}

/// <summary>
/// Customer health segment.
/// </summary>
public enum CustomerHealthSegment
{
    /// <summary>Champion - highly engaged advocate</summary>
    Champion = 0,

    /// <summary>Healthy - consistent usage and satisfaction</summary>
    Healthy = 1,

    /// <summary>Passive - using but not engaged</summary>
    Passive = 2,

    /// <summary>At Risk - declining engagement</summary>
    AtRisk = 3,

    /// <summary>Critical - immediate churn risk</summary>
    Critical = 4
}

/// <summary>
/// Retention action type.
/// </summary>
public enum RetentionActionType
{
    /// <summary>Executive business review</summary>
    ExecutiveReview = 0,

    /// <summary>Product training session</summary>
    ProductTraining = 1,

    /// <summary>Custom development or feature fast-track</summary>
    CustomDevelopment = 2,

    /// <summary>Pricing negotiation</summary>
    PricingNegotiation = 3,

    /// <summary>Dedicated support</summary>
    DedicatedSupport = 4,

    /// <summary>Success plan development</summary>
    SuccessPlan = 5,

    /// <summary>Escalate to leadership</summary>
    Escalate = 6,

    /// <summary>Renewal incentive offer</summary>
    RenewalIncentive = 7,

    /// <summary>Multi-year deal</summary>
    MultiYearDeal = 8
}

#endregion

/// <summary>
/// AI-generated churn risk prediction for accounts.
/// Uses Allen AI models for prediction.
/// </summary>
public class ChurnRisk : BaseEntity
{
    #region Account Reference

    /// <summary>Account ID</summary>
    [Column("AccountId")]
    public int AccountId { get; set; }

    /// <summary>Navigation to Account</summary>
    public Account? Account { get; set; }

    #endregion

    #region Risk Assessment

    /// <summary>Churn probability (0-1)</summary>
    public decimal ChurnProbability { get; set; }

    /// <summary>Churn risk level</summary>
    public ChurnRiskLevel RiskLevel { get; set; }

    /// <summary>Confidence in prediction (0-1)</summary>
    public decimal Confidence { get; set; }

    /// <summary>Risk trend (positive = worsening)</summary>
    public decimal RiskTrend { get; set; }

    /// <summary>Previous churn probability</summary>
    public decimal? PreviousProbability { get; set; }

    #endregion

    #region Customer Health

    /// <summary>Customer health score (0-100)</summary>
    public decimal HealthScore { get; set; }

    /// <summary>Customer health segment</summary>
    public CustomerHealthSegment HealthSegment { get; set; }

    /// <summary>NPS score (-100 to 100)</summary>
    public int? NPSScore { get; set; }

    /// <summary>Customer satisfaction score (1-5)</summary>
    public decimal? CSATScore { get; set; }

    /// <summary>Customer effort score (1-5)</summary>
    public decimal? CESScore { get; set; }

    #endregion

    #region Engagement Metrics

    /// <summary>Product usage score (0-100)</summary>
    public decimal UsageScore { get; set; }

    /// <summary>Feature adoption percentage (0-100)</summary>
    public decimal FeatureAdoption { get; set; }

    /// <summary>Days since last login</summary>
    public int DaysSinceLastLogin { get; set; }

    /// <summary>Daily active users (for business accounts)</summary>
    public int? DailyActiveUsers { get; set; }

    /// <summary>Login frequency (per month)</summary>
    public decimal? MonthlyLogins { get; set; }

    /// <summary>Usage trend (positive = increasing)</summary>
    public decimal UsageTrend { get; set; }

    #endregion

    #region Support Metrics

    /// <summary>Open support tickets count</summary>
    public int OpenTickets { get; set; }

    /// <summary>Critical tickets in last 90 days</summary>
    public int CriticalTickets90Days { get; set; }

    /// <summary>Average ticket resolution time (hours)</summary>
    public decimal? AvgResolutionTimeHours { get; set; }

    /// <summary>Support satisfaction score (1-5)</summary>
    public decimal? SupportSatisfaction { get; set; }

    /// <summary>Escalations in last 90 days</summary>
    public int Escalations90Days { get; set; }

    #endregion

    #region Financial Metrics

    /// <summary>Annual recurring revenue at risk</summary>
    public decimal ARRAtRisk { get; set; }

    /// <summary>Contract end date</summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>Days until renewal</summary>
    public int? DaysUntilRenewal { get; set; }

    /// <summary>Lifetime value</summary>
    public decimal? LifetimeValue { get; set; }

    /// <summary>Expansion potential</summary>
    public decimal? ExpansionPotential { get; set; }

    /// <summary>Account tenure (months)</summary>
    public int AccountTenureMonths { get; set; }

    #endregion

    #region Churn Drivers

    /// <summary>Primary churn driver</summary>
    public ChurnDriverCategory? PrimaryDriver { get; set; }

    /// <summary>All identified churn drivers (JSON array with weights)</summary>
    public string? ChurnDriversJson { get; set; }

    /// <summary>Key risk indicators (JSON array)</summary>
    public string? RiskIndicatorsJson { get; set; }

    /// <summary>Negative sentiment signals</summary>
    public string? NegativeSentimentJson { get; set; }

    #endregion

    #region Retention Strategy

    /// <summary>Recommended retention action</summary>
    public RetentionActionType? RecommendedAction { get; set; }

    /// <summary>Action urgency (1=immediate, 5=low)</summary>
    public int ActionUrgency { get; set; } = 3;

    /// <summary>All recommended actions (JSON array)</summary>
    public string? ActionRecommendationsJson { get; set; }

    /// <summary>Retention playbook to follow</summary>
    public string? RetentionPlaybook { get; set; }

    /// <summary>Estimated save probability if action taken</summary>
    public decimal? SaveProbability { get; set; }

    /// <summary>AI-generated retention insights</summary>
    public string? AIInsights { get; set; }

    #endregion

    #region Similar Customer Analysis

    /// <summary>Similar churned customers count</summary>
    public int? SimilarChurnedCount { get; set; }

    /// <summary>Similar retained customers count</summary>
    public int? SimilarRetainedCount { get; set; }

    /// <summary>Common churn patterns</summary>
    public string? ChurnPatternsJson { get; set; }

    /// <summary>Successful save patterns</summary>
    public string? SavePatternsJson { get; set; }

    #endregion

    #region Scoring Metadata

    /// <summary>When risk was assessed</summary>
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When assessment expires</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>Model version</summary>
    public string ModelVersion { get; set; } = "1.0";

    /// <summary>AI Model used</summary>
    public int? AIModelId { get; set; }

    /// <summary>Navigation to AI Model</summary>
    public AIModel? AIModel { get; set; }

    #endregion
}
