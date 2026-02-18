// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Attribution Enumerations

/// <summary>
/// FUNCTIONAL: Attribution model for credit distribution.
/// TECHNICAL: Determines how revenue credit is allocated across touchpoints.
/// </summary>
public enum CampaignAttributionModel
{
    /// <summary>100% credit to first touchpoint</summary>
    FirstTouch = 0,

    /// <summary>100% credit to last touchpoint</summary>
    LastTouch = 1,

    /// <summary>Equal credit across all touchpoints</summary>
    Linear = 2,

    /// <summary>More credit to recent touchpoints</summary>
    TimeDecay = 3,

    /// <summary>40% first, 40% last, 20% middle</summary>
    UShape = 4,

    /// <summary>Weighted based on channel performance</summary>
    DataDriven = 5,

    /// <summary>Custom weighting rules</summary>
    Custom = 6,

    /// <summary>Equal credit to first, lead creation, and last</summary>
    WShape = 7,

    /// <summary>Position-based (first and last get more)</summary>
    PositionBased = 8
}

/// <summary>
/// FUNCTIONAL: Type of marketing touchpoint.
/// TECHNICAL: Categorizes interaction for reporting.
/// </summary>
public enum TouchpointType
{
    /// <summary>First visit to website</summary>
    FirstWebVisit = 0,

    /// <summary>Marketing email opened</summary>
    EmailOpen = 1,

    /// <summary>Marketing email clicked</summary>
    EmailClick = 2,

    /// <summary>Form submission</summary>
    FormSubmission = 3,

    /// <summary>Content download</summary>
    ContentDownload = 4,

    /// <summary>Webinar registration</summary>
    WebinarRegistration = 5,

    /// <summary>Webinar attendance</summary>
    WebinarAttendance = 6,

    /// <summary>Event attendance</summary>
    EventAttendance = 7,

    /// <summary>Social media interaction</summary>
    Social = 8,

    /// <summary>Paid ad click</summary>
    PaidAdClick = 9,

    /// <summary>Organic search</summary>
    OrganicSearch = 10,

    /// <summary>Direct visit</summary>
    DirectVisit = 11,

    /// <summary>Referral</summary>
    Referral = 12,

    /// <summary>Demo request</summary>
    DemoRequest = 13,

    /// <summary>Trial signup</summary>
    TrialSignup = 14,

    /// <summary>Chat interaction</summary>
    Chat = 15,

    /// <summary>Phone call</summary>
    Phone = 16,

    /// <summary>Meeting</summary>
    Meeting = 17,

    /// <summary>Lead creation (MQL)</summary>
    LeadCreation = 18,

    /// <summary>Opportunity creation</summary>
    OpportunityCreation = 19,

    /// <summary>Quote sent</summary>
    QuoteSent = 20,

    /// <summary>Deal closed</summary>
    DealClosed = 21
}

/// <summary>
/// FUNCTIONAL: Channel category for touchpoints.
/// TECHNICAL: Groups touchpoints for channel analysis.
/// </summary>
public enum MarketingChannel
{
    /// <summary>Organic search traffic</summary>
    OrganicSearch = 0,

    /// <summary>Paid search (SEM/PPC)</summary>
    PaidSearch = 1,

    /// <summary>Social media organic</summary>
    SocialOrganic = 2,

    /// <summary>Social media paid</summary>
    SocialPaid = 3,

    /// <summary>Email marketing</summary>
    Email = 4,

    /// <summary>Display advertising</summary>
    Display = 5,

    /// <summary>Affiliate/partner</summary>
    Affiliate = 6,

    /// <summary>Referral</summary>
    Referral = 7,

    /// <summary>Direct traffic</summary>
    Direct = 8,

    /// <summary>Content marketing</summary>
    Content = 9,

    /// <summary>Events/trade shows</summary>
    Events = 10,

    /// <summary>Webinars</summary>
    Webinars = 11,

    /// <summary>Outbound sales</summary>
    Outbound = 12,

