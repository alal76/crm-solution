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
/// Customer-facing portal service.
/// Exposes service-request tickets (scoped to the calling portal user),
/// knowledge-base articles and portal configuration.
/// </summary>
public class PortalService : IPortalService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<PortalService> _logger;

    public PortalService(ICrmDbContext db, ILogger<PortalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Tickets ───────────────────────────────────────────────────────────────

    public async Task<PagedResultDto<PortalTicketDto>> GetMyTicketsAsync(
        int portalUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var portalUser = await _db.PortalUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct);

        if (portalUser == null)
            return new PagedResultDto<PortalTicketDto> { Page = page, PageSize = pageSize };

        var query = _db.ServiceRequests.AsNoTracking()
            .Where(sr => !sr.IsDeleted && sr.RequesterEmail == portalUser.Email);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(sr => sr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sr => new PortalTicketDto
            {
                Id = sr.Id,
                Title = sr.Subject,
                Description = sr.Description,
                Status = sr.Status.ToString(),
                Priority = sr.Priority.ToString(),
                TicketNumber = sr.TicketNumber,
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResultDto<PortalTicketDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PortalTicketDto?> GetTicketAsync(
        int portalUserId, int ticketId, CancellationToken ct = default)
    {
        var portalUser = await _db.PortalUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct);

        if (portalUser == null) return null;

        var sr = await _db.ServiceRequests.AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == ticketId && !s.IsDeleted && s.RequesterEmail == portalUser.Email, ct);

        return sr == null ? null : MapTicket(sr);
    }

    public async Task<PortalTicketDto> CreateTicketAsync(
        int portalUserId, PortalCreateTicketDto dto, CancellationToken ct = default)
    {
        var portalUser = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("Portal user not found.");

        var priority = dto.Priority switch
        {
            "Low" => ServiceRequestPriority.Low,
            "High" => ServiceRequestPriority.High,
            "Critical" => ServiceRequestPriority.Critical,
            _ => ServiceRequestPriority.Medium
        };

        var now = DateTime.UtcNow;
        var ticketNumber = $"PT-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var sr = new ServiceRequest
        {
            TicketNumber = ticketNumber,
            Subject = dto.Title,
            Description = dto.Description,
            Priority = priority,
            Status = ServiceRequestStatus.New,
            Channel = ServiceRequestChannel.SelfServicePortal,
            RequesterEmail = portalUser.Email,
            RequesterName = portalUser.DisplayName,
            ContactId = portalUser.ContactId,
            AccountId = portalUser.AccountId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.ServiceRequests.Add(sr);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Portal ticket {TicketNumber} created by user {Email}", ticketNumber, portalUser.Email);
        return MapTicket(sr);
    }

    // ── Comments ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PortalCommentDto>> GetTicketCommentsAsync(
        int portalUserId, int ticketId, CancellationToken ct = default)
    {
        var portalUser = await _db.PortalUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct);

        if (portalUser == null) return Enumerable.Empty<PortalCommentDto>();

        // Verify the ticket belongs to this portal user
        var ticketExists = await _db.ServiceRequests.AsNoTracking()
            .AnyAsync(s => s.Id == ticketId && !s.IsDeleted && s.RequesterEmail == portalUser.Email, ct);

        if (!ticketExists) return Enumerable.Empty<PortalCommentDto>();

        var notes = await _db.Notes.AsNoTracking()
            .Where(n => n.ServiceRequestId == ticketId && !n.IsDeleted &&
                        n.Visibility != NoteVisibility.Private)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(ct);

        return notes.Select(n => new PortalCommentDto
        {
            Id = n.Id,
            Content = n.Content,
            AuthorName = string.IsNullOrEmpty(n.Title) ? "Support" : n.Title,
            IsStaff = n.CreatedByUserId.HasValue,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<PortalCommentDto> AddTicketCommentAsync(
        int portalUserId, int ticketId, PortalAddCommentDto dto, CancellationToken ct = default)
    {
        var portalUser = await _db.PortalUsers
            .FirstOrDefaultAsync(u => u.Id == portalUserId && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("Portal user not found.");

        var ticketExists = await _db.ServiceRequests.AsNoTracking()
            .AnyAsync(s => s.Id == ticketId && !s.IsDeleted && s.RequesterEmail == portalUser.Email, ct);

        if (!ticketExists)
            throw new InvalidOperationException("Ticket not found or access denied.");

        var now = DateTime.UtcNow;
        var note = new Note
        {
            Title = portalUser.DisplayName ?? portalUser.Email,
            Content = dto.Content,
            ServiceRequestId = ticketId,
            EntityType = "ServiceRequest",
            EntityId = ticketId,
            Visibility = NoteVisibility.Public,
            NoteType = NoteType.General,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync(ct);

        return new PortalCommentDto
        {
            Id = note.Id,
            Content = note.Content,
            AuthorName = note.Title,
            IsStaff = false,
            CreatedAt = note.CreatedAt
        };
    }

    // ── Knowledge Base ────────────────────────────────────────────────────────

    public async Task<PagedResultDto<PortalKBArticleDto>> GetKnowledgeArticlesAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.KnowledgeArticles.AsNoTracking()
            .Where(a => !a.IsDeleted && a.Status == CRM.Core.Entities.KnowledgeBase.ArticleStatus.Published);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search) ||
                                     (a.Summary != null && a.Summary.Contains(search)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new PortalKBArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                Content = a.Content,
                ViewCount = a.ViewCount,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResultDto<PortalKBArticleDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PortalKBArticleDto?> GetKnowledgeArticleAsync(int id, CancellationToken ct = default)
    {
        var article = await _db.KnowledgeArticles.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted &&
                                      a.Status == CRM.Core.Entities.KnowledgeBase.ArticleStatus.Published, ct);

        if (article == null) return null;

        // Increment view count (fire-and-forget style, ignore failures)
        try
        {
            var tracked = await _db.KnowledgeArticles.FindAsync(new object[] { id }, ct);
            if (tracked != null)
            {
                tracked.ViewCount++;
                tracked.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to increment view count for article {Id}", id);
        }

        return new PortalKBArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Summary = article.Summary,
            Content = article.Content,
            ViewCount = article.ViewCount + 1,
            CreatedAt = article.CreatedAt
        };
    }

    // ── Config ────────────────────────────────────────────────────────────────

    public async Task<PortalConfigDto> GetConfigAsync(CancellationToken ct = default)
    {
        var config = await _db.PortalConfigs.AsNoTracking()
            .FirstOrDefaultAsync(c => !c.IsDeleted, ct);

        return config == null
            ? new PortalConfigDto { IsEnabled = false, AllowSelfRegistration = true }
            : MapConfig(config);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PortalTicketDto MapTicket(ServiceRequest sr) => new PortalTicketDto
    {
        Id = sr.Id,
        Title = sr.Subject,
        Description = sr.Description,
        Status = sr.Status.ToString(),
        Priority = sr.Priority.ToString(),
        TicketNumber = sr.TicketNumber,
        CreatedAt = sr.CreatedAt,
        UpdatedAt = sr.UpdatedAt
    };

    private static PortalConfigDto MapConfig(PortalConfig c) => new PortalConfigDto
    {
        IsEnabled = c.IsEnabled,
        AllowSelfRegistration = c.AllowSelfRegistration,
        WelcomeMessage = c.WelcomeMessage,
        SupportEmail = c.SupportEmail,
        LogoUrl = c.LogoUrl,
        PrimaryColor = c.PrimaryColor,
        PortalTitle = c.PortalTitle,
        AllowedDomains = c.AllowedDomains
    };
}
