// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for managing ITSM service queues
/// </summary>
public class ServiceQueueService : IServiceQueueService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ServiceQueueService> _logger;

    public ServiceQueueService(
        ICrmDbContext dbContext,
        ILogger<ServiceQueueService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ServiceQueueDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var queue = await _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, ct);

            return queue != null ? MapToDto(queue) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service queue {Id}", id);
            throw;
        }
    }

    public async Task<List<ServiceQueueDto>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var queues = await _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>()
                .AsNoTracking()
                .Where(q => !q.IsDeleted)
                .OrderBy(q => q.Name)
                .ToListAsync(ct);

            return queues.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all service queues");
            throw;
        }
    }

    public async Task<ServiceQueueDto> CreateAsync(CreateServiceQueueDto dto, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Queue name is required");

            var queue = new CRM.Core.Entities.ITSM.ServiceQueue
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>().Add(queue);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Service queue created: {QueueName} (ID: {QueueId})", queue.Name, queue.Id);

            return MapToDto(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service queue");
            throw;
        }
    }

    public async Task<ServiceQueueDto> UpdateAsync(int id, UpdateServiceQueueDto dto, CancellationToken ct = default)
    {
        try
        {
            var queue = await _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>()
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, ct);

            if (queue == null)
                throw new KeyNotFoundException($"Service queue with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                queue.Name = dto.Name;

            if (dto.Description != null)
                queue.Description = dto.Description;

            if (dto.IsActive.HasValue)
                queue.IsActive = dto.IsActive.Value;

            queue.UpdatedAt = DateTime.UtcNow;

            _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>().Update(queue);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Service queue updated: {QueueName} (ID: {QueueId})", queue.Name, queue.Id);

            return MapToDto(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service queue {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var queue = await _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>()
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, ct);

            if (queue == null)
                throw new KeyNotFoundException($"Service queue with ID {id} not found");

            queue.IsDeleted = true;
            queue.UpdatedAt = DateTime.UtcNow;

            _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>().Update(queue);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Service queue deleted: {QueueId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service queue {Id}", id);
            throw;
        }
    }

    public async Task AssignToQueueAsync(int serviceRequestId, int queueId, CancellationToken ct = default)
    {
        try
        {
            var queue = await _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>()
                .FirstOrDefaultAsync(q => q.Id == queueId && !q.IsDeleted, ct);

            if (queue == null)
                throw new KeyNotFoundException($"Service queue with ID {queueId} not found");

            var request = await _dbContext.ServiceRequests
                .FirstOrDefaultAsync(r => r.Id == serviceRequestId && !r.IsDeleted, ct);

            if (request == null)
                throw new KeyNotFoundException($"Service request with ID {serviceRequestId} not found");

            _logger.LogInformation("Service request {ServiceRequestId} assigned to queue {QueueId}",
                serviceRequestId, queueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning service request to queue");
            throw;
        }
    }

    public async Task<List<ServiceRequestQueueItemDto>> GetQueueItemsAsync(int queueId, CancellationToken ct = default)
    {
        try
        {
            var items = await _dbContext.ServiceRequests
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Select(r => new ServiceRequestQueueItemDto
                {
                    Id = r.Id,
                    Title = r.Subject,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(ct);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue items for queue {QueueId}", queueId);
            throw;
        }
    }

    public async Task<ServiceQueueDto> GetQueueStatsAsync(int queueId, CancellationToken ct = default)
    {
        try
        {
            var queue = await _dbContext.Set<CRM.Core.Entities.ITSM.ServiceQueue>()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == queueId && !q.IsDeleted, ct);

            if (queue == null)
                throw new KeyNotFoundException($"Service queue with ID {queueId} not found");

            return MapToDto(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue stats for queue {QueueId}", queueId);
            throw;
        }
    }

    private ServiceQueueDto MapToDto(CRM.Core.Entities.ITSM.ServiceQueue queue)
    {
        return new ServiceQueueDto
        {
            Id = queue.Id,
            Name = queue.Name,
            Description = queue.Description,
            IsActive = queue.IsActive,
            CreatedAt = queue.CreatedAt,
            UpdatedAt = queue.UpdatedAt
        };
    }
}
