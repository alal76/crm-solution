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

namespace CRM.Core.Entities;

/// <summary>
/// Relationship status
/// </summary>
public enum RelationshipStatus
{
    Active = 0,
    Inactive = 1,
    Pending = 2,
    Terminated = 3
}

/// <summary>
/// Strategic importance level
/// </summary>
public enum StrategicImportance
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Represents a relationship between two accounts/customers
/// </summary>
public class AccountRelationship : BaseEntity
{
    /// <summary>
    /// The source (from) customer/account
    /// </summary>
    [Required]
    public int SourceAccountId { get; set; }

    /// <summary>
    /// The target (to) customer/account
    /// </summary>
    [Required]
    public int TargetAccountId { get; set; }

    /// <summary>
    /// The type of relationship
    /// </summary>
    [Required]
    public int RelationshipTypeId { get; set; }

    /// <summary>
    /// Current status of the relationship
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Strength score 0-100
    /// </summary>
    public int StrengthScore { get; set; } = 50;

    /// <summary>
    /// Strategic importance level
    /// </summary>
    [MaxLength(50)]
    public string StrategicImportance { get; set; } = "Medium";

    /// <summary>
    /// When the relationship started
    /// </summary>
    public DateTime? RelationshipStartDate { get; set; }

    /// <summary>
    /// When the relationship ended (if terminated)
    /// </summary>
    public DateTime? RelationshipEndDate { get; set; }

    /// <summary>
    /// Last review date
    /// </summary>
    public DateTime? LastReviewedDate { get; set; }

    /// <summary>
    /// Next scheduled review
    /// </summary>
    public DateTime? NextReviewDate { get; set; }

    /// <summary>
    /// Annual revenue impact of this relationship
    /// </summary>
    public decimal? AnnualRevenueImpact { get; set; }

    /// <summary>
    /// Cost savings from this relationship
    /// </summary>
    public decimal? CostSavings { get; set; }

    /// <summary>
    /// Description of the relationship
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Terms and conditions
    /// </summary>
    public string? TermsConditions { get; set; }

    /// <summary>
    /// User who last updated this record
    /// </summary>
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Account? SourceAccount { get; set; }
    public virtual Account? TargetAccount { get; set; }
    public virtual RelationshipType? RelationshipType { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual ICollection<RelationshipInteraction> Interactions { get; set; } = new List<RelationshipInteraction>();
}
