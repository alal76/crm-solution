// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for AI agent usage tracking.
/// </summary>
public interface IAIAgentUsageService
{
    Task<AIAgentUsageDto> CreateAsync(CreateAIAgentUsageDto dto, CancellationToken ct = default);
    Task<AIAgentUsageDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<AIAgentUsageDto>> GetAllAsync(CancellationToken ct = default);
}
