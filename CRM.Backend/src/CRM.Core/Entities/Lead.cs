namespace CRM.Core.Entities;

#region Lead Enumerations

/// <summary>
/// FUNCTIONAL: Lead lifecycle status indicating where in the funnel the lead is.
/// TECHNICAL: Controls workflow triggers and visibility rules.
/// </summary>
public enum LeadStatus
{
    /// <summary>New lead, not yet contacted</summary>
    New = 0,
    
    /// <summary>Lead is being contacted/worked</summary>
    Contacted = 1,
    
    /// <summary>Initial response received from lead</summary>
    Responded = 2,
    
    /// <summary>Lead shows buying interest</summary>
    Engaged = 3,
    
    /// <summary>Actively evaluating, meeting scheduled</summary>
    Working = 4,
    
    /// <summary>Lead meets qualification criteria</summary>
    Qualified = 5,
    
    /// <summary>Lead is nurturing (not ready to buy)</summary>
    Nurturing = 6,
    
    /// <summary>Converted to opportunity/customer</summary>
    Converted = 7,
    
    /// <summary>Lead is not a fit</summary>
    Disqualified = 8,
    
    /// <summary>Could not reach/no response</summary>
    Unresponsive = 9,
    
    /// <summary>Lead requested no contact</summary>
    OptedOut = 10,
    
    /// <summary>Duplicate of another lead</summary>
    Duplicate = 11,
    
    /// <summary>Recycled for future follow-up</summary>
    Recycled = 12
}

/// <summary>
/// FUNCTIONAL: Quality/temperature rating of the lead.
/// TECHNICAL: Used for prioritization and routing.
/// </summary>
public enum LeadRating
{
    /// <summary>Unknown or not yet rated</summary>
    Unknown = 0,
    
    /// <summary>Low potential, long-term nurture</summary>
    Cold = 1,
    
    /// <summary>Some interest, needs development</summary>
    Warm = 2,
    
    /// <summary>High interest, ready to engage</summary>
    Hot = 3,
    
    /// <summary>Immediate opportunity</summary>
    Urgent = 4
}

/// <summary>
/// FUNCTIONAL: Original source channel of the lead.
/// TECHNICAL: Used for attribution and ROI analysis.
/// </summary>
public enum LeadSource
{
    /// <summary>Unknown origin</summary>
    Unknown = 0,
    
    /// <summary>Website form submission</summary>
    WebsiteForm = 1,
    
    /// <summary>Landing page conversion</summary>
    LandingPage = 2,
    
    /// <summary>Organic search (SEO)</summary>
    OrganicSearch = 3,
    
    /// <summary>Paid search (SEM/PPC)</summary>
    PaidSearch = 4,
    
    /// <summary>Social media (organic)</summary>
    SocialMediaOrganic = 5,
    
    /// <summary>Social media (paid/ads)</summary>
    SocialMediaPaid = 6,
    
    /// <summary>Email campaign response</summary>
    EmailCampaign = 7,
    
    /// <summary>Inbound phone call</summary>
    PhoneInbound = 8,
    
    /// <summary>Outbound cold call</summary>
    PhoneOutbound = 9,
    
    /// <summary>Customer or partner referral</summary>
    Referral = 10,
    
    /// <summary>Trade show or conference</summary>
    TradeShow = 11,
    
    /// <summary>Webinar registration/attendee</summary>
    Webinar = 12,
    
    /// <summary>Content download (whitepaper, ebook)</summary>
    ContentDownload = 13,
    
    /// <summary>Demo request</summary>
    DemoRequest = 14,
    
    /// <summary>Free trial signup</summary>
    FreeTrial = 15,
    
    /// <summary>Partner/channel</summary>
    Partner = 16,
    
    /// <summary>Direct mail response</summary>
    DirectMail = 17,
    
    /// <summary>Chat/chatbot</summary>
    LiveChat = 18,
    
    /// <summary>Third-party/purchased list</summary>
    PurchasedList = 19,
    
    /// <summary>API/integration</summary>
    ApiImport = 20,
    
    /// <summary>Affiliate marketing</summary>
    Affiliate = 21,
    
    /// <summary>PR/media coverage</summary>
    PublicRelations = 22,
    
    /// <summary>Event (in-person)</summary>
    Event = 23,
    
