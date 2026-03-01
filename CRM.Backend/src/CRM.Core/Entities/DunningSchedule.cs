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
/// Dunning Schedule — configures a single step in the dunning workflow.
/// Each step fires when an invoice is overdue by <see cref="DaysOverdue"/> days.
/// Steps are ordered by <see cref="StepOrder"/> and only active steps are processed.
/// BACK-010: Dunning Scheduler CRUD
/// </summary>
[Table("DunningSchedules")]
public class DunningSchedule : BaseEntity
{
    /// <summary>Descriptive name for this dunning step (e.g. "3-Day Reminder").</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// How many days past the invoice due date triggers this step.
    /// Must be unique among active steps.
    /// </summary>
    public int DaysOverdue { get; set; }

    /// <summary>Subject line for the dunning email sent by this step.</summary>
    [Required]
    [MaxLength(500)]
    public string EmailSubject { get; set; } = string.Empty;

    /// <summary>Email body template for this step. Supports basic merge tokens.</summary>
    [Required]
    [MaxLength(10000)]
    public string EmailBody { get; set; } = string.Empty;

    /// <summary>Whether this step participates in automated processing.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Ordinal position used to sort steps (lower = earlier).</summary>
    public int StepOrder { get; set; }
}
