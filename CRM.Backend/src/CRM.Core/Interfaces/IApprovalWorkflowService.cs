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

namespace CRM.Core.Interfaces;

using CRM.Core.Entities;

/// <summary>
/// Service interface for approval workflow operations - handling quote, discount,
/// and contract approval processes including matrix-based routing, escalation, and tracking.
/// </summary>
public interface IApprovalWorkflowService
{
    #region Approval Matrix Management

    /// <summary>
    /// Gets all approval matrices with optional filtering.
    /// </summary>
    Task<IEnumerable<DiscountApprovalMatrix>> GetAllMatricesAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an approval matrix by ID with all levels.
    /// </summary>
    Task<DiscountApprovalMatrix?> GetMatrixByIdAsync(int matrixId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new approval matrix.
    /// </summary>
    Task<DiscountApprovalMatrix> CreateMatrixAsync(DiscountApprovalMatrix matrix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing approval matrix.
    /// </summary>
    Task<DiscountApprovalMatrix> UpdateMatrixAsync(DiscountApprovalMatrix matrix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an approval matrix (soft delete).
    /// </summary>
    Task<bool> DeleteMatrixAsync(int matrixId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates an approval matrix.
    /// </summary>
    Task<DiscountApprovalMatrix> ActivateMatrixAsync(int matrixId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates an approval matrix.
    /// </summary>
    Task<DiscountApprovalMatrix> DeactivateMatrixAsync(int matrixId, CancellationToken cancellationToken = default);

    #endregion

    #region Approval Level Management

    /// <summary>
    /// Gets all approval levels for a matrix.
    /// </summary>
    Task<IEnumerable<ApprovalLevel>> GetMatrixLevelsAsync(int matrixId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an approval level by ID.
    /// </summary>
    Task<ApprovalLevel?> GetLevelByIdAsync(int levelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an approval level to a matrix.
    /// </summary>
    Task<ApprovalLevel> AddLevelAsync(int matrixId, ApprovalLevel level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an approval level.
    /// </summary>
    Task<ApprovalLevel> UpdateLevelAsync(ApprovalLevel level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an approval level.
    /// </summary>
    Task<bool> RemoveLevelAsync(int levelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders approval levels within a matrix.
    /// </summary>
    Task<IEnumerable<ApprovalLevel>> ReorderLevelsAsync(
        int matrixId,
        IEnumerable<int> levelIdsInOrder,
        CancellationToken cancellationToken = default);

    #endregion

    #region Approval Group Management

    /// <summary>
    /// Gets all approval groups.
    /// </summary>
    Task<IEnumerable<ApprovalGroup>> GetAllGroupsAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an approval group by ID with members.
    /// </summary>
    Task<ApprovalGroup?> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an approval group.
    /// </summary>
    Task<ApprovalGroup> CreateGroupAsync(ApprovalGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an approval group.
    /// </summary>
    Task<ApprovalGroup> UpdateGroupAsync(ApprovalGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an approval group (soft delete).
    /// </summary>
    Task<bool> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a member to an approval group.
    /// </summary>
    Task<ApprovalGroupMember> AddGroupMemberAsync(
        int groupId,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a member from an approval group.
    /// </summary>
    Task<bool> RemoveGroupMemberAsync(int groupId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets members of an approval group.
    /// </summary>
    Task<IEnumerable<ApprovalGroupMember>> GetGroupMembersAsync(
        int groupId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Approval Request Management

    /// <summary>
    /// Gets all approval requests with filtering.
    /// </summary>
    Task<IEnumerable<ApprovalRequest>> GetAllRequestsAsync(
        DiscountApprovalStatus? status = null,
        int? submitterId = null,
        int? quoteId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an approval request by ID with steps.
    /// </summary>
    Task<ApprovalRequest?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an approval request by request number.
    /// </summary>
    Task<ApprovalRequest?> GetRequestByNumberAsync(string requestNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending approvals for a user.
    /// </summary>
    Task<IEnumerable<ApprovalRequest>> GetPendingApprovalsForUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approval requests submitted by a user.
    /// </summary>
    Task<IEnumerable<ApprovalRequest>> GetRequestsBySubmitterAsync(
        int submitterId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Approval Workflow Operations

    /// <summary>
    /// Submits a quote for approval.
    /// </summary>
    Task<ApprovalSubmissionResult> SubmitForApprovalAsync(
        int quoteId,
        int submitterId,
        string? justification = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines the required approval levels for a quote.
    /// </summary>
    Task<ApprovalRequirementResult> DetermineApprovalRequirementsAsync(
        int quoteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending approval step.
    /// </summary>
    Task<ApprovalActionResult> ApproveStepAsync(
        int requestId,
        int approverId,
        string? comments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a pending approval step.
    /// </summary>
    Task<ApprovalActionResult> RejectStepAsync(
        int requestId,
        int approverId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalls (cancels) a submitted approval request.
    /// </summary>
    Task<ApprovalRequest> RecallRequestAsync(
        int requestId,
        int userId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reassigns a pending approval step to a different user.
    /// </summary>
    Task<ApprovalStep> ReassignStepAsync(
        int stepId,
        int newAssigneeId,
        int reassignedById,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Escalates a pending approval step.
    /// </summary>
    Task<ApprovalStep> EscalateStepAsync(
        int stepId,
        int escalatedById,
        string? reason = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Matrix Selection

    /// <summary>
    /// Finds the applicable approval matrix for a quote.
    /// </summary>
    Task<DiscountApprovalMatrix?> FindApplicableMatrixAsync(
        int quoteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a discount amount/percentage requires approval.
    /// </summary>
    Task<bool> RequiresApprovalAsync(
        decimal discountPercent,
        decimal? dealAmount = null,
        int? matrixId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the maximum discount a user can approve without escalation.
    /// </summary>
    Task<decimal> GetUserApprovalLimitAsync(int userId, CancellationToken cancellationToken = default);

    #endregion

    #region Notifications & Reminders

    /// <summary>
    /// Sends reminder notifications for overdue approvals.
    /// </summary>
    Task<int> SendOverdueRemindersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes auto-escalations for timed-out steps.
    /// </summary>
    Task<int> ProcessAutoEscalationsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Statistics & Reporting

    /// <summary>
    /// Gets approval workflow statistics.
    /// </summary>
    Task<ApprovalStatistics> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approver performance statistics.
    /// </summary>
    Task<IEnumerable<ApproverPerformance>> GetApproverPerformanceAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approval history for a quote.
    /// </summary>
    Task<IEnumerable<ApprovalRequest>> GetQuoteApprovalHistoryAsync(
        int quoteId,
        CancellationToken cancellationToken = default);

    #endregion
}

#region Supporting Types

/// <summary>
/// Result of submitting a request for approval.
/// </summary>
public class ApprovalSubmissionResult
{
    public bool Success { get; set; }
    public ApprovalRequest? Request { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresApproval { get; set; }
    public bool AutoApproved { get; set; }
    public int RequiredLevels { get; set; }
    public List<string> ApproverNames { get; set; } = new();
}

/// <summary>
/// Result of determining approval requirements.
/// </summary>
public class ApprovalRequirementResult
{
    public bool RequiresApproval { get; set; }
    public int? ApplicableMatrixId { get; set; }
    public string? MatrixName { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DealAmount { get; set; }
    public int RequiredLevels { get; set; }
    public List<ApprovalLevelInfo> Levels { get; set; } = new();
    public string? ReasonForApproval { get; set; }
}

/// <summary>
/// Information about an approval level.
/// </summary>
public class ApprovalLevelInfo
{
    public int LevelOrder { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int? ApproverUserId { get; set; }
    public string? ApproverName { get; set; }
    public string? ApproverRole { get; set; }
    public int? ApprovalGroupId { get; set; }
    public string? GroupName { get; set; }
    public ApprovalThresholdType ThresholdType { get; set; }
    public decimal MinValue { get; set; }
    public decimal? MaxValue { get; set; }
}

/// <summary>
/// Result of an approval action (approve/reject).
/// </summary>
public class ApprovalActionResult
{
    public bool Success { get; set; }
    public ApprovalRequest? Request { get; set; }
    public ApprovalStep? CurrentStep { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsFullyApproved { get; set; }
    public bool IsRejected { get; set; }
    public int? NextLevelOrder { get; set; }
    public string? NextApproverName { get; set; }
}

/// <summary>
/// Statistics for approval workflows.
/// </summary>
public class ApprovalStatistics
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int RecalledRequests { get; set; }
    public int AutoApprovedRequests { get; set; }
    public decimal AverageTimeToApprovalHours { get; set; }
    public decimal TotalDiscountApproved { get; set; }
    public decimal AverageDiscountPercent { get; set; }
    public int OverdueSteps { get; set; }
    public int EscalatedRequests { get; set; }
    public Dictionary<string, int> RequestsByStatus { get; set; } = new();
    public Dictionary<string, int> RequestsByMatrix { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Performance statistics for an approver.
/// </summary>
public class ApproverPerformance
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int TotalApproved { get; set; }
    public int TotalRejected { get; set; }
    public int TotalReassigned { get; set; }
    public int TotalEscalated { get; set; }
    public decimal AverageResponseTimeHours { get; set; }
    public int CurrentPending { get; set; }
    public int OverdueCount { get; set; }
    public double ApprovalRate { get; set; }
}

#endregion
