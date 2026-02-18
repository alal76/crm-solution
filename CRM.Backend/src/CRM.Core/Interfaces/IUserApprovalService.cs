// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for user approval workflow
/// </summary>
public interface IUserApprovalService
{
    Task<IEnumerable<UserApprovalRequestDto>> GetApprovalRequestsAsync(int? status = null);
    Task<UserApprovalRequestDto?> GetApprovalRequestByIdAsync(int id);
    Task CreateApprovalRequestAsync(string email, string firstName, string lastName, string? company = null, string? phone = null);
    Task<UserDto> ApproveUserAsync(int approvalRequestId, int reviewedByUserId, ApproveUserRequest request);
    Task RejectUserAsync(int approvalRequestId, int reviewedByUserId, string rejectionReason);
}
