// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for export job management.
/// </summary>
public interface IExportJobService
{
    Task<ExportJobDto> CreateAsync(CreateExportJobDto dto, CancellationToken ct = default);
    Task<ExportJobDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ExportJobDto>> GetAllAsync(CancellationToken ct = default);
}
