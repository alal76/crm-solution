// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// CRUD service for <see cref="CRM.Core.Entities.DunningSchedule"/> steps.
///
/// Dunning schedule steps configure automated email reminders sent when invoices
/// become overdue.  Each step fires when the invoice is overdue by
/// <c>DaysOverdue</c> days; steps are processed in ascending <c>StepOrder</c>.
///
/// BACK-010: Dunning Scheduler CRUD
/// </summary>
public interface IDunningScheduleService
{
    /// <summary>
    /// Returns all dunning schedule steps ordered by <c>StepOrder</c>.
    /// </summary>
    /// <param name="activeOnly">When <see langword="true"/> returns only active steps; when <see langword="false"/>  returns only inactive; when <see langword="null"/> returns all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<DunningScheduleDto>> GetAllAsync(
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single dunning schedule step by its primary key.
    /// Returns <see langword="null"/> when not found.
    /// </summary>
    Task<DunningScheduleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new dunning schedule step.</summary>
    /// <param name="dto">Creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created step as a DTO.</returns>
    Task<DunningScheduleDto> CreateAsync(CreateDunningScheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update to an existing dunning schedule step.
    /// </summary>
    /// <param name="id">ID of the step to update.</param>
    /// <param name="dto">Fields to update; <see langword="null"/> properties are skipped.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated step as a DTO.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the step is not found.</exception>
    Task<DunningScheduleDto> UpdateAsync(int id, UpdateDunningScheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a dunning schedule step.
    /// </summary>
    /// <param name="id">ID of the step to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when deleted; <see langword="false"/> when not found.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
