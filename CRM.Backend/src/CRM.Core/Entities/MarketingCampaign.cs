namespace CRM.Core.Entities;

#region Campaign Enumerations

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

#endregion

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// Comprehensive Marketing Campaign entity for planning, executing, and measuring 
/// marketing initiatives across all channels.
/// 
/// CAMPAIGN PLANNING:
/// - Objectives and success criteria
/// - Budget allocation and tracking
/// - Timeline management
/// - Resource assignment
/// 
/// AUDIENCE TARGETING:
/// - Demographic targeting
/// - Firmographic criteria
/// - Behavioral segments
/// - Account lists (ABM)
/// - Exclusion rules
/// 
/// MULTI-CHANNEL EXECUTION:
/// - Email campaigns
/// - Social media
/// - Paid advertising
/// - Events/webinars
/// - Content marketing
/// 
/// LEAD GENERATION:
/// - Generated leads collection
/// - Attribution tracking
/// - Lead quality scoring
/// - Conversion tracking
/// 
/// PERFORMANCE ANALYTICS:
/// - Impressions and reach
/// - Engagement metrics
/// - Conversion funnel
/// - Revenue attribution
/// - ROI calculation
/// 
/// TECHNICAL VIEW:
/// ===============
/// Campaign entity with comprehensive tracking and multi-touch attribution.
/// 
/// Key Relationships:
/// - Campaign → Leads (generated)
/// - Campaign → Opportunities (influenced)
/// - Campaign → Parent/Child (hierarchy)
/// - Campaign → User (owner/team)
/// </summary>
public class MarketingCampaign : BaseEntity
{
    #region Basic Information
    
