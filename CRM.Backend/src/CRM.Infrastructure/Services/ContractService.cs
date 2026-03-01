// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IContractService for contract management operations.
/// </summary>
public class ContractService : IContractService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ContractService> _logger;

    public ContractService(ICrmDbContext context, ILogger<ContractService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<Contract>> GetAllAsync(
        int? accountId = null,
        ContractStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Where(c => !c.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(c => c.AccountId == accountId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber && !c.IsDeleted, cancellationToken);
    }

    public async Task<Contract> CreateAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        ValidateContractInput(contract);

        contract.ContractNumber = await GenerateContractNumberAsync(cancellationToken);
        contract.CreatedAt = DateTime.UtcNow;

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created contract {ContractNumber} for account {AccountId}", contract.ContractNumber, contract.AccountId);
        return contract;
    }

    public async Task<Contract> UpdateAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Contracts.FindAsync(new object[] { contract.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Contract {contract.Id} not found");
        }

        ValidateContractInput(contract);

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated contract {ContractId}", contract.Id);
        return contract;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts.FindAsync(new object[] { id }, cancellationToken);
        if (contract == null)
        {
            return false;
        }

        contract.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted contract {ContractId}", id);
        return true;
    }

    #endregion

    #region Contract Operations

    public async Task<Contract> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId && !q.IsDeleted, cancellationToken);

        if (quote == null)
        {
            throw new InvalidOperationException($"Quote {quoteId} not found");
        }

        var contract = new Contract
        {
            ContractNumber = await GenerateContractNumberAsync(cancellationToken),
            Name = $"Contract for {quote.QuoteNumber}",
            AccountId = quote.AccountId ?? 0,
            ContactId = quote.ContactId,
            Status = ContractStatus.Draft,
            ContractType = ContractType.Service,
            TotalValue = quote.TotalAmount,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created contract {ContractNumber} from quote {QuoteId}", contract.ContractNumber, quoteId);
        return contract;
    }

    public async Task<Contract> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var contract = new Contract
        {
            ContractNumber = await GenerateContractNumberAsync(cancellationToken),
            Name = $"Contract for {order.OrderNumber}",
            AccountId = order.AccountId,
            ContactId = order.ContactId,
            Status = ContractStatus.Draft,
            ContractType = ContractType.Service,
            TotalValue = order.TotalAmount,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created contract {ContractNumber} from order {OrderId}", contract.ContractNumber, orderId);
        return contract;
    }

    public async Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken = default)
    {
        var prefix = $"CON-{DateTime.UtcNow:yyMM}-";
        var lastContract = await _context.Contracts
            .Where(c => c.ContractNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ContractNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastContract != null)
        {
            var lastNum = lastContract.ContractNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastNum, out var num))
            {
                sequence = num + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    public async Task<Contract> CloneForRenewalAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var original = await GetByIdAsync(contractId, cancellationToken);
        if (original == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        var renewal = new Contract
        {
            ContractNumber = await GenerateContractNumberAsync(cancellationToken),
            Name = $"{original.Name} - Renewal",
            Description = original.Description,
            AccountId = original.AccountId,
            ContactId = original.ContactId,
            OwnerId = original.OwnerId,
            ParentContractId = original.Id,
            Status = ContractStatus.Draft,
            ContractType = original.ContractType,
            TotalValue = original.TotalValue,
            StartDate = original.EndDate,
            EndDate = original.EndDate.AddYears(1),
            TermsAndConditions = original.TermsAndConditions,
            PaymentTerms = original.PaymentTerms,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(renewal);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned contract {OriginalId} for renewal as {NewNumber}", contractId, renewal.ContractNumber);
        return renewal;
    }

    #endregion

    #region Status Management

    public async Task<Contract> UpdateStatusAsync(int contractId, ContractStatus status, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.Status = status;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated contract {ContractId} status to {Status}", contractId, status);
        return contract;
    }

    public async Task<Contract> ActivateAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = DateTime.UtcNow;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated contract {ContractId}", contractId);
        return contract;
    }

    public async Task<Contract> SuspendAsync(int contractId, string reason, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.Status = ContractStatus.OnHold;
        contract.SuspensionReason = reason;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Suspended contract {ContractId}: {Reason}", contractId, reason);
        return contract;
    }

    public async Task<Contract> TerminateAsync(int contractId, string reason, DateTime? terminationDate = null, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.Status = ContractStatus.Terminated;
        contract.TerminationReason = reason;
        contract.TerminatedAt = terminationDate ?? DateTime.UtcNow;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Terminated contract {ContractId}: {Reason}", contractId, reason);
        return contract;
    }

    public async Task<Contract> ExpireAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.Status = ContractStatus.Expired;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expired contract {ContractId}", contractId);
        return contract;
    }

    #endregion

    #region Renewal

    public async Task<Contract> InitiateRenewalAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.RenewalInitiatedAt = DateTime.UtcNow;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Initiated renewal for contract {ContractId}", contractId);
        return contract;
    }

    public async Task<Contract> CompleteRenewalAsync(int contractId, int newContractId, CancellationToken cancellationToken = default)
    {
        var original = await GetByIdAsync(contractId, cancellationToken);
        if (original == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        var renewal = await GetByIdAsync(newContractId, cancellationToken);
        if (renewal == null)
        {
            throw new InvalidOperationException($"Renewal contract {newContractId} not found");
        }

        original.Status = ContractStatus.Renewed;
        original.RenewalCompletedAt = DateTime.UtcNow;

        renewal.ParentContractId = contractId;

        _context.Contracts.Update(original);
        _context.Contracts.Update(renewal);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed renewal: Contract {OriginalId} renewed as {NewId}", contractId, newContractId);
        return original;
    }

    public async Task<IEnumerable<Contract>> GetContractsDueForRenewalAsync(int withinDays, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(withinDays);

        return await _context.Contracts
            .Include(c => c.Account)
            .Where(c => !c.IsDeleted)
            .Where(c => c.Status == ContractStatus.Active)
            .Where(c => c.EndDate <= cutoffDate && c.EndDate >= DateTime.UtcNow.Date)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetRenewalHistoryAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var history = new List<Contract>();
        var contract = await GetByIdAsync(contractId, cancellationToken);

        while (contract != null)
        {
            history.Add(contract);
            if (contract.ParentContractId.HasValue)
            {
                contract = await GetByIdAsync(contract.ParentContractId.Value, cancellationToken);
            }
            else
            {
                break;
            }
        }

        return history;
    }

    #endregion

    #region Amendment

    public async Task<Contract> CreateAmendmentAsync(int contractId, Contract amendment, CancellationToken cancellationToken = default)
    {
        var original = await GetByIdAsync(contractId, cancellationToken);
        if (original == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        amendment.ContractNumber = await GenerateContractNumberAsync(cancellationToken) + "-AMD";
        amendment.ParentContractId = contractId;
        amendment.ContractType = ContractType.Amendment;
        amendment.Status = ContractStatus.Draft;
        amendment.CreatedAt = DateTime.UtcNow;

        _context.Contracts.Add(amendment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created amendment {AmendmentNumber} for contract {ContractId}", amendment.ContractNumber, contractId);
        return amendment;
    }

    public async Task<IEnumerable<Contract>> GetAmendmentsAsync(int contractId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.ParentContractId == contractId && c.ContractType == ContractType.Amendment && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Signature

    public async Task<bool> SendForSignatureAsync(int contractId, IEnumerable<ContractSigner> signers, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.SentForSignatureAt = DateTime.UtcNow;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractId} sent for signature to {SignerCount} signers", contractId, signers.Count());
        return true;
    }

    public async Task<Contract> RecordSignatureAsync(int contractId, int signerId, string signatureData, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.SignedDate = DateTime.UtcNow;
        contract.SignedBy = signerId.ToString();

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded signature on contract {ContractId} by signer {SignerId}", contractId, signerId);
        return contract;
    }

    public async Task<ContractSignatureStatus> GetSignatureStatusAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        return new ContractSignatureStatus
        {
            ContractId = contractId,
            AllSigned = contract.IsSigned,
            TotalSigners = 1,
            SignedCount = contract.IsSigned ? 1 : 0,
            Signers = new List<CRM.Core.Interfaces.SignerStatus>
            {
                new CRM.Core.Interfaces.SignerStatus
                {
                    SignerId = int.TryParse(contract.SignedBy, out var id) ? id : 0,
                    HasSigned = contract.IsSigned,
                    SignedAt = contract.SignedDate
                }
            }
        };
    }

    #endregion

    #region Queries

    public async Task<IEnumerable<Contract>> GetActiveContractsAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Account)
            .Where(c => c.AccountId == accountId && c.Status == ContractStatus.Active && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Account)
            .Where(c => !c.IsDeleted)
            .Where(c => c.Status == ContractStatus.Active)
            .Where(c => c.EndDate >= fromDate && c.EndDate <= toDate)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ContractStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts.Where(c => !c.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= toDate.Value);
        }

        var contracts = await query.ToListAsync(cancellationToken);

        var totalContracts = contracts.Count;
        var activeContracts = contracts.Count(c => c.Status == ContractStatus.Active);
        var expiringContracts = contracts.Count(c => c.Status == ContractStatus.Active && c.EndDate <= DateTime.UtcNow.AddDays(30));
        var expiredContracts = contracts.Count(c => c.Status == ContractStatus.Expired);
        var renewedContracts = contracts.Count(c => c.Status == ContractStatus.Renewed);

        return new ContractStatistics
        {
            TotalContracts = totalContracts,
            ActiveContracts = activeContracts,
            ExpiringContracts = expiringContracts,
            ExpiredContracts = expiredContracts,
            PendingRenewals = contracts.Count(c => c.RenewalInitiatedAt.HasValue && !c.RenewalCompletedAt.HasValue),
            TotalContractValue = contracts.Sum(c => c.TotalValue),
            ActiveContractValue = contracts.Where(c => c.Status == ContractStatus.Active).Sum(c => c.TotalValue),
            RenewalRate = totalContracts > 0 ? (double)renewedContracts / totalContracts * 100 : 0,
            AverageContractLength = CalculateAverageContractLength(contracts),
            ContractsByType = contracts.GroupBy(c => c.ContractType).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private double CalculateAverageContractLength(List<Contract> contracts)
    {
        if (!contracts.Any())
        {
            return 0;
        }

        var totalDays = contracts.Sum(c => (c.EndDate - c.StartDate).TotalDays);
        return totalDays / contracts.Count;
    }

    public async Task<IEnumerable<Contract>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _context.Contracts
            .Include(c => c.Account)
            .Where(c => !c.IsDeleted)
            .Where(c => c.ContractNumber.ToLower().Contains(term) ||
                        c.Name.ToLower().Contains(term) ||
                        (c.Description != null && c.Description.ToLower().Contains(term)))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalContractValueAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.AccountId == accountId && c.Status == ContractStatus.Active && !c.IsDeleted)
            .SumAsync(c => c.TotalValue, cancellationToken);
    }

    #endregion

    #region Documents

    public async Task<bool> AttachDocumentAsync(int contractId, string documentPath, string documentType, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        contract.DocumentUrl = documentPath;

        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attached document to contract {ContractId}", contractId);
        return true;
    }

    public async Task<IEnumerable<ContractDocument>> GetDocumentsAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            return Enumerable.Empty<ContractDocument>();
        }

        var documents = new List<ContractDocument>();
        if (!string.IsNullOrEmpty(contract.DocumentUrl))
        {
            documents.Add(new ContractDocument
            {
                Id = contract.Id,
                FileName = Path.GetFileName(contract.DocumentUrl),
                FilePath = contract.DocumentUrl,
                DocumentType = "Contract",
                UploadedAt = contract.CreatedAt
            });
        }

        return documents;
    }

    public async Task<byte[]> GenerateContractPdfAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        // Build text content lines for the PDF page
        var textLines = new List<string>
        {
            "CONTRACT DOCUMENT",
            "",
            $"Contract Number: {contract.ContractNumber}",
            $"Contract Name: {contract.Name}",
            $"Status: {contract.Status}",
            "",
            $"Account: {contract.Account?.Company ?? "N/A"}",
            $"Start Date: {contract.StartDate:yyyy-MM-dd}",
            $"End Date: {contract.EndDate:yyyy-MM-dd}",
            $"Total Value: {contract.TotalValue:F2}",
            ""
        };

        textLines.Add("Description:");
        var desc = contract.Description ?? "N/A";
        foreach (var descLine in desc.Split('\n'))
            textLines.Add(descLine.TrimEnd('\r'));

        textLines.Add("");
        textLines.Add("Terms and Conditions:");
        var terms = contract.TermsAndConditions ?? "Standard terms apply.";
        foreach (var termLine in terms.Split('\n'))
            textLines.Add(termLine.TrimEnd('\r'));

        textLines.Add("");
        textLines.Add($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

        // Build PDF content stream (text drawing operators)
        var contentSb = new System.Text.StringBuilder();
        contentSb.Append("BT\n");
        contentSb.Append("/F1 18 Tf\n");
        contentSb.Append("72 720 Td\n");

        var isFirstLine = true;
        foreach (var line in textLines)
        {
            if (isFirstLine)
            {
                contentSb.Append($"({EscapePdfString(line)}) Tj\n");
                contentSb.Append("/F1 11 Tf\n");
                contentSb.Append("0 -24 Td\n");
                isFirstLine = false;
            }
            else
            {
                contentSb.Append("0 -16 Td\n");
                contentSb.Append($"({EscapePdfString(line)}) Tj\n");
            }
        }

        contentSb.Append("ET\n");
        var contentBytes = System.Text.Encoding.ASCII.GetBytes(contentSb.ToString());

        // Build a valid PDF 1.4 document with proper cross-reference table
        using var ms = new MemoryStream();

        void Write(string s)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(s);
            ms.Write(bytes, 0, bytes.Length);
        }

        var offsets = new List<long>();

        Write("%PDF-1.4\n");

        // Object 1: Catalog
        offsets.Add(ms.Position);
        Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        // Object 2: Pages
        offsets.Add(ms.Position);
        Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        // Object 3: Page
        offsets.Add(ms.Position);
        Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");

        // Object 4: Content stream
        offsets.Add(ms.Position);
        Write($"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        ms.Write(contentBytes, 0, contentBytes.Length);
        Write("\nendstream\nendobj\n");

        // Object 5: Font (Helvetica - built-in PDF Type1 font)
        offsets.Add(ms.Position);
        Write("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        // Cross-reference table
        var xrefOffset = ms.Position;
        Write("xref\n");
        Write($"0 {offsets.Count + 1}\n");
        Write("0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            Write($"{offset:D10} 00000 n \n");
        }

        // Trailer
        Write($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");

        _logger.LogInformation("Generated PDF for contract {ContractId} ({Bytes} bytes)", contractId, ms.Length);
        return ms.ToArray();
    }

    /// <summary>
    /// Escapes a string for safe inclusion in a PDF text string (parentheses delimited).
    /// </summary>
    private static string EscapePdfString(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '(':
                    sb.Append("\\(");
                    break;
                case ')':
                    sb.Append("\\)");
                    break;
                case '\r':
                    break;
                case '\n':
                    sb.Append(' ');
                    break;
                default:
                    // Only include printable ASCII; replace others with '?'
                    sb.Append(c >= 32 && c <= 126 ? c : '?');
                    break;
            }
        }
        return sb.ToString();
    }

    #endregion

    #region Validation Helpers

    private static void ValidateContractInput(Contract contract)
    {
        if (contract.EndDate != default && contract.StartDate != default && contract.EndDate <= contract.StartDate)
        {
            throw new ArgumentException("EndDate must be after StartDate.", nameof(contract));
        }

        if (contract.TotalValue < 0)
        {
            throw new ArgumentException("Contract value must be zero or positive.", nameof(contract));
        }
    }

    #endregion

    #region Bulk Operations (TODO-SALES005-013)

    /// <summary>Bulk update status for multiple contracts.</summary>
    public async Task<int> BulkUpdateStatusAsync(IEnumerable<int> contractIds, ContractStatus status, CancellationToken cancellationToken = default)
    {
        var ids = contractIds.ToList();
        if (!ids.Any())
        {
            return 0;
        }

        _logger.LogInformation("Bulk updating {Count} contracts to status {Status}", ids.Count, status);

        var contracts = await _context.Contracts
            .Where(c => ids.Contains(c.Id) && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var contract in contracts)
        {
            contract.Status = status;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Bulk updated {Count} contracts to status {Status}", contracts.Count, status);

        return contracts.Count;
    }

    #endregion

    #region Version History (TODO-SALES005-016)

    /// <summary>Gets version history for a contract.</summary>
    public async Task<IEnumerable<ContractVersion>> GetVersionHistoryAsync(int contractId, CancellationToken cancellationToken = default)
    {
        return await _context.ContractVersions
            .Where(v => v.ContractId == contractId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Creates a new version snapshot of a contract.</summary>
    public async Task<ContractVersion> CreateVersionSnapshotAsync(int contractId, string changeDescription, int? modifiedByUserId = null, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        // Mark previous versions as not current
        var previousVersions = await _context.ContractVersions
            .Where(v => v.ContractId == contractId && v.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var pv in previousVersions)
        {
            pv.IsCurrent = false;
        }

        // Get next version number
        var lastVersion = await _context.ContractVersions
            .Where(v => v.ContractId == contractId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextVersionNumber = (lastVersion?.VersionNumber ?? 0) + 1;

        // Create snapshot JSON
        var snapshot = System.Text.Json.JsonSerializer.Serialize(new
        {
            contract.Name,
            contract.ContractNumber,
            contract.Description,
            ContractType = contract.ContractType,
            contract.Status,
            contract.StartDate,
            contract.EndDate,
            contract.TotalValue,
            contract.TermsAndConditions,
            contract.PaymentTerms,
            contract.AutoRenew,
            contract.RenewalNoticeDays
        });

        var version = new ContractVersion
        {
            ContractId = contractId,
            VersionNumber = nextVersionNumber,
            ChangeDescription = changeDescription,
            ChangesJson = "[]", // Could be populated with actual diff
            SnapshotJson = snapshot,
            IsCurrent = true,
            CreatedById = modifiedByUserId ?? 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContractVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created version {Version} for contract {ContractId}", nextVersionNumber, contractId);
        return version;
    }

    /// <summary>Restores a contract to a previous version.</summary>
    public async Task<Contract> RestoreVersionAsync(int contractId, int versionId, CancellationToken cancellationToken = default)
    {
        var contract = await GetByIdAsync(contractId, cancellationToken);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract {contractId} not found");
        }

        var version = await _context.ContractVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.ContractId == contractId, cancellationToken);

        if (version == null)
        {
            throw new InvalidOperationException($"Version {versionId} not found for contract {contractId}");
        }

        if (string.IsNullOrEmpty(version.SnapshotJson))
        {
            throw new InvalidOperationException($"Version {versionId} has no snapshot data");
        }

        // Parse snapshot and restore fields
        var snapshot = System.Text.Json.JsonDocument.Parse(version.SnapshotJson);
        var root = snapshot.RootElement;

        if (root.TryGetProperty("Name", out var name))
        {
            contract.Name = name.GetString() ?? contract.Name;
        }
        if (root.TryGetProperty("Description", out var desc))
        {
            contract.Description = desc.GetString();
        }
        if (root.TryGetProperty("TotalValue", out var value))
        {
            contract.TotalValue = value.GetDecimal();
        }
        if (root.TryGetProperty("TermsAndConditions", out var terms))
        {
            contract.TermsAndConditions = terms.GetString();
        }
        if (root.TryGetProperty("PaymentTerms", out var payment))
        {
            contract.PaymentTerms = payment.GetString();
        }


        // Create a new version recording the restore
        await CreateVersionSnapshotAsync(contractId, $"Restored from version {version.VersionNumber}", null, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Restored contract {ContractId} to version {VersionId}", contractId, versionId);
        return contract;
    }

    #endregion
}
