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

using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing service requests.
/// 
/// HEXAGONAL ARCHITECTURE:
/// - Implements IServiceRequestInputPort (primary/driving port)
/// - Implements IServiceRequestService (backward compatibility)
/// - Uses ICrmDbContext (secondary/driven port)
/// </summary>
public class ServiceRequestService : IServiceRequestService, IServiceRequestInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ServiceRequestService> _logger;
    private static int _ticketCounter = 0;
    private static readonly object _ticketLock = new();
    private readonly NormalizationService _normalizationService;

    public ServiceRequestService(ICrmDbContext context, ILogger<ServiceRequestService> logger, NormalizationService normalizationService)
    {
        _context = context;
        _logger = logger;
        _normalizationService = normalizationService;
    }

    #region Helper Methods

    private string GenerateTicketNumber()
    {
        lock (_ticketLock)
        {
            _ticketCounter++;
            return $"SR-{DateTime.UtcNow:yyyyMMdd}-{_ticketCounter:D5}";
        }
    }

    private static string GetChannelName(ServiceRequestChannel channel) => channel switch
    {
        ServiceRequestChannel.WhatsApp => "WhatsApp",
        ServiceRequestChannel.Email => "Email",
        ServiceRequestChannel.Phone => "Phone",
        ServiceRequestChannel.InPerson => "In Person",
        ServiceRequestChannel.SelfServicePortal => "Self Service Portal",
        ServiceRequestChannel.SocialMedia => "Social Media",
        ServiceRequestChannel.LiveChat => "Live Chat",
        ServiceRequestChannel.API => "API",
        _ => channel.ToString()
    };

    private static string GetStatusName(ServiceRequestStatus status) => status switch
    {
        ServiceRequestStatus.New => "New",
        ServiceRequestStatus.Open => "Open",
        ServiceRequestStatus.InProgress => "In Progress",
        ServiceRequestStatus.PendingCustomer => "Pending Customer",
        ServiceRequestStatus.PendingInternal => "Pending Internal",
        ServiceRequestStatus.Escalated => "Escalated",
        ServiceRequestStatus.Resolved => "Resolved",
        ServiceRequestStatus.Closed => "Closed",
        ServiceRequestStatus.Cancelled => "Cancelled",
        ServiceRequestStatus.OnHold => "On Hold",
        ServiceRequestStatus.Reopened => "Reopened",
        _ => status.ToString()
    };

    private static string GetPriorityName(ServiceRequestPriority priority) => priority switch
    {
        ServiceRequestPriority.Low => "Low",
        ServiceRequestPriority.Medium => "Medium",
        ServiceRequestPriority.High => "High",
        ServiceRequestPriority.Critical => "Critical",
        ServiceRequestPriority.Urgent => "Urgent",
        _ => priority.ToString()
    };

    private async Task<ServiceRequestDto> MapToDto(ServiceRequest entity)
    {
        var dto = new ServiceRequestDto
        {
            Id = entity.Id,
            TicketNumber = entity.TicketNumber,
            Subject = entity.Subject,
            Description = entity.Description,
            Channel = entity.Channel,
            ChannelName = GetChannelName(entity.Channel),
            Status = entity.Status,
            StatusName = GetStatusName(entity.Status),
            Priority = entity.Priority,
            PriorityName = GetPriorityName(entity.Priority),
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name,
            SubcategoryId = entity.SubcategoryId,
            SubcategoryName = entity.Subcategory?.Name,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer != null ? $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim() : null,
            ContactId = entity.ContactId,
            ContactName = entity.Contact != null ? $"{entity.Contact.FirstName} {entity.Contact.LastName}".Trim() : null,
            RequesterName = entity.RequesterName,
            RequesterEmail = entity.RequesterEmail,
            RequesterPhone = entity.RequesterPhone,
            AssignedToUserId = entity.AssignedToUserId,
            AssignedToUserName = entity.AssignedToUser != null ? $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim() : null,
            AssignedToGroupId = entity.AssignedToGroupId,
            AssignedToGroupName = entity.AssignedToGroup?.Name,
            CreatedByUserId = entity.CreatedByUserId,
            CreatedByUserName = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}".Trim() : null,
            WorkflowId = entity.WorkflowId,
            WorkflowName = entity.Workflow?.Name,
            CurrentWorkflowStep = entity.CurrentWorkflowStep,
            ResponseDueDate = entity.ResponseDueDate,
            ResolutionDueDate = entity.ResolutionDueDate,
            FirstResponseDate = entity.FirstResponseDate,
            ResolvedDate = entity.ResolvedDate,
            ClosedDate = entity.ClosedDate,
            ResponseSlaBreached = entity.ResponseSlaBreached,
            ResolutionSlaBreached = entity.ResolutionSlaBreached,
            ExternalReferenceId = entity.ExternalReferenceId,
            SourcePhoneNumber = entity.SourcePhoneNumber,
            SourceEmailAddress = entity.SourceEmailAddress,
            ResolutionSummary = entity.ResolutionSummary,
            ResolutionCode = entity.ResolutionCode,
            RootCause = entity.RootCause,
            SatisfactionRating = entity.SatisfactionRating,
            CustomerFeedback = entity.CustomerFeedback,
            RelatedOpportunityId = entity.RelatedOpportunityId,
            RelatedOpportunityName = entity.RelatedOpportunity?.Name,
            RelatedProductId = entity.RelatedProductId,
            RelatedProductName = entity.RelatedProduct?.Name,
            ParentServiceRequestId = entity.ParentServiceRequestId,
            ParentTicketNumber = entity.ParentServiceRequest?.TicketNumber,
                 // Prefer normalized tags stored in EntityTags when available
                 Tags = await _normalizationService.GetTagsAsync("ServiceRequest", entity.Id) ?? entity.Tags,
            InternalNotes = entity.InternalNotes,
            EscalationLevel = entity.EscalationLevel,
            ReopenCount = entity.ReopenCount,
            IsVipCustomer = entity.IsVipCustomer,
            EstimatedEffortHours = entity.EstimatedEffortHours,
            ActualEffortHours = entity.ActualEffortHours,
            IsOpen = entity.IsOpen,
            AgeInHours = entity.AgeInHours,
            TimeToFirstResponseHours = entity.TimeToFirstResponseHours,
            TimeToResolutionHours = entity.TimeToResolutionHours,
            IsResponseSlaAtRisk = entity.IsResponseSlaAtRisk,
            IsResolutionSlaAtRisk = entity.IsResolutionSlaAtRisk,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ChildRequestCount = entity.ChildServiceRequests?.Count ?? 0
        };

        // Get custom field values (service-specific table)
        dto.CustomFieldValues = await GetCustomFieldValuesAsync(entity.Id);

        return dto;
    }

    private ServiceRequestListDto MapToListDto(ServiceRequest entity) => new()
    {
        Id = entity.Id,
        TicketNumber = entity.TicketNumber,
        Subject = entity.Subject,
        Channel = entity.Channel,
        ChannelName = GetChannelName(entity.Channel),
        Status = entity.Status,
        StatusName = GetStatusName(entity.Status),
        Priority = entity.Priority,
        PriorityName = GetPriorityName(entity.Priority),
        CategoryName = entity.Category?.Name,
        SubcategoryName = entity.Subcategory?.Name,
        CustomerName = entity.Customer != null ? $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim() : entity.RequesterName,
        AssignedToUserName = entity.AssignedToUser != null ? $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim() : null,
        ResponseDueDate = entity.ResponseDueDate,
        ResolutionDueDate = entity.ResolutionDueDate,
        ResponseSlaBreached = entity.ResponseSlaBreached,
        ResolutionSlaBreached = entity.ResolutionSlaBreached,
        IsVipCustomer = entity.IsVipCustomer,
        EscalationLevel = entity.EscalationLevel,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private void CalculateSlaDeadlines(ServiceRequest request)
    {
        int? responseHours = null;
        int? resolutionHours = null;

        // Get SLA from subcategory first, then category
        if (request.Subcategory != null)
        {
            responseHours = request.Subcategory.ResponseTimeHours;
            resolutionHours = request.Subcategory.ResolutionTimeHours;
        }

        if (!responseHours.HasValue && request.Category != null)
        {
            responseHours = request.Category.DefaultResponseTimeHours;
        }
        if (!resolutionHours.HasValue && request.Category != null)
        {
            resolutionHours = request.Category.DefaultResolutionTimeHours;
        }

        // Default SLAs if not configured
        responseHours ??= 4; // 4 hours for first response
        resolutionHours ??= 24; // 24 hours for resolution

        // Adjust based on priority
        var priorityMultiplier = request.Priority switch
        {
            ServiceRequestPriority.Critical => 0.25,
            ServiceRequestPriority.Urgent => 0.5,
            ServiceRequestPriority.High => 0.75,
            ServiceRequestPriority.Medium => 1.0,
            ServiceRequestPriority.Low => 1.5,
            _ => 1.0
        };

        request.ResponseDueDate = request.CreatedAt.AddHours(responseHours.Value * priorityMultiplier);
        request.ResolutionDueDate = request.CreatedAt.AddHours(resolutionHours.Value * priorityMultiplier);
    }

    #endregion

    #region Service Request Operations

    public async Task<PagedServiceRequestResult> GetServiceRequestsAsync(ServiceRequestFilterDto filter)
    {
        var query = _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.Customer)
            .Include(sr => sr.Contact)
            .Include(sr => sr.AssignedToUser)
            .Include(sr => sr.AssignedToGroup)
            .Where(sr => !sr.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.ToLower();
            query = query.Where(sr => 
                sr.TicketNumber.ToLower().Contains(search) ||
                sr.Subject.ToLower().Contains(search) ||
                (sr.Description != null && sr.Description.ToLower().Contains(search)) ||
                (sr.RequesterName != null && sr.RequesterName.ToLower().Contains(search)) ||
                (sr.RequesterEmail != null && sr.RequesterEmail.ToLower().Contains(search)));
        }

        if (filter.Statuses?.Any() == true)
            query = query.Where(sr => filter.Statuses.Contains(sr.Status));

        if (filter.Priorities?.Any() == true)
            query = query.Where(sr => filter.Priorities.Contains(sr.Priority));

        if (filter.Channels?.Any() == true)
            query = query.Where(sr => filter.Channels.Contains(sr.Channel));

        if (filter.CategoryIds?.Any() == true)
            query = query.Where(sr => sr.CategoryId.HasValue && filter.CategoryIds.Contains(sr.CategoryId.Value));

        if (filter.SubcategoryIds?.Any() == true)
            query = query.Where(sr => sr.SubcategoryId.HasValue && filter.SubcategoryIds.Contains(sr.SubcategoryId.Value));

        if (filter.CustomerId.HasValue)
            query = query.Where(sr => sr.CustomerId == filter.CustomerId);

        if (filter.ContactId.HasValue)
            query = query.Where(sr => sr.ContactId == filter.ContactId);

        if (filter.AssignedToUserId.HasValue)
            query = query.Where(sr => sr.AssignedToUserId == filter.AssignedToUserId);

        if (filter.AssignedToGroupId.HasValue)
            query = query.Where(sr => sr.AssignedToGroupId == filter.AssignedToGroupId);

        if (filter.WorkflowId.HasValue)
            query = query.Where(sr => sr.WorkflowId == filter.WorkflowId);

        if (filter.IsOpen.HasValue)
        {
            if (filter.IsOpen.Value)
                query = query.Where(sr => sr.Status != ServiceRequestStatus.Closed && 
                                          sr.Status != ServiceRequestStatus.Cancelled && 
                                          sr.Status != ServiceRequestStatus.Resolved);
            else
                query = query.Where(sr => sr.Status == ServiceRequestStatus.Closed || 
                                          sr.Status == ServiceRequestStatus.Cancelled || 
                                          sr.Status == ServiceRequestStatus.Resolved);
        }

        if (filter.IsVipCustomer.HasValue)
            query = query.Where(sr => sr.IsVipCustomer == filter.IsVipCustomer);

        if (filter.ResponseSlaBreached.HasValue)
            query = query.Where(sr => sr.ResponseSlaBreached == filter.ResponseSlaBreached);

        if (filter.ResolutionSlaBreached.HasValue)
            query = query.Where(sr => sr.ResolutionSlaBreached == filter.ResolutionSlaBreached);

        if (filter.CreatedFrom.HasValue)
            query = query.Where(sr => sr.CreatedAt >= filter.CreatedFrom);

        if (filter.CreatedTo.HasValue)
            query = query.Where(sr => sr.CreatedAt <= filter.CreatedTo);

        if (filter.ResolvedFrom.HasValue)
            query = query.Where(sr => sr.ResolvedDate >= filter.ResolvedFrom);

        if (filter.ResolvedTo.HasValue)
            query = query.Where(sr => sr.ResolvedDate <= filter.ResolvedTo);

        if (!string.IsNullOrWhiteSpace(filter.Tags))
        {
            var tags = filter.Tags.Split(',').Select(t => t.Trim().ToLower());
            query = query.Where(sr => sr.Tags != null && tags.Any(t => sr.Tags.ToLower().Contains(t)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "ticketnumber" => filter.SortDescending ? query.OrderByDescending(sr => sr.TicketNumber) : query.OrderBy(sr => sr.TicketNumber),
            "subject" => filter.SortDescending ? query.OrderByDescending(sr => sr.Subject) : query.OrderBy(sr => sr.Subject),
            "status" => filter.SortDescending ? query.OrderByDescending(sr => sr.Status) : query.OrderBy(sr => sr.Status),
            "priority" => filter.SortDescending ? query.OrderByDescending(sr => sr.Priority) : query.OrderBy(sr => sr.Priority),
            "createdat" => filter.SortDescending ? query.OrderByDescending(sr => sr.CreatedAt) : query.OrderBy(sr => sr.CreatedAt),
            "updatedat" => filter.SortDescending ? query.OrderByDescending(sr => sr.UpdatedAt) : query.OrderBy(sr => sr.UpdatedAt),
            "responseduedate" => filter.SortDescending ? query.OrderByDescending(sr => sr.ResponseDueDate) : query.OrderBy(sr => sr.ResponseDueDate),
            "resolutionduedate" => filter.SortDescending ? query.OrderByDescending(sr => sr.ResolutionDueDate) : query.OrderBy(sr => sr.ResolutionDueDate),
            _ => filter.SortDescending ? query.OrderByDescending(sr => sr.CreatedAt) : query.OrderBy(sr => sr.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedServiceRequestResult
        {
            Items = items.Select(MapToListDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ServiceRequestDto?> GetServiceRequestByIdAsync(int id)
    {
        var entity = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.Customer)
            .Include(sr => sr.Contact)
            .Include(sr => sr.AssignedToUser)
            .Include(sr => sr.AssignedToGroup)
            .Include(sr => sr.CreatedByUser)
            .Include(sr => sr.Workflow)
            .Include(sr => sr.RelatedOpportunity)
            .Include(sr => sr.RelatedProduct)
            .Include(sr => sr.ParentServiceRequest)
            .Include(sr => sr.ChildServiceRequests)
            .FirstOrDefaultAsync(sr => sr.Id == id && !sr.IsDeleted);

        return entity != null ? await MapToDto(entity) : null;
    }

    public async Task<ServiceRequestDto?> GetServiceRequestByTicketNumberAsync(string ticketNumber)
    {
        var entity = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.Customer)
            .Include(sr => sr.Contact)
            .Include(sr => sr.AssignedToUser)
            .Include(sr => sr.AssignedToGroup)
            .Include(sr => sr.CreatedByUser)
            .Include(sr => sr.Workflow)
            .FirstOrDefaultAsync(sr => sr.TicketNumber == ticketNumber && !sr.IsDeleted);

        return entity != null ? await MapToDto(entity) : null;
    }

    public async Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, int? createdByUserId)
    {
        var entity = new ServiceRequest
        {
            TicketNumber = GenerateTicketNumber(),
            Subject = dto.Subject,
            Description = dto.Description,
            Channel = dto.Channel,
            Status = ServiceRequestStatus.New,
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            CustomerId = dto.CustomerId,
            ContactId = dto.ContactId,
            RequesterName = dto.RequesterName,
            RequesterEmail = dto.RequesterEmail,
            RequesterPhone = dto.RequesterPhone,
            AssignedToUserId = dto.AssignedToUserId,
            AssignedToGroupId = dto.AssignedToGroupId,
            CreatedByUserId = createdByUserId,
            WorkflowId = dto.WorkflowId,
            ExternalReferenceId = dto.ExternalReferenceId,
            SourcePhoneNumber = dto.SourcePhoneNumber,
            SourceEmailAddress = dto.SourceEmailAddress,
            ConversationId = dto.ConversationId,
            RelatedOpportunityId = dto.RelatedOpportunityId,
            RelatedProductId = dto.RelatedProductId,
            ParentServiceRequestId = dto.ParentServiceRequestId,
            Tags = dto.Tags,
            InternalNotes = dto.InternalNotes,
            IsVipCustomer = dto.IsVipCustomer,
            EstimatedEffortHours = dto.EstimatedEffortHours,
            CreatedAt = DateTime.UtcNow
        };

        // Load category and subcategory for SLA calculation
        if (dto.CategoryId.HasValue)
        {
            entity.Category = await _context.ServiceRequestCategories.FindAsync(dto.CategoryId.Value);
        }
        if (dto.SubcategoryId.HasValue)
        {
            entity.Subcategory = await _context.ServiceRequestSubcategories.FindAsync(dto.SubcategoryId.Value);
            
            // Apply default workflow from subcategory if not specified
            if (!dto.WorkflowId.HasValue && entity.Subcategory?.DefaultWorkflowId.HasValue == true)
            {
                entity.WorkflowId = entity.Subcategory.DefaultWorkflowId;
            }
            
            // Apply default priority from subcategory if not set
            if (dto.Priority == ServiceRequestPriority.Medium && entity.Subcategory?.DefaultPriority.HasValue == true)
            {
                entity.Priority = entity.Subcategory.DefaultPriority.Value;
            }
        }

        // Calculate SLA deadlines
        CalculateSlaDeadlines(entity);

        _context.ServiceRequests.Add(entity);
        await _context.SaveChangesAsync();

        // Set custom field values
        if (dto.CustomFieldValues?.Any() == true)
        {
            await SetCustomFieldValuesAsync(entity.Id, dto.CustomFieldValues);
        }

        _logger.LogInformation("Created service request {TicketNumber} via {Channel}", entity.TicketNumber, entity.Channel);

        return await GetServiceRequestByIdAsync(entity.Id) ?? throw new InvalidOperationException("Failed to create service request");
    }

    public async Task<ServiceRequestDto> UpdateServiceRequestAsync(int id, UpdateServiceRequestDto dto, int? modifiedByUserId)
    {
        var entity = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .FirstOrDefaultAsync(sr => sr.Id == id && !sr.IsDeleted)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.Subject = dto.Subject;
        entity.Description = dto.Description;
        entity.Channel = dto.Channel;
        entity.Status = dto.Status;
        entity.Priority = dto.Priority;
        entity.CategoryId = dto.CategoryId;
        entity.SubcategoryId = dto.SubcategoryId;
        entity.CustomerId = dto.CustomerId;
        entity.ContactId = dto.ContactId;
        entity.RequesterName = dto.RequesterName;
        entity.RequesterEmail = dto.RequesterEmail;
        entity.RequesterPhone = dto.RequesterPhone;
        entity.AssignedToUserId = dto.AssignedToUserId;
        entity.AssignedToGroupId = dto.AssignedToGroupId;
        entity.WorkflowId = dto.WorkflowId;
        entity.CurrentWorkflowStep = dto.CurrentWorkflowStep;
        entity.ResponseDueDate = dto.ResponseDueDate;
        entity.ResolutionDueDate = dto.ResolutionDueDate;
        entity.ResolutionSummary = dto.ResolutionSummary;
        entity.ResolutionCode = dto.ResolutionCode;
        entity.RootCause = dto.RootCause;
        entity.RelatedOpportunityId = dto.RelatedOpportunityId;
        entity.RelatedProductId = dto.RelatedProductId;
        entity.ParentServiceRequestId = dto.ParentServiceRequestId;
        entity.Tags = dto.Tags;
        entity.InternalNotes = dto.InternalNotes;
        entity.IsVipCustomer = dto.IsVipCustomer;
        entity.EstimatedEffortHours = dto.EstimatedEffortHours;
        entity.ActualEffortHours = dto.ActualEffortHours;
        entity.LastModifiedByUserId = modifiedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        // Check SLA breaches
        if (!entity.FirstResponseDate.HasValue && entity.ResponseDueDate < DateTime.UtcNow)
        {
            entity.ResponseSlaBreached = true;
        }
        if (!entity.ResolvedDate.HasValue && entity.ResolutionDueDate < DateTime.UtcNow)
        {
            entity.ResolutionSlaBreached = true;
        }

        await _context.SaveChangesAsync();

        // Update custom field values
        if (dto.CustomFieldValues != null)
        {
            await SetCustomFieldValuesAsync(entity.Id, dto.CustomFieldValues);
        }

        _logger.LogInformation("Updated service request {TicketNumber}", entity.TicketNumber);

        return await GetServiceRequestByIdAsync(entity.Id) ?? throw new InvalidOperationException("Failed to update service request");
    }

    public async Task<bool> DeleteServiceRequestAsync(int id)
    {
        var entity = await _context.ServiceRequests.FindAsync(id);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Soft deleted service request {TicketNumber}", entity.TicketNumber);
        return true;
    }

    public async Task<List<ServiceRequestListDto>> GetServiceRequestsByCustomerAsync(int customerId)
    {
        var entities = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.AssignedToUser)
            .Where(sr => sr.CustomerId == customerId && !sr.IsDeleted)
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        return entities.Select(MapToListDto).ToList();
    }

    public async Task<List<ServiceRequestListDto>> GetServiceRequestsByContactAsync(int contactId)
    {
        var entities = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.AssignedToUser)
            .Include(sr => sr.Customer)
            .Where(sr => sr.ContactId == contactId && !sr.IsDeleted)
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        return entities.Select(MapToListDto).ToList();
    }

    public async Task<List<ServiceRequestListDto>> GetServiceRequestsByAssigneeAsync(int userId)
    {
        var entities = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.Customer)
            .Where(sr => sr.AssignedToUserId == userId && !sr.IsDeleted && sr.Status != ServiceRequestStatus.Closed)
            .OrderByDescending(sr => sr.Priority)
            .ThenBy(sr => sr.ResolutionDueDate)
            .ToListAsync();

        return entities.Select(MapToListDto).ToList();
    }

    public async Task<List<ServiceRequestListDto>> GetServiceRequestsByGroupAsync(int groupId)
    {
        var entities = await _context.ServiceRequests
            .Include(sr => sr.Category)
            .Include(sr => sr.Subcategory)
            .Include(sr => sr.Customer)
            .Include(sr => sr.AssignedToUser)
            .Where(sr => sr.AssignedToGroupId == groupId && !sr.IsDeleted && sr.Status != ServiceRequestStatus.Closed)
            .OrderByDescending(sr => sr.Priority)
            .ThenBy(sr => sr.ResolutionDueDate)
            .ToListAsync();

        return entities.Select(MapToListDto).ToList();
    }

    #endregion

    #region Status Operations

    public async Task<ServiceRequestDto> UpdateStatusAsync(int id, ServiceRequestStatus newStatus, int? modifiedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        var oldStatus = entity.Status;
        entity.Status = newStatus;
        entity.LastModifiedByUserId = modifiedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        // Handle status-specific logic
        if (newStatus == ServiceRequestStatus.Resolved && !entity.ResolvedDate.HasValue)
        {
            entity.ResolvedDate = DateTime.UtcNow;
        }
        else if (newStatus == ServiceRequestStatus.Closed && !entity.ClosedDate.HasValue)
        {
            entity.ClosedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated service request {TicketNumber} status from {OldStatus} to {NewStatus}", 
            entity.TicketNumber, oldStatus, newStatus);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> MarkFirstResponseAsync(int id, int? userId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        if (!entity.FirstResponseDate.HasValue)
        {
            entity.FirstResponseDate = DateTime.UtcNow;
            entity.LastModifiedByUserId = userId;
            entity.UpdatedAt = DateTime.UtcNow;

            // Check if SLA was breached
            if (entity.ResponseDueDate.HasValue && entity.FirstResponseDate > entity.ResponseDueDate)
            {
                entity.ResponseSlaBreached = true;
            }

            // Update status if still new
            if (entity.Status == ServiceRequestStatus.New)
            {
                entity.Status = ServiceRequestStatus.Open;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Marked first response for service request {TicketNumber}", entity.TicketNumber);
        }

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> ResolveServiceRequestAsync(int id, string resolutionSummary, string? resolutionCode, string? rootCause, int? resolvedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.Status = ServiceRequestStatus.Resolved;
        entity.ResolutionSummary = resolutionSummary;
        entity.ResolutionCode = resolutionCode;
        entity.RootCause = rootCause;
        entity.ResolvedDate = DateTime.UtcNow;
        entity.LastModifiedByUserId = resolvedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        // Check if resolution SLA was breached
        if (entity.ResolutionDueDate.HasValue && entity.ResolvedDate > entity.ResolutionDueDate)
        {
            entity.ResolutionSlaBreached = true;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Resolved service request {TicketNumber}", entity.TicketNumber);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> CloseServiceRequestAsync(int id, int? closedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.Status = ServiceRequestStatus.Closed;
        entity.ClosedDate = DateTime.UtcNow;
        entity.LastModifiedByUserId = closedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Closed service request {TicketNumber}", entity.TicketNumber);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> ReopenServiceRequestAsync(int id, string reason, int? reopenedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.Status = ServiceRequestStatus.Reopened;
        entity.ReopenCount++;
        entity.ResolvedDate = null;
        entity.ClosedDate = null;
        entity.InternalNotes = string.IsNullOrEmpty(entity.InternalNotes) 
            ? $"Reopened: {reason}" 
            : $"{entity.InternalNotes}\n\nReopened: {reason}";
        entity.LastModifiedByUserId = reopenedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reopened service request {TicketNumber}. Reopen count: {ReopenCount}", 
            entity.TicketNumber, entity.ReopenCount);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> EscalateServiceRequestAsync(int id, string reason, int? escalatedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.Status = ServiceRequestStatus.Escalated;
        entity.EscalationLevel++;
        entity.InternalNotes = string.IsNullOrEmpty(entity.InternalNotes) 
            ? $"Escalated (Level {entity.EscalationLevel}): {reason}" 
            : $"{entity.InternalNotes}\n\nEscalated (Level {entity.EscalationLevel}): {reason}";
        entity.LastModifiedByUserId = escalatedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Escalated service request {TicketNumber} to level {Level}", 
            entity.TicketNumber, entity.EscalationLevel);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    #endregion

    #region Assignment Operations

    public async Task<ServiceRequestDto> AssignToUserAsync(int id, int userId, int? assignedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.AssignedToUserId = userId;
        entity.LastModifiedByUserId = assignedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.Status == ServiceRequestStatus.New)
        {
            entity.Status = ServiceRequestStatus.Open;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned service request {TicketNumber} to user {UserId}", entity.TicketNumber, userId);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> AssignToGroupAsync(int id, int groupId, int? assignedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.AssignedToGroupId = groupId;
        entity.AssignedToUserId = null; // Clear individual assignment when assigning to group
        entity.LastModifiedByUserId = assignedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned service request {TicketNumber} to group {GroupId}", entity.TicketNumber, groupId);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestDto> UnassignAsync(int id, int? modifiedByUserId)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.AssignedToUserId = null;
        entity.AssignedToGroupId = null;
        entity.LastModifiedByUserId = modifiedByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Unassigned service request {TicketNumber}", entity.TicketNumber);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    #endregion

    #region Custom Field Operations

    public async Task SetCustomFieldValuesAsync(int serviceRequestId, List<SetCustomFieldValueDto> values)
    {
        // Remove existing values
        var existingValues = await _context.ServiceRequestCustomFieldValues
            .Where(v => v.ServiceRequestId == serviceRequestId)
            .ToListAsync();
        
        _context.ServiceRequestCustomFieldValues.RemoveRange(existingValues);

        // Add new values
        foreach (var value in values)
        {
            var fieldValue = new ServiceRequestCustomFieldValue
            {
                ServiceRequestId = serviceRequestId,
                CustomFieldDefinitionId = value.CustomFieldDefinitionId,
                TextValue = value.TextValue,
                NumericValue = value.NumericValue,
                DateValue = value.DateValue,
                BooleanValue = value.BooleanValue,
                CreatedAt = DateTime.UtcNow
            };
            _context.ServiceRequestCustomFieldValues.Add(fieldValue);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ServiceRequestCustomFieldValueDto>> GetCustomFieldValuesAsync(int serviceRequestId)
    {
        var values = await _context.ServiceRequestCustomFieldValues
            .Include(v => v.CustomFieldDefinition)
            .Where(v => v.ServiceRequestId == serviceRequestId)
            .ToListAsync();

        return values.Select(v => new ServiceRequestCustomFieldValueDto
        {
            Id = v.Id,
            ServiceRequestId = v.ServiceRequestId,
            CustomFieldDefinitionId = v.CustomFieldDefinitionId,
            FieldKey = v.CustomFieldDefinition?.FieldKey ?? string.Empty,
            FieldName = v.CustomFieldDefinition?.Name ?? string.Empty,
            FieldType = v.CustomFieldDefinition?.FieldType ?? CustomFieldType.Text,
            TextValue = v.TextValue,
            NumericValue = v.NumericValue,
            DateValue = v.DateValue,
            BooleanValue = v.BooleanValue,
            DisplayValue = GetDisplayValue(v)
        }).ToList();
    }

    private static object? GetDisplayValue(ServiceRequestCustomFieldValue value)
    {
        if (value.CustomFieldDefinition == null) return value.TextValue;

        return value.CustomFieldDefinition.FieldType switch
        {
            CustomFieldType.Text or CustomFieldType.TextArea or CustomFieldType.Email or 
            CustomFieldType.Phone or CustomFieldType.Url or CustomFieldType.Dropdown or 
            CustomFieldType.MultiSelect => value.TextValue,
            CustomFieldType.Number or CustomFieldType.Decimal => value.NumericValue,
            CustomFieldType.Date or CustomFieldType.DateTime => value.DateValue,
            CustomFieldType.Boolean => value.BooleanValue,
            _ => value.TextValue
        };
    }

    #endregion

    #region Feedback Operations

    public async Task<ServiceRequestDto> SubmitFeedbackAsync(int id, int rating, string? feedback)
    {
        var entity = await _context.ServiceRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request {id} not found");

        entity.SatisfactionRating = rating;
        entity.CustomerFeedback = feedback;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Submitted feedback for service request {TicketNumber}: Rating {Rating}", 
            entity.TicketNumber, rating);

        return await GetServiceRequestByIdAsync(id) ?? throw new InvalidOperationException();
    }

    #endregion

    #region Statistics

    public async Task<ServiceRequestStatisticsDto> GetStatisticsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var requests = _context.ServiceRequests.Where(sr => !sr.IsDeleted);

        var stats = new ServiceRequestStatisticsDto
        {
            TotalRequests = await requests.CountAsync(),
            OpenRequests = await requests.CountAsync(sr => 
                sr.Status != ServiceRequestStatus.Closed && 
                sr.Status != ServiceRequestStatus.Cancelled && 
                sr.Status != ServiceRequestStatus.Resolved),
            NewRequests = await requests.CountAsync(sr => sr.Status == ServiceRequestStatus.New),
            InProgressRequests = await requests.CountAsync(sr => sr.Status == ServiceRequestStatus.InProgress),
            PendingRequests = await requests.CountAsync(sr => 
                sr.Status == ServiceRequestStatus.PendingCustomer || 
                sr.Status == ServiceRequestStatus.PendingInternal),
            EscalatedRequests = await requests.CountAsync(sr => sr.Status == ServiceRequestStatus.Escalated),
            ResolvedToday = await requests.CountAsync(sr => sr.ResolvedDate.HasValue && sr.ResolvedDate.Value.Date == today),
            CreatedToday = await requests.CountAsync(sr => sr.CreatedAt.Date == today),
            SlaBreachedCount = await requests.CountAsync(sr => sr.ResponseSlaBreached || sr.ResolutionSlaBreached),
            SlaAtRiskCount = await requests.CountAsync(sr => 
                (!sr.FirstResponseDate.HasValue && sr.ResponseDueDate.HasValue && sr.ResponseDueDate.Value <= DateTime.UtcNow.AddHours(2)) ||
                (!sr.ResolvedDate.HasValue && sr.ResolutionDueDate.HasValue && sr.ResolutionDueDate.Value <= DateTime.UtcNow.AddHours(4)))
        };

        // Calculate averages
        var resolvedRequests = await requests
            .Where(sr => sr.ResolvedDate.HasValue)
            .ToListAsync();

        if (resolvedRequests.Any())
        {
            stats.AverageResolutionTimeHours = resolvedRequests
                .Where(sr => sr.TimeToResolutionHours.HasValue)
                .Average(sr => sr.TimeToResolutionHours!.Value);
        }

        var respondedRequests = await requests
            .Where(sr => sr.FirstResponseDate.HasValue)
            .ToListAsync();

        if (respondedRequests.Any())
        {
            stats.AverageFirstResponseTimeHours = respondedRequests
                .Where(sr => sr.TimeToFirstResponseHours.HasValue)
                .Average(sr => sr.TimeToFirstResponseHours!.Value);
        }

        var ratedRequests = await requests
            .Where(sr => sr.SatisfactionRating.HasValue)
            .ToListAsync();

        if (ratedRequests.Any())
        {
            stats.CustomerSatisfactionAverage = ratedRequests.Average(sr => sr.SatisfactionRating!.Value);
        }

        // Group by channel
        stats.ByChannel = await requests
            .GroupBy(sr => sr.Channel)
            .Select(g => new { Channel = GetChannelName(g.Key), Count = g.Count() })
            .ToDictionaryAsync(x => x.Channel, x => x.Count);

        // Group by category
        stats.ByCategory = await requests
            .Where(sr => sr.Category != null)
            .GroupBy(sr => sr.Category!.Name)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);

        // Group by priority
        stats.ByPriority = await requests
            .GroupBy(sr => sr.Priority)
            .Select(g => new { Priority = GetPriorityName(g.Key), Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        // Group by status
        stats.ByStatus = await requests
            .GroupBy(sr => sr.Status)
            .Select(g => new { Status = GetStatusName(g.Key), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return stats;
    }

    public async Task<int> GetOpenRequestsCountAsync()
    {
        return await _context.ServiceRequests
            .Where(sr => !sr.IsDeleted && 
                         sr.Status != ServiceRequestStatus.Closed && 
                         sr.Status != ServiceRequestStatus.Cancelled && 
                         sr.Status != ServiceRequestStatus.Resolved)
            .CountAsync();
    }

    public async Task<int> GetSlaBreachedCountAsync()
    {
        return await _context.ServiceRequests
            .Where(sr => !sr.IsDeleted && (sr.ResponseSlaBreached || sr.ResolutionSlaBreached))
            .CountAsync();
    }

    #endregion
}
