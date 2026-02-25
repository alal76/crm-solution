// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for exporting audit logs to file formats (CSV, JSON).
/// TODO-SYS006-008
/// </summary>
public interface IAuditLogExportService
{
    /// <summary>
    /// Export filtered audit logs as UTF-8 CSV bytes.
    /// </summary>
    /// <param name="request">Filter and paging parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CSV content as a byte array.</returns>
    Task<byte[]> ExportToCsvAsync(
        AuditLogExportRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export filtered audit logs as UTF-8 JSON bytes.
    /// </summary>
    /// <param name="request">Filter and paging parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON content as a byte array.</returns>
    Task<byte[]> ExportToJsonAsync(
        AuditLogExportRequestDto request,
        CancellationToken cancellationToken = default);
}
