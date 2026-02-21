// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for marketing campaign response.
/// </summary>
public class CampaignDto
{
    // ── Identity ─────────────────────────────────────────────────────────────
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CampaignCode { get; set; }
    public string? Description { get; set; }

    // ── Enums (int) ───────────────────────────────────────────────────────────
    public int Objective { get; set; }
    public int CampaignType { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }

    // ── Basic Info ────────────────────────────────────────────────────────────
    public string? ObjectiveText { get; set; }
    public string? Type { get; set; }
    public int PrimarySuccessMetric { get; set; }
    public string? Theme { get; set; }
    public string? ValueProposition { get; set; }

    // ── Timeline ──────────────────────────────────────────────────────────────
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? ActualStartDate { get; set; }
    public string? ActualEndDate { get; set; }
    public int? DurationDays { get; set; }
    public bool IsEvergreen { get; set; }
    public string? Timezone { get; set; }
    public string? Schedule { get; set; }

    // ── Budget & Financials ───────────────────────────────────────────────────
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal? DailyBudget { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal ActualRevenue { get; set; }
    public decimal? PipelineInfluenced { get; set; }
    public decimal? PipelineCreated { get; set; }
    public decimal? CostPerLead { get; set; }
    public decimal? CostPerMql { get; set; }
    public decimal? CostPerSql { get; set; }
    public decimal? CostPerOpportunity { get; set; }
    public decimal? CostPerAcquisition { get; set; }
    public string? CurrencyCode { get; set; }

    // ── Legacy computed aliases (preserved for backward compatibility) ─────────
    [JsonIgnore]
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }

