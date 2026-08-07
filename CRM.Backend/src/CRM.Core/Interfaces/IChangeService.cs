// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing change requests.
/// </summary>
/// <remarks>
/// Superseded by <see cref="CRM.Core.Interfaces.ITSM.IChangeManagementServiceEx"/>, which
/// implements the full ITSM change lifecycle (CAB, approvals, scheduling, implementation,
/// rollback, conflicts, blackout periods, comments, metrics) against the same <c>Changes</c>
/// table. <c>ChangesController</c> now depends on the Ex service.
/// Kept as a working shim rather than deleted; do not wire this into any controller.
/// </remarks>
[Obsolete("Superseded by IChangeManagementServiceEx, which implements the full ITSM change lifecycle. ChangesController now uses IChangeManagementServiceEx. Do not use in new code.")]
public interface IChangeService
{
    /// <summary>
    /// Get all changes with pagination.
    /// </summary>
    Task<PaginatedDto<ChangeDto>> GetAllAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a change by ID.
    /// </summary>
    Task<ChangeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new change request.
    /// </summary>
    Task<ChangeDto> CreateAsync(CreateChangeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing change request.
    /// </summary>
    Task<ChangeDto> UpdateAsync(int id, UpdateChangeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a change request.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a change request for approval.
    /// </summary>
    Task<ChangeDto> SubmitAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a change request.
    /// </summary>
    Task<ChangeDto> ApproveAsync(int id, ChangeApprovalDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject a change request.
    /// </summary>
    Task<ChangeDto> RejectAsync(int id, ChangeRejectionDto dto, CancellationToken cancellationToken = default);
}
