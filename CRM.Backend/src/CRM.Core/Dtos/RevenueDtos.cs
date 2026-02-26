// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>Full read model for a single revenue snapshot.</summary>
public class RevenueSnapshotDto
{
    public int Id { get; set; }
    public DateTime SnapshotDate { get; set; }
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public decimal NewMRR { get; set; }
    public decimal ExpansionMRR { get; set; }
    public decimal ContractionMRR { get; set; }
    public decimal ChurnMRR { get; set; }
    public decimal NetNewMRR { get; set; }
    public int CustomerCount { get; set; }
    public int NewCustomers { get; set; }
    public int ChurnedCustomers { get; set; }
    public string? Notes { get; set; }
    public string SnapshotType { get; set; } = "Monthly";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Aggregated revenue metrics including trend and KPIs.</summary>
public class RevenueMetricsDto
{
    /// <summary>Most recent MRR value.</summary>
    public decimal CurrentMRR { get; set; }

    /// <summary>Most recent ARR value (= CurrentMRR * 12).</summary>
    public decimal CurrentARR { get; set; }

    /// <summary>Month-over-month MRR growth percentage.</summary>
    public decimal MoMGrowthRate { get; set; }

    /// <summary>Churn rate as a percentage of previous period customers.</summary>
    public decimal ChurnRate { get; set; }

    /// <summary>Expansion MRR as a percentage of prior-period MRR.</summary>
    public decimal ExpansionRate { get; set; }

    /// <summary>Net Revenue Retention = (MRR + Expansion - Contraction - Churn) / MRR * 100.</summary>
    public decimal NetRevenueRetention { get; set; }

    /// <summary>Current total paying customers.</summary>
    public int TotalCustomers { get; set; }

    /// <summary>Average MRR per paying customer.</summary>
    public decimal AverageRevenuePerCustomer { get; set; }

    /// <summary>Trend of the last 12 monthly snapshots (ascending by date).</summary>
    public List<RevenueSnapshotDto> Trend { get; set; } = new();
}

/// <summary>MRR movement breakdown for a single period (waterfall chart data).</summary>
public class RevenueMRRMovementDto
{
    public DateTime Period { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal OpeningMRR { get; set; }
    public decimal NewMRR { get; set; }
    public decimal ExpansionMRR { get; set; }
    public decimal ContractionMRR { get; set; }
    public decimal ChurnMRR { get; set; }
    public decimal ClosingMRR { get; set; }
}

/// <summary>DTO for manually creating a revenue snapshot.</summary>
public class CreateRevenueSnapshotDto
{
    [Required]
    public DateTime SnapshotDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MRR { get; set; }

    [Range(0, double.MaxValue)]
    public decimal NewMRR { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ExpansionMRR { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ContractionMRR { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ChurnMRR { get; set; }

    public int CustomerCount { get; set; }
    public int NewCustomers { get; set; }
    public int ChurnedCustomers { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string SnapshotType { get; set; } = "Monthly";
}
