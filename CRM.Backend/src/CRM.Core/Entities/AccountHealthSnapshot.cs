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

using System.ComponentModel.DataAnnotations.Schema;
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Health trend direction
/// </summary>
public enum HealthTrend
{
    Improving = 0,
    Stable = 1,
    Declining = 2,
    Critical = 3
}

/// <summary>
/// Periodic snapshots of account health metrics
/// </summary>
public class AccountHealthSnapshot : BaseEntity
{
    /// <summary>
    /// The customer this snapshot is for
    /// </summary>
    [Required]
    [Column("CustomerId")]
    public int AccountId { get; set; }

    /// <summary>
    /// Date of the snapshot
    /// </summary>
    [Required]
    public DateTime SnapshotDate { get; set; }

    /// <summary>
    /// Overall health score (0-100)
    /// </summary>
    public int OverallHealthScore { get; set; } = 0;

    /// <summary>
    /// Engagement score (0-100)
    /// </summary>
    public int EngagementScore { get; set; } = 0;

    /// <summary>
    /// Product adoption score (0-100)
    /// </summary>
    public int ProductAdoptionScore { get; set; } = 0;

    /// <summary>
    /// Support satisfaction score (0-100)
    /// </summary>
    public int SupportSatisfactionScore { get; set; } = 0;

    /// <summary>
    /// Financial health score (0-100)
    /// </summary>
    public int FinancialHealthScore { get; set; } = 0;

    /// <summary>
    /// Relationship score (0-100)
    /// </summary>
    public int RelationshipScore { get; set; } = 0;

    /// <summary>
    /// Number of active users
    /// </summary>
    public int? ActiveUsersCount { get; set; }

    /// <summary>
    /// Feature adoption rate percentage
    /// </summary>
    public decimal? FeatureAdoptionRate { get; set; }

    /// <summary>
    /// Number of support tickets in period
    /// </summary>
    public int? SupportTicketsCount { get; set; }

    /// <summary>
    /// Number of resolved tickets
    /// </summary>
    public int? SupportTicketsResolved { get; set; }

    /// <summary>
    /// Average support response time in hours
    /// </summary>
    public decimal? AverageResponseTimeHours { get; set; }

    /// <summary>
    /// Net Promoter Score (-100 to 100)
    /// </summary>
    public int? NPSScore { get; set; }

    /// <summary>
    /// Risk factors as JSON array
    /// </summary>
    public string? RiskFactors { get; set; }

    /// <summary>
    /// Warning signals as JSON array
    /// </summary>
    public string? WarningSignals { get; set; }

    /// <summary>
    /// Growth indicators as JSON array
    /// </summary>
    public string? GrowthIndicators { get; set; }

    /// <summary>
    /// Analyst notes
    /// </summary>
    public string? AnalystNotes { get; set; }

    /// <summary>
    /// Previous period health score for comparison
    /// </summary>
    public int? PreviousHealthScore { get; set; }

    /// <summary>
    /// Current health trend
    /// </summary>
    [MaxLength(20)]
    public string HealthTrend { get; set; } = "Stable";

    // Navigation property
    public virtual Account? Account { get; set; }
}
