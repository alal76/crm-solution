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
/// Service interface for contract export operations.
/// Handles PDF, Excel, and Word export of contracts.
/// </summary>
public interface IContractExportService
{
    /// <summary>
    /// Exports a contract to the specified format.
    /// </summary>
    /// <param name="contractId">Contract ID to export</param>
    /// <param name="format">Export format (PDF, Excel, Word)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export result with file bytes</returns>
    Task<ContractExportResultDto> ExportAsync(
        int contractId,
        string format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a contract to PDF format.
    /// </summary>
    Task<byte[]> ExportToPdfAsync(int contractId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a contract to Excel format.
    /// </summary>
    Task<byte[]> ExportToExcelAsync(int contractId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a contract to Word format.
    /// </summary>
    Task<byte[]> ExportToWordAsync(int contractId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports multiple contracts to a single file.
    /// </summary>
    /// <param name="contractIds">Contract IDs to export</param>
    /// <param name="format">Export format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export result</returns>
    Task<ContractExportResultDto> ExportBulkAsync(
        int[] contractIds,
        string format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets supported export formats.
    /// </summary>
    IEnumerable<string> GetSupportedFormats();
}
