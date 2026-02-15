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

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for email sequence response.
/// </summary>
public class EmailSequenceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public string? SequenceType { get; set; }
    public int TotalEnrolled { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalActive { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal ReplyRate { get; set; }
    public decimal ConversionRate { get; set; }
    public string? DefaultFromName { get; set; }
    public string? DefaultFromEmail { get; set; }
    public string? DefaultReplyTo { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<EmailSequenceStepDto> Steps { get; set; } = new();
}

/// <summary>
/// DTO for creating email sequence.
/// </summary>
public class CreateEmailSequenceDto
{
    [Required(ErrorMessage = "Sequence name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "SequenceType cannot exceed 50 characters")]
    public string? SequenceType { get; set; }

    [StringLength(100)]
    public string? DefaultFromName { get; set; }

    [EmailAddress(ErrorMessage = "DefaultFromEmail must be a valid email address")]
    public string? DefaultFromEmail { get; set; }

    [EmailAddress(ErrorMessage = "DefaultReplyTo must be a valid email address")]
    public string? DefaultReplyTo { get; set; }

    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }

    [Range(1, 24)]
    public int? SendingStartHour { get; set; }

    [Range(1, 24)]
    public int? SendingEndHour { get; set; }
}

/// <summary>
/// DTO for updating email sequence.
/// </summary>
public class UpdateEmailSequenceDto
{
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string? Name { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? DefaultFromName { get; set; }

    [EmailAddress(ErrorMessage = "DefaultFromEmail must be a valid email address")]
    public string? DefaultFromEmail { get; set; }

    [EmailAddress(ErrorMessage = "DefaultReplyTo must be a valid email address")]
    public string? DefaultReplyTo { get; set; }

    [Range(1, 24)]
    public int? SendingStartHour { get; set; }

    [Range(1, 24)]
    public int? SendingEndHour { get; set; }

    public bool? ExitOnReply { get; set; }
    public bool? ExitOnMeetingBooked { get; set; }
    public bool? ExitOnBounce { get; set; }
    public bool? ExitOnUnsubscribe { get; set; }
}

/// <summary>
/// DTO for email sequence step.
/// </summary>
public class EmailSequenceStepDto
{
    public int Id { get; set; }
    public int SequenceId { get; set; }
    public int StepNumber { get; set; }
    public string StepType { get; set; } = "Email";
    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? HtmlContent { get; set; }
    public string? TextContent { get; set; }
    public int? TemplateId { get; set; }
    public int DelayDays { get; set; }
    public int DelayHours { get; set; }
    public int DelayMinutes { get; set; }
    public string TimingMode { get; set; } = "Delay";
    public TimeSpan? SpecificTime { get; set; }
    public bool SendOnWeekends { get; set; }
    public bool IsABTest { get; set; }
    public string? ABVariant { get; set; }
    public int ABTestPercentage { get; set; }
    public int TotalSent { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalReplied { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating email sequence step.
/// </summary>
public class CreateEmailSequenceStepDto
{
    [Required(ErrorMessage = "Step name is required")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string StepType { get; set; } = "Email";

    [StringLength(255)]
    public string? Subject { get; set; }

    public string? HtmlContent { get; set; }
    public string? TextContent { get; set; }
    public int? TemplateId { get; set; }

    [Range(0, 365)]
    public int DelayDays { get; set; }

    [Range(0, 23)]
    public int DelayHours { get; set; }

    [Range(0, 59)]
    public int DelayMinutes { get; set; }

    [StringLength(50)]
    public string TimingMode { get; set; } = "Delay";

    public TimeSpan? SpecificTime { get; set; }
    public bool SendOnWeekends { get; set; }
    public bool IsABTest { get; set; }

    [StringLength(10)]
    public string? ABVariant { get; set; }

    [Range(1, 100)]
    public int ABTestPercentage { get; set; } = 50;
}

/// <summary>
/// DTO for email sequence enrollment.
/// </summary>
public class EmailSequenceEnrollmentDto
{
    public int Id { get; set; }
    public int SequenceId { get; set; }
    public int? ContactId { get; set; }
    public int? LeadId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int? CurrentStepId { get; set; }
    public int CurrentStepNumber { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExitedAt { get; set; }
    public string? ExitReason { get; set; }
    public DateTime? NextStepScheduledAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int TotalEmailsSent { get; set; }
    public int TotalEmailsOpened { get; set; }
    public int TotalLinksClicked { get; set; }
}

/// <summary>
/// DTO for creating email sequence enrollment.
/// </summary>
public class CreateEmailSequenceEnrollmentDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be valid")]
    public string Email { get; set; } = string.Empty;

    public int? ContactId { get; set; }
    public int? LeadId { get; set; }

    [StringLength(500)]
    public string? MergeFieldData { get; set; }

    [StringLength(100)]
    public string? EnrollmentSource { get; set; } = "Manual";
}

/// <summary>
/// DTO for sequence execution result.
/// </summary>
public class EmailSequenceExecutionResultDto
{
    public int SequenceId { get; set; }
    public int EnrollmentsProcessed { get; set; }
    public int EmailsSent { get; set; }
    public int Errors { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}

/// <summary>
/// DTO for sequence analytics.
/// </summary>
public class EmailSequenceAnalyticsDto
{
    public int SequenceId { get; set; }
    public string SequenceName { get; set; } = string.Empty;
    public int TotalEnrolled { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalActive { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal ReplyRate { get; set; }
    public decimal ConversionRate { get; set; }
    public int TotalEmailsSent { get; set; }
    public int TotalOpens { get; set; }
    public int TotalClicks { get; set; }
    public int TotalReplies { get; set; }
    public int TotalBounces { get; set; }
    public int UnsubscribeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecuted { get; set; }
    public List<StepAnalyticsDto> StepAnalytics { get; set; } = new();
}

/// <summary>
/// DTO for step-level analytics.
/// </summary>
public class StepAnalyticsDto
{
    public int StepId { get; set; }
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int TotalSent { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalReplied { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal ReplyRate { get; set; }
}
