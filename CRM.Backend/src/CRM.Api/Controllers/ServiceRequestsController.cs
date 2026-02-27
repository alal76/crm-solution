// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing service requests (support tickets).
/// Provides comprehensive CRUD operations, status management, assignments, and statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ServiceRequestsController : ControllerBase
{
    private const string ServiceRequestNotFoundMessage = "Service request {0} not found";
    private const string GenericErrorMessage = "An error occurred";
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
    /// Get service requests with filtering and pagination.
    /// </summary>
    /// <param name="filter">Filter criteria for service requests</param>
    /// <returns>Paged list of service requests</returns>
    /// <response code="200">Returns the list of service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedServiceRequestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get a service request by ID.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <returns>The service request details</returns>
    /// <response code="200">Returns the service request</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(int id)
    {
        try
        {
            var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
            if (request == null)
                return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the service request");
        }
    }

    /// <summary>
    /// Get a service request by ticket number.
    /// </summary>
    /// <param name="ticketNumber">The ticket number</param>
    /// <returns>The service request details</returns>
    /// <response code="200">Returns the service request</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("ticket/{ticketNumber}")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceRequestDto>> GetServiceRequestByTicketNumber(string ticketNumber)
    {
        try
        {
            var request = await _serviceRequestService.GetServiceRequestByTicketNumberAsync(ticketNumber);
            if (request == null)
                return NotFound(string.Format(ServiceRequestNotFoundMessage, ticketNumber));
            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request {TicketNumber}", ticketNumber);
            return StatusCode(500, "An error occurred while retrieving the service request");
        }
    }

    /// <summary>
    /// Create a new service request.
    /// </summary>
    /// <param name="dto">The service request creation data</param>
    /// <returns>The created service request</returns>
    /// <response code="201">Returns the newly created service request</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Update an existing service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="dto">The service request update data</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service request {Id}", id);
            return StatusCode(500, "An error occurred while updating the service request");
        }
    }

    /// <summary>
    /// Update custom field values for a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="values">The custom field values to set</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request</response>
    /// <response code="400">If the custom field values are invalid</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}/custom-fields")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceRequestDto>> UpdateCustomFields(int id, [FromBody] List<SetCustomFieldValueDto> values)
    {
        try
        {
            var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
            if (request == null)
                return NotFound(string.Format(ServiceRequestNotFoundMessage, id));

            var updateDto = new UpdateServiceRequestDto
            {
                CustomFieldValues = values
            };

            var userId = GetCurrentUserId();
            var result = await _serviceRequestService.UpdateServiceRequestAsync(id, updateDto, userId);
            if (result == null)
                return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating custom fields for service request {Id}", id);
            return StatusCode(500, "An error occurred while updating custom fields");
        }
    }

    /// <summary>
    /// Delete a service request (soft delete).
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Service request deleted successfully</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        try
        {
            var result = await _serviceRequestService.DeleteServiceRequestAsync(id);
            if (!result)
                return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
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
    /// Get service requests by account.
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <returns>List of service requests for the account</returns>
    /// <response code="200">Returns the list of service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(List<ServiceRequestListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ServiceRequestListDto>>> GetByAccount(int accountId)
    {
        try
        {
            var requests = await _serviceRequestService.GetServiceRequestsByAccountAsync(accountId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service requests for account {AccountId}", accountId);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Get service requests by contact.
    /// </summary>
    /// <param name="contactId">The contact ID</param>
    /// <returns>List of service requests for the contact</returns>
    /// <response code="200">Returns the list of service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("contact/{contactId}")]
    [ProducesResponseType(typeof(List<ServiceRequestListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Get service requests assigned to a user.
    /// </summary>
    /// <param name="userId">The assignee's user ID</param>
    /// <returns>List of service requests for the assignee</returns>
    /// <response code="200">Returns the list of service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("assignee/{userId}")]
    [ProducesResponseType(typeof(List<ServiceRequestListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Get my assigned service requests.
    /// </summary>
    /// <returns>List of service requests assigned to the current user</returns>
    /// <response code="200">Returns the list of service requests</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("my-requests")]
    [ProducesResponseType(typeof(List<ServiceRequestListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Get service requests assigned to a group.
    /// </summary>
    /// <param name="groupId">The group ID</param>
    /// <returns>List of service requests for the group</returns>
    /// <response code="200">Returns the list of service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("group/{groupId}")]
    [ProducesResponseType(typeof(List<ServiceRequestListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
        }
    }

    #endregion

    #region Status Operations

    /// <summary>
    /// Update service request status.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="newStatus">The new status</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request</response>
    /// <response code="400">If the status transition is invalid</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Mark first response on a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/first-response")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking first response for service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Resolve a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="dto">The resolution details</param>
    /// <returns>The resolved service request</returns>
    /// <response code="200">Returns the resolved service request</response>
    /// <response code="400">If the resolution data is invalid</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/resolve")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Close a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <returns>The closed service request</returns>
    /// <response code="200">Returns the closed service request</response>
    /// <response code="400">If the request cannot be closed</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/close")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Reopen a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="dto">The reopen reason</param>
    /// <returns>The reopened service request</returns>
    /// <response code="200">Returns the reopened service request</response>
    /// <response code="400">If the request cannot be reopened</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/reopen")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Escalate a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="dto">The escalation reason</param>
    /// <returns>The escalated service request</returns>
    /// <response code="200">Returns the escalated service request</response>
    /// <response code="400">If the escalation reason is invalid</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/escalate")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Mark a service request as expedited for priority handling.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="dto">The expedite reason</param>
    /// <returns>The expedited service request</returns>
    /// <response code="200">Returns the expedited service request</response>
    /// <response code="400">If the expedite reason is missing</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/expedite")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceRequestDto>> Expedite(int id, [FromBody] ExpediteServiceRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("An expedite reason is required.");

            var userId = GetCurrentUserId();
            var request = await _serviceRequestService.ExpediteServiceRequestAsync(id, dto.Reason, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expediting service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    #endregion

    #region Assignment Operations

    /// <summary>
    /// Assign service request to a user.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="userId">The user ID to assign to</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request</response>
    /// <response code="404">If the service request or user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/assign/user/{userId}")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning service request {Id} to user {UserId}", id, userId);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Assign service request to a group.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="groupId">The group ID to assign to</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request</response>
    /// <response code="404">If the service request or group is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/assign/group/{groupId}")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning service request {Id} to group {GroupId}", id, groupId);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Unassign a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the unassigned service request</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/unassign")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    #endregion

    #region Feedback

    /// <summary>
    /// Submit feedback for a service request.
    /// </summary>
    /// <param name="id">The service request ID</param>
    /// <param name="dto">The feedback details including rating and comments</param>
    /// <returns>The updated service request</returns>
    /// <response code="200">Returns the updated service request with feedback</response>
    /// <response code="400">If the feedback data is invalid</response>
    /// <response code="404">If the service request is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/feedback")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceRequestDto>> SubmitFeedback(int id, [FromBody] SubmitFeedbackDto dto)
    {
        try
        {
            var request = await _serviceRequestService.SubmitFeedbackAsync(id, dto.Rating, dto.Feedback);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(string.Format(ServiceRequestNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for service request {Id}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get service request statistics.
    /// </summary>
    /// <returns>Statistics summary for service requests</returns>
    /// <response code="200">Returns service request statistics</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ServiceRequestStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Get count of open service requests.
    /// </summary>
    /// <returns>The number of open service requests</returns>
    /// <response code="200">Returns the count of open service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("count/open")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Get count of SLA breached service requests.
    /// </summary>
    /// <returns>The number of service requests with breached SLA</returns>
    /// <response code="200">Returns the count of SLA breached service requests</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("count/sla-breached")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, GenericErrorMessage);
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

public class ExpediteServiceRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class SubmitFeedbackDto
{
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}

#endregion
