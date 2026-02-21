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
/// Represents a data import job request.
/// </summary>
[Table("ImportJobs")]
public class ImportJob : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Entity { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public int? SubmittedByUserId { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? TotalRecords { get; set; }

    public int? SuccessCount { get; set; }

    public int? FailureCount { get; set; }

    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    [ForeignKey("SubmittedByUserId")]
    public virtual User? SubmittedByUser { get; set; }
}
