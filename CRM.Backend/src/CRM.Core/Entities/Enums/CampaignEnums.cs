// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// FUNCTIONAL: Campaign lifecycle status.
/// TECHNICAL: Controls campaign execution and reporting.
/// </summary>
public enum CampaignStatus
{
    /// <summary>Campaign being planned/configured</summary>
    Draft = 0,

    /// <summary>Campaign approved, awaiting start</summary>
    Scheduled = 1,

    /// <summary>Campaign actively running</summary>
    Active = 2,

    /// <summary>Campaign temporarily stopped</summary>
    Paused = 3,

    /// <summary>Campaign finished successfully</summary>
    Completed = 4,

    /// <summary>Campaign stopped before completion</summary>
    Cancelled = 5,

    /// <summary>Campaign archived for historical reference</summary>
    Archived = 6,

    /// <summary>Awaiting approval</summary>
    PendingApproval = 7,

    /// <summary>Campaign rejected in approval</summary>
    Rejected = 8,

    /// <summary>Campaign in review/optimization phase</summary>
    InReview = 9
}

/// <summary>
/// FUNCTIONAL: Primary campaign channel/type.
/// TECHNICAL: Determines available features and metrics.
/// </summary>
public enum CampaignType
{
    /// <summary>Email marketing campaign</summary>
    Email = 0,

    /// <summary>Social media marketing</summary>
    SocialMedia = 1,

    /// <summary>Paid search (PPC/SEM)</summary>
    PaidSearch = 2,

    /// <summary>Display/banner advertising</summary>
    DisplayAds = 3,

    /// <summary>Content marketing (blog, resources)</summary>
    ContentMarketing = 4,

    /// <summary>Search engine optimization</summary>
    SEO = 5,

    /// <summary>In-person or virtual event</summary>
    Event = 6,

    /// <summary>Webinar or online seminar</summary>
    Webinar = 7,

    /// <summary>Physical direct mail</summary>
    DirectMail = 8,

    /// <summary>Outbound phone campaigns</summary>
    Telemarketing = 9,

    /// <summary>Customer referral program</summary>
    Referral = 10,

    /// <summary>Affiliate marketing</summary>
    Affiliate = 11,

    /// <summary>Influencer partnerships</summary>
    Influencer = 12,

    /// <summary>Public relations</summary>
    PR = 13,

    /// <summary>Trade show or conference</summary>
    TradeShow = 14,

    /// <summary>Video marketing</summary>
    Video = 15,

    /// <summary>Podcast sponsorship/hosting</summary>
    Podcast = 16,

    /// <summary>SMS/text message campaign</summary>
    SMS = 17,

    /// <summary>Push notification campaign</summary>
    PushNotification = 18,

    /// <summary>Retargeting/remarketing</summary>
    Retargeting = 19,

    /// <summary>Account-based marketing</summary>
    ABM = 20,

    /// <summary>Partner co-marketing</summary>
    PartnerMarketing = 21,

    /// <summary>Product launch campaign</summary>
    ProductLaunch = 22,

    /// <summary>Brand awareness campaign</summary>
    BrandAwareness = 23,

    /// <summary>Multi-channel integrated campaign</summary>
    Integrated = 24,

    /// <summary>Other campaign type</summary>
    Other = 25
}

/// <summary>
/// FUNCTIONAL: Campaign priority for resource allocation.
/// TECHNICAL: Used for scheduling and conflict resolution.
/// </summary>
public enum CampaignPriority
{
    /// <summary>Low priority campaign</summary>
    Low = 0,

    /// <summary>Standard priority</summary>
    Medium = 1,

    /// <summary>High priority campaign</summary>
    High = 2,

    /// <summary>Critical/urgent campaign</summary>
    Critical = 3,

    /// <summary>Strategic initiative</summary>
    Strategic = 4
}

/// <summary>
/// FUNCTIONAL: Primary campaign objective/goal.
/// TECHNICAL: Drives KPI selection and success criteria.
/// </summary>
public enum CampaignObjective
{
    /// <summary>Not specified</summary>
    NotSpecified = 0,

    /// <summary>Generate new leads</summary>
    LeadGeneration = 1,

    /// <summary>Increase brand awareness</summary>
    BrandAwareness = 2,

    /// <summary>Drive product sales</summary>
    Sales = 3,

    /// <summary>Engage existing customers</summary>
    CustomerEngagement = 4,

    /// <summary>Retain/reduce churn</summary>
    CustomerRetention = 5,

    /// <summary>Upsell/cross-sell</summary>
    Upsell = 6,

    /// <summary>Product education</summary>
    ProductEducation = 7,

    /// <summary>Event promotion/attendance</summary>
    EventPromotion = 8,

    /// <summary>Content distribution</summary>
    ContentPromotion = 9,

    /// <summary>Market research/feedback</summary>
    MarketResearch = 10,

