// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Entities.Reports;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a report sharing configuration between a report and a user.
/// Enables collaborative report access with permission levels.
/// TODO-RPT-03
/// </summary>
[Table("ReportShares")]
public class ReportShare : BaseEntity
{
    /// <summary>
    /// The ID of the report being shared.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// The ID of the user the report is shared with.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Permission level: View, Edit, Admin.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Permission { get; set; } = "View";

    /// <summary>
    /// The ID of the user who shared the report.
    /// </summary>
    public int SharedByUserId { get; set; }

    /// <summary>
    /// Optional expiration date for the share.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Notes about the share.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties

    /// <summary>
    /// The report being shared.
    /// </summary>
    [ForeignKey(nameof(ReportId))]
    public virtual ReportDefinition? Report { get; set; }

    /// <summary>
    /// The user the report is shared with.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// The user who created the share.
    /// </summary>
    [ForeignKey(nameof(SharedByUserId))]
    public virtual User? SharedByUser { get; set; }
}
