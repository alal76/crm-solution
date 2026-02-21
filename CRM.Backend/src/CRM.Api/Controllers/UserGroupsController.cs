// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for user group management including role-based permissions and membership.
/// Provides CRUD operations for user groups and member management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class UserGroupsController : ControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly ILogger<UserGroupsController> _logger;

    public UserGroupsController(
        IUserGroupService userGroupService,
        ILogger<UserGroupsController> logger)
    {
        _userGroupService = userGroupService;
        _logger = logger;
    }

    /// <summary>
    /// Get all user groups.
    /// </summary>
    /// <returns>A list of all user groups</returns>
    /// <response code="200">Returns the list of user groups</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserGroupDto>>> GetAll()
    {
        try
        {
            var groups = await _userGroupService.GetAllGroupsAsync();
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user groups");
            return StatusCode(500, new { error = "Failed to retrieve user groups" });
        }
    }

    /// <summary>
    /// Get a specific user group by ID.
    /// </summary>
    /// <param name="id">The user group ID</param>
    /// <returns>The user group details</returns>
    /// <response code="200">Returns the user group</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the user group is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGroupDto>> GetById(int id)
    {
        try
        {
            var group = await _userGroupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound(new { error = "Group not found" });

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user group {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve user group" });
        }
    }

    /// <summary>
    /// Create a new user group.
    /// </summary>
    /// <param name="request">The user group creation request</param>
    /// <returns>The created user group</returns>
    /// <response code="201">Returns the newly created user group</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGroupDto>> Create([FromBody] CreateUserGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "Group name is required" });

            var group = await _userGroupService.CreateGroupAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Conflict(new { error = $"A user group with name '{request.Name}' already exists" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user group");
            return StatusCode(500, new { error = "Failed to create user group" });
        }
    }

    /// <summary>
    /// Update an existing user group.
    /// </summary>
    /// <param name="id">The user group ID to update</param>
    /// <param name="request">The user group update request</param>
    /// <returns>The updated user group</returns>
    /// <response code="200">Returns the updated user group</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the user group is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserGroupDto>> Update(int id, [FromBody] CreateUserGroupRequest request)
    {
        try
        {
            var group = await _userGroupService.UpdateGroupAsync(id, request);
            if (group == null)
                return NotFound(new { error = "Group not found" });

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user group {Id}", id);
            return StatusCode(500, new { error = "Failed to update user group" });
        }
    }

    /// <summary>
    /// Delete a user group.
    /// </summary>
    /// <param name="id">The user group ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">User group deleted successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the user group is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _userGroupService.DeleteGroupAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user group {Id}", id);
            return StatusCode(500, new { error = "Failed to delete user group" });
        }
    }

    /// <summary>
    /// Get members of a specific group.
    /// </summary>
    /// <param name="id">The user group ID</param>
    /// <returns>A list of group members</returns>
    /// <response code="200">Returns the list of group members</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the user group is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(IEnumerable<UserGroupMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserGroupMemberDto>>> GetMembers(int id)
    {
        try
        {
            var members = await _userGroupService.GetGroupMembersAsync(id);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving group members for {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve group members" });
        }
    }

    /// <summary>
    /// Add a user to a group.
    /// </summary>
    /// <param name="id">The user group ID</param>
    /// <param name="userId">The user ID to add to the group</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the user group or user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddMember(int id, int userId)
    {
        try
        {
            await _userGroupService.AddUserToGroupAsync(id, userId);
            return Ok(new { message = "User added to group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", userId, id);
            return StatusCode(500, new { error = "Failed to add user to group" });
        }
    }

    /// <summary>
    /// Remove a user from a group.
    /// </summary>
    /// <param name="id">The user group ID</param>
    /// <param name="userId">The user ID to remove from the group</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the user group or user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveMember(int id, int userId)
    {
        try
        {
            await _userGroupService.RemoveUserFromGroupAsync(id, userId);
            return Ok(new { message = "User removed from group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", userId, id);
            return StatusCode(500, new { error = "Failed to remove user from group" });
        }
    }
}