    /// <summary>
    /// FUNCTIONAL: Campaign name
    /// TECHNICAL: Required, searchable
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Internal campaign code
    /// TECHNICAL: Unique identifier for tracking
    /// </summary>
    public string? CampaignCode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Full description
    /// TECHNICAL: Rich text supported
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Campaign objective text
    /// TECHNICAL: What we want to achieve
    /// </summary>
    public string? Objective { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Campaign objective type
    /// TECHNICAL: Drives KPI selection
    /// </summary>
    public CampaignObjective ObjectiveType { get; set; } = CampaignObjective.LeadGeneration;
    
    /// <summary>
    /// FUNCTIONAL: Primary campaign channel
    /// TECHNICAL: Determines available features
    /// </summary>
    public CampaignType CampaignType { get; set; } = CampaignType.Email;
    
    /// <summary>
    /// FUNCTIONAL: Campaign lifecycle status
    /// TECHNICAL: Controls execution
    /// </summary>
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    
    /// <summary>
    /// FUNCTIONAL: Campaign priority
    /// TECHNICAL: For resource allocation
    /// </summary>
    public CampaignPriority Priority { get; set; } = CampaignPriority.Medium;
    
    /// <summary>
    /// FUNCTIONAL: Legacy type field
    /// TECHNICAL: Backward compatibility
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Primary success metric
    /// TECHNICAL: How success is measured
    /// </summary>
    public SuccessMetric PrimarySuccessMetric { get; set; } = SuccessMetric.LeadsGenerated;
    
    /// <summary>
    /// FUNCTIONAL: Campaign theme/headline
    /// TECHNICAL: For creative consistency
    /// </summary>
    public string? Theme { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Value proposition message
    /// TECHNICAL: Key message for campaign
    /// </summary>
    public string? ValueProposition { get; set; }
    
    #endregion
    
    #region Timeline & Scheduling
    
    /// <summary>
    /// FUNCTIONAL: Planned start date
    /// TECHNICAL: Scheduled launch
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Planned end date
    /// TECHNICAL: Scheduled end
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Actual start date
    /// TECHNICAL: When actually launched
    /// </summary>
    public DateTime? ActualStartDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Actual end date
    /// TECHNICAL: When actually ended
    /// </summary>
    public DateTime? ActualEndDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Campaign duration (days)
    /// TECHNICAL: Calculated or specified
    /// </summary>
    public int? DurationDays { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Is evergreen (no end date)?
    /// TECHNICAL: Continuous campaign
    /// </summary>
    public bool IsEvergreen { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Campaign timezone
    /// TECHNICAL: For scheduling
    /// </summary>
    public string? Timezone { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Send/publish schedule
    /// TECHNICAL: JSON schedule config
    /// </summary>
    public string? Schedule { get; set; }
    
    #endregion
    
    #region Budget & Financials
    
    /// <summary>
    /// FUNCTIONAL: Planned total budget
    /// TECHNICAL: Maximum spend
    /// </summary>
    public decimal Budget { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Actual spend to date
    /// TECHNICAL: Running total
    /// </summary>
    public decimal ActualCost { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Daily budget limit
    /// TECHNICAL: For pacing
    /// </summary>
    public decimal? DailyBudget { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Monthly budget limit
    /// TECHNICAL: For pacing
    /// </summary>
    public decimal? MonthlyBudget { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Expected revenue outcome
    /// TECHNICAL: Projected revenue
    /// </summary>
    public decimal? ExpectedRevenue { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Actual attributed revenue
    /// TECHNICAL: From converted leads/opps
    /// </summary>
    public decimal? ActualRevenue { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Pipeline influenced
    /// TECHNICAL: Total opportunity value touched
    /// </summary>
    public decimal? PipelineInfluenced { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Pipeline created
    /// TECHNICAL: New opportunity value created
    /// </summary>
    public decimal? PipelineCreated { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per lead
    /// TECHNICAL: ActualCost / LeadsGenerated
    /// </summary>
    public decimal? CostPerLead { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per MQL
    /// TECHNICAL: ActualCost / MQLsGenerated
    /// </summary>
    public decimal? CostPerMql { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per SQL
    /// TECHNICAL: ActualCost / SQLsGenerated
    /// </summary>
    public decimal? CostPerSql { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per opportunity
    /// TECHNICAL: ActualCost / Opportunities
    /// </summary>
    public decimal? CostPerOpportunity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per acquisition
    /// TECHNICAL: ActualCost / CustomersAcquired
    /// </summary>
    public decimal? CostPerAcquisition { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Currency for all amounts
    /// TECHNICAL: ISO 4217 code
    /// </summary>
    public string? CurrencyCode { get; set; } = "USD";
    
    #endregion
    
    #region Target Audience
    
    /// <summary>
    /// FUNCTIONAL: Target audience size
    /// TECHNICAL: Total addressable audience
    /// </summary>
    public int TargetAudience { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Audience description
    /// TECHNICAL: Who we're targeting
    /// </summary>
    public string? TargetAudienceDescription { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Audience type
    /// TECHNICAL: Prospects, leads, customers
    /// </summary>
    public AudienceType AudienceType { get; set; } = AudienceType.Prospects;
    
    /// <summary>
    /// FUNCTIONAL: Demographic criteria
    /// TECHNICAL: JSON: {age, gender, income, education}
    /// </summary>
    public string? TargetDemographics { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Firmographic criteria
    /// TECHNICAL: JSON: {company size, revenue, industry}
    /// </summary>
    public string? TargetFirmographics { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Geographic targets
    /// TECHNICAL: Countries, regions, cities
    /// </summary>
    public string? TargetGeography { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Industry targets
    /// TECHNICAL: Comma-separated or JSON
    /// </summary>
    public string? TargetIndustries { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer segments targeted
    /// TECHNICAL: Segment names/IDs
    /// </summary>
    public string? TargetSegments { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Buyer personas targeted
    /// TECHNICAL: Persona names/IDs
    /// </summary>
    public string? TargetPersonas { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Job titles targeted
    /// TECHNICAL: For B2B targeting
    /// </summary>
    public string? TargetJobTitles { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Seniority levels targeted
    /// TECHNICAL: C-level, VP, Director, etc.
    /// </summary>
    public string? TargetSeniorityLevels { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Target accounts for ABM
    /// TECHNICAL: JSON array of account IDs
    /// </summary>
    public string? TargetAccounts { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Exclusion criteria
    /// TECHNICAL: Who to exclude
    /// </summary>
    public string? ExclusionCriteria { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Suppression list IDs
    /// TECHNICAL: Lists to exclude
    /// </summary>
    public string? SuppressionLists { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Audience list/segment ID
    /// TECHNICAL: External list reference
    /// </summary>
    public string? AudienceListId { get; set; }
    
    #endregion
    
    #region Lead Generation Metrics
    
    /// <summary>
    /// FUNCTIONAL: Total leads generated
    /// TECHNICAL: New lead count
    /// </summary>
    public int LeadsGenerated { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Marketing qualified leads
    /// TECHNICAL: MQLs from campaign
    /// </summary>
    public int MqlsGenerated { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Sales qualified leads
    /// TECHNICAL: SQLs from campaign
    /// </summary>
    public int SqlsGenerated { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Sales accepted leads
    /// TECHNICAL: SALs from campaign
    /// </summary>
    public int SalsGenerated { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Opportunities created
    /// TECHNICAL: New opps from campaign
    /// </summary>
    public int OpportunitiesCreated { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Opportunities influenced
    /// TECHNICAL: Touched existing opps
    /// </summary>
    public int OpportunitiesInfluenced { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Deals won
    /// TECHNICAL: Closed-won from campaign
    /// </summary>
    public int DealsWon { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Customers acquired
    /// TECHNICAL: New customers from campaign
    /// </summary>
    public int CustomersAcquired { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Lead-to-MQL rate
    /// TECHNICAL: MQLs / Leads × 100
    /// </summary>
    public double LeadToMqlRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: MQL-to-SQL rate
    /// TECHNICAL: SQLs / MQLs × 100
    /// </summary>
    public double MqlToSqlRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: SQL-to-Opportunity rate
    /// TECHNICAL: Opps / SQLs × 100
    /// </summary>
    public double SqlToOpportunityRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Opportunity-to-Win rate
    /// TECHNICAL: Wins / Opps × 100
    /// </summary>
    public double OpportunityToWinRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Overall conversion rate
    /// TECHNICAL: End-to-end funnel
    /// </summary>
    public double ConversionRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Average lead score
    /// TECHNICAL: Quality of leads generated
    /// </summary>
    public double AverageLeadScore { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Lead quality rating
    /// TECHNICAL: A, B, C, D distribution
    /// </summary>
    public string? LeadQualityDistribution { get; set; }
    
    #endregion
    
    #region Reach & Engagement Metrics
    
    /// <summary>
    /// FUNCTIONAL: Total impressions served
    /// TECHNICAL: Ad/content views
    /// </summary>
    public long Impressions { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Unique reach
    /// TECHNICAL: Unique people reached
    /// </summary>
    public long Reach { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Frequency
    /// TECHNICAL: Avg impressions per person
    /// </summary>
    public double Frequency { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Total clicks
    /// TECHNICAL: Link clicks
    /// </summary>
    public int Clicks { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Click-through rate
    /// TECHNICAL: Clicks / Impressions × 100
    /// </summary>
    public double ClickThroughRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Landing page visits
    /// TECHNICAL: Page views from campaign
    /// </summary>
    public int LandingPageVisits { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Form submissions
    /// TECHNICAL: Lead capture forms
    /// </summary>
    public int FormSubmissions { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Form conversion rate
    /// TECHNICAL: Submissions / Visits × 100
    /// </summary>
    public double FormConversionRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Content downloads
    /// TECHNICAL: Gated content downloads
    /// </summary>
    public int ContentDownloads { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Video views
    /// TECHNICAL: For video campaigns
    /// </summary>
    public int VideoViews { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Video completion rate
    /// TECHNICAL: % watched to end
    /// </summary>
    public double VideoCompletionRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Demo requests
    /// TECHNICAL: Product demo requests
    /// </summary>
    public int DemoRequests { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Trial signups
    /// TECHNICAL: Free trial starts
    /// </summary>
    public int TrialSignups { get; set; } = 0;
    
    #endregion
    
    #region Email Campaign Metrics
    
    /// <summary>
    /// FUNCTIONAL: Emails sent
    /// TECHNICAL: Total sends
    /// </summary>
    public int EmailsSent { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Emails delivered
    /// TECHNICAL: Successfully delivered
    /// </summary>
    public int EmailsDelivered { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Delivery rate
    /// TECHNICAL: Delivered / Sent × 100
    /// </summary>
    public double DeliveryRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Emails opened
    /// TECHNICAL: Unique opens
    /// </summary>
    public int EmailsOpened { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Open rate
    /// TECHNICAL: Opens / Delivered × 100
    /// </summary>
    public double OpenRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Email clicks
    /// TECHNICAL: Unique clicks
    /// </summary>
    public int EmailClicks { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Email click rate
    /// TECHNICAL: Clicks / Delivered × 100
    /// </summary>
    public double EmailClickRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Click-to-open rate
    /// TECHNICAL: Clicks / Opens × 100
    /// </summary>
    public double ClickToOpenRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Hard bounces
    /// TECHNICAL: Permanent delivery failures
    /// </summary>
    public int HardBounces { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Soft bounces
    /// TECHNICAL: Temporary delivery failures
    /// </summary>
    public int SoftBounces { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Total bounces
    /// TECHNICAL: All bounces
    /// </summary>
    public int Bounces { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Bounce rate
    /// TECHNICAL: Bounces / Sent × 100
    /// </summary>
    public double BounceRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Unsubscribes
    /// TECHNICAL: Opt-outs from campaign
    /// </summary>
    public int Unsubscribes { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Unsubscribe rate
    /// TECHNICAL: Unsubs / Delivered × 100
    /// </summary>
    public double UnsubscribeRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Spam complaints
    /// TECHNICAL: Marked as spam
    /// </summary>
    public int SpamComplaints { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Complaint rate
    /// TECHNICAL: Complaints / Delivered × 100
    /// </summary>
    public double ComplaintRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Email forwards
    /// TECHNICAL: Forwarded to others
    /// </summary>
    public int EmailForwards { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: List growth
    /// TECHNICAL: Net new subscribers
    /// </summary>
    public int ListGrowth { get; set; } = 0;
    
    #endregion
    
    #region Social Media Metrics
    
    /// <summary>
    /// FUNCTIONAL: Social reach
    /// TECHNICAL: People who saw content
    /// </summary>
    public long SocialReach { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Total engagements
    /// TECHNICAL: All interactions
    /// </summary>
    public int SocialEngagement { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Engagement rate
    /// TECHNICAL: Engagements / Reach × 100
    /// </summary>
    public double SocialEngagementRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Shares/Retweets/Reposts
    /// TECHNICAL: Content amplification
    /// </summary>
    public int SocialShares { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Comments/Replies
    /// TECHNICAL: Conversation starters
    /// </summary>
    public int SocialComments { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Likes/Reactions
    /// TECHNICAL: Quick engagements
    /// </summary>
    public int SocialLikes { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Saves/Bookmarks
    /// TECHNICAL: Content saves
    /// </summary>
    public int SocialSaves { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: New followers gained
    /// TECHNICAL: Follower growth
    /// </summary>
    public int NewFollowers { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Profile visits
    /// TECHNICAL: Profile clicks
    /// </summary>
    public int ProfileVisits { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Mentions
    /// TECHNICAL: Brand mentions
    /// </summary>
    public int Mentions { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Social sentiment score
    /// TECHNICAL: -100 to +100
    /// </summary>
    public int? SentimentScore { get; set; }
    
    #endregion
    
    #region Paid Advertising Metrics
    
    /// <summary>
    /// FUNCTIONAL: Ad spend
    /// TECHNICAL: Total ad cost
    /// </summary>
    public decimal? AdSpend { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per click
    /// TECHNICAL: AdSpend / Clicks
    /// </summary>
    public decimal? CostPerClick { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost per thousand impressions
    /// TECHNICAL: AdSpend / (Impressions/1000)
    /// </summary>
    public decimal? CostPerMille { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Return on ad spend
    /// TECHNICAL: Revenue / AdSpend
    /// </summary>
    public double? Roas { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Quality score
    /// TECHNICAL: Platform quality metric
    /// </summary>
    public double? QualityScore { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Ad position/rank
    /// TECHNICAL: Average position
    /// </summary>
    public double? AveragePosition { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Impression share
    /// TECHNICAL: % of available impressions
    /// </summary>
    public double? ImpressionShare { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Bid keywords
    /// TECHNICAL: JSON array for PPC
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Negative keywords
    /// TECHNICAL: Excluded terms
    /// </summary>
    public string? NegativeKeywords { get; set; }
    
    #endregion
    
    #region Event/Webinar Metrics
    
    /// <summary>
    /// FUNCTIONAL: Event registrations
    /// TECHNICAL: Total registrants
    /// </summary>
    public int Registrations { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Event attendance
    /// TECHNICAL: Actual attendees
    /// </summary>
    public int Attendance { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Attendance rate
    /// TECHNICAL: Attendance / Registrations × 100
    /// </summary>
    public double AttendanceRate { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: No-shows
    /// TECHNICAL: Registered but didn't attend
    /// </summary>
    public int NoShows { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Event capacity
    /// TECHNICAL: Maximum attendees
    /// </summary>
    public int? EventCapacity { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Event location
    /// TECHNICAL: Venue or URL
    /// </summary>
    public string? EventLocation { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Event date/time
    /// TECHNICAL: When event occurs
    /// </summary>
    public DateTime? EventDateTime { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Webinar platform
    /// TECHNICAL: Zoom, WebEx, etc.
    /// </summary>
    public string? WebinarPlatform { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Webinar recording URL
    /// TECHNICAL: On-demand link
    /// </summary>
    public string? WebinarRecordingUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: On-demand views
    /// TECHNICAL: Recording views
    /// </summary>
    public int OnDemandViews { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Poll responses
    /// TECHNICAL: Interactive engagement
    /// </summary>
    public int PollResponses { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Q&amp;A questions
    /// TECHNICAL: Questions submitted
    /// </summary>
    public int QuestionsAsked { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Event satisfaction score
    /// TECHNICAL: Post-event rating
    /// </summary>
    public double? EventSatisfactionScore { get; set; }
    
    #endregion
    
    #region ROI & Performance
    
    /// <summary>
    /// FUNCTIONAL: Return on investment
    /// TECHNICAL: (Revenue - Cost) / Cost × 100
    /// </summary>
    public double ROI { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Target ROI
    /// TECHNICAL: Goal ROI %
    /// </summary>
    public double? TargetRoi { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Target lead count
    /// TECHNICAL: Goal leads
    /// </summary>
    public int? TargetLeads { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Target conversions
    /// TECHNICAL: Goal conversions
    /// </summary>
    public int? TargetConversions { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Goal achievement %
    /// TECHNICAL: Progress to target
    /// </summary>
    public double? GoalAchievementPercent { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Campaign health score
    /// TECHNICAL: 0-100 performance score
    /// </summary>
    public int? CampaignHealthScore { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Benchmark comparison
    /// TECHNICAL: vs industry/historical
    /// </summary>
    public string? BenchmarkComparison { get; set; }
    
    #endregion
    
    #region Content & Creative
    
    /// <summary>
    /// FUNCTIONAL: Email subject line
    /// TECHNICAL: Main subject
    /// </summary>
    public string? MessageSubject { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Preheader text
    /// TECHNICAL: Email preview text
    /// </summary>
    public string? PreheaderText { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Message body/content
    /// TECHNICAL: Main content HTML/text
    /// </summary>
    public string? MessageBody { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: From name
    /// TECHNICAL: Sender name
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: From email
    /// TECHNICAL: Sender email
    /// </summary>
    public string? FromEmail { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Reply-to email
    /// TECHNICAL: Reply address
    /// </summary>
    public string? ReplyToEmail { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Call to action text
    /// TECHNICAL: Button/link text
    /// </summary>
    public string? CallToAction { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: CTA button URL
    /// TECHNICAL: Primary CTA link
    /// </summary>
    public string? CtaUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Landing page URL
    /// TECHNICAL: Campaign landing page
    /// </summary>
    public string? LandingPageUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Tracking URL
    /// TECHNICAL: With UTM parameters
    /// </summary>
    public string? TrackingUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Ad creative URLs
    /// TECHNICAL: JSON array of creative assets
    /// </summary>
    public string? CreativeAssets { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Email template ID
    /// TECHNICAL: Template reference
    /// </summary>
    public string? TemplateId { get; set; }
    
    #endregion
    
    #region UTM Tracking
    
    /// <summary>
    /// FUNCTIONAL: UTM source
    /// TECHNICAL: Traffic source
    /// </summary>
    public string? UtmSource { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM medium
    /// TECHNICAL: Marketing medium
    /// </summary>
    public string? UtmMedium { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM campaign
    /// TECHNICAL: Campaign name
    /// </summary>
    public string? UtmCampaign { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM content
    /// TECHNICAL: Ad/creative variant
    /// </summary>
    public string? UtmContent { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM term
    /// TECHNICAL: Keywords
    /// </summary>
    public string? UtmTerm { get; set; }
    
    #endregion
    
    #region A/B Testing
    
    /// <summary>
    /// FUNCTIONAL: Is A/B test?
    /// TECHNICAL: Test running
    /// </summary>
    public bool IsABTest { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Test variants
    /// TECHNICAL: JSON array of variants
    /// </summary>
    public string? ABTestVariants { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Test metric
    /// TECHNICAL: What's being measured
    /// </summary>
    public string? ABTestMetric { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Test winner
    /// TECHNICAL: Winning variant ID
    /// </summary>
    public string? WinningVariant { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Statistical significance
    /// TECHNICAL: Confidence level %
    /// </summary>
    public double? StatisticalSignificance { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Test results
    /// TECHNICAL: JSON with per-variant metrics
    /// </summary>
    public string? ABTestResults { get; set; }
    
    #endregion
    
    #region Channels & Platforms
    
    /// <summary>
    /// FUNCTIONAL: Marketing channels used
    /// TECHNICAL: JSON array: ["email", "facebook", "google"]
    /// </summary>
    public string? Channels { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Specific platforms
    /// TECHNICAL: Platform list
    /// </summary>
    public string? Platforms { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Social networks used
    /// TECHNICAL: JSON array
    /// </summary>
    public string? SocialNetworks { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Ad platforms used
    /// TECHNICAL: Google Ads, Facebook, etc.
    /// </summary>
    public string? AdPlatforms { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External campaign IDs
    /// TECHNICAL: JSON {platform: id}
    /// </summary>
    public string? ExternalCampaignIds { get; set; }
    
    #endregion
    
    #region Assignment & Ownership
    
    /// <summary>
    /// FUNCTIONAL: Campaign owner user ID
    /// TECHNICAL: FK to User
    /// </summary>
    public int? OwnerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Assigned to user ID
    /// TECHNICAL: FK to User
    /// </summary>
    public int? AssignedToUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Team members
    /// TECHNICAL: JSON array of user IDs
    /// </summary>
    public string? TeamMembers { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Approver user ID
    /// TECHNICAL: Who approved campaign
    /// </summary>
    public int? ApprovedByUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Approval date
    /// TECHNICAL: When approved
    /// </summary>
    public DateTime? ApprovedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Department responsible
    /// TECHNICAL: Marketing, Sales, etc.
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Cost center code
    /// TECHNICAL: For budget tracking
    /// </summary>
    public string? CostCenter { get; set; }
    
    #endregion
    
    #region Campaign Hierarchy
    
    /// <summary>
    /// FUNCTIONAL: Parent campaign ID
    /// TECHNICAL: For campaign hierarchy
    /// </summary>
    public int? ParentCampaignId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Related campaigns
    /// TECHNICAL: JSON array of campaign IDs
    /// </summary>
    public string? RelatedCampaigns { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Campaign program
    /// TECHNICAL: Grouping identifier
    /// </summary>
    public string? Program { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Initiative name
    /// TECHNICAL: Strategic initiative
    /// </summary>
    public string? Initiative { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fiscal quarter
    /// TECHNICAL: Q1, Q2, Q3, Q4
    /// </summary>
    public string? FiscalQuarter { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fiscal year
    /// TECHNICAL: Year number
    /// </summary>
    public int? FiscalYear { get; set; }
    
    #endregion
    
    #region Classification & Tags
    
    /// <summary>
    /// FUNCTIONAL: Campaign tags
    /// TECHNICAL: Comma-separated
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Campaign category
    /// TECHNICAL: For organization
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sub-category
    /// TECHNICAL: Deeper classification
    /// </summary>
    public string? SubCategory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Region/territory
    /// TECHNICAL: Geographic scope
    /// </summary>
    public string? Region { get; set; }
    
    #endregion
    
    #region Documentation & Notes
    
    /// <summary>
    /// FUNCTIONAL: Campaign notes
    /// TECHNICAL: General notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Internal notes
    /// TECHNICAL: Team-only notes
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Success criteria
    /// TECHNICAL: How success defined
    /// </summary>
    public string? SuccessCriteria { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Lessons learned
    /// TECHNICAL: Post-campaign insights
    /// </summary>
    public string? LessonsLearned { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Attached files
    /// TECHNICAL: JSON array of file URLs
    /// </summary>
    public string? Attachments { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Brief document URL
    /// TECHNICAL: Campaign brief
    /// </summary>
    public string? BriefUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Report URL
    /// TECHNICAL: Performance report
    /// </summary>
    public string? ReportUrl { get; set; }
    
    #endregion
    
    #region Custom Fields & Integration
    
    /// <summary>
    /// FUNCTIONAL: Custom fields
    /// TECHNICAL: JSON for extensibility
    /// </summary>
    public string? CustomFields { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External ID
    /// TECHNICAL: External system reference
    /// </summary>
    public string? ExternalId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sync status
    /// TECHNICAL: Integration sync state
    /// </summary>
    public string? SyncStatus { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Last sync date
    /// TECHNICAL: When last synced
    /// </summary>
    public DateTime? LastSyncDate { get; set; }
    
    #endregion
    
    #region Navigation Properties
    
    /// <summary>Leads generated by this campaign (primary source)</summary>
    public ICollection<Lead>? GeneratedLeads { get; set; }
    
    /// <summary>Leads that converted via this campaign</summary>
    public ICollection<Lead>? ConvertedLeads { get; set; }
    
    /// <summary>All leads touched by this campaign</summary>
    public ICollection<Lead>? TouchedLeads { get; set; }
    
    /// <summary>Products promoted in this campaign</summary>
    public ICollection<Product>? Products { get; set; }
    
    /// <summary>Campaign metrics over time</summary>
    public ICollection<CampaignMetric>? Metrics { get; set; }
    
    /// <summary>Opportunities influenced by this campaign</summary>
    public ICollection<Opportunity>? Opportunities { get; set; }
    
    /// <summary>Campaign owner</summary>
    public User? Owner { get; set; }
    
    /// <summary>Assigned team member</summary>
    public User? AssignedToUser { get; set; }
    
    /// <summary>User who approved campaign</summary>
    public User? ApprovedByUser { get; set; }
    
    /// <summary>Parent campaign in hierarchy</summary>
    public MarketingCampaign? ParentCampaign { get; set; }
    
    /// <summary>Child campaigns</summary>
    public ICollection<MarketingCampaign>? ChildCampaigns { get; set; }
    
    #endregion
    
    #region Calculated Properties
    
    /// <summary>
    /// Is campaign currently running
    /// </summary>
    public bool IsActive => Status == CampaignStatus.Active;
    
    /// <summary>
    /// Budget utilization percentage
    /// </summary>
    public double BudgetUtilization => Budget > 0 ? (double)(ActualCost / Budget) * 100 : 0;
    
    /// <summary>
    /// Is campaign over budget
    /// </summary>
    public bool IsOverBudget => ActualCost > Budget;
    
    /// <summary>
    /// Budget remaining
    /// </summary>
    public decimal BudgetRemaining => Budget - ActualCost;
    
    /// <summary>
    /// Days until campaign end
    /// </summary>
    public int? DaysRemaining => EndDate.HasValue ? (EndDate.Value - DateTime.UtcNow).Days : null;
    
    /// <summary>
    /// Is campaign ending soon (within 7 days)
    /// </summary>
    public bool IsEndingSoon => DaysRemaining.HasValue && DaysRemaining.Value <= 7 && DaysRemaining.Value > 0;
    
    /// <summary>
    /// Overall engagement rate
    /// </summary>
    public double OverallEngagementRate => Impressions > 0 ? (double)Clicks / Impressions * 100 : 0;
    
    #endregion
}
