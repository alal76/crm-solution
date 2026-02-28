// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;

namespace CRM.Core.Dtos;

// ==================================================================================
// Nurture Enrollment DTOs
// ==================================================================================

/// <summary>Response DTO for a nurture enrollment record.</summary>
public sealed class NurtureEnrollmentDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Parent sequence identifier.</summary>
    public int SequenceId { get; set; }

    /// <summary>Email address of the enrollee.</summary>
    public string EnrolleeEmail { get; set; } = string.Empty;

    /// <summary>Display name of the enrollee.</summary>
    public string? EnrolleeName { get; set; }

    /// <summary>What triggered the enrolment.</summary>
    public NurtureEnrollmentTrigger Trigger { get; set; }

    /// <summary>Zero-based current step index.</summary>
    public int CurrentStep { get; set; }

    /// <summary>Whether all steps have been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Whether the enrollee has opted out.</summary>
    public bool IsUnsubscribed { get; set; }

    /// <summary>When the next step will be processed.</summary>
    public DateTime? NextStepAt { get; set; }

    /// <summary>When the enrolment was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Request DTO to enrol leads into a nurture sequence.</summary>
public sealed class EnrollLeadsDto
{
    /// <summary>List of lead IDs to enrol.</summary>
    [Required]
    public List<int> LeadIds { get; set; } = new();

    /// <summary>What triggered the enrolment.</summary>
    public NurtureEnrollmentTrigger Trigger { get; set; } = NurtureEnrollmentTrigger.ManualEnroll;
}

// ==================================================================================
// UTM Tracking Link DTOs
// ==================================================================================

/// <summary>Response DTO for a campaign tracking link.</summary>
public sealed class CampaignTrackingLinkDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Parent campaign identifier.</summary>
    public int CampaignId { get; set; }

    /// <summary>Original destination URL.</summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>UTM-tagged + token-appended tracked URL.</summary>
    public string TrackedUrl { get; set; } = string.Empty;

    /// <summary>Human-readable link alias.</summary>
    public string? LinkAlias { get; set; }

    /// <summary>UTM source parameter.</summary>
    public string? UtmSource { get; set; }

    /// <summary>UTM medium parameter.</summary>
    public string? UtmMedium { get; set; }

    /// <summary>UTM campaign parameter.</summary>
    public string? UtmCampaign { get; set; }

    /// <summary>UTM content parameter.</summary>
    public string? UtmContent { get; set; }

    /// <summary>Short tracking token embedded in the tracked URL.</summary>
    public string? TrackingToken { get; set; }

    /// <summary>Total click count.</summary>
    public int ClickCount { get; set; }

    /// <summary>When the link was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Request DTO to create a campaign tracking link.</summary>
public sealed class CreateTrackingLinkDto
{
    /// <summary>Original destination URL.</summary>
    [Required]
    [Url]
    [MaxLength(2048)]
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>Human-readable alias for this link.</summary>
    [MaxLength(100)]
    public string? LinkAlias { get; set; }

    /// <summary>UTM source parameter (e.g., "newsletter").</summary>
    [MaxLength(200)]
    public string? UtmSource { get; set; }

    /// <summary>UTM medium parameter (e.g., "email").</summary>
    [MaxLength(200)]
    public string? UtmMedium { get; set; }

    /// <summary>UTM campaign parameter.</summary>
    [MaxLength(200)]
    public string? UtmCampaign { get; set; }

    /// <summary>UTM content parameter (A/B creative variant).</summary>
    [MaxLength(200)]
    public string? UtmContent { get; set; }
}

// ==================================================================================
// Unsubscribe DTOs
// ==================================================================================

/// <summary>Request DTO to unsubscribe or update preferences.</summary>
public sealed class UnsubscribeRequestDto
{
    /// <summary>Email address to unsubscribe.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Signed token from the unsubscribe link (optional for token-less preference update).</summary>
    [MaxLength(500)]
    public string? Token { get; set; }

    /// <summary>Reason for unsubscribing.</summary>
    public UnsubscribeReason Reason { get; set; }

    /// <summary>Optional free-text note.</summary>
    [MaxLength(1000)]
    public string? ReasonNote { get; set; }

    /// <summary>Whether to receive product-update emails.</summary>
    public bool ReceiveProductUpdates { get; set; } = false;
}

/// <summary>Response DTO showing the current unsubscribe / preference state for an email.</summary>
public sealed class UnsubscribeStatusDto
{
    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Whether this email has an active unsubscribe record.</summary>
    public bool IsUnsubscribed { get; set; }

    /// <summary>Whether the contact wishes to receive product-update emails.</summary>
    public bool ReceiveProductUpdates { get; set; }

    /// <summary>Always true — transactional emails cannot be opted out.</summary>
    public bool ReceiveTransactional { get; set; } = true;

    /// <summary>When the unsubscribe occurred.</summary>
    public DateTime? UnsubscribedAt { get; set; }
}

// ==================================================================================
// Campaign Execution DTOs
// ==================================================================================

// Note: ScheduleCampaignDto already defined in CampaignDtos.cs

/// <summary>Current execution statistics for a campaign.</summary>
public sealed class CampaignExecutionStatusDto
{
    /// <summary>Campaign identifier.</summary>
    public int CampaignId { get; set; }

    /// <summary>Current campaign status.</summary>
    public CampaignStatus Status { get; set; }

    /// <summary>Total number of recipients targeted.</summary>
    public int TotalRecipients { get; set; }

    /// <summary>Number of emails sent.</summary>
    public int SendCount { get; set; }

    /// <summary>Number of unique opens.</summary>
    public int OpenCount { get; set; }

    /// <summary>Number of unique clicks.</summary>
    public int ClickCount { get; set; }

    /// <summary>Number of unsubscribes.</summary>
    public int UnsubscribeCount { get; set; }

    /// <summary>Number of bounces.</summary>
    public int BounceCount { get; set; }

    /// <summary>Open rate as a percentage (0–100).</summary>
    public double OpenRate { get; set; }

    /// <summary>Click-to-open rate as a percentage (0–100).</summary>
    public double ClickRate { get; set; }

    /// <summary>Unsubscribe rate as a percentage (0–100).</summary>
    public double UnsubscribeRate { get; set; }

    /// <summary>When the campaign started sending.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>When the campaign was completed or cancelled.</summary>
    public DateTime? CompletedAt { get; set; }
}

// ==================================================================================
// Email-Provider Webhook DTO
// ==================================================================================

/// <summary>Request DTO received from the email provider's tracking webhook.</summary>
public sealed class EmailTrackingWebhookDto
{
    /// <summary>Provider-assigned message ID for correlation.</summary>
    [Required]
    [MaxLength(200)]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>Event name (e.g., "delivered", "open", "click", "bounce", "unsubscribe").</summary>
    [Required]
    [MaxLength(50)]
    public string Event { get; set; } = string.Empty;

    /// <summary>Recipient email address.</summary>
    [MaxLength(320)]
    public string? RecipientEmail { get; set; }

    /// <summary>Clicked URL (for click events).</summary>
    [MaxLength(2048)]
    public string? ClickedUrl { get; set; }

    /// <summary>User-agent of the recipient's client.</summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>IP address of the recipient.</summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>Unix epoch timestamp of the event.</summary>
    public long? Timestamp { get; set; }
}