    /// <summary>Other source</summary>
    Other = 24
}

/// <summary>
/// FUNCTIONAL: Reason why lead was disqualified.
/// TECHNICAL: Used for analytics and lead quality improvement.
/// </summary>
public enum DisqualificationReason
{
    /// <summary>Not specified</summary>
    NotSpecified = 0,
    
    /// <summary>No budget/cannot afford</summary>
    NoBudget = 1,
    
    /// <summary>No authority to purchase</summary>
    NoAuthority = 2,
    
    /// <summary>No need/fit for product</summary>
    NoNeed = 3,
    
    /// <summary>No timeline/not ready</summary>
    NoTimeline = 4,
    
    /// <summary>Chose competitor</summary>
    ChoseCompetitor = 5,
    
    /// <summary>Invalid/fake contact info</summary>
    InvalidContact = 6,
    
    /// <summary>Student/researcher (not buyer)</summary>
    NotABuyer = 7,
    
    /// <summary>Geographic mismatch</summary>
    WrongGeography = 8,
    
    /// <summary>Industry not served</summary>
    WrongIndustry = 9,
    
    /// <summary>Company too small</summary>
    CompanyTooSmall = 10,
    
    /// <summary>Company too large</summary>
    CompanyTooLarge = 11,
    
    /// <summary>Spam/bot submission</summary>
    SpamBot = 12,
    
    /// <summary>Existing customer</summary>
    ExistingCustomer = 13,
    
    /// <summary>Competitor/vendor</summary>
    Competitor = 14,
    
    /// <summary>Duplicate lead</summary>
    Duplicate = 15,
    
    /// <summary>Unresponsive after multiple attempts</summary>
    Unresponsive = 16,
    
    /// <summary>Other reason</summary>
    Other = 17
}

/// <summary>
/// FUNCTIONAL: Industry classification for lead's company.
/// TECHNICAL: Standard industry codes for segmentation.
/// </summary>
public enum LeadIndustry
{
    Unknown = 0,
    Technology = 1,
    Healthcare = 2,
    Finance = 3,
    Manufacturing = 4,
    Retail = 5,
    Education = 6,
    Government = 7,
    NonProfit = 8,
    RealEstate = 9,
    Construction = 10,
    Transportation = 11,
    Energy = 12,
    Telecommunications = 13,
    Media = 14,
    Hospitality = 15,
    Agriculture = 16,
    Legal = 17,
    Consulting = 18,
    Insurance = 19,
    Pharmaceutical = 20,
    Aerospace = 21,
    Automotive = 22,
    FoodBeverage = 23,
    Other = 24
}

/// <summary>
/// FUNCTIONAL: Company size category.
/// TECHNICAL: Used for segmentation and qualification.
/// </summary>
public enum CompanySize
{
    Unknown = 0,
    Solo = 1,           // 1 employee
    Micro = 2,          // 2-10 employees
    Small = 3,          // 11-50 employees
    Medium = 4,         // 51-200 employees
    MidMarket = 5,      // 201-1000 employees
    Enterprise = 6,     // 1001-5000 employees
    LargeEnterprise = 7 // 5000+ employees
}

/// <summary>
/// FUNCTIONAL: Lead conversion type.
/// TECHNICAL: What entity was created from the lead.
/// </summary>
public enum ConversionType
{
    /// <summary>Not yet converted</summary>
    NotConverted = 0,
    
    /// <summary>Converted to Contact only</summary>
    Contact = 1,
    
    /// <summary>Converted to Customer (Account)</summary>
    Customer = 2,
    
    /// <summary>Converted to Contact + Customer</summary>
    ContactAndCustomer = 3,
    
    /// <summary>Converted to Opportunity</summary>
    Opportunity = 4,
    
    /// <summary>Converted to Contact + Customer + Opportunity</summary>
    Full = 5
}