    // ── Target Audience ───────────────────────────────────────────────────────
    public int TargetAudience { get; set; }
    public string? TargetAudienceDescription { get; set; }
    public int AudienceType { get; set; }
    public string? TargetDemographics { get; set; }
    public string? TargetFirmographics { get; set; }
    public string? TargetGeography { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetSegments { get; set; }
    public string? TargetPersonas { get; set; }
    public string? TargetJobTitles { get; set; }
    public string? TargetSeniorityLevels { get; set; }
    public string? TargetAccounts { get; set; }
    public string? ExclusionCriteria { get; set; }
    public string? SuppressionLists { get; set; }
    public string? AudienceListId { get; set; }

    // ── Lead Generation Metrics ───────────────────────────────────────────────
    public int LeadsGenerated { get; set; }
    public int MqlsGenerated { get; set; }
    public int SqlsGenerated { get; set; }
    public int SalsGenerated { get; set; }
    public int OpportunitiesCreated { get; set; }
    public int OpportunitiesInfluenced { get; set; }
    public int DealsWon { get; set; }
    public int AccountsAcquired { get; set; }
    public double LeadToMqlRate { get; set; }
    public double MqlToSqlRate { get; set; }
    public double SqlToOpportunityRate { get; set; }
    public double OpportunityToWinRate { get; set; }
    public double ConversionRate { get; set; }
    public double AverageLeadScore { get; set; }
    public string? LeadQualityDistribution { get; set; }

    // ── Reach & Engagement ────────────────────────────────────────────────────
    public long Impressions { get; set; }
    public long Reach { get; set; }
    public double Frequency { get; set; }
    public int Clicks { get; set; }
    public double ClickThroughRate { get; set; }
    public int LandingPageVisits { get; set; }
    public int FormSubmissions { get; set; }
    public double FormConversionRate { get; set; }
    public int ContentDownloads { get; set; }
    public int VideoViews { get; set; }
    public double VideoCompletionRate { get; set; }
    public int DemoRequests { get; set; }
    public int TrialSignups { get; set; }

    // ── Email Campaign Metrics ────────────────────────────────────────────────
    public int EmailsSent { get; set; }
    public int EmailsDelivered { get; set; }
    public double DeliveryRate { get; set; }
    public int EmailsOpened { get; set; }
    public double OpenRate { get; set; }
    public int EmailClicks { get; set; }
    public double EmailClickRate { get; set; }
    public double ClickToOpenRate { get; set; }
    public int HardBounces { get; set; }
    public int SoftBounces { get; set; }
    public int Bounces { get; set; }
    public double BounceRate { get; set; }
    public int Unsubscribes { get; set; }
    public double UnsubscribeRate { get; set; }
    public int SpamComplaints { get; set; }
    public double ComplaintRate { get; set; }
    public int EmailForwards { get; set; }
    public int ListGrowth { get; set; }

    // ── Social Media Metrics ──────────────────────────────────────────────────
    public long SocialReach { get; set; }
    public int SocialEngagement { get; set; }
    public double SocialEngagementRate { get; set; }
    public int SocialShares { get; set; }
    public int SocialComments { get; set; }
    public int SocialLikes { get; set; }
    public int SocialSaves { get; set; }
    public int NewFollowers { get; set; }
    public int ProfileVisits { get; set; }
    public int Mentions { get; set; }
    public int? SentimentScore { get; set; }

    // ── Paid Advertising Metrics ──────────────────────────────────────────────
    public decimal? AdSpend { get; set; }
    public decimal? CostPerClick { get; set; }
    public decimal? CostPerMille { get; set; }
    public double? Roas { get; set; }
    public double? QualityScore { get; set; }
    public double? AveragePosition { get; set; }
    public double? ImpressionShare { get; set; }
    public string? Keywords { get; set; }
    public string? NegativeKeywords { get; set; }

    // ── Event / Webinar Metrics ───────────────────────────────────────────────
    public int Registrations { get; set; }
    public int Attendance { get; set; }
    public double AttendanceRate { get; set; }
    public int NoShows { get; set; }
    public int? EventCapacity { get; set; }
    public string? EventLocation { get; set; }
    public string? EventDateTime { get; set; }
    public string? WebinarPlatform { get; set; }
    public string? WebinarRecordingUrl { get; set; }
    public int OnDemandViews { get; set; }
    public int PollResponses { get; set; }
    public int QuestionsAsked { get; set; }
    public double? EventSatisfactionScore { get; set; }

    // ── ROI & Performance ─────────────────────────────────────────────────────
    // ROI value is calculated server-side and is returned via analytics endpoints; omit
    // from the primary CampaignDto to avoid JSON naming collisions with TargetRoi.
    [JsonIgnore]
    public double ROI { get; set; }
    public double? TargetRoi { get; set; }
    public int? TargetLeads { get; set; }
    public int? TargetConversions { get; set; }
    public double? GoalAchievementPercent { get; set; }
    public int? CampaignHealthScore { get; set; }
    public string? BenchmarkComparison { get; set; }

    // ── Content & Creative ────────────────────────────────────────────────────
    public string? MessageSubject { get; set; }
    public string? PreheaderText { get; set; }
    public string? MessageBody { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? CallToAction { get; set; }
    public string? CtaUrl { get; set; }
    public string? LandingPageUrl { get; set; }
    public string? TrackingUrl { get; set; }
    public string? CreativeAssets { get; set; }
    public string? TemplateId { get; set; }

    // ── UTM Tracking ──────────────────────────────────────────────────────────
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }

    // ── A/B Testing ───────────────────────────────────────────────────────────
    public bool IsABTest { get; set; }
    public string? ABTestVariants { get; set; }
    public string? ABTestMetric { get; set; }
    public string? WinningVariant { get; set; }
    public double? StatisticalSignificance { get; set; }
    public string? ABTestResults { get; set; }

    // ── Channels & Platforms ──────────────────────────────────────────────────
    public string? Channels { get; set; }
    public string? Platforms { get; set; }
    public string? SocialNetworks { get; set; }
    public string? AdPlatforms { get; set; }
    public string? ExternalCampaignIds { get; set; }

    // ── Assignment & Ownership ────────────────────────────────────────────────
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? TeamMembers { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedDate { get; set; }
    public string? Department { get; set; }
    public string? CostCenter { get; set; }

    // ── Campaign Hierarchy ────────────────────────────────────────────────────
    public int? ParentCampaignId { get; set; }
    public string? RelatedCampaigns { get; set; }
    public string? Program { get; set; }
    public string? Initiative { get; set; }
    public string? FiscalQuarter { get; set; }
    public int? FiscalYear { get; set; }

    // ── Classification & Tags ─────────────────────────────────────────────────
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Region { get; set; }

    // ── Documentation & Notes ─────────────────────────────────────────────────
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? SuccessCriteria { get; set; }
    public string? LessonsLearned { get; set; }
    public string? Attachments { get; set; }
    public string? BriefUrl { get; set; }
    public string? ReportUrl { get; set; }

    // ── Custom Fields & Integration ───────────────────────────────────────────
    public string? CustomFields { get; set; }
    public string? ExternalId { get; set; }
    public string? SyncStatus { get; set; }
    public string? LastSyncDate { get; set; }

    // ── Calculated / Audit ────────────────────────────────────────────────────
    public bool IsActive { get; set; }
    public bool IsOverBudget { get; set; }
    public decimal BudgetRemaining { get; set; }
    public double BudgetUtilization { get; set; }
    public int? DaysRemaining { get; set; }
    public bool IsEndingSoon { get; set; }
    public double OverallEngagementRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating marketing campaign.
/// </summary>
public class CreateCampaignDto
{
    // ── Required ──────────────────────────────────────────────────────────────
    [Required(ErrorMessage = "Campaign name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    // ── Basic Info ────────────────────────────────────────────────────────────
    [StringLength(50)]
    public string? CampaignCode { get; set; }

    [StringLength(10000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Objective { get; set; }

    [Range(0, 20)]
    public int? ObjectiveType { get; set; }

    [Required(ErrorMessage = "Campaign type is required")]
    [Range(0, 20)]
    public int CampaignType { get; set; }

    [Range(0, 10)]
    public int Priority { get; set; } = 1;

    [Range(0, 10)]
    public int? Status { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [Range(0, 20)]
    public int? PrimarySuccessMetric { get; set; }

    [StringLength(500)]
    public string? Theme { get; set; }

    [StringLength(2000)]
    public string? ValueProposition { get; set; }

    // ── Timeline ──────────────────────────────────────────────────────────────
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? ActualStartDate { get; set; }
    public string? ActualEndDate { get; set; }
    public int? DurationDays { get; set; }
    public bool IsEvergreen { get; set; } = false;

    [StringLength(50)]
    public string? Timezone { get; set; }

    [StringLength(5000)]
    public string? Schedule { get; set; }

    // ── Budget & Financials ───────────────────────────────────────────────────
    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ActualCost { get; set; }

    public decimal? DailyBudget { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal? ActualRevenue { get; set; }

    [StringLength(3)]
    public string? CurrencyCode { get; set; }

    // ── Target Audience ───────────────────────────────────────────────────────
    [Range(0, int.MaxValue)]
    public int TargetAudience { get; set; }

    [StringLength(5000)]
    public string? TargetAudienceDescription { get; set; }

    [Range(0, 10)]
    public int? AudienceType { get; set; }

    public string? TargetDemographics { get; set; }
    public string? TargetFirmographics { get; set; }
    public string? TargetGeography { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetSegments { get; set; }
    public string? TargetPersonas { get; set; }
    public string? TargetJobTitles { get; set; }
    public string? TargetSeniorityLevels { get; set; }
    public string? TargetAccounts { get; set; }
    public string? ExclusionCriteria { get; set; }
    public string? SuppressionLists { get; set; }
    public string? AudienceListId { get; set; }

    // ── Content & Creative ────────────────────────────────────────────────────
    public string? MessageSubject { get; set; }
    public string? PreheaderText { get; set; }
    public string? MessageBody { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? CallToAction { get; set; }
    public string? CtaUrl { get; set; }
    public string? LandingPageUrl { get; set; }
    public string? TrackingUrl { get; set; }
    public string? CreativeAssets { get; set; }
    public string? TemplateId { get; set; }

    // ── UTM Tracking ──────────────────────────────────────────────────────────
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }

    // ── A/B Testing ───────────────────────────────────────────────────────────
    public bool IsABTest { get; set; } = false;
    public string? ABTestVariants { get; set; }
    public string? ABTestMetric { get; set; }

    // ── Channels & Platforms ──────────────────────────────────────────────────
    public string? Channels { get; set; }
    public string? Platforms { get; set; }
    public string? SocialNetworks { get; set; }
    public string? AdPlatforms { get; set; }
    public string? ExternalCampaignIds { get; set; }

    // ── Assignment & Ownership ────────────────────────────────────────────────
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? TeamMembers { get; set; }
    public string? Department { get; set; }
    public string? CostCenter { get; set; }

    // ── Campaign Hierarchy ────────────────────────────────────────────────────
    public int? ParentCampaignId { get; set; }
    public string? RelatedCampaigns { get; set; }
    public string? Program { get; set; }
    public string? Initiative { get; set; }
    public string? FiscalQuarter { get; set; }
    public int? FiscalYear { get; set; }

    // ── Classification & Tags ─────────────────────────────────────────────────
    [StringLength(500)]
    public string? Tags { get; set; }

    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Region { get; set; }

    // ── ROI & Performance Targets ─────────────────────────────────────────────
    public double? TargetRoi { get; set; }
    public int? TargetLeads { get; set; }
    public int? TargetConversions { get; set; }

    // ── Documentation & Notes ─────────────────────────────────────────────────
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? SuccessCriteria { get; set; }
    public string? BriefUrl { get; set; }

    // ── Custom Fields & Integration ───────────────────────────────────────────
    public string? CustomFields { get; set; }
    public string? ExternalId { get; set; }

    // ── Event / Webinar ───────────────────────────────────────────────────────
    public int? EventCapacity { get; set; }
    public string? EventLocation { get; set; }
    public string? EventDateTime { get; set; }
    public string? WebinarPlatform { get; set; }
    public string? WebinarRecordingUrl { get; set; }

    // ── Paid Advertising ──────────────────────────────────────────────────────
    public string? Keywords { get; set; }
    public string? NegativeKeywords { get; set; }

    // ── Legacy field (preserved for backward compatibility) ───────────────────
    [StringLength(500)]
    public string? SegmentCriteria { get; set; }
}

/// <summary>
/// DTO for updating marketing campaign.
/// </summary>
public class UpdateCampaignDto
{
    // ── Basic Info ────────────────────────────────────────────────────────────
    [StringLength(255, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? CampaignCode { get; set; }

    [StringLength(10000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Objective { get; set; }

    [Range(0, 20)]
    public int? ObjectiveType { get; set; }

    [Range(0, 20)]
    public int? CampaignType { get; set; }

    [Range(0, 10)]
    public int? Priority { get; set; }

    [Range(0, int.MaxValue)]
    public int? Status { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [Range(0, 20)]
    public int? PrimarySuccessMetric { get; set; }

    [StringLength(500)]
    public string? Theme { get; set; }

    [StringLength(2000)]
    public string? ValueProposition { get; set; }

    // ── Timeline ──────────────────────────────────────────────────────────────
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? ActualStartDate { get; set; }
    public string? ActualEndDate { get; set; }
    public int? DurationDays { get; set; }
    public bool? IsEvergreen { get; set; }

    [StringLength(50)]
    public string? Timezone { get; set; }

    [StringLength(5000)]
    public string? Schedule { get; set; }

    // ── Budget & Financials ───────────────────────────────────────────────────
    [Range(0, double.MaxValue)]
    public decimal? Budget { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ActualCost { get; set; }

    public decimal? DailyBudget { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal? ActualRevenue { get; set; }

    [StringLength(3)]
    public string? CurrencyCode { get; set; }

    // ── Target Audience ───────────────────────────────────────────────────────
    [Range(0, int.MaxValue)]
    public int? TargetAudience { get; set; }

    [StringLength(5000)]
    public string? TargetAudienceDescription { get; set; }

    [Range(0, 10)]
    public int? AudienceType { get; set; }

    public string? TargetDemographics { get; set; }
    public string? TargetFirmographics { get; set; }
    public string? TargetGeography { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetSegments { get; set; }
    public string? TargetPersonas { get; set; }
    public string? TargetJobTitles { get; set; }
    public string? TargetSeniorityLevels { get; set; }
    public string? TargetAccounts { get; set; }
    public string? ExclusionCriteria { get; set; }
    public string? SuppressionLists { get; set; }
    public string? AudienceListId { get; set; }

    // ── Content & Creative ────────────────────────────────────────────────────
    public string? MessageSubject { get; set; }
    public string? PreheaderText { get; set; }
    public string? MessageBody { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? CallToAction { get; set; }
    public string? CtaUrl { get; set; }
    public string? LandingPageUrl { get; set; }
    public string? TrackingUrl { get; set; }
    public string? CreativeAssets { get; set; }
    public string? TemplateId { get; set; }

    // ── UTM Tracking ──────────────────────────────────────────────────────────
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }

    // ── A/B Testing ───────────────────────────────────────────────────────────
    public bool? IsABTest { get; set; }
    public string? ABTestVariants { get; set; }
    public string? ABTestMetric { get; set; }
    public string? WinningVariant { get; set; }

    // ── Channels & Platforms ──────────────────────────────────────────────────
    public string? Channels { get; set; }
    public string? Platforms { get; set; }
    public string? SocialNetworks { get; set; }
    public string? AdPlatforms { get; set; }
    public string? ExternalCampaignIds { get; set; }

    // ── Assignment & Ownership ────────────────────────────────────────────────
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? TeamMembers { get; set; }
    public string? Department { get; set; }
    public string? CostCenter { get; set; }

    // ── Campaign Hierarchy ────────────────────────────────────────────────────
    public int? ParentCampaignId { get; set; }
    public string? RelatedCampaigns { get; set; }
    public string? Program { get; set; }
    public string? Initiative { get; set; }
    public string? FiscalQuarter { get; set; }
    public int? FiscalYear { get; set; }

    // ── Classification & Tags ─────────────────────────────────────────────────
    [StringLength(500)]
    public string? Tags { get; set; }

    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Region { get; set; }

    // ── ROI & Performance Targets ─────────────────────────────────────────────
    public double? TargetRoi { get; set; }
    public int? TargetLeads { get; set; }
    public int? TargetConversions { get; set; }

    // ── Documentation & Notes ─────────────────────────────────────────────────
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? SuccessCriteria { get; set; }
    public string? LessonsLearned { get; set; }
    public string? Attachments { get; set; }
    public string? BriefUrl { get; set; }
    public string? ReportUrl { get; set; }

    // ── Custom Fields & Integration ───────────────────────────────────────────
    public string? CustomFields { get; set; }
    public string? ExternalId { get; set; }
    public string? SyncStatus { get; set; }

    // ── Event / Webinar ───────────────────────────────────────────────────────
    public int? EventCapacity { get; set; }
    public string? EventLocation { get; set; }
    public string? EventDateTime { get; set; }
    public string? WebinarPlatform { get; set; }
    public string? WebinarRecordingUrl { get; set; }

    // ── Paid Advertising ──────────────────────────────────────────────────────
    public string? Keywords { get; set; }
    public string? NegativeKeywords { get; set; }

    // ── Metrics that can be manually set ─────────────────────────────────────
    public int? LeadsGenerated { get; set; }
    public int? MqlsGenerated { get; set; }
    public int? SqlsGenerated { get; set; }
    public int? SalsGenerated { get; set; }
    public int? OpportunitiesCreated { get; set; }
    public int? OpportunitiesInfluenced { get; set; }
    public int? DealsWon { get; set; }
    public int? AccountsAcquired { get; set; }
    public long? Impressions { get; set; }
    public long? Reach { get; set; }
    public int? Clicks { get; set; }
    public int? EmailsSent { get; set; }
    public int? EmailsDelivered { get; set; }
    public int? EmailsOpened { get; set; }
    public int? EmailClicks { get; set; }
    public int? HardBounces { get; set; }
    public int? SoftBounces { get; set; }
    public int? Bounces { get; set; }
    public int? Unsubscribes { get; set; }
    public int? SpamComplaints { get; set; }
    public int? Registrations { get; set; }
    public int? Attendance { get; set; }
    public int? NoShows { get; set; }
    public decimal? PipelineInfluenced { get; set; }
    public decimal? PipelineCreated { get; set; }
    public decimal? AdSpend { get; set; }
    public decimal? CostPerClick { get; set; }
    public decimal? CostPerMille { get; set; }
    public double? Roas { get; set; }
}

/// <summary>
/// DTO for campaign recipient.
/// </summary>
public class CampaignRecipientDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int? ContactId { get; set; }
    public int? LeadId { get; set; }
    public int? AccountId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Status { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? EngagedAt { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public decimal Money { get; set; }
}

/// <summary>
/// DTO for adding campaign recipients.
/// </summary>
public class AddCampaignRecipientsDto
{
    [Required]
    public List<int> RecipientIds { get; set; } = new();

    [StringLength(100)]
    public string? RecipientType { get; set; } = "Contact";
}

/// <summary>
/// DTO for campaign metrics.
/// </summary>
public class CampaignMetricsDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public int LeadsGenerated { get; set; }
    public int MqlsGenerated { get; set; }
    public int SqlsGenerated { get; set; }
    public decimal ReveneGenerated { get; set; }
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalBudget { get; set; }
    public int ActualSpend { get; set; }
    public int BudgetRemaining { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal BounceRate { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public int? TotalRecipients { get; set; }
    public int? SentCount { get; set; }
    public int? OpenCount { get; set; }
    public int? ClickCount { get; set; }
    public int? BounceCount { get; set; }
}

/// <summary>
/// DTO for campaign execution result.
/// </summary>
public class CampaignExecutionResultDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int RecipientsCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO for campaign preview.
/// </summary>
public class CampaignPreviewDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string? PreviewHtml { get; set; }
    public string? PreviewText { get; set; }
    public string? Subject { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public int EstimatedRecipients { get; set; }
}

/// <summary>
/// DTO for duplicating campaign.
/// </summary>
public class DuplicateCampaignDto
{
    [Required(ErrorMessage = "New campaign name is required")]
    [StringLength(255)]
    public string NewName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? NewCampaignCode { get; set; }

    public bool CopyRecipients { get; set; }
    public bool CopyMetrics { get; set; }
}

/// <summary>
/// DTO for cloning campaign.
/// </summary>
public class CloneCampaignDto
{
    [Required(ErrorMessage = "New campaign name is required")]
    [StringLength(255)]
    public string NewName { get; set; } = string.Empty;

    public bool IncludeRecipients { get; set; } = true;
    public bool ResetMetrics { get; set; } = true;
}

/// <summary>
/// DTO for scheduling campaign.
/// </summary>
public class ScheduleCampaignDto
{
    [Required]
    public DateTime ScheduledDate { get; set; }

    [Range(0, 23)]
    public int? ScheduledHour { get; set; }

    [Range(0, 59)]
    public int? ScheduledMinute { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for retargeting campaign.
/// </summary>
public class RetargetCampaignDto
{
    [Required]
    [StringLength(100)]
    public string Criteria { get; set; } = string.Empty;

    public List<int>? ExcludeRecipientIds { get; set; }
    public List<int>? IncludeRecipientIds { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for analyzing campaign.
/// </summary>
public class CampaignAnalysisDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
    public double ClickThroughRate { get; set; }
    public double ConversionRate { get; set; }
    public double DateQualificationRate { get; set; }
    public double SalesQualificationRate { get; set; }
    public int TopPerformingSegment { get; set; }
    public string? Insights { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// DTO for campaign list response with pagination.
/// </summary>
public class CampaignListDto
{
    public List<CampaignDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// DTO for campaign analysis results.
/// </summary>
public class CampaignAnalysisResultDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
    public int LeadsGenerated { get; set; }
    public int MqlsGenerated { get; set; }
    public int SqlsGenerated { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public string? Insights { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// DTO for campaign metrics preview.
/// </summary>
public class CampaignMetricsPreviewDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal Roi { get; set; }
    public int BudgetRemaining { get; set; }
}

/// <summary>
/// DTO for duplicating a campaign.
/// </summary>
public class CampaignDuplicationDto
{
    public int SourceCampaignId { get; set; }
    public int TargetCampaignId { get; set; }
    public bool CopyRecipients { get; set; } = false;
    public bool CopyMetrics { get; set; } = false;
    public bool ResetDates { get; set; } = true;
}

/// <summary>
/// DTO for campaign retargeting.
/// </summary>
public class CampaignRetargetingDto
{
    public int CampaignId { get; set; }

    [StringLength(500)]
    public string? Criteria { get; set; }

    public List<int>? ExcludeRecipientIds { get; set; }
}

/// <summary>
/// Result of campaign retargeting.
/// </summary>
public class CampaignRetargetingResultDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int NewRecipientsCount { get; set; }
    public int ProcessedCount { get; set; }
    public int SkippedCount { get; set; }
    public DateTime RetargetedAt { get; set; }
}
