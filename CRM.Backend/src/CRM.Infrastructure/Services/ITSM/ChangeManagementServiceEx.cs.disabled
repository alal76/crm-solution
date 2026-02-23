// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Extended implementation of Change Management service handling the full change lifecycle.
/// Implements CRUD, workflow transitions, CAB management, impact analysis, rollback planning,
/// implementation tracking, conflict detection, blackout periods, comments, and metrics.
/// CAB functionality is modelled using ChangeApproval records (ApprovalRole.CABMember/CABChair)
/// and the Change.CABDate field since there is no dedicated CAB table.
/// </summary>
public class ChangeManagementServiceEx : IChangeManagementServiceEx
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ChangeManagementServiceEx> _logger;

    public ChangeManagementServiceEx(
        IDbContextResolver dbContextResolver,
        ILogger<ChangeManagementServiceEx> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    // =========================================================================
    // CRUD Operations
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeDto> CreateChangeAsync(CreateChangeDto dto, int requestorId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();

        var change = new Change
        {
            Number = await GenerateChangeNumberAsync(context, cancellationToken),
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Type = dto.Type,
            Risk = dto.Risk,
            Impact = dto.Impact,
            ImplementationPlan = dto.ImplementationPlan,
            BackoutPlan = dto.BackoutPlan,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            State = ChangeState.New,
            ApprovalStatus = ApprovalStatus.Requested,
            RequestorId = requestorId,
            CreatedById = requestorId,
            CreatedAt = DateTime.UtcNow
        };

        context.Changes.Add(change);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created change {ChangeNumber} by user {UserId}", change.Number, requestorId);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> GetChangeByIdAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes
            .Include(c => c.Requestor)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.ChangeId == changeId && !c.IsDeleted, cancellationToken);

        if (change == null)
            throw new KeyNotFoundException($"Change {changeId} not found");

        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<ChangeDto> Items, int TotalCount)> ListChangesAsync(
        ChangeFilterDto filter, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var query = context.Changes
            .Include(c => c.Requestor)
            .Include(c => c.AssignedTo)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(c => c.Number.Contains(filter.SearchTerm) ||
                                     c.ShortDescription.Contains(filter.SearchTerm));
        }

        if (filter.State.HasValue)
            query = query.Where(c => c.State == filter.State.Value);

        if (filter.Type.HasValue)
            query = query.Where(c => c.Type == filter.Type.Value);

        if (filter.ApprovalStatus.HasValue)
            query = query.Where(c => c.ApprovalStatus == filter.ApprovalStatus.Value);

        if (filter.PlannedStartFrom.HasValue)
            query = query.Where(c => c.PlannedStartDate.HasValue && c.PlannedStartDate.Value >= filter.PlannedStartFrom.Value);

        if (filter.PlannedStartTo.HasValue)
            query = query.Where(c => c.PlannedStartDate.HasValue && c.PlannedStartDate.Value <= filter.PlannedStartTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var changes = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<ChangeDto>();
        foreach (var change in changes)
            dtos.Add(await MapChangeToDtoAsync(change, context, cancellationToken));

        return (dtos, totalCount);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> UpdateChangeAsync(int changeId, CreateChangeDto dto, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        change.ShortDescription = dto.ShortDescription;
        change.Description = dto.Description;
        change.Type = dto.Type;
        change.Risk = dto.Risk;
        change.Impact = dto.Impact;
        change.PlannedStartDate = dto.PlannedStartDate;
        change.PlannedEndDate = dto.PlannedEndDate;
        change.ImplementationPlan = dto.ImplementationPlan;
        change.BackoutPlan = dto.BackoutPlan;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated change {ChangeId} by user {UserId}", changeId, modifiedById);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> CancelChangeAsync(int changeId, string reason, int cancelledById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        if (change.State == ChangeState.Closed || change.State == ChangeState.Implement)
            throw new InvalidOperationException($"Change in state {change.State} cannot be cancelled");

        change.State = ChangeState.Cancelled;
        change.ClosureNotes = reason;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cancelled change {ChangeNumber} by user {UserId}. Reason: {Reason}", change.Number, cancelledById, reason);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteChangeAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        change.IsDeleted = true;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted (soft) change {ChangeId}", changeId);
    }

    // =========================================================================
    // Workflow Operations
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeDto> RequestApprovalAsync(int changeId, int requestedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        if (change.State != ChangeState.New)
            throw new InvalidOperationException($"Change must be in New state to request approval, current state: {change.State}");

        change.State = ChangeState.Assess;
        change.ApprovalStatus = ApprovalStatus.Requested;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Requested approval for change {ChangeNumber} by user {UserId}", change.Number, requestedById);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeApprovalDto> ApproveChangeAsync(int changeId, int approverId, string? comments = null, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var approval = new ChangeApproval
        {
            ChangeId = changeId,
            ApproverId = approverId,
            ApprovalRole = ApprovalRole.ChangeManager,
            ApprovalStatus = ApprovalStatus.Approved,
            Comments = comments,
            ApprovalDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.ChangeApprovals.Add(approval);

        if (change.State == ChangeState.Assess || change.State == ChangeState.AwaitingApproval)
        {
            change.State = ChangeState.Authorize;
            change.ApprovalStatus = ApprovalStatus.Approved;
            change.ModifiedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Approved change {ChangeId} by approver {ApproverId}", changeId, approverId);

        return MapApprovalToDto(approval, null);
    }

    /// <inheritdoc/>
    public async Task<ChangeApprovalDto> RejectChangeAsync(int changeId, int approverId, string reason, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var approval = new ChangeApproval
        {
            ChangeId = changeId,
            ApproverId = approverId,
            ApprovalRole = ApprovalRole.ChangeManager,
            ApprovalStatus = ApprovalStatus.Rejected,
            Comments = reason,
            ApprovalDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.ChangeApprovals.Add(approval);

        change.State = ChangeState.Rejected;
        change.ApprovalStatus = ApprovalStatus.Rejected;
        change.ApprovalNotes = reason;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Rejected change {ChangeId} by approver {ApproverId}. Reason: {Reason}", changeId, approverId, reason);

        return MapApprovalToDto(approval, null);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> ScheduleChangeAsync(int changeId, ScheduleChangeImplementationDto dto, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        if (!dto.BypassCAB)
        {
            // Check for blackout conflicts
            var blackoutConflicts = await context.ChangeBlackouts
                .Where(b => !b.IsDeleted &&
                            ((b.StartDate <= dto.ScheduledStartDate && b.EndDate >= dto.ScheduledStartDate) ||
                             (b.StartDate <= dto.ScheduledEndDate && b.EndDate >= dto.ScheduledEndDate)))
                .Select(b => b.Name)
                .ToListAsync(cancellationToken);

            if (blackoutConflicts.Any())
                throw new InvalidOperationException($"Scheduling conflicts with blackout periods: {string.Join(", ", blackoutConflicts)}");
        }

        change.PlannedStartDate = dto.ScheduledStartDate;
        change.PlannedEndDate = dto.ScheduledEndDate;
        change.MaintenanceWindow = !string.IsNullOrEmpty(dto.MaintenanceWindow);
        change.State = ChangeState.Scheduled;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Scheduled change {ChangeNumber} from {Start} to {End}", change.Number, dto.ScheduledStartDate, dto.ScheduledEndDate);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> StartImplementationAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        if (change.State != ChangeState.Scheduled && change.State != ChangeState.Authorize)
            throw new InvalidOperationException($"Change must be Scheduled or Authorized to start implementation, current state: {change.State}");

        change.State = ChangeState.Implement;
        change.ActualStartDate = DateTime.UtcNow;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Started implementation of change {ChangeNumber} by user {UserId}", change.Number, modifiedById);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> CompleteChangeAsync(int changeId, bool successful, string? notes, int completedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        if (change.State != ChangeState.Implement)
            throw new InvalidOperationException($"Change must be in Implement state to complete, current state: {change.State}");

        change.State = successful ? ChangeState.Review : ChangeState.Failed;
        change.ChangeSuccess = successful;
        change.ActualEndDate = DateTime.UtcNow;
        change.ImplementationNotes = notes;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Completed change {ChangeNumber} - Successful: {Successful}", change.Number, successful);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> CloseChangeAsync(int changeId, string closureCode, string? postImplementationReview, int closedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        change.State = ChangeState.Closed;
        change.ClosureCode = closureCode;
        change.PostImplementationReview = postImplementationReview;
        change.ReviewDate = DateTime.UtcNow;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Closed change {ChangeNumber} with code {ClosureCode}", change.Number, closureCode);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    // =========================================================================
    // CAB Management
    // CAB functionality is backed by Change.CABDate + ChangeApproval records
    // with ApprovalRole = CABMember / CABChair. CABId == ChangeId (1-to-1).
    // =========================================================================

    /// <inheritdoc/>
    public async Task<CABDto> CreateCABAsync(CreateCABDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { dto.ChangeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {dto.ChangeId} not found");

        // Store the CAB date on the change
        change.CABDate = dto.ScheduledDate;
        change.ModifiedAt = DateTime.UtcNow;

        // Add each member as a ChangeApproval record with CABMember role
        var addedMembers = new List<ChangeApproval>();
        foreach (var memberId in dto.MemberIds)
        {
            var existing = await context.ChangeApprovals
                .AnyAsync(a => a.ChangeId == dto.ChangeId && a.ApproverId == memberId && !a.IsDeleted, cancellationToken);

            if (!existing)
            {
                var approval = new ChangeApproval
                {
                    ChangeId = dto.ChangeId,
                    ApproverId = memberId,
                    ApprovalRole = ApprovalRole.CABMember,
                    ApprovalStatus = ApprovalStatus.Requested,
                    CreatedAt = DateTime.UtcNow
                };
                context.ChangeApprovals.Add(approval);
                addedMembers.Add(approval);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created CAB for change {ChangeId} with {MemberCount} members", dto.ChangeId, dto.MemberIds.Count);

        return await BuildCabDtoAsync(dto.ChangeId, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CABDto> ScheduleCABMeetingAsync(int cabId, DateTime meetingDate, string? meetingLocation, int modifiedById, CancellationToken cancellationToken = default)
    {
        // cabId == ChangeId
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { cabId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change (CAB) {cabId} not found");

        change.CABDate = meetingDate;
        // meetingLocation stored in RiskAssessmentNotes as a simple workaround since there's no dedicated field
        if (!string.IsNullOrEmpty(meetingLocation))
            change.RiskAssessmentNotes = $"CAB Location: {meetingLocation}";

        change.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Scheduled CAB meeting for change {ChangeId} on {MeetingDate}", cabId, meetingDate);
        return await BuildCabDtoAsync(cabId, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CABMemberDto> RecordCABDecisionAsync(int cabId, int cabMemberId, ApprovalStatus status, string? comments, int decidedById, CancellationToken cancellationToken = default)
    {
        // cabId == ChangeId; cabMemberId == ApproverId
        var context = _dbContextResolver.ResolveContext();
        var approval = await context.ChangeApprovals
            .Include(a => a.Approver)
            .FirstOrDefaultAsync(a => a.ChangeId == cabId &&
                                      a.ApproverId == cabMemberId &&
                                      (a.ApprovalRole == ApprovalRole.CABMember || a.ApprovalRole == ApprovalRole.CABChair) &&
                                      !a.IsDeleted, cancellationToken);

        if (approval == null)
            throw new KeyNotFoundException($"CAB member {cabMemberId} not found for change {cabId}");

        approval.ApprovalStatus = status;
        approval.Comments = comments;
        approval.ApprovalDate = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recorded CAB decision {Status} for change {ChangeId} by member {MemberId}", status, cabId, cabMemberId);

        return new CABMemberDto
        {
            CABMemberId = approval.ApprovalId,
            CABId = cabId,
            UserId = approval.ApproverId,
            UserName = approval.Approver?.Username,
            Role = approval.ApprovalRole.ToString(),
            VotingStatus = approval.ApprovalStatus.ToString(),
            Comments = approval.Comments,
            VotedAt = approval.ApprovalDate
        };
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> FinalizeCABApprovalAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        // Check all CAB member votes
        var cabApprovals = await context.ChangeApprovals
            .Where(a => a.ChangeId == changeId &&
                        (a.ApprovalRole == ApprovalRole.CABMember || a.ApprovalRole == ApprovalRole.CABChair) &&
                        !a.IsDeleted)
            .ToListAsync(cancellationToken);

        bool allApproved = cabApprovals.Any() &&
                           cabApprovals.All(a => a.ApprovalStatus == ApprovalStatus.Approved);

        bool anyRejected = cabApprovals.Any(a => a.ApprovalStatus == ApprovalStatus.Rejected);

        if (anyRejected)
        {
            change.ApprovalStatus = ApprovalStatus.Rejected;
            change.State = ChangeState.Rejected;
        }
        else if (allApproved)
        {
            change.ApprovalStatus = ApprovalStatus.Approved;
            change.State = ChangeState.Authorize;
        }

        change.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Finalized CAB approval for change {ChangeId} - AllApproved: {AllApproved}", changeId, allApproved);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CABDto> GetCABAsync(int cabId, CancellationToken cancellationToken = default)
    {
        // cabId == ChangeId
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { cabId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change (CAB) {cabId} not found");

        return await BuildCabDtoAsync(cabId, context, cancellationToken);
    }

    // =========================================================================
    // Approval Management
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeApprovalDto> RecordApprovalAsync(int changeId, int approverId, ApprovalStatus status, string? comments, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var approval = new ChangeApproval
        {
            ChangeId = changeId,
            ApproverId = approverId,
            ApprovalRole = ApprovalRole.ChangeManager,
            ApprovalStatus = status,
            Comments = comments,
            ApprovalDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.ChangeApprovals.Add(approval);

        // Update aggregate approval status on the change
        if (status == ApprovalStatus.Approved)
            change.ApprovalStatus = ApprovalStatus.Approved;
        else if (status == ApprovalStatus.Rejected)
            change.ApprovalStatus = ApprovalStatus.Rejected;

        change.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded approval {Status} for change {ChangeId} by approver {ApproverId}", status, changeId, approverId);
        return MapApprovalToDto(approval, null);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChangeApprovalDto>> GetApprovalsAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var approvals = await context.ChangeApprovals
            .Include(a => a.Approver)
            .Where(a => a.ChangeId == changeId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return approvals.Select(a => MapApprovalToDto(a, a.Approver?.Username));
    }

    // =========================================================================
    // Impact Analysis
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeImpactDto> AnalyzeChangeImpactAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        // Retrieve impacted CIs with their configuration items
        var impactedCIs = await context.ChangeImpactedCIs
            .Include(ci => ci.ConfigurationItem)
            .Where(ci => ci.ChangeId == changeId && !ci.IsDeleted)
            .ToListAsync(cancellationToken);

        var impactedServices = impactedCIs.Select(ci => new ImpactedServiceDto
        {
            CIId = ci.CIId,
            CIName = ci.ConfigurationItem?.CIName ?? string.Empty,
            ImpactLevel = ci.Impact,
            RiskLevel = change.Risk,
            MitigationPlan = change.RiskMitigationPlan,
            BackoutTime = change.EstimatedDurationMinutes
        }).ToList();

        // Build risk assessment based on change's risk and impact settings
        var riskAssessments = new List<RiskAssessmentDto>
        {
            new RiskAssessmentDto
            {
                RiskType = "Implementation Risk",
                Description = change.RiskAssessmentNotes ?? $"Change type {change.Type} with {change.Risk} risk profile",
                SeverityLevel = change.Risk,
                Mitigation = change.RiskMitigationPlan,
                ContingencyTime = change.EstimatedDurationMinutes
            }
        };

        if (change.Impact == ChangeImpact.High)
        {
            riskAssessments.Add(new RiskAssessmentDto
            {
                RiskType = "Business Impact",
                Description = "High business impact detected - additional approvals may be required",
                SeverityLevel = ChangeRisk.High,
                Mitigation = change.BackoutPlan
            });
        }

        double totalRiskScore = (double)change.Risk * impactedServices.Count + (double)change.Impact;

        _logger.LogInformation("Analyzed impact for change {ChangeId}: {CICount} CIs impacted", changeId, impactedCIs.Count);

        return new ChangeImpactDto
        {
            ChangeId = changeId,
            ChangeNumber = change.Number,
            ImpactedServices = impactedServices,
            RiskAssessments = riskAssessments,
            TotalRiskScore = totalRiskScore,
            BackoutPlanRequired = change.Risk == ChangeRisk.High || change.Impact == ChangeImpact.High
        };
    }

    /// <inheritdoc/>
    public async Task<ChangeImpactDto> DocumentImpactedServicesAsync(int changeId, List<int> impactedCIIds, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        // Add each CI if not already linked
        foreach (var ciId in impactedCIIds)
        {
            var existing = await context.ChangeImpactedCIs
                .AnyAsync(ci => ci.ChangeId == changeId && ci.CIId == ciId, cancellationToken);

            if (!existing)
            {
                context.ChangeImpactedCIs.Add(new ChangeImpactedCI
                {
                    ChangeId = changeId,
                    CIId = ciId,
                    Impact = change.Impact,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Documented {Count} impacted CIs for change {ChangeId}", impactedCIIds.Count, changeId);

        return await AnalyzeChangeImpactAsync(changeId, modifiedById, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeImpactDto> IdentifyRollbackRisksAsync(int changeId, int modifiedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var impact = await AnalyzeChangeImpactAsync(changeId, modifiedById, cancellationToken);

        // Append rollback-specific risk
        impact.RiskAssessments.Add(new RiskAssessmentDto
        {
            RiskType = "Rollback Risk",
            Description = string.IsNullOrEmpty(change.BackoutPlan)
                ? "No backout plan defined - rollback risk is HIGH"
                : $"Backout plan available. Estimated rollback time: {change.EstimatedDurationMinutes ?? 60} minutes",
            SeverityLevel = string.IsNullOrEmpty(change.BackoutPlan) ? ChangeRisk.High : ChangeRisk.Low,
            Mitigation = change.BackoutPlan,
            ContingencyTime = change.EstimatedDurationMinutes ?? 60
        });

        _logger.LogInformation("Identified rollback risks for change {ChangeId}", changeId);
        return impact;
    }

    // =========================================================================
    // Rollback Management
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeRollbackPlanDto> CreateRollbackPlanAsync(int changeId, string rollbackSteps, int estimatedTimeMinutes, int createdById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        change.BackoutPlan = rollbackSteps;
        change.EstimatedDurationMinutes = estimatedTimeMinutes;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created rollback plan for change {ChangeId} with {Minutes} min estimate", changeId, estimatedTimeMinutes);

        return BuildRollbackPlanDto(change);
    }

    /// <inheritdoc/>
    public async Task<ChangeRollbackPlanDto> GetRollbackPlanAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        return BuildRollbackPlanDto(change);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> ExecuteRollbackAsync(int changeId, ExecuteRollbackDto dto, int executedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        change.State = ChangeState.Failed;
        change.ChangeSuccess = false;
        change.ClosureNotes = $"Rollback executed. Reason: {dto.Reason}. Notes: {dto.RollbackNotes}";
        change.ActualEndDate = dto.RollbackCompletedAt ?? DateTime.UtcNow;
        change.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Executed rollback for change {ChangeNumber}. Reason: {Reason}", change.Number, dto.Reason);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeDto> ValidateRollbackSuccessAsync(int changeId, ValidateRollbackSuccessDto dto, int validatedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        if (dto.RollbackSuccessful)
        {
            change.State = ChangeState.Cancelled;
            change.ImplementationNotes = $"Rollback successful. {dto.ValidationNotes}. Checks: {dto.PostRollbackChecks}";
        }
        else
        {
            change.ImplementationNotes = $"Rollback validation FAILED. {dto.ValidationNotes}";
        }

        change.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Validated rollback for change {ChangeId} - Success: {Success}", changeId, dto.RollbackSuccessful);
        return await MapChangeToDtoAsync(change, context, cancellationToken);
    }

    // =========================================================================
    // Implementation Tracking
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IEnumerable<ChangeImplementationTaskDto>> GetImplementationTasksAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var tasks = await context.ChangeTasks
            .Include(t => t.AssignedTo)
            .Where(t => t.ChangeId == changeId && !t.IsDeleted)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);

        return tasks.Select(MapTaskToDto);
    }

    /// <inheritdoc/>
    public async Task<ChangeImplementationTaskDto> CreateImplementationTaskAsync(int changeId, CreateChangeImplementationTaskDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var task = new ChangeTask
        {
            ChangeId = changeId,
            TaskName = dto.TaskName,
            Description = dto.Description,
            AssignedToId = dto.AssignedToId,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            DisplayOrder = dto.DisplayOrder,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        context.ChangeTasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created implementation task '{TaskName}' for change {ChangeId}", dto.TaskName, changeId);
        return MapTaskToDto(task);
    }

    /// <inheritdoc/>
    public async Task<ChangeImplementationTaskDto> CompleteImplementationTaskAsync(int taskId, int completedById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var task = await context.ChangeTasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.IsDeleted, cancellationToken);

        if (task == null)
            throw new KeyNotFoundException($"Implementation task {taskId} not found");

        task.IsCompleted = true;
        task.ActualEndDate = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Completed implementation task {TaskId} by user {UserId}", taskId, completedById);
        return MapTaskToDto(task);
    }

    /// <inheritdoc/>
    public async Task<(int TotalTasks, int CompletedTasks, double ProgressPercent)> GetImplementationProgressAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var allTasks = await context.ChangeTasks
            .Where(t => t.ChangeId == changeId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        int totalTasks = allTasks.Count;
        int completedTasks = allTasks.Count(t => t.IsCompleted);
        double progressPercent = totalTasks == 0 ? 0.0 : Math.Round((completedTasks / (double)totalTasks) * 100.0, 2);

        return (totalTasks, completedTasks, progressPercent);
    }

    // =========================================================================
    // Conflict Management
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeConflictCheckDto> CheckConflictsAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var result = new ChangeConflictCheckDto
        {
            ChangeId = changeId,
            ConflictsDetected = false,
            ConflictingChanges = new List<ConflictingChangeDto>(),
            ScheduleConflicts = new List<ScheduleConflictDto>()
        };

        if (!change.PlannedStartDate.HasValue || !change.PlannedEndDate.HasValue)
            return result;

        var startDate = change.PlannedStartDate.Value;
        var endDate = change.PlannedEndDate.Value;

        // Check overlapping changes
        var overlapping = await context.Changes
            .Where(c => c.ChangeId != changeId &&
                        !c.IsDeleted &&
                        c.State == ChangeState.Scheduled &&
                        c.PlannedStartDate.HasValue &&
                        c.PlannedEndDate.HasValue &&
                        ((c.PlannedStartDate.Value <= startDate && c.PlannedEndDate.Value >= startDate) ||
                         (c.PlannedStartDate.Value <= endDate && c.PlannedEndDate.Value >= endDate)))
            .ToListAsync(cancellationToken);

        foreach (var other in overlapping)
        {
            result.ConflictingChanges.Add(new ConflictingChangeDto
            {
                ChangeId = other.ChangeId,
                ChangeNumber = other.Number,
                ShortDescription = other.ShortDescription,
                PlannedStart = other.PlannedStartDate,
                PlannedEnd = other.PlannedEndDate,
                ConflictReason = "Overlapping scheduled maintenance window"
            });
        }

        // Check blackout periods
        var blackouts = await context.ChangeBlackouts
            .Where(b => !b.IsDeleted &&
                        ((b.StartDate <= startDate && b.EndDate >= startDate) ||
                         (b.StartDate <= endDate && b.EndDate >= endDate)))
            .ToListAsync(cancellationToken);

        foreach (var blackout in blackouts)
        {
            result.ScheduleConflicts.Add(new ScheduleConflictDto
            {
                Description = $"Blackout period: {blackout.Name}",
                ConflictPeriodStart = blackout.StartDate,
                ConflictPeriodEnd = blackout.EndDate,
                ConflictType = "BlackoutPeriod"
            });
        }

        result.ConflictsDetected = result.ConflictingChanges.Any() || result.ScheduleConflicts.Any();
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> DetectBlackoutConflictsAsync(int changeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        return await context.ChangeBlackouts
            .AnyAsync(b => !b.IsDeleted &&
                           ((b.StartDate <= startDate && b.EndDate >= startDate) ||
                            (b.StartDate <= endDate && b.EndDate >= endDate) ||
                            (b.StartDate >= startDate && b.EndDate <= endDate)),
                      cancellationToken);
    }

    // =========================================================================
    // Blackout Period Management
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IEnumerable<ChangeBlackoutDto>> GetBlackoutPeriodsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var blackouts = await context.ChangeBlackouts
            .Where(b => !b.IsDeleted &&
                        ((b.StartDate >= startDate && b.StartDate <= endDate) ||
                         (b.EndDate >= startDate && b.EndDate <= endDate) ||
                         (b.StartDate <= startDate && b.EndDate >= endDate)))
            .OrderBy(b => b.StartDate)
            .ToListAsync(cancellationToken);

        return blackouts.Select(b => new ChangeBlackoutDto
        {
            BlackoutId = b.BlackoutId,
            Name = b.Name,
            Description = b.Description,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            Reason = b.Reason,
            CreatedAt = b.CreatedAt
        });
    }

    /// <inheritdoc/>
    public async Task<ChangeBlackoutDto> CreateBlackoutPeriodAsync(CreateChangeBlackoutDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();

        var blackout = new ChangeBlackout
        {
            // ChangeId = 0 represents a standalone (non-change-specific) blackout period
            ChangeId = 0,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        context.ChangeBlackouts.Add(blackout);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created blackout period '{Name}' from {StartDate} to {EndDate}", dto.Name, dto.StartDate, dto.EndDate);

        return new ChangeBlackoutDto
        {
            BlackoutId = blackout.BlackoutId,
            Name = blackout.Name,
            Description = blackout.Description,
            StartDate = blackout.StartDate,
            EndDate = blackout.EndDate,
            Reason = blackout.Reason,
            CreatedAt = blackout.CreatedAt
        };
    }

    // =========================================================================
    // Comments
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeCommentDto> AddCommentAsync(int changeId, CreateChangeCommentDto dto, int createdById, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        if (change == null || change.IsDeleted)
            throw new KeyNotFoundException($"Change {changeId} not found");

        var comment = new ChangeComment
        {
            ChangeId = changeId,
            Comment = dto.Comment,
            IsInternal = dto.IsInternal,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        context.ChangeComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added comment to change {ChangeId} by user {UserId}", changeId, createdById);

        return new ChangeCommentDto
        {
            CommentId = comment.CommentId,
            ChangeId = comment.ChangeId,
            Comment = comment.Comment,
            IsInternal = comment.IsInternal,
            CreatedById = comment.CreatedById,
            CreatedAt = comment.CreatedAt
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChangeCommentDto>> GetCommentsAsync(int changeId, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();
        var comments = await context.ChangeComments
            .Include(c => c.CreatedBy)
            .Where(c => c.ChangeId == changeId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(c => new ChangeCommentDto
        {
            CommentId = c.CommentId,
            ChangeId = c.ChangeId,
            Comment = c.Comment,
            IsInternal = c.IsInternal,
            CreatedById = c.CreatedById,
            CreatedByName = c.CreatedBy?.Username,
            CreatedAt = c.CreatedAt
        });
    }

    // =========================================================================
    // Metrics
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ChangeMetricsDto> GetChangeMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var context = _dbContextResolver.ResolveContext();

        var query = context.Changes.Where(c => !c.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(c => c.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.CreatedAt <= toDate.Value);

        var allChanges = await query.ToListAsync(cancellationToken);

        int total = allChanges.Count;
        int successful = allChanges.Count(c => c.ChangeSuccess == true);
        int failed = allChanges.Count(c => c.State == ChangeState.Failed);
        int cancelled = allChanges.Count(c => c.State == ChangeState.Cancelled);
        int rollbacks = allChanges.Count(c => c.State == ChangeState.Failed && c.ClosureNotes != null && c.ClosureNotes.Contains("Rollback executed"));

        double successRate = total == 0 ? 0.0 : Math.Round((successful / (double)total) * 100.0, 2);
        double rollbackRate = total == 0 ? 0.0 : Math.Round((rollbacks / (double)total) * 100.0, 2);

        var implemented = allChanges
            .Where(c => c.ActualStartDate.HasValue && c.ActualEndDate.HasValue)
            .Select(c => (c.ActualEndDate!.Value - c.ActualStartDate!.Value).TotalMinutes)
            .ToList();

        double avgImplTime = implemented.Any() ? Math.Round(implemented.Average(), 2) : 0.0;

        return new ChangeMetricsDto
        {
            TotalChanges = total,
            SuccessfulChanges = successful,
            FailedChanges = failed,
            CancelledChanges = cancelled,
            SuccessRate = successRate,
            AverageImplementationTimeMinutes = avgImplTime,
            RequiredRollbacks = rollbacks,
            RollbackRate = rollbackRate,
            ReportGeneratedAt = DateTime.UtcNow
        };
    }

    // =========================================================================
    // Private Helpers
    // =========================================================================

    private async Task<ChangeDto> MapChangeToDtoAsync(Change change, ICrmDbContext context, CancellationToken cancellationToken)
    {
        var impactedCICount = await context.ChangeImpactedCIs
            .CountAsync(ci => ci.ChangeId == change.ChangeId, cancellationToken);

        return new ChangeDto
        {
            ChangeId = change.ChangeId,
            Number = change.Number,
            ShortDescription = change.ShortDescription,
            Type = change.Type,
            State = change.State,
            ApprovalStatus = change.ApprovalStatus,
            Risk = change.Risk,
            Impact = change.Impact,
            PlannedStartDate = change.PlannedStartDate,
            PlannedEndDate = change.PlannedEndDate,
            RequestorId = change.RequestorId,
            RequestorName = change.Requestor?.Username,
            CreatedAt = change.CreatedAt
        };
    }

    private async Task<string> GenerateChangeNumberAsync(ICrmDbContext context, CancellationToken cancellationToken)
    {
        var lastChange = await context.Changes
            .OrderByDescending(c => c.ChangeId)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = lastChange != null ? lastChange.ChangeId + 1 : 1;
        return $"CHG{nextNumber:D7}";
    }

    private async Task<CABDto> BuildCabDtoAsync(int changeId, ICrmDbContext context, CancellationToken cancellationToken)
    {
        var change = await context.Changes.FindAsync(new object[] { changeId }, cancellationToken);

        var members = await context.ChangeApprovals
            .Include(a => a.Approver)
            .Where(a => a.ChangeId == changeId &&
                        (a.ApprovalRole == ApprovalRole.CABMember || a.ApprovalRole == ApprovalRole.CABChair) &&
                        !a.IsDeleted)
            .ToListAsync(cancellationToken);

        return new CABDto
        {
            CABId = changeId,
            ChangeId = changeId,
            ChangeNumber = change?.Number,
            ScheduledDate = change?.CABDate ?? DateTime.MinValue,
            Status = change?.ApprovalStatus.ToString(),
            Members = members.Select(a => new CABMemberDto
            {
                CABMemberId = a.ApprovalId,
                CABId = changeId,
                UserId = a.ApproverId,
                UserName = a.Approver?.Username,
                Role = a.ApprovalRole.ToString(),
                VotingStatus = a.ApprovalStatus.ToString(),
                Comments = a.Comments,
                VotedAt = a.ApprovalDate
            }).ToList(),
            CreatedAt = change?.CreatedAt ?? DateTime.UtcNow
        };
    }

    private static ChangeApprovalDto MapApprovalToDto(ChangeApproval approval, string? approverName)
    {
        return new ChangeApprovalDto
        {
            ApprovalId = approval.ApprovalId,
            ChangeId = approval.ChangeId,
            ApproverId = approval.ApproverId,
            ApproverName = approverName ?? approval.Approver?.Username,
            ApprovalRole = approval.ApprovalRole.ToString(),
            ApprovalStatus = approval.ApprovalStatus,
            ApprovalDate = approval.ApprovalDate,
            Comments = approval.Comments,
            CreatedAt = approval.CreatedAt
        };
    }

    private static ChangeImplementationTaskDto MapTaskToDto(ChangeTask task)
    {
        return new ChangeImplementationTaskDto
        {
            TaskId = task.TaskId,
            ChangeId = task.ChangeId,
            TaskName = task.TaskName,
            Description = task.Description,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.Username,
            PlannedStartDate = task.PlannedStartDate,
            PlannedEndDate = task.PlannedEndDate,
            ActualStartDate = task.ActualStartDate,
            ActualEndDate = task.ActualEndDate,
            IsCompleted = task.IsCompleted,
            DisplayOrder = task.DisplayOrder,
            CreatedAt = task.CreatedAt
        };
    }

    private static ChangeRollbackPlanDto BuildRollbackPlanDto(Change change)
    {
        var tasks = new List<string>();
        if (!string.IsNullOrEmpty(change.BackoutPlan))
        {
            // Split multi-step plans by newlines or numbered items
            tasks = change.BackoutPlan
                .Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        return new ChangeRollbackPlanDto
        {
            ChangeId = change.ChangeId,
            RollbackSteps = change.BackoutPlan,
            EstimatedRollbackTimeMinutes = change.EstimatedDurationMinutes,
            RollbackTasks = tasks,
            BackoutTriggers = change.RiskAssessmentNotes,
            RisksIfNotRolledBack = change.Impact == ChangeImpact.High
                ? "High business impact if rollback is not executed"
                : null
        };
    }
}
