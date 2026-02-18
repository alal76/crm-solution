// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// Contract data transfer object for read operations
/// </summary>
public class ContractDto
{
    public int Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    
    public ContractStatus Status { get; set; }
    public ContractType ContractType { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AnnualValue { get; set; }
    
    public DateTime? ActivatedAt { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public DateTime? SignedDate { get; set; }
    
    public bool IsSigned { get; set; }
    public bool AutoRenew { get; set; }
    public int RenewalTermMonths { get; set; }
    
    public string? PaymentTerms { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? DocumentUrl { get; set; }
    
    public int? ParentContractId { get; set; }
    public int? OpportunityId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public bool IsExpiringSoon => Status == ContractStatus.Active && EndDate <= DateTime.UtcNow.AddDays(30);
    public bool IsExpired => Status == ContractStatus.Expired || (Status == ContractStatus.Active && EndDate < DateTime.UtcNow.Date);
    public int DaysUntilExpiry => (EndDate.Date - DateTime.UtcNow.Date).Days;
}

/// <summary>
/// Create contract DTO
/// </summary>
public class CreateContractDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public int AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? OwnerId { get; set; }
    
    public ContractType ContractType { get; set; } = ContractType.Service;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalValue { get; set; }
    public decimal? AnnualValue { get; set; }
    
    public bool AutoRenew { get; set; }
    public int RenewalTermMonths { get; set; } = 12;
    
    public string? PaymentTerms { get; set; }
    public string? TermsAndConditions { get; set; }
    
    public int? OpportunityId { get; set; }
}

/// <summary>
/// Update contract DTO
/// </summary>
public class UpdateContractDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    public int? ContactId { get; set; }
    public int? OwnerId { get; set; }
    
    public ContractType? ContractType { get; set; }
    
    public DateTime? EndDate { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? AnnualValue { get; set; }
    
    public bool? AutoRenew { get; set; }
    public int? RenewalTermMonths { get; set; }
    
    public string? PaymentTerms { get; set; }
    public string? TermsAndConditions { get; set; }
}

/// <summary>
/// Contract filter DTO
/// </summary>
public class ContractFilterDto
{
    public int? AccountId { get; set; }
    public ContractStatus? Status { get; set; }
    public ContractType? ContractType { get; set; }
    public bool? AutoRenew { get; set; }
    public DateTime? ExpiringBefore { get; set; }
    public DateTime? ExpiringAfter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "StartDate";
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// Contract line item DTO
/// </summary>
public class ContractLineItemDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Create contract line item DTO
/// </summary>
public class CreateContractLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Contract renewal DTO
/// </summary>
public class ContractRenewalDto
{
    public int Id { get; set; }
    public int OriginalContractId { get; set; }
    public int RenewalContractId { get; set; }
    public DateTime OriginalEndDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public DateTime RenewalDate { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Contract statistics DTO
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

/// <summary>
/// Contract signature status DTO
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
/// Signer status DTO
/// </summary>
public class SignerStatus
{
    public int SignerId { get; set; }
    public string? SignerName { get; set; }
    public string? SignerEmail { get; set; }
    public bool HasSigned { get; set; }
    public DateTime? SignedAt { get; set; }
}

/// <summary>
/// Contract document DTO
/// </summary>
public class ContractDocument
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? UploadedBy { get; set; }
}

/// <summary>
/// Terminate contract request DTO
/// </summary>
public class TerminateContractRequestDto
{
    public string Reason { get; set; } = string.Empty;
    public DateTime? TerminationDate { get; set; }
}

/// <summary>
/// Initiate renewal request DTO
/// </summary>
public class InitiateRenewalRequestDto
{
    public int RenewalTermMonths { get; set; } = 12;
    public string? RenewalNotes { get; set; }
}
