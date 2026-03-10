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
/// Role of team member in an opportunity.
/// TODO-CRM003-08: Opportunity team/split commission tracking
/// </summary>
public enum OpportunityTeamRole
{
    /// <summary>Account Executive - primary owner</summary>
    AccountExecutive = 0,

    /// <summary>Sales Development Representative</summary>
    SDR = 1,

    /// <summary>Sales Engineer / Solution Architect</summary>
    SalesEngineer = 2,

    /// <summary>Account Manager</summary>
    AccountManager = 3,

    /// <summary>Sales Manager / Overlay</summary>
    SalesManager = 4,

    /// <summary>Partner representative</summary>
    Partner = 5,

    /// <summary>Executive sponsor</summary>
    ExecutiveSponsor = 6,

    /// <summary>Customer Success</summary>
    CustomerSuccess = 7,

    /// <summary>Other role</summary>
    Other = 99
}

/// <summary>
/// Opportunity team member for tracking who is involved in a deal
/// and managing commission splits.
/// TODO-CRM003-08: Opportunity team/split commission tracking
/// </summary>
[Table("OpportunityTeamMembers")]
public class OpportunityTeamMember : BaseEntity
{
    /// <summary>Opportunity ID</summary>
    public int OpportunityId { get; set; }

    /// <summary>User ID of the team member</summary>
    public int UserId { get; set; }

    /// <summary>Role in the opportunity</summary>
    public OpportunityTeamRole Role { get; set; } = OpportunityTeamRole.Other;

    /// <summary>Commission split percentage (0-100)</summary>
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal SplitPercentage { get; set; } = 0;

    /// <summary>Whether this is the primary owner</summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>Date added to opportunity</summary>
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    /// <summary>Date removed from opportunity (if applicable)</summary>
    public DateTime? DateRemoved { get; set; }

    /// <summary>Reason for involvement</summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>Notes about this team member's contribution</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>Commission plan to use for this team member (overrides default)</summary>
    public int? CommissionPlanId { get; set; }

    #region Navigation Properties

    /// <summary>Opportunity navigation</summary>
    [ForeignKey("OpportunityId")]
    public virtual Opportunity Opportunity { get; set; } = null!;

    /// <summary>User navigation</summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>Commission plan navigation</summary>
    [ForeignKey("CommissionPlanId")]
    public virtual CommissionPlan? CommissionPlan { get; set; }

    #endregion
}