    /// <summary>Other</summary>
    Other = 13
}

#endregion

/// <summary>
/// Campaign attribution configuration and settings.
/// </summary>
public class AttributionSetting : BaseEntity
{
    /// <summary>Default attribution model for revenue</summary>
    public CampaignAttributionModel DefaultRevenueModel { get; set; } = CampaignAttributionModel.UShape;

    /// <summary>Default attribution model for conversions</summary>
    public CampaignAttributionModel DefaultConversionModel { get; set; } = CampaignAttributionModel.LastTouch;

    /// <summary>Attribution window in days</summary>
    public int AttributionWindowDays { get; set; } = 90;

    /// <summary>Time decay half-life in days</summary>
    public int TimeDecayHalfLifeDays { get; set; } = 7;

    /// <summary>Include anonymous touchpoints</summary>
    public bool IncludeAnonymousTouchpoints { get; set; } = true;

    /// <summary>Custom model weights (JSON)</summary>
    public string? CustomModelWeights { get; set; }

    /// <summary>Channel grouping rules (JSON)</summary>
    public string? ChannelGroupingRules { get; set; }
}

/// <summary>
/// Individual marketing touchpoint in customer journey.
/// </summary>
public class CampaignTouchpoint : BaseEntity
{
    #region Identification

    /// <summary>Unique touchpoint identifier</summary>
    public string TouchpointId { get; set; } = string.Empty;

    /// <summary>Touchpoint type</summary>
    public TouchpointType TouchpointType { get; set; }

    /// <summary>Marketing channel</summary>
    public MarketingChannel Channel { get; set; }

    /// <summary>Touchpoint timestamp</summary>
    public DateTime TouchpointDate { get; set; } = DateTime.UtcNow;

    #endregion

    #region Source Details

    /// <summary>Source (e.g., google, facebook)</summary>
    public string? Source { get; set; }

    /// <summary>Medium (e.g., cpc, email)</summary>
    public string? Medium { get; set; }

    /// <summary>Campaign name</summary>
    public string? CampaignName { get; set; }

    /// <summary>Content identifier</summary>
    public string? Content { get; set; }

    /// <summary>Keyword/term</summary>
    public string? Term { get; set; }

    /// <summary>Landing page URL</summary>
    public string? LandingPageUrl { get; set; }

    /// <summary>Referrer URL</summary>
    public string? ReferrerUrl { get; set; }

    #endregion

    #region Engagement Details

    /// <summary>Specific asset/content viewed</summary>
    public string? AssetName { get; set; }

    /// <summary>Asset type (ebook, webinar, etc.)</summary>
    public string? AssetType { get; set; }

    /// <summary>Form submitted</summary>
    public string? FormName { get; set; }

    /// <summary>Email campaign name</summary>
    public string? EmailCampaignName { get; set; }

    /// <summary>Ad creative ID</summary>
    public string? AdCreativeId { get; set; }

    /// <summary>Ad group/set name</summary>
    public string? AdGroupName { get; set; }

    #endregion

    #region Attribution Credits

    /// <summary>First-touch credit (0-1)</summary>
    public decimal FirstTouchCredit { get; set; } = 0;

    /// <summary>Last-touch credit (0-1)</summary>
    public decimal LastTouchCredit { get; set; } = 0;

    /// <summary>Linear credit (0-1)</summary>
    public decimal LinearCredit { get; set; } = 0;

    /// <summary>Time-decay credit (0-1)</summary>
    public decimal TimeDecayCredit { get; set; } = 0;

    /// <summary>U-shape credit (0-1)</summary>
    public decimal UShapeCredit { get; set; } = 0;

    /// <summary>Custom model credit (0-1)</summary>
    public decimal CustomCredit { get; set; } = 0;

    /// <summary>Position in journey (1 = first)</summary>
    public int PositionInJourney { get; set; }

    /// <summary>Total touchpoints in journey</summary>
    public int TotalTouchpointsInJourney { get; set; }

