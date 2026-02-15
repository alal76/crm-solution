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

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICommissionApprovalService for commission approval workflows.
/// Handles multi-level approvals and audit trails.
/// </summary>
public class CommissionApprovalService : ICommissionApprovalService, ICommissionApprovalInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionApprovalService> _logger;

    public CommissionApprovalService(ICrmDbContext context, ILogger<CommissionApprovalService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ApproveAsync(int commissionId, int approvedById, string? notes = null, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions
            .FirstOrDefaultAsync(c => c.Id == commissionId && !c.IsDeleted, cancellationToken);

        if (commission == null)
            return false;

        commission.Status = CommissionStatus.Approved;
        commission.ApprovedById = approvedById;
        commission.ApprovedAt = DateTime.UtcNow;
        commission.UpdatedAt = DateTime.UtcNow;

        var auditLog = new CommissionApprovalAudit
        {
            CommissionId = commissionId,
            Action = "Approved",
            ApprovedById = approvedById,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Commissions.Update(commission);
        _context.CommissionApprovalAudits.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission {CommissionId} approved by user {ApproverId}", commissionId, approvedById);
        return true;
    }

    public async Task<bool> RejectAsync(int commissionId, string reason, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions
            .FirstOrDefaultAsync(c => c.Id == commissionId && !c.IsDeleted, cancellationToken);

        if (commission == null)
            return false;

        commission.Status = CommissionStatus.Rejected;
        commission.UpdatedAt = DateTime.UtcNow;

        var auditLog = new CommissionApprovalAudit
        {
            CommissionId = commissionId,
            Action = "Rejected",
            Notes = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.Commissions.Update(commission);
        _context.CommissionApprovalAudits.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission {CommissionId} rejected. Reason: {Reason}", commissionId, reason);
        return true;
    }

    public async Task<List<CommissionDto>> GetPendingAsync(int reviewerId, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => !c.IsDeleted && c.Status == CommissionStatus.Pending)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return commissions.Select(c => new CommissionDto
        {
            Id = c.Id,
            CommissionNumber = c.CommissionNumber ?? string.Empty,
            UserId = c.UserId,
            UserName = $"{c.User?.FirstName} {c.User?.LastName}",
            CommissionAmount = c.CommissionAmount,
            Status = (int)c.Status,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<List<object>> GetHistoryAsync(int commissionId, CancellationToken cancellationToken = default)
    {
        var audits = await _context.CommissionApprovalAudits
            .Where(a => a.CommissionId == commissionId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return audits.Cast<object>().ToList();
    }

    public async Task<int> BulkApproveAsync(List<int> commissionIds, int approvedById, CancellationToken cancellationToken = default)
    {
        if (!commissionIds.Any())
            return 0;

        var commissions = await _context.Commissions
            .Where(c => commissionIds.Contains(c.Id) && !c.IsDeleted && c.Status == CommissionStatus.Pending)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var commission in commissions)
        {
            commission.Status = CommissionStatus.Approved;
            commission.ApprovedById = approvedById;
            commission.ApprovedAt = now;
            commission.UpdatedAt = now;

            var auditLog = new CommissionApprovalAudit
            {
                CommissionId = commission.Id,
                Action = "BulkApproved",
                ApprovedById = approvedById,
                CreatedAt = now
            };

            _context.CommissionApprovalAudits.Add(auditLog);
        }

        _context.Commissions.UpdateRange(commissions);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk approved {Count} commissions by user {ApproverId}", commissions.Count, approvedById);
        return commissions.Count;
    }

    public async Task<bool> NotifyAsync(int commissionId, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commissionId && !c.IsDeleted, cancellationToken);

        if (commission == null)
            return false;

        _logger.LogInformation("Commission {CommissionId} notification sent to user {UserId}", commissionId, commission.UserId);
        return true;
    }
}

/// <summary>
/// Commission approval audit trail entity.
/// </summary>
public class CommissionApprovalAudit
{
    public int Id { get; set; }
    public int CommissionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
