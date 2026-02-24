// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Ports.Input;

/// <summary>
/// Personal data export model — aggregates all personal data for a subject.
/// </summary>
public class PersonalDataExport
{
    public string SubjectType { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    /// <summary>All personal data fields mapped by section → field → value.</summary>
    public Dictionary<string, Dictionary<string, object?>> Data { get; set; } = new();
}

/// <summary>
/// DTO for a GDPR access log entry.
/// </summary>
public class GdprAccessLogDto
{
    public int Id { get; set; }
    public int RequestedByUserId { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// GDPR Article 15 compliance service — Right of Access, Right to Erasure.
/// TODO-SYS006-004
/// </summary>
public interface IGdprService
{
    /// <summary>
    /// Logs a data access event for audit purposes.
    /// </summary>
    Task LogAccessAsync(
        int userId,
        string subjectType,
        int subjectId,
        string action,
        string ipAddress,
        string? notes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Exports all personal data for a subject (Article 15).
    /// </summary>
    Task<PersonalDataExport> ExportPersonalDataAsync(
        string subjectType,
        int subjectId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the access log for a specific data subject.
    /// </summary>
    Task<IEnumerable<GdprAccessLogDto>> GetAccessLogsAsync(
        string subjectType,
        int subjectId,
        CancellationToken ct = default);

    /// <summary>
    /// Anonymises (erases) personal data for a subject (Article 17 — Right to be Forgotten).
    /// Soft-deletes the record and replaces personal fields with anonymised values.
    /// </summary>
    Task<bool> ErasePersonalDataAsync(
        string subjectType,
        int subjectId,
        int requestingUserId,
        string ipAddress,
        CancellationToken ct = default);
}
