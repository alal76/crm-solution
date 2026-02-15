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
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// CRM Task status enumeration
/// </summary>
public enum CrmTaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Deferred = 3,
    Waiting = 4,
    Cancelled = 5
}

/// <summary>
/// CRM Task priority enumeration
/// </summary>
public enum CrmTaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// CRM Task type enumeration
/// </summary>
public enum CrmTaskType
{
    Call = 0,
    Email = 1,
    Meeting = 2,
    FollowUp = 3,
    Demo = 4,
    Proposal = 5,
    Contract = 6,
    Research = 7,
    Other = 8
}

/// <summary>
/// CRM Task entity for managing to-do items and follow-ups
/// </summary>
public class CrmTask : BaseEntity
{
    #region Basic Information

    /// <summary>Task subject/title</summary>
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>Task description</summary>
    [MaxLength(5000)]
    public string? Description { get; set; }

    /// <summary>Type of task</summary>
    public CrmTaskType TaskType { get; set; } = CrmTaskType.Other;

    /// <summary>Current status</summary>
    public CrmTaskStatus Status { get; set; } = CrmTaskStatus.NotStarted;

    /// <summary>Task priority</summary>
    public CrmTaskPriority Priority { get; set; } = CrmTaskPriority.Normal;

    #endregion

    #region Dates & Timing

    /// <summary>Due date for the task</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Planned start date</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Actual completion date</summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>Reminder date/time</summary>
    public DateTime? ReminderDate { get; set; }

    /// <summary>Whether reminder is set</summary>
    public bool HasReminder { get; set; } = false;

    #endregion

    #region Progress & Effort

    /// <summary>Percentage complete (0-100)</summary>
    [Range(0, 100)]
    public int PercentComplete { get; set; } = 0;

    /// <summary>Estimated effort in minutes</summary>
    [Range(0, 525600)] // Max 1 year in minutes
    public int? EstimatedMinutes { get; set; }

    /// <summary>Actual effort in minutes</summary>
    [Range(0, 525600)]
    public int? ActualMinutes { get; set; }

    #endregion

    #region Recurrence

    /// <summary>Whether task recurs</summary>
    public bool IsRecurring { get; set; } = false;

    /// <summary>Recurrence pattern (JSON: daily, weekly, monthly, etc.)</summary>
    [MaxLength(500)]
    public string? RecurrencePattern { get; set; }

    /// <summary>End date for recurrence</summary>
    public DateTime? RecurrenceEndDate { get; set; }

    /// <summary>Parent task ID (for recurring instances)</summary>
    public int? ParentTaskId { get; set; }

    #endregion

    #region Relationships

    /// <summary>Associated account ID</summary>
    [Column("AccountId")]
    public int? AccountId { get; set; }

    /// <summary>Associated contact ID</summary>
    public int? ContactId { get; set; }

    /// <summary>Associated opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Associated campaign ID</summary>
    public int? CampaignId { get; set; }

    /// <summary>Assigned user ID</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>Assigned group ID for workflow queue</summary>
    public int? AssignedToGroupId { get; set; }

    /// <summary>User who created the task</summary>
    public int? CreatedByUserId { get; set; }

    #endregion

    #region Classification & Metadata

    /// <summary>Comma-separated tags</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>Task category</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>Attachments (JSON array of file URLs)</summary>
    [MaxLength(5000)]
    public string? Attachments { get; set; }

    /// <summary>Custom fields (JSON)</summary>
    [MaxLength(10000)]
    public string? CustomFields { get; set; }

    #endregion

    #region Navigation Properties

    public Account? Account { get; set; }
    public Opportunity? Opportunity { get; set; }
    public MarketingCampaign? Campaign { get; set; }
    public User? AssignedToUser { get; set; }
    public UserGroup? AssignedToGroup { get; set; }
    public User? CreatedByUser { get; set; }
    public CrmTask? ParentTask { get; set; }
    public ICollection<CrmTask>? SubTasks { get; set; }

    #endregion
}
