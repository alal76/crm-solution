// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Ports.Input;

/// <summary>
/// Input port for PDF document generation.
/// TODO-SALES003-010
/// </summary>
public interface IPdfGenerationService
{
    /// <summary>
    /// Generates a PDF for an invoice.
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken ct = default);
}
