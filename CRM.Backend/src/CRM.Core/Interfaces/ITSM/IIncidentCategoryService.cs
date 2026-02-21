// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for managing ITSM incident categories.
/// </summary>
public interface IIncidentCategoryService
{
    Task<IncidentCategoryDto> CreateAsync(CreateIncidentCategoryDto dto, CancellationToken ct = default);
    Task<IncidentCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<IncidentCategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
}
