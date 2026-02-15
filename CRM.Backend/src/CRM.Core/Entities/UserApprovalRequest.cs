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

namespace CRM.Core.Entities;

/// <summary>
/// Approval status for new user registrations
/// </summary>
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// User approval request for managing new user sign-ups
/// </summary>
public class UserApprovalRequest : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; } // Store password hash for use when approved
    public int Status { get; set; } = (int)ApprovalStatus.Pending;

    /// <summary>Whether this request has been approved (computed from Status)</summary>
    public bool IsApproved => Status == (int)ApprovalStatus.Approved;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public int? AssignedUserId { get; set; } // User created after approval

    // Navigation properties
    public virtual User? ReviewedByUser { get; set; }
    public virtual User? AssignedUser { get; set; }
}
