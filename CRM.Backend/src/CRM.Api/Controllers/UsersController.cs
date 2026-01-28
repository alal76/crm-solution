using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing users, their profiles, departments, and contact links
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IContactsService _contactsService;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IRepository<User> userRepository,
        IRepository<UserProfile> profileRepository,
        IRepository<Department> departmentRepository,
        IContactsService contactsService,
        ICrmDbContext dbContext,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _departmentRepository = departmentRepository;
        _contactsService = contactsService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with contact information
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            // Use DbContext directly with Include for navigation properties
            var users = await _dbContext.Set<User>()
                .AsNoTracking()
                .Include(u => u.Department)
                .Include(u => u.UserProfile)
                .Include(u => u.PrimaryGroup)
                .Where(u => !u.IsDeleted)
                .ToListAsync();
            
            var contacts = await _contactsService.GetAllAsync();
            var contactDict = contacts.ToDictionary(c => c.Id);
            
            var userDtos = users
                .Select(u => MapToDto(u, u.ContactId.HasValue && contactDict.ContainsKey(u.ContactId.Value) ? contactDict[u.ContactId.Value] : null))
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
            var contacts = await _contactsService.GetAllAsync();
            var contactDict = contacts.ToDictionary(c => c.Id);
            
            var departmentUsers = users
                .Where(u => !u.IsDeleted && u.DepartmentId == departmentId)
                .Select(u => MapToDto(u, u.ContactId.HasValue && contactDict.ContainsKey(u.ContactId.Value) ? contactDict[u.ContactId.Value] : null))
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
    /// Get user by ID with contact information
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

            ContactDto? contact = null;
            if (user.ContactId.HasValue)
            {
                contact = await _contactsService.GetByIdAsync(user.ContactId.Value);
            }

            return Ok(MapToDto(user, contact));
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

            return Ok(MapToDto(user, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning profile to user {Id}", id);
            return StatusCode(500, new { message = "Error assigning profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Update user (edit) including contact link
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            user.Email = updateDto.Email ?? user.Email;
            user.FirstName = updateDto.FirstName ?? user.FirstName;
            user.LastName = updateDto.LastName ?? user.LastName;
            user.Role = updateDto.Role ?? user.Role;
            user.IsActive = updateDto.IsActive ?? user.IsActive;
            user.DepartmentId = updateDto.DepartmentId ?? user.DepartmentId;
            user.UserProfileId = updateDto.UserProfileId ?? user.UserProfileId;
            user.PrimaryGroupId = updateDto.PrimaryGroupId ?? user.PrimaryGroupId;
            
            // Handle contact link - allow explicit null to unlink
            if (updateDto.ContactId.HasValue)
            {
                var contact = await _contactsService.GetByIdAsync(updateDto.ContactId.Value);
                if (contact == null)
                    return BadRequest(new { message = "Contact not found" });
                user.ContactId = updateDto.ContactId;
            }
            
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            ContactDto? linkedContact = null;
            if (user.ContactId.HasValue)
            {
                linkedContact = await _contactsService.GetByIdAsync(user.ContactId.Value);
            }

            return Ok(MapToDto(user, linkedContact));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Id}", id);
            return StatusCode(500, new { message = "Error updating user", error = ex.Message });
        }
    }

    /// <summary>
    /// Link user to a contact record
    /// </summary>
    [HttpPost("{id}/link-contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> LinkUserToContact(int id, [FromBody] LinkUserContactDto linkDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            ContactDto? contact = null;
            if (linkDto.ContactId.HasValue)
            {
                contact = await _contactsService.GetByIdAsync(linkDto.ContactId.Value);
                if (contact == null)
                    return BadRequest(new { message = "Contact not found" });

                // Check if contact is already linked to another user
                var allUsers = await _userRepository.GetAllAsync();
                var existingLink = allUsers.FirstOrDefault(u => u.ContactId == linkDto.ContactId && u.Id != id && !u.IsDeleted);
                if (existingLink != null)
                    return BadRequest(new { message = $"Contact is already linked to user: {existingLink.Username}" });
            }

            user.ContactId = linkDto.ContactId;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            _logger.LogInformation("User {UserId} linked to contact {ContactId}", id, linkDto.ContactId);

            return Ok(MapToDto(user, contact));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking user {Id} to contact", id);
            return StatusCode(500, new { message = "Error linking user to contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Unlink user from contact
    /// </summary>
    [HttpPost("{id}/unlink-contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UnlinkUserFromContact(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            user.ContactId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            _logger.LogInformation("User {UserId} unlinked from contact", id);

            return Ok(MapToDto(user, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking user {Id} from contact", id);
            return StatusCode(500, new { message = "Error unlinking user from contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Get users by contact (find user linked to a specific contact)
    /// </summary>
    [HttpGet("by-contact/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto?>> GetUserByContact(int contactId)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.ContactId == contactId && !u.IsDeleted);

            if (user == null)
                return Ok(null);

            var contact = await _contactsService.GetByIdAsync(contactId);
            return Ok(MapToDto(user, contact));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Id}", id);
            return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
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

            return Ok(MapToDto(user, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing profile from user {Id}", id);
            return StatusCode(500, new { message = "Error removing profile", error = ex.Message });
        }
    }

    private UserDto MapToDto(User user, ContactDto? contact)
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
            PrimaryGroupId = user.PrimaryGroupId,
            PrimaryGroupName = user.PrimaryGroup?.Name,
            ContactId = user.ContactId,
            ContactName = contact != null ? $"{contact.FirstName} {contact.LastName}" : null,
            ContactEmail = contact?.EmailPrimary,
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
