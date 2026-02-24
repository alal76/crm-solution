// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for contract version data.
/// </summary>
public class ContractVersionDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string? VersionLabel { get; set; }
    public string? ChangeDescription { get; set; }
    public string? Reason { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsArchived { get; set; }
    public int CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ContractFieldChangeDto>? Changes { get; set; }
}

/// <summary>
/// DTO for contract field change.
/// </summary>
public class ContractFieldChangeDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = "Modified";
}

/// <summary>
/// DTO for creating a contract version.
/// </summary>
public class CreateContractVersionDto
{
    public int ContractId { get; set; }
    public string? VersionLabel { get; set; }
    public string? ChangeDescription { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for contract export request.
/// </summary>
public class ContractExportRequestDto
{
    public int ContractId { get; set; }
    public string Format { get; set; } = "PDF"; // PDF, Excel, Word
    public bool IncludeHistory { get; set; }
    public bool IncludeSignatures { get; set; }
}

/// <summary>
/// DTO for contract export result.
/// </summary>
public class ContractExportResultDto
{
    public int ContractId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for bulk status update request.
/// </summary>
public class BulkContractStatusUpdateDto
{
    public List<int> ContractIds { get; set; } = new();
    public int NewStatus { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for bulk status update result.
/// </summary>
public class BulkContractStatusUpdateResultDto
{
    public int TotalRequested { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkUpdateFailureDto> Failures { get; set; } = new();
}

/// <summary>
/// DTO for bulk update failure details.
/// </summary>
public class BulkUpdateFailureDto
{
    public int ContractId { get; set; }
    public string Error { get; set; } = string.Empty;
}
