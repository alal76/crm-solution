// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for service queue response
/// </summary>
public class ServiceQueueDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? AssignmentGroup { get; set; }
    public int? DefaultSLAPolicyId { get; set; }
    public int? MaxQueueDepth { get; set; }
    public int? CurrentQueueDepth { get; set; }
    public decimal? AverageWaitTimeSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating service queue
/// </summary>
public class CreateServiceQueueDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public string? AssignmentGroup { get; set; }
    public int? DefaultSLAPolicyId { get; set; }
    public int? MaxQueueDepth { get; set; }
}

/// <summary>
/// DTO for updating service queue
/// </summary>
public class UpdateServiceQueueDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
    public string? AssignmentGroup { get; set; }
    public int? DefaultSLAPolicyId { get; set; }
    public int? MaxQueueDepth { get; set; }
}

/// <summary>
/// DTO for service request in queue
/// </summary>
public class ServiceRequestQueueItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
}
