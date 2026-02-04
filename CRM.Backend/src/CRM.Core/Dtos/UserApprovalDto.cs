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

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for user approval requests
/// </summary>
public class UserApprovalRequestDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public int Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserName { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// DTO for approving user registration
/// </summary>
public class ApproveUserRequest
{
    public int ApprovalRequestId { get; set; }
    public string? AssignedRole { get; set; } = "Sales";
    public int? DepartmentId { get; set; }
    public int? UserProfileId { get; set; }
}

/// <summary>
/// DTO for rejecting user registration
/// </summary>
public class RejectUserRequest
{
    public int ApprovalRequestId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}
