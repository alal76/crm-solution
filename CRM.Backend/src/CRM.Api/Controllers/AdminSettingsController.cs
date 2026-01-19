using Microsoft.AspNetCore.Mvc;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Api.Authorization;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for admin settings including user approval, groups, and database management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[RequireRole(UserRole.Admin)]
public class AdminSettingsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserApprovalService _approvalService;
    private readonly IDatabaseBackupService _backupService;
    private readonly ILogger<AdminSettingsController> _logger;

    public AdminSettingsController(
        IUserService userService,
        IUserGroupService userGroupService,
        IUserApprovalService approvalService,
        IDatabaseBackupService backupService,
        ILogger<AdminSettingsController> logger)
    {
        _userService = userService;
        _userGroupService = userGroupService;
        _approvalService = approvalService;
        _backupService = backupService;
        _logger = logger;
    }

    #region User Approval

    /// <summary>
    /// Get all pending user approval requests
    /// </summary>
    [HttpGet("approval-requests")]
    public async Task<ActionResult<IEnumerable<UserApprovalRequestDto>>> GetApprovalRequests([FromQuery] int? status = null)
    {
        try
        {
            var requests = await _approvalService.GetApprovalRequestsAsync(status);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approval requests");
            return StatusCode(500, new { error = "Failed to retrieve approval requests" });
        }
    }

    /// <summary>
    /// Get approval request by ID
    /// </summary>
    [HttpGet("approval-requests/{id}")]
    public async Task<ActionResult<UserApprovalRequestDto>> GetApprovalRequest(int id)
    {
        try
        {
            var request = await _approvalService.GetApprovalRequestByIdAsync(id);
            if (request == null)
                return NotFound(new { error = "Approval request not found" });

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving approval request {id}");
            return StatusCode(500, new { error = "Failed to retrieve approval request" });
        }
    }

    /// <summary>
    /// Approve a user registration request
    /// </summary>
    [HttpPost("approval-requests/{id}/approve")]
    public async Task<ActionResult<UserDto>> ApproveUser(int id, [FromBody] ApproveUserRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User ID not found in token" });

            var createdUser = await _approvalService.ApproveUserAsync(id, int.Parse(userId), request);
            return Ok(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving user request {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reject a user registration request
    /// </summary>
    [HttpPost("approval-requests/{id}/reject")]
    public async Task<ActionResult> RejectUser(int id, [FromBody] RejectUserRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User ID not found in token" });

            await _approvalService.RejectUserAsync(id, int.Parse(userId), request.RejectionReason);
            return Ok(new { message = "User request rejected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting user request {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region User Groups

    /// <summary>
    /// Get all user groups
    /// </summary>
    [HttpGet("groups")]
    public async Task<ActionResult<IEnumerable<UserGroupDto>>> GetGroups()
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
    /// Create a new user group
    /// </summary>
    [HttpPost("groups")]
    public async Task<ActionResult<UserGroupDto>> CreateGroup([FromBody] CreateUserGroupRequest request)
    {
        try
        {
            var group = await _userGroupService.CreateGroupAsync(request);
            return CreatedAtAction(nameof(GetGroupById), new { id = group.Id }, group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user group");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get group by ID with members
    /// </summary>
    [HttpGet("groups/{id}")]
    public async Task<ActionResult<UserGroupDto>> GetGroupById(int id)
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
            _logger.LogError(ex, $"Error retrieving group {id}");
            return StatusCode(500, new { error = "Failed to retrieve group" });
        }
    }

    /// <summary>
    /// Update a user group
    /// </summary>
    [HttpPut("groups/{id}")]
    public async Task<ActionResult<UserGroupDto>> UpdateGroup(int id, [FromBody] CreateUserGroupRequest request)
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
            _logger.LogError(ex, $"Error updating group {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user group
    /// </summary>
    [HttpDelete("groups/{id}")]
    public async Task<ActionResult> DeleteGroup(int id)
    {
        try
        {
            await _userGroupService.DeleteGroupAsync(id);
            return Ok(new { message = "Group deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting group {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get group members
    /// </summary>
    [HttpGet("groups/{id}/members")]
    public async Task<ActionResult<IEnumerable<UserGroupMemberDto>>> GetGroupMembers(int id)
    {
        try
        {
            var members = await _userGroupService.GetGroupMembersAsync(id);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving group members for group {id}");
            return StatusCode(500, new { error = "Failed to retrieve group members" });
        }
    }

    /// <summary>
    /// Add user to group
    /// </summary>
    [HttpPost("groups/{groupId}/members/{userId}")]
    public async Task<ActionResult> AddUserToGroup(int groupId, int userId)
    {
        try
        {
            await _userGroupService.AddUserToGroupAsync(groupId, userId);
            return Ok(new { message = "User added to group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding user {userId} to group {groupId}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove user from group
    /// </summary>
    [HttpDelete("groups/{groupId}/members/{userId}")]
    public async Task<ActionResult> RemoveUserFromGroup(int groupId, int userId)
    {
        try
        {
            await _userGroupService.RemoveUserFromGroupAsync(groupId, userId);
            return Ok(new { message = "User removed from group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing user {userId} from group {groupId}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Database Management

    /// <summary>
    /// Create a database backup
    /// </summary>
    [HttpPost("database/backup")]
    public async Task<ActionResult<DatabaseBackupDto>> CreateBackup([FromBody] CreateDatabaseBackupRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User ID not found in token" });

            var backup = await _backupService.CreateBackupAsync(int.Parse(userId), request);
            return CreatedAtAction(nameof(GetBackup), new { id = backup.Id }, backup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database backup");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all database backups
    /// </summary>
    [HttpGet("database/backups")]
    public async Task<ActionResult<IEnumerable<DatabaseBackupDto>>> GetBackups()
    {
        try
        {
            var backups = await _backupService.GetAllBackupsAsync();
            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving backups");
            return StatusCode(500, new { error = "Failed to retrieve backups" });
        }
    }

    /// <summary>
    /// Get backup by ID
    /// </summary>
    [HttpGet("database/backups/{id}")]
    public async Task<ActionResult<DatabaseBackupDto>> GetBackup(int id)
    {
        try
        {
            var backup = await _backupService.GetBackupByIdAsync(id);
            if (backup == null)
                return NotFound(new { error = "Backup not found" });

            return Ok(backup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving backup {id}");
            return StatusCode(500, new { error = "Failed to retrieve backup" });
        }
    }

    /// <summary>
    /// Restore database from backup
    /// </summary>
    [HttpPost("database/restore")]
    public async Task<ActionResult> RestoreBackup([FromBody] RestoreDatabaseBackupRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User ID not found in token" });

            await _backupService.RestoreBackupAsync(request.BackupId, request.TargetDatabase, int.Parse(userId));
            return Ok(new { message = "Database restore initiated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a backup
    /// </summary>
    [HttpDelete("database/backups/{id}")]
    public async Task<ActionResult> DeleteBackup(int id)
    {
        try
        {
            await _backupService.DeleteBackupAsync(id);
            return Ok(new { message = "Backup deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting backup {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Migrate database to different provider
    /// </summary>
    [HttpPost("database/migrate")]
    public async Task<ActionResult> MigrateDatabase([FromBody] DatabaseMigrationConfig config)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User ID not found in token" });

            await _backupService.MigrateDatabaseAsync(config, int.Parse(userId));
            return Ok(new { message = "Database migration initiated. This may take several minutes." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating database");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get seed database setup script
    /// </summary>
    [HttpGet("database/seed-script")]
    public async Task<ActionResult> GetSeedScript([FromQuery] string targetDatabase = "")
    {
        try
        {
            var script = await _backupService.GenerateSeedScriptAsync(targetDatabase);
            return Ok(new
            {
                script = script,
                targetDatabase = targetDatabase,
                description = "Execute this script to set up the seed database"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating seed script");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion
}
