// Temporarily disabled - requires entity alignment refactoring
#if ITSM_ADVANCED
// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for Change Advisory Board (CAB) workflow operations.
/// Manages multi-level approval processes for change requests.
/// </summary>
public interface ICABWorkflowService
{
    /// <summary>
    /// Initialize CAB workflow for a change request based on its type and risk level.
    /// </summary>
    Task<CABWorkflow> InitializeWorkflowAsync(int changeRequestId, int initiatedById);
    
    /// <summary>
    /// Submit a vote/decision for a CAB approval stage.
    /// </summary>
    Task<bool> SubmitApprovalAsync(int changeRequestId, int approverId, bool isApproved, string? comments = null);
    
    /// <summary>
    /// Get the current CAB workflow status for a change request.
    /// </summary>
    Task<CABWorkflowStatus?> GetWorkflowStatusAsync(int changeRequestId);
    
    /// <summary>
    /// Get pending approvals for a specific approver.
    /// </summary>
    Task<List<PendingChangeApproval>> GetPendingApprovalsAsync(int approverId);
    
    /// <summary>
    /// Check if a change request requires CAB review.
    /// </summary>
    Task<bool> RequiresCABReviewAsync(int changeRequestId);
    
    /// <summary>
    /// Schedule a CAB meeting for reviewing changes.
    /// </summary>
    Task<int> ScheduleCABMeetingAsync(DateTime meetingDate, List<int> changeRequestIds, int scheduledById);
    
    /// <summary>
    /// Get the approval chain for a change type and risk level.
    /// </summary>
    Task<List<ApprovalLevel>> GetApprovalChainAsync(ChangeType changeType, string riskLevel);
}

/// <summary>
/// Represents a CAB workflow instance for a change request.
/// </summary>
public class CABWorkflow
{
    public int WorkflowId { get; set; }
    public int ChangeRequestId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime InitiatedAt { get; set; }
    public int InitiatedById { get; set; }
    public List<CABApprovalStage> Stages { get; set; } = new();
}