    /// <summary>Product launch</summary>
    ProductLaunch = 11,

    /// <summary>Reactivate dormant leads</summary>
    Reactivation = 12,

    /// <summary>Competitive displacement</summary>
    CompetitiveWin = 13,

    /// <summary>Referral generation</summary>
    Referrals = 14,

    /// <summary>Account penetration (ABM)</summary>
    AccountPenetration = 15
}

/// <summary>
/// FUNCTIONAL: Campaign audience type.
/// TECHNICAL: Determines targeting capabilities.
/// </summary>
public enum AudienceType
{
    /// <summary>New prospects</summary>
    Prospects = 0,

    /// <summary>Existing leads in funnel</summary>
    Leads = 1,

    /// <summary>Current customers</summary>
    Customers = 2,

    /// <summary>Former/churned customers</summary>
    FormerCustomers = 3,

    /// <summary>Partners/affiliates</summary>
    Partners = 4,

    /// <summary>Mixed audience</summary>
    Mixed = 5,

    /// <summary>Specific target accounts (ABM)</summary>
    TargetAccounts = 6,

    /// <summary>Lookalike audience</summary>
    Lookalike = 7
}

/// <summary>
/// FUNCTIONAL: How campaign success is measured.
/// TECHNICAL: Primary KPI for reporting.
/// </summary>
public enum SuccessMetric
{
    /// <summary>Number of leads generated</summary>
    LeadsGenerated = 0,

    /// <summary>Marketing qualified leads</summary>
    MQLs = 1,

    /// <summary>Sales qualified leads</summary>
    SQLs = 2,

    /// <summary>Opportunities created</summary>
    Opportunities = 3,

    /// <summary>Revenue generated</summary>
    Revenue = 4,

    /// <summary>Click-through rate</summary>
    CTR = 5,

    /// <summary>Conversion rate</summary>
    ConversionRate = 6,

    /// <summary>Engagement rate</summary>
    EngagementRate = 7,

    /// <summary>Cost per lead</summary>
    CostPerLead = 8,

    /// <summary>Return on investment</summary>
    ROI = 9,

    /// <summary>Event registrations</summary>
    Registrations = 10,

    /// <summary>Event attendance</summary>
    Attendance = 11,

    /// <summary>Content downloads</summary>
    Downloads = 12,

    /// <summary>Demo requests</summary>
    DemoRequests = 13,

    /// <summary>Trial signups</summary>
    Trials = 14
}

/// <summary>
/// FUNCTIONAL: Type of email sequence step.
/// TECHNICAL: Determines step behaviour in nurture flows.
/// </summary>
public enum SequenceStepType
{
    /// <summary>Send an email</summary>
    Email = 0,

    /// <summary>Wait/delay before next step</summary>
    Wait = 1,

    /// <summary>Conditional branch</summary>
    Condition = 2,

    /// <summary>Apply a tag to the lead/contact</summary>
    Tag = 3
}

/// <summary>
/// FUNCTIONAL: Tracking events captured per email send.
/// TECHNICAL: Fired by email provider webhooks.
/// </summary>
public enum EmailTrackingEvent
{
    /// <summary>Email was handed off to provider</summary>
    Sent = 0,

    /// <summary>Email reached recipient inbox</summary>
    Delivered = 1,

    /// <summary>Recipient opened the email</summary>
    Opened = 2,

    /// <summary>Recipient clicked a link</summary>
    Clicked = 3,

    /// <summary>Email hard/soft bounced</summary>
    Bounced = 4,

    /// <summary>Recipient unsubscribed</summary>
    Unsubscribed = 5,

    /// <summary>Marked as spam</summary>
    SpamReported = 6
}

/// <summary>
/// FUNCTIONAL: What triggered a lead's enrolment in a nurture sequence.
/// TECHNICAL: Stored on NurtureEnrollment for auditing.
/// </summary>
public enum NurtureEnrollmentTrigger
{
    /// <summary>Lead record was created</summary>
    LeadCreated = 0,

    /// <summary>Lead status changed to a qualifying state</summary>
    LeadStatusChanged = 1,

    /// <summary>Manually enrolled by a user</summary>
    ManualEnroll = 2,

    /// <summary>Web form submission triggered enrolment</summary>
    WebFormSubmit = 3
}

/// <summary>
/// FUNCTIONAL: Reason a contact chose to unsubscribe.
/// TECHNICAL: Stored on UnsubscribeRecord for reporting.
/// </summary>
public enum UnsubscribeReason
{
    /// <summary>Not interested in content</summary>
    NotInterested = 0,

    /// <summary>Receiving emails too frequently</summary>
    TooFrequent = 1,

    /// <summary>Content is not relevant</summary>
    Irrelevant = 2,

    /// <summary>Never subscribed voluntarily</summary>
    NeverSubscribed = 3,

    /// <summary>Other reason</summary>
    Other = 4
}
