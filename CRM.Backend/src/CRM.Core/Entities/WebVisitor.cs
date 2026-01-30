using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Web Visitor Enumerations

/// <summary>
/// FUNCTIONAL: Source of web visitor identification.
/// TECHNICAL: Determines how visitor was linked to record.
/// </summary>
public enum VisitorIdentificationSource
{
    /// <summary>Anonymous visitor, not yet identified</summary>
    Anonymous = 0,
    
    /// <summary>Identified via form submission</summary>
    FormSubmission = 1,
    
    /// <summary>Identified via email click</summary>
    EmailClick = 2,
    
    /// <summary>Identified via login</summary>
    Login = 3,
    
    /// <summary>Identified via chat</summary>
    Chat = 4,
    
    /// <summary>Identified via cookie match</summary>
    Cookie = 5,
    
    /// <summary>Identified via IP/company lookup</summary>
    CompanyLookup = 6,
    
    /// <summary>Identified via social sign-in</summary>
    Social = 7,
    
    /// <summary>Manually matched</summary>
    Manual = 8
}

/// <summary>
/// FUNCTIONAL: Type of page visited.
/// TECHNICAL: Used for scoring and segmentation.
/// </summary>
public enum PageCategory
{
    /// <summary>Home/landing page</summary>
    Home = 0,
    
    /// <summary>Product pages</summary>
    Product = 1,
    
    /// <summary>Pricing page</summary>
    Pricing = 2,
    
    /// <summary>Features page</summary>
    Features = 3,
    
    /// <summary>Blog/content</summary>
    Blog = 4,
    
    /// <summary>Case study</summary>
    CaseStudy = 5,
    
    /// <summary>Documentation</summary>
    Documentation = 6,
    
    /// <summary>Demo request</summary>
    Demo = 7,
    
    /// <summary>Contact page</summary>
    Contact = 8,
    
    /// <summary>About/company</summary>
    About = 9,
    
    /// <summary>Careers</summary>
    Careers = 10,
    
    /// <summary>Thank you page</summary>
    ThankYou = 11,
    
    /// <summary>Other/uncategorized</summary>
    Other = 12
}

#endregion

/// <summary>
/// Web visitor tracking entity for behavioral analytics.
/// Tracks anonymous and identified visitors across sessions.
/// </summary>
public class WebVisitor : BaseEntity
{
    #region Identification
    
    /// <summary>Unique visitor ID (cookie-based)</summary>
    public string VisitorId { get; set; } = string.Empty;
    
    /// <summary>Fingerprint ID for device identification</summary>
    public string? FingerprintId { get; set; }
    
    /// <summary>Whether visitor has been identified</summary>
    public bool IsIdentified { get; set; } = false;
    
    /// <summary>How visitor was identified</summary>
    public VisitorIdentificationSource IdentificationSource { get; set; } = VisitorIdentificationSource.Anonymous;
    
    /// <summary>Date visitor was identified</summary>
    public DateTime? IdentifiedAt { get; set; }
    
    #endregion
    
    #region Contact Information (when identified)
    
    /// <summary>Email address</summary>
    public string? Email { get; set; }
    
    /// <summary>First name</summary>
    public string? FirstName { get; set; }
    
    /// <summary>Last name</summary>
    public string? LastName { get; set; }
    
    /// <summary>Phone number</summary>
    public string? Phone { get; set; }
    
    #endregion
    
    #region Company Information
    
    /// <summary>Company name (from IP lookup or form)</summary>
    public string? Company { get; set; }
    
    /// <summary>Industry</summary>
    public string? Industry { get; set; }
    
    /// <summary>Company size</summary>
    public string? CompanySize { get; set; }
    
    /// <summary>Company domain</summary>
    public string? CompanyDomain { get; set; }
    
    #endregion
    
    #region Geographic Information
    
    /// <summary>IP address (last known)</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>Country</summary>
    public string? Country { get; set; }
    
