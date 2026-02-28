// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Ports.Input;

/// <summary>
/// Paper format options for PDF generation.
/// </summary>
public enum PaperFormat
{
    /// <summary>A4 paper (210 × 297 mm), standard in most countries.</summary>
    A4 = 0,

    /// <summary>US Letter paper (8.5 × 11 inches).</summary>
    Letter = 1,

    /// <summary>US Legal paper (8.5 × 14 inches).</summary>
    Legal = 2,
}

/// <summary>
/// Options for PDF document generation.
/// </summary>
public sealed class PdfOptions
{
    /// <summary>Optional document title written into the PDF metadata.</summary>
    public string? DocumentTitle { get; set; }

    /// <summary>When <c>true</c> a header and footer are rendered on each page.</summary>
    public bool IncludeHeaderFooter { get; set; } = true;

    /// <summary>Paper format to use (default A4).</summary>
    public PaperFormat Format { get; set; } = PaperFormat.A4;
}

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

    /// <summary>
    /// Generates a PDF for a quote.
    /// </summary>
    /// <param name="quoteId">Quote ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> GenerateQuotePdfAsync(int quoteId, CancellationToken ct = default);

    /// <summary>
    /// Generates a PDF from raw HTML content.
    /// </summary>
    /// <param name="htmlContent">HTML to render as a PDF page.</param>
    /// <param name="options">Optional generation options; uses defaults when <c>null</c>.</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>PDF bytes (or UTF-8 HTML bytes when a PDF library is not configured).</returns>
    Task<byte[]> GenerateFromHtmlAsync(string htmlContent, PdfOptions? options = null, CancellationToken ct = default);
}