#endregion

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// Comprehensive Lead entity representing potential customers before qualification.
/// 
/// LEAD LIFECYCLE:
/// - New → Contacted → Engaged → Qualified → Converted OR Disqualified
/// - Can be recycled for future follow-up
/// - Links to source campaign for attribution
/// 
/// CAMPAIGN ATTRIBUTION:
/// - Primary campaign (first touch)
/// - Converting campaign (last touch)
/// - All touched campaigns (multi-touch)
/// - UTM parameters for granular tracking
/// 
/// CONVERSION TRACKING:
/// - Links to created Contact, Customer, and Opportunity
/// - Conversion date and converted by user
/// - Revenue attribution back to lead source
/// 
/// QUALIFICATION (BANT):
/// - Budget: Has budget? Amount?
/// - Authority: Decision maker? Role?
/// - Need: Identified pain? Use case?
/// - Timeline: When purchasing?
/// 
/// ENGAGEMENT TRACKING:
/// - Activity counts (calls, emails, meetings)
/// - Website visits, content downloads
/// - Email engagement (opens, clicks)
/// - Score based on behavior
/// 
/// TECHNICAL VIEW:
/// ===============
/// Self-contained lead entity with comprehensive tracking.
/// Links to Campaign for attribution.
/// On conversion, creates Contact/Customer/Opportunity records.
/// 
/// Key Relationships:
/// - Campaign (source) → Lead (generated)
/// - Lead (converted) → Contact, Customer, Opportunity
/// - User (owner) → Lead (assigned)
/// </summary>
public class Lead : BaseEntity
{
    #region Contact Information
    
