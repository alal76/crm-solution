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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Landing page status.
/// </summary>
public enum LandingPageStatus
{
    /// <summary>Draft - not published.</summary>
    Draft = 0,

    /// <summary>Published and accessible.</summary>
    Published = 1,

    /// <summary>Archived - no longer accessible.</summary>
    Archived = 2,

    /// <summary>Scheduled for publishing.</summary>
    Scheduled = 3,
}

/// <summary>
/// Landing page template type.
/// </summary>
public enum LandingPageTemplate
{
    /// <summary>Blank template.</summary>
    Blank = 0,

    /// <summary>Lead capture template.</summary>
    LeadCapture = 1,

    /// <summary>Product showcase template.</summary>
    ProductShowcase = 2,

    /// <summary>Event registration template.</summary>
    EventRegistration = 3,

    /// <summary>Webinar registration template.</summary>
    WebinarRegistration = 4,

    /// <summary>Ebook download template.</summary>
    EbookDownload = 5,

    /// <summary>Thank you page template.</summary>
    ThankYou = 6,
}

/// <summary>
/// Landing page entity for marketing campaigns.
/// Part of Marketing and Sales gap analysis implementation (G6).
/// </summary>
public class LandingPage : BaseEntity
{
    /// <summary>
    /// Name of the landing page for internal reference.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL slug for the landing page (e.g., "summer-promo-2025").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Page title for browser tab.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Meta description for SEO.
    /// </summary>
    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Meta keywords for SEO (comma-separated).
    /// </summary>
    [MaxLength(500)]
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Template used for the landing page.
    /// </summary>
    public LandingPageTemplate Template { get; set; } = LandingPageTemplate.Blank;

    /// <summary>
    /// Current status of the landing page.
    /// </summary>
    public LandingPageStatus Status { get; set; } = LandingPageStatus.Draft;

    /// <summary>
    /// JSON representation of the page structure (blocks and layout).
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ContentJson { get; set; }

    /// <summary>
    /// Compiled HTML content for rendering.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Custom CSS styles for the page.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? CustomCss { get; set; }

    /// <summary>
    /// Custom JavaScript for the page.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? CustomJs { get; set; }

    /// <summary>
    /// Featured image URL for social sharing.
    /// </summary>
    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    /// <summary>
    /// Facebook pixel ID for tracking.
    /// </summary>
    [MaxLength(100)]
    public string? FacebookPixelId { get; set; }

    /// <summary>
    /// Google Analytics tracking ID.
    /// </summary>
    [MaxLength(50)]
    public string? GoogleAnalyticsId { get; set; }

    /// <summary>
    /// Custom tracking code (placed in head).
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? TrackingCode { get; set; }

    /// <summary>
    /// Associated form definition for lead capture.
    /// </summary>
    public int? FormDefinitionId { get; set; }

    /// <summary>
    /// Navigation property to the form.
    /// </summary>
    [ForeignKey(nameof(FormDefinitionId))]
    public virtual FormDefinition? FormDefinition { get; set; }

    /// <summary>
    /// Associated marketing campaign.
    /// </summary>
    public int? CampaignId { get; set; }

    /// <summary>
    /// Navigation property to the campaign.
    /// </summary>
    [ForeignKey(nameof(CampaignId))]
    public virtual MarketingCampaign? Campaign { get; set; }

    /// <summary>
    /// Thank you/confirmation page after form submission.
    /// </summary>
    public int? ThankYouPageId { get; set; }

    /// <summary>
    /// Redirect URL after form submission (alternative to thank you page).
    /// </summary>
    [MaxLength(500)]
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// User who created the landing page.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Navigation property to the creator.
    /// </summary>
    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User? CreatedByUser { get; set; }

    /// <summary>
    /// Published date/time.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Scheduled publish date/time.
    /// </summary>
    public DateTime? ScheduledPublishAt { get; set; }

    /// <summary>
    /// Scheduled unpublish date/time.
    /// </summary>
    public DateTime? ScheduledUnpublishAt { get; set; }

    /// <summary>
    /// Whether the page is active (not soft-deleted or disabled).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// A/B testing variant (null for original, "A", "B", etc. for variants).
    /// </summary>
    [MaxLength(10)]
    public string? ABTestVariant { get; set; }

    /// <summary>
    /// Original landing page ID if this is an A/B variant.
    /// </summary>
    public int? OriginalPageId { get; set; }

    /// <summary>
    /// Traffic percentage for A/B testing (0-100).
    /// </summary>
    public int? ABTestTrafficPercentage { get; set; }

    /// <summary>
    /// Total page views.
    /// </summary>
    public int PageViews { get; set; } = 0;

    /// <summary>
    /// Unique visitors count.
    /// </summary>
    public int UniqueVisitors { get; set; } = 0;

    /// <summary>
    /// Total form submissions/conversions.
    /// </summary>
    public int Conversions { get; set; } = 0;

    /// <summary>
    /// Conversion rate (Conversions / UniqueVisitors * 100).
    /// </summary>
    [NotMapped]
    public decimal ConversionRate => UniqueVisitors > 0 ? (decimal)Conversions / UniqueVisitors * 100 : 0;

    /// <summary>
    /// Average time on page in seconds.
    /// </summary>
    public double AverageTimeOnPage { get; set; } = 0;

    /// <summary>
    /// Bounce rate percentage.
    /// </summary>
    public decimal BounceRate { get; set; } = 0;