/// <summary>
/// Represents a single approval stage in the CAB workflow.
/// </summary>
public class CABApprovalStage
{
    public int StageId { get; set; }
    public int StageNumber { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public int? ApprovalGroupId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Skipped
    public DateTime? DecisionAt { get; set; }
    public string? Comments { get; set; }
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// Current workflow status summary.
/// </summary>
public class CABWorkflowStatus
{
    public int ChangeRequestId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string OverallStatus { get; set; } = "Pending";
    public int CurrentStage { get; set; }
    public int TotalStages { get; set; }
    public List<CABApprovalStage> Stages { get; set; } = new();
    public DateTime? CompletedAt { get; set; }
    public bool IsFullyApproved { get; set; }
    public bool IsRejected { get; set; }
}

/// <summary>
/// Pending approval for an approver.
/// </summary>
public class PendingChangeApproval
{
    public int ChangeRequestId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string ChangeTitle { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public int CurrentStage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
}

/// <summary>
/// Approval level in the chain.
/// </summary>
public class ApprovalLevel
{
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "User"; // User, Group, Role
    public int? ApproverId { get; set; }
    public int? GroupId { get; set; }
    public string? RoleName { get; set; }
    public bool IsRequired { get; set; } = true;
    public int TimeoutHours { get; set; } = 72;
}

/// <summary>
/// Service for managing Change Advisory Board workflows and approvals.
/// </summary>
public class CABWorkflowService : ICABWorkflowService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<CABWorkflowService> _logger;

    public CABWorkflowService(
        IDbContextResolver dbContextResolver,
        ILogger<CABWorkflowService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<CABWorkflow> InitializeWorkflowAsync(int changeRequestId, int initiatedById)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var change = await context.Changes
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);
        
        if (change == null)
        {
            throw new ArgumentException($"Change request {changeRequestId} not found");
        }

        // Get approval chain based on change type and risk
        var approvalChain = await GetApprovalChainAsync(change.ChangeType, change.RiskLevel ?? "Medium");
        
        var workflow = new CABWorkflow
        {
            ChangeRequestId = changeRequestId,
            Status = "In Progress",
            InitiatedAt = DateTime.UtcNow,
            InitiatedById = initiatedById,
            Stages = approvalChain.Select((level, index) => new CABApprovalStage
            {
                StageNumber = index + 1,
                StageName = level.Name,
                ApproverId = level.ApproverId,
                ApprovalGroupId = level.GroupId,
                Status = index == 0 ? "Pending" : "Waiting",
                IsRequired = level.IsRequired
            }).ToList()
        };

        // Update change status
        change.Status = ChangeState.AwaitingApproval;
        change.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();

        // Store workflow in database - using ChangeApprovals table
        foreach (var stage in workflow.Stages)
        {
            var approval = new ChangeApproval
            {
                ChangeRequestId = changeRequestId,
                ApproverLevel = stage.StageNumber,
                ApproverName = stage.StageName,
                ApproverId = stage.ApproverId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            context.ChangeApprovals.Add(approval);
        }

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Initialized CAB workflow for change {ChangeNumber} with {StageCount} stages",
            change.Number, workflow.Stages.Count);

        return workflow;
    }

    public async Task<bool> SubmitApprovalAsync(int changeRequestId, int approverId, bool isApproved, string? comments = null)
    {
        var context = _dbContextResolver.ResolveContext();
        
        // Get current pending approval for this approver
        var approval = await context.ChangeApprovals
            .Where(a => a.ChangeRequestId == changeRequestId && 
                       a.ApproverId == approverId &&
                       a.Status == "Pending")
            .FirstOrDefaultAsync();

        if (approval == null)
        {
            _logger.LogWarning("No pending approval found for change {ChangeId} and approver {ApproverId}",
                changeRequestId, approverId);
            return false;
        }

        approval.Status = isApproved ? "Approved" : "Rejected";
        approval.DecisionDate = DateTime.UtcNow;
        approval.Comments = comments;

        var change = await context.Changes
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change == null)
        {
            return false;
        }

        if (!isApproved)
        {
            // Rejection - fail the entire workflow
            change.Status = ChangeState.Rejected;
            change.UpdatedAt = DateTime.UtcNow;

            // Add note
            var note = new ChangeNote
            {
                ChangeRequestId = changeRequestId,
                NoteText = $"Change rejected at approval stage {approval.ApproverLevel} ({approval.ApproverName}). Reason: {comments ?? "No reason provided"}",
                IsInternal = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = approverId
            };
            context.ITSMChangeNotes.Add(note);
        }
        else
        {
            // Check if there are more approvals needed
            var allApprovals = await context.ChangeApprovals
                .Where(a => a.ChangeRequestId == changeRequestId)
                .OrderBy(a => a.ApproverLevel)
                .ToListAsync();

            var pendingApprovals = allApprovals.Where(a => a.Status == "Pending").ToList();
            
            if (pendingApprovals.Count == 0)
            {
                // All approvals complete - change is approved
                change.Status = ChangeState.Approved;
                change.ApprovedAt = DateTime.UtcNow;
                change.ApprovedById = approverId;
                change.UpdatedAt = DateTime.UtcNow;

                var note = new ChangeNote
                {
                    ChangeRequestId = changeRequestId,
                    NoteText = "All CAB approvals received. Change is approved and ready for scheduling.",
                    IsInternal = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = approverId
                };
                context.ITSMChangeNotes.Add(note);
            }
            else
            {
                // Activate next approval stage
                var nextApproval = pendingApprovals.First();
                // Notify next approver (would integrate with notification service)
            }
        }

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Approval submitted for change {ChangeId}: {Decision} by approver {ApproverId}",
            changeRequestId, isApproved ? "Approved" : "Rejected", approverId);

