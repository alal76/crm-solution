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

using CRM.Core.Entities;

namespace CRM.Core.Dtos;

#region Category DTOs

public class ServiceRequestCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? IconName { get; set; }
    public string? ColorCode { get; set; }
    public int? DefaultResponseTimeHours { get; set; }
    public int? DefaultResolutionTimeHours { get; set; }
    public int SubcategoryCount { get; set; }
    public int ServiceRequestCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateServiceRequestCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? IconName { get; set; }
    public string? ColorCode { get; set; }
    public int? DefaultResponseTimeHours { get; set; }
    public int? DefaultResolutionTimeHours { get; set; }
}

public class UpdateServiceRequestCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? IconName { get; set; }
    public string? ColorCode { get; set; }
    public int? DefaultResponseTimeHours { get; set; }
    public int? DefaultResolutionTimeHours { get; set; }
}

#endregion

#region Subcategory DTOs

public class ServiceRequestSubcategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public ServiceRequestPriority? DefaultPriority { get; set; }
    public int? DefaultWorkflowId { get; set; }
    public string? DefaultWorkflowName { get; set; }
    public int ServiceRequestCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateServiceRequestSubcategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public ServiceRequestPriority? DefaultPriority { get; set; }
    public int? DefaultWorkflowId { get; set; }
}

public class UpdateServiceRequestSubcategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public ServiceRequestPriority? DefaultPriority { get; set; }
    public int? DefaultWorkflowId { get; set; }
}

#endregion

#region Custom Field Definition DTOs

public class ServiceRequestCustomFieldDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DropdownOptions { get; set; }
    public List<string>? DropdownOptionsList { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateServiceRequestCustomFieldDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public List<string>? DropdownOptions { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
}

public class UpdateServiceRequestCustomFieldDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public List<string>? DropdownOptions { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
}

#endregion

#region Custom Field Value DTOs

public class ServiceRequestCustomFieldValueDto
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public int CustomFieldDefinitionId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
    public object? DisplayValue { get; set; }
}

