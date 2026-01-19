using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing users, their profiles, and departments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IRepository<User> userRepository,
        IRepository<UserProfile> profileRepository,
        IRepository<Department> departmentRepository,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users
                .Where(u => !u.IsDeleted)
                .Select(u => MapToDto(u))
                .ToList();

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
        }
    }

    /// <summary>
    /// Get users by department
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByDepartment(int departmentId)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var departmentUsers = users
                .Where(u => !u.IsDeleted && u.DepartmentId == departmentId)
                .Select(u => MapToDto(u))
                .ToList();

            return Ok(departmentUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for department {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            return Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {Id}", id);
            return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign user to department and profile
    /// </summary>
    [HttpPost("{id}/assign-profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> AssignUserProfile(int id, [FromBody] AssignProfileDto assignDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            // Verify profile exists
            var profile = await _profileRepository.GetByIdAsync(assignDto.UserProfileId);
            if (profile == null || profile.IsDeleted)
                return BadRequest(new { message = "Profile not found" });

            // Verify department exists if provided
            if (assignDto.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(assignDto.DepartmentId.Value);
                if (department == null || department.IsDeleted)
                    return BadRequest(new { message = "Department not found" });

                user.DepartmentId = assignDto.DepartmentId;
            }

            user.UserProfileId = assignDto.UserProfileId;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            return Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning profile to user {Id}", id);
            return StatusCode(500, new { message = "Error assigning profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove user from profile
    /// </summary>
    [HttpPost("{id}/remove-profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> RemoveUserProfile(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            user.UserProfileId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            return Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing profile from user {Id}", id);
            return StatusCode(500, new { message = "Error removing profile", error = ex.Message });
        }
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = ((UserRole)user.Role).ToString(),
            IsActive = user.IsActive,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name,
            UserProfileId = user.UserProfileId,
            UserProfileName = user.UserProfile?.Name,
            CreatedAt = user.CreatedAt,
            LastLoginDate = user.LastLoginDate
        };
    }
}

/// <summary>
/// DTO for assigning profile to user
/// </summary>
public class AssignProfileDto
{
    public int UserProfileId { get; set; }
    public int? DepartmentId { get; set; }
}
