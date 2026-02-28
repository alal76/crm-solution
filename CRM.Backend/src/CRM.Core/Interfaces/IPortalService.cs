// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Customer-facing portal service contract.
/// Exposes tickets (service requests), knowledge-base articles and portal config
/// scoped to the authenticated portal user.
/// </summary>
public interface IPortalService
{
    // ── Tickets ───────────────────────────────────────────────────────────────
    Task<PagedResultDto<PortalTicketDto>> GetMyTicketsAsync(int portalUserId, int page, int pageSize, CancellationToken ct = default);
    Task<PortalTicketDto?> GetTicketAsync(int portalUserId, int ticketId, CancellationToken ct = default);
    Task<PortalTicketDto> CreateTicketAsync(int portalUserId, PortalCreateTicketDto dto, CancellationToken ct = default);

    // ── Ticket Comments ───────────────────────────────────────────────────────
    Task<IEnumerable<PortalCommentDto>> GetTicketCommentsAsync(int portalUserId, int ticketId, CancellationToken ct = default);
    Task<PortalCommentDto> AddTicketCommentAsync(int portalUserId, int ticketId, PortalAddCommentDto dto, CancellationToken ct = default);

    // ── Knowledge Base ────────────────────────────────────────────────────────
    Task<PagedResultDto<PortalKBArticleDto>> GetKnowledgeArticlesAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<PortalKBArticleDto?> GetKnowledgeArticleAsync(int id, CancellationToken ct = default);

    // ── Config ────────────────────────────────────────────────────────────────
    Task<PortalConfigDto> GetConfigAsync(CancellationToken ct = default);
    // ── Profile (PORTAL-019) ─────────────────────────────────────────────────────
    Task<PortalUserDto> GetProfileAsync(int portalUserId, CancellationToken ct = default);
    Task<PortalUserDto> UpdateProfileAsync(int portalUserId, UpdatePortalProfileDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(int portalUserId, ChangePortalPasswordDto dto, CancellationToken ct = default);

    // ── Attachments (PORTAL-022) ───────────────────────────────────────────────────
    Task<PortalAttachmentDto> UploadAttachmentAsync(int ticketId, int portalUserId, string fileName, string contentType, System.IO.Stream fileStream, long fileSize, CancellationToken ct = default);
    Task<IEnumerable<PortalAttachmentDto>> GetAttachmentsAsync(int ticketId, int portalUserId, CancellationToken ct = default);

    // ── Cancel (PORTAL-023) ──────────────────────────────────────────────────────────
    Task CancelTicketAsync(int ticketId, int portalUserId, CancellationToken ct = default);}
