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
using ContactModel = CRM.Core.Models.Contact;

namespace CRM.Core.Entities;

#region Opportunity Enumerations

/// <summary>
/// Sales pipeline stages an opportunity progresses through.
/// </summary>
public enum OpportunityStage
{
    /// <summary>Initial discovery phase (10% probability)</summary>
    Discovery = 0,

    /// <summary>Qualifying using BANT criteria (25% probability)</summary>
    Qualification = 1,

    /// <summary>Formal proposal or quote submitted (50% probability)</summary>
    Proposal = 2,

    /// <summary>Active negotiation on terms and pricing (75% probability)</summary>
    Negotiation = 3,

    /// <summary>Deal successfully closed (100% probability)</summary>
    ClosedWon = 4,

    /// <summary>Deal lost to competition or no decision (0% probability)</summary>
    ClosedLost = 5
}

/// <summary>
/// Qualification reason based on BANT methodology.
/// </summary>
public enum QualificationReason
{
    /// <summary>Has budget available</summary>
    Budget = 0,

    /// <summary>Has clear business need</summary>
    Need = 1,

    /// <summary>Timeline aligns with sales cycle</summary>
    Timing = 2,

    /// <summary>Decision maker involved</summary>
    Authority = 3,

    /// <summary>Product/service is good fit</summary>
    Fit = 4
}

/// <summary>
/// Pricing model for opportunities (aligned with Product pricing).
/// </summary>
public enum OpportunityPricingModel
{
    /// <summary>Recurring subscription (monthly/annual)</summary>
    Subscription = 0,

    /// <summary>One-time purchase</summary>
    OneTime = 1,

    /// <summary>Usage-based or consumption pricing</summary>
    UsageBased = 2,

    /// <summary>Hybrid model (subscription + usage)</summary>
    Hybrid = 3
}

#endregion

/// <summary>
/// Opportunity Entity (3NF Normalized)
/// Created when a Lead is converted. Represents a potential deal.
///
/// Key Relationships:
/// - Account (customer/organization)
/// - Contact (primary contact)
/// - User (sales owner - AE)
/// - Lead (original source lead)
/// - Products (via OpportunityProduct junction table)
/// </summary>
public class Opportunity : BaseEntity
{
    #region Identification

    /// <summary>Opportunity name (usually Account - Product)</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    #endregion

    #region Pipeline & Probability

    /// <summary>Current sales stage</summary>
    public OpportunityStage Stage { get; set; } = OpportunityStage.Discovery;

    /// <summary>Probability of closing (0-100%)</summary>
    [Range(0, 100)]
    public int Probability { get; set; } = 10;

    #endregion

    #region Commercial Fields

    /// <summary>Estimated deal value</summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } = 0;

    /// <summary>Currency code (USD, EUR, GBP, etc.)</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>Forecasted close date</summary>
    public DateTime? ExpectedCloseDate { get; set; }

    /// <summary>Pricing model for the deal</summary>
    public OpportunityPricingModel PricingModel { get; set; } = OpportunityPricingModel.Subscription;

    /// <summary>Contract term in months</summary>
    [Range(1, 120)]
    public int TermLengthMonths { get; set; } = 12;

    #endregion

    #region Solution & Qualification

    /// <summary>Proposed solution details</summary>
    [MaxLength(4000)]
    public string? SolutionNotes { get; set; }

    /// <summary>Primary qualification reason (BANT)</summary>
    public QualificationReason? QualificationReason { get; set; }

    /// <summary>SDR to AE handoff qualification notes</summary>
    [MaxLength(4000)]
    public string? QualificationNotes { get; set; }

    /// <summary>Sales territory/region</summary>
    [MaxLength(100)]
    public string? Region { get; set; }

    #endregion

    #region Foreign Keys

    /// <summary>Customer/company for this opportunity (required)</summary>
    [Column("CustomerId")]
    public int AccountId { get; set; }

    /// <summary>Primary contact person</summary>
    public int? PrimaryContactId { get; set; }

    /// <summary>Assigned sales owner (AE)</summary>
    public int? SalesOwnerId { get; set; }

    /// <summary>Original lead that was converted</summary>
    public int? LeadId { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>Customer/company</summary>
    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    /// <summary>Primary contact</summary>
    [ForeignKey("PrimaryContactId")]
    public virtual ContactModel? PrimaryContact { get; set; }

    /// <summary>Sales owner (Account Executive)</summary>
    [ForeignKey("SalesOwnerId")]
    public virtual User? SalesOwner { get; set; }

    /// <summary>Source lead</summary>
    [ForeignKey("LeadId")]
    public virtual Lead? Lead { get; set; }

    /// <summary>Products included in the deal (many-to-many)</summary>
    public virtual ICollection<OpportunityProduct> Products { get; set; } = new List<OpportunityProduct>();

    #endregion

    #region Computed Properties

    /// <summary>Whether the opportunity is still open</summary>
    [NotMapped]
    public bool IsOpen => Stage != OpportunityStage.ClosedWon && Stage != OpportunityStage.ClosedLost;

    /// <summary>Whether the opportunity was won</summary>
    [NotMapped]
    public bool IsWon => Stage == OpportunityStage.ClosedWon;

    /// <summary>Weighted amount (Amount * Probability / 100)</summary>
    [NotMapped]
    public decimal WeightedAmount => Amount * Probability / 100;

    #endregion

    #region Aliases

    /// <summary>Alias for Amount for service compatibility</summary>
    [NotMapped]
    public decimal EstimatedValue
    {
        get => Amount;
        set => Amount = value;
    }

    #endregion
}

/// <summary>
/// Junction table for Opportunity-Product many-to-many relationship.
/// Tracks which products are included in the deal with quantity.
/// </summary>
[Table("OpportunityProducts")]
public class OpportunityProduct
{
    /// <summary>Opportunity ID</summary>
    public int OpportunityId { get; set; }

    /// <summary>Product ID</summary>
    public int ProductId { get; set; }

    /// <summary>Quantity/units/seats</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Unit price (may differ from product list price)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }

    /// <summary>Discount percentage</summary>
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }

    /// <summary>Line total (Quantity * UnitPrice after discount)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? LineTotal { get; set; }

    /// <summary>Alias for LineTotal for service compatibility</summary>
    [NotMapped]
    public decimal? TotalPrice
    {
        get => LineTotal;
        set => LineTotal = value;
    }

    /// <summary>When the product was added to the opportunity</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Notes about this product line</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Soft delete flag</summary>
    public bool IsDeleted { get; set; } = false;

    #region Navigation Properties

    /// <summary>Opportunity navigation</summary>
    [ForeignKey("OpportunityId")]
    public virtual Opportunity Opportunity { get; set; } = null!;

    /// <summary>Product navigation</summary>
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    #endregion
}
