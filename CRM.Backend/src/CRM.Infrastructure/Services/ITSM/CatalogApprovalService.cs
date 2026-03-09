// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for service catalog request approval workflows.
/// </summary>
public interface ICatalogApprovalService
{
    /// <summary>
    /// Submit a service request for approval.
    /// </summary>
    Task<ApprovalWorkflow> SubmitForApprovalAsync(int serviceRequestId, int submittedById);

    /// <summary>
    /// Process an approval action (approve/reject).
    /// </summary>
    Task<ApprovalAction> ProcessApprovalAsync(int workflowId, int approverId, ApprovalDecision decision, string? comments = null);

    /// <summary>
    /// Get the current approval status for a service request.
    /// </summary>
    Task<ApprovalWorkflow?> GetApprovalStatusAsync(int serviceRequestId);

    /// <summary>
    /// Get pending approvals for a specific approver.
    /// </summary>
    Task<List<PendingServiceRequestApproval>> GetPendingApprovalsAsync(int approverId);

    /// <summary>
    /// Escalate an overdue approval.
    /// </summary>
    Task<bool> EscalateApprovalAsync(int workflowId, string reason);

    /// <summary>
    /// Withdraw a pending approval request.
    /// </summary>
    Task<bool> WithdrawApprovalAsync(int serviceRequestId, int requestedById, string reason);

    /// <summary>
    /// Get approval history for a service request.
    /// </summary>
    Task<List<ApprovalAction>> GetApprovalHistoryAsync(int serviceRequestId);

    /// <summary>
    /// Get approval rules for a catalog item.
    /// </summary>
    Task<CatalogApprovalRule> GetApprovalRuleAsync(int catalogItemId);
}

