// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for contract management operations.
/// Handles contract lifecycle from creation to renewal.
/// </summary>
public interface IContractService
{
    #region CRUD Operations

    /// <summary>Gets all contracts with optional filtering.</summary>
    Task<IEnumerable<Contract>> GetAllAsync(
        int? customerId = null,
        ContractStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a contract by ID.</summary>
    Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a contract by contract number.</summary>
    Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken cancellationToken = default);

    /// <summary>Creates a new contract.</summary>
    Task<Contract> CreateAsync(Contract contract, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing contract.</summary>
    Task<Contract> UpdateAsync(Contract contract, CancellationToken cancellationToken = default);

    /// <summary>Deletes a contract (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Contract Operations

    /// <summary>Creates a contract from a quote.</summary>
    Task<Contract> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);

    /// <summary>Creates a contract from an order.</summary>
    Task<Contract> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Generates the next contract number.</summary>
    Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>Clones an existing contract for renewal.</summary>
    Task<Contract> CloneForRenewalAsync(int contractId, CancellationToken cancellationToken = default);

    #endregion

    #region Status Management

    /// <summary>Updates contract status.</summary>
    Task<Contract> UpdateStatusAsync(int contractId, ContractStatus status, CancellationToken cancellationToken = default);

    /// <summary>Activates a contract.</summary>
    Task<Contract> ActivateAsync(int contractId, CancellationToken cancellationToken = default);

    /// <summary>Suspends a contract.</summary>
    Task<Contract> SuspendAsync(int contractId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Terminates a contract.</summary>
    Task<Contract> TerminateAsync(int contractId, string reason, DateTime? terminationDate = null, CancellationToken cancellationToken = default);

    /// <summary>Expires a contract.</summary>
    Task<Contract> ExpireAsync(int contractId, CancellationToken cancellationToken = default);

    #endregion

    #region Renewal

    /// <summary>Initiates contract renewal process.</summary>
    Task<Contract> InitiateRenewalAsync(int contractId, CancellationToken cancellationToken = default);

    /// <summary>Completes contract renewal.</summary>
    Task<Contract> CompleteRenewalAsync(int contractId, int newContractId, CancellationToken cancellationToken = default);

    /// <summary>Gets contracts due for renewal within days.</summary>
    Task<IEnumerable<Contract>> GetContractsDueForRenewalAsync(int withinDays, CancellationToken cancellationToken = default);

    /// <summary>Gets renewal history for a contract.</summary>
    Task<IEnumerable<Contract>> GetRenewalHistoryAsync(int contractId, CancellationToken cancellationToken = default);

    #endregion

    #region Amendment

    /// <summary>Creates an amendment to a contract.</summary>
    Task<Contract> CreateAmendmentAsync(int contractId, Contract amendment, CancellationToken cancellationToken = default);

    /// <summary>Gets all amendments for a contract.</summary>
    Task<IEnumerable<Contract>> GetAmendmentsAsync(int contractId, CancellationToken cancellationToken = default);

    #endregion

    #region Signature

    /// <summary>Sends contract for signature.</summary>
    Task<bool> SendForSignatureAsync(int contractId, IEnumerable<ContractSigner> signers, CancellationToken cancellationToken = default);

    /// <summary>Records a signature on the contract.</summary>
    Task<Contract> RecordSignatureAsync(int contractId, int signerId, string signatureData, CancellationToken cancellationToken = default);

    /// <summary>Gets signature status for a contract.</summary>
    Task<ContractSignatureStatus> GetSignatureStatusAsync(int contractId, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets active contracts for a customer.</summary>
    Task<IEnumerable<Contract>> GetActiveContractsAsync(int customerId, CancellationToken cancellationToken = default);

    /// <summary>Gets expiring contracts within a date range.</summary>
    Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets contract statistics.</summary>
    Task<ContractStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Searches contracts by criteria.</summary>
    Task<IEnumerable<Contract>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>Gets contract value over time for a customer.</summary>
    Task<decimal> GetTotalContractValueAsync(int customerId, CancellationToken cancellationToken = default);

    #endregion

    #region Documents

    /// <summary>Attaches a document to a contract.</summary>
    Task<bool> AttachDocumentAsync(int contractId, string documentPath, string documentType, CancellationToken cancellationToken = default);

    /// <summary>Gets documents for a contract.</summary>
    Task<IEnumerable<ContractDocument>> GetDocumentsAsync(int contractId, CancellationToken cancellationToken = default);

    /// <summary>Generates contract PDF.</summary>
    Task<byte[]> GenerateContractPdfAsync(int contractId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Contract signer information.
/// </summary>
public class ContractSigner
{
    public int? ContactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int SigningOrder { get; set; }
}

/// <summary>
/// Contract signature status.
/// </summary>
public class ContractSignatureStatus
{
    public int ContractId { get; set; }
    public bool AllSigned { get; set; }
    public int TotalSigners { get; set; }
    public int SignedCount { get; set; }
    public List<SignerStatus> Signers { get; set; } = new();
}

/// <summary>
/// Individual signer status.
/// </summary>
public class SignerStatus
{
    public int SignerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasSigned { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
}

/// <summary>
/// Contract document information.
/// </summary>
public class ContractDocument
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? UploadedBy { get; set; }
}

/// <summary>
/// Contract statistics for reporting.
/// </summary>
public class ContractStatistics
{
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int ExpiringContracts { get; set; }
    public int ExpiredContracts { get; set; }
    public int PendingRenewals { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal ActiveContractValue { get; set; }
    public double RenewalRate { get; set; }
    public double AverageContractLength { get; set; }
    public Dictionary<ContractType, int> ContractsByType { get; set; } = new();
}
