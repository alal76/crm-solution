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

using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing service requests
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(
        IServiceRequestService serviceRequestService,
        ILogger<ServiceRequestsController> logger)
    {
        _serviceRequestService = serviceRequestService;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #region CRUD Operations

    /// <summary>
    /// Get service requests with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedServiceRequestResult>> GetServiceRequests([FromQuery] ServiceRequestFilterDto filter)
    {
        try
        {
            var result = await _serviceRequestService.GetServiceRequestsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service requests");
            return StatusCode(500, "An error occurred while retrieving service requests");
        }
    }

    /// <summary>
    /// Get a service request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(int id)
    {
        try
        {
            var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
            if (request == null)
                return NotFound($"Service request {id} not found");
            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the service request");
        }
    }

    /// <summary>
    /// Get a service request by ticket number
    /// </summary>
    [HttpGet("ticket/{ticketNumber}")]
    public async Task<ActionResult<ServiceRequestDto>> GetServiceRequestByTicketNumber(string ticketNumber)
    {
        try
        {
            var request = await _serviceRequestService.GetServiceRequestByTicketNumberAsync(ticketNumber);
            if (request == null)
                return NotFound($"Service request {ticketNumber} not found");
            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request {TicketNumber}", ticketNumber);
            return StatusCode(500, "An error occurred while retrieving the service request");
        }
    }

    /// <summary>
    /// Create a new service request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceRequestDto>> CreateServiceRequest([FromBody] CreateServiceRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.CreateServiceRequestAsync(dto, userId);
            return CreatedAtAction(nameof(GetServiceRequest), new { id = request.Id }, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service request");
            return StatusCode(500, "An error occurred while creating the service request");
        }
    }

    /// <summary>
    /// Update an existing service request
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceRequestDto>> UpdateServiceRequest(int id, [FromBody] UpdateServiceRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.UpdateServiceRequestAsync(id, dto, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service request {Id}", id);
            return StatusCode(500, "An error occurred while updating the service request");
        }
    }

    /// <summary>
    /// Delete a service request (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        try
        {
            var result = await _serviceRequestService.DeleteServiceRequestAsync(id);
            if (!result)
                return NotFound($"Service request {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service request {Id}", id);
            return StatusCode(500, "An error occurred while deleting the service request");
        }
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Get service requests by customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<ServiceRequestListDto>>> GetByCustomer(int customerId)
    {
        try
        {
            var requests = await _serviceRequestService.GetServiceRequestsByCustomerAsync(customerId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service requests for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get service requests by contact
    /// </summary>
    [HttpGet("contact/{contactId}")]
    public async Task<ActionResult<List<ServiceRequestListDto>>> GetByContact(int contactId)
    {
        try
        {
            var requests = await _serviceRequestService.GetServiceRequestsByContactAsync(contactId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service requests for contact {ContactId}", contactId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get service requests assigned to a user
    /// </summary>
    [HttpGet("assignee/{userId}")]
    public async Task<ActionResult<List<ServiceRequestListDto>>> GetByAssignee(int userId)
    {
        try
        {
            var requests = await _serviceRequestService.GetServiceRequestsByAssigneeAsync(userId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service requests for assignee {UserId}", userId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get my assigned service requests
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<ActionResult<List<ServiceRequestListDto>>> GetMyRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();
            
            var requests = await _serviceRequestService.GetServiceRequestsByAssigneeAsync(userId.Value);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my service requests");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get service requests assigned to a group
    /// </summary>
    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<List<ServiceRequestListDto>>> GetByGroup(int groupId)
    {
        try
        {
            var requests = await _serviceRequestService.GetServiceRequestsByGroupAsync(groupId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service requests for group {GroupId}", groupId);
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Status Operations

    /// <summary>
    /// Update service request status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ServiceRequestDto>> UpdateStatus(int id, [FromBody] ServiceRequestStatus newStatus)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.UpdateStatusAsync(id, newStatus, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Mark first response on a service request
    /// </summary>
    [HttpPost("{id}/first-response")]
    public async Task<ActionResult<ServiceRequestDto>> MarkFirstResponse(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.MarkFirstResponseAsync(id, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking first response for service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Resolve a service request
    /// </summary>
    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<ServiceRequestDto>> Resolve(int id, [FromBody] ResolveServiceRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.ResolveServiceRequestAsync(
                id, dto.ResolutionSummary, dto.ResolutionCode, dto.RootCause, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Close a service request
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<ActionResult<ServiceRequestDto>> Close(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.CloseServiceRequestAsync(id, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Reopen a service request
    /// </summary>
    [HttpPost("{id}/reopen")]
    public async Task<ActionResult<ServiceRequestDto>> Reopen(int id, [FromBody] ReopenServiceRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.ReopenServiceRequestAsync(id, dto.Reason, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Escalate a service request
    /// </summary>
    [HttpPost("{id}/escalate")]
    public async Task<ActionResult<ServiceRequestDto>> Escalate(int id, [FromBody] EscalateServiceRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.EscalateServiceRequestAsync(id, dto.Reason, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Assignment Operations

    /// <summary>
    /// Assign service request to a user
    /// </summary>
    [HttpPost("{id}/assign/user/{userId}")]
    public async Task<ActionResult<ServiceRequestDto>> AssignToUser(int id, int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var request = await _serviceRequestService.AssignToUserAsync(id, userId, currentUserId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning service request {Id} to user {UserId}", id, userId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Assign service request to a group
    /// </summary>
    [HttpPost("{id}/assign/group/{groupId}")]
    public async Task<ActionResult<ServiceRequestDto>> AssignToGroup(int id, int groupId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var request = await _serviceRequestService.AssignToGroupAsync(id, groupId, currentUserId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning service request {Id} to group {GroupId}", id, groupId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Unassign service request
    /// </summary>
    [HttpPost("{id}/unassign")]
    public async Task<ActionResult<ServiceRequestDto>> Unassign(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.UnassignAsync(id, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Feedback

    /// <summary>
    /// Submit customer feedback
    /// </summary>
    [HttpPost("{id}/feedback")]
    public async Task<ActionResult<ServiceRequestDto>> SubmitFeedback(int id, [FromBody] SubmitFeedbackDto dto)
    {
        try
        {
            var request = await _serviceRequestService.SubmitFeedbackAsync(id, dto.Rating, dto.Feedback);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Service request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for service request {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get service request statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ServiceRequestStatisticsDto>> GetStatistics()
    {
        try
        {
            var stats = await _serviceRequestService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request statistics");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get open requests count
    /// </summary>
    [HttpGet("count/open")]
    public async Task<ActionResult<int>> GetOpenCount()
    {
        try
        {
            var count = await _serviceRequestService.GetOpenRequestsCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting open requests count");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get SLA breached requests count
    /// </summary>
    [HttpGet("count/sla-breached")]
    public async Task<ActionResult<int>> GetSlaBreachedCount()
    {
        try
        {
            var count = await _serviceRequestService.GetSlaBreachedCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA breached count");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion
}

#region Request DTOs

public class ResolveServiceRequestDto
{
    public string ResolutionSummary { get; set; } = string.Empty;
    public string? ResolutionCode { get; set; }
    public string? RootCause { get; set; }
}

public class ReopenServiceRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class EscalateServiceRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class SubmitFeedbackDto
{
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}

#endregion
