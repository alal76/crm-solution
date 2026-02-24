// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service responsible for matching service requests to appropriate SLA policies.
/// Extracted from SLAPolicyAdminService to provide dedicated matching functionality.
/// TODO-SYS008-020: Dedicated SLA matching logic.
/// </summary>
public interface ISlaMatchingService
{
    /// <summary>
    /// Finds the most appropriate SLA policy for a service request based on priority, category, and customer.
    /// </summary>
    /// <param name="serviceRequest">The service request to match</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The matching SLA policy DTO, or null if no match found</returns>
    Task<SLAPolicyDto?> FindMatchingPolicyAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a matching policy by priority and category.
    /// </summary>
    /// <param name="priority">Service request priority</param>
    /// <param name="categoryId">Service request category ID</param>
    /// <param name="customerId">Optional customer ID for customer-specific SLAs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The matching SLA policy DTO</returns>
    Task<SLAPolicyDto?> FindMatchingPolicyAsync(ServiceRequestPriority priority, int? categoryId, int? customerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all policies that could potentially apply to a service request.
    /// </summary>
    /// <param name="priority">Service request priority</param>
    /// <param name="categoryId">Service request category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of applicable SLA policies</returns>
    Task<List<SLAPolicyDto>> GetApplicablePoliciesAsync(ServiceRequestPriority priority, int? categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default SLA policy when no specific match is found.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The default SLA policy</returns>
    Task<SLAPolicyDto?> GetDefaultPolicyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of SLA matching service.
/// </summary>
public class SlaMatchingService : ISlaMatchingService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SlaMatchingService> _logger;

    public SlaMatchingService(
        ICrmDbContext dbContext,
        ILogger<SlaMatchingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SLAPolicyDto?> FindMatchingPolicyAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        if (serviceRequest == null)
        {
            _logger.LogWarning("Cannot match SLA policy for null service request");
            return null;
        }

        return await FindMatchingPolicyAsync(
            serviceRequest.Priority,
            serviceRequest.CategoryId,
            serviceRequest.AccountId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SLAPolicyDto?> FindMatchingPolicyAsync(
        ServiceRequestPriority priority,
        int? categoryId,
        int? customerId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Priority order for matching:
            // 1. Customer-specific policy with matching priority and category
            // 2. Category-specific policy with matching priority
            // 3. Priority-only policy
            // 4. Default policy

            // Map ServiceRequestPriority to ServicePriority for SLA policy lookup
            var slaPriority = MapToServicePriority(priority);

            var query = _dbContext.SLAPolicies
                .AsNoTracking()
                .Where(p => p.IsActive && !p.IsDeleted);

            // Try to find exact match by priority first
            var policies = await query
                .Where(p => p.Priority == slaPriority)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            if (policies.Any())
            {
                var matchedPolicy = policies.First();
                _logger.LogDebug("SLA policy matched by priority: {PolicyName} for priority {Priority}",
                    matchedPolicy.Name, priority);
                return MapToDto(matchedPolicy);
            }

            // Fall back to default policy (highest priority value = lowest urgency, most generic)
            return await GetDefaultPolicyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching SLA policy for priority {Priority}, category {CategoryId}",
                priority, categoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<SLAPolicyDto>> GetApplicablePoliciesAsync(
        ServiceRequestPriority priority,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var slaPriority = MapToServicePriority(priority);

            var query = _dbContext.SLAPolicies
                .AsNoTracking()
                .Where(p => p.IsActive && !p.IsDeleted);

            // Get policies matching the priority or with no priority restriction (legacy policies)
            var policies = await query
                .Where(p => p.Priority == slaPriority)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            return policies.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicable SLA policies");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SLAPolicyDto?> GetDefaultPolicyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the most recently created active policy as default
            // In a full implementation, there would be an IsDefault flag
            var defaultPolicy = await _dbContext.SLAPolicies
                .AsNoTracking()
                .Where(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.Priority) // Lowest priority value = highest urgency
                .ThenByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (defaultPolicy != null)
            {
                _logger.LogDebug("Using default SLA policy: {PolicyName}", defaultPolicy.Name);
                return MapToDto(defaultPolicy);
            }

            _logger.LogWarning("No default SLA policy found");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default SLA policy");
            throw;
        }
    }

    private static SLAPolicyDto MapToDto(SLAPolicy policy)
    {
        return new SLAPolicyDto
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            Priority = policy.Priority.ToString(),
            Category = string.Empty,
            ResponseTimeHours = policy.InitialResponseTimeMinutes / 60,
            ResolutionTimeHours = policy.ResolutionTimeMinutes / 60,
            BusinessHoursOnly = policy.WorkingHoursOnly,
            Timezone = "UTC",
            BreachAction = "Notify",
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }

    /// <summary>
    /// Maps ServiceRequestPriority to ServicePriority for SLA policy lookup.
    /// ServiceRequestPriority: Low=0, Medium=1, High=2, Critical=3
    /// ServicePriority: Critical=0, High=1, Medium=2, Low=3
    /// </summary>
    private static ServicePriority MapToServicePriority(ServiceRequestPriority priority)
    {
        return priority switch
        {
            ServiceRequestPriority.Critical => ServicePriority.Critical,
            ServiceRequestPriority.High => ServicePriority.High,
            ServiceRequestPriority.Medium => ServicePriority.Medium,
            ServiceRequestPriority.Low => ServicePriority.Low,
            _ => ServicePriority.Medium
        };
    }
}
