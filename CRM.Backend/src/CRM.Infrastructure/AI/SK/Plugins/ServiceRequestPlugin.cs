// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.ComponentModel;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM Service Request (ticket) management.
/// Provides AI-accessible functions for viewing, assigning, resolving, and closing support tickets.
/// </summary>
public class ServiceRequestPlugin : CrmPluginBase
{
    private readonly IServiceRequestService _serviceRequestService;
    private readonly ICrmDbContext _context;

    /// <inheritdoc />
    public override string PluginName => "ServiceRequest";

    /// <inheritdoc />
    public override string Description => "Manage CRM service requests (tickets) — search, view SLA status, assign, update priority, add comments, resolve, and close tickets.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRequestPlugin"/> class.
    /// </summary>
    /// <param name="serviceRequestService">The service request service.</param>
    /// <param name="context">The database context for direct queries.</param>
    /// <param name="logger">The logger instance.</param>
    public ServiceRequestPlugin(
        IServiceRequestService serviceRequestService,
        ICrmDbContext context,
        ILogger<ServiceRequestPlugin> logger) : base(logger)
    {
        _serviceRequestService = serviceRequestService ?? throw new ArgumentNullException(nameof(serviceRequestService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Read Methods

    /// <summary>
    /// Retrieves a single service request by its ID.
    /// </summary>
    [KernelFunction("GetTicket")]
    [Description("Get detailed information about a specific support ticket (service request) by its ID.")]
    public async Task<string> GetTicketAsync(
        [Description("The unique identifier of the ticket.")] int ticketId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticket = await _serviceRequestService.GetServiceRequestByIdAsync(ticketId);
            return ticket != null
                ? SuccessResult(ticket)
                : ErrorResult("GetTicket", $"Ticket with ID {ticketId} not found.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving ticket {TicketId}", ticketId);
            return ErrorResult("GetTicket", ex.Message);
        }
    }

    /// <summary>
    /// Searches service requests using a text query.
    /// </summary>
    [KernelFunction("SearchTickets")]
    [Description("Search for support tickets by subject, description, ticket number, or customer.")]
    public async Task<string> SearchTicketsAsync(
        [Description("The search query string.")] string query,
        [Description("Maximum number of results to return.")] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = new ServiceRequestFilterDto
            {
                SearchTerm = query,
                PageSize = maxResults,
                Page = 1
            };

            var result = await _serviceRequestService.GetServiceRequestsAsync(filter);
            return SuccessResult(new { count = result.Items.Count, totalCount = result.TotalCount, tickets = result.Items });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching tickets with query '{Query}'", query);
            return ErrorResult("SearchTickets", ex.Message);
        }
    }

    /// <summary>
    /// Gets the SLA status for a specific ticket.
    /// </summary>
    [KernelFunction("GetSLAStatus")]
    [Description("Get the SLA compliance status for a specific support ticket, including response and resolution deadlines.")]
    public async Task<string> GetSLAStatusAsync(
        [Description("The unique identifier of the ticket.")] int ticketId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticket = await _serviceRequestService.GetServiceRequestByIdAsync(ticketId);
            if (ticket == null)
                return ErrorResult("GetSLAStatus", $"Ticket with ID {ticketId} not found.");

            return SuccessResult(new
            {
                ticketId,
                status = ticket.Status,
                priority = ticket.Priority,
                responseSlaBreached = ticket.ResponseSlaBreached,
                resolutionSlaBreached = ticket.ResolutionSlaBreached,
                firstResponseAt = ticket.FirstResponseDate,
                resolvedAt = ticket.ResolvedDate,
                createdAt = ticket.CreatedAt
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving SLA status for ticket {TicketId}", ticketId);
            return ErrorResult("GetSLAStatus", ex.Message);
        }
    }

    #endregion

    #region Write Methods

    /// <summary>
    /// Assigns a ticket to a specific user.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Assigns a support ticket to a user.")]
    [KernelFunction("AssignTicket")]
    [Description("Assign a support ticket to a specific user for handling.")]
    public async Task<string> AssignTicketAsync(
        [Description("The unique identifier of the ticket.")] int ticketId,
        [Description("The user ID to assign the ticket to.")] int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _serviceRequestService.AssignToUserAsync(ticketId, userId, null);
            return SuccessResult(new { assigned = true, ticketId, assignedToUserId = userId });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning ticket {TicketId} to user {UserId}", ticketId, userId);
            return ErrorResult("AssignTicket", ex.Message);
        }
    }

    /// <summary>
    /// Updates the priority of a ticket.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Changes the priority level of a support ticket.")]
    [KernelFunction("UpdatePriority")]
    [Description("Update the priority of a support ticket (e.g., Low, Medium, High, Critical).")]
    public async Task<string> UpdatePriorityAsync(
        [Description("The unique identifier of the ticket.")] int ticketId,
        [Description("The new priority level (e.g., Low, Medium, High, Critical).")] string priority,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.Id == ticketId && !sr.IsDeleted, cancellationToken);

            if (entity == null)
                return ErrorResult("UpdatePriority", $"Ticket with ID {ticketId} not found.");

            if (Enum.TryParse<ServiceRequestPriority>(priority, true, out var parsedPriority))
            {
                entity.Priority = parsedPriority;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return SuccessResult(new { updated = true, ticketId, priority });
            }

            return ErrorResult("UpdatePriority", $"Invalid priority: '{priority}'. Use Low, Medium, High, or Critical.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating priority for ticket {TicketId}", ticketId);
            return ErrorResult("UpdatePriority", ex.Message);
        }
    }

    /// <summary>
    /// Adds a comment/note to a ticket.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Adds a comment note to a support ticket.")]
    [KernelFunction("AddComment")]
    [Description("Add a comment or internal note to a support ticket.")]
    public async Task<string> AddCommentAsync(
        [Description("The unique identifier of the ticket.")] int ticketId,
        [Description("The comment text to add.")] string comment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticket = await _serviceRequestService.GetServiceRequestByIdAsync(ticketId);
            if (ticket == null)
                return ErrorResult("AddComment", $"Ticket with ID {ticketId} not found.");

            var note = new Note
            {
                Title = "AI-Generated Comment",
                Content = comment,
                EntityType = "ServiceRequest",
                EntityId = ticketId,
                NoteType = NoteType.General,
                Visibility = NoteVisibility.Team,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return SuccessResult(new { noteId = note.Id, ticketId, comment });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding comment to ticket {TicketId}", ticketId);
            return ErrorResult("AddComment", ex.Message);
        }
    }

    /// <summary>
    /// Closes a support ticket.
    /// </summary>
    [RequiresApproval(Tier = "standard", Description = "Closes a support ticket, ending SLA tracking.")]
    [KernelFunction("CloseTicket")]
    [Description("Close a support ticket after it has been resolved and confirmed by the customer.")]
    public async Task<string> CloseTicketAsync(
        [Description("The unique identifier of the ticket to close.")] int ticketId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _serviceRequestService.CloseServiceRequestAsync(ticketId, null);
            return SuccessResult(new { closed = true, ticketId });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing ticket {TicketId}", ticketId);
            return ErrorResult("CloseTicket", ex.Message);
        }
    }

    /// <summary>
    /// Resolves a support ticket with a resolution summary.
    /// </summary>
    [RequiresApproval(Tier = "standard", Description = "Resolves a support ticket with a resolution summary.")]
    [KernelFunction("ResolveTicket")]
    [Description("Resolve a support ticket by providing a resolution summary describing how the issue was addressed.")]
    public async Task<string> ResolveTicketAsync(
        [Description("The unique identifier of the ticket.")] int ticketId,
        [Description("A summary of how the issue was resolved.")] string resolution,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _serviceRequestService.ResolveServiceRequestAsync(
                ticketId, resolution, null, null, null);
            return SuccessResult(new { resolved = true, ticketId, resolution });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving ticket {TicketId}", ticketId);
            return ErrorResult("ResolveTicket", ex.Message);
        }
    }

    #endregion
}
