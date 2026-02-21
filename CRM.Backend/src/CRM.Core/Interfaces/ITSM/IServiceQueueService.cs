// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for managing service queues
/// </summary>
public interface IServiceQueueService
{
    /// <summary>Creates a new service queue</summary>
    Task<ServiceQueueDto> CreateAsync(CreateServiceQueueDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing service queue</summary>
    Task<ServiceQueueDto> UpdateAsync(int id, UpdateServiceQueueDto dto, CancellationToken ct = default);

    /// <summary>Gets a service queue by ID</summary>
    Task<ServiceQueueDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Gets all service queues</summary>
    Task<List<ServiceQueueDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Deletes a service queue (soft delete)</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Assigns a service request to a queue</summary>
    Task AssignToQueueAsync(int serviceRequestId, int queueId, CancellationToken ct = default);

    /// <summary>Gets all service requests in a queue</summary>
    Task<List<ServiceRequestQueueItemDto>> GetQueueItemsAsync(int queueId, CancellationToken ct = default);

    /// <summary>Gets queue statistics</summary>
    Task<ServiceQueueDto> GetQueueStatsAsync(int queueId, CancellationToken ct = default);
}
