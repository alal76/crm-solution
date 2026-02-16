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

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for change request details.
/// </summary>
public class ChangeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
}

/// <summary>
/// DTO for creating a new change request.
/// </summary>
public class CreateChangeDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(255, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    public int? AssignedToId { get; set; }
}

/// <summary>
/// DTO for updating a change request.
/// </summary>
public class UpdateChangeDto
{
    [StringLength(255, MinimumLength = 1)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 5)]
    public int? Priority { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public int? AssignedToId { get; set; }
}

/// <summary>
/// DTO for change approval.
/// </summary>
public class ChangeApprovalDto
{
    public string? ApproverNotes { get; set; }
}

/// <summary>
/// DTO for change rejection.
/// </summary>
public class ChangeRejectionDto
{
    public string? RejectionReason { get; set; }
}
