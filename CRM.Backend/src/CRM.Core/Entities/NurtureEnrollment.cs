// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Tracks a lead or contact's enrolment in an email nurture sequence.
/// Each record represents one enrollee progressing through a sequence.
/// </summary>
public class NurtureEnrollment : BaseEntity
{
    /// <summary>FK to the parent email sequence.</summary>
    public int SequenceId { get; set; }

    /// <summary>Navigation: the parent email sequence.</summary>
    public EmailSequence Sequence { get; set; } = null!;

    /// <summary>FK to the lead (if enrolled as a lead).</summary>
    public int? LeadId { get; set; }

    /// <summary>FK to the contact (if enrolled as a contact).</summary>
    public int? ContactId { get; set; }

    /// <summary>Email address of the enrollee.</summary>
    [Required]
    [MaxLength(320)]
    public string EnrolleeEmail { get; set; } = string.Empty;

    /// <summary>Display name of the enrollee.</summary>
    [MaxLength(200)]
    public string? EnrolleeName { get; set; }

    /// <summary>What caused this enrolment.</summary>
    public NurtureEnrollmentTrigger Trigger { get; set; }

    /// <summary>Zero-based index of the step the enrollee is currently on.</summary>
    public int CurrentStep { get; set; } = 0;

    /// <summary>UTC timestamp when the next step should be processed.</summary>
    public DateTime? NextStepAt { get; set; }

    /// <summary>Whether the enrollee has completed all steps.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Whether the enrollee has opted out.</summary>
    public bool IsUnsubscribed { get; set; }

    /// <summary>UTC timestamp when the enrollee was completed.</summary>
    public DateTime? CompletedAt { get; set; }
}