public class SetCustomFieldValueDto
{
    public int CustomFieldDefinitionId { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
}

#endregion

#region Service Request DTOs

public class ServiceRequestDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Channel & Status
    public ServiceRequestChannel Channel { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public ServiceRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public ServiceRequestPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    
    // Categorization
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    
    // Customer & Contact
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
    public string? RequesterPhone { get; set; }
    
    // Assignment
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int? AssignedToGroupId { get; set; }
    public string? AssignedToGroupName { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    
    // Workflow
    public int? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public string? CurrentWorkflowStep { get; set; }
    
    // SLA
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public DateTime? FirstResponseDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public bool ResponseSlaBreached { get; set; }
    public bool ResolutionSlaBreached { get; set; }
    
    // Channel-specific
    public string? ExternalReferenceId { get; set; }
    public string? SourcePhoneNumber { get; set; }
    public string? SourceEmailAddress { get; set; }
    
    // Resolution
    public string? ResolutionSummary { get; set; }
    public string? ResolutionCode { get; set; }
    public string? RootCause { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? CustomerFeedback { get; set; }
    
    // Related entities
    public int? RelatedOpportunityId { get; set; }
    public string? RelatedOpportunityName { get; set; }
    public int? RelatedProductId { get; set; }
    public string? RelatedProductName { get; set; }
    public int? ParentServiceRequestId { get; set; }
    public string? ParentTicketNumber { get; set; }
    
    // Additional
    public string? Tags { get; set; }
    public string? InternalNotes { get; set; }
    public int EscalationLevel { get; set; }
    public int ReopenCount { get; set; }
    public bool IsVipCustomer { get; set; }
    public decimal? EstimatedEffortHours { get; set; }
    public decimal? ActualEffortHours { get; set; }
    
    // Computed
    public bool IsOpen { get; set; }
    public double AgeInHours { get; set; }
    public double? TimeToFirstResponseHours { get; set; }
    public double? TimeToResolutionHours { get; set; }
    public bool IsResponseSlaAtRisk { get; set; }
    public bool IsResolutionSlaAtRisk { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Custom fields
    public List<ServiceRequestCustomFieldValueDto> CustomFieldValues { get; set; } = new();
    
    // Child requests count
    public int ChildRequestCount { get; set; }
}

public class CreateServiceRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ServiceRequestChannel Channel { get; set; } = ServiceRequestChannel.SelfServicePortal;
    public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;
    
    // Categorization
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    
    // Customer & Contact
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
    public string? RequesterPhone { get; set; }
    
    // Assignment
    public int? AssignedToUserId { get; set; }
    public int? AssignedToGroupId { get; set; }
    
    // Workflow
    public int? WorkflowId { get; set; }
    
    // Channel-specific
    public string? ExternalReferenceId { get; set; }
    public string? SourcePhoneNumber { get; set; }
    public string? SourceEmailAddress { get; set; }
    public string? ConversationId { get; set; }
    
    // Related entities
    public int? RelatedOpportunityId { get; set; }
    public int? RelatedProductId { get; set; }
    public int? ParentServiceRequestId { get; set; }
    
    // Additional
    public string? Tags { get; set; }
    public string? InternalNotes { get; set; }
    public bool IsVipCustomer { get; set; }
    public decimal? EstimatedEffortHours { get; set; }
    
    // Custom field values
    public List<SetCustomFieldValueDto>? CustomFieldValues { get; set; }
}

public class UpdateServiceRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ServiceRequestChannel Channel { get; set; }
    public ServiceRequestStatus Status { get; set; }
    public ServiceRequestPriority Priority { get; set; }
    
    // Categorization
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    
    // Customer & Contact
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
    public string? RequesterPhone { get; set; }
    
    // Assignment
    public int? AssignedToUserId { get; set; }
    public int? AssignedToGroupId { get; set; }
    
    // Workflow
    public int? WorkflowId { get; set; }
    public string? CurrentWorkflowStep { get; set; }
    
    // SLA
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    
    // Resolution
    public string? ResolutionSummary { get; set; }
    public string? ResolutionCode { get; set; }
    public string? RootCause { get; set; }
    
    // Related entities
    public int? RelatedOpportunityId { get; set; }
    public int? RelatedProductId { get; set; }
    public int? ParentServiceRequestId { get; set; }
    
    // Additional
    public string? Tags { get; set; }
    public string? InternalNotes { get; set; }
    public bool IsVipCustomer { get; set; }
    public decimal? EstimatedEffortHours { get; set; }
    public decimal? ActualEffortHours { get; set; }
    
    // Custom field values
    public List<SetCustomFieldValueDto>? CustomFieldValues { get; set; }
}

#endregion

#region Filter & Search DTOs

public class ServiceRequestFilterDto
{
    public string? SearchTerm { get; set; }
    public List<ServiceRequestStatus>? Statuses { get; set; }
    public List<ServiceRequestPriority>? Priorities { get; set; }
    public List<ServiceRequestChannel>? Channels { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? SubcategoryIds { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? AssignedToGroupId { get; set; }
    public int? WorkflowId { get; set; }
    public bool? IsOpen { get; set; }
    public bool? IsVipCustomer { get; set; }
    public bool? ResponseSlaBreached { get; set; }
    public bool? ResolutionSlaBreached { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? ResolvedFrom { get; set; }
    public DateTime? ResolvedTo { get; set; }
    public string? Tags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class ServiceRequestListDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public ServiceRequestChannel Channel { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public ServiceRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public ServiceRequestPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
    public string? CustomerName { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public bool ResponseSlaBreached { get; set; }
    public bool ResolutionSlaBreached { get; set; }
    public bool IsVipCustomer { get; set; }
    public int EscalationLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PagedServiceRequestResult
{
    public List<ServiceRequestListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

#endregion

#region Dashboard & Statistics DTOs

public class ServiceRequestStatisticsDto
{
    public int TotalRequests { get; set; }
    public int OpenRequests { get; set; }
    public int NewRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int PendingRequests { get; set; }
    public int EscalatedRequests { get; set; }
    public int ResolvedToday { get; set; }
    public int CreatedToday { get; set; }
    public int SlaBreachedCount { get; set; }
    public int SlaAtRiskCount { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double AverageFirstResponseTimeHours { get; set; }
    public double CustomerSatisfactionAverage { get; set; }
    
    public Dictionary<string, int> ByChannel { get; set; } = new();
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByAssignee { get; set; } = new();
}

#endregion
