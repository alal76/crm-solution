// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Stub PDF generation service.
/// Returns a minimal valid PDF containing a placeholder message.
/// Replace with a real PDF library (QuestPDF, PdfSharpCore, etc.) when ready.
/// TODO-SALES003-010
/// </summary>
public class PdfGenerationService : IPdfGenerationService
{
    private readonly ILogger<PdfGenerationService> _logger;

    public PdfGenerationService(ILogger<PdfGenerationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "PdfGenerationService: Returning stub PDF for invoice {InvoiceId}. " +
            "PDF generation requires a PDF library to be configured.",
            invoiceId);

        // Return a minimal, syntactically valid PDF 1.4 file so the Content-Type is honoured
        // and the browser opens it without crashing.
        var message = $"PDF generation requires a PDF library to be configured.\nInvoice ID: {invoiceId}";
        var bytes = BuildMinimalPdf(message);
        return Task.FromResult(bytes);
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateQuotePdfAsync(int quoteId, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "PdfGenerationService: Returning stub PDF for quote {QuoteId}. " +
            "PDF generation requires a PDF library to be configured.",
            quoteId);

        var message = $"PDF generation requires a PDF library to be configured.\nQuote ID: {quoteId}";
        var bytes = BuildMinimalPdf(message);
        return Task.FromResult(bytes);
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateFromHtmlAsync(string htmlContent, PdfOptions? options = null, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "PdfGenerationService: PDF library not configured — returning UTF-8 HTML bytes. " +
            "Install QuestPDF, PdfSharpCore, or configure wkhtmltopdf/Playwright to enable real PDF output.");

        // Prefix so consumers can detect that this is HTML, not binary PDF.
        const string HtmlPrefix = "<!-- PDF generation requires wkhtmltopdf or Playwright - returning HTML bytes -->\n";
        var bytes = Encoding.UTF8.GetBytes(HtmlPrefix + htmlContent);
        return Task.FromResult(bytes);
    }

    /// <summary>
    /// Builds the smallest well-formed PDF that contains a single text page.
    /// This is intentionally minimal and does NOT require any NuGet packages.
    /// </summary>
    private static byte[] BuildMinimalPdf(string message)
    {
        // Escape message for PDF string literals
        var escaped = message.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        sb.AppendLine("1 0 obj");
        sb.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
        sb.AppendLine("endobj");
        sb.AppendLine("2 0 obj");
        sb.AppendLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        sb.AppendLine("endobj");
        sb.AppendLine("3 0 obj");
        sb.AppendLine("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792]");
        sb.AppendLine("   /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>");
        sb.AppendLine("endobj");

        var stream = $"BT /F1 12 Tf 72 720 Td ({escaped}) Tj ET";
        sb.AppendLine("4 0 obj");
        sb.AppendLine($"<< /Length {stream.Length} >>");
        sb.AppendLine("stream");
        sb.AppendLine(stream);
        sb.AppendLine("endstream");
        sb.AppendLine("endobj");
        sb.AppendLine("5 0 obj");
        sb.AppendLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        sb.AppendLine("endobj");
        sb.AppendLine("xref");
        sb.AppendLine("0 6");
        sb.AppendLine("0000000000 65535 f ");
        // Cross-reference table entries are approximations — sufficient for stub use
        sb.AppendLine("0000000009 00000 n ");
        sb.AppendLine("0000000058 00000 n ");
        sb.AppendLine("0000000115 00000 n ");
        sb.AppendLine("0000000266 00000 n ");
        sb.AppendLine("0000000395 00000 n ");
        sb.AppendLine("trailer");
        sb.AppendLine("<< /Size 6 /Root 1 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine("476");
        sb.AppendLine("%%EOF");

        return Encoding.Latin1.GetBytes(sb.ToString());
    }
}
