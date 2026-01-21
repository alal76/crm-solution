/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing service requests
/// </summary>
public interface IServiceRequestService
{
    #region Service Request Operations
    
    /// <summary>Get all service requests with optional filtering</summary>
    Task<PagedServiceRequestResult> GetServiceRequestsAsync(ServiceRequestFilterDto filter);
    
    /// <summary>Get a service request by ID</summary>
    Task<ServiceRequestDto?> GetServiceRequestByIdAsync(int id);
    
    /// <summary>Get a service request by ticket number</summary>
    Task<ServiceRequestDto?> GetServiceRequestByTicketNumberAsync(string ticketNumber);
    
    /// <summary>Create a new service request</summary>
    Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, int? createdByUserId);
    
    /// <summary>Update an existing service request</summary>
    Task<ServiceRequestDto> UpdateServiceRequestAsync(int id, UpdateServiceRequestDto dto, int? modifiedByUserId);
    
    /// <summary>Delete a service request (soft delete)</summary>
    Task<bool> DeleteServiceRequestAsync(int id);
    
    /// <summary>Get service requests by customer</summary>
    Task<List<ServiceRequestListDto>> GetServiceRequestsByCustomerAsync(int customerId);
    
    /// <summary>Get service requests by contact</summary>
    Task<List<ServiceRequestListDto>> GetServiceRequestsByContactAsync(int contactId);
    
    /// <summary>Get service requests assigned to a user</summary>
    Task<List<ServiceRequestListDto>> GetServiceRequestsByAssigneeAsync(int userId);
    
    /// <summary>Get service requests assigned to a group</summary>
    Task<List<ServiceRequestListDto>> GetServiceRequestsByGroupAsync(int groupId);
    
    #endregion
    
    #region Status Operations
    
    /// <summary>Update the status of a service request</summary>
    Task<ServiceRequestDto> UpdateStatusAsync(int id, ServiceRequestStatus newStatus, int? modifiedByUserId);
    
    /// <summary>Mark first response on a service request</summary>
    Task<ServiceRequestDto> MarkFirstResponseAsync(int id, int? userId);
    
    /// <summary>Resolve a service request</summary>
    Task<ServiceRequestDto> ResolveServiceRequestAsync(int id, string resolutionSummary, string? resolutionCode, string? rootCause, int? resolvedByUserId);
    
    /// <summary>Close a service request</summary>
    Task<ServiceRequestDto> CloseServiceRequestAsync(int id, int? closedByUserId);
    
    /// <summary>Reopen a service request</summary>
    Task<ServiceRequestDto> ReopenServiceRequestAsync(int id, string reason, int? reopenedByUserId);
    
    /// <summary>Escalate a service request</summary>
    Task<ServiceRequestDto> EscalateServiceRequestAsync(int id, string reason, int? escalatedByUserId);
    
    #endregion
    
    #region Assignment Operations
    
    /// <summary>Assign a service request to a user</summary>
    Task<ServiceRequestDto> AssignToUserAsync(int id, int userId, int? assignedByUserId);
    
    /// <summary>Assign a service request to a group</summary>
    Task<ServiceRequestDto> AssignToGroupAsync(int id, int groupId, int? assignedByUserId);
    
    /// <summary>Unassign a service request</summary>
    Task<ServiceRequestDto> UnassignAsync(int id, int? modifiedByUserId);
    
    #endregion
    
    #region Custom Field Operations
    
    /// <summary>Set custom field values for a service request</summary>
    Task SetCustomFieldValuesAsync(int serviceRequestId, List<SetCustomFieldValueDto> values);
    
    /// <summary>Get custom field values for a service request</summary>
    Task<List<ServiceRequestCustomFieldValueDto>> GetCustomFieldValuesAsync(int serviceRequestId);
    
    #endregion
    
    #region Feedback Operations
    
    /// <summary>Submit customer satisfaction feedback</summary>
    Task<ServiceRequestDto> SubmitFeedbackAsync(int id, int rating, string? feedback);
    
    #endregion
    
    #region Statistics
    
    /// <summary>Get service request statistics</summary>
    Task<ServiceRequestStatisticsDto> GetStatisticsAsync();
    
    /// <summary>Get open requests count</summary>
    Task<int> GetOpenRequestsCountAsync();
    
    /// <summary>Get SLA breached requests count</summary>
    Task<int> GetSlaBreachedCountAsync();
    
    #endregion
}

/// <summary>
/// Service interface for managing service request categories
/// </summary>
public interface IServiceRequestCategoryService
{
    Task<List<ServiceRequestCategoryDto>> GetAllCategoriesAsync(bool includeInactive = false);
    Task<ServiceRequestCategoryDto?> GetCategoryByIdAsync(int id);
    Task<ServiceRequestCategoryDto> CreateCategoryAsync(CreateServiceRequestCategoryDto dto);
    Task<ServiceRequestCategoryDto> UpdateCategoryAsync(int id, UpdateServiceRequestCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> ReorderCategoriesAsync(List<int> categoryIds);
}

/// <summary>
/// Service interface for managing service request subcategories
/// </summary>
public interface IServiceRequestSubcategoryService
{
    Task<List<ServiceRequestSubcategoryDto>> GetAllSubcategoriesAsync(bool includeInactive = false);
    Task<List<ServiceRequestSubcategoryDto>> GetSubcategoriesByCategoryAsync(int categoryId, bool includeInactive = false);
    Task<ServiceRequestSubcategoryDto?> GetSubcategoryByIdAsync(int id);
    Task<ServiceRequestSubcategoryDto> CreateSubcategoryAsync(CreateServiceRequestSubcategoryDto dto);
    Task<ServiceRequestSubcategoryDto> UpdateSubcategoryAsync(int id, UpdateServiceRequestSubcategoryDto dto);
    Task<bool> DeleteSubcategoryAsync(int id);
    Task<bool> ReorderSubcategoriesAsync(int categoryId, List<int> subcategoryIds);
}

/// <summary>
/// Service interface for managing service request custom field definitions
/// </summary>
public interface IServiceRequestCustomFieldService
{
    Task<List<ServiceRequestCustomFieldDefinitionDto>> GetAllFieldDefinitionsAsync(bool includeInactive = false);
    Task<List<ServiceRequestCustomFieldDefinitionDto>> GetFieldDefinitionsByCategoryAsync(int? categoryId, int? subcategoryId);
    Task<ServiceRequestCustomFieldDefinitionDto?> GetFieldDefinitionByIdAsync(int id);
    Task<ServiceRequestCustomFieldDefinitionDto> CreateFieldDefinitionAsync(CreateServiceRequestCustomFieldDefinitionDto dto);
    Task<ServiceRequestCustomFieldDefinitionDto> UpdateFieldDefinitionAsync(int id, UpdateServiceRequestCustomFieldDefinitionDto dto);
    Task<bool> DeleteFieldDefinitionAsync(int id);
    Task<bool> ReorderFieldDefinitionsAsync(List<int> fieldIds);
    Task<int> GetActiveFieldCountAsync();
}
