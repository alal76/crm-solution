// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for managing per-user email digest configuration (REV-FE-002), assembling digest
/// content section-by-section from existing CRM data, and sending the rendered digest email.
/// Backs GET/PUT /api/users/me/email-digest, POST /api/users/me/email-digest/preview, and the
/// hourly EmailDigestJob Hangfire recurring job.
/// </summary>
public interface IEmailDigestService
{
    /// <summary>Gets the current user's digest configuration, or sensible defaults if none saved yet.</summary>
    Task<EmailDigestConfigDto> GetConfigAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the current user's digest configuration.</summary>
    Task<EmailDigestConfigDto> UpdateConfigAsync(int userId, EmailDigestConfigDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assembles digest content for a user based on which sections are enabled on the given config.
    /// "New leads"/"recent activities"/"upcoming tasks"/"overdue tasks" use the real ITaskService/
    /// IActivityService/Leads query. TeamPerformance/KpiSummary are v1 best-effort aggregates —
    /// see remarks on EmailDigestContentDto.
    /// </summary>
    Task<EmailDigestContentDto> BuildDigestContentAsync(int userId, EmailDigestConfig config, CancellationToken cancellationToken = default);

    /// <summary>Renders a simple HTML email body from assembled digest content.</summary>
    string RenderHtml(User user, EmailDigestContentDto content, EmailDigestConfig config);

    /// <summary>
    /// Builds content, renders it, and sends it immediately to the user via INotificationPort.
    /// Used both by the "Send Preview" endpoint (isPreview=true, does not update LastSentAt) and by
    /// EmailDigestJob for scheduled sends (isPreview=false, updates LastSentAt on success).
    /// </summary>
    Task<bool> SendDigestAsync(User user, EmailDigestConfig config, bool isPreview, CancellationToken cancellationToken = default);
}