    /// <summary>Days to conversion</summary>
    public int? DaysToConversion { get; set; }

    #endregion

    #region Revenue Attribution

    /// <summary>Attributed revenue (first-touch)</summary>
    public decimal? FirstTouchRevenue { get; set; }

    /// <summary>Attributed revenue (last-touch)</summary>
    public decimal? LastTouchRevenue { get; set; }

    /// <summary>Attributed revenue (linear)</summary>
    public decimal? LinearRevenue { get; set; }

    /// <summary>Attributed revenue (custom)</summary>
    public decimal? CustomRevenue { get; set; }

    /// <summary>Attributed pipeline value</summary>
    public decimal? AttributedPipeline { get; set; }

    #endregion

    #region Relationships

    /// <summary>Lead ID</summary>
    public int? LeadId { get; set; }

    /// <summary>Navigation to lead</summary>
    public Lead? Lead { get; set; }

    /// <summary>Contact ID</summary>
    public int? ContactId { get; set; }

    /// <summary>Navigation to contact</summary>
    public Contact? Contact { get; set; }

    /// <summary>Account ID</summary>
    public int? AccountId { get; set; }

    /// <summary>Navigation to account</summary>
    public Account? Account { get; set; }

    /// <summary>Opportunity ID (if attributed to opportunity)</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Navigation to opportunity</summary>
    public Opportunity? Opportunity { get; set; }

    /// <summary>Campaign ID</summary>
    public int? CampaignId { get; set; }

    /// <summary>Navigation to campaign</summary>
    public MarketingCampaign? Campaign { get; set; }

    /// <summary>Web visitor ID</summary>
    public int? WebVisitorId { get; set; }

    /// <summary>Navigation to web visitor</summary>
    public WebVisitor? WebVisitor { get; set; }

    /// <summary>Form submission ID</summary>
    public int? FormSubmissionId { get; set; }

    /// <summary>Navigation to form submission</summary>
    public FormSubmission? FormSubmission { get; set; }

    #endregion

    #region Device/Location

    /// <summary>Device type</summary>
    public string? DeviceType { get; set; }

    /// <summary>Country</summary>
    public string? Country { get; set; }

    /// <summary>Region</summary>
    public string? Region { get; set; }

    /// <summary>City</summary>
    public string? City { get; set; }

    #endregion
}

/// <summary>
/// Aggregated campaign attribution report.
/// </summary>
public class CampaignAttributionSummary : BaseEntity
{
    /// <summary>Report period start</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Report period end</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Campaign ID</summary>
    public int CampaignId { get; set; }

    /// <summary>Navigation to campaign</summary>
    public MarketingCampaign? Campaign { get; set; }

    /// <summary>Attribution model used</summary>
    public CampaignAttributionModel Model { get; set; }

    /// <summary>Total touchpoints</summary>
    public int TotalTouchpoints { get; set; }

    /// <summary>Unique leads touched</summary>
    public int UniquLeads { get; set; }

    /// <summary>Attributed conversions</summary>
    public decimal AttributedConversions { get; set; }

    /// <summary>Attributed revenue</summary>
    public decimal AttributedRevenue { get; set; }

    /// <summary>Attributed pipeline</summary>
    public decimal AttributedPipeline { get; set; }

    /// <summary>Cost per attributed conversion</summary>
    public decimal? CostPerConversion { get; set; }

    /// <summary>Return on ad spend</summary>
    public decimal? ROAS { get; set; }

    /// <summary>Campaign cost in period</summary>
    public decimal? CampaignCost { get; set; }

    /// <summary>First-touch conversions</summary>
    public decimal FirstTouchConversions { get; set; }

    /// <summary>Last-touch conversions</summary>
    public decimal LastTouchConversions { get; set; }

    /// <summary>Average touchpoints to conversion</summary>
    public decimal? AvgTouchpointsToConversion { get; set; }

    /// <summary>Average days to conversion</summary>
    public decimal? AvgDaysToConversion { get; set; }
}
