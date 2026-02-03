// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.
// See LICENSE file in the project root for full license information.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
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
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

public enum ChangeType
{
    Standard = 1,
    Normal = 2,
    Emergency = 3
}

public enum ChangeState
{
    New = 1,
    Assess = 2,
    Authorize = 3,
    Scheduled = 4,
    Implement = 5,
    Review = 6,
    Closed = 7,
    Cancelled = 8
}

public enum ChangeRisk
{
    High = 1,
    Medium = 2,
    Low = 3
}

public enum ChangeImpact
{
    High = 1,
    Medium = 2,
    Low = 3
}

public enum ApprovalStatus
{
    Requested = 1,
    Approved = 2,
    Rejected = 3,
    MoreInfo = 4
}

public class Change
{
    [Key]
    public int ChangeId { get; set; }

    [Required]
    [StringLength(20)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public ChangeType Type { get; set; }

    // Classification
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ServiceRequestCategory? Category { get; set; }

    public int? ConfigurationItemId { get; set; }

    [ForeignKey(nameof(ConfigurationItemId))]
    public ConfigurationItem? ConfigurationItem { get; set; }

    public int? ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public Service? Service { get; set; }

    // Planning
    [Required]
    public int RequestorId { get; set; }

    [ForeignKey(nameof(RequestorId))]
    public User? Requestor { get; set; }

    public int? AssignedToId { get; set; }

    [ForeignKey(nameof(AssignedToId))]
    public User? AssignedTo { get; set; }

    public int? ImplementationGroupId { get; set; }

    [ForeignKey(nameof(ImplementationGroupId))]
    public UserGroup? ImplementationGroup { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public bool MaintenanceWindow { get; set; } = false;

    // Risk Assessment
    [Required]
    public ChangeRisk Risk { get; set; }

    [Required]
    public ChangeImpact Impact { get; set; }

    public string? RiskAssessmentNotes { get; set; }

    public string? RiskMitigationPlan { get; set; }

    // Implementation
    public string? ImplementationPlan { get; set; }

    public string? BackoutPlan { get; set; }

    public string? TestingPlan { get; set; }

    public string? ImplementationNotes { get; set; }

    // Approval
    [Required]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Requested;

    public DateTime? CABDate { get; set; }

    public string? ApprovalNotes { get; set; }

    // State
    [Required]
    public ChangeState State { get; set; } = ChangeState.New;

    // Closure
    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public bool? ChangeSuccess { get; set; }

    [StringLength(100)]
    public string? ClosureCode { get; set; }

    public string? ClosureNotes { get; set; }

    public string? PostImplementationReview { get; set; }

    public DateTime? ReviewDate { get; set; }

    // Tracking
    public bool ConflictDetected { get; set; } = false;

    public string? ConflictDetails { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<ChangeApproval>? Approvals { get; set; }

    public ICollection<ChangeImpactedCI>? ImpactedCIs { get; set; }

    public ICollection<ChangeTask>? Tasks { get; set; }

    public ICollection<ChangeComment>? Comments { get; set; }

    public ICollection<ChangeAttachment>? Attachments { get; set; }
}

public enum ApprovalRole
{
    CABMember = 1,
    CABChair = 2,
    ChangeManager = 3,
    ITDirector = 4,
    BusinessOwner = 5,
    TechnicalOwner = 6
}

public class ChangeApproval
{
    [Key]
    public int ApprovalId { get; set; }

    [Required]
    public int ChangeId { get; set; }

    [ForeignKey(nameof(ChangeId))]
    public Change? Change { get; set; }

    [Required]
    public int ApproverId { get; set; }

    [ForeignKey(nameof(ApproverId))]
    public User? Approver { get; set; }

    public ApprovalRole ApprovalRole { get; set; }

    [Required]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Requested;

    public DateTime? ApprovalDate { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ChangeBlackout
{
    [Key]
    public int BlackoutId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
}

public class ChangeImpactedCI
{
    [Key]
    public int ChangeImpactedCIId { get; set; }

    [Required]
    public int ChangeId { get; set; }

    [ForeignKey(nameof(ChangeId))]
    public Change? Change { get; set; }

    [Required]
    public int CIId { get; set; }

    [ForeignKey(nameof(CIId))]
    public ConfigurationItem? ConfigurationItem { get; set; }

    public ChangeImpact Impact { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ChangeTask
{
    [Key]
    public int TaskId { get; set; }

    [Required]
    public int ChangeId { get; set; }

    [ForeignKey(nameof(ChangeId))]
    public Change? Change { get; set; }

    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssignedToId { get; set; }

    [ForeignKey(nameof(AssignedToId))]
    public User? AssignedTo { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ChangeComment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int ChangeId { get; set; }

    [ForeignKey(nameof(ChangeId))]
    public Change? Change { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = true;

    [Required]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ChangeAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Required]
    public int ChangeId { get; set; }

    [ForeignKey(nameof(ChangeId))]
    public Change? Change { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [Required]
    public int UploadedById { get; set; }

    [ForeignKey(nameof(UploadedById))]
    public User? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
