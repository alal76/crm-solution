using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for user group management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
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
    /// Get all user groups
    /// </summary>
    [HttpGet]
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
    /// Get a specific user group by ID
    /// </summary>
    [HttpGet("{id}")]
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
    /// Create a new user group
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserGroupDto>> Create([FromBody] CreateUserGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "Group name is required" });

            var group = await _userGroupService.CreateGroupAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user group");
            return StatusCode(500, new { error = "Failed to create user group" });
        }
    }

    /// <summary>
    /// Update an existing user group
    /// </summary>
    [HttpPut("{id}")]
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
    /// Delete a user group
    /// </summary>
    [HttpDelete("{id}")]
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
    /// Get members of a specific group
    /// </summary>
    [HttpGet("{id}/members")]
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
    /// Add a user to a group
    /// </summary>
    [HttpPost("{id}/members/{userId}")]
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
    /// Remove a user from a group
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
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