    /// <summary>
    /// FUNCTIONAL: Lead's salutation/title
    /// TECHNICAL: Mr., Mrs., Ms., Dr., etc.
    /// </summary>
    public string? Salutation { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Lead's first name
    /// TECHNICAL: Required for personalization
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Lead's last name
    /// TECHNICAL: Required for personalization
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Lead's suffix
    /// TECHNICAL: Jr., Sr., III, etc.
    /// </summary>
    public string? Suffix { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Primary email address
    /// TECHNICAL: Used for communications and deduplication
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Secondary/personal email
    /// TECHNICAL: Alternate contact
    /// </summary>
    public string? SecondaryEmail { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Primary phone number
    /// TECHNICAL: For sales calls
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Mobile phone number
    /// TECHNICAL: For SMS and direct contact
    /// </summary>
    public string? MobilePhone { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fax number
    /// TECHNICAL: For traditional communications
    /// </summary>
    public string? FaxNumber { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: LinkedIn profile URL
    /// TECHNICAL: For research and social selling
    /// </summary>
    public string? LinkedInUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Twitter handle
    /// TECHNICAL: For social engagement
    /// </summary>
    public string? TwitterHandle { get; set; }
    
    #endregion
    
    #region Company Information
    
    /// <summary>
    /// FUNCTIONAL: Lead's company/organization name
    /// TECHNICAL: Used for account matching
    /// </summary>
    public string Company { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Job title/position
    /// TECHNICAL: Used for persona matching
    /// </summary>
    public string? JobTitle { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Department within company
    /// TECHNICAL: For routing and targeting
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Industry classification
    /// TECHNICAL: For segmentation
    /// </summary>
    public LeadIndustry Industry { get; set; } = LeadIndustry.Unknown;
    
    /// <summary>
    /// FUNCTIONAL: Industry as text (if Other)
    /// TECHNICAL: Flexible industry capture
    /// </summary>
    public string? IndustryOther { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Company website URL
    /// TECHNICAL: For enrichment and research
    /// </summary>
    public string? Website { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of employees
    /// TECHNICAL: For qualification
    /// </summary>
    public int? NumberOfEmployees { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Company size category
    /// TECHNICAL: Standardized size bucket
    /// </summary>
    public CompanySize CompanySize { get; set; } = CompanySize.Unknown;
    
    /// <summary>
    /// FUNCTIONAL: Annual revenue estimate
    /// TECHNICAL: For qualification
    /// </summary>
    public decimal? AnnualRevenue { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Revenue range text
    /// TECHNICAL: &lt;1M, 1-10M, 10-50M, etc.
    /// </summary>
    public string? RevenueRange { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Company description
    /// TECHNICAL: What the company does
    /// </summary>
    public string? CompanyDescription { get; set; }
    
    #endregion
    
    #region Address Information
    
    /// <summary>
    /// FUNCTIONAL: Street address line 1
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Street address line 2
    /// </summary>
    public string? Address2 { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: City
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: State/Province
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Postal/ZIP code
    /// </summary>
    public string? ZipCode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Country
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Geographic region
    /// TECHNICAL: For territory assignment
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Lead's timezone
    /// TECHNICAL: For contact timing
    /// </summary>
    public string? Timezone { get; set; }
    
    #endregion
    
    #region Lead Status & Qualification
    
    /// <summary>
    /// FUNCTIONAL: Current lead status
    /// TECHNICAL: Drives workflow
    /// </summary>
    public LeadStatus Status { get; set; } = LeadStatus.New;
    
    /// <summary>
    /// FUNCTIONAL: Lead temperature/quality rating
    /// TECHNICAL: For prioritization
    /// </summary>
    public LeadRating Rating { get; set; } = LeadRating.Unknown;
    
    /// <summary>
    /// FUNCTIONAL: Behavioral/engagement score
    /// TECHNICAL: 0-100 scale, auto-calculated
    /// </summary>
    public int LeadScore { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Demographic/fit score
    /// TECHNICAL: 0-100 scale, based on firmographics
    /// </summary>
    public int FitScore { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Behavioral/interest score
    /// TECHNICAL: 0-100 scale, based on engagement
    /// </summary>
    public int BehaviorScore { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Combined qualification grade
    /// TECHNICAL: A, B, C, D, F
    /// </summary>
    public string? Grade { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Is lead Marketing Qualified (MQL)?
    /// TECHNICAL: Meets marketing criteria
    /// </summary>
    public bool IsMql { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Date lead became MQL
    /// TECHNICAL: For funnel velocity tracking
    /// </summary>
    public DateTime? MqlDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Is lead Sales Qualified (SQL)?
    /// TECHNICAL: Accepted by sales
    /// </summary>
    public bool IsSql { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Date lead became SQL
    /// TECHNICAL: For funnel velocity tracking
    /// </summary>
    public DateTime? SqlDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Is lead Sales Accepted (SAL)?
    /// TECHNICAL: Being actively worked
    /// </summary>
    public bool IsSal { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Date lead became SAL
    /// TECHNICAL: For funnel velocity tracking
    /// </summary>
    public DateTime? SalDate { get; set; }
    
    #endregion
    
    #region BANT Qualification
    
    /// <summary>
    /// FUNCTIONAL: Has budget confirmed?
    /// TECHNICAL: B in BANT
    /// </summary>
    public bool HasBudget { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Budget amount if known
    /// TECHNICAL: For deal sizing
    /// </summary>
    public decimal? BudgetAmount { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Budget range text
    /// TECHNICAL: If exact not known
    /// </summary>
    public string? BudgetRange { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fiscal year budget approved?
    /// TECHNICAL: Current year funds available
    /// </summary>
    public bool? BudgetApproved { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Has authority to buy?
    /// TECHNICAL: A in BANT
    /// </summary>
    public bool HasAuthority { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Decision maker type
    /// TECHNICAL: Final, Influencer, Recommender, etc.
    /// </summary>
    public string? AuthorityLevel { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Who is the economic buyer?
    /// TECHNICAL: Name of decision maker if not lead
    /// </summary>
    public string? EconomicBuyer { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Has identified need?
    /// TECHNICAL: N in BANT
    /// </summary>
    public bool HasNeed { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Primary pain point
    /// TECHNICAL: Problem trying to solve
    /// </summary>
    public string? PrimaryPainPoint { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Use case/application
    /// TECHNICAL: How they'll use product
    /// </summary>
    public string? UseCase { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Current solution in use
    /// TECHNICAL: Competitive intel
    /// </summary>
    public string? CurrentSolution { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Has purchase timeline?
    /// TECHNICAL: T in BANT
    /// </summary>
    public bool HasTimeline { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Expected purchase date
    /// TECHNICAL: When decision expected
    /// </summary>
    public DateTime? ExpectedPurchaseDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Timeline urgency
    /// TECHNICAL: Immediate, 1-3 months, 3-6 months, etc.
    /// </summary>
    public string? TimelineDescription { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Overall BANT score
    /// TECHNICAL: 0-4, one point per confirmed
    /// </summary>
    public int BantScore { get; set; } = 0;
    
    #endregion
    
    #region Lead Source & Attribution
    
    /// <summary>
    /// FUNCTIONAL: Primary source channel
    /// TECHNICAL: First touch attribution
    /// </summary>
    public LeadSource Source { get; set; } = LeadSource.Unknown;
    
    /// <summary>
    /// FUNCTIONAL: Source details/description
    /// TECHNICAL: Specific source info
    /// </summary>
    public string? SourceDescription { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Primary/first-touch campaign
    /// TECHNICAL: FK to MarketingCampaign
    /// </summary>
    public int? PrimaryCampaignId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Converting/last-touch campaign
    /// TECHNICAL: FK to MarketingCampaign
    /// </summary>
    public int? ConvertingCampaignId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Most recent campaign touched
    /// TECHNICAL: FK to MarketingCampaign
    /// </summary>
    public int? LastCampaignId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: All campaigns that touched this lead
    /// TECHNICAL: JSON array of campaign IDs
    /// </summary>
    public string? CampaignHistory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of campaigns touched
    /// TECHNICAL: For multi-touch analysis
    /// </summary>
    public int CampaignTouchCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: UTM source parameter
    /// TECHNICAL: From tracking URL
    /// </summary>
    public string? UtmSource { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM medium parameter
    /// TECHNICAL: From tracking URL
    /// </summary>
    public string? UtmMedium { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM campaign parameter
    /// TECHNICAL: From tracking URL
    /// </summary>
    public string? UtmCampaign { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM term parameter
    /// TECHNICAL: From tracking URL (keywords)
    /// </summary>
    public string? UtmTerm { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: UTM content parameter
    /// TECHNICAL: From tracking URL (ad variant)
    /// </summary>
    public string? UtmContent { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Referrer URL
    /// TECHNICAL: Where lead came from
    /// </summary>
    public string? ReferrerUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Landing page URL
    /// TECHNICAL: First page visited
    /// </summary>
    public string? LandingPageUrl { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Referring customer/partner ID
    /// TECHNICAL: For referral tracking
    /// </summary>
    public int? ReferredByCustomerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Referrer name (if not customer)
    /// TECHNICAL: Text referrer name
    /// </summary>
    public string? ReferrerName { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Partner ID if from partner
    /// TECHNICAL: Channel attribution
    /// </summary>
    public int? PartnerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Affiliate code
    /// TECHNICAL: For affiliate tracking
    /// </summary>
    public string? AffiliateCode { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Google Click ID
    /// TECHNICAL: For Google Ads attribution
    /// </summary>
    public string? Gclid { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Facebook Click ID
    /// TECHNICAL: For Facebook Ads attribution
    /// </summary>
    public string? Fbclid { get; set; }
    
    #endregion
    
    #region Engagement Tracking
    
    /// <summary>
    /// FUNCTIONAL: Total website visits
    /// TECHNICAL: From tracking
    /// </summary>
    public int WebsiteVisits { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Total page views
    /// TECHNICAL: From tracking
    /// </summary>
    public int PageViews { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Last website visit date
    /// TECHNICAL: For recency scoring
    /// </summary>
    public DateTime? LastWebsiteVisit { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Content downloads count
    /// TECHNICAL: Whitepapers, ebooks, etc.
    /// </summary>
    public int ContentDownloads { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Downloaded content items
    /// TECHNICAL: JSON array of content IDs/names
    /// </summary>
    public string? DownloadedContent { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Webinars attended
    /// TECHNICAL: Count of webinar attendance
    /// </summary>
    public int WebinarsAttended { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Events attended
    /// TECHNICAL: Count of event attendance
    /// </summary>
    public int EventsAttended { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Emails sent count
    /// TECHNICAL: Total emails sent
    /// </summary>
    public int EmailsSent { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Emails opened count
    /// TECHNICAL: Total emails opened
    /// </summary>
    public int EmailsOpened { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Email clicks count
    /// TECHNICAL: Total link clicks
    /// </summary>
    public int EmailClicks { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Last email open date
    /// TECHNICAL: For recency
    /// </summary>
    public DateTime? LastEmailOpenDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Last email click date
    /// TECHNICAL: For recency
    /// </summary>
    public DateTime? LastEmailClickDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Email bounce status
    /// TECHNICAL: Hard, Soft, None
    /// </summary>
    public string? EmailBounceStatus { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Calls made to lead
    /// TECHNICAL: Outbound call count
    /// </summary>
    public int CallsMade { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Calls connected
    /// TECHNICAL: Successful connections
    /// </summary>
    public int CallsConnected { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Last call date
    /// TECHNICAL: Most recent call
    /// </summary>
    public DateTime? LastCallDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Meetings scheduled
    /// TECHNICAL: Total meetings
    /// </summary>
    public int MeetingsScheduled { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Meetings completed
    /// TECHNICAL: Held meetings
    /// </summary>
    public int MeetingsCompleted { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Last meeting date
    /// TECHNICAL: Most recent meeting
    /// </summary>
    public DateTime? LastMeetingDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Total touchpoints
    /// TECHNICAL: All interactions
    /// </summary>
    public int TotalTouchpoints { get; set; } = 0;
    
    #endregion
    
    #region Product Interest
    
    /// <summary>
    /// FUNCTIONAL: Primary product of interest
    /// TECHNICAL: FK to Product
    /// </summary>
    public int? PrimaryProductInterestId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: All products of interest
    /// TECHNICAL: JSON array of product IDs
    /// </summary>
    public string? ProductInterests { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Requested demo?
    /// TECHNICAL: Demo request flag
    /// </summary>
    public bool RequestedDemo { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Demo request date
    /// TECHNICAL: When requested
    /// </summary>
    public DateTime? DemoRequestDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Demo completed?
    /// TECHNICAL: Demo delivered
    /// </summary>
    public bool DemoCompleted { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Demo completion date
    /// TECHNICAL: When demo delivered
    /// </summary>
    public DateTime? DemoCompletedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Trial signup?
    /// TECHNICAL: Started free trial
    /// </summary>
    public bool StartedTrial { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Trial start date
    /// TECHNICAL: When trial began
    /// </summary>
    public DateTime? TrialStartDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Trial end date
    /// TECHNICAL: When trial expires
    /// </summary>
    public DateTime? TrialEndDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Trial status
    /// TECHNICAL: Active, Expired, Converted
    /// </summary>
    public string? TrialStatus { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Estimated deal value
    /// TECHNICAL: Potential revenue
    /// </summary>
    public decimal? EstimatedValue { get; set; }
    
    #endregion
    
    #region Communication Preferences
    
    /// <summary>
    /// FUNCTIONAL: Opted in to email
    /// TECHNICAL: Marketing consent
    /// </summary>
    public bool OptInEmail { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Email opt-in date
    /// TECHNICAL: Consent timestamp
    /// </summary>
    public DateTime? OptInEmailDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Opted in to SMS
    /// TECHNICAL: SMS consent
    /// </summary>
    public bool OptInSms { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Opted in to phone calls
    /// TECHNICAL: Call consent
    /// </summary>
    public bool OptInPhone { get; set; } = true;
    
    /// <summary>
    /// FUNCTIONAL: Preferred contact method
    /// TECHNICAL: Email, Phone, LinkedIn
    /// </summary>
    public string? PreferredContactMethod { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Preferred contact time
    /// TECHNICAL: Morning, Afternoon, Evening
    /// </summary>
    public string? PreferredContactTime { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Do Not Call flag
    /// TECHNICAL: Regulatory compliance
    /// </summary>
    public bool DoNotCall { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Do Not Email flag
    /// TECHNICAL: Unsubscribed
    /// </summary>
    public bool DoNotEmail { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Preferred language
    /// TECHNICAL: For communications
    /// </summary>
    public string? PreferredLanguage { get; set; }
    
    #endregion
    
    #region Assignment & Workflow
    
    /// <summary>
    /// FUNCTIONAL: Assigned owner user ID
    /// TECHNICAL: FK to User
    /// </summary>
    public int? OwnerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: When lead was assigned
    /// TECHNICAL: For SLA tracking
    /// </summary>
    public DateTime? AssignedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Assignment method
    /// TECHNICAL: Manual, RoundRobin, Territory, etc.
    /// </summary>
    public string? AssignmentMethod { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sales territory
    /// TECHNICAL: For routing
    /// </summary>
    public string? Territory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Lead queue/pool
    /// TECHNICAL: If not yet assigned
    /// </summary>
    public string? LeadQueue { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Last activity date
    /// TECHNICAL: Most recent action
    /// </summary>
    public DateTime? LastActivityDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Next follow-up date
    /// TECHNICAL: Scheduled action
    /// </summary>
    public DateTime? NextFollowUpDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Next action description
    /// TECHNICAL: What's planned
    /// </summary>
    public string? NextAction { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Days since last contact
    /// TECHNICAL: Stale lead detection
    /// </summary>
    public int? DaysSinceLastContact { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Is lead stale/neglected?
    /// TECHNICAL: No activity threshold
    /// </summary>
    public bool IsStale { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Number of times recycled
    /// TECHNICAL: For lead quality
    /// </summary>
    public int RecycleCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Last recycled date
    /// TECHNICAL: When put back in pool
    /// </summary>
    public DateTime? LastRecycledDate { get; set; }
    
    #endregion
    
    #region Conversion Information
    
    /// <summary>
    /// FUNCTIONAL: Is lead converted?
    /// TECHNICAL: Terminal state
    /// </summary>
    public bool IsConverted { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Conversion date
    /// TECHNICAL: When converted
    /// </summary>
    public DateTime? ConvertedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: User who converted
    /// TECHNICAL: FK to User
    /// </summary>
    public int? ConvertedByUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Type of conversion
    /// TECHNICAL: What was created
    /// </summary>
    public ConversionType ConversionType { get; set; } = ConversionType.NotConverted;
    
    /// <summary>
    /// FUNCTIONAL: Generated Contact ID
    /// TECHNICAL: FK to Contact table
    /// </summary>
    public int? ConvertedContactId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Generated Customer/Account ID
    /// TECHNICAL: FK to Customer table
    /// </summary>
    public int? ConvertedCustomerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Generated Opportunity ID
    /// TECHNICAL: FK to Opportunity table
    /// </summary>
    public int? ConvertedOpportunityId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Revenue from converted opportunity
    /// TECHNICAL: Attributed revenue
    /// </summary>
    public decimal? ConvertedRevenue { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Days from creation to conversion
    /// TECHNICAL: Velocity metric
    /// </summary>
    public int? DaysToConvert { get; set; }
    
    #endregion
    
    #region Disqualification Information
    
    /// <summary>
    /// FUNCTIONAL: Is lead disqualified?
    /// TECHNICAL: Removed from funnel
    /// </summary>
    public bool IsDisqualified { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Disqualification reason
    /// TECHNICAL: Why removed
    /// </summary>
    public DisqualificationReason DisqualificationReason { get; set; } = DisqualificationReason.NotSpecified;
    
    /// <summary>
    /// FUNCTIONAL: Disqualification notes
    /// TECHNICAL: Additional context
    /// </summary>
    public string? DisqualificationNotes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Disqualification date
    /// TECHNICAL: When disqualified
    /// </summary>
    public DateTime? DisqualifiedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: User who disqualified
    /// TECHNICAL: FK to User
    /// </summary>
    public int? DisqualifiedByUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Competitor chosen (if applicable)
    /// TECHNICAL: Lost to competitor
    /// </summary>
    public string? CompetitorChosen { get; set; }
    
    #endregion
    
    #region Duplicate Detection
    
    /// <summary>
    /// FUNCTIONAL: Is this a duplicate lead?
    /// TECHNICAL: Flagged for merge
    /// </summary>
    public bool IsDuplicate { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Master lead ID (if duplicate)
    /// TECHNICAL: FK to Lead
    /// </summary>
    public int? MasterLeadId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Merge date (if duplicate)
    /// TECHNICAL: When merged
    /// </summary>
    public DateTime? MergedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Duplicate check performed?
    /// TECHNICAL: Deduplication ran
    /// </summary>
    public bool DuplicateCheckPerformed { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Potential duplicate IDs
    /// TECHNICAL: JSON array of lead IDs
    /// </summary>
    public string? PotentialDuplicates { get; set; }
    
    #endregion
    
    #region Data Enrichment
    
    /// <summary>
    /// FUNCTIONAL: Data enrichment performed?
    /// TECHNICAL: Third-party enrichment
    /// </summary>
    public bool IsEnriched { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Enrichment date
    /// TECHNICAL: When enriched
    /// </summary>
    public DateTime? EnrichedDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Enrichment source
    /// TECHNICAL: Clearbit, ZoomInfo, etc.
    /// </summary>
    public string? EnrichmentSource { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Enriched company data
    /// TECHNICAL: JSON from enrichment
    /// </summary>
    public string? EnrichedCompanyData { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Enriched person data
    /// TECHNICAL: JSON from enrichment
    /// </summary>
    public string? EnrichedPersonData { get; set; }
    
    #endregion
    
    #region Documentation & Notes
    
    /// <summary>
    /// FUNCTIONAL: Lead description/summary
    /// TECHNICAL: Overview text
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Qualification notes
    /// TECHNICAL: Discovery call notes
    /// </summary>
    public string? QualificationNotes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: General notes
    /// TECHNICAL: Free-form notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Internal notes (not for lead)
    /// TECHNICAL: Team-only notes
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Tags for categorization
    /// TECHNICAL: Comma-separated
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Form submission data
    /// TECHNICAL: JSON of original form
    /// </summary>
    public string? FormData { get; set; }
    
    #endregion
    
    #region Custom Fields & Integration
    
    /// <summary>
    /// FUNCTIONAL: Custom fields
    /// TECHNICAL: JSON object for extensibility
    /// </summary>
    public string? CustomFields { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External system ID
    /// TECHNICAL: For integrations
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
    
    /// <summary>Primary campaign that generated this lead</summary>
    public MarketingCampaign? PrimaryCampaign { get; set; }
    
    /// <summary>Campaign that converted this lead</summary>
    public MarketingCampaign? ConvertingCampaign { get; set; }
    
    /// <summary>Most recent campaign</summary>
    public MarketingCampaign? LastCampaign { get; set; }
    
    /// <summary>Lead owner</summary>
    public User? Owner { get; set; }
    
    /// <summary>User who converted the lead</summary>
    public User? ConvertedByUser { get; set; }
    
    /// <summary>User who disqualified the lead</summary>
    public User? DisqualifiedByUser { get; set; }
    
    /// <summary>Master lead (if this is duplicate)</summary>
    public Lead? MasterLead { get; set; }
    
    /// <summary>Duplicate leads pointing to this as master</summary>
    public ICollection<Lead>? DuplicateLeads { get; set; }
    
    /// <summary>Generated customer from conversion</summary>
    public Customer? ConvertedCustomer { get; set; }
    
    /// <summary>Generated opportunity from conversion</summary>
    public Opportunity? ConvertedOpportunity { get; set; }
    
    /// <summary>Referring customer</summary>
    public Customer? ReferredByCustomer { get; set; }
    
    /// <summary>Primary product of interest</summary>
    public Product? PrimaryProductInterest { get; set; }
    
    /// <summary>Activities related to this lead</summary>
    public ICollection<Activity>? Activities { get; set; }
    
    /// <summary>Interactions with this lead</summary>
    public ICollection<Interaction>? Interactions { get; set; }

    /// <summary>Contact information links (addresses, phones/emails, social accounts)</summary>
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }
    
    #endregion
    
    #region Calculated Properties
    
    /// <summary>
    /// Full name of the lead
    /// </summary>
    public string FullName => $"{Salutation} {FirstName} {LastName} {Suffix}".Trim();
    
    /// <summary>
    /// Display name (Name at Company)
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(Company) 
        ? FullName 
        : $"{FirstName} {LastName} at {Company}";
    
    /// <summary>
    /// Is lead still active (not converted or disqualified)
    /// </summary>
    public bool IsActive => !IsConverted && !IsDisqualified && Status != LeadStatus.Duplicate;
    
    /// <summary>
    /// Is lead qualified (MQL or SQL)
    /// </summary>
    public bool IsQualified => IsMql || IsSql;
    
    /// <summary>
    /// Email engagement rate
    /// </summary>
    public double EmailEngagementRate => EmailsSent > 0 ? (double)EmailsOpened / EmailsSent * 100 : 0;
    
    /// <summary>
    /// Call connection rate
    /// </summary>
    public double CallConnectionRate => CallsMade > 0 ? (double)CallsConnected / CallsMade * 100 : 0;
    
    /// <summary>
    /// Age in days since creation
    /// </summary>
    public int AgeInDays => (DateTime.UtcNow - CreatedAt).Days;
    
    #endregion
}
