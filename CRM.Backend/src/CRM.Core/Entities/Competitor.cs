// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Threat level of competitor in a deal.
/// TODO-CRM003-03: Competitor tracking on opportunities
/// </summary>
public enum CompetitorThreatLevel
{
    /// <summary>Unknown threat level</summary>
    Unknown = 0,
    
    /// <summary>Low threat - not a serious contender</summary>
    Low = 1,
    
    /// <summary>Medium threat - competitive presence</summary>
    Medium = 2,
    
    /// <summary>High threat - actively competing</summary>
    High = 3,
    
    /// <summary>Critical threat - likely to win</summary>
    Critical = 4
}

/// <summary>
/// Status of competitor in an opportunity.
/// </summary>
public enum OpportunityCompetitorStatus
{
    /// <summary>Competitor identified</summary>
    Identified = 0,
    
    /// <summary>Competitor is actively competing</summary>
    Active = 1,
    
    /// <summary>Competitor is leading</summary>
    Leading = 2,
    
    /// <summary>Competitor has been eliminated</summary>
    Eliminated = 3,
    
    /// <summary>Competitor won the deal</summary>
    Won = 4
}

/// <summary>
/// Competitor entity for tracking competitive landscape.
/// TODO-CRM003-03: Competitor tracking on opportunities
/// </summary>
[Table("Competitors")]
public class Competitor : BaseEntity
{
    /// <summary>Competitor company name</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Short description of competitor</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>Competitor website</summary>
    [MaxLength(500)]
    public string? Website { get; set; }
    
    /// <summary>Industry segment</summary>
    [MaxLength(100)]
    public string? Industry { get; set; }
    
    /// <summary>Key strengths of competitor</summary>
    [MaxLength(2000)]
    public string? Strengths { get; set; }
    
    /// <summary>Key weaknesses of competitor</summary>
    [MaxLength(2000)]
    public string? Weaknesses { get; set; }
    
    /// <summary>Our competitive advantages against this competitor</summary>
    [MaxLength(2000)]
    public string? OurAdvantages { get; set; }
    
    /// <summary>Primary products/services they offer</summary>
    [MaxLength(1000)]
    public string? PrimaryProducts { get; set; }
    
    /// <summary>Approximate pricing tier (Low, Medium, High, Premium)</summary>
    [MaxLength(50)]
    public string? PricingTier { get; set; }
    
    /// <summary>Estimated market share percentage</summary>
    [Range(0, 100)]
    public decimal? MarketSharePercent { get; set; }
    
    /// <summary>Whether this is an active competitor</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Win rate against this competitor</summary>
    [Range(0, 100)]
    public decimal? WinRateAgainst { get; set; }
    
    /// <summary>Internal notes about competitor</summary>
    [MaxLength(4000)]
    public string? Notes { get; set; }
    
    #region Navigation Properties
    
    /// <summary>Opportunities where this competitor is involved</summary>
    public virtual ICollection<OpportunityCompetitor> OpportunityCompetitors { get; set; } = new List<OpportunityCompetitor>();
    
    #endregion
}

/// <summary>
/// Junction table linking opportunities to competitors.
/// TODO-CRM003-03: Competitor tracking on opportunities
/// </summary>
[Table("OpportunityCompetitors")]
public class OpportunityCompetitor
{
    /// <summary>Opportunity ID</summary>
    public int OpportunityId { get; set; }
    
    /// <summary>Competitor ID</summary>
    public int CompetitorId { get; set; }
    
    /// <summary>Threat level of this competitor in the deal</summary>
    public CompetitorThreatLevel ThreatLevel { get; set; } = CompetitorThreatLevel.Medium;
    
    /// <summary>Status of competitor in opportunity</summary>
    public OpportunityCompetitorStatus Status { get; set; } = OpportunityCompetitorStatus.Identified;
    
    /// <summary>Competitor's quoted price (if known)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CompetitorPrice { get; set; }
    
    /// <summary>When competitor was identified</summary>
    public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Notes about competitor for this specific deal</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>Did we win or lose against this competitor?</summary>
    public bool? WonAgainst { get; set; }
    
    #region Navigation Properties
    
    /// <summary>Opportunity navigation</summary>
    [ForeignKey("OpportunityId")]
    public virtual Opportunity Opportunity { get; set; } = null!;
    
    /// <summary>Competitor navigation</summary>
    [ForeignKey("CompetitorId")]
    public virtual Competitor Competitor { get; set; } = null!;
    
    #endregion
}
