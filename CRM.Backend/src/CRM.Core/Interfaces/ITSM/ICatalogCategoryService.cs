// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for managing ITSM catalog categories.
/// </summary>
public interface ICatalogCategoryService
{
    Task<CatalogCategoryDto> CreateAsync(CreateCatalogCategoryDto dto, CancellationToken ct = default);
    Task<CatalogCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<CatalogCategoryDto>> GetAllAsync(CancellationToken ct = default);
}