    /// <summary>
    /// Additional settings as JSON.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? SettingsJson { get; set; }

    /// <summary>
    /// Navigation property to page blocks.
    /// </summary>
    public virtual ICollection<LandingPageBlock> Blocks { get; set; } = new List<LandingPageBlock>();

    /// <summary>
    /// Navigation property to page visits/analytics.
    /// </summary>
    public virtual ICollection<LandingPageVisit> Visits { get; set; } = new List<LandingPageVisit>();
}

/// <summary>
/// Block type for landing page content.
/// </summary>
public enum LandingPageBlockType
{
    /// <summary>Hero section with heading and CTA.</summary>
    Hero = 0,

    /// <summary>Text content block.</summary>
    Text = 1,

    /// <summary>Image block.</summary>
    Image = 2,

    /// <summary>Video embed block.</summary>
    Video = 3,

    /// <summary>Form embed block.</summary>
    Form = 4,

    /// <summary>Button/CTA block.</summary>
    Button = 5,

    /// <summary>Two-column layout.</summary>
    TwoColumn = 6,

    /// <summary>Three-column layout.</summary>
    ThreeColumn = 7,

    /// <summary>Feature list block.</summary>
    Features = 8,

    /// <summary>Testimonial block.</summary>
    Testimonial = 9,

    /// <summary>Pricing table block.</summary>
    Pricing = 10,

    /// <summary>FAQ accordion block.</summary>
    FAQ = 11,

    /// <summary>Social proof/logos block.</summary>
    SocialProof = 12,

    /// <summary>Countdown timer block.</summary>
    Countdown = 13,

    /// <summary>Custom HTML block.</summary>
    CustomHtml = 14,

    /// <summary>Divider/spacer block.</summary>
    Divider = 15,

    /// <summary>Navigation/header block.</summary>
    Header = 16,

    /// <summary>Footer block.</summary>
    Footer = 17,
}

/// <summary>
/// Building block for landing page content.
/// </summary>
public class LandingPageBlock : BaseEntity
{
    /// <summary>
    /// Parent landing page.
    /// </summary>
    public int LandingPageId { get; set; }

    /// <summary>
    /// Navigation property to the landing page.
    /// </summary>
    [ForeignKey(nameof(LandingPageId))]
    public virtual LandingPage? LandingPage { get; set; }

    /// <summary>
    /// Block type.
    /// </summary>
    public LandingPageBlockType BlockType { get; set; }

    /// <summary>
    /// Display order within the page.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Block content as JSON (structure depends on BlockType).
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ContentJson { get; set; }

    /// <summary>
    /// Block-specific CSS styles.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? StyleJson { get; set; }

    /// <summary>
    /// Visibility condition (show on mobile, desktop, etc.).
    /// </summary>
    [MaxLength(100)]
    public string? VisibilityCondition { get; set; }

    /// <summary>
    /// Whether the block is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Landing page visit/analytics record.
/// </summary>
public class LandingPageVisit : BaseEntity
{
    /// <summary>
    /// Parent landing page.
    /// </summary>
    public int LandingPageId { get; set; }

    /// <summary>
    /// Navigation property to the landing page.
    /// </summary>
    [ForeignKey(nameof(LandingPageId))]
    public virtual LandingPage? LandingPage { get; set; }

    /// <summary>
    /// Anonymous visitor ID (from cookie/fingerprint).
    /// </summary>
    [MaxLength(100)]
    public string? VisitorId { get; set; }

    /// <summary>
    /// Visitor IP address (hashed for privacy).
    /// </summary>
    [MaxLength(64)]
    public string? IpAddressHash { get; set; }

    /// <summary>
    /// User agent string.
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Referrer URL.
    /// </summary>
    [MaxLength(500)]
    public string? Referrer { get; set; }

    /// <summary>
    /// UTM source parameter.
    /// </summary>
    [MaxLength(200)]
    public string? UtmSource { get; set; }

    /// <summary>
    /// UTM medium parameter.
    /// </summary>
    [MaxLength(200)]
    public string? UtmMedium { get; set; }

    /// <summary>
    /// UTM campaign parameter.
    /// </summary>
    [MaxLength(200)]
    public string? UtmCampaign { get; set; }

    /// <summary>
    /// UTM term parameter.
    /// </summary>
    [MaxLength(200)]
    public string? UtmTerm { get; set; }

    /// <summary>
    /// UTM content parameter.
    /// </summary>
    [MaxLength(200)]
    public string? UtmContent { get; set; }

    /// <summary>
    /// Visit start time.
    /// </summary>
    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time spent on page in seconds.
    /// </summary>
    public int? TimeOnPageSeconds { get; set; }

    /// <summary>
    /// Whether the visitor converted (submitted form).
    /// </summary>
    public bool Converted { get; set; } = false;

    /// <summary>
    /// Conversion timestamp.
    /// </summary>
    public DateTime? ConvertedAt { get; set; }

    /// <summary>
    /// Associated lead ID if converted.
    /// </summary>
    public int? LeadId { get; set; }

    /// <summary>
    /// Device type (desktop, mobile, tablet).
    /// </summary>
    [MaxLength(20)]
    public string? DeviceType { get; set; }

    /// <summary>
    /// Browser name.
    /// </summary>
    [MaxLength(50)]
    public string? Browser { get; set; }

    /// <summary>
    /// Operating system.
    /// </summary>
    [MaxLength(50)]
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// Country (from IP geolocation).
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// City (from IP geolocation).
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }
}