/// <summary>
/// Approval workflow for a service request.
/// </summary>
public class ApprovalWorkflow
{
    public int WorkflowId { get; set; }
    public int ServiceRequestId { get; set; }
    public string ServiceRequestNumber { get; set; } = string.Empty;
    public int CatalogItemId { get; set; }
    public string CatalogItemName { get; set; } = string.Empty;
    public WorkflowState State { get; set; }
    public int CurrentStage { get; set; }
    public int TotalStages { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int SubmittedById { get; set; }
    public string? SubmittedByName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ApprovalOutcome? FinalOutcome { get; set; }
    public List<ApprovalStage> Stages { get; set; } = new();
    public TimeSpan? EstimatedCompletionTime { get; set; }
}

public enum WorkflowState
{
    Pending,
    InProgress,
    Approved,
    Rejected,
    Withdrawn,
    Escalated,
    Expired
}

public enum ApprovalOutcome
{
    Approved,
    Rejected,
    PartiallyApproved,
    Withdrawn
}

/// <summary>
/// A stage in the approval workflow.
/// </summary>
public class ApprovalStage
{
    public int StageId { get; set; }
    public int StageNumber { get; set; }
    public string StageName { get; set; } = string.Empty;
    public ApproverType ApproverType { get; set; }
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int? ApproverGroupId { get; set; }
    public string? ApproverGroupName { get; set; }
    public StageState State { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ApprovalDecision? Decision { get; set; }
    public string? Comments { get; set; }
    public bool IsRequired { get; set; } = true;
}

public enum ApproverType
{
    Manager,
    SpecificUser,
    GroupMember,
    CostCenter,
    Custom
}

public enum StageState
{
    Pending,
    Active,
    Completed,
    Skipped,
    Escalated
}

public enum ApprovalDecision
{
    Approve,
    Reject,
    RequestInfo,
    Delegate
}

/// <summary>
/// An approval action taken.
/// </summary>
public class ApprovalAction
{
    public int ActionId { get; set; }
    public int WorkflowId { get; set; }
    public int StageId { get; set; }
    public ApprovalDecision Decision { get; set; }
    public int ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime ActionAt { get; set; }
    public string? Comments { get; set; }
    public int? DelegatedToId { get; set; }
    public string? DelegatedToName { get; set; }
}

/// <summary>
/// A pending approval item for an approver.
/// </summary>
public class PendingServiceRequestApproval
{
    public int WorkflowId { get; set; }
    public int StageId { get; set; }
    public int ServiceRequestId { get; set; }
    public string ServiceRequestNumber { get; set; } = string.Empty;
    public string CatalogItemName { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public int DaysOverdue { get; set; }
    public bool IsUrgent { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Justification { get; set; }
}

/// <summary>
/// Approval rule configuration for a catalog item.
/// </summary>
public class CatalogApprovalRule
{
    public int RuleId { get; set; }
    public int CatalogItemId { get; set; }
    public string CatalogItemName { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public List<ApprovalStageDefinition> StageDefinitions { get; set; } = new();
    public int DefaultApprovalDays { get; set; } = 3;
    public bool AutoApproveIfUnderCost { get; set; }
    public decimal? AutoApproveCostThreshold { get; set; }
    public bool EscalateOnOverdue { get; set; } = true;
    public int EscalateAfterDays { get; set; } = 2;
}

/// <summary>
/// Definition of an approval stage in a rule.
/// </summary>
public class ApprovalStageDefinition
{
    public int StageNumber { get; set; }
    public string StageName { get; set; } = string.Empty;
    public ApproverType ApproverType { get; set; }
    public int? SpecificApproverId { get; set; }
    public int? ApproverGroupId { get; set; }
    public bool IsRequired { get; set; } = true;
    public int SLAHours { get; set; } = 72;
}

/// <summary>
/// Service for managing catalog request approval workflows.
/// </summary>
public class CatalogApprovalService : ICatalogApprovalService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CatalogApprovalService> _logger;

    // In-memory storage for workflows (would be database in production)
    private static readonly List<ApprovalWorkflow> _workflows = new();
    private static readonly List<ApprovalAction> _actions = new();
    private static readonly Dictionary<int, CatalogApprovalRule> _rules = new();
    private static readonly object _collectionsLock = new();
    private static int _nextWorkflowId = 0;
    private static int _nextStageId = 0;
    private static int _nextActionId = 0;

    static CatalogApprovalService()
    {
        // Initialize default approval rules
        _rules[1] = new CatalogApprovalRule
        {
            RuleId = 1,
            CatalogItemId = 1,
            CatalogItemName = "New Laptop Request",
            RequiresApproval = true,
            StageDefinitions = new List<ApprovalStageDefinition>
            {
                new() { StageNumber = 1, StageName = "Manager Approval", ApproverType = ApproverType.Manager, IsRequired = true, SLAHours = 48 },
                new() { StageNumber = 2, StageName = "IT Approval", ApproverType = ApproverType.GroupMember, ApproverGroupId = 1, IsRequired = true, SLAHours = 24 }
            },
            AutoApproveIfUnderCost = true,
            AutoApproveCostThreshold = 500m,
            EscalateOnOverdue = true,
            EscalateAfterDays = 2
        };

        _rules[2] = new CatalogApprovalRule
        {
            RuleId = 2,
            CatalogItemId = 2,
            CatalogItemName = "Software License Request",
            RequiresApproval = true,
            StageDefinitions = new List<ApprovalStageDefinition>
            {
                new() { StageNumber = 1, StageName = "Manager Approval", ApproverType = ApproverType.Manager, IsRequired = true, SLAHours = 48 },
                new() { StageNumber = 2, StageName = "Software Review", ApproverType = ApproverType.GroupMember, ApproverGroupId = 2, IsRequired = true, SLAHours = 72 },
                new() { StageNumber = 3, StageName = "Finance Approval", ApproverType = ApproverType.GroupMember, ApproverGroupId = 3, IsRequired = false, SLAHours = 48 }
            },
            AutoApproveIfUnderCost = false,
            EscalateOnOverdue = true,
            EscalateAfterDays = 3
        };

        _rules[3] = new CatalogApprovalRule
        {
            RuleId = 3,
            CatalogItemId = 3,
            CatalogItemName = "Password Reset",
            RequiresApproval = false,
            StageDefinitions = new List<ApprovalStageDefinition>()
        };
    }

    public CatalogApprovalService(
        ICrmDbContext context,
        ILogger<CatalogApprovalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApprovalWorkflow> SubmitForApprovalAsync(int serviceRequestId, int submittedById)
    {
        var serviceRequest = await _context.CatalogRequests
            .Include(sr => sr.CatalogItem)
            .Include(sr => sr.RequestedBy)
            .FirstOrDefaultAsync(sr => sr.RequestId == serviceRequestId);

        if (serviceRequest == null)
        {
            throw new ArgumentException($"Service request {serviceRequestId} not found");
        }

        // Get approval rule for this catalog item
        var rule = await GetApprovalRuleAsync(serviceRequest.CatalogItemId);

        if (!rule.RequiresApproval)
        {
            // Auto-approve if no approval required
            return new ApprovalWorkflow
            {
                WorkflowId = 0,
                ServiceRequestId = serviceRequestId,
                State = WorkflowState.Approved,
                FinalOutcome = ApprovalOutcome.Approved,
                SubmittedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
        }

        // Check auto-approve threshold
        if (rule.AutoApproveIfUnderCost &&
            rule.AutoApproveCostThreshold.HasValue &&
            serviceRequest.CatalogItem?.Price < rule.AutoApproveCostThreshold)
        {
            return new ApprovalWorkflow
            {
                WorkflowId = 0,
                ServiceRequestId = serviceRequestId,
                State = WorkflowState.Approved,
                FinalOutcome = ApprovalOutcome.Approved,
                SubmittedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
        }

        // Create workflow
        var workflow = new ApprovalWorkflow
        {
            WorkflowId = Interlocked.Increment(ref _nextWorkflowId),
            ServiceRequestId = serviceRequestId,
            ServiceRequestNumber = $"CR-{serviceRequest.RequestId}",
            CatalogItemId = serviceRequest.CatalogItemId,
            CatalogItemName = serviceRequest.CatalogItem?.Name ?? "Unknown",
            State = WorkflowState.InProgress,
            CurrentStage = 1,
            TotalStages = rule.StageDefinitions.Count,
            SubmittedAt = DateTime.UtcNow,
            SubmittedById = submittedById,
            SubmittedByName = serviceRequest.RequestedBy != null ? $"{serviceRequest.RequestedBy.FirstName} {serviceRequest.RequestedBy.LastName}" : "Unknown",
            Stages = new List<ApprovalStage>()
        };

        // Create stages from rule
        foreach (var stageDef in rule.StageDefinitions)
        {
            var stage = new ApprovalStage
            {
                StageId = Interlocked.Increment(ref _nextStageId),
                StageNumber = stageDef.StageNumber,
                StageName = stageDef.StageName,
                ApproverType = stageDef.ApproverType,
                ApproverId = stageDef.SpecificApproverId,
                ApproverGroupId = stageDef.ApproverGroupId,
                IsRequired = stageDef.IsRequired,
                State = stageDef.StageNumber == 1 ? StageState.Active : StageState.Pending,
                DueAt = DateTime.UtcNow.AddHours(stageDef.SLAHours)
            };

            // Set approver name based on type
            if (stageDef.ApproverType == ApproverType.Manager)
            {
                stage.ApproverName = "Manager of Requestor";
            }
            else if (stageDef.ApproverGroupId.HasValue)
            {
                stage.ApproverGroupName = GetGroupName(stageDef.ApproverGroupId.Value);
            }

            if (stageDef.StageNumber == 1)
            {
                stage.AssignedAt = DateTime.UtcNow;
            }

            workflow.Stages.Add(stage);
        }

        workflow.EstimatedCompletionTime = TimeSpan.FromHours(
            rule.StageDefinitions.Sum(s => s.SLAHours));

        lock (_collectionsLock) { _workflows.Add(workflow); }

        // Update service request status
        serviceRequest.State = CatalogRequestState.PendingApproval;
        serviceRequest.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created approval workflow {WorkflowId} for service request {RequestId}",
            workflow.WorkflowId, serviceRequest.RequestId);

        return workflow;
    }

    public async Task<ApprovalAction> ProcessApprovalAsync(
        int workflowId,
        int approverId,
        ApprovalDecision decision,
        string? comments = null)
    {
        var workflow = _workflows.FirstOrDefault(w => w.WorkflowId == workflowId);
        if (workflow == null)
        {
            throw new ArgumentException($"Workflow {workflowId} not found");
        }

        var currentStage = workflow.Stages.FirstOrDefault(s => s.State == StageState.Active);
        if (currentStage == null)
        {
            throw new InvalidOperationException("No active stage found in workflow");
        }
        var approver = await _context.Users.FindAsync(approverId);

        // Record the action
        var action = new ApprovalAction
        {
            ActionId = Interlocked.Increment(ref _nextActionId),
            WorkflowId = workflowId,
            StageId = currentStage.StageId,
            Decision = decision,
            ApproverId = approverId,
            ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : "Unknown",
            ActionAt = DateTime.UtcNow,
            Comments = comments
        };

        lock (_collectionsLock) { _actions.Add(action); }

        // Update stage
        currentStage.Decision = decision;
        currentStage.CompletedAt = DateTime.UtcNow;
        currentStage.State = StageState.Completed;
        currentStage.Comments = comments;

        // Process decision
        switch (decision)
        {
            case ApprovalDecision.Approve:
                await ProcessApprovalDecisionAsync(workflow, _context);
                break;

            case ApprovalDecision.Reject:
                workflow.State = WorkflowState.Rejected;
                workflow.FinalOutcome = ApprovalOutcome.Rejected;
                workflow.CompletedAt = DateTime.UtcNow;
                await UpdateServiceRequestStatusAsync(workflow.ServiceRequestId, CatalogRequestState.Cancelled, _context);
                break;

            case ApprovalDecision.RequestInfo:
                // Keep stage active, request more info
                currentStage.State = StageState.Active;
                currentStage.CompletedAt = null;
                currentStage.Decision = null;
                break;

            case ApprovalDecision.Delegate:
                // Keep stage active, update approver
                currentStage.State = StageState.Active;
                currentStage.CompletedAt = null;
                currentStage.Decision = null;
                action.DelegatedToId = approverId; // Would be actual delegatee
                break;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Processed approval for workflow {WorkflowId}: {Decision} by {Approver}",
            workflowId, decision, approver != null ? $"{approver.FirstName} {approver.LastName}" : "Unknown");

        return action;
    }

    public async Task<ApprovalWorkflow?> GetApprovalStatusAsync(int serviceRequestId)
    {
        ApprovalWorkflow? wf;
        lock (_collectionsLock) { wf = _workflows.FirstOrDefault(w => w.ServiceRequestId == serviceRequestId); }
        return await Task.FromResult(wf);
    }

    public async Task<List<PendingServiceRequestApproval>> GetPendingApprovalsAsync(int approverId)
    {
        var pending = new List<PendingServiceRequestApproval>();

        List<ApprovalWorkflow> workflowSnapshot;
        lock (_collectionsLock) { workflowSnapshot = _workflows.ToList(); }
        foreach (var workflow in workflowSnapshot.Where(w => w.State == WorkflowState.InProgress))
        {
            var activeStage = workflow.Stages.FirstOrDefault(s => s.State == StageState.Active);
            if (activeStage == null) continue;

            // Check if this approver should handle this stage
            bool shouldHandle = false;

            if (activeStage.ApproverType == ApproverType.SpecificUser &&
                activeStage.ApproverId == approverId)
            {
                shouldHandle = true;
            }
            else if (activeStage.ApproverType == ApproverType.Manager)
            {
                // Would check if approver is manager of requestor
                shouldHandle = true; // Simplified for demo
            }
            else if (activeStage.ApproverType == ApproverType.GroupMember)
            {
                // Would check if approver is in the group
                shouldHandle = true; // Simplified for demo
            }

            if (shouldHandle)
            {
                var sr = await _context.CatalogRequests
                    .Include(r => r.RequestedBy)
                    .Include(r => r.CatalogItem)
                    .FirstOrDefaultAsync(r => r.RequestId == workflow.ServiceRequestId);

                if (sr != null)
                {
                    var daysOverdue = activeStage.DueAt.HasValue
                        ? (int)Math.Max(0, (DateTime.UtcNow - activeStage.DueAt.Value).TotalDays)
                        : 0;

                    pending.Add(new PendingServiceRequestApproval
                    {
                        WorkflowId = workflow.WorkflowId,
                        StageId = activeStage.StageId,
                        ServiceRequestId = workflow.ServiceRequestId,
                        ServiceRequestNumber = workflow.ServiceRequestNumber,
                        CatalogItemName = workflow.CatalogItemName,
                        RequestedByName = sr.RequestedBy != null ? $"{sr.RequestedBy.FirstName} {sr.RequestedBy.LastName}".Trim() : "Unknown",
                        SubmittedAt = workflow.SubmittedAt,
                        DueAt = activeStage.DueAt,
                        DaysOverdue = daysOverdue,
                        IsUrgent = (sr.CatalogItem?.Priority ?? 2) == 1 || daysOverdue > 0,
                        EstimatedCost = sr.CatalogItem?.Price,
                        Justification = sr.VariableValues
                    });
                }
            }
        }

        return pending.OrderByDescending(p => p.IsUrgent)
                      .ThenBy(p => p.SubmittedAt)
                      .ToList();
    }

    public async Task<bool> EscalateApprovalAsync(int workflowId, string reason)
    {
        ApprovalWorkflow? escalateWf;
        lock (_collectionsLock) { escalateWf = _workflows.FirstOrDefault(w => w.WorkflowId == workflowId); }
        var workflow = escalateWf;
        if (workflow == null) return false;

        var activeStage = workflow.Stages.FirstOrDefault(s => s.State == StageState.Active);
        if (activeStage == null) return false;

        activeStage.State = StageState.Escalated;
        workflow.State = WorkflowState.Escalated;

        _logger.LogWarning(
            "Escalated workflow {WorkflowId} at stage {Stage}: {Reason}",
            workflowId, activeStage.StageName, reason);

        return await Task.FromResult(true);
    }

    public async Task<bool> WithdrawApprovalAsync(int serviceRequestId, int requestedById, string reason)
    {
        ApprovalWorkflow? withdrawWf;
        lock (_collectionsLock)
        {
            withdrawWf = _workflows.FirstOrDefault(w =>
                w.ServiceRequestId == serviceRequestId &&
                w.SubmittedById == requestedById);
        }

        if (withdrawWf == null) return false;

        var workflow = withdrawWf;

        if (workflow.State != WorkflowState.InProgress && workflow.State != WorkflowState.Pending)
        {
            return false; // Can't withdraw completed workflows
        }

        workflow.State = WorkflowState.Withdrawn;
        workflow.FinalOutcome = ApprovalOutcome.Withdrawn;
        workflow.CompletedAt = DateTime.UtcNow;

        // Record withdrawal action
        lock (_collectionsLock)
        {
            _actions.Add(new ApprovalAction
            {
                ActionId = Interlocked.Increment(ref _nextActionId),
                WorkflowId = workflow.WorkflowId,
                StageId = workflow.CurrentStage,
                Decision = ApprovalDecision.Reject,
                ApproverId = requestedById,
                ActionAt = DateTime.UtcNow,
                Comments = $"Withdrawn: {reason}"
            });
        }
        await UpdateServiceRequestStatusAsync(serviceRequestId, CatalogRequestState.Cancelled, _context);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Workflow {WorkflowId} withdrawn for service request {RequestId}",
            workflow.WorkflowId, serviceRequestId);

        return true;
    }

    public async Task<List<ApprovalAction>> GetApprovalHistoryAsync(int serviceRequestId)
    {
        ApprovalWorkflow? histWf;
        List<ApprovalAction> actionsSnapshot;
        lock (_collectionsLock)
        {
            histWf = _workflows.FirstOrDefault(w => w.ServiceRequestId == serviceRequestId);
            actionsSnapshot = _actions.ToList();
        }

        if (histWf == null) return new List<ApprovalAction>();

        return await Task.FromResult(
            actionsSnapshot.Where(a => a.WorkflowId == histWf.WorkflowId)
                    .OrderBy(a => a.ActionAt)
                    .ToList());
    }

    public async Task<CatalogApprovalRule> GetApprovalRuleAsync(int catalogItemId)
    {
        if (_rules.TryGetValue(catalogItemId, out var rule))
        {
            return await Task.FromResult(rule);
        }

        // Default rule if not configured
        return new CatalogApprovalRule
        {
            RuleId = 0,
            CatalogItemId = catalogItemId,
            RequiresApproval = true,
            StageDefinitions = new List<ApprovalStageDefinition>
            {
                new()
                {
                    StageNumber = 1,
                    StageName = "Manager Approval",
                    ApproverType = ApproverType.Manager,
                    IsRequired = true,
                    SLAHours = 72
                }
            },
            DefaultApprovalDays = 3,
            EscalateOnOverdue = true,
            EscalateAfterDays = 2
        };
    }

    private async Task ProcessApprovalDecisionAsync(ApprovalWorkflow workflow, ICrmDbContext context)
    {
        // Check if there are more stages
        var nextStage = workflow.Stages
            .Where(s => s.State == StageState.Pending)
            .OrderBy(s => s.StageNumber)
            .FirstOrDefault();

        if (nextStage != null)
        {
            // Activate next stage
            nextStage.State = StageState.Active;
            nextStage.AssignedAt = DateTime.UtcNow;
            workflow.CurrentStage = nextStage.StageNumber;
        }
        else
        {
            // All stages completed - fully approved
            workflow.State = WorkflowState.Approved;
            workflow.FinalOutcome = ApprovalOutcome.Approved;
            workflow.CompletedAt = DateTime.UtcNow;

            await UpdateServiceRequestStatusAsync(
                workflow.ServiceRequestId,
                CatalogRequestState.InProgress,
                _context);
        }
    }

    private async Task UpdateServiceRequestStatusAsync(
        int serviceRequestId,
        CatalogRequestState status,
        ICrmDbContext context)
    {
        var sr = await _context.CatalogRequests.FindAsync(serviceRequestId);
        if (sr != null)
        {
            sr.State = status;
            sr.ModifiedAt = DateTime.UtcNow;
        }
    }

    private static string GetGroupName(int groupId)
    {
        return groupId switch
        {
            1 => "IT Department",
            2 => "Software Review Board",
            3 => "Finance Department",
            _ => $"Group {groupId}"
        };
    }
}


