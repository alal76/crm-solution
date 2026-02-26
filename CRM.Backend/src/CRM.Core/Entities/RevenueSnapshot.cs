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
/// Represents a point-in-time snapshot of revenue metrics (ARR/MRR).
/// Used for tracking Monthly Recurring Revenue, churn, expansion, and growth over time.
/// </summary>
[Table("RevenueSnapshots")]
public class RevenueSnapshot : BaseEntity
{
    /// <summary>Date of this snapshot (usually the last day of the period).</summary>
    public DateTime SnapshotDate { get; set; }

    /// <summary>Monthly Recurring Revenue at the time of this snapshot.</summary>
    [Range(0, double.MaxValue)]
    public decimal MRR { get; set; }

    /// <summary>Annual Recurring Revenue (= MRR * 12).</summary>
    [Range(0, double.MaxValue)]
    public decimal ARR { get; set; }

    /// <summary>MRR added from brand-new customers this period.</summary>
    [Range(0, double.MaxValue)]
    public decimal NewMRR { get; set; }

    /// <summary>MRR gained from upgrades or upsells to existing customers.</summary>
    [Range(0, double.MaxValue)]
    public decimal ExpansionMRR { get; set; }

    /// <summary>MRR lost to downgrades (stored as positive number).</summary>
    [Range(0, double.MaxValue)]
    public decimal ContractionMRR { get; set; }

    /// <summary>MRR lost to cancellations (stored as positive number).</summary>
    [Range(0, double.MaxValue)]
    public decimal ChurnMRR { get; set; }

    /// <summary>Net change in MRR = NewMRR + ExpansionMRR - ContractionMRR - ChurnMRR.</summary>
    public decimal NetNewMRR { get; set; }

    /// <summary>Total number of paying customers at snapshot time.</summary>
    public int CustomerCount { get; set; }

    /// <summary>Number of new customers acquired this period.</summary>
    public int NewCustomers { get; set; }

    /// <summary>Number of customers lost (churned) this period.</summary>
    public int ChurnedCustomers { get; set; }

    /// <summary>Optional notes or context for this snapshot.</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>Snapshot granularity: "Monthly", "Weekly", or "Daily".</summary>
    [Required]
    [MaxLength(20)]
    public string SnapshotType { get; set; } = "Monthly";
}
