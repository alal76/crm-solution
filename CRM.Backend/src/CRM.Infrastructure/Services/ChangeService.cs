// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0.

using CRM.Core.Dtos;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing change requests.
/// Bridges between the simplified CRM.Core.Dtos and the ITSM Change entity.
/// </summary>
public class ChangeService : IChangeService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ChangeService> _logger;

    public ChangeService(CrmDbContext context, ILogger<ChangeService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaginatedDto<ChangeDto>> GetAllAsync(
        int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Changes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ChangeState>(status, true, out var state))
        {
            query = query.Where(c => c.State == state);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);

        return new PaginatedDto<ChangeDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ChangeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var change = await _context.Changes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ChangeId == id, cancellationToken);

        return change == null ? null : MapToDto(change);
    }

    public async Task<ChangeDto> CreateAsync(CreateChangeDto dto, CancellationToken cancellationToken = default)
    {
        var count = await _context.Changes.CountAsync(cancellationToken);
        var change = new Change
        {
            Number = $"CHG-{count + 1:D5}",
            ShortDescription = dto.Title,
            Description = dto.Description,
            Type = (ChangeType)Math.Clamp(dto.Priority, 1, 3),
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium,
            State = ChangeState.New,
            ApprovalStatus = CRM.Core.Entities.ITSM.ApprovalStatus.Requested,
            RequestorId = dto.AssignedToId ?? 1,
            AssignedToId = dto.AssignedToId,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Changes.Add(change);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created change {ChangeId}: {Title}", change.ChangeId, dto.Title);
        return MapToDto(change);
    }

    public async Task<ChangeDto> UpdateAsync(int id, UpdateChangeDto dto, CancellationToken cancellationToken = default)
    {
        var change = await _context.Changes.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Change {id} not found");

        if (!string.IsNullOrEmpty(dto.Title))
            change.ShortDescription = dto.Title;
        if (dto.Description != null)
            change.Description = dto.Description;
        if (dto.AssignedToId.HasValue)
            change.AssignedToId = dto.AssignedToId;
        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<ChangeState>(dto.Status, true, out var state))
            change.State = state;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(change);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var change = await _context.Changes.FindAsync(new object[] { id }, cancellationToken);
        if (change == null) return false;

        _context.Changes.Remove(change);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ChangeDto> SubmitAsync(int id, CancellationToken cancellationToken = default)
    {
        var change = await _context.Changes.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Change {id} not found");

        change.State = ChangeState.Assess;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(change);
    }

    public async Task<ChangeDto> ApproveAsync(int id, ChangeApprovalDto dto, CancellationToken cancellationToken = default)
    {
        var change = await _context.Changes.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Change {id} not found");

        change.ApprovalStatus = CRM.Core.Entities.ITSM.ApprovalStatus.Approved;
        change.State = ChangeState.Approved;
        change.ApprovalNotes = dto.ApproverNotes;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(change);
    }

    public async Task<ChangeDto> RejectAsync(int id, ChangeRejectionDto dto, CancellationToken cancellationToken = default)
    {
        var change = await _context.Changes.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Change {id} not found");

        change.ApprovalStatus = CRM.Core.Entities.ITSM.ApprovalStatus.Rejected;
        change.State = ChangeState.Rejected;
        change.ApprovalNotes = dto.RejectionReason;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(change);
    }

    private static ChangeDto MapToDto(Change c) => new()
    {
        Id = c.ChangeId,
        Title = c.ShortDescription,
        Description = c.Description,
        Status = c.State.ToString(),
        Priority = (int)c.Risk,
        CreatedAt = c.CreatedAt,
        UpdatedAt = null,
        AssignedToId = c.AssignedToId,
    };
}