        return true;
    }

    public async Task<CABWorkflowStatus?> GetWorkflowStatusAsync(int changeRequestId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var change = await context.Changes
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change == null) return null;

        var approvals = await context.ChangeApprovals
            .Where(a => a.ChangeRequestId == changeRequestId)
            .OrderBy(a => a.ApproverLevel)
            .ToListAsync();

        if (approvals.Count == 0) return null;

        var status = new CABWorkflowStatus
        {
            ChangeRequestId = changeRequestId,
            ChangeNumber = change.Number,
            TotalStages = approvals.Count,
            Stages = approvals.Select(a => new CABApprovalStage
            {
                StageNumber = a.ApproverLevel,
                StageName = a.ApproverName ?? $"Level {a.ApproverLevel}",
                ApproverId = a.ApproverId,
                Status = a.Status ?? "Pending",
                DecisionAt = a.DecisionDate,
                Comments = a.Comments
            }).ToList()
        };

        // Calculate current stage
        var pendingStages = status.Stages.Where(s => s.Status == "Pending").ToList();
        status.CurrentStage = pendingStages.Any() 
            ? pendingStages.Min(s => s.StageNumber) 
            : status.TotalStages;

        // Determine overall status
        status.IsRejected = status.Stages.Any(s => s.Status == "Rejected");
        status.IsFullyApproved = !status.IsRejected && status.Stages.All(s => s.Status == "Approved");
        status.OverallStatus = status.IsRejected ? "Rejected" 
            : status.IsFullyApproved ? "Approved" 
            : "In Progress";

        if (status.IsFullyApproved || status.IsRejected)
        {
            status.CompletedAt = status.Stages
                .Where(s => s.DecisionAt.HasValue)
                .Max(s => s.DecisionAt);
        }

        return status;
    }

    public async Task<List<PendingChangeApproval>> GetPendingApprovalsAsync(int approverId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var pendingApprovals = await context.ChangeApprovals
            .Where(a => a.ApproverId == approverId && a.Status == "Pending")
            .Join(context.Changes,
                a => a.ChangeRequestId,
                c => c.ChangeRequestId,
                (a, c) => new { Approval = a, Change = c })
            .ToListAsync();

        return pendingApprovals.Select(x => new PendingApproval
        {
            ChangeRequestId = x.Change.ChangeRequestId,
            ChangeNumber = x.Change.Number,
            ChangeTitle = x.Change.Title,
            ChangeType = x.Change.ChangeType,
            RiskLevel = x.Change.RiskLevel ?? "Medium",
            RequestedAt = x.Approval.CreatedAt,
            CurrentStage = x.Approval.ApproverLevel,
            StageName = x.Approval.ApproverName ?? $"Level {x.Approval.ApproverLevel}"
        }).ToList();
    }

    public async Task<bool> RequiresCABReviewAsync(int changeRequestId)
    {
        var context = _dbContextResolver.ResolveContext();
        
        var change = await context.Changes
            .FirstOrDefaultAsync(c => c.ChangeRequestId == changeRequestId);

        if (change == null) return false;

        // Standard and Emergency changes require CAB review
        // Normal changes with high risk also require CAB
        return change.ChangeType == ChangeType.Standard ||
               change.ChangeType == ChangeType.Emergency ||
               change.RiskLevel?.ToLower() == "high" ||
               change.RiskLevel?.ToLower() == "critical";
    }

    public async Task<int> ScheduleCABMeetingAsync(DateTime meetingDate, List<int> changeRequestIds, int scheduledById)
    {
        var context = _dbContextResolver.ResolveContext();
        
        // This would create a CAB meeting record
        // For now, just add notes to each change
        foreach (var changeId in changeRequestIds)
        {
            var note = new ChangeNote
            {
                ChangeRequestId = changeId,
                NoteText = $"Scheduled for CAB review on {meetingDate:g}",
                IsInternal = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = scheduledById
            };
            context.ITSMChangeNotes.Add(note);
        }

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Scheduled CAB meeting for {Date} with {Count} changes",
            meetingDate, changeRequestIds.Count);

        return changeRequestIds.Count;
    }

    public Task<List<ApprovalLevel>> GetApprovalChainAsync(ChangeType changeType, string riskLevel)
    {
        // Default approval chains based on change type and risk
        var chain = new List<ApprovalLevel>();

        switch (changeType)
        {
            case ChangeType.Standard:
                // Standard changes have a single-level approval
                chain.Add(new ApprovalLevel
                {
                    Level = 1,
                    Name = "Change Manager",
                    Type = "Role",
                    RoleName = "ChangeManager",
                    IsRequired = true,
                    TimeoutHours = 48
                });
                break;

            case ChangeType.Normal:
                // Normal changes based on risk
                chain.Add(new ApprovalLevel
                {
                    Level = 1,
                    Name = "Technical Lead",
                    Type = "Role",
                    RoleName = "TechLead",
                    IsRequired = true,
                    TimeoutHours = 48
                });

                if (riskLevel.ToLower() is "high" or "critical")
                {
                    chain.Add(new ApprovalLevel
                    {
                        Level = 2,
                        Name = "CAB Review",
                        Type = "Group",
                        GroupId = 1, // CAB group
                        IsRequired = true,
                        TimeoutHours = 72
                    });
                }
                break;

            case ChangeType.Emergency:
                // Emergency changes need expedited approval
                chain.Add(new ApprovalLevel
                {
                    Level = 1,
                    Name = "Emergency CAB",
                    Type = "Role",
                    RoleName = "ECAB",
                    IsRequired = true,
                    TimeoutHours = 4
                });
                chain.Add(new ApprovalLevel
                {
                    Level = 2,
                    Name = "IT Director",
                    Type = "Role",
                    RoleName = "ITDirector",
                    IsRequired = true,
                    TimeoutHours = 2
                });
                break;
        }

        return Task.FromResult(chain);
    }
}


#endif
