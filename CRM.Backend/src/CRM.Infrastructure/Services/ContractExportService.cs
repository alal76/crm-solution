// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IContractExportService for contract export operations.
/// Handles PDF, Excel, and Word export of contracts.
/// </summary>
public class ContractExportService : IContractExportService
{
    private const string ContractNotFoundMessage = "Contract {0} not found";

    private readonly ICrmDbContext _context;
    private readonly ILogger<ContractExportService> _logger;
    private readonly IContractService _contractService;

    public ContractExportService(
        ICrmDbContext context, 
        ILogger<ContractExportService> logger,
        IContractService contractService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
    }

    public async Task<ContractExportResultDto> ExportAsync(
        int contractId,
        string format,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted, cancellationToken);

        if (contract == null)
        {
            throw new InvalidOperationException(string.Format(ContractNotFoundMessage, contractId));
        }

        byte[] content;
        string contentType;
        string extension;

        switch (format.ToUpperInvariant())
        {
            case "PDF":
                content = await ExportToPdfAsync(contractId, cancellationToken);
                contentType = "application/pdf";
                extension = "pdf";
                break;
            case "EXCEL":
            case "XLSX":
                content = await ExportToExcelAsync(contractId, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                extension = "xlsx";
                break;
            case "WORD":
            case "DOCX":
                content = await ExportToWordAsync(contractId, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                extension = "docx";
                break;
            default:
                throw new InvalidOperationException($"Unsupported export format: {format}");
        }

        _logger.LogInformation("Contract {ContractId} exported to {Format}", contractId, format);

        return new ContractExportResultDto
        {
            ContractId = contractId,
            FileName = $"{contract.ContractNumber}.{extension}",
            ContentType = contentType,
            Content = content,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<byte[]> ExportToPdfAsync(int contractId, CancellationToken cancellationToken = default)
    {
        // Use the existing PDF generation from ContractService
        return await _contractService.GenerateContractPdfAsync(contractId, cancellationToken);
    }

    public async Task<byte[]> ExportToExcelAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted, cancellationToken);

        if (contract == null)
        {
            throw new InvalidOperationException(string.Format(ContractNotFoundMessage, contractId));
        }

        // Simple CSV-style Excel content (for stub purposes)
        // In production, use a library like EPPlus or ClosedXML
        var sb = new StringBuilder();
        sb.AppendLine("Field,Value");
        sb.AppendLine($"Contract Number,\"{contract.ContractNumber}\"");
        sb.AppendLine($"Name,\"{contract.Name}\"");
        sb.AppendLine($"Status,\"{contract.Status}\"");
        sb.AppendLine($"Type,\"{contract.ContractType}\"");
        sb.AppendLine($"Account,\"{contract.Account?.Company ?? "N/A"}\"");
        sb.AppendLine($"Contact,\"{contract.Contact?.FirstName} {contract.Contact?.LastName}\"");
        sb.AppendLine($"Start Date,\"{contract.StartDate:yyyy-MM-dd}\"");
        sb.AppendLine($"End Date,\"{contract.EndDate:yyyy-MM-dd}\"");
        sb.AppendLine($"Total Value,\"{contract.TotalValue:F2}\"");
        sb.AppendLine($"Auto Renew,\"{contract.AutoRenew}\"");
        sb.AppendLine($"Is Signed,\"{contract.IsSigned}\"");
        sb.AppendLine($"Description,\"{(contract.Description ?? "").Replace("\"", "\"\"")}\"");
        sb.AppendLine($"Terms,\"{(contract.TermsAndConditions ?? "").Replace("\"", "\"\"")}\"");
        sb.AppendLine($"Created At,\"{contract.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");

        _logger.LogInformation("Contract {ContractId} exported to Excel format", contractId);
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportToWordAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted, cancellationToken);

        if (contract == null)
        {
            throw new InvalidOperationException(string.Format(ContractNotFoundMessage, contractId));
        }

        // Simple RTF content (for stub purposes)
        // In production, use a library like DocX or OpenXML SDK
        var sb = new StringBuilder();
        sb.AppendLine(@"{\rtf1\ansi\deff0");
        sb.AppendLine(@"{\fonttbl{\f0 Arial;}}");
        sb.AppendLine(@"\f0\fs28\b CONTRACT DOCUMENT\b0\par\par");
        sb.AppendLine($@"\fs22\b Contract Number:\b0  {contract.ContractNumber}\par");
        sb.AppendLine($@"\b Contract Name:\b0  {EscapeRtf(contract.Name)}\par");
        sb.AppendLine($@"\b Status:\b0  {contract.Status}\par");
        sb.AppendLine($@"\b Type:\b0  {contract.ContractType}\par\par");
        sb.AppendLine(@"\b PARTIES\b0\par");
        sb.AppendLine($@"Account: {EscapeRtf(contract.Account?.Company ?? "N/A")}\par");
        sb.AppendLine($@"Contact: {EscapeRtf($"{contract.Contact?.FirstName} {contract.Contact?.LastName}")}\par\par");
        sb.AppendLine(@"\b DATES\b0\par");
        sb.AppendLine($@"Start Date: {contract.StartDate:yyyy-MM-dd}\par");
        sb.AppendLine($@"End Date: {contract.EndDate:yyyy-MM-dd}\par\par");
        sb.AppendLine(@"\b VALUE\b0\par");
        sb.AppendLine($@"Total Value: {contract.TotalValue:C}\par");
        sb.AppendLine($@"Auto Renew: {(contract.AutoRenew ? "Yes" : "No")}\par\par");
        sb.AppendLine(@"\b DESCRIPTION\b0\par");
        sb.AppendLine($@"{EscapeRtf(contract.Description ?? "N/A")}\par\par");
        sb.AppendLine(@"\b TERMS AND CONDITIONS\b0\par");
        sb.AppendLine($@"{EscapeRtf(contract.TermsAndConditions ?? "Standard terms apply.")}\par\par");
        sb.AppendLine($@"\fs18 Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\par");
        sb.AppendLine("}");

        _logger.LogInformation("Contract {ContractId} exported to Word format", contractId);
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<ContractExportResultDto> ExportBulkAsync(
        int[] contractIds,
        string format,
        CancellationToken cancellationToken = default)
    {
        if (contractIds == null || contractIds.Length == 0)
        {
            throw new ArgumentException("At least one contract ID is required", nameof(contractIds));
        }

        var contracts = await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Where(c => contractIds.Contains(c.Id) && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!contracts.Any())
        {
            throw new InvalidOperationException("No contracts found for the provided IDs");
        }

        // For bulk export, create a combined file
        var sb = new StringBuilder();
        
        if (format.ToUpperInvariant() == "EXCEL" || format.ToUpperInvariant() == "XLSX")
        {
            sb.AppendLine("Contract Number,Name,Status,Type,Account,Start Date,End Date,Value");
            foreach (var contract in contracts)
            {
                sb.AppendLine($"\"{contract.ContractNumber}\",\"{contract.Name}\",\"{contract.Status}\",\"{contract.ContractType}\",\"{contract.Account?.Company}\",\"{contract.StartDate:yyyy-MM-dd}\",\"{contract.EndDate:yyyy-MM-dd}\",\"{contract.TotalValue:F2}\"");
            }
        }
        else
        {
            foreach (var contract in contracts)
            {
                sb.AppendLine($"Contract: {contract.ContractNumber} - {contract.Name}");
                sb.AppendLine($"Status: {contract.Status}, Type: {contract.ContractType}");
                sb.AppendLine($"Account: {contract.Account?.Company}");
                sb.AppendLine($"Dates: {contract.StartDate:yyyy-MM-dd} to {contract.EndDate:yyyy-MM-dd}");
                sb.AppendLine($"Value: {contract.TotalValue:C}");
                sb.AppendLine("---");
            }
        }

        _logger.LogInformation("Bulk export of {Count} contracts to {Format}", contracts.Count, format);

        return new ContractExportResultDto
        {
            ContractId = contractIds[0],
            FileName = $"contracts_export_{DateTime.UtcNow:yyyyMMdd}.{(format.ToUpperInvariant() == "EXCEL" ? "csv" : "txt")}",
            ContentType = format.ToUpperInvariant() == "EXCEL" ? "text/csv" : "text/plain",
            Content = Encoding.UTF8.GetBytes(sb.ToString()),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public IEnumerable<string> GetSupportedFormats()
    {
        return new[] { "PDF", "EXCEL", "XLSX", "WORD", "DOCX" };
    }

    private static string EscapeRtf(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("\\", "\\\\")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("\n", "\\par ");
    }
}
