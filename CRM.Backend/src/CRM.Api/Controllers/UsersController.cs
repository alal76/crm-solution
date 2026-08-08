// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing users, their profiles, departments, and contact links.
/// Provides comprehensive user management including CRUD operations, profile assignment,
/// contact linking, and user preferences management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : CrmControllerBase
{
    private const string UserNotFoundMessage = "User not found";

    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IContactsService _contactsService;
    private readonly IUserService _userService;
    private readonly ICrmDbContext _dbContext;
    private readonly IEmailDigestService _emailDigestService;
    private readonly ILogger<UsersController> _logger;

    public UsersController( // NOSONAR
        IRepository<User> userRepository,
        IRepository<UserProfile> profileRepository,
        IRepository<Department> departmentRepository,
        IContactsService contactsService,
        IUserService userService,
        ICrmDbContext dbContext,
        IEmailDigestService emailDigestService,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _departmentRepository = departmentRepository;
        _contactsService = contactsService;
        _userService = userService;
        _dbContext = dbContext;
        _emailDigestService = emailDigestService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <remarks>
    /// If password is not provided, the user will be required to set their password on first login.
    /// </remarks>
    /// <param name="request">The user creation request containing email, name, and optional password</param>
    /// <returns>The newly created user</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the request data is invalid or email already exists</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return BadRequest(new { message = "First name is required" });
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "Last name is required" });
            }

            UserDto userDto;
            // Use provided username or fall back to email
            var effectiveUsername = !string.IsNullOrWhiteSpace(request.Username)
                ? request.Username.Trim()
                : request.Email.Trim();

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                // Create user without password - they will set it on first login
                userDto = await _userService.CreateUserWithoutPasswordAsync(
                    request.Email.Trim(),
                    request.FirstName.Trim(),
                    request.LastName.Trim(),
                    request.RoleId,
                    effectiveUsername);
            }
            else
            {
                // Create user with password
                userDto = await _userService.CreateUserAsync(
                    request.Email.Trim(),
                    request.FirstName.Trim(),
                    request.LastName.Trim(),
                    request.Password,
                    request.RoleId,
                    effectiveUsername);
            }

            // Update department and group if provided
            if (request.DepartmentId.HasValue || request.PrimaryGroupId.HasValue)
            {
                var user = await _dbContext.Users.FindAsync(userDto.Id);
                if (user != null)
                {
                    if (request.DepartmentId.HasValue)
                    {
                        user.DepartmentId = request.DepartmentId.Value;
                    }
                    if (request.PrimaryGroupId.HasValue)
                    {
                        user.PrimaryGroupId = request.PrimaryGroupId.Value;
                    }
                    await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
                }
            }

            return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User creation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all users with contact information.
    /// </summary>
    /// <returns>A list of all active users</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
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

    /// <summary>
    /// Get users by department.
    /// </summary>
    /// <param name="departmentId">The department ID to filter by</param>
    /// <returns>A list of users in the specified department</returns>
    /// <response code="200">Returns the list of users in the department</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("department/{departmentId}")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByDepartment(int departmentId)
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

    /// <summary>
    /// Get user by ID with contact information.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user details including linked contact information</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        ContactDto? contact = null;
        if (user.ContactId.HasValue)
        {
            contact = await _contactsService.GetByIdAsync(user.ContactId.Value);
        }

        return Ok(MapToDto(user, contact));
    }

    /// <summary>
    /// Assign user to department and profile.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="assignDto">The profile and department assignment details</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="400">If the profile or department is not found</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/assign-profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> AssignUserProfile(int id, [FromBody] AssignProfileDto assignDto)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        // Verify profile exists
        var profile = await _profileRepository.GetByIdAsync(assignDto.UserProfileId);
        if (profile == null || profile.IsDeleted)
        {
            return BadRequest(new { message = "Profile not found" });
        }

        // Verify department exists if provided
        if (assignDto.DepartmentId.HasValue)
        {
            var department = await _departmentRepository.GetByIdAsync(assignDto.DepartmentId.Value);
            if (department == null || department.IsDeleted)
            {
                return BadRequest(new { message = "Department not found" });
            }

            user.DepartmentId = assignDto.DepartmentId;
        }

        user.UserProfileId = assignDto.UserProfileId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return Ok(MapToDto(user, null));
    }

    /// <summary>
    /// Update user (edit) including contact link.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="updateDto">The user update details</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="400">If the update data is invalid</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

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
            {
                return BadRequest(new { message = "Contact not found" });
            }
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

    /// <summary>
    /// Link user to a contact record.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="linkDto">The contact link details</param>
    /// <returns>The updated user with linked contact</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="400">If the contact is not found or already linked</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/link-contact")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> LinkUserToContact(int id, [FromBody] LinkUserContactDto linkDto)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        ContactDto? contact = null;
        if (linkDto.ContactId.HasValue)
        {
            contact = await _contactsService.GetByIdAsync(linkDto.ContactId.Value);
            if (contact == null)
            {
                return BadRequest(new { message = "Contact not found" });
            }

            // Check if contact is already linked to another user
            var allUsers = await _userRepository.GetAllAsync();
            var existingLink = allUsers.FirstOrDefault(u => u.ContactId == linkDto.ContactId && u.Id != id && !u.IsDeleted);
            if (existingLink != null)
            {
                return BadRequest(new { message = $"Contact is already linked to user: {existingLink.Username}" });
            }
        }

        user.ContactId = linkDto.ContactId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        _logger.LogInformation("User {UserId} linked to contact {ContactId}", id, linkDto.ContactId);

        return Ok(MapToDto(user, contact));
    }

    /// <summary>
    /// Unlink user from contact.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The updated user without contact link</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/unlink-contact")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UnlinkUserFromContact(int id)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        user.ContactId = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        _logger.LogInformation("User {UserId} unlinked from contact", id);

        return Ok(MapToDto(user, null));
    }

    /// <summary>
    /// Get users by contact (find user linked to a specific contact).
    /// </summary>
    /// <param name="contactId">The contact ID to search by</param>
    /// <returns>The user linked to the contact, or null if not found</returns>
    /// <response code="200">Returns the user or null</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("by-contact/{contactId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto?>> GetUserByContact(int contactId)
    {
                var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.ContactId == contactId && !u.IsDeleted);

        if (user == null)
        {
            return Ok(null);
        }

        var contact = await _contactsService.GetByIdAsync(contactId);
        return Ok(MapToDto(user, contact));
    }

    /// <summary>
    /// Delete user (soft delete).
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">User was successfully deleted</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteUser(int id)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return Ok(new { message = "User deleted successfully" });
    }

    /// <summary>
    /// Remove user from profile.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The updated user without profile</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/remove-profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> RemoveUserProfile(int id)
    {
                var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        user.UserProfileId = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        return Ok(MapToDto(user, null));
    }

    /// <summary>
    /// Get current user's preferences.
    /// </summary>
    /// <returns>The user's preference settings</returns>
    /// <response code="200">Returns the user preferences</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("me/preferences")]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserPreferencesDto>> GetMyPreferences()
    {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        return Ok(new UserPreferencesDto
        {
            ThemePreference = user.ThemePreference ?? "system",
            HeaderColor = user.HeaderColor,
            Language = user.Language,
            Timezone = user.Timezone,
            DateFormat = user.DateFormat,
            TimeFormat = user.TimeFormat,
            RowsPerPage = user.RowsPerPage,
            EmailNotifications = user.EmailNotifications,
            DesktopNotifications = user.DesktopNotifications,
            CompactMode = user.CompactMode
        });
    }

    /// <summary>
    /// Update current user's preferences (theme, header color, etc.).
    /// </summary>
    /// <param name="dto">The preferences to update</param>
    /// <returns>The updated user preferences</returns>
    /// <response code="200">Returns the updated preferences</response>
    /// <response code="400">If the preference data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("me/preferences")]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserPreferencesDto>> UpdateMyPreferences([FromBody] UpdatePreferencesDto dto)
    {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        // Validate theme preference
        var validThemes = new[] { "system", "light", "dark", "high-contrast" };
        if (!string.IsNullOrEmpty(dto.ThemePreference) && !validThemes.Contains(dto.ThemePreference.ToLower()))
        {
            return BadRequest(new { message = "Invalid theme preference. Valid values: system, light, dark, high-contrast" });
        }

        // Update theme preference
        if (!string.IsNullOrEmpty(dto.ThemePreference))
        {
            user.ThemePreference = dto.ThemePreference.ToLower();
        }

        // Update header color
        if (dto.HeaderColor != null)
        {
            // Allow empty string to clear
        }
            user.HeaderColor = string.IsNullOrEmpty(dto.HeaderColor) ? null : dto.HeaderColor;

        // Update regional settings
        if (dto.Language != null)
        {
            user.Language = string.IsNullOrEmpty(dto.Language) ? null : dto.Language;
        }
        if (dto.Timezone != null)
        {
            user.Timezone = string.IsNullOrEmpty(dto.Timezone) ? null : dto.Timezone;
        }
        if (dto.DateFormat != null)
        {
            user.DateFormat = string.IsNullOrEmpty(dto.DateFormat) ? null : dto.DateFormat;
        }
        if (dto.TimeFormat != null)
        {
            user.TimeFormat = string.IsNullOrEmpty(dto.TimeFormat) ? null : dto.TimeFormat;
        }

        // Update display settings
        if (dto.RowsPerPage.HasValue)
        {
            user.RowsPerPage = dto.RowsPerPage;
        }
        if (dto.CompactMode.HasValue)
        {
            user.CompactMode = dto.CompactMode;
        }

        // Update notification settings
        if (dto.EmailNotifications.HasValue)
        {
            user.EmailNotifications = dto.EmailNotifications;
        }
        if (dto.DesktopNotifications.HasValue)
        {
            user.DesktopNotifications = dto.DesktopNotifications;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveAsync();

        _logger.LogInformation("User {UserId} updated preferences: theme={Theme}", userId, user.ThemePreference);

        return Ok(new UserPreferencesDto
        {
            ThemePreference = user.ThemePreference ?? "system",
            HeaderColor = user.HeaderColor,
            Language = user.Language,
            Timezone = user.Timezone,
            DateFormat = user.DateFormat,
            TimeFormat = user.TimeFormat,
            RowsPerPage = user.RowsPerPage,
            EmailNotifications = user.EmailNotifications,
            DesktopNotifications = user.DesktopNotifications,
            CompactMode = user.CompactMode
        });
    }

    /// <summary>
    /// Get the current user's email digest configuration (REV-FE-002).
    /// </summary>
    /// <returns>The user's email digest settings, or defaults if never configured.</returns>
    /// <response code="200">Returns the email digest configuration</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("me/email-digest")]
    [ProducesResponseType(typeof(EmailDigestConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EmailDigestConfigDto>> GetMyEmailDigest(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var dto = await _emailDigestService.GetConfigAsync(userId, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    /// Create or update the current user's email digest configuration (REV-FE-002).
    /// </summary>
    /// <param name="dto">The email digest configuration to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The saved email digest configuration</returns>
    /// <response code="200">Returns the saved email digest configuration</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPut("me/email-digest")]
    [ProducesResponseType(typeof(EmailDigestConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EmailDigestConfigDto>> UpdateMyEmailDigest(
        [FromBody] EmailDigestConfigDto dto,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var saved = await _emailDigestService.UpdateConfigAsync(userId, dto, cancellationToken);
        return Ok(saved);
    }

    /// <summary>
    /// Sends a one-off preview of the current user's email digest immediately (REV-FE-002).
    /// Uses the currently-saved section selections (or sensible defaults if never saved).
    /// Does not affect the digest's scheduled send tracking.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Preview email was sent</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="502">If the preview email failed to send</response>
    [HttpPost("me/email-digest/preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SendMyEmailDigestPreview(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        var configDto = await _emailDigestService.GetConfigAsync(userId, cancellationToken);
        var config = new EmailDigestConfig
        {
            UserId = userId,
            IsEnabled = true,
            Frequency = configDto.Frequency.Equals("weekly", StringComparison.OrdinalIgnoreCase)
                ? EmailDigestFrequency.Weekly
                : configDto.Frequency.Equals("monthly", StringComparison.OrdinalIgnoreCase)
                    ? EmailDigestFrequency.Monthly
                    : EmailDigestFrequency.Daily,
            DayOfWeek = configDto.DayOfWeek,
            DayOfMonth = configDto.DayOfMonth,
            Timezone = configDto.Timezone,
            IncludeNewLeads = configDto.Sections.NewLeads,
            IncludeOpenOpportunities = configDto.Sections.OpenOpportunities,
            IncludeRecentActivities = configDto.Sections.RecentActivities,
            IncludeUpcomingTasks = configDto.Sections.UpcomingTasks,
            IncludeOverdueTasks = configDto.Sections.OverdueTasks,
            IncludeTeamPerformance = configDto.Sections.TeamPerformance,
            IncludeKpiSummary = configDto.Sections.KpiSummary
        };

        var sent = await _emailDigestService.SendDigestAsync(user, config, isPreview: true, cancellationToken);
        if (!sent)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Failed to send preview email digest" });
        }

        return Ok(new { message = "Preview email digest sent" });
    }

    private static UserDto MapToDto(User user, ContactDto? contact)
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
            IsLocked = user.IsLocked,
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
            LastLoginDate = user.LastLoginAt,
            HeaderColor = user.HeaderColor,
            PhotoUrl = user.PhotoUrl,

            // Security / account status
            TwoFactorEnabled = user.TwoFactorEnabled,
            PasswordLastChangedAt = user.PasswordLastChangedAt,
            MustResetPassword = user.MustResetPassword,
            EmailVerified = user.EmailVerified,
            PasswordNeverSet = user.PasswordNeverSet,
            CommissionPlanId = user.CommissionPlanId,

            // Preferences
            ThemePreference = user.ThemePreference,
            Language = user.Language,
            Timezone = user.Timezone,
            DateFormat = user.DateFormat,
            TimeFormat = user.TimeFormat,
            RowsPerPage = user.RowsPerPage,
            EmailNotifications = user.EmailNotifications,
            DesktopNotifications = user.DesktopNotifications,
            CompactMode = user.CompactMode
        };
    }

    #region Role Management (EP-029)

    /// <summary>
    /// Get all roles/groups for a specific user.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>List of groups the user belongs to</returns>
    /// <response code="200">Returns the user's roles/groups</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        var memberships = await _dbContext.UserGroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == id)
            .ToListAsync();

        var groupIds = memberships.Select(m => m.UserGroupId).ToList();
        var groups = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(g => groupIds.Contains(g.Id) && !g.IsDeleted)
            .ToListAsync();

        var result = groups.Select(g => new
        {
            GroupId = g.Id,
            g.Name,
            g.Description,
            JoinedAt = memberships.FirstOrDefault(m => m.UserGroupId == g.Id)?.CreatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Assign a user to a role/group.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="roleId">The group/role ID to assign</param>
    /// <returns>Confirmation of assignment</returns>
    /// <response code="200">Role assigned successfully</response>
    /// <response code="404">If the user or role is not found</response>
    /// <response code="409">If the user is already in this role</response>
    [HttpPost("{id}/roles/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(int id, int roleId)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound(new { message = UserNotFoundMessage });
        }

        var group = await _dbContext.UserGroups
            .FirstOrDefaultAsync(g => g.Id == roleId && !g.IsDeleted);
        if (group == null)
        {
            return NotFound(new { message = "Role/group not found" });
        }

        var existingMembership = await _dbContext.UserGroupMembers
            .AnyAsync(m => m.UserId == id && m.UserGroupId == roleId);
        if (existingMembership)
        {
            return Conflict(new { message = "User is already assigned to this role/group" });
        }

        var membership = new UserGroupMember
        {
            UserId = id,
            UserGroupId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.UserGroupMembers.Add(membership);
        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

        _logger.LogInformation("User {UserId} assigned to group {GroupId}", id, roleId);

        return Ok(new { Message = "Role assigned successfully.", UserId = id, GroupId = roleId, GroupName = group.Name });
    }

    #endregion
}

/// <summary>
/// DTO for assigning profile to user
/// </summary>
public class AssignProfileDto
{
    public int UserProfileId { get; set; }
    public int? DepartmentId { get; set; }
}

/// <summary>
/// DTO for user preferences response
/// </summary>
public class UserPreferencesDto
{
    public string ThemePreference { get; set; } = "system";
    public string? HeaderColor { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public int? RowsPerPage { get; set; }
    public bool? EmailNotifications { get; set; }
    public bool? DesktopNotifications { get; set; }
    public bool? CompactMode { get; set; }
}

/// <summary>
/// DTO for updating user preferences
/// </summary>
public class UpdatePreferencesDto
{
    public string? ThemePreference { get; set; }
    public string? HeaderColor { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public int? RowsPerPage { get; set; }
    public bool? EmailNotifications { get; set; }
    public bool? DesktopNotifications { get; set; }
    public bool? CompactMode { get; set; }
}
