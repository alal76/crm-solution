// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.ITSM;

namespace CRM.Core.DTOs.ITSM;

// ============================================================================
// Change Management Workflow DTOs
// ============================================================================

/// <summary>
/// DTO for Change Approval Board (CAB) operations.
/// </summary>
public class CABDto
{
    public int CABId { get; set; }
    public int ChangeId { get; set; }
    public string? ChangeNumber { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Status { get; set; }
    public List<CABMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for CAB member details.
/// </summary>
public class CABMemberDto
{
    public int CABMemberId { get; set; }
    public int CABId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public string? VotingStatus { get; set; }
    public string? Comments { get; set; }
    public DateTime? VotedAt { get; set; }
}

/// <summary>
/// DTO for creating a CAB.
/// </summary>
public class CreateCABDto
{
    [Required]
    public int ChangeId { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    [Required]
    public List<int> MemberIds { get; set; } = new();
}

/// <summary>
/// DTO for recording a CAB decision.
/// </summary>
public class RecordCABDecisionDto
{
    [Required]
    public int CABMemberId { get; set; }

    [Required]
    public ApprovalStatus ApprovalStatus { get; set; }

    public string? Comments { get; set; }
}

/// <summary>
/// DTO for change approval tracking.
/// </summary>
public class ChangeApprovalDto
{
    public int ApprovalId { get; set; }
    public int ChangeId { get; set; }
    public int ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string? ApprovalRole { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for change impact analysis.
/// </summary>
public class ChangeImpactDto
{
    public int ChangeId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public List<ImpactedServiceDto> ImpactedServices { get; set; } = new();
    public List<RiskAssessmentDto> RiskAssessments { get; set; } = new();
    public double? TotalRiskScore { get; set; }
    public bool BackoutPlanRequired { get; set; }
}

/// <summary>
/// DTO for an impacted service in change impact analysis.
/// </summary>
public class ImpactedServiceDto
{
    public int CIId { get; set; }
    public string CIName { get; set; } = string.Empty;
    public ChangeImpact ImpactLevel { get; set; }
    public ChangeRisk RiskLevel { get; set; }
    public string? MitigationPlan { get; set; }
    public int? BackoutTime { get; set; }
}

/// <summary>
/// DTO for risk assessment details in change.
/// </summary>
public class RiskAssessmentDto
{
    public string RiskType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChangeRisk SeverityLevel { get; set; }
    public string? Mitigation { get; set; }
    public int? ContingencyTime { get; set; }
}

/// <summary>
/// DTO for change implementation task.
/// </summary>
public class ChangeImplementationTaskDto
{
    public int TaskId { get; set; }
    public int ChangeId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public bool IsCompleted { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a change implementation task.
/// </summary>
public class CreateChangeImplementationTaskDto
{
    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssignedToId { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for change rollback plan.
/// </summary>
public class ChangeRollbackPlanDto
{
    public int ChangeId { get; set; }
    public string? RollbackSteps { get; set; }
    public int? EstimatedRollbackTimeMinutes { get; set; }
    public List<string> RollbackTasks { get; set; } = new();
    public string? BackoutTriggers { get; set; }
    public string? RisksIfNotRolledBack { get; set; }
}

/// <summary>
/// DTO for executing a change rollback.
/// </summary>
public class ExecuteRollbackDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;

    public string? RollbackNotes { get; set; }

    public DateTime? RollbackCompletedAt { get; set; }
}

/// <summary>
/// DTO for validating rollback success.
/// </summary>
public class ValidateRollbackSuccessDto
{
    [Required]
    public bool RollbackSuccessful { get; set; }

    public string? ValidationNotes { get; set; }

    [StringLength(500)]
    public string? PostRollbackChecks { get; set; }
}

/// <summary>
/// DTO for change conflict detection results.
/// </summary>
public class ChangeConflictCheckDto
{
    public int ChangeId { get; set; }
    public bool ConflictsDetected { get; set; }
    public List<ConflictingChangeDto> ConflictingChanges { get; set; } = new();
    public List<ScheduleConflictDto> ScheduleConflicts { get; set; } = new();
    public string? ConflictResolutionNotes { get; set; }
}

/// <summary>
/// DTO for a conflicting change.
/// </summary>
public class ConflictingChangeDto
{
    public int ChangeId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public string ConflictReason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for schedule conflicts.
/// </summary>
public class ScheduleConflictDto
{
    public string Description { get; set; } = string.Empty;
    public DateTime ConflictPeriodStart { get; set; }
    public DateTime ConflictPeriodEnd { get; set; }
    public string ConflictType { get; set; } = string.Empty;
}

/// <summary>
/// DTO for change comment/note.
/// </summary>
public class ChangeCommentDto
{
    public int CommentId { get; set; }
    public int ChangeId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public int CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a change comment.
/// </summary>
public class CreateChangeCommentDto
{
    [Required]
    [StringLength(5000)]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = true;
}

/// <summary>
/// DTO for change metrics and statistics.
/// </summary>
public class ChangeMetricsDto
{
    public int TotalChanges { get; set; }
    public int SuccessfulChanges { get; set; }
    public int FailedChanges { get; set; }
    public int CancelledChanges { get; set; }
    public double SuccessRate { get; set; }
    public double AverageImplementationTimeMinutes { get; set; }
    public int RequiredRollbacks { get; set; }
    public double RollbackRate { get; set; }
    public DateTime? ReportGeneratedAt { get; set; }
}

/// <summary>
/// DTO for scheduling a change implementation.
/// </summary>
public class ScheduleChangeImplementationDto
{
    [Required]
    public DateTime ScheduledStartDate { get; set; }

    [Required]
    public DateTime ScheduledEndDate { get; set; }

    public string? MaintenanceWindow { get; set; }

    public bool BypassCAB { get; set; } = false;
}
