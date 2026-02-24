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
/// Web-to-Lead Form Configuration (TODO-CRM002-04)
/// Allows building and configuring forms that capture leads from websites.
/// </summary>
public class WebToLeadForm : BaseEntity
{
    /// <summary>
    /// Form name/title
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Form description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// JSON array defining form fields and their configuration
    /// Example: [{"name":"firstName","type":"text","required":true},...]
    /// </summary>
    public string? FieldsJson { get; set; }

    /// <summary>
    /// Target lead source to assign to captured leads
    /// </summary>
    public int? TargetLeadSourceId { get; set; }

    /// <summary>
    /// Navigation to lead source
    /// </summary>
    [ForeignKey("TargetLeadSourceId")]
    public virtual LeadSourceConfig? TargetLeadSource { get; set; }

    /// <summary>
    /// Whether CAPTCHA is enabled for spam protection
    /// </summary>
    public bool CaptchaEnabled { get; set; } = true;

    /// <summary>
    /// Email address to notify when lead is captured
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? NotifyEmail { get; set; }

    /// <summary>
    /// Comma-separated list of notification email addresses
    /// </summary>
    [MaxLength(1000)]
    public string? NotifyEmails { get; set; }

    /// <summary>
    /// URL to redirect user after successful submission
    /// </summary>
    [MaxLength(500)]
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Thank you message to display after submission
    /// </summary>
    [MaxLength(2000)]
    public string? ThankYouMessage { get; set; }

    /// <summary>
    /// Whether form is active and accepting submissions
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Unique embed code/key for the form
    /// </summary>
    [MaxLength(100)]
    public string? EmbedKey { get; set; }

    /// <summary>
    /// Default owner to assign leads to
    /// </summary>
    public int? DefaultOwnerId { get; set; }

    /// <summary>
    /// Navigation to default owner
    /// </summary>
    [ForeignKey("DefaultOwnerId")]
    public virtual User? DefaultOwner { get; set; }

    /// <summary>
    /// CSS styling for the form (JSON or raw CSS)
    /// </summary>
    public string? CustomStyling { get; set; }

    /// <summary>
    /// Total number of submissions received
    /// </summary>
    public int SubmissionCount { get; set; } = 0;

    /// <summary>
    /// Last submission date
    /// </summary>
    public DateTime? LastSubmissionAt { get; set; }
}
