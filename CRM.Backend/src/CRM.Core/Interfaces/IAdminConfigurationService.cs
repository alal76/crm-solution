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

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing admin configuration (Sales, Service Desk, etc.)
/// </summary>
public interface IAdminConfigurationService
{
    #region Commission Rules

    /// <summary>
    /// Gets all commission rules
    /// </summary>
    Task<IEnumerable<CommissionRuleDto>> GetCommissionRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific commission rule by ID
    /// </summary>
    Task<CommissionRuleDto?> GetCommissionRuleByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new commission rule
    /// </summary>
    Task<CommissionRuleDto> CreateCommissionRuleAsync(CreateCommissionRuleDto dto, int? createdByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a commission rule
    /// </summary>
    Task<CommissionRuleDto?> UpdateCommissionRuleAsync(int id, UpdateCommissionRuleDto dto, int? modifiedByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a commission rule (soft delete)
    /// </summary>
    Task<bool> DeleteCommissionRuleAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Discount Rules

    /// <summary>
    /// Gets all discount rules
    /// </summary>
    Task<IEnumerable<DiscountRuleDto>> GetDiscountRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific discount rule by ID
    /// </summary>
    Task<DiscountRuleDto?> GetDiscountRuleByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new discount rule
    /// </summary>
    Task<DiscountRuleDto> CreateDiscountRuleAsync(CreateDiscountRuleDto dto, int? createdByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a discount rule
    /// </summary>
    Task<DiscountRuleDto?> UpdateDiscountRuleAsync(int id, UpdateDiscountRuleDto dto, int? modifiedByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a discount rule (soft delete)
    /// </summary>
    Task<bool> DeleteDiscountRuleAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default);

    #endregion

    #region SLA Policies

    /// <summary>
    /// Gets all SLA policies
    /// </summary>
    Task<IEnumerable<SLAPolicyDto>> GetSLAPoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific SLA policy by ID
    /// </summary>
    Task<SLAPolicyDto?> GetSLAPolicyByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new SLA policy
    /// </summary>
    Task<SLAPolicyDto> CreateSLAPolicyAsync(CreateSLAPolicyDto request, int? createdByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an SLA policy
    /// </summary>
    Task<SLAPolicyDto?> UpdateSLAPolicyAsync(int id, UpdateSLAPolicyDto request, int? modifiedByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an SLA policy (soft delete)
    /// </summary>
    Task<bool> DeleteSLAPolicyAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Escalation Rules

    /// <summary>
    /// Gets all escalation rules
    /// </summary>
    Task<IEnumerable<EscalationRuleDto>> GetEscalationRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific escalation rule by ID
    /// </summary>
    Task<EscalationRuleDto?> GetEscalationRuleByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new escalation rule
    /// </summary>
    Task<EscalationRuleDto> CreateEscalationRuleAsync(CreateEscalationRuleDto request, int? createdByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an escalation rule
    /// </summary>
    Task<EscalationRuleDto?> UpdateEscalationRuleAsync(int id, UpdateEscalationRuleDto request, int? modifiedByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an escalation rule (soft delete)
    /// </summary>
    Task<bool> DeleteEscalationRuleAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Service Queues

    /// <summary>
    /// Gets all service queues
    /// </summary>
    Task<IEnumerable<ServiceQueueDto>> GetServiceQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific service queue by ID
    /// </summary>
    Task<ServiceQueueDto?> GetServiceQueueByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new service queue
    /// </summary>
    Task<ServiceQueueDto> CreateServiceQueueAsync(CreateServiceQueueDto request, int? createdByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a service queue
    /// </summary>
    Task<ServiceQueueDto?> UpdateServiceQueueAsync(int id, UpdateServiceQueueDto request, int? modifiedByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a service queue (soft delete)
    /// </summary>
    Task<bool> DeleteServiceQueueAsync(int id, int? deletedByUserId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Configuration Overview

    /// <summary>
    /// Gets complete admin configuration overview
    /// </summary>
    Task<AdminConfigurationDto> GetConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sales admin configuration
    /// </summary>
    Task<SalesAdminConfigDto> GetSalesConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets service desk admin configuration
    /// </summary>
    Task<ServiceDeskAdminConfigDto> GetServiceDeskConfigAsync(CancellationToken cancellationToken = default);

    #endregion
}
