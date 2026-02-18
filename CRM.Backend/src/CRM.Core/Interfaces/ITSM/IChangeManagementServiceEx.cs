// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for managing changes in IT Service Management.
/// Handles complete change lifecycle: creation, approval, scheduling, implementation, and closure.
/// Includes CAB (Change Approval Board) management, impact analysis, and rollback planning.
/// </summary>
public interface IChangeManagementServiceEx
{
    // CRUD Operations
    /// <summary>Creates a new change request.</summary>
    Task<ChangeDto> CreateChangeAsync(CreateChangeDto dto, int requestorId, CancellationToken cancellationToken = default);

    /// <summary>Gets a change by ID.</summary>
    Task<ChangeDto> GetChangeByIdAsync(int changeId, CancellationToken cancellationToken = default);

    /// <summary>Gets all changes with filtering and pagination.</summary>
    Task<(IEnumerable<ChangeDto> Items, int TotalCount)> ListChangesAsync(
        ChangeFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>Updates change details.</summary>
    Task<ChangeDto> UpdateChangeAsync(int changeId, CreateChangeDto dto, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Cancels a change request.</summary>
    Task<ChangeDto> CancelChangeAsync(int changeId, string reason, int cancelledById, CancellationToken cancellationToken = default);

    /// <summary>Deletes (soft delete) a change.</summary>
    Task DeleteChangeAsync(int changeId, CancellationToken cancellationToken = default);

    // Workflow Operations
    /// <summary>Requests approval for a change.</summary>
    Task<ChangeDto> RequestApprovalAsync(int changeId, int requestedById, CancellationToken cancellationToken = default);

    /// <summary>Approves a change request.</summary>
    Task<ChangeApprovalDto> ApproveChangeAsync(
        int changeId, int approverId, string? comments = null, CancellationToken cancellationToken = default);

    /// <summary>Rejects a change request.</summary>
    Task<ChangeApprovalDto> RejectChangeAsync(
        int changeId, int approverId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Schedules a change for implementation.</summary>
    Task<ChangeDto> ScheduleChangeAsync(
        int changeId, ScheduleChangeImplementationDto dto, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Starts implementation of a scheduled change.</summary>
    Task<ChangeDto> StartImplementationAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Completes a change implementation.</summary>
    Task<ChangeDto> CompleteChangeAsync(int changeId, bool successful, string? notes, int completedById, CancellationToken cancellationToken = default);

    /// <summary>Closes a completed change.</summary>
    Task<ChangeDto> CloseChangeAsync(
        int changeId, string closureCode, string? postImplementationReview, int closedById, CancellationToken cancellationToken = default);

    // CAB Management
    /// <summary>Creates a Change Approval Board for a change.</summary>
    Task<CABDto> CreateCABAsync(CreateCABDto dto, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Schedules a CAB meeting.</summary>
    Task<CABDto> ScheduleCABMeetingAsync(int cabId, DateTime meetingDate, string? meetingLocation, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Records a CAB member's decision.</summary>
    Task<CABMemberDto> RecordCABDecisionAsync(
        int cabId, int cabMemberId, ApprovalStatus status, string? comments, int decidedById, CancellationToken cancellationToken = default);

    /// <summary>Finalizes CAB approval decision.</summary>
    Task<ChangeDto> FinalizeCABApprovalAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Gets CAB details for a change.</summary>
    Task<CABDto> GetCABAsync(int cabId, CancellationToken cancellationToken = default);

    // Approval Management
    /// <summary>Records an approval decision on a change.</summary>
    Task<ChangeApprovalDto> RecordApprovalAsync(
        int changeId, int approverId, ApprovalStatus status, string? comments, CancellationToken cancellationToken = default);

    /// <summary>Gets all approvals for a change.</summary>
    Task<IEnumerable<ChangeApprovalDto>> GetApprovalsAsync(int changeId, CancellationToken cancellationToken = default);

    // Impact Analysis
    /// <summary>Analyzes the change impact on configuration items and services.</summary>
    Task<ChangeImpactDto> AnalyzeChangeImpactAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Documents impacted services and configurations.</summary>
    Task<ChangeImpactDto> DocumentImpactedServicesAsync(
        int changeId, List<int> impactedCIIds, int modifiedById, CancellationToken cancellationToken = default);

    /// <summary>Identifies risks related to rollback.</summary>
    Task<ChangeImpactDto> IdentifyRollbackRisksAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default);

    // Rollback Management
    /// <summary>Creates a rollback plan for a change.</summary>
    Task<ChangeRollbackPlanDto> CreateRollbackPlanAsync(
        int changeId, string rollbackSteps, int estimatedTimeMinutes, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Gets the rollback plan for a change.</summary>
    Task<ChangeRollbackPlanDto> GetRollbackPlanAsync(int changeId, CancellationToken cancellationToken = default);

    /// <summary>Executes a rollback of a failed change.</summary>
    Task<ChangeDto> ExecuteRollbackAsync(
        int changeId, ExecuteRollbackDto dto, int executedById, CancellationToken cancellationToken = default);

    /// <summary>Validates that a rollback was successful.</summary>
    Task<ChangeDto> ValidateRollbackSuccessAsync(
        int changeId, ValidateRollbackSuccessDto dto, int validatedById, CancellationToken cancellationToken = default);

    // Implementation Tracking
    /// <summary>Gets all implementation tasks for a change.</summary>
    Task<IEnumerable<ChangeImplementationTaskDto>> GetImplementationTasksAsync(int changeId, CancellationToken cancellationToken = default);

    /// <summary>Creates an implementation task.</summary>
    Task<ChangeImplementationTaskDto> CreateImplementationTaskAsync(
        int changeId, CreateChangeImplementationTaskDto dto, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Completes an implementation task.</summary>
    Task<ChangeImplementationTaskDto> CompleteImplementationTaskAsync(
        int taskId, int completedById, CancellationToken cancellationToken = default);

    /// <summary>Tracks implementation progress.</summary>
    Task<(int TotalTasks, int CompletedTasks, double ProgressPercent)> GetImplementationProgressAsync(
        int changeId, CancellationToken cancellationToken = default);

    // Conflict Management
    /// <summary>Checks for conflicts with other scheduled changes.</summary>
    Task<ChangeConflictCheckDto> CheckConflictsAsync(int changeId, CancellationToken cancellationToken = default);

    /// <summary>Detects scheduling conflicts with blackout periods.</summary>
    Task<bool> DetectBlackoutConflictsAsync(
        int changeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Blackout Period Management
    /// <summary>Gets blackout periods that overlap with a date range.</summary>
    Task<IEnumerable<ChangeBlackoutDto>> GetBlackoutPeriodsAsync(
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>Creates a change blackout period.</summary>
    Task<ChangeBlackoutDto> CreateBlackoutPeriodAsync(
        CreateChangeBlackoutDto dto, int createdById, CancellationToken cancellationToken = default);

    // Comments
    /// <summary>Adds a comment to a change.</summary>
    Task<ChangeCommentDto> AddCommentAsync(
        int changeId, CreateChangeCommentDto dto, int createdById, CancellationToken cancellationToken = default);

    /// <summary>Gets all comments for a change.</summary>
    Task<IEnumerable<ChangeCommentDto>> GetCommentsAsync(int changeId, CancellationToken cancellationToken = default);

    // Metrics
    /// <summary>Gets change management metrics and statistics.</summary>
    Task<ChangeMetricsDto> GetChangeMetricsAsync(
        DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}

/// <summary>DTO for change blackout period.</summary>
public class ChangeBlackoutDto
{
    public int BlackoutId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for creating a change blackout period.</summary>
public class CreateChangeBlackoutDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}
