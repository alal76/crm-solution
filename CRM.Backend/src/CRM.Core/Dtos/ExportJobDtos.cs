// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for export job response.
/// </summary>
public class ExportJobDto
{
    public int Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? RequestedByUserId { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? TotalRecords { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating an export job.
/// </summary>
public class CreateExportJobDto
{
    public string Entity { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? Status { get; set; }
    public int? RequestedByUserId { get; set; }
    public string? RequestedDate { get; set; }
}
