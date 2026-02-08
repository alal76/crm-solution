// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0. See LICENSE for details.

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
        int? customerId = null,
        ContractStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Where(c => !c.IsDeleted);

        if (customerId.HasValue)
        {
            query = query.Where(c => c.AccountId == customerId.Value);
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
        contract.ContractNumber = await GenerateContractNumberAsync(cancellationToken);
        contract.CreatedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;

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

        contract.UpdatedAt = DateTime.UtcNow;
        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated contract {ContractId}", contract.Id);
        return contract;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts.FindAsync(new object[] { id }, cancellationToken);
        if (contract == null) return false;

        contract.IsDeleted = true;
        contract.UpdatedAt = DateTime.UtcNow;
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
        contract.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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
        original.UpdatedAt = DateTime.UtcNow;

        renewal.ParentContractId = contractId;
        renewal.UpdatedAt = DateTime.UtcNow;

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
        amendment.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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
        contract.UpdatedAt = DateTime.UtcNow;

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

    public async Task<IEnumerable<Contract>> GetActiveContractsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Account)
            .Where(c => c.AccountId == customerId && c.Status == ContractStatus.Active && !c.IsDeleted)
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
        if (!contracts.Any()) return 0;

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

    public async Task<decimal> GetTotalContractValueAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.AccountId == customerId && c.Status == ContractStatus.Active && !c.IsDeleted)
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
        contract.UpdatedAt = DateTime.UtcNow;

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

        // Placeholder PDF generation - would use a library like iTextSharp or similar
        var content = $@"CONTRACT DOCUMENT

Contract Number: {contract.ContractNumber}
Contract Name: {contract.Name}
Status: {contract.Status}

Account: {contract.Account?.Company ?? "N/A"}
Start Date: {contract.StartDate.ToShortDateString()}
End Date: {contract.EndDate.ToShortDateString()}
Total Value: {contract.TotalValue:C}

Description:
{contract.Description ?? "N/A"}

Terms and Conditions:
{contract.TermsAndConditions ?? "Standard terms apply."}

Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";

        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    #endregion
}
