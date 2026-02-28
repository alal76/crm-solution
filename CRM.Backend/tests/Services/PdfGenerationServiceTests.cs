// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: BACK-008 (PDF Generation Service)
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: IPdfGenerationService.cs (CRM.Core/Ports/Input),
//   PdfGenerationService.cs (CRM.Infrastructure/Services).
//
// Constructor: PdfGenerationService(ILogger<PdfGenerationService> logger)
// Methods:
//   Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken ct)
//   Task<byte[]> GenerateQuotePdfAsync(int quoteId, CancellationToken ct)
//   Task<byte[]> GenerateFromHtmlAsync(string htmlContent, PdfOptions? options, CancellationToken ct)

using CRM.Core.Ports.Input;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for PdfGenerationService (BACK-008).
/// Verifies that the stub implementation returns non-empty byte arrays
/// for invoices, quotes, and raw HTML input.
/// </summary>
public class PdfGenerationServiceTests
{
    private readonly Mock<ILogger<PdfGenerationService>> _mockLogger;
    private readonly PdfGenerationService _service;

    public PdfGenerationServiceTests()
    {
        _mockLogger = new Mock<ILogger<PdfGenerationService>>();
        _service = new PdfGenerationService(_mockLogger.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GenerateInvoicePdfAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateInvoicePdfAsync_ShouldReturnNonEmptyBytes_WhenCalledWithValidId()
    {
        // Act
        var result = await _service.GenerateInvoicePdfAsync(42);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateInvoicePdfAsync_ShouldReturnPdfBytes_ThatStartWithPdfHeader()
    {
        // The stub builds a minimal PDF 1.4 document.
        var result = await _service.GenerateInvoicePdfAsync(1);

        var text = System.Text.Encoding.Latin1.GetString(result);
        text.Should().StartWith("%PDF-1.4");
    }

    // ────────────────────────────────────────────────────────────────────────
    // GenerateQuotePdfAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateQuotePdfAsync_ShouldReturnNonEmptyBytes_WhenCalledWithValidId()
    {
        // Act
        var result = await _service.GenerateQuotePdfAsync(10);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateQuotePdfAsync_ShouldReturnPdfBytes_ThatStartWithPdfHeader()
    {
        var result = await _service.GenerateQuotePdfAsync(5);

        var text = System.Text.Encoding.Latin1.GetString(result);
        text.Should().StartWith("%PDF-1.4");
    }

    [Fact]
    public async Task GenerateQuotePdfAsync_ShouldIncludeQuoteId_InPdfContent()
    {
        const int QuoteId = 99;
        var result = await _service.GenerateQuotePdfAsync(QuoteId);

        var text = System.Text.Encoding.Latin1.GetString(result);
        text.Should().Contain("99");
    }

    // ────────────────────────────────────────────────────────────────────────
    // GenerateFromHtmlAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateFromHtmlAsync_ShouldReturnNonEmptyBytes_WhenCalledWithHtmlContent()
    {
        // Act
        var result = await _service.GenerateFromHtmlAsync("<html><body>Hello</body></html>");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateFromHtmlAsync_ShouldIncludeHtmlContent_InReturnedBytes()
    {
        const string Html = "<html><body>Invoice #42</body></html>";
        var result = await _service.GenerateFromHtmlAsync(Html);

        var text = System.Text.Encoding.UTF8.GetString(result);
        text.Should().Contain("Invoice #42");
    }

    [Fact]
    public async Task GenerateFromHtmlAsync_ShouldIncludeHtmlComment_IndicatingPdfLibraryIsMissing()
    {
        var result = await _service.GenerateFromHtmlAsync("<p>test</p>");

        var text = System.Text.Encoding.UTF8.GetString(result);
        text.Should().Contain("PDF generation requires wkhtmltopdf or Playwright");
    }

    [Fact]
    public async Task GenerateFromHtmlAsync_ShouldAcceptNullOptions_WithoutThrowing()
    {
        // Act
        var act = async () => await _service.GenerateFromHtmlAsync("<p>ok</p>", options: null);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GenerateFromHtmlAsync_ShouldAcceptPdfOptions_WithoutThrowing()
    {
        var options = new PdfOptions
        {
            DocumentTitle = "Test Doc",
            IncludeHeaderFooter = false,
            Format = PaperFormat.Letter,
        };

        var act = async () => await _service.GenerateFromHtmlAsync("<p>doc</p>", options);

        await act.Should().NotThrowAsync();
    }
}
