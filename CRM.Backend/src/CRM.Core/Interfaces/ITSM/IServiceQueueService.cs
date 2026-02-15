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