    /// <summary>Country code (ISO 3166-1)</summary>
    public string? CountryCode { get; set; }
    
    /// <summary>Region/state</summary>
    public string? Region { get; set; }
    
    /// <summary>City</summary>
    public string? City { get; set; }
    
    /// <summary>Postal/ZIP code</summary>
    public string? PostalCode { get; set; }
    
    /// <summary>Timezone</summary>
    public string? Timezone { get; set; }
    
    /// <summary>Latitude</summary>
    public decimal? Latitude { get; set; }
    
    /// <summary>Longitude</summary>
    public decimal? Longitude { get; set; }
    
    #endregion
    
    #region Device Information
    
    /// <summary>Browser name</summary>
    public string? Browser { get; set; }
    
    /// <summary>Browser version</summary>
    public string? BrowserVersion { get; set; }
    
    /// <summary>Operating system</summary>
    public string? OperatingSystem { get; set; }
    
    /// <summary>Device type (desktop, mobile, tablet)</summary>
    public string? DeviceType { get; set; }
    
    /// <summary>Screen resolution</summary>
    public string? ScreenResolution { get; set; }
    
    /// <summary>User agent string</summary>
    public string? UserAgent { get; set; }
    
    /// <summary>Language preference</summary>
    public string? Language { get; set; }
    
    #endregion
    
    #region Traffic Source (First Touch)
    
    /// <summary>Original referrer URL</summary>
    public string? FirstReferrer { get; set; }
    
    /// <summary>Original landing page</summary>
    public string? FirstLandingPage { get; set; }
    
    /// <summary>Original UTM source</summary>
    public string? FirstUtmSource { get; set; }
    
    /// <summary>Original UTM medium</summary>
    public string? FirstUtmMedium { get; set; }
    
    /// <summary>Original UTM campaign</summary>
    public string? FirstUtmCampaign { get; set; }
    
    /// <summary>Original UTM content</summary>
    public string? FirstUtmContent { get; set; }
    
    /// <summary>Original UTM term</summary>
    public string? FirstUtmTerm { get; set; }
    
    #endregion
    
    #region Traffic Source (Last Touch)
    
    /// <summary>Most recent referrer</summary>
    public string? LastReferrer { get; set; }
    
    /// <summary>Most recent landing page</summary>
    public string? LastLandingPage { get; set; }
    
    /// <summary>Most recent UTM source</summary>
    public string? LastUtmSource { get; set; }
    
    /// <summary>Most recent UTM medium</summary>
    public string? LastUtmMedium { get; set; }
    
    /// <summary>Most recent UTM campaign</summary>
    public string? LastUtmCampaign { get; set; }
    
    /// <summary>Most recent UTM content</summary>
    public string? LastUtmContent { get; set; }
    
    /// <summary>Most recent UTM term</summary>
    public string? LastUtmTerm { get; set; }
    
    #endregion
    
    #region Engagement Metrics
    
    /// <summary>First visit date</summary>
    public DateTime FirstVisitAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Last visit date</summary>
    public DateTime LastVisitAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Total sessions</summary>
    public int TotalSessions { get; set; } = 1;
    
    /// <summary>Total page views</summary>
    public int TotalPageViews { get; set; } = 0;
    
    /// <summary>Total time on site (seconds)</summary>
    public int TotalTimeOnSite { get; set; } = 0;
    
    /// <summary>Average pages per session</summary>
    public decimal AveragePagePerSession => TotalSessions > 0 ? (decimal)TotalPageViews / TotalSessions : 0;
    
    /// <summary>Forms submitted count</summary>
    public int FormsSubmitted { get; set; } = 0;
    
    /// <summary>Files downloaded count</summary>
    public int FilesDownloaded { get; set; } = 0;
    
    /// <summary>Videos watched count</summary>
    public int VideosWatched { get; set; } = 0;
    
    #endregion
    
    #region Scoring
    
