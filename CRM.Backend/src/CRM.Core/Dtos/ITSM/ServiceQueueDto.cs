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
