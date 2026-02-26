// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing threaded record comments on any CRM entity.
/// Supports @mentions (stored as JSON user-ID array in MentionedUserIds).
/// </summary>
public class RecordCommentService : IRecordCommentService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<RecordCommentService> _logger;

    public RecordCommentService(ICrmDbContext context, ILogger<RecordCommentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecordCommentDto>> GetByEntityAsync(
        string entityType, int entityId, CancellationToken ct = default)
    {
        _logger.LogDebug("GetByEntityAsync: entityType={EntityType}, entityId={EntityId}", entityType, entityId);

        // Load top-level comments
        var topLevel = await _context.RecordComments
            .AsNoTracking()
            .Where(rc => !rc.IsDeleted
                      && rc.EntityType == entityType
                      && rc.EntityId == entityId
                      && rc.ParentCommentId == null)
            .OrderBy(rc => rc.CreatedAt)
            .ToListAsync(ct);

        if (!topLevel.Any())
            return Enumerable.Empty<RecordCommentDto>();

        var topLevelIds = topLevel.Select(c => c.Id).ToList();

        // Load replies to top-level comments (one level deep shown in UI)
        var replies = await _context.RecordComments
            .AsNoTracking()
            .Where(rc => !rc.IsDeleted
                      && rc.ParentCommentId != null
                      && topLevelIds.Contains(rc.ParentCommentId.Value))
            .OrderBy(rc => rc.CreatedAt)
            .ToListAsync(ct);

        // Build author lookup
        var authorIds = topLevel.Select(c => c.AuthorId)
            .Union(replies.Select(r => r.AuthorId))
            .Distinct()
            .ToList();

        var authorMap = await BuildAuthorMapAsync(authorIds, ct);

        // Map to DTOs
        var replyLookup = replies.GroupBy(r => r.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = topLevel.Select(c => MapToDto(c, authorMap, replyLookup, requestUserId: null)).ToList();

        _logger.LogInformation("GetByEntityAsync returned {Count} top-level comments", result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<RecordCommentDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("GetByIdAsync: id={Id}", id);

        var comment = await _context.RecordComments
            .AsNoTracking()
            .FirstOrDefaultAsync(rc => rc.Id == id && !rc.IsDeleted, ct);

        if (comment == null)
        {
            _logger.LogWarning("RecordComment not found: {Id}", id);
            return null;
        }

        var authorMap = await BuildAuthorMapAsync([comment.AuthorId], ct);
        return MapToDto(comment, authorMap, null, requestUserId: null);
    }

    /// <inheritdoc />
    public async Task<RecordCommentDto> CreateAsync(
        CreateRecordCommentDto dto, int authorId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        _logger.LogDebug("CreateAsync: entityType={EntityType}, entityId={EntityId}, authorId={AuthorId}",
            dto.EntityType, dto.EntityId, authorId);

        var comment = new RecordComment
        {
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Content = dto.Content,
            AuthorId = authorId,
            ParentCommentId = dto.ParentCommentId,
            MentionedUserIds = dto.MentionedUserIds,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.RecordComments.Add(comment);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created RecordComment id={Id} on {EntityType}/{EntityId}",
            comment.Id, comment.EntityType, comment.EntityId);

        var authorMap = await BuildAuthorMapAsync([authorId], ct);
        return MapToDto(comment, authorMap, null, requestUserId: authorId);
    }

    /// <inheritdoc />
    public async Task<RecordCommentDto?> UpdateAsync(
        int id, UpdateRecordCommentDto dto, int userId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        _logger.LogDebug("UpdateAsync: id={Id}, userId={UserId}", id, userId);

        var comment = await _context.RecordComments
            .FirstOrDefaultAsync(rc => rc.Id == id && !rc.IsDeleted, ct);

        if (comment == null)
        {
            _logger.LogWarning("RecordComment not found for update: {Id}", id);
            return null;
        }

        if (comment.AuthorId != userId)
        {
            _logger.LogWarning("Unauthorized update attempt on RecordComment {Id} by user {UserId}", id, userId);
            return null;
        }

        comment.Content = dto.Content;
        comment.MentionedUserIds = dto.MentionedUserIds;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated RecordComment id={Id}", id);

        var authorMap = await BuildAuthorMapAsync([comment.AuthorId], ct);
        return MapToDto(comment, authorMap, null, requestUserId: userId);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        _logger.LogDebug("DeleteAsync: id={Id}, userId={UserId}", id, userId);

        var comment = await _context.RecordComments
            .FirstOrDefaultAsync(rc => rc.Id == id && !rc.IsDeleted, ct);

        if (comment == null)
        {
            _logger.LogWarning("RecordComment not found for delete: {Id}", id);
            return false;
        }

        // Owner or Admin can delete
        if (comment.AuthorId != userId)
        {
            // Check if user is admin
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null || (UserRole)user.Role != UserRole.Admin)
            {
                _logger.LogWarning("Unauthorized delete attempt on RecordComment {Id} by user {UserId}", id, userId);
                return false;
            }
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted RecordComment id={Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecordCommentDto>> GetThreadAsync(
        int parentCommentId, CancellationToken ct = default)
    {
        _logger.LogDebug("GetThreadAsync: parentCommentId={ParentCommentId}", parentCommentId);

        var replies = await _context.RecordComments
            .AsNoTracking()
            .Where(rc => !rc.IsDeleted && rc.ParentCommentId == parentCommentId)
            .OrderBy(rc => rc.CreatedAt)
            .ToListAsync(ct);

        if (!replies.Any())
            return Enumerable.Empty<RecordCommentDto>();

        var authorIds = replies.Select(r => r.AuthorId).Distinct().ToList();
        var authorMap = await BuildAuthorMapAsync(authorIds, ct);

        return replies.Select(r => MapToDto(r, authorMap, null, requestUserId: null));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Dictionary<int, (string FullName, string? AvatarUrl)>> BuildAuthorMapAsync(
        IEnumerable<int> authorIds, CancellationToken ct)
    {
        var ids = authorIds.ToList();
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync(ct);

        return users.ToDictionary(
            u => u.Id,
            u => ($"{u.FirstName} {u.LastName}".Trim(), (string?)null));
    }

    private static RecordCommentDto MapToDto(
        RecordComment comment,
        Dictionary<int, (string FullName, string? AvatarUrl)> authorMap,
        Dictionary<int, List<RecordComment>>? replyLookup,
        int? requestUserId)
    {
        authorMap.TryGetValue(comment.AuthorId, out var author);

        var dto = new RecordCommentDto
        {
            Id = comment.Id,
            EntityType = comment.EntityType,
            EntityId = comment.EntityId,
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            AuthorName = author.FullName ?? "Unknown",
            AuthorAvatarUrl = author.AvatarUrl,
            ParentCommentId = comment.ParentCommentId,
            MentionedUserIds = comment.MentionedUserIds,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            CanEdit = requestUserId.HasValue && comment.AuthorId == requestUserId.Value,
            CanDelete = requestUserId.HasValue && comment.AuthorId == requestUserId.Value
        };

        if (replyLookup != null && replyLookup.TryGetValue(comment.Id, out var replies))
        {
            dto.Replies = replies.Select(r => MapToDto(r, authorMap, null, requestUserId)).ToList();
        }

        return dto;
    }
}