    /// <summary>Behavioral score based on activity</summary>
    public int BehaviorScore { get; set; } = 0;
    
    /// <summary>Fit score based on company/profile</summary>
    public int FitScore { get; set; } = 0;
    
    /// <summary>Combined lead score</summary>
    public int TotalScore { get; set; } = 0;
    
    /// <summary>Interest topics (from page visits)</summary>
    public string? InterestTopics { get; set; } // JSON array
    
    /// <summary>Buying stage (awareness, consideration, decision)</summary>
    public string? BuyingStage { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Linked lead ID</summary>
    public int? LeadId { get; set; }
    
    /// <summary>Navigation to lead</summary>
    public Lead? Lead { get; set; }
    
    /// <summary>Linked contact ID</summary>
    public int? ContactId { get; set; }
    
    /// <summary>Navigation to contact</summary>
    public Contact? Contact { get; set; }
    
    /// <summary>Linked account ID (from company lookup)</summary>
    public int? AccountId { get; set; }
    
    /// <summary>Navigation to account</summary>
    public Account? Account { get; set; }
    
    /// <summary>Page views</summary>
    public ICollection<WebPageView> PageViews { get; set; } = new List<WebPageView>();
    
    /// <summary>Sessions</summary>
    public ICollection<WebSession> Sessions { get; set; } = new List<WebSession>();
    
    /// <summary>Form submissions</summary>
    public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    
    #endregion
}

/// <summary>
/// Individual web session for a visitor.
/// </summary>
public class WebSession : BaseEntity
{
    /// <summary>Session ID</summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>Session start time</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Session end time</summary>
    public DateTime? EndedAt { get; set; }
    
    /// <summary>Session duration (seconds)</summary>
    public int Duration { get; set; } = 0;
    
    /// <summary>Page views in this session</summary>
    public int PageViewCount { get; set; } = 0;
    
    /// <summary>Landing page URL</summary>
    public string? LandingPage { get; set; }
    
    /// <summary>Exit page URL</summary>
    public string? ExitPage { get; set; }
    
    /// <summary>Referrer URL</summary>
    public string? Referrer { get; set; }
    
    /// <summary>UTM parameters (JSON)</summary>
    public string? UtmParameters { get; set; }
    
    /// <summary>IP address for this session</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>Parent visitor ID</summary>
    public int WebVisitorId { get; set; }
    
    /// <summary>Navigation to visitor</summary>
    public WebVisitor? WebVisitor { get; set; }
    
    /// <summary>Page views in this session</summary>
    public ICollection<WebPageView> PageViews { get; set; } = new List<WebPageView>();
}

/// <summary>
/// Individual page view within a session.
/// </summary>
public class WebPageView : BaseEntity
{
    /// <summary>Page URL</summary>
    public string PageUrl { get; set; } = string.Empty;
    
    /// <summary>Page path (without domain)</summary>
    public string? PagePath { get; set; }
    
    /// <summary>Page title</summary>
    public string? PageTitle { get; set; }
    
    /// <summary>Page category</summary>
    public PageCategory Category { get; set; } = PageCategory.Other;
    
    /// <summary>View timestamp</summary>
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Time on page (seconds)</summary>
    public int TimeOnPage { get; set; } = 0;
    
    /// <summary>Scroll depth percentage (0-100)</summary>
    public int? ScrollDepth { get; set; }
    
    /// <summary>Referrer URL</summary>
    public string? Referrer { get; set; }
    
    /// <summary>Query parameters (JSON)</summary>
    public string? QueryParameters { get; set; }
    
    /// <summary>Parent visitor ID</summary>
    public int WebVisitorId { get; set; }
    
    /// <summary>Navigation to visitor</summary>
    public WebVisitor? WebVisitor { get; set; }
    
    /// <summary>Session ID</summary>
    public int? WebSessionId { get; set; }
    
    /// <summary>Navigation to session</summary>
    public WebSession? WebSession { get; set; }
}
