// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Services;

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing approval workflows - quote/discount approvals with matrix-based routing,
/// multi-level approval chains, escalation, and tracking.
/// </summary>
public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ApprovalWorkflowService> _logger;
    private readonly INotificationPort? _notificationPort;

    public ApprovalWorkflowService(
        ICrmDbContext context,
        ILogger<ApprovalWorkflowService> logger,
        INotificationPort? notificationPort = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationPort = notificationPort;
    }

    #region Approval Matrix Management

    public async Task<IEnumerable<DiscountApprovalMatrix>> GetAllMatricesAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DiscountApprovalMatrices
            .Include(m => m.Levels.OrderBy(l => l.LevelOrder))
            .Where(m => !m.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        return await query.OrderBy(m => m.Priority).ThenBy(m => m.Name).ToListAsync(cancellationToken);
    }

    public async Task<DiscountApprovalMatrix?> GetMatrixByIdAsync(int matrixId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountApprovalMatrices
            .Include(m => m.Levels.OrderBy(l => l.LevelOrder))
            .FirstOrDefaultAsync(m => m.Id == matrixId && !m.IsDeleted, cancellationToken);
    }

    public async Task<DiscountApprovalMatrix> CreateMatrixAsync(DiscountApprovalMatrix matrix, CancellationToken cancellationToken = default)
    {
        matrix.CreatedAt = DateTime.UtcNow;
        _context.DiscountApprovalMatrices.Add(matrix);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created approval matrix {MatrixId}: {MatrixName}", matrix.Id, matrix.Name);
        return matrix;
    }

    public async Task<DiscountApprovalMatrix> UpdateMatrixAsync(DiscountApprovalMatrix matrix, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DiscountApprovalMatrices
            .FirstOrDefaultAsync(m => m.Id == matrix.Id && !m.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Matrix {matrix.Id} not found");

        existing.Name = matrix.Name;
        existing.Description = matrix.Description;
        existing.IsActive = matrix.IsActive;
        existing.Priority = matrix.Priority;
        existing.AppliesToAllProducts = matrix.AppliesToAllProducts;
        existing.ProductCategories = matrix.ProductCategories;
        existing.CustomerSegments = matrix.CustomerSegments;
        existing.Regions = matrix.Regions;
        existing.RequireAllLevels = matrix.RequireAllLevels;
        existing.AllowParallelApproval = matrix.AllowParallelApproval;
        existing.AutoEscalateHours = matrix.AutoEscalateHours;
        existing.ReminderHours = matrix.ReminderHours;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated approval matrix {MatrixId}: {MatrixName}", matrix.Id, matrix.Name);
        return existing;
    }

    public async Task<bool> DeleteMatrixAsync(int matrixId, CancellationToken cancellationToken = default)
    {
        var matrix = await _context.DiscountApprovalMatrices
            .FirstOrDefaultAsync(m => m.Id == matrixId && !m.IsDeleted, cancellationToken);

        if (matrix == null)
            return false;

        matrix.IsDeleted = true;
        matrix.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted approval matrix {MatrixId}", matrixId);
        return true;
    }

    public async Task<DiscountApprovalMatrix> ActivateMatrixAsync(int matrixId, CancellationToken cancellationToken = default)
    {
        var matrix = await GetMatrixByIdAsync(matrixId, cancellationToken)
            ?? throw new InvalidOperationException($"Matrix {matrixId} not found");

        matrix.IsActive = true;
        matrix.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return matrix;
    }

    public async Task<DiscountApprovalMatrix> DeactivateMatrixAsync(int matrixId, CancellationToken cancellationToken = default)
    {
        var matrix = await GetMatrixByIdAsync(matrixId, cancellationToken)
            ?? throw new InvalidOperationException($"Matrix {matrixId} not found");

        matrix.IsActive = false;
        matrix.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return matrix;
    }

    #endregion

    #region Approval Level Management

    public async Task<IEnumerable<ApprovalLevel>> GetMatrixLevelsAsync(int matrixId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalLevels
            .Include(l => l.ApproverUser)
            .Where(l => l.DiscountApprovalMatrixId == matrixId && !l.IsDeleted)
            .OrderBy(l => l.LevelOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApprovalLevel?> GetLevelByIdAsync(int levelId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalLevels
            .Include(l => l.ApproverUser)
            .Include(l => l.DiscountApprovalMatrix)
            .FirstOrDefaultAsync(l => l.Id == levelId && !l.IsDeleted, cancellationToken);
    }

    public async Task<ApprovalLevel> AddLevelAsync(int matrixId, ApprovalLevel level, CancellationToken cancellationToken = default)
    {
        level.DiscountApprovalMatrixId = matrixId;
        level.CreatedAt = DateTime.UtcNow;

        // Auto-assign order if not set
        if (level.LevelOrder == 0)
        {
            var maxOrder = await _context.ApprovalLevels
                .Where(l => l.DiscountApprovalMatrixId == matrixId && !l.IsDeleted)
                .MaxAsync(l => (int?)l.LevelOrder, cancellationToken) ?? 0;
            level.LevelOrder = maxOrder + 1;
        }

        _context.ApprovalLevels.Add(level);
        await _context.SaveChangesAsync(cancellationToken);
        return level;
    }

    public async Task<ApprovalLevel> UpdateLevelAsync(ApprovalLevel level, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ApprovalLevels
            .FirstOrDefaultAsync(l => l.Id == level.Id && !l.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Level {level.Id} not found");

        existing.Name = level.Name;
        existing.LevelOrder = level.LevelOrder;
        existing.ThresholdType = level.ThresholdType;
        existing.MinValue = level.MinValue;
        existing.MaxValue = level.MaxValue;
        existing.ApproverUserId = level.ApproverUserId;
        existing.ApproverRole = level.ApproverRole;
        existing.UseSubmitterManager = level.UseSubmitterManager;
        existing.ManagerLevelsUp = level.ManagerLevelsUp;
        existing.ApprovalGroupId = level.ApprovalGroupId;
        existing.RequireAllGroupMembers = level.RequireAllGroupMembers;
        existing.CanSkip = level.CanSkip;
        existing.AutoApproveIfSelf = level.AutoApproveIfSelf;
        existing.TimeoutHours = level.TimeoutHours;
        existing.EscalationUserId = level.EscalationUserId;
        existing.SendEmailOnPending = level.SendEmailOnPending;
        existing.NotificationTemplateId = level.NotificationTemplateId;
        existing.IncludeQuoteDetails = level.IncludeQuoteDetails;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveLevelAsync(int levelId, CancellationToken cancellationToken = default)
    {
        var level = await _context.ApprovalLevels
            .FirstOrDefaultAsync(l => l.Id == levelId && !l.IsDeleted, cancellationToken);

        if (level == null)
            return false;

        level.IsDeleted = true;
        level.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<ApprovalLevel>> ReorderLevelsAsync(
        int matrixId,
        IEnumerable<int> levelIdsInOrder,
        CancellationToken cancellationToken = default)
    {
        var levels = await _context.ApprovalLevels
            .Where(l => l.DiscountApprovalMatrixId == matrixId && !l.IsDeleted)
            .ToListAsync(cancellationToken);

        var order = 1;
        foreach (var levelId in levelIdsInOrder)
        {
            var level = levels.FirstOrDefault(l => l.Id == levelId);
            if (level != null)
            {
                level.LevelOrder = order++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return levels.OrderBy(l => l.LevelOrder);
    }

    #endregion

    #region Approval Group Management

    public async Task<IEnumerable<ApprovalGroup>> GetAllGroupsAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ApprovalGroups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .Where(g => !g.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(g => g.IsActive == isActive.Value);

        return await query.OrderBy(g => g.Name).ToListAsync(cancellationToken);
    }

    public async Task<ApprovalGroup?> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalGroups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted, cancellationToken);
    }

    public async Task<ApprovalGroup> CreateGroupAsync(ApprovalGroup group, CancellationToken cancellationToken = default)
    {
        group.CreatedAt = DateTime.UtcNow;
        _context.ApprovalGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created approval group {GroupId}: {GroupName}", group.Id, group.Name);
        return group;
    }

    public async Task<ApprovalGroup> UpdateGroupAsync(ApprovalGroup group, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ApprovalGroups
            .FirstOrDefaultAsync(g => g.Id == group.Id && !g.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Group {group.Id} not found");

        existing.Name = group.Name;
        existing.Description = group.Description;
        existing.IsActive = group.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var group = await _context.ApprovalGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted, cancellationToken);

        if (group == null)
            return false;

        group.IsDeleted = true;
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ApprovalGroupMember> AddGroupMemberAsync(
        int groupId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.ApprovalGroupMembers
            .FirstOrDefaultAsync(m => m.ApprovalGroupId == groupId && m.UserId == userId, cancellationToken);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.IsDeleted = false;
        }
        else
        {
            existing = new ApprovalGroupMember
            {
                ApprovalGroupId = groupId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.ApprovalGroupMembers.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveGroupMemberAsync(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.ApprovalGroupMembers
            .FirstOrDefaultAsync(m => m.ApprovalGroupId == groupId && m.UserId == userId, cancellationToken);

        if (member == null)
            return false;

        member.IsActive = false;
        member.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<ApprovalGroupMember>> GetGroupMembersAsync(
        int groupId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalGroupMembers
            .Include(m => m.User)
            .Where(m => m.ApprovalGroupId == groupId && m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.Order)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Approval Request Management

    public async Task<IEnumerable<ApprovalRequest>> GetAllRequestsAsync(
        DiscountApprovalStatus? status = null,
        int? submitterId = null,
        int? quoteId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ApprovalRequests
            .Include(r => r.Quote)
            .Include(r => r.Submitter)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .ThenInclude(s => s.AssignedTo)
            .Where(r => !r.IsDeleted);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (submitterId.HasValue)
            query = query.Where(r => r.SubmitterId == submitterId.Value);

        if (quoteId.HasValue)
            query = query.Where(r => r.QuoteId == quoteId.Value);

        return await query.OrderByDescending(r => r.SubmittedAt ?? r.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<ApprovalRequest?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(r => r.Quote)
            .Include(r => r.Submitter)
            .Include(r => r.DiscountApprovalMatrix)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .ThenInclude(s => s.AssignedTo)
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted, cancellationToken);
    }

    public async Task<ApprovalRequest?> GetRequestByNumberAsync(string requestNumber, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(r => r.Quote)
            .Include(r => r.Submitter)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber && !r.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<ApprovalRequest>> GetPendingApprovalsForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var requests = await _context.ApprovalRequests
            .Include(r => r.Quote)
            .Include(r => r.Submitter)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .ThenInclude(s => s.AssignedTo)
            .Where(r => r.Status == DiscountApprovalStatus.Pending && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        // Filter to requests with pending steps assigned to this user
        return requests.Where(r => r.Steps.Any(s =>
            s.Status == DiscountApprovalStatus.Pending &&
            s.AssignedToId == userId));
    }

    public async Task<IEnumerable<ApprovalRequest>> GetRequestsBySubmitterAsync(
        int submitterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(r => r.Quote)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .Where(r => r.SubmitterId == submitterId && !r.IsDeleted)
            .OrderByDescending(r => r.SubmittedAt ?? r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Approval Workflow Operations

    public async Task<ApprovalSubmissionResult> SubmitForApprovalAsync(
        int quoteId,
        int submitterId,
        string? justification = null,
        CancellationToken cancellationToken = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Account)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

        if (quote == null)
            return new ApprovalSubmissionResult { Success = false, ErrorMessage = "Quote not found" };

        // Determine approval requirements
        var requirements = await DetermineApprovalRequirementsAsync(quoteId, cancellationToken);

        if (!requirements.RequiresApproval)
        {
            return new ApprovalSubmissionResult
            {
                Success = true,
                RequiresApproval = false,
                AutoApproved = true
            };
        }

        // Create approval request
        var request = new ApprovalRequest
        {
            RequestNumber = $"APR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Status = DiscountApprovalStatus.Pending,
            DiscountApprovalMatrixId = requirements.ApplicableMatrixId,
            QuoteId = quoteId,
            DiscountPercent = requirements.DiscountPercent,
            DiscountAmount = requirements.DiscountAmount,
            DealAmount = requirements.DealAmount,
            Justification = justification,
            CurrentLevel = 1,
            MaxLevelRequired = requirements.RequiredLevels,
            SubmittedAt = DateTime.UtcNow,
            SubmitterId = submitterId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        // Create approval steps
        var approverNames = new List<string>();
        foreach (var levelInfo in requirements.Levels)
        {
            var step = new ApprovalStep
            {
                ApprovalRequestId = request.Id,
                StepOrder = levelInfo.LevelOrder,
                ApprovalLevelId = null, // Set if level has an ID
                Status = levelInfo.LevelOrder == 1 ? DiscountApprovalStatus.Pending : DiscountApprovalStatus.NotSubmitted,
                AssignedToId = levelInfo.ApproverUserId,
                AssignedAt = levelInfo.LevelOrder == 1 ? DateTime.UtcNow : null,
                DueAt = levelInfo.LevelOrder == 1 ? DateTime.UtcNow.AddHours(24) : null,
                CreatedAt = DateTime.UtcNow
            };
            _context.ApprovalSteps.Add(step);

            if (!string.IsNullOrEmpty(levelInfo.ApproverName))
                approverNames.Add(levelInfo.ApproverName);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Submitted approval request {RequestNumber} for quote {QuoteId}",
            request.RequestNumber, quoteId);

        return new ApprovalSubmissionResult
        {
            Success = true,
            Request = request,
            RequiresApproval = true,
            RequiredLevels = requirements.RequiredLevels,
            ApproverNames = approverNames
        };
    }

    public async Task<ApprovalRequirementResult> DetermineApprovalRequirementsAsync(
        int quoteId,
        CancellationToken cancellationToken = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

        if (quote == null)
            return new ApprovalRequirementResult { RequiresApproval = false };

        var totalAmount = quote.TotalAmount;
        var discountPercent = quote.DiscountPercent;
        var discountAmount = totalAmount * discountPercent / 100;

        // Find applicable matrix
        var matrix = await FindApplicableMatrixAsync(quoteId, cancellationToken);

        if (matrix == null || !matrix.Levels.Any())
        {
            return new ApprovalRequirementResult
            {
                RequiresApproval = false,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                DealAmount = totalAmount
            };
        }

        // Determine required levels based on discount
        var requiredLevels = matrix.Levels
            .Where(l => !l.IsDeleted)
            .Where(l => l.ThresholdType == ApprovalThresholdType.DiscountPercent &&
                       discountPercent >= l.MinValue &&
                       (!l.MaxValue.HasValue || discountPercent <= l.MaxValue))
            .OrderBy(l => l.LevelOrder)
            .ToList();

        if (!requiredLevels.Any())
        {
            return new ApprovalRequirementResult
            {
                RequiresApproval = false,
                ApplicableMatrixId = matrix.Id,
                MatrixName = matrix.Name,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                DealAmount = totalAmount
            };
        }

        var levelInfos = new List<ApprovalLevelInfo>();
        foreach (var level in requiredLevels)
        {
            var user = level.ApproverUserId.HasValue
                ? await _context.Users.FirstOrDefaultAsync(u => u.Id == level.ApproverUserId.Value, cancellationToken)
                : null;

            levelInfos.Add(new ApprovalLevelInfo
            {
                LevelOrder = level.LevelOrder,
                LevelName = level.Name,
                ApproverUserId = level.ApproverUserId,
                ApproverName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                ApproverRole = level.ApproverRole,
                ApprovalGroupId = level.ApprovalGroupId,
                ThresholdType = level.ThresholdType,
                MinValue = level.MinValue,
                MaxValue = level.MaxValue
            });
        }

        return new ApprovalRequirementResult
        {
            RequiresApproval = true,
            ApplicableMatrixId = matrix.Id,
            MatrixName = matrix.Name,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            DealAmount = totalAmount,
            RequiredLevels = requiredLevels.Count,
            Levels = levelInfos,
            ReasonForApproval = $"Discount of {discountPercent:F1}% requires {requiredLevels.Count} level(s) of approval"
        };
    }

    public async Task<ApprovalActionResult> ApproveStepAsync(
        int requestId,
        int approverId,
        string? comments = null,
        CancellationToken cancellationToken = default)
    {
        var request = await GetRequestByIdAsync(requestId, cancellationToken);
        if (request == null)
            return new ApprovalActionResult { Success = false, ErrorMessage = "Request not found" };

        // Find the pending step for this approver
        var currentStep = request.Steps
            .FirstOrDefault(s => s.Status == DiscountApprovalStatus.Pending && s.AssignedToId == approverId);

        if (currentStep == null)
            return new ApprovalActionResult { Success = false, ErrorMessage = "No pending step found for this approver" };

        // Approve the step
        currentStep.Status = DiscountApprovalStatus.Approved;
        currentStep.ActedById = approverId;
        currentStep.ActedAt = DateTime.UtcNow;
        currentStep.Comments = comments;

        // Check if this was the last level
        var nextStep = request.Steps
            .Where(s => s.StepOrder > currentStep.StepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (nextStep == null)
        {
            // All levels approved
            request.Status = DiscountApprovalStatus.Approved;
            request.CompletedAt = DateTime.UtcNow;
            request.TimeToApprovalHours = (decimal)(DateTime.UtcNow - request.SubmittedAt!.Value).TotalHours;

            _logger.LogInformation("Approval request {RequestNumber} fully approved", request.RequestNumber);
        }
        else
        {
            // Advance to next level
            nextStep.Status = DiscountApprovalStatus.Pending;
            nextStep.AssignedAt = DateTime.UtcNow;
            nextStep.DueAt = DateTime.UtcNow.AddHours(24);
            request.CurrentLevel = nextStep.StepOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var nextApprover = nextStep?.AssignedToId.HasValue == true
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == nextStep.AssignedToId.Value, cancellationToken)
            : null;

        return new ApprovalActionResult
        {
            Success = true,
            Request = request,
            CurrentStep = currentStep,
            IsFullyApproved = request.Status == DiscountApprovalStatus.Approved,
            NextLevelOrder = nextStep?.StepOrder,
            NextApproverName = nextApprover != null ? $"{nextApprover.FirstName} {nextApprover.LastName}" : null
        };
    }

    public async Task<ApprovalActionResult> RejectStepAsync(
        int requestId,
        int approverId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var request = await GetRequestByIdAsync(requestId, cancellationToken);
        if (request == null)
            return new ApprovalActionResult { Success = false, ErrorMessage = "Request not found" };

        var currentStep = request.Steps
            .FirstOrDefault(s => s.Status == DiscountApprovalStatus.Pending && s.AssignedToId == approverId);

        if (currentStep == null)
            return new ApprovalActionResult { Success = false, ErrorMessage = "No pending step found for this approver" };

        // Reject the step
        currentStep.Status = DiscountApprovalStatus.Rejected;
        currentStep.ActedById = approverId;
        currentStep.ActedAt = DateTime.UtcNow;
        currentStep.Comments = reason;

        // Reject the entire request
        request.Status = DiscountApprovalStatus.Rejected;
        request.CompletedAt = DateTime.UtcNow;
        request.FinalNotes = reason;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Approval request {RequestNumber} rejected: {Reason}", request.RequestNumber, reason);

        return new ApprovalActionResult
        {
            Success = true,
            Request = request,
            CurrentStep = currentStep,
            IsRejected = true
        };
    }

    public async Task<ApprovalRequest> RecallRequestAsync(
        int requestId,
        int userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var request = await GetRequestByIdAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Request {requestId} not found");

        if (request.SubmitterId != userId)
            throw new InvalidOperationException("Only the submitter can recall a request");

        if (request.Status != DiscountApprovalStatus.Pending)
            throw new InvalidOperationException("Can only recall pending requests");

        request.Status = DiscountApprovalStatus.Recalled;
        request.CompletedAt = DateTime.UtcNow;
        request.FinalNotes = reason ?? "Recalled by submitter";

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Approval request {RequestNumber} recalled by submitter", request.RequestNumber);

        return request;
    }

    public async Task<ApprovalStep> ReassignStepAsync(
        int stepId,
        int newAssigneeId,
        int reassignedById,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var step = await _context.ApprovalSteps
            .Include(s => s.ApprovalRequest)
            .FirstOrDefaultAsync(s => s.Id == stepId, cancellationToken)
            ?? throw new InvalidOperationException($"Step {stepId} not found");

        if (step.Status != DiscountApprovalStatus.Pending)
            throw new InvalidOperationException("Can only reassign pending steps");

        step.AssignedToId = newAssigneeId;
        step.AssignedAt = DateTime.UtcNow;
        step.Comments = $"Reassigned from user {step.AssignedToId} by user {reassignedById}. {reason}";

        await _context.SaveChangesAsync(cancellationToken);
        return step;
    }

    public async Task<ApprovalStep> EscalateStepAsync(
        int stepId,
        int escalatedById,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var step = await _context.ApprovalSteps
            .Include(s => s.ApprovalLevel)
            .Include(s => s.ApprovalRequest)
            .FirstOrDefaultAsync(s => s.Id == stepId, cancellationToken)
            ?? throw new InvalidOperationException($"Step {stepId} not found");

        if (step.Status != DiscountApprovalStatus.Pending)
            throw new InvalidOperationException("Can only escalate pending steps");

        var escalationUserId = step.ApprovalLevel?.EscalationUserId;
        if (!escalationUserId.HasValue)
            throw new InvalidOperationException("No escalation path defined for this level");

        step.WasEscalated = true;
        step.EscalatedToId = escalationUserId;
        step.EscalatedAt = DateTime.UtcNow;
        step.AssignedToId = escalationUserId;
        step.Comments = $"Escalated by user {escalatedById}. {reason}";

        if (step.ApprovalRequest != null)
            step.ApprovalRequest.Status = DiscountApprovalStatus.Escalated;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Step {StepId} escalated to user {UserId}", stepId, escalationUserId);

        return step;
    }

    #endregion

    #region Matrix Selection

    public async Task<DiscountApprovalMatrix?> FindApplicableMatrixAsync(
        int quoteId,
        CancellationToken cancellationToken = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.Account)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

        if (quote == null)
            return null;

        // Get active matrices ordered by priority
        var matrices = await _context.DiscountApprovalMatrices
            .Include(m => m.Levels.Where(l => !l.IsDeleted).OrderBy(l => l.LevelOrder))
            .Where(m => m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.Priority)
            .ToListAsync(cancellationToken);

        foreach (var matrix in matrices)
        {
            // Check if matrix applies to this quote
            if (!matrix.AppliesToAllProducts)
            {
                // Check product categories, customer segments, regions
                // This is a simplified check - full implementation would check all criteria
                if (!string.IsNullOrEmpty(matrix.ProductCategories))
                {
                    // Would check if quote products match categories
                }
            }

            // First matching matrix wins (by priority)
            if (matrix.Levels.Any())
                return matrix;
        }

        return matrices.FirstOrDefault();
    }

    public async Task<bool> RequiresApprovalAsync(
        decimal discountPercent,
        decimal? dealAmount = null,
        int? matrixId = null,
        CancellationToken cancellationToken = default)
    {
        DiscountApprovalMatrix? matrix;

        if (matrixId.HasValue)
        {
            matrix = await GetMatrixByIdAsync(matrixId.Value, cancellationToken);
        }
        else
        {
            // Get first active matrix
            matrix = await _context.DiscountApprovalMatrices
                .Include(m => m.Levels)
                .Where(m => m.IsActive && !m.IsDeleted)
                .OrderBy(m => m.Priority)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (matrix?.Levels == null || !matrix.Levels.Any())
            return false;

        return matrix.Levels.Any(l =>
            l.ThresholdType == ApprovalThresholdType.DiscountPercent &&
            discountPercent >= l.MinValue);
    }

    public async Task<decimal> GetUserApprovalLimitAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Find approval levels where this user is an approver
        var levels = await _context.ApprovalLevels
            .Where(l => l.ApproverUserId == userId && !l.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!levels.Any())
            return 0;

        // Return the maximum discount this user can approve
        return levels
            .Where(l => l.ThresholdType == ApprovalThresholdType.DiscountPercent)
            .Max(l => l.MaxValue ?? l.MinValue);
    }

    #endregion

    #region Notifications & Reminders

    public async Task<int> SendOverdueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var overdueSteps = await _context.ApprovalSteps
            .Include(s => s.ApprovalRequest)
            .Include(s => s.AssignedTo)
            .Where(s => s.Status == DiscountApprovalStatus.Pending &&
                       s.DueAt.HasValue &&
                       s.DueAt < DateTime.UtcNow &&
                       !s.ReminderSent)
            .ToListAsync(cancellationToken);

        foreach (var step in overdueSteps)
        {
            step.ReminderSent = true;
            step.ReminderSentAt = DateTime.UtcNow;

            _logger.LogInformation("Sending reminder for overdue step {StepId} assigned to user {UserId}",
                step.Id, step.AssignedToId);

            if (_notificationPort != null && step.AssignedTo?.Email != null)
            {
                try
                {
                    var email = new EmailNotificationRequest
                    {
                        To = step.AssignedTo.Email,
                        Subject = $"Reminder: Approval request #{step.ApprovalRequestId} is overdue",
                        Body = $"<p>Your approval for request <strong>#{step.ApprovalRequestId}</strong> " +
                               $"(Step {step.StepOrder}) is overdue.</p>" +
                               $"<p>Originally due: {step.DueAt:yyyy-MM-dd HH:mm} UTC</p>" +
                               $"<p>Please review and take action at your earliest convenience.</p>",
                        IsHtml = true,
                    };
                    await _notificationPort.SendEmailAsync(email, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send overdue reminder email for step {StepId} to {Email}",
                        step.Id, step.AssignedTo.Email);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return overdueSteps.Count;
    }

    public async Task<int> ProcessAutoEscalationsAsync(CancellationToken cancellationToken = default)
    {
        var stepsToEscalate = await _context.ApprovalSteps
            .Include(s => s.ApprovalLevel)
            .Include(s => s.ApprovalRequest)
            .Where(s => s.Status == DiscountApprovalStatus.Pending &&
                       s.ApprovalLevel != null &&
                       s.ApprovalLevel.TimeoutHours.HasValue &&
                       s.ApprovalLevel.EscalationUserId.HasValue &&
                       s.AssignedAt.HasValue &&
                       !s.WasEscalated)
            .ToListAsync(cancellationToken);

        var escalated = 0;
        foreach (var step in stepsToEscalate)
        {
            var timeout = step.ApprovalLevel!.TimeoutHours!.Value;
            if (step.AssignedAt!.Value.AddHours(timeout) < DateTime.UtcNow)
            {
                try
                {
                    await EscalateStepAsync(step.Id, 0, "Auto-escalated due to timeout", cancellationToken);
                    escalated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to auto-escalate step {StepId}", step.Id);
                }
            }
        }

        return escalated;
    }

    #endregion

    #region Statistics & Reporting

    public async Task<ApprovalStatistics> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var requests = await _context.ApprovalRequests
            .Include(r => r.DiscountApprovalMatrix)
            .Include(r => r.Steps)
            .Where(r => r.SubmittedAt >= from && r.SubmittedAt <= to && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var overdueSteps = await _context.ApprovalSteps
            .Where(s => s.Status == DiscountApprovalStatus.Pending &&
                       s.DueAt.HasValue &&
                       s.DueAt < DateTime.UtcNow)
            .CountAsync(cancellationToken);

        return new ApprovalStatistics
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == DiscountApprovalStatus.Pending),
            ApprovedRequests = requests.Count(r => r.Status == DiscountApprovalStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == DiscountApprovalStatus.Rejected),
            RecalledRequests = requests.Count(r => r.Status == DiscountApprovalStatus.Recalled),
            AutoApprovedRequests = requests.Count(r => r.Status == DiscountApprovalStatus.AutoApproved),
            AverageTimeToApprovalHours = requests.Where(r => r.TimeToApprovalHours.HasValue).Any()
                ? requests.Where(r => r.TimeToApprovalHours.HasValue).Average(r => r.TimeToApprovalHours!.Value)
                : 0,
            TotalDiscountApproved = requests.Where(r => r.Status == DiscountApprovalStatus.Approved).Sum(r => r.DiscountAmount),
            AverageDiscountPercent = requests.Any() ? requests.Average(r => r.DiscountPercent) : 0,
            OverdueSteps = overdueSteps,
            EscalatedRequests = requests.Count(r => r.Status == DiscountApprovalStatus.Escalated || r.Steps.Any(s => s.WasEscalated)),
            RequestsByStatus = requests.GroupBy(r => r.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            RequestsByMatrix = requests.Where(r => r.DiscountApprovalMatrix != null)
                .GroupBy(r => r.DiscountApprovalMatrix!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            FromDate = from,
            ToDate = to
        };
    }

    public async Task<IEnumerable<ApproverPerformance>> GetApproverPerformanceAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var steps = await _context.ApprovalSteps
            .Include(s => s.AssignedTo)
            .Where(s => s.AssignedAt >= from && s.AssignedAt <= to)
            .ToListAsync(cancellationToken);

        var approverStats = steps
            .Where(s => s.AssignedToId.HasValue)
            .GroupBy(s => s.AssignedToId!.Value)
            .Select(g =>
            {
                var user = g.First().AssignedTo;
                var completedSteps = g.Where(s => s.ActedAt.HasValue && s.AssignedAt.HasValue);
                var avgResponseHours = completedSteps.Any()
                    ? completedSteps.Average(s => (s.ActedAt!.Value - s.AssignedAt!.Value).TotalHours)
                    : 0;

                return new ApproverPerformance
                {
                    UserId = g.Key,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    TotalAssigned = g.Count(),
                    TotalApproved = g.Count(s => s.Status == DiscountApprovalStatus.Approved),
                    TotalRejected = g.Count(s => s.Status == DiscountApprovalStatus.Rejected),
                    TotalReassigned = 0, // Would need to track reassignments
                    TotalEscalated = g.Count(s => s.WasEscalated),
                    AverageResponseTimeHours = (decimal)avgResponseHours,
                    CurrentPending = g.Count(s => s.Status == DiscountApprovalStatus.Pending),
                    OverdueCount = g.Count(s => s.Status == DiscountApprovalStatus.Pending &&
                                               s.DueAt.HasValue &&
                                               s.DueAt < DateTime.UtcNow),
                    ApprovalRate = g.Any(s => s.Status == DiscountApprovalStatus.Approved ||
                                             s.Status == DiscountApprovalStatus.Rejected)
                        ? (double)g.Count(s => s.Status == DiscountApprovalStatus.Approved) /
                          g.Count(s => s.Status == DiscountApprovalStatus.Approved ||
                                      s.Status == DiscountApprovalStatus.Rejected) * 100
                        : 0
                };
            })
            .OrderByDescending(p => p.TotalAssigned)
            .ToList();

        return approverStats;
    }

    public async Task<IEnumerable<ApprovalRequest>> GetQuoteApprovalHistoryAsync(
        int quoteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(r => r.Submitter)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .ThenInclude(s => s.ActedBy)
            .Where(r => r.QuoteId == quoteId && !r.IsDeleted)
            .OrderByDescending(r => r.SubmittedAt ?? r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
