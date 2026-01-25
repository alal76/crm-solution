// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Lead Entity - 3NF Normalized Structure

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContactModel = CRM.Core.Models.Contact;

namespace CRM.Core.Entities;

#region Lead Enumerations

/// <summary>
/// Lead lifecycle status indicating where in the funnel the lead is.
/// </summary>
public enum LeadLifecycleStatus
{
    /// <summary>New lead, not yet contacted</summary>
    New = 0,
    
    /// <summary>Lead is being actively worked</summary>
    Working = 1,
    
    /// <summary>Lead is in nurturing phase (not ready to buy)</summary>
    Nurturing = 2,
    
    /// <summary>Lead meets qualification criteria (MQL/SQL)</summary>
    Qualified = 3,
    
    /// <summary>Lead is not a fit or invalid</summary>
    Disqualified = 4,
    
    /// <summary>Converted to opportunity</summary>
    Converted = 5
}

/// <summary>
/// Original source channel of the lead.
/// </summary>
public enum LeadSource
{
    /// <summary>Website form or landing page</summary>
    Web = 0,
    
    /// <summary>Marketing campaign</summary>
    Campaign = 1,
    
    /// <summary>Customer or partner referral</summary>
    Referral = 2,
    
    /// <summary>Trade show, webinar, or conference</summary>
    Event = 3,
    
    /// <summary>Partner or channel</summary>
    Partner = 4,
    
    /// <summary>Manually entered or imported</summary>
    Manual = 5
}

#endregion

/// <summary>
/// Lead Entity (3NF Normalized)
/// Represents a potential customer before qualification.
/// 
/// Key Relationships:
/// - Account (if matched to existing company)
/// - Contact (if matched to existing person)
/// - Campaign (source marketing campaign)
/// - User (owner - SDR/Marketing)
/// - Products (via LeadProductInterest junction table)
/// </summary>
public class Lead : BaseEntity
{
    #region Lead Status & Scoring
    
    /// <summary>Lead lifecycle status</summary>
    public LeadLifecycleStatus Status { get; set; } = LeadLifecycleStatus.New;
    
    /// <summary>Original source channel</summary>
    public LeadSource Source { get; set; } = LeadSource.Web;
    
    /// <summary>Combined lead score (fit + engagement)</summary>
    public int Score { get; set; } = 0;
    
    /// <summary>Fit score based on company size, industry, persona match</summary>
    public int FitScore { get; set; } = 0;
    
    /// <summary>Engagement score based on opens, clicks, page visits, content downloads</summary>
    public int EngagementScore { get; set; } = 0;
    
    #endregion
    
    #region Qualification & Notes
    
    /// <summary>SDR/Marketing qualification notes</summary>
    [MaxLength(4000)]
    public string? QualificationNotes { get; set; }
    
    /// <summary>When lead became Marketing Qualified Lead (MQL)</summary>
    public DateTime? MqlDate { get; set; }
    
    /// <summary>When Sales accepted the lead (SQL)</summary>
    public DateTime? SqlDate { get; set; }
    
    /// <summary>Sales territory/region</summary>
    [MaxLength(100)]
    public string? Region { get; set; }
    
    /// <summary>Custom tags for categorization (JSON array)</summary>
    [MaxLength(2000)]
    public string? Tags { get; set; }
    
    #endregion
    
    #region Contact Information (Captured)
    
    /// <summary>Lead's first name</summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    /// <summary>Lead's last name</summary>
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    /// <summary>Lead's email address</summary>
    [MaxLength(255)]
    public string? Email { get; set; }
    
    /// <summary>Lead's phone number</summary>
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    /// <summary>Lead's job title</summary>
    [MaxLength(100)]
    public string? Title { get; set; }
    
    /// <summary>Lead's company name (if not matched to Account)</summary>
    [MaxLength(255)]
    public string? CompanyName { get; set; }
    
    /// <summary>Lead's company website</summary>
    [MaxLength(500)]
    public string? Website { get; set; }
    
    #endregion
    
    #region Foreign Keys
    
    /// <summary>Lead owner (SDR/Marketing user)</summary>
    public int? OwnerId { get; set; }
    
    /// <summary>Source marketing campaign</summary>
    public int? CampaignId { get; set; }
    
    /// <summary>Matched existing account (if applicable)</summary>
    public int? AccountId { get; set; }
    
    /// <summary>Matched existing contact (if applicable)</summary>
    public int? ContactId { get; set; }
    
    #endregion
    
    #region Navigation Properties
    
    /// <summary>Lead owner user</summary>
    [ForeignKey("OwnerId")]
    public virtual User? Owner { get; set; }
    
    /// <summary>Source marketing campaign</summary>
    [ForeignKey("CampaignId")]
    public virtual MarketingCampaign? Campaign { get; set; }
    
    /// <summary>Matched account (company)</summary>
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }
    
    /// <summary>Matched contact (person)</summary>
    [ForeignKey("ContactId")]
    public virtual ContactModel? Contact { get; set; }
    
    /// <summary>Products/services of interest (many-to-many)</summary>
    public virtual ICollection<LeadProductInterest> ProductInterests { get; set; } = new List<LeadProductInterest>();
    
    /// <summary>Opportunities created from this lead</summary>
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    
    #endregion
    
    #region Computed Properties
    
    /// <summary>Full name of the lead</summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    /// <summary>Whether the lead is still open (not converted or disqualified)</summary>
    [NotMapped]
    public bool IsOpen => Status != LeadLifecycleStatus.Converted && Status != LeadLifecycleStatus.Disqualified;
    
    #endregion
}

/// <summary>
/// Junction table for Lead-Product many-to-many relationship.
/// Tracks which products/services a lead is interested in.
/// </summary>
[Table("LeadProductInterests")]
public class LeadProductInterest
{
    /// <summary>Lead ID</summary>
    public int LeadId { get; set; }
    
    /// <summary>Product ID</summary>
    public int ProductId { get; set; }
    
    /// <summary>When the interest was recorded</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Interest level (1-5)</summary>
    public int? InterestLevel { get; set; }
    
    /// <summary>Notes about the product interest</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    #region Navigation Properties
    
    /// <summary>Lead navigation</summary>
    [ForeignKey("LeadId")]
    public virtual Lead Lead { get; set; } = null!;
    
    /// <summary>Product navigation</summary>
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
    
    #endregion
}
