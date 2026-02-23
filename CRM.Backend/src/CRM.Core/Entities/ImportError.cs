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
/// Tracked error from an import job.
/// Each record represents a single validation or processing error for a specific row/field.
/// </summary>
[Table("ImportErrors")]
public class ImportError : BaseEntity
{
    public int ImportJobId { get; set; }

    public int RowNumber { get; set; }

    [Required]
    [StringLength(100)]
    public string Field { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string ErrorMessage { get; set; } = string.Empty;

    [StringLength(500)]
    public string? RawValue { get; set; }

    /// <summary>
    /// Severity of the error: Error, Warning, Info
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Severity { get; set; } = "Error";

    // Navigation
    [ForeignKey("ImportJobId")]
    public virtual ImportJob? ImportJob { get; set; }
}
